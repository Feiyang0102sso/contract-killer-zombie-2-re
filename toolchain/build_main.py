"""Build and link the complete main binary (tuned): DLL → backfill MVID → AOT → native → link; output in build/tuned/ and bin/.

Steps:
  1. toolchain/build_dll.py builds build/vanilla/Assembly-CSharp.dll with the Mac Mono (--skip-dll skips this)
  2. Backfill the original MVID and PE timestamp, require the SHA-256 of the original DLL, write to build/tuned/Managed/;
     the AOT image records the MVID, so backfill must come before AOT. LitJSON (third-party, out of scope) comes from Assets/3rd_party/
  3. toolchain/build_main.sh in Linux: copies the 6 Unity assemblies from the Unity 3.5.7 installer (byte-identical to the game's),
     generates RegisterMonoModules.cpp, AOT, assemble, native code, ld64 link in the original order (toolchain/link_order.txt)
  4. Copy the final outputs to bin/: main binary bin/ckzombies2, assemblies bin/Data/Managed/,
     same layout as ckzombies2.app, so the contents of bin/ can be copied straight over the .app.
     The main binary in bin/ gets an ad-hoc signature via toolchain/sign_adhoc.py (the original's entitlements);
     without it there is no keychain access and the game shows "cannot connect to server" on launch; build/tuned/ stays unsigned for comparison

Runs on Linux (as root, called by toolchain/build.sh) or on Windows (Linux steps go through WSL, env CKZ2_DISTRO, default ckz2-build).
Usage (in the repository root): python toolchain/build_main.py [--skip-dll]
"""
import argparse
import hashlib
import logging
import shutil
import struct
import subprocess
import sys
from pathlib import Path

import build_dll
import sign_adhoc

# ---- Constants ----
TOOLCHAIN = Path(__file__).resolve().parent
REPO = TOOLCHAIN.parent
BUILT_DLL = REPO / "build" / "vanilla" / "Assembly-CSharp.dll"
BUILD_DIR = REPO / "build" / "tuned"
MANAGED_DIR = BUILD_DIR / "Managed"
LITJSON_DLL = REPO / "Assets" / "3rd_party" / "LitJSON.dll"
ASSEMBLIES = ["Assembly-CSharp", "LitJSON", "Mono.Security", "System.Core", "System.Xml", "System",
              "UnityEngine", "mscorlib"]
BUILD_MAIN_SCRIPT = TOOLCHAIN / "build_main.sh"
MAIN_BINARY = BUILD_DIR / "ckzombies2"
BIN_DIR = REPO / "bin"
BIN_MANAGED = BIN_DIR / "Data" / "Managed"
# Identity of the original Assembly-CSharp.dll (iOS 2.0.2): smcs writes a fresh MVID and timestamp on every build.
# The AOT image records the MVID and the runtime checks it against the DLL, so both must be the original values
ORIGINAL_MVID = bytes.fromhex("7e255f8e5830794196414da55dd24a2d")  # {8e5f257e-3058-4179-9641-4da55dd24a2d}
ORIGINAL_TIMESTAMP = 0x52162418
ORIGINAL_SHA256 = "7479c3da7dbdb9ddf9eb11e88e1c168e9955f5550e50204e95c1a54470369192"
# PE / CLI metadata layout
PE_POINTER_OFFSET = 0x3C
CLI_HEADER_DIRECTORY = 14
MVID_SIZE = 16

log = logging.getLogger("build_main")


# ---- 1/2. DLL and MVID backfill ----
def rva_to_offset(sections, rva):
    """Convert an RVA to a file offset via the section table."""
    for virtual_address, virtual_size, raw_offset in sections:
        if virtual_address <= rva < virtual_address + virtual_size:
            return rva - virtual_address + raw_offset
    raise ValueError(f"RVA {rva:#x} not in any section")


