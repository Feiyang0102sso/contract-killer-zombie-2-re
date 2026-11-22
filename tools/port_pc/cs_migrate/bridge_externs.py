"""Rewrite DllImport("__Internal") extern declarations: unchanged on iOS, forwarded to the same-named PcNative function on other platforms.

Only functions already implemented in PcPort/PcNative.cs are handled; other externs stay unchanged. Call sites are not modified.

    #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern int Foo(int size, byte[] buf);
    #else
        private static int Foo(int size, byte[] buf)
        {
            return PcNative.Foo(size, buf);
        }
    #endif

Usage: python tools/port_pc/cs_migrate/bridge_externs.py <project root> (rewrites code under Source/Runtime)
"""

import re
import sys
from pathlib import Path

PC_NATIVE_FILE = "PcPort/PcNative.cs"
# Public static method names in PcNative
PC_NATIVE_METHOD = re.compile(r"public static \S+ (\w+)\(")
# One DllImport + extern declaration (only whitespace between the attribute line and the declaration line)
EXTERN_DECL = re.compile(
    r"(?P<indent>[ \t]*)\[DllImport\(\"__Internal\"[^\]]*\)\]\s*\n"
    r"[ \t]*(?P<mods>(?:public |private |internal |protected )*)static extern "
    # Parameters may contain parentheses such as MarshalAs(...), so match up to ");"
    r"(?P<ret>[\w.\[\]<>]+) (?P<name>\w+)\((?P<params>[^;]*)\);"
)
# Attributes on parameters, e.g. [Out][MarshalAs(UnmanagedType.LPArray)]
# Require a letter right after [ to avoid stripping the [] of array types
PARAM_ATTRIBUTE = re.compile(r"\[[A-Za-z][^\]]*\]")


def load_pc_native_methods(assets_dir):
    """Read the method names already implemented in PcNative."""
    text = (assets_dir / PC_NATIVE_FILE).read_text(encoding="utf-8-sig")
    names = set()
    for name in PC_NATIVE_METHOD.findall(text):
        names.add(name)
    return names


def build_arguments(params):
    """Build the argument list from the parameter list: drop attributes and types, keep ref/out."""
    params = PARAM_ATTRIBUTE.sub("", params).strip()
    if not params:
        return ""
    arguments = []
    for part in params.split(","):
        tokens = part.split()
        name = tokens[-1]
        if tokens[0] in ("ref", "out"):
            name = tokens[0] + " " + name
        arguments.append(name)
    return ", ".join(arguments)


def make_replacement(match):
    """Generate both branches of the #if UNITY_IOS block."""
    indent = match.group("indent")
    mods = match.group("mods")
    ret = match.group("ret")
    name = match.group("name")
    params = match.group("params")
    # Parameters on the PC path do not need marshaling attributes
    plain_params = PARAM_ATTRIBUTE.sub("", params).strip()
    body_lines = []
    if name in IMPLEMENTED:
        # PcNative has a real implementation: forward the call
        arguments = build_arguments(params)
        call = f"PcNative.{name}({arguments})"
        if ret == "void":
            body_lines.append(f"{indent}\t{call};")
        else:
            body_lines.append(f"{indent}\treturn {call};")
    else:
        # Other plugins: empty stub in place, out parameters get default values, return the default of the return type
        body_lines.append(f"{indent}\tPcNative.Stub(\"{name}\");")
        for out_name in collect_out_params(plain_params):
            body_lines.append(f"{indent}\t{out_name} = default;")
        if ret != "void":
            body_lines.append(f"{indent}\treturn default({ret});")
    lines = [
        "#if UNITY_IOS",
        match.group(0),
        "#else",
        f"{indent}{mods}static {ret} {name}({plain_params})",
        f"{indent}{{",
    ]
    lines.extend(body_lines)
    lines.append(f"{indent}}}")
    lines.append("#endif")
    return "\n".join(lines)


def collect_out_params(plain_params):
    """Return the names of all out parameters."""
    names = []
    if not plain_params:
        return names
    for part in plain_params.split(","):
        tokens = part.split()
        if tokens and tokens[0] == "out":
            names.append(tokens[-1])
    return names


# Method names implemented in PcNative, filled in main()
IMPLEMENTED = set()


def main():
    """Walk all .cs files under Assets and replace every DllImport("__Internal") extern declaration."""
    assets_dir = Path(sys.argv[1]) / "Source" / "Runtime"
    implemented = load_pc_native_methods(assets_dir)
    IMPLEMENTED.update(implemented)
    print(f"[bridge] PcNative implements: {sorted(implemented)}")
    for cs_path in sorted(assets_dir.rglob("*.cs")):
        if "PcPort" in cs_path.parts:
            continue
        raw = cs_path.read_bytes()
        if b"__Internal" not in raw:
            continue
        has_crlf = b"\r\n" in raw
        bom = b""
        if raw.startswith(b"\xef\xbb\xbf"):
            bom = b"\xef\xbb\xbf"
        text = raw.decode("utf-8-sig").replace("\r\n", "\n")
        replaced = []

        def replace(match):
            # Skip declarations that are already bridged (previous line is #if UNITY_IOS)
            start = match.start()
            before = text[:start].rstrip("\n").rsplit("\n", 1)[-1]
            if before.strip() == "#if UNITY_IOS":
                return match.group(0)
            replaced.append(match.group("name"))
            return make_replacement(match)

        new_text = EXTERN_DECL.sub(replace, text)
        if not replaced:
            continue
        if has_crlf:
            new_text = new_text.replace("\n", "\r\n")
        cs_path.write_bytes(bom + new_text.encode("utf-8"))
        print(f"[bridge] {cs_path.relative_to(assets_dir)}: {', '.join(replaced)}")


if __name__ == "__main__":
    main()
