"""Parse the legacy NavMesh (Detour binary) exported from Unity 3.5, extract the walkable polygons, and write JSON for the editor to re-bake.

Data layout (measured from m_MeshData, consistent with the tile save format of the Recast sample):
    MSET header: magic 'MSET', version, numTiles, dtNavMeshParams(orig[3], tileWidth, tileHeight, maxTiles, maxPolys)
    Per tile: tileRef(u32), dataSize(u32), then dtMeshHeader('DNAV') + verts + polys + ...
dtMeshHeader fields (version 13): magic, version, x, y, layer, userId, polyCount, vertCount, maxLinkCount,
    detailMeshCount, detailVertCount, detailTriCount, bvNodeCount, offMeshConCount, offMeshBase,
    unknown, walkableHeight, walkableRadius, walkableClimb, bmin[3], bmax[3], bvQuantFactor
dtPoly (Unity 3.5 variant, 36 bytes, measured from the data): firstLink(u32), verts[6](u16), neis[6](u16), flags(u16),
    unknown(u16), vertCount(u8)@32, areaAndtype(u8)@33, padding(2)

Usage: python navmesh_extract.py <NavMesh.asset> [<output.json>]
"""

import json
import re
import struct
import sys
from pathlib import Path

MESH_SET_MAGIC = b"TESM"   # 'MSET' little-endian
TILE_MAGIC = b"VAND"       # 'DNAV' little-endian
MAX_VERTS_PER_POLY = 6
POLY_SIZE = 36
HEADER_INTS = 16           # magic .. unknown, 16 ints in total
# Polygon type of external off-mesh connections (high 2 bits of areaAndtype)
POLY_TYPE_OFFMESH = 1


def read_mesh_data(asset_path):
    """Get the hex string of m_MeshData from the YAML."""
    text = Path(asset_path).read_text(encoding="utf-8")
    match = re.search(r"m_MeshData: ([0-9a-f]+)", text)
    return bytes.fromhex(match.group(1))


def parse_tile(data, offset):
    """Parse one tile, returns (list of polygon vertices, header info)."""
    header = struct.unpack_from("<4s15i", data, offset)
    if header[0] != TILE_MAGIC:
        raise ValueError(f"bad tile magic at {offset}: {header[0]!r}")
    (_, version, tile_x, tile_y, layer, user_id, poly_count, vert_count, max_link, detail_mesh_count,
     detail_vert_count, detail_tri_count, bv_node_count, off_mesh_count, off_mesh_base, unknown) = header
    floats = struct.unpack_from("<10f", data, offset + HEADER_INTS * 4)
    info = {
        "version": version,
        "polyCount": poly_count,
        "vertCount": vert_count,
        "offMeshConCount": off_mesh_count,
        "walkableHeight": floats[0],
        "walkableRadius": floats[1],
        "walkableClimb": floats[2],
        "bmin": floats[3:6],
        "bmax": floats[6:9],
    }
    cursor = offset + HEADER_INTS * 4 + 10 * 4
    verts = []
    for index in range(vert_count):
        verts.append(struct.unpack_from("<3f", data, cursor + index * 12))
    cursor += vert_count * 12

    polygons = []
    for index in range(poly_count):
        base = cursor + index * POLY_SIZE
        poly_verts = struct.unpack_from("<6H", data, base + 4)
        poly_vert_count = data[base + 32]
        area_and_type = data[base + 33]
        if area_and_type >> 6 == POLY_TYPE_OFFMESH:
            continue
        if poly_vert_count < 3 or poly_vert_count > MAX_VERTS_PER_POLY:
            raise ValueError(f"bad poly vertCount {poly_vert_count} at poly {index}")
        indices = list(poly_verts[:poly_vert_count])
        for vertex_index in indices:
            if vertex_index >= vert_count:
                raise ValueError(f"poly {index} vertex index {vertex_index} out of range")
        polygons.append(indices)
    return verts, polygons, info


def extract(asset_path):
    """Parse the whole NavMesh, merging the vertices and triangles of all tiles."""
    data = read_mesh_data(asset_path)
    if data[:4] != MESH_SET_MAGIC:
        raise ValueError(f"bad mesh set magic: {data[:4]!r}")
    version, num_tiles = struct.unpack_from("<2i", data, 4)
    params_size = 3 * 4 + 4 + 4 + 4 + 4  # orig[3], tileWidth, tileHeight, maxTiles, maxPolys
    cursor = 12 + params_size
    all_vertices = []
    triangles = []
    infos = []
    for _ in range(num_tiles):
        tile_ref, data_size = struct.unpack_from("<2I", data, cursor)
        cursor += 8
        verts, polygons, info = parse_tile(data, cursor)
        base = len(all_vertices)
        all_vertices.extend(verts)
        # Convex polygons are triangulated as a fan
        for poly in polygons:
            for k in range(1, len(poly) - 1):
                triangles.append([base + poly[0], base + poly[k], base + poly[k + 1]])
        infos.append(info)
        cursor += data_size
    return {"vertices": all_vertices, "triangles": triangles, "tiles": infos, "version": version}


def main():
    """Parse and print an overview, optionally write JSON."""
    result = extract(sys.argv[1])
    print(f"[navmesh] tiles={len(result['tiles'])} vertices={len(result['vertices'])} triangles={len(result['triangles'])}")
    for info in result["tiles"]:
        print(f"[navmesh] tile: {info}")
    if len(sys.argv) > 2:
        # Unity's JsonUtility does not support nested arrays, so a flat array is also written
        flat_vertices = []
        for vertex in result["vertices"]:
            flat_vertices.extend(vertex)
        flat_triangles = []
        for triangle in result["triangles"]:
            flat_triangles.extend(triangle)
        result["flatVertices"] = flat_vertices
        result["flatTriangles"] = flat_triangles
        first_tile = result["tiles"][0]
        result["walkableHeight"] = first_tile["walkableHeight"]
        result["walkableRadius"] = first_tile["walkableRadius"]
        result["walkableClimb"] = first_tile["walkableClimb"]
        Path(sys.argv[2]).write_text(json.dumps(result), encoding="utf-8")
        print(f"[navmesh] written {sys.argv[2]}")


if __name__ == "__main__":
    main()
