#!/bin/bash
# Build and link the complete main executable ckzombies2 (run inside the build Linux). Called by toolchain/build_main.py.
# Usage: build_main.sh <repo> <build_dir> [tuned]
#   Input: <build_dir>/Managed/ with Assembly-CSharp.dll (MVID backfilled) and LitJSON.dll, prepared by build_main.py
#   Steps: copy the 6 Unity assemblies from the installer → generate RegisterMonoModules.cpp → AOT → assemble → native → link
#   Link order: toolchain/link_order.txt, the object names in the original debug map's N_OSO order (archive members excluded)
# Outputs: <build_dir>/aot/*.dll.{s,o}, <build_dir>/native/*.o, <build_dir>/RegisterMonoModules.cpp, <build_dir>/ckzombies2
#
# Only tuned is supported: vanilla still lacks 11 glue units and the third-party SDK libraries (Tapjoy, Kontagent, Flurry, MAT, PlayHaven,
# FacebookSDK).
#
# Where the link arguments come from:
#   - Unity 3.5.7 template, Release: OTHER_LDFLAGS = -all_load -weak_framework CoreMotion -weak-lSystem (Glu removed -Wl,-S,-x;
#     the original keeps full STABS), plus -dead_strip (see LOAD_FLAGS); IPHONEOS_DEPLOYMENT_TARGET taken from the original's LC_VERSION_MIN 5.1
#   - added by the clang-421 driver (clang -###): -demangle -dynamic, crt1.3.1.o, -lSystem, libclang_rt.ios.a
#   - dylibs listed one by one following the original's LC_LOAD_DYLIB / LC_LOAD_WEAK_DYLIB order and weak-link attributes
set -euo pipefail

# ---- Constants ----
TOOLCHAIN=$(cd "$(dirname "$0")" && pwd)
XCODE45="$TOOLCHAIN/xcode45.sh"
DEVELOPER=/work/xcode45/Xcode.app/Contents/Developer
SDK=$DEVELOPER/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS6.0.sdk
CLANG_RT=$DEVELOPER/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang/4.1/lib/darwin/libclang_rt.ios.a
UNITY_LIB=/work/ckz2/unity357/PlaybackEngines/iPhonePlayer/iPhone-Trampoline/Libraries/libiPhone-lib.a
# The game's bundled Unity assemblies are byte-identical to these installer files (UnityEngine.dll is the iOS build)
UNITY_ENGINE=/work/ckz2/unity357/PlaybackEngines/iPhonePlayer/Managed/UnityEngine.dll
UNITY_MONO_DIR=/work/ckz2/unity357_mono
UNITY_PROFILE=$UNITY_MONO_DIR/lib/mono/unity
UNITY_PROFILE_ASSEMBLIES="Mono.Security System.Core System.Xml System mscorlib"
UNITY_VERSION=3.5.7f6
LOADER32=/work/ckz2/maloader32/ld-mac
SHIM=/work/ckz2/darwin_shim32.so
GENERATOR_SOURCE=$TOOLCHAIN/register_mono_modules/RegisterMonoModulesGen.cs
LINK_ORDER=$TOOLCHAIN/link_order.txt
MIN_VERSION=5.1
# The template has only -all_load; the original additionally enables dead code stripping (DEAD_CODE_STRIPPING = YES). Evidence: with only -all_load,
# the FMOD vorbis members of libiPhone-lib.a reference nonexistent symbols and the link fails (the original indeed lacks these functions); with only -ObjC the member set is wrong;
# -all_load -dead_strip pulls in libiPhone-lib.a members (551) identical to the original debug map, in the same order.
# Can be overridden via the LOAD_FLAGS environment variable for experiments
LOAD_FLAGS=${LOAD_FLAGS:--all_load -dead_strip}
# SKIP_COMPILE=1: reuse the existing aot/ and native/, relink only
SKIP_COMPILE=${SKIP_COMPILE:-0}
# AOT assembler flags taken from the Unity Xcode project template (see toolchain/README.md)
ASSEMBLE_FLAGS="-x assembler-with-cpp -arch armv7 -isysroot $SDK -miphoneos-version-min=$MIN_VERSION -mno-thumb -g"
ASSEMBLIES="Assembly-CSharp LitJSON Mono.Security System.Core System.Xml System UnityEngine mscorlib"
# Original dylib order (W = weak link). tuned does not need the libraries used by the SDKs but links them anyway, so the load commands match the original
DYLIBS=(
    "W framework CoreMotion"
    "W lib System"
    "- framework Security"
    "- framework StoreKit"
    "W framework Foundation"
    "- framework UIKit"
    "- framework OpenGLES"
    "- framework QuartzCore"
    "- framework OpenAL"
    "- lib iconv.2"
    "- framework AudioToolbox"
    "- framework CFNetwork"
    "- framework MediaPlayer"
    "- framework CoreLocation"
    "- framework SystemConfiguration"
    "W framework iAd"
    "W framework CoreMedia"
    "W framework CoreVideo"
    "W framework AVFoundation"
    "- framework CoreGraphics"
    "W framework GameKit"
    "- framework MobileCoreServices"
    "- framework CoreTelephony"
    "W framework AdSupport"
    "W framework Accounts"
    "W framework Social"
    "- lib sqlite3"
    "- framework CoreData"
    "- lib stdc++.6"
    "- framework CoreFoundation"
    "- lib objc"
)

