"""从未改动的 ULegacy 导出里提取旧版粒子组件（Unity 3.5 的 legacy particles），输出 JSON 供编辑器转换。

旧版粒子在 Unity 2018.3 被删除，Unity 6 导入时把这些组件静默丢掉，所以只能从原始导出的 YAML 读参数：
    EllipsoidParticleEmitter (classID 15)、ParticleAnimator (12)、ParticleRenderer (26)、WorldParticleCollider (76)
以挂载它们的 GameObject 的 fileID 为键。工程素材是导出的副本，fileID 不变（场景被 Unity 6 重存过也不变）。

用法：python legacy_particles_extract.py <导出根目录> <输出.json>
    例：python tools/legacy_particles_extract.py ../unity_assets/ULegacy-Ripper_0.0.6 pc_data/particles/legacy_particles.json
"""

import json
import re
import sys
from pathlib import Path

# 要提取的组件：classID → JSON 里的分组名
LEGACY_CLASSES = {
    "15": "emitter",
    "12": "animator",
    "26": "renderer",
    "76": "collider",
}
ASSET_SUFFIXES = (".unity", ".prefab")
DOCUMENT_HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)")
VECTOR_FIELD = re.compile(r"(\w+): (-?[0-9.eE+-]+)")
FILE_REFERENCE = re.compile(r"\{fileID: (-?\d+)(?:, guid: ([0-9a-f]+), type: \d+)?\}")


def parse_value(text):
    """把 YAML 里的标量或 {x: .., y: ..} 内联映射转成 Python 值。"""
    text = text.strip()
    if text.startswith("{fileID"):
        match = FILE_REFERENCE.match(text)
        reference = {"fileID": int(match.group(1))}
        if match.group(2):
            reference["guid"] = match.group(2)
        return reference
    if text.startswith("{"):
        result = {}
        for key, number in VECTOR_FIELD.findall(text):
            result[key] = float(number)
        return result
    try:
        return float(text)
    except ValueError:
        return text


def parse_component(lines):
    """解析一个组件文档的正文（不含首行类型名）。

    只处理这几类组件会用到的结构：两格缩进的字段、四格缩进的子字段（colorAnimation[i]、UV Animation、
    m_CollidesWith），以及 m_Materials 这样的列表。
    """
    fields = {}
    current_key = None
    for line in lines:
        if not line.strip():
            continue
        indent = len(line) - len(line.lstrip(" "))
        stripped = line.strip()
        if stripped.startswith("- "):
            # 列表项，属于上一个两格缩进的字段
            fields.setdefault(current_key, [])
            if not isinstance(fields[current_key], list):
                fields[current_key] = []
            fields[current_key].append(parse_value(stripped[2:]))
            continue
        key, _, value = stripped.partition(":")
        if indent == 2:
            current_key = key
            if value.strip() and value.strip() != "[]":
                fields[key] = parse_value(value)
            elif value.strip() == "[]":
                fields[key] = []
            else:
                fields[key] = {}
        elif indent >= 4 and isinstance(fields.get(current_key), dict):
            fields[current_key][key] = parse_value(value)
    return fields


def split_documents(text):
    """把 Unity YAML 拆成 (classID, fileID, 正文行列表)。"""
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


def extract_asset(path, relative_path):
    """提取一个场景或预制体里的全部旧版粒子，按 GameObject 归组。"""
    text = path.read_text(encoding="utf-8")
    game_object_names = {}
    by_game_object = {}
    for class_id, file_id, lines in split_documents(text):
        if class_id == "1":
            for line in lines:
                if line.startswith("  m_Name: "):
                    game_object_names[file_id] = line[len("  m_Name: "):].strip()
            continue
        group = LEGACY_CLASSES.get(class_id)
        if group is None:
            continue
        # lines[0] 是类型名（例如 "EllipsoidParticleEmitter:"）
        fields = parse_component(lines[1:])
        owner = fields["m_GameObject"]["fileID"]
        entry = by_game_object.setdefault(owner, {"gameObjectFileId": owner})
        entry[group] = fields
    entries = []
    for owner, entry in by_game_object.items():
        entry["assetPath"] = relative_path
        entry["gameObjectName"] = game_object_names.get(owner, "")
        entries.append(entry)
    return entries


def color_from_rgba(value):
    """Unity 的 ColorRGBA32 序列化成一个 uint：最低字节是 r，最高字节是 a。"""
    packed = int(value)
    red = packed & 0xFF
    green = (packed >> 8) & 0xFF
    blue = (packed >> 16) & 0xFF
    alpha = (packed >> 24) & 0xFF
    return [red / 255.0, green / 255.0, blue / 255.0, alpha / 255.0]


