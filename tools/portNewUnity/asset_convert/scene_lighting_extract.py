"""Extract each scene's lightmap assignment and light probes from the untouched ULegacy export, as JSON for the editor to write back.

Unity 6 drops this data when it opens a 3.5 scene:
    - LightmapSettings.m_Lightmaps (lightmap list) and m_LightProbes (LightProbes asset reference);
    - every renderer's m_LightmapIndex / m_LightmapTilingOffset (Unity 5+ keeps them in the LightingDataAsset);
    - the 3.5-format LightProbes asset itself cannot be read either.
So nothing in the scenes has a lightmap, and probe-lit characters get SH = 0 (black).

Output: <project root>/pc_data/lighting/<scene name>.json, one per scene, fields in build_scene_entry().
LightProbes asset (3.5-format YAML):
    bakedPositions: probe positions
    bakedCoefficients: 27 floats per probe, sh[i*3+c] is coefficient i of color channel c
    tetrahedra: indices[0..3] (probe indices; outer cells have indices[3] = -1),
                neighbors[0..3] (neighbor across the face opposite vertex i; for outer cells neighbors[3] is the inner tetrahedron)
    hullRays: one per probe, the direction outer cells extrude along (zero for interior probes)

Usage: python scene_lighting_extract.py <export root> <project root>
"""

import json
import re
import sys
from pathlib import Path

RENDERER_CLASSES = {"23", "137"}   # MeshRenderer, SkinnedMeshRenderer
NO_LIGHTMAP = 255
LIGHTMAP_LIMIT = 254               # 0..253 are valid lightmap indices
DOCUMENT_HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)")
GUID = re.compile(r"guid: ([0-9a-f]{32})")
VECTOR4 = re.compile(r"\{x: ([-0-9.eE+]+), y: ([-0-9.eE+]+), z: ([-0-9.eE+]+), w: ([-0-9.eE+]+)\}")
VECTOR3 = re.compile(r"\{x: ([-0-9.eE+]+), y: ([-0-9.eE+]+), z: ([-0-9.eE+]+)\}")
PROBE_SECTIONS = ("bakedPositions:", "bakedCoefficients:", "tetrahedra:", "hullRays:")


def build_guid_map(export_root):
    """GUID -> asset path (read from the .meta files)."""
    guid_map = {}
    for meta in export_root.joinpath("Assets").rglob("*.meta"):
        text = meta.read_text(encoding="utf-8", errors="ignore")
        match = GUID.search(text)
        if match:
            guid_map[match.group(1)] = meta.with_suffix("")
    return guid_map


def split_documents(text):
    """Split a YAML scene into (classID, fileID, body lines)."""
    documents = []
    current = None
    for line in text.splitlines():
        match = DOCUMENT_HEADER.match(line)
        if match:
            current = (match.group(1), int(match.group(2)), [])
            documents.append(current)
        elif current is not None:
            current[2].append(line)
    return documents


def parse_lightmap_settings(lines):
    """Lightmap GUID list and LightProbes asset GUID from LightmapSettings."""
    lightmap_guids = []
    probes_guid = ""
    for line in lines:
        stripped = line.strip()
        if stripped.startswith("m_LightProbes:"):
            match = GUID.search(stripped)
            if match:
                probes_guid = match.group(1)
        elif stripped.startswith("- m_Lightmap:"):
            match = GUID.search(stripped)
            if match:
                lightmap_guids.append(match.group(1))
            else:
                lightmap_guids.append("")
    return lightmap_guids, probes_guid


def parse_renderer(lines):
    """A renderer's lightmap index, scale/offset and whether it uses light probes."""
    index = NO_LIGHTMAP
    scale_offset = [1.0, 1.0, 0.0, 0.0]
    use_probes = False
    for line in lines:
        stripped = line.strip()
        if stripped.startswith("m_LightmapIndex:"):
            index = int(stripped.split(":")[1])
        elif stripped.startswith("m_LightmapTilingOffset:"):
            match = VECTOR4.search(stripped)
            scale_offset = []
            for group in match.groups():
                scale_offset.append(float(group))
        elif stripped.startswith("m_UseLightProbes:"):
            use_probes = stripped.endswith("1")
    return index, scale_offset, use_probes


