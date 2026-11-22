"""Fix: the newer XmlSerializer does not allow nullable types (bool? int? ...) to be marked [XmlAttribute].

Mono 2.6 in Unity 3.5 allows this, but the newer version throws as soon as the serializer is constructed, so the
whole XML (asset bundle index, store config) fails to parse. Fix: mark the nullable field itself with [XmlIgnore]
and add a string proxy property with the same attribute name to handle reading and writing.
When the value is null the proxy returns null, so XmlSerializer omits the attribute, matching the original behavior.

Usage: python tools/portNewUnity/cs_migrate/nullable_xml_fix.py <project root> (rewrites code under Source/Runtime)
"""

import re
import sys
from pathlib import Path

# Nullable field: [XmlAttribute("name")] followed on the next line by public T? fieldName;
NULLABLE_ATTRIBUTE = re.compile(
    r'(?P<indent>[ \t]*)\[XmlAttribute(?:\((?:AttributeName\s*=\s*)?"(?P<xml>[^"]*)"\))?\][ \t]*\n'
    # The type may contain dots (nested enum, e.g. PlayerData.Slots)
    r'[ \t]*(?P<access>public|private|protected|internal) (?P<type>[\w.]+)\? (?P<name>\w+);'
)

# Parse expression for each type ({v} is the string value)
PARSERS = {
    "int": "XmlConvert.ToInt32({v})",
    "long": "XmlConvert.ToInt64({v})",
    "double": "XmlConvert.ToDouble({v})",
    "decimal": "XmlConvert.ToDecimal({v})",
    "bool": "PcXml.ParseBool({v})",
}

# Suffix of the proxy property name
PROXY_SUFFIX = "_XmlText"

def make_replacement(match):
    """Generate the [XmlIgnore] field plus the string proxy property."""
    i = match.group("indent")
    xml_name = match.group("xml") or match.group("name")
    access = match.group("access")
    value_type = match.group("type")
    name = match.group("name")
    if value_type in PARSERS:
        parser = PARSERS[value_type].format(v="value")
        formatter = f"XmlConvert.ToString({name}.Value)"
    else:
        # Anything else is treated as an enum: XmlSerializer reads/writes by member name by default
        parser = f"({value_type})System.Enum.Parse(typeof({value_type}), value)"
        formatter = f"{name}.Value.ToString()"
    lines = [
        f"{i}[XmlIgnore]",
        f"{i}{access} {value_type}? {name};",
        "",
        f"{i}/// <summary>PC 移植：{name} 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>",
        f"{i}[XmlAttribute(\"{xml_name}\")]",
        f"{i}public string {name}{PROXY_SUFFIX}",
        f"{i}{{",
        f"{i}\tget",
        f"{i}\t{{",
        f"{i}\t\tif (!{name}.HasValue)",
        f"{i}\t\t{{",
        f"{i}\t\t\treturn null;",
        f"{i}\t\t}}",
        f"{i}\t\treturn {formatter};",
        f"{i}\t}}",
        f"{i}\tset",
        f"{i}\t{{",
        f"{i}\t\tif (string.IsNullOrEmpty(value))",
        f"{i}\t\t{{",
        f"{i}\t\t\t{name} = null;",
        f"{i}\t\t\treturn;",
        f"{i}\t\t}}",
        f"{i}\t\t{name} = {parser};",
        f"{i}\t}}",
        f"{i}}}",
    ]
    return "\n".join(lines)


def main():
    """Walk all .cs files under Assets and rewrite them."""
    assets_dir = Path(sys.argv[1]) / "Source" / "Runtime"
    total = 0
    for cs_path in sorted(assets_dir.rglob("*.cs")):
        raw = cs_path.read_bytes()
        if b"XmlAttribute" not in raw:
            continue
        has_crlf = b"\r\n" in raw
        bom = b""
        if raw.startswith(b"\xef\xbb\xbf"):
            bom = b"\xef\xbb\xbf"
        text = raw.decode("utf-8-sig").replace("\r\n", "\n")
        matches = list(NULLABLE_ATTRIBUTE.finditer(text))
        if not matches:
            continue
        new_text = NULLABLE_ATTRIBUTE.sub(make_replacement, text)
        if "using System.Xml;" not in new_text:
            new_text = "using System.Xml;\n" + new_text
        if has_crlf:
            new_text = new_text.replace("\n", "\r\n")
        cs_path.write_bytes(bom + new_text.encode("utf-8"))
        total += len(matches)
        print(f"[fix] {cs_path.relative_to(assets_dir)}: {len(matches)} field(s)")
    print(f"[fix] total fields: {total}")


if __name__ == "__main__":
    main()
