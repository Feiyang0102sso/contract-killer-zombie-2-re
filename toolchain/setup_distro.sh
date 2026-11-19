#!/bin/bash
# One-time toolchain setup inside the build Linux (Ubuntu 24.04 x86_64, run as root).
# On Windows, setup.bat creates the WSL distro and calls this script; on a native Ubuntu run it with sudo.
#
# Usage: setup_distro.sh [xcode-dmg]
#   xcode-dmg  Xcode 4.5 (4G182) installer, default toolchain/downloads/Xcode_4.5.dmg (needs an Apple ID, see toolchain/README.md)
#   The Unity 3.5.7 macOS installer is expected at toolchain/downloads/unity-3.5.7.dmg (public; downloaded here when missing)
#
# Steps (each one is skipped when its result already exists, so re-running is cheap):
#   1. apt packages, including the 32-bit build environment (i386 architecture)
#   2. maloader (runs Apple's x86_64 Mach-O tools on Linux) at a fixed commit + 3 fixes (patches/maloader.patch)
#   3. maloader32: the same tree + patches/maloader32.patch, built 32-bit (mono-xcompiler and the Mac mono are i386 Mach-O)
#   4. darwin_shim32.so: Darwin functions missing for the i386 programs (injected via LD_PRELOAD), plus Apple Libc's qsort
#   5. Unity 3.5.7 macOS installer → /work/ckz2/unity357 (iOS build tools, Xcode project template, iOS UnityEngine.dll)
#      and /work/ckz2/unity357_mono (the editor's Mono: runs smcs to reproduce the original DLL byte for byte)
#   6. Xcode 4.5 → /work/xcode45 (clang-421.11.65, ld64-134.9, cctools-836 as, iOS 6.0 SDK)
#   7. Self check: versions of every tool; AOT + assemble LitJSON.dll once
# All in-distro paths are the ones the build scripts use; they also end up in the debug info of the objects, so do not change them.
set -euo pipefail

TOOLCHAIN=$(cd "$(dirname "$0")" && pwd)
REPO=$(dirname "$TOOLCHAIN")
XCODE_DMG=${1:-$TOOLCHAIN/downloads/Xcode_4.5.dmg}

# ---- Constants ----
WORK=/work/ckz2
PROBE=$WORK/probe
MALOADER_URL=https://github.com/shinh/maloader.git
MALOADER_COMMIT=464a90fdfd06a54c9da5d1a3725ed6229c0d3d60
MALOADER=$WORK/maloader
LOADER=$MALOADER/ld-mac
MALOADER32=$WORK/maloader32
LOADER32=$MALOADER32/ld-mac
SHIM=$WORK/darwin_shim32.so
UNITY_DMG=$TOOLCHAIN/downloads/unity-3.5.7.dmg
UNITY_DMG_URL=https://download.unity3d.com/download_unity/unity-3.5.7.dmg
UNITY_DMG_SHA256=f4277807286bb012dd5a1e4898c8e71c7980f12bffc77a3ebc51b03be3e60ab1
UNITY_DIR=$WORK/unity357
UNITY_CONTENTS=Unity/Unity.app/Contents
XCOMPILER=$UNITY_DIR/BuildTargetTools/iPhonePlayer/mono-xcompiler
# Mono bundled with the Unity editor (i386 Mach-O); on Mac, Unity uses it to run smcs to compile Assembly-CSharp.dll
UNITY_MONO_DIR=$WORK/unity357_mono
MAC_MONO=$UNITY_MONO_DIR/bin/mono
UNITY_PROFILE=$UNITY_MONO_DIR/lib/mono/unity
XCODE_DMG_SHA256=f7c8e0cd0a212d8af0b468187e7d3843fb3b88dcbd1dd6765f93254194dec440
XCODE_DIR=/work/xcode45
# libssl-dev: maloader dlopens the unversioned libcrypto.so at run time (ld64 computes its UUID with MD5)
APT_PACKAGES="make g++ g++-multilib git ca-certificates curl rsync python3 7zip cpio file uuid-dev zlib1g-dev libssl-dev"
APT_PACKAGES_I386="uuid-dev:i386 zlib1g-dev:i386"
# Assembler flags for AOT .s (same as build_main.sh)
SDK=$XCODE_DIR/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS6.0.sdk
AOT_ASSEMBLE_FLAGS="-x assembler-with-cpp -arch armv7 -isysroot $SDK -miphoneos-version-min=5.1 -mno-thumb -g"

