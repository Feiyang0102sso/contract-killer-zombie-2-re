#!/bin/bash
# Cross-compile a managed DLL to armv7 assembly with Unity 3.5.7's mono-xcompiler (run inside the build Linux).
# Usage: aot.sh <dll> <out.s>
#   The DLL's directory must contain every assembly it references (mscorlib, UnityEngine, ...), i.e. Unity's Data/Managed at build time.
#
# Arguments and environment variables copied from Unity 3.5.7 UnityEditor.MonoCrossCompile.CrossCompileAOT (iOS, Release build):
#   CrossCompileOptions = Static; FastICall is enabled only when scriptCallOptimization = FastButNoExceptions,
#   and per-method comparison with the original confirms it is off; Development/AllowDebugging are both off, hence nodebug and no explicit-null-checks.
#   PlayerSettings.aotOptions = "ntrampolines=4096,nrgctx-trampolines=4096,nimt-trampolines=4096"; Unity inserts it verbatim
#   between nodebug and static and passes it for every assembly. These three options only affect mscorlib (trampolines are generated only in the corlib image),
#   so comparing only Assembly-CSharp and LitJSON cannot reveal them; evidence: after linking, the original mscorlib's specific_trampolines (81920 bytes),
#   static_rgctx_trampolines (98304) and imt_thunks (262144) are 4x, 4x and 32x the defaults of 1024, 1024 and 128 entries.
#   The order of the three within the string cannot be determined from the output and does not affect it.
set -euo pipefail

# ---- Constants ----
LOADER32=/work/ckz2/maloader32/ld-mac
SHIM=/work/ckz2/darwin_shim32.so
XCOMPILER=/work/ckz2/unity357/BuildTargetTools/iPhonePlayer/mono-xcompiler
PLAYER_AOT_OPTIONS="ntrampolines=4096,nrgctx-trampolines=4096,nimt-trampolines=4096"
AOT_OPTIONS="full,asmonly,nodebug,$PLAYER_AOT_OPTIONS,static"

if [ $# -ne 2 ]; then
    echo "usage: aot.sh <dll> <out.s>" >&2
    exit 2
fi
input=$(realpath "$1")
output=$(realpath -m "$2")
managed_dir=$(dirname "$input")
name=$(basename "$input")

# Unity runs the compiler inside the assembly directory: the output file is written there first, then moved to the target location
cd "$managed_dir"
export MONO_PATH=$managed_dir
export GAC_PATH=$managed_dir
export GC_DONT_GC="yes please"
LD_PRELOAD=$SHIM "$LOADER32" "$XCOMPILER" "--aot=$AOT_OPTIONS,outfile=$name.s" "$name" > "$managed_dir/$name.aot.log" 2>&1 || {
    # maloader prints an "undefined symbol" line for every unimplemented Darwin symbol; on failure, show only the other lines
    grep -v "undefined symbol" "$managed_dir/$name.aot.log" | tail -20 >&2
    exit 1
}
if [ "$managed_dir/$name.s" != "$output" ]; then
    mv -f "$managed_dir/$name.s" "$output"
fi
summary=$(grep "^Compiled" "$managed_dir/$name.aot.log")
rm -f "$managed_dir/$name.aot.log"
echo "aot $name: $summary -> $output"
