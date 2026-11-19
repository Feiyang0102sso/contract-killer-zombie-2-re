"""把 DllImport("__Internal") 的 extern 声明改成：iOS 上照旧，其他平台转调 PcNative 的同名函数。

只处理 PcPort/PcNative.cs 里已经实现的函数；其余 extern 保持不变。调用处不改。

    #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern int Foo(int size, byte[] buf);
    #else
        private static int Foo(int size, byte[] buf)
        {
            return PcNative.Foo(size, buf);
        }
    #endif

用法：python tools/bridge_externs.py <工程根目录>（改 Source/Runtime 下的代码）
"""

import re
import sys
from pathlib import Path

PC_NATIVE_FILE = "PcPort/PcNative.cs"
# PcNative 里的公开静态函数名
PC_NATIVE_METHOD = re.compile(r"public static \S+ (\w+)\(")
# 一条 DllImport + extern 声明（属性行与声明行之间只有空白）
EXTERN_DECL = re.compile(
    r"(?P<indent>[ \t]*)\[DllImport\(\"__Internal\"[^\]]*\)\]\s*\n"
    r"[ \t]*(?P<mods>(?:public |private |internal |protected )*)static extern "
    # 形参里可能有 MarshalAs(...) 这样的括号，所以匹配到 ");" 为止
    r"(?P<ret>[\w.\[\]<>]+) (?P<name>\w+)\((?P<params>[^;]*)\);"
)
# 参数上的特性，例如 [Out][MarshalAs(UnmanagedType.LPArray)]
# 要求 [ 后面紧跟字母，避免误删数组类型的 []
PARAM_ATTRIBUTE = re.compile(r"\[[A-Za-z][^\]]*\]")


def load_pc_native_methods(assets_dir):
    """读取 PcNative 已实现的函数名。"""
    text = (assets_dir / PC_NATIVE_FILE).read_text(encoding="utf-8-sig")
    names = set()
    for name in PC_NATIVE_METHOD.findall(text):
        names.add(name)
    return names


def build_arguments(params):
    """由形参列表生成实参列表：去掉特性和类型，保留 ref/out。"""
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
    """生成 #if UNITY_IOS 两路代码。"""
    indent = match.group("indent")
    mods = match.group("mods")
    ret = match.group("ret")
    name = match.group("name")
    params = match.group("params")
    # PC 路径上的形参不需要封送特性
    plain_params = PARAM_ATTRIBUTE.sub("", params).strip()
    body_lines = []
    if name in IMPLEMENTED:
        # PcNative 有真正的实现：转调
        arguments = build_arguments(params)
        call = f"PcNative.{name}({arguments})"
        if ret == "void":
            body_lines.append(f"{indent}\t{call};")
        else:
            body_lines.append(f"{indent}\treturn {call};")
    else:
        # 其余插件：就地空实现，out 参数赋默认值，返回类型默认值
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
    """返回所有 out 形参的名字。"""
    names = []
    if not plain_params:
        return names
    for part in plain_params.split(","):
        tokens = part.split()
        if tokens and tokens[0] == "out":
            names.append(tokens[-1])
    return names


# PcNative 已实现的函数名，main() 里填充
IMPLEMENTED = set()


def main():
    """遍历 Assets 下所有 .cs，替换全部 DllImport("__Internal") extern 声明。"""
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
            # 已经桥接过的声明（前一行是 #if UNITY_IOS）不重复处理
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