if [ "$(id -u)" != 0 ]; then
    echo "setup_distro.sh: run as root (WSL: wsl -u root, Linux: sudo)" >&2
    exit 1
fi
if [ "$(uname -m)" != x86_64 ]; then
    echo "setup_distro.sh: maloader needs an x86_64 Linux, this is $(uname -m)" >&2
    exit 1
fi
mkdir -p "$WORK" "$PROBE"

echo "== 1/7 apt packages"
export DEBIAN_FRONTEND=noninteractive
if ! dpkg --print-foreign-architectures | grep -q i386; then
    dpkg --add-architecture i386
fi
missing_packages=""
for package in $APT_PACKAGES $APT_PACKAGES_I386; do
    if ! dpkg -s "$package" >/dev/null 2>&1; then
        missing_packages="$missing_packages $package"
    fi
done
if [ -n "$missing_packages" ]; then
    apt-get update -q
    # shellcheck disable=SC2086
    apt-get install -y -q --no-install-recommends $missing_packages
else
    echo "already installed"
fi

echo "== 2/7 maloader $MALOADER_COMMIT"
patch_hash=$(sha256sum "$TOOLCHAIN/patches/maloader.patch" | cut -d' ' -f1)
if [ -x "$LOADER" ] && [ "$(cat "$MALOADER/.ckz2-patch.sha256" 2>/dev/null)" = "$patch_hash" ]; then
    echo "already built: $LOADER"
else
    rm -rf "$MALOADER"
    mkdir -p "$MALOADER"
    cd "$MALOADER"
    git init -q
    git fetch -q --depth 1 "$MALOADER_URL" "$MALOADER_COMMIT"
    git checkout -q FETCH_HEAD
    # glibc >= 2.32 header, N_REF_TO_WEAK on undefined symbols, extrel binding
    git apply "$TOOLCHAIN/patches/maloader.patch"
    make release > build.log 2>&1 || { tail -30 build.log; echo "maloader build failed (log $MALOADER/build.log)" >&2; exit 1; }
    echo "$patch_hash" > .ckz2-patch.sha256
    echo "built: $LOADER"
fi

echo "== 3/7 maloader32"
patch_hash=$(cat "$TOOLCHAIN/patches/maloader.patch" "$TOOLCHAIN/patches/maloader32.patch" | sha256sum | cut -d' ' -f1)
if [ -x "$LOADER32" ] && [ "$(cat "$MALOADER32/.ckz2-patch.sha256" 2>/dev/null)" = "$patch_hash" ]; then
    echo "already built: $LOADER32"
else
    rm -rf "$MALOADER32"
    rsync -a --exclude=.git "$MALOADER/" "$MALOADER32/"
    cd "$MALOADER32"
    patch -p1 < "$TOOLCHAIN/patches/maloader32.patch"
    make clean >/dev/null
    # the release target overrides GCC_EXTRA_FLAGS and drops -m32, so pass it directly
    make all BITS=32 "GCC_EXTRA_FLAGS=-m32 -DNOLOG -DNDEBUG" > build.log 2>&1 || { tail -20 build.log; exit 1; }
    echo "$patch_hash" > .ckz2-patch.sha256
    echo "built: $LOADER32"
fi

echo "== 4/7 darwin_shim32.so"
gcc -m32 -shared -fPIC -O2 -Wall -o "$SHIM" "$TOOLCHAIN/darwin_shim/darwin_shim.c" -lpthread
echo "built: $SHIM"