if [ $# -lt 2 ]; then
    echo "usage: build_main.sh <repo> <build_dir> [tuned]" >&2
    exit 2
fi
repo=$(realpath "$1")
build_dir=$(realpath "$2")
flavor=${3:-tuned}
if [ "$flavor" != "tuned" ]; then
    echo "build_main.sh: only tuned can be linked (vanilla lacks the SDK glue and libraries)" >&2
    exit 2
fi
managed=$build_dir/Managed
aot_dir=$build_dir/aot
native_dir=$build_dir/native
output=$build_dir/ckzombies2
mkdir -p "$aot_dir"

# Run a .NET program on the Mac Mono (maloader prints an "undefined symbol" line for every unimplemented Darwin symbol; drop them)
mac_mono() {
    MONO_PATH=$UNITY_PROFILE MONO_CFG_DIR=$UNITY_MONO_DIR/etc GC_DONT_GC="yes please"         LD_PRELOAD=$SHIM "$LOADER32" "$UNITY_MONO_DIR/bin/mono" "$@" 2> >(grep -v 'undefined symbol' >&2)
}

if [ "$SKIP_COMPILE" != "1" ]; then
# ---- 0. Unity assemblies and RegisterMonoModules.cpp ----
echo "== managed"
cp -f "$UNITY_ENGINE" "$managed/"
for name in $UNITY_PROFILE_ASSEMBLIES; do
    cp -f "$UNITY_PROFILE/$name.dll" "$managed/"
done
for name in $ASSEMBLIES; do
    test -f "$managed/$name.dll" || { echo "missing $managed/$name.dll" >&2; exit 1; }
done
# Generator following Unity's own logic (Mono.Cecil 0.6 from the unity profile); recompiled when the source is newer
generator=$build_dir/tools/RegisterMonoModulesGen.exe
mkdir -p "$build_dir/tools"
if [ ! -f "$generator" ] || [ "$GENERATOR_SOURCE" -nt "$generator" ]; then
    mac_mono "$UNITY_PROFILE/smcs.exe" "-r:$UNITY_PROFILE/Mono.Cecil.dll" "-out:$generator" "$GENERATOR_SOURCE" > /dev/null
fi
mac_mono "$generator" "$build_dir/RegisterMonoModules.cpp" "$UNITY_VERSION" "$managed"/*.dll

# ---- 1. AOT: 8 assemblies in parallel ----
echo "== AOT"
pids=()
for name in $ASSEMBLIES; do
    bash "$TOOLCHAIN/aot.sh" "$managed/$name.dll" "$aot_dir/$name.dll.s" &
    pids+=($!)
done
for pid in "${pids[@]}"; do
    wait "$pid"
done

# ---- 2. Assemble AOT output ----
echo "== assemble"
pids=()
for name in $ASSEMBLIES; do
    # shellcheck disable=SC2086
    bash "$XCODE45" clang $ASSEMBLE_FLAGS -c "$aot_dir/$name.dll.s" -o "$aot_dir/$name.dll.o" &
    pids+=($!)
done
for pid in "${pids[@]}"; do
    wait "$pid"
done

# ---- 3. Native code (Classes, plugins, stubs, RegisterMonoModules) ----
echo "== native"
REGISTER_MONO_MODULES="$build_dir/RegisterMonoModules.cpp" bash "$TOOLCHAIN/build_native.sh" "$repo" "$native_dir" "$flavor"
fi

# ---- 4. Link ----
echo "== link"
filelist=$build_dir/link_filelist.txt
: > "$filelist"
while read -r object; do
    case "$object" in
        ''|'#'*) continue ;;
    esac
    if [ -f "$aot_dir/$object" ]; then
        echo "$aot_dir/$object" >> "$filelist"
    elif [ -f "$native_dir/$object" ]; then
        echo "$native_dir/$object" >> "$filelist"
    else
        echo "missing object in link order: $object" >&2
        exit 1
    fi
done < "$LINK_ORDER"
# Objects that were built but are absent from the original link order (none in theory) go last
for object in "$native_dir"/*.o; do
    if ! grep -qxF "$object" "$filelist"; then
        echo "extra object (not in original link order): $(basename "$object")" >&2
        echo "$object" >> "$filelist"
    fi
done

# shellcheck disable=SC2206
arguments=(-demangle -dynamic -arch armv7 $LOAD_FLAGS -iphoneos_version_min "$MIN_VERSION.0"
           -syslibroot "$SDK" -o "$output" -lcrt1.3.1.o -filelist "$filelist" "$UNITY_LIB")
for entry in "${DYLIBS[@]}"; do
    read -r weak kind name <<< "$entry"
    if [ "$kind" = "framework" ]; then
        if [ "$weak" = "W" ]; then
            arguments+=(-weak_framework "$name")
        else
            arguments+=(-framework "$name")
        fi
    else
        if [ "$weak" = "W" ]; then
            arguments+=("-weak-l$name")
        else
            arguments+=("-l$name")
        fi
    fi
done
arguments+=(-lSystem "$CLANG_RT")
bash "$XCODE45" ld "${arguments[@]}"
ls -l "$output"
echo "linked $output"
