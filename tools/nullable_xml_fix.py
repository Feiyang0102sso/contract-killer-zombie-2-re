"""修复：新版 XmlSerializer 不允许把可空类型（bool? int? ...）标成 [XmlAttribute]。

Unity 3.5 的 Mono 2.6 允许这种写法，新版会在构造序列化器时直接抛异常，导致整份 XML（资源包索引、
商店配置）都解析失败。改法：可空字段本身加 [XmlIgnore]，另加一个同名特性的字符串代理属性负责读写。
值为 null 时代理返回 null，XmlSerializer 不写这个特性，与原行为一致。

用法：python tools/nullable_xml_fix.py <工程根目录>（改 Source/Runtime 下的代码）
"""

import re
import sys
from pathlib import Path

# 可空字段：[XmlAttribute("名称")] 下一行是 public T? 字段名;
NULLABLE_ATTRIBUTE = re.compile(
    r'(?P<indent>[ \t]*)\[XmlAttribute(?:\((?:AttributeName\s*=\s*)?"(?P<xml>[^"]*)"\))?\][ \t]*\n'
    # 类型可能带点号（嵌套枚举，如 PlayerData.Slots）
    r'[ \t]*(?P<access>public|private|protected|internal) (?P<type>[\w.]+)\? (?P<name>\w+);'
)

# 各类型的解析表达式（{v} 为字符串值）
PARSERS = {
    "int": "XmlConvert.ToInt32({v})",
    "long": "XmlConvert.ToInt64({v})",
    "double": "XmlConvert.ToDouble({v})",
    "decimal": "XmlConvert.ToDecimal({v})",
    "bool": "PcXml.ParseBool({v})",
}

# 代理属性名后缀
PROXY_SUFFIX = "_XmlText"

def make_replacement(match):
    """生成 [XmlIgnore] 字段 + 字符串代理属性。"""
    i = match.group("indent")
    xml_name = match.group("xml") or match.group("name")
    access = match.group("access")
    value_type = match.group("type")
    name = match.group("name")
    if value_type in PARSERS:
        parser = PARSERS[value_type].format(v="value")
        formatter = f"XmlConvert.ToString({name}.Value)"
    else:
        # 其余视为枚举：XmlSerializer 默认按成员名读写
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
    """遍历 Assets 下所有 .cs 并改写。"""
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