echo "== 5/7 Unity 3.5.7"
if [ -x "$XCOMPILER" ] && [ -f "$MAC_MONO" ]; then
    echo "already extracted: $UNITY_DIR, $UNITY_MONO_DIR"
else
    if [ ! -f "$UNITY_DMG" ]; then
        echo "download $UNITY_DMG_URL (about 980 MB)"
        mkdir -p "$(dirname "$UNITY_DMG")"
        curl -L --fail -o "$UNITY_DMG.part" "$UNITY_DMG_URL"
        mv -f "$UNITY_DMG.part" "$UNITY_DMG"
    fi
    echo "checking sha256 of $UNITY_DMG"
    echo "$UNITY_DMG_SHA256  $UNITY_DMG" | sha256sum --check --quiet
    stage=$PROBE/unity_extract
    rm -rf "$stage"
    mkdir -p "$stage/root"
    cd "$stage"
    # dmg (HFS+) -> Unity.pkg (xar) -> Unity.pkg/Payload (gzip cpio); extract only what the build needs
    7z x -y -bd "$UNITY_DMG" "unity/Unity.pkg" >/dev/null
    7z x -y -bd unity/Unity.pkg "Unity.pkg/Payload" >/dev/null
    cd root
    gzip -dc ../Unity.pkg/Payload | cpio -id --quiet \
        "./$UNITY_CONTENTS/BuildTargetTools/iPhonePlayer/*" \
        "./$UNITY_CONTENTS/PlaybackEngines/iPhonePlayer/*" \
        "./$UNITY_CONTENTS/Frameworks/Managed/UnityEditor.dll" \
        "./$UNITY_CONTENTS/Frameworks/Mono/*"
    rm -rf "$UNITY_DIR" "$UNITY_MONO_DIR"
    mkdir -p "$UNITY_DIR" "$UNITY_MONO_DIR"
    mv "$UNITY_CONTENTS/BuildTargetTools" "$UNITY_CONTENTS/PlaybackEngines" "$UNITY_DIR/"
    # UnityEditor.dll is not used by the build; kept as the reference for Unity's build logic (decompile it to check)
    mv "$UNITY_CONTENTS/Frameworks/Managed/UnityEditor.dll" "$UNITY_DIR/"
    # Frameworks/Mono: bin/mono, smcs and the 2.0.5.0 assemblies in lib/mono/unity
    mv "$UNITY_CONTENTS/Frameworks/Mono/"* "$UNITY_MONO_DIR/"
    chmod +x "$XCOMPILER"
    cd "$WORK"
    rm -rf "$stage"
    echo "extracted: $UNITY_DIR ($(du -sh "$UNITY_DIR" | cut -f1)), $UNITY_MONO_DIR ($(du -sh "$UNITY_MONO_DIR" | cut -f1))"
fi

echo "== 6/7 Xcode 4.5 (4G182)"
if [ "$(cat "$XCODE_DIR/.dmg-sha256" 2>/dev/null)" = "$XCODE_DMG_SHA256" ]; then
    echo "already extracted: $XCODE_DIR"
