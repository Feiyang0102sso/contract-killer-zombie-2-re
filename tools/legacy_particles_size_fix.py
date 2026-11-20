"""Rewrite the SizeOverLifetime curve of converted legacy particles directly in scene/prefab YAML.

Mirrors PcLegacyParticleConvert.ApplySizeGrow (size(t) = (1 + sizeGrow)^t, 5 keys, SmoothTangents(i, 0)),
without opening scenes in the editor (that churns EZGUI meshes). Only touches particles with sizeGrow != 0
listed in pc_data/particles/legacy_particles.json, matched by GameObject fileID. Safe to re-run.
Keep it in sync with ApplySizeGrow if the converter changes.

Usage (from the repo root): python tools/legacy_particles_size_fix.py . [--write]
Without --write it only reports how many particles each asset would get.
"""
import json
import math
import re
import struct
import sys
from pathlib import Path

JSON_PATH = "pc_data/particles/legacy_particles.json"
SIZE_CURVE_KEYS = 5
MIN_GROWTH_BASE = 0.0001
KEY_TEMPLATE = """        - serializedVersion: 3
          time: {time}
          value: {value}
          inSlope: {slope}
          outSlope: {slope}
          tangentMode: 0
          weightedMode: 0
          inWeight: {in_weight}
          outWeight: {out_weight}
"""


def to_float32(value):
    """Round a double to float32."""
    return struct.unpack("<f", struct.pack("<f", value))[0]


def format_float(value):
    """Shortest float32 text without exponent, like Unity's YAML writer."""
    value = to_float32(value)
    if value == int(value) and abs(value) < 1e7:
        return str(int(value))
    text = ""
    for digits in range(1, 10):
        text = "%.*g" % (digits, value)
        if to_float32(float(text)) == value:
            break
    if "e" in text:
        # Unity writes plain decimals, so expand the exponent form with the same significant digits
        mantissa, exponent = text.split("e")
        digit_count = len(mantissa.replace("-", "").replace(".", ""))
        decimals = max(0, digit_count - 1 - int(exponent))
        text = format(float(text), "." + str(decimals) + "f")
    return text


def build_curve(entry):
    """Returns (multiplier, list of (time, value, slope))."""
    mean_energy = (entry["minEnergy"] + entry["maxEnergy"]) * 0.5
    growth_base = max(1.0 + entry["sizeGrow"], MIN_GROWTH_BASE)
    times = []
    values = []
    for i in range(SIZE_CURVE_KEYS):
        time = i / (SIZE_CURVE_KEYS - 1)
        times.append(time)
        values.append(to_float32(math.pow(growth_base, time * mean_energy)))
    max_value = max(values)
    normalized = []
    for value in values:
        normalized.append(to_float32(value / max_value))
    # SmoothTangents(i, 0): one-sided difference at the ends, central difference inside
    keys = []
    last = SIZE_CURVE_KEYS - 1
    for i in range(SIZE_CURVE_KEYS):
        if i == 0:
            slope = (normalized[1] - normalized[0]) / (times[1] - times[0])
        elif i == last:
            slope = (normalized[last] - normalized[last - 1]) / (times[last] - times[last - 1])
        else:
            slope = (normalized[i + 1] - normalized[i - 1]) / (times[i + 1] - times[i - 1])
        keys.append((times[i], normalized[i], slope))
    return max_value, keys


def curve_yaml(keys):
    """m_Curve list text for the given keys."""
    text = "        m_Curve:\n"
    last = len(keys) - 1
    for index, key in enumerate(keys):
        in_weight = "0.33333334"
        out_weight = "0.33333334"
        if index == 0:
            in_weight = "0"
        if index == last:
            out_weight = "0"
        text += KEY_TEMPLATE.format(time=format_float(key[0]), value=format_float(key[1]), slope=format_float(key[2]),
                                    in_weight=in_weight, out_weight=out_weight)
    return text


def patch_document(document, entry):
    """Rewrites scalar and maxCurve inside SizeModule.curve of one ParticleSystem document."""
    module_start = document.index("  SizeModule:\n")
    curve_start = document.index("    curve:\n", module_start)
    curve_end = document.index("    y:\n", curve_start)
    section = document[curve_start:curve_end]
    multiplier, keys = build_curve(entry)
    section, count = re.subn(r"\n      scalar: [^\n]+\n", "\n      scalar: " + format_float(multiplier) + "\n", section, count=1)
    if count != 1:
        raise RuntimeError("scalar not found: " + entry["gameObjectName"])
    max_start = section.index("      maxCurve:\n")
    list_start = section.index("        m_Curve:\n", max_start)
    list_end = section.index("        m_PreInfinity:", list_start)
    section = section[:list_start] + curve_yaml(keys) + section[list_end:]
    return document[:curve_start] + section + document[curve_end:]


def main():
    root = Path(sys.argv[1])
    write = "--write" in sys.argv
    entries = json.loads((root / JSON_PATH).read_text(encoding="utf-8"))["entries"]
    by_asset = {}
    for entry in entries:
        if entry["sizeGrow"] == 0:
            continue
        by_asset.setdefault(entry["assetPath"], {})[entry["gameObjectFileId"]] = entry
    total = 0
    for asset_path, by_id in by_asset.items():
        path = root / asset_path
        text = path.read_text(encoding="utf-8")
        documents = re.split(r"(?=^--- !u!)", text, flags=re.M)
        patched = 0
        for index, document in enumerate(documents):
            if not document.startswith("--- !u!198 "):
                continue
            match = re.search(r"\n  m_GameObject: \{fileID: (-?\d+)\}", document)
            if match is None or match.group(1) not in by_id:
                continue
            documents[index] = patch_document(document, by_id[match.group(1)])
            patched += 1
        print(f"{asset_path}: patched {patched} / {len(by_id)}")
        total += patched
        if write:
            path.write_text("".join(documents), encoding="utf-8", newline="")
    print("total patched", total)


if __name__ == "__main__":
    main()