def vector(fields, key):
    """取三维向量字段，返回 [x, y, z]。"""
    value = fields[key]
    return [value["x"], value["y"], value["z"]]


def flatten(entry):
    """把原始字段整理成编辑器脚本用的扁平结构（字段名与 PcLegacyParticleConvert.LegacyEntry 一致）。"""
    emitter = entry["emitter"]
    animator = entry["animator"]
    renderer = entry["renderer"]
    result = {
        "assetPath": entry["assetPath"],
        "gameObjectFileId": str(entry["gameObjectFileId"]),
        "gameObjectName": entry["gameObjectName"],
        # 发射器
        "emitterEnabled": emitter["m_Enabled"] == 1,
        "emit": emitter["m_Emit"] == 1,
        "minSize": emitter["minSize"],
        "maxSize": emitter["maxSize"],
        "minEnergy": emitter["minEnergy"],
        "maxEnergy": emitter["maxEnergy"],
        "minEmission": emitter["minEmission"],
        "maxEmission": emitter["maxEmission"],
        "worldVelocity": vector(emitter, "worldVelocity"),
        "localVelocity": vector(emitter, "localVelocity"),
        "rndVelocity": vector(emitter, "rndVelocity"),
        "emitterVelocityScale": emitter["emitterVelocityScale"],
        "tangentVelocity": vector(emitter, "tangentVelocity"),
        "angularVelocity": emitter["angularVelocity"],
        "rndAngularVelocity": emitter["rndAngularVelocity"],
        "rndRotation": emitter["rndRotation"] == 1,
        "useWorldSpace": emitter["Simulate in Worldspace?"] == 1,
        "oneShot": emitter["m_OneShot"] == 1,
        "ellipsoid": vector(emitter, "m_Ellipsoid"),
        "minEmitterRange": emitter["m_MinEmitterRange"],
        # 动画器
        "doesAnimateColor": animator["Does Animate Color?"] == 1,
        "colorAnimation": [],
        "sizeGrow": animator["sizeGrow"],
        "rndForce": vector(animator, "rndForce"),
        "force": vector(animator, "force"),
        "damping": animator["damping"],
        "autodestruct": animator["autodestruct"] == 1,
        # 渲染器
        "rendererEnabled": renderer["m_Enabled"] == 1,
        "materialGuids": [],
        "renderMode": int(renderer["m_StretchParticles"]),
        "lengthScale": renderer["m_LengthScale"],
        "velocityScale": renderer["m_VelocityScale"],
        "cameraVelocityScale": renderer["m_CameraVelocityScale"],
        "maxParticleSize": renderer["m_MaxParticleSize"],
        "uvTilesX": int(renderer["UV Animation"]["x Tile"]),
        "uvTilesY": int(renderer["UV Animation"]["y Tile"]),
        "uvCycles": renderer["UV Animation"]["cycles"],
        # 碰撞（WorldParticleCollider，可选）
        "hasCollider": "collider" in entry,
        "bounceFactor": 0.0,
        "collisionEnergyLoss": 0.0,
        "collidesWith": -1,
        "minKillVelocity": 0.0,
    }
    for index in range(5):
        rgba = animator["colorAnimation[" + str(index) + "]"]["rgba"]
        result["colorAnimation"].extend(color_from_rgba(rgba))
    for material in renderer.get("m_Materials", []):
        result["materialGuids"].append(material.get("guid", ""))
    if "collider" in entry:
        collider = entry["collider"]
        result["bounceFactor"] = collider["m_BounceFactor"]
        result["collisionEnergyLoss"] = collider["m_CollisionEnergyLoss"]
        # 32 位掩码按有符号 int 存，C# 的 LayerMask 是 int
        bits = int(collider["m_CollidesWith"]["m_Bits"])
        if bits >= 2 ** 31:
            bits -= 2 ** 32
        result["collidesWith"] = bits
        result["minKillVelocity"] = collider["m_MinKillVelocity"]
    return result


def main():
    export_root = Path(sys.argv[1])
    output_path = Path(sys.argv[2])
    entries = []
    for path in sorted(export_root.joinpath("Assets").rglob("*")):
        if path.suffix not in ASSET_SUFFIXES:
            continue
        relative_path = path.relative_to(export_root).as_posix()
        for entry in extract_asset(path, relative_path):
            if "emitter" not in entry:
                print(f"skip {relative_path} {entry['gameObjectFileId']}: no emitter")
                continue
            entries.append(flatten(entry))
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps({"entries": entries}, indent=1), encoding="utf-8")
    assets = set()
    for entry in entries:
        assets.add(entry["assetPath"])
    print(f"extracted {len(entries)} legacy particle objects from {len(assets)} assets -> {output_path}")


if __name__ == "__main__":
    main()
