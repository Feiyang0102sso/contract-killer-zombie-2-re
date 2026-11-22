"""Replace the scripts decompiled by AssetRipper with the restored C# source, reusing the original scripts' GUIDs.

Scenes and prefabs reference scripts by the MonoScript GUID, so every restored source file must get
the GUID used by the same-named class at decompile time. Matching rule: (namespace, file name) equals the
decompiled file's (directory path, file name).

Usage:
    python replace_scripts.py <pilot project dir> <Assets dir of the restoration repo>
    (Used when building the pilot project from scratch before migration; after migration the code lives in Source/, and this script is kept only as a record.)
"""

import re
import shutil
import sys
from pathlib import Path

# Directories holding the decompiled scripts (relative to the pilot project's Assets)
DECOMPILED_DIRS = ["Scripts/Assembly-CSharp", "Scripts/LitJSON"]
# Decompile root of the main assembly: the directory hierarchy below it is the namespace
DECOMPILED_MAIN_DIR = "Scripts/Assembly-CSharp"
# Source directories to copy from the restoration repo (relative to its Assets)
SOURCE_DIRS = ["Packages", "Scripts", "Rendering"]
# Third-party assemblies in the restoration repo: copied as is
SOURCE_DLLS = ["3rd_party/LitJSON.dll"]
# Match the first namespace declaration in the source
NAMESPACE_PATTERN = re.compile(r"^\s*namespace\s+([\w.]+)", re.MULTILINE)
# Suffixes of text files that reference assets: used to count missing script references
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
    """Read the guid from a .meta file."""
    for line in meta_path.read_text(encoding="utf-8").splitlines():
        if line.startswith("guid:"):
            return line.split(":", 1)[1].strip()
    return None


def build_decompiled_map(assets_dir):
    """Decompiled scripts: (namespace, class name) -> guid."""
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
    """Restored source: (first namespace, file name)."""
    text = cs_path.read_text(encoding="utf-8-sig", errors="replace")
    match = NAMESPACE_PATTERN.search(text)
    namespace = ""
    if match:
        namespace = match.group(1)
    return (namespace, cs_path.stem)


def collect_referenced_guids(assets_dir):
    """Collect every guid referenced by scene, prefab and asset files."""
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
    """Entry point: delete decompiled scripts -> copy restored source -> write .meta -> report unmatched items."""
    pilot_assets = Path(sys.argv[1]) / "Assets"
    source_assets = Path(sys.argv[2])

    decompiled = build_decompiled_map(pilot_assets)
    referenced = collect_referenced_guids(pilot_assets)
    print(f"[map] guids referenced by scenes/prefabs/assets: {len(referenced)}")

    # 1. Delete the decompiled scripts (together with the directory's own .meta)
    for relative in DECOMPILED_DIRS:
        target = pilot_assets / relative
        if target.exists():
            shutil.rmtree(target)
        dir_meta = target.with_name(target.name + ".meta")
        if dir_meta.exists():
            dir_meta.unlink()
        print(f"[clean] removed {relative}")

    # 2. Copy the restored source, reusing the guid by (namespace, file name)
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

    # 3. Report: classes present in the decompiled output but unmatched in the source; focus on those referenced by assets
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