else
    if [ ! -f "$XCODE_DMG" ]; then
        echo "Xcode installer not found: $XCODE_DMG" >&2
        echo "Download Xcode 4.5 (xcode_4.5.dmg, 4G182) from developer.apple.com with your Apple ID" >&2
        echo "and put it at toolchain/downloads/Xcode_4.5.dmg, or pass its path as the first argument." >&2
        exit 1
    fi
    echo "checking sha256 of $XCODE_DMG"
    echo "$XCODE_DMG_SHA256  $XCODE_DMG" | sha256sum --check --quiet
    # The dmg directly contains Xcode/Xcode.app (about 3.5 GB, about 1 minute). 7z refuses 58 symlinks pointing to a parent directory or an absolute path,
    # and exits with code 2; accept that as long as all errors are of this kind. The only one affecting the iOS toolchain is nameser.h, restored below
    stage=$PROBE/xcode_extract
    rm -rf "$stage"
    7z x -y -bd "-o$stage" "$XCODE_DMG" > "$PROBE/xcode_extract.log" 2>&1 || true
    other_errors=$(grep '^ERROR:' "$PROBE/xcode_extract.log" | grep -v 'link path was ignored' || true)
    if [ -n "$other_errors" ] || [ ! -x "$stage/Xcode/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang" ]; then
        echo "$other_errors" | head -20 >&2
        echo "Xcode extraction failed, log: $PROBE/xcode_extract.log" >&2
        exit 1
    fi
    rm -rf "$XCODE_DIR"
    mkdir -p "$XCODE_DIR"
    mv "$stage/Xcode/Xcode.app" "$XCODE_DIR/Xcode.app"
    rm -rf "$stage" "$PROBE/xcode_extract.log"
    ln -sfn ../nameser.h "$SDK/usr/include/arpa/nameser.h"
    echo "$XCODE_DMG_SHA256" > "$XCODE_DIR/.dmg-sha256"
    echo "extracted: $XCODE_DIR"
fi

echo "== 7/7 self check"
version=$(LD_PRELOAD=$SHIM "$LOADER32" "$XCOMPILER" --version 2>/dev/null | head -1)
echo "$version"
echo "$version" | grep -q "2.6.5" || { echo "self check failed: mono-xcompiler does not run" >&2; exit 1; }
smcs_version=$(MONO_PATH=$UNITY_PROFILE MONO_CFG_DIR=$UNITY_MONO_DIR/etc GC_DONT_GC="yes please" \
    LD_PRELOAD=$SHIM "$LOADER32" "$MAC_MONO" "$UNITY_PROFILE/smcs.exe" --version 2>/dev/null | head -1)
echo "$smcs_version (Mac mono)"
echo "$smcs_version" | grep -q "2.6.5.0" || { echo "self check failed: Mac mono cannot run smcs" >&2; exit 1; }
# ld -v exits non-zero without inputs, so only the printed version is checked; without -o, as leaves an empty a.out
clang_version=$(bash "$TOOLCHAIN/xcode45.sh" clang --version 2>&1 | head -1)
ld_version=$(bash "$TOOLCHAIN/xcode45.sh" ld -v 2>&1 | head -1 || true)
as_version=$(bash "$TOOLCHAIN/xcode45.sh" as -v -o /dev/null < /dev/null 2>&1 | head -1 || true)
echo "$clang_version"
echo "$ld_version"
echo "$as_version"
echo "$clang_version" | grep -q "clang-421.11.65" || { echo "self check failed: clang-421.11.65" >&2; exit 1; }
echo "$ld_version" | grep -q "ld64-134.9" || { echo "self check failed: ld64-134.9" >&2; exit 1; }
echo "$as_version" | grep -q "cctools-836" || { echo "self check failed: as cctools-836" >&2; exit 1; }
# AOT + assemble the third-party LitJSON.dll (small, references only the framework assemblies)
selftest=$PROBE/selftest
rm -rf "$selftest"
mkdir -p "$selftest"
cp "$REPO/Assets/3rd_party/LitJSON.dll" "$selftest/"
for name in mscorlib System System.Core System.Xml Mono.Security; do
    cp "$UNITY_PROFILE/$name.dll" "$selftest/"
done
bash "$TOOLCHAIN/aot.sh" "$selftest/LitJSON.dll" "$selftest/LitJSON.dll.s"
# shellcheck disable=SC2086
bash "$TOOLCHAIN/xcode45.sh" clang $AOT_ASSEMBLE_FLAGS -c "$selftest/LitJSON.dll.s" -o "$selftest/LitJSON.dll.o"
echo "assembled LitJSON.dll.o ($(stat -c %s "$selftest/LitJSON.dll.o") bytes)"
rm -rf "$selftest"
echo "setup done"
