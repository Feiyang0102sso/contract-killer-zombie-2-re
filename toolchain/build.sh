#!/bin/bash
# One-click build inside the build Linux (run as root after setup_distro.sh):
# Assembly-CSharp.dll → AOT → native code → link, then the signed main binary and the 8 assemblies in <repo>/bin.
# Usage: build.sh [--skip-dll]
#   --skip-dll  reuse build/vanilla/Assembly-CSharp.dll
# Environment: SKIP_COMPILE=1 relinks only (reuses build/tuned/aot and build/tuned/native)
set -euo pipefail

TOOLCHAIN=$(cd "$(dirname "$0")" && pwd)
REPO=$(dirname "$TOOLCHAIN")

if [ "$(id -u)" != 0 ]; then
    echo "build.sh: run as root (the native build sets up the original build machine path under /Applications)" >&2
    exit 1
fi
for required in /work/ckz2/maloader/ld-mac /work/ckz2/maloader32/ld-mac /work/ckz2/darwin_shim32.so \
                /work/ckz2/unity357/BuildTargetTools/iPhonePlayer/mono-xcompiler /work/ckz2/unity357_mono/bin/mono \
                /work/xcode45/.dmg-sha256; do
    if [ ! -e "$required" ]; then
        echo "build.sh: toolchain missing ($required), run setup first (setup.bat or toolchain/setup_distro.sh)" >&2
        exit 1
    fi
done

cd "$REPO"
python3 -u "$TOOLCHAIN/build_main.py" "$@"
