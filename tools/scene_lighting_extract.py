"""从未改动的 ULegacy 导出里提取每个场景的光照贴图分配和光照探针，输出 JSON 供编辑器写回场景。

Unity 6 打开 3.5 的场景时把这些数据丢了：
    - LightmapSettings.m_Lightmaps（光照贴图列表）和 m_LightProbes（LightProbes 资源引用）；
    - 每个渲染器的 m_LightmapIndex / m_LightmapTilingOffset（Unity 5 起改由 LightingDataAsset 保存）；
    - 3.5 格式的 LightProbes 资源本身也读不了。
所以场景里的物体全部没有光照贴图，用探针的角色拿到的 SH 是 0（全黑）。

输出：<工程根目录>/pc_data/lighting/<场景名>.json，每个场景一个，字段见 build_scene_entry()。
LightProbes 资源（3.5 格式 YAML）：
    bakedPositions: 探针位置
    bakedCoefficients: 每个探针 27 个浮点，sh[i*3+c] 是第 i 个系数的第 c 个颜色通道
    tetrahedra: indices[0..3]（探针下标，外壳四面体的 indices[3] 为 -1）、neighbors[0..3]（与第 i 个顶点相对的面的邻居）

用法：python scene_lighting_extract.py <导出根目录> <工程根目录>
"""

import json
import re
import sys
from pathlib import Path

RENDERER_CLASSES = {"23", "137"}   # MeshRenderer、SkinnedMeshRenderer
NO_LIGHTMAP = 255
LIGHTMAP_LIMIT = 254               # 0～253 是有效的光照贴图下标
DOCUMENT_HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)")
GUID = re.compile(r"guid: ([0-9a-f]{32})")
VECTOR4 = re.compile(r"\{x: ([-0-9.eE+]+), y: ([-0-9.eE+]+), z: ([-0-9.eE+]+), w: ([-0-9.eE+]+)\}")
VECTOR3 = re.compile(r"\{x: ([-0-9.eE+]+), y: ([-0-9.eE+]+), z: ([-0-9.eE+]+)\}")


def build_guid_map(export_root):
    """GUID → 资源路径（从 .meta 文件读）。"""
    guid_map = {}
    for meta in export_root.joinpath("Assets").rglob("*.meta"):
        text = meta.read_text(encoding="utf-8", errors="ignore")
        match = GUID.search(text)
        if match:
            guid_map[match.group(1)] = meta.with_suffix("")
    return guid_map


def split_documents(text):
    """拆成 (classID, fileID, 正文行列表)。"""
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
    """取 LightmapSettings 里的光照贴图 GUID 列表和 LightProbes 资源 GUID。"""
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
    """取渲染器的光照贴图下标、缩放偏移和是否用探针。"""
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
    """解析 3.5 格式的 LightProbes 资源。返回扁平数组组成的字典。"""
    positions = []
    coefficients = []
    tetra_indices = []
    tetra_neighbors = []
    section = ""
    for line in path.read_text(encoding="utf-8").splitlines():
        stripped = line.strip()
        if stripped in ("bakedPositions:", "bakedCoefficients:", "tetrahedra:", "hullRays:"):
            section = stripped[:-1]
            continue
        if section == "bakedPositions":
            match = VECTOR3.search(stripped)
            if match:
                for group in match.groups():
                    positions.append(float(group))
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
    return {
        "positions": positions,
        "coefficients": coefficients,
        "tetraIndices": tetra_indices,
        "tetraNeighbors": tetra_neighbors,
    }


def build_scene_entry(scene_path, relative_path, guid_map):
    """一个场景的 JSON 内容；没有任何光照数据时返回 None。"""
    lightmap_guids = []
    probes_guid = ""
    renderer_ids = []
    renderer_indices = []
    renderer_scale_offsets = []
    probe_renderer_ids = []
    for class_id, file_id, lines in split_documents(scene_path.read_text(encoding="utf-8")):
        if class_id == "157":   # LightmapSettings（3.5 的 classID）
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
    export_root = Path(sys.argv[1])
    pilot_root = Path(sys.argv[2])
    output_dir = pilot_root / "pc_data" / "lighting"
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
        # JsonUtility 不接受 null 的嵌套对象，没有探针时给空数组
        if entry["probes"] is None:
            entry["probes"] = {"positions": [], "coefficients": [], "tetraIndices": [], "tetraNeighbors": []}
        output = output_dir / (scene_path.stem + ".json")
        output.write_text(json.dumps(entry), encoding="utf-8")
        print(f"{relative_path}: lightmaps={len(entry['lightmapGuids'])} lightmapped={len(entry['rendererFileIds'])} "
              f"probeUsers={len(entry['probeRendererFileIds'])} probes={probe_count} tetrahedra={tetra_count}")
        count += 1
    print(f"wrote {count} scenes -> {output_dir}")


if __name__ == "__main__":
    main()