def locate_identity(data):
    """Return (PE timestamp offset, MVID offset). The MVID is the first entry of the #GUID heap (Module table Mvid index is 1)."""
    pe_offset = struct.unpack_from("<I", data, PE_POINTER_OFFSET)[0]
    timestamp_offset = pe_offset + 8
    section_count = struct.unpack_from("<H", data, pe_offset + 6)[0]
    optional_size = struct.unpack_from("<H", data, pe_offset + 20)[0]
    optional_offset = pe_offset + 24
    # PE32 data directories start at optional header +96, 8 bytes each
    directory_offset = optional_offset + 96 + 8 * CLI_HEADER_DIRECTORY
    cli_rva = struct.unpack_from("<I", data, directory_offset)[0]
    sections = []
    section_offset = optional_offset + optional_size
    for index in range(section_count):
        entry = section_offset + 40 * index
        virtual_size, virtual_address, raw_size, raw_offset = struct.unpack_from("<IIII", data, entry + 8)
        sections.append((virtual_address, max(virtual_size, raw_size), raw_offset))
    cli_offset = rva_to_offset(sections, cli_rva)
    metadata_rva = struct.unpack_from("<I", data, cli_offset + 8)[0]
    metadata_offset = rva_to_offset(sections, metadata_rva)
    version_length = struct.unpack_from("<I", data, metadata_offset + 12)[0]
    cursor = metadata_offset + 16 + version_length
    stream_count = struct.unpack_from("<H", data, cursor + 2)[0]
    cursor += 4
    for index in range(stream_count):
        stream_offset, stream_size = struct.unpack_from("<II", data, cursor)
        name_end = data.index(b"\0", cursor + 8)
        name = data[cursor + 8:name_end].decode("ascii")
        cursor = (name_end + 4) & ~3
        if name == "#GUID":
            return timestamp_offset, metadata_offset + stream_offset
    raise ValueError("#GUID stream not found")


def backfill_identity(built):
    """Write the original timestamp and MVID into the rebuilt DLL and return the new bytes."""
    timestamp_offset, mvid_offset = locate_identity(built)
    patched = bytearray(built)
    struct.pack_into("<I", patched, timestamp_offset, ORIGINAL_TIMESTAMP)
    patched[mvid_offset:mvid_offset + MVID_SIZE] = ORIGINAL_MVID
    return bytes(patched)


def prepare_managed():
    """Prepare build/tuned/Managed: the backfilled Assembly-CSharp + LitJSON (build_main.sh adds the 6 Unity assemblies)."""
    MANAGED_DIR.mkdir(parents=True, exist_ok=True)
    patched = backfill_identity(BUILT_DLL.read_bytes())
    digest = hashlib.sha256(patched).hexdigest()
    if digest != ORIGINAL_SHA256:
        raise SystemExit(f"Assembly-CSharp.dll differs from original after MVID backfill (sha256 {digest})")
    (MANAGED_DIR / "Assembly-CSharp.dll").write_bytes(patched)
    log.info("Assembly-CSharp.dll identical to original after backfill (sha256 %s)", digest[:16])
    # Third-party LitJSON comes from the repo's Assets/3rd_party/ (Unity builds copy managed DLLs in Assets into Data/Managed)
    shutil.copyfile(LITJSON_DLL, MANAGED_DIR / "LitJSON.dll")


# ---- 3. AOT, native code and link in Linux ----
def run_build_main():
    command = build_dll.linux_command(["bash", build_dll.linux_path(BUILD_MAIN_SCRIPT), build_dll.linux_path(REPO),
                                       build_dll.linux_path(BUILD_DIR), "tuned"])
    log.info("running build_main.sh")
    result = subprocess.run(command)
    if result.returncode != 0:
        raise SystemExit("build_main.sh failed")


# ---- 4. Final outputs ----
def publish_bin():
    """Place the main binary (signed) and the 8 assemblies in bin/ at their .app locations, for copying over ckzombies2.app as a whole."""
    BIN_MANAGED.mkdir(parents=True, exist_ok=True)
    sign_adhoc.sign(MAIN_BINARY, BIN_DIR / MAIN_BINARY.name)
    for assembly in ASSEMBLIES:
        shutil.copy2(MANAGED_DIR / (assembly + ".dll"), BIN_MANAGED / (assembly + ".dll"))
    log.info("published %s and %d assemblies to %s", MAIN_BINARY.name, len(ASSEMBLIES), BIN_DIR)


def main():
    logging.basicConfig(level=logging.INFO, format="%(message)s")
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--skip-dll", action="store_true", help="reuse build/vanilla/Assembly-CSharp.dll")
    args = parser.parse_args()

    if not args.skip_dll:
        if build_dll.main() != 0:
            raise SystemExit("build_dll failed")
    prepare_managed()
    run_build_main()
    publish_bin()
    log.info("done: %s", BIN_DIR / MAIN_BINARY.name)
    return 0


if __name__ == "__main__":
    sys.exit(main())
