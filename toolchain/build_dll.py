"""Compile the sources under Assets/ with Unity 3.5.7's smcs and Unity's original arguments to produce Assembly-CSharp.dll.

Following Unity's procedure:
  - Collect every .cs under Assets/, skipping Editor directories and firstpass directories (Plugins, Standard Assets)
  - Sort by path alphabetically and pass to smcs (type order is therefore determined by path)
  - Run from the repository root, with source paths written as relative Assets/... paths
  - Arguments: -debug -target:library -nowarn:0169 -out:… -r:…

smcs runs on the Mac Mono bundled with Unity 3.5.7 (toolchain/smcs_mac.sh, loaded by maloader in Linux, ~25 s).
This is how the original was built; the output is byte-identical to the original except MVID and timestamp.
On Linux the script is run directly; on Windows it goes through the WSL distro (env CKZ2_DISTRO, default ckz2-build).

Usage (in the repository root): python toolchain/build_dll.py   → build/vanilla/Assembly-CSharp.dll (plus .mdb)
"""
import os
import subprocess
import sys
from pathlib import Path

# ---- Constants ----
TOOLCHAIN = Path(__file__).resolve().parent
REPO = TOOLCHAIN.parent
ASSETS = REPO / "Assets"
OUTPUT_DIR = REPO / "build" / "vanilla"
# The assembly name is written into the #Strings heap and must match the original
OUTPUT_NAME = "Assembly-CSharp.dll"
RESPONSE_FILE = OUTPUT_DIR / "sources.rsp"
# Unity compiles scripts in these directories into other assemblies
EDITOR_DIR_NAME = "Editor"
FIRSTPASS_TOP_DIRS = {"Plugins", "Standard Assets", "Pro Standard Assets"}
SMCS_MAC_SCRIPT = TOOLCHAIN / "smcs_mac.sh"
# WSL distro created by setup.bat (only used on Windows)
WSL_DISTRO = os.environ.get("CKZ2_DISTRO", "ckz2-build")


def collect_sources():
    """Collect sources by Unity's rules; return the sorted relative paths (Assets/...)."""
    sources = []
    for path in ASSETS.rglob("*.cs"):
        relative = path.relative_to(REPO).as_posix()
        parts = relative.split("/")
        if EDITOR_DIR_NAME in parts[1:-1]:
            continue
        if len(parts) > 2 and parts[1] in FIRSTPASS_TOP_DIRS:
            continue
        sources.append(relative)
    # The original's 64 PO anchors are strictly increasing by path in TypeDef order (both case-sensitive and case-insensitive); plain string order is used here
    sources.sort()
    return sources


def write_response_file(sources):
    """Write the source list into a response file."""
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    # Paths may contain spaces (e.g. Sprite Scripts), so each path in the response file is double-quoted
    quoted = []
    for source in sources:
        quoted.append('"' + source + '"')
    RESPONSE_FILE.write_text("\n".join(quoted) + "\n", encoding="utf-8")


def linux_path(path):
    """Path as seen from Linux: unchanged on Linux, E:\\a\\b → /mnt/e/a/b on Windows."""
    resolved = Path(path).resolve()
    if os.name != "nt":
        return str(resolved)
    drive = resolved.drive.rstrip(":").lower()
    rest = resolved.as_posix()[len(resolved.drive):]
    return "/mnt/" + drive + rest


def linux_command(arguments):
    """Prefix a Linux command line with wsl on Windows (as root, like setup); return it unchanged on Linux."""
    if os.name != "nt":
        return list(arguments)
    command = ["wsl", "-d", WSL_DISTRO, "-u", "root", "--exec"]
    command.extend(arguments)
    return command


def compile_sources(sources):
    """Run smcs with the Mac Mono; return (exit code, output text)."""
    write_response_file(sources)
    command = linux_command(["bash", linux_path(SMCS_MAC_SCRIPT), linux_path(REPO), linux_path(RESPONSE_FILE),
                             linux_path(OUTPUT_DIR / OUTPUT_NAME)])
    result = subprocess.run(command, cwd=REPO, capture_output=True, text=True)
    return result.returncode, result.stdout + result.stderr


def main():
    sources = collect_sources()
    if not sources:
        print(f"no .cs files under {ASSETS}")
        return 1
    print(f"compiling {len(sources)} files (Mac mono)")
    returncode, output = compile_sources(sources)
    # On success smcs_mac.sh prints only a one-line summary (including the warning count); on failure it prints the compiler errors
    print(output.strip())
    if returncode != 0:
        print("build failed")
        return 1
    output_path = OUTPUT_DIR / OUTPUT_NAME
    print(f"built {output_path} ({output_path.stat().st_size} bytes)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
