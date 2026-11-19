#!/bin/bash
# Run tools from Xcode 4.5 (4G182) inside the build Linux: clang, ld, as, otool, nm, libtool, etc.
# Usage: xcode45.sh <tool> [args...]
#   e.g.: xcode45.sh clang -x assembler-with-cpp -arch armv7 -isysroot "$SDK" -mno-thumb -g -c a.s -o a.o
#
# These tools are all x86_64 Mach-O, run with the 64-bit maloader built by setup_distro.sh.
# The clang driver locates as/ld relative to its own path, so the Xcode.app directory layout must be kept intact.
set -euo pipefail

# ---- Constants ----
LOADER=/work/ckz2/maloader/ld-mac
DEVELOPER=/work/xcode45/Xcode.app/Contents/Developer
TOOL_DIR=$DEVELOPER/Toolchains/XcodeDefault.xctoolchain/usr/bin
AS_ARM=$DEVELOPER/Toolchains/XcodeDefault.xctoolchain/usr/libexec/as/arm/as

if [ $# -lt 1 ]; then
    echo "usage: xcode45.sh <tool> [args...]" >&2
    exit 2
fi
tool=$1
shift

# `as` in usr/bin is only a dispatcher; the actual ARM assembler is libexec/as/arm
if [ "$tool" = "as" ]; then
    program=$AS_ARM
else
    program=$TOOL_DIR/$tool
fi
if [ ! -f "$program" ]; then
    echo "xcode45.sh: no such tool: $program" >&2
    exit 2
fi

# maloader prints an "undefined symbol" line to stderr for every unimplemented Darwin symbol; filter them out here
"$LOADER" "$program" "$@" 2> >(grep -v ': undefined symbol$' >&2)
