"""用还原的 C# 源码替换 AssetRipper 反编译出的脚本，并沿用原脚本的 GUID。

场景和预制体通过 MonoScript 的 GUID 引用脚本，所以每个还原源码文件必须拿到
反编译时同名类所用的 GUID。对应规则：(命名空间, 文件名) 与反编译文件的
(目录路径, 文件名) 一致。

用法：
    python replace_scripts.py <试点工程目录> <还原仓库的 Assets 目录>
    （迁移前从零搭试点工程时用；迁移后代码在 Source/，本脚本只作记录。）
"""

import re
import shutil
import sys
from pathlib import Path

# 反编译脚本所在目录（相对试点工程 Assets）
DECOMPILED_DIRS = ["Scripts/Assembly-CSharp", "Scripts/LitJSON"]
# 主程序集的反编译根目录：其下的目录层级就是命名空间
DECOMPILED_MAIN_DIR = "Scripts/Assembly-CSharp"
# 要从还原仓库复制的源码目录（相对还原仓库 Assets）
SOURCE_DIRS = ["Packages", "Scripts", "Rendering"]
# 还原仓库里的第三方程序集：原样复制
SOURCE_DLLS = ["3rd_party/LitJSON.dll"]
# 匹配源码里第一个命名空间声明
NAMESPACE_PATTERN = re.compile(r"^\s*namespace\s+([\w.]+)", re.MULTILINE)
# 引用资源的文本文件后缀：用来统计丢失的脚本引用
REFERENCING_SUFFIXES = [".unity", ".prefab", ".asset"]

META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
"""


def read_guid(meta_path):
    """从 .meta 文件读出 guid。"""
    for line in meta_path.read_text(encoding="utf-8").splitlines():
        if line.startswith("guid:"):
            return line.split(":", 1)[1].strip()
    return None


def build_decompiled_map(assets_dir):
    """反编译脚本：(命名空间, 类名) -> guid。"""
    result = {}
    main_dir = assets_dir / DECOMPILED_MAIN_DIR
    for cs_path in main_dir.rglob("*.cs"):
        meta_path = cs_path.with_name(cs_path.name + ".meta")
        if not meta_path.exists():
            continue
        relative_dir = cs_path.parent.relative_to(main_dir)
        namespace = ".".join(relative_dir.parts)
        key = (namespace, cs_path.stem)
        result[key] = read_guid(meta_path)
    print(f"[map] decompiled scripts with guid: {len(result)}")
    return result


def source_key(cs_path):
    """还原源码：(第一个命名空间, 文件名)。"""
    text = cs_path.read_text(encoding="utf-8-sig", errors="replace")
    match = NAMESPACE_PATTERN.search(text)
    namespace = ""
    if match:
        namespace = match.group(1)
    return (namespace, cs_path.stem)


def collect_referenced_guids(assets_dir):
    """统计场景、预制体、资源文件里引用到的所有 guid。"""
    guid_pattern = re.compile(r"guid: ([0-9a-f]{32})")
    referenced = set()
    for path in assets_dir.rglob("*"):
        if path.suffix not in REFERENCING_SUFFIXES:
            continue
        text = path.read_text(encoding="utf-8", errors="replace")
        for guid in guid_pattern.findall(text):
            referenced.add(guid)
    return referenced


def main():
    """入口：删除反编译脚本 → 复制还原源码 → 写 .meta → 报告未匹配项。"""
    pilot_assets = Path(sys.argv[1]) / "Assets"
    source_assets = Path(sys.argv[2])

    decompiled = build_decompiled_map(pilot_assets)
    referenced = collect_referenced_guids(pilot_assets)
    print(f"[map] guids referenced by scenes/prefabs/assets: {len(referenced)}")

    # 1. 删除反编译脚本（连同目录自身的 .meta）
    for relative in DECOMPILED_DIRS:
        target = pilot_assets / relative
        if target.exists():
            shutil.rmtree(target)
        dir_meta = target.with_name(target.name + ".meta")
        if dir_meta.exists():
            dir_meta.unlink()
        print(f"[clean] removed {relative}")

    # 2. 复制还原源码，按 (命名空间, 文件名) 沿用 guid
    used_keys = set()
    copied = 0
    matched = 0
    for relative in SOURCE_DIRS:
        for cs_path in sorted((source_assets / relative).rglob("*.cs")):
            destination = pilot_assets / cs_path.relative_to(source_assets)
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(cs_path, destination)
            copied += 1
            key = source_key(cs_path)
            guid = decompiled.get(key)
            if guid is None:
                continue
            meta_path = destination.with_name(destination.name + ".meta")
            meta_path.write_text(META_TEMPLATE.format(guid=guid), encoding="utf-8")
            used_keys.add(key)
            matched += 1
    print(f"[copy] source files copied: {copied}, given original guid: {matched}")

    for relative in SOURCE_DLLS:
        destination = pilot_assets / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source_assets / relative, destination)
        print(f"[copy] dll: {relative}")

    # 3. 报告：反编译里有、源码里没对上的类，重点看被资源引用的
    missing_referenced = []
    missing_other = 0
    for key in sorted(decompiled):
        if key in used_keys:
            continue
        if decompiled[key] in referenced:
            missing_referenced.append(key)
        else:
            missing_other += 1
    print(f"[report] unmatched classes not referenced by assets: {missing_other}")
    print(f"[report] unmatched classes REFERENCED by assets: {len(missing_referenced)}")
    for namespace, name in missing_referenced:
        print(f"    {namespace}.{name}")


if __name__ == "__main__":
    main()