def parse_light_probes(path):
    """Parse a 3.5-format LightProbes asset into a dict of flat arrays."""
    positions = []
    coefficients = []
    tetra_indices = []
    tetra_neighbors = []
    hull_rays = []
    section = ""
    for line in path.read_text(encoding="utf-8").splitlines():
        stripped = line.strip()
        if stripped in PROBE_SECTIONS:
            section = stripped[:-1]
            continue
        if section == "bakedPositions" or section == "hullRays":
            match = VECTOR3.search(stripped)
            if match:
                for group in match.groups():
                    if section == "bakedPositions":
                        positions.append(float(group))
                    else:
                        hull_rays.append(float(group))
        elif section == "bakedCoefficients":
            match = re.match(r"-?\s*sh\[(\d+)\]: ([-0-9.eE+]+)", stripped)
            if match:
                coefficients.append(float(match.group(2)))
        elif section == "tetrahedra":
            match = re.match(r"-?\s*(indices|neighbors)\[\d\]: (-?\d+)", stripped)
            if match:
                if match.group(1) == "indices":
                    tetra_indices.append(int(match.group(2)))
                else:
                    tetra_neighbors.append(int(match.group(2)))
    probe_count = len(positions) // 3
    if len(coefficients) != probe_count * 27:
        raise ValueError(f"{path}: {probe_count} probes but {len(coefficients)} coefficients")
    if len(tetra_indices) != len(tetra_neighbors):
        raise ValueError(f"{path}: tetrahedra indices/neighbors mismatch")
    if len(hull_rays) != len(positions):
        raise ValueError(f"{path}: {probe_count} probes but {len(hull_rays) // 3} hull rays")
    return {
        "positions": positions,
        "coefficients": coefficients,
        "tetraIndices": tetra_indices,
        "tetraNeighbors": tetra_neighbors,
        "hullRays": hull_rays,
    }


def build_scene_entry(scene_path, relative_path, guid_map):
    """JSON content of one scene; None when the scene has no lighting data at all."""
    lightmap_guids = []
    probes_guid = ""
    renderer_ids = []
    renderer_indices = []
    renderer_scale_offsets = []
    probe_renderer_ids = []
    for class_id, file_id, lines in split_documents(scene_path.read_text(encoding="utf-8")):
        if class_id == "157":   # LightmapSettings (3.5 classID)
            lightmap_guids, probes_guid = parse_lightmap_settings(lines)
        elif class_id in RENDERER_CLASSES:
            index, scale_offset, use_probes = parse_renderer(lines)
            if 0 <= index < LIGHTMAP_LIMIT:
                renderer_ids.append(str(file_id))
                renderer_indices.append(index)
                renderer_scale_offsets.extend(scale_offset)
            if use_probes:
                probe_renderer_ids.append(str(file_id))
    if not lightmap_guids and not probes_guid:
        return None
    entry = {
        "scenePath": relative_path,
        "lightmapGuids": lightmap_guids,
        "rendererFileIds": renderer_ids,
        "rendererLightmapIndices": renderer_indices,
        "rendererScaleOffsets": renderer_scale_offsets,
        "probeRendererFileIds": probe_renderer_ids,
        "probes": None,
    }
    if probes_guid:
        entry["probes"] = parse_light_probes(guid_map[probes_guid])
    return entry


def main():
    """Entry point."""
    export_root = Path(sys.argv[1])
    project_root = Path(sys.argv[2])
    output_dir = project_root / "pc_data" / "lighting"
    output_dir.mkdir(parents=True, exist_ok=True)
    guid_map = build_guid_map(export_root)
    count = 0
    for scene_path in sorted(export_root.joinpath("Assets").rglob("*.unity")):
        relative_path = scene_path.relative_to(export_root).as_posix()
        entry = build_scene_entry(scene_path, relative_path, guid_map)
        if entry is None:
            continue
        probe_count = 0
        tetra_count = 0
        if entry["probes"] is not None:
            probe_count = len(entry["probes"]["positions"]) // 3
            tetra_count = len(entry["probes"]["tetraIndices"]) // 4
        # JsonUtility does not accept a null nested object: use empty arrays when there are no probes
        if entry["probes"] is None:
            entry["probes"] = {"positions": [], "coefficients": [], "tetraIndices": [], "tetraNeighbors": [], "hullRays": []}
        output = output_dir / (scene_path.stem + ".json")
        output.write_text(json.dumps(entry), encoding="utf-8")
        print(f"{relative_path}: lightmaps={len(entry['lightmapGuids'])} lightmapped={len(entry['rendererFileIds'])} "
              f"probeUsers={len(entry['probeRendererFileIds'])} probes={probe_count} tetrahedra={tetra_count}")
        count += 1
    print(f"wrote {count} scenes -> {output_dir}")


if __name__ == "__main__":
    main()
