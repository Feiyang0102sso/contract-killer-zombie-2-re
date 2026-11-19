"""把 ULegacy 导出的着色器（Unity 3.5 iOS 版，Program 里是 hlsl2glsl 生成的 GLSL ES）翻译成 Unity 6 能编译的 CG/HLSL。

导出的 ShaderLab 里，每个带程序的 Pass 是这样的：
    Program "vp" { SubProgram "gles " { Keywords { "LIGHTMAP_ON" } "!!GLES ... #ifdef VERTEX ... #endif #ifdef FRAGMENT ... #endif" } ... }
    Program "fp" { SubProgram "gles " { Keywords {...} "!!GLES" } ... }      ← 全是空的，代码都在 vp 里
Unity 6 不认这种格式（"expecting TOK_SETTEXTURE"），材质全部显示粉色。

翻译规则（逐字保留原算法，只换语法）：
    - 每个 SubProgram（一组关键字）各自翻成一个完整的程序（结构体、uniform、vert、frag），放进 #if 分支；
      关键字条件按 Unity 6 的关键字写，例如 LIGHTMAP_OFF → !defined(LIGHTMAP_ON)。
    - GLSL 矩阵是列主序：M[i] 是第 i 列，翻成列 swizzle（M._m0i_m1i_m2i_m3i）；(M * v) 翻成 mul(M, v)。
      两者配套，矩阵的逻辑含义与原代码一致。
    - 内置量换成 Unity 6 的名字（glstate_matrix_mvp → UNITY_MATRIX_MVP，_Object2World → unity_ObjectToWorld 等）；
      光照贴图采样换成 UNITY_SAMPLE_TEX2D，解码算法（2.0 * lm.rgb，iOS 的 dLDR）原样保留。
    - unity_Scale（Unity 4 以前的缩放补偿，Unity 5 起删除）与 _World2Object（当时不含缩放）配套还原：
      s = unity_ObjectToWorld 第 0 列的长度，pc_unity_Scale.w = 1/s，pc_World2Object = s × unity_WorldToObject。均匀缩放时与原版一致。
    - 生成的程序带 UNITY_SHADER_NO_UPGRADE，防止 Unity 导入时的着色器升级器回写文件。

用法：python gles_to_hlsl.py <原始导出根目录> <工程根目录> [着色器相对路径 ...]
    不给路径时处理导出里全部带 "!!GLES" 的着色器。源文件总是取原始导出，所以可以重复运行。
"""

import re
import sys
from pathlib import Path

# ---- 常量 ----

PRECISION = re.compile(r"\b(lowp|mediump|highp)\s+")

# 内置 uniform：Unity 6 的 CGPROGRAM 自动包含 UnityShaderVariables.cginc，这些不用（也不能）再声明
BUILTIN_UNIFORMS = {
    "_Time", "_SinTime", "_CosTime", "unity_DeltaTime", "_WorldSpaceCameraPos", "_ProjectionParams",
    "_ScreenParams", "_ZBufferParams", "_WorldSpaceLightPos0", "_LightPositionRange",
    "unity_4LightPosX0", "unity_4LightPosY0", "unity_4LightPosZ0", "unity_4LightAtten0",
    "unity_LightColor", "unity_LightPosition", "unity_LightAtten", "unity_SpotDirection",
    "unity_SHAr", "unity_SHAg", "unity_SHAb", "unity_SHBr", "unity_SHBg", "unity_SHBb", "unity_SHC",
    "unity_LightmapST", "unity_Lightmap", "glstate_lightmodel_ambient",
}

# 旧名字 → Unity 6 的名字（矩阵、灯光）
RENAMES = {
    "glstate_matrix_mvp": "UNITY_MATRIX_MVP",
    "gl_ModelViewProjectionMatrix": "UNITY_MATRIX_MVP",
    "glstate_matrix_modelview0": "UNITY_MATRIX_MV",
    "gl_ModelViewMatrix": "UNITY_MATRIX_MV",
    "glstate_matrix_invtrans_modelview0": "UNITY_MATRIX_IT_MV",
    "glstate_matrix_projection": "UNITY_MATRIX_P",
    "glstate_matrix_texture0": "pc_TextureMatrix0",
    "gl_TextureMatrix0": "pc_TextureMatrix0",
    "unity_Scale": "pc_unity_Scale",
    "_Object2World": "unity_ObjectToWorld",
    "_World2Object": "pc_World2Object",
    "_LightMatrix0": "unity_WorldToLight",
    "unity_LightColor0": "unity_LightColor[0]",
    "unity_LightColor1": "unity_LightColor[1]",
    "unity_LightColor2": "unity_LightColor[2]",
    "unity_LightColor3": "unity_LightColor[3]",
    "unity_LightPosition0": "unity_LightPosition[0]",
    "unity_LightPosition1": "unity_LightPosition[1]",
    "unity_LightPosition2": "unity_LightPosition[2]",
    "unity_LightPosition3": "unity_LightPosition[3]",
    "unity_LightAtten0": "unity_LightAtten[0]",
    "unity_LightAtten1": "unity_LightAtten[1]",
    "unity_LightAtten2": "unity_LightAtten[2]",
    "unity_LightAtten3": "unity_LightAtten[3]",
    "glstate_light0_spotDirection": "unity_SpotDirection[0]",
    "glstate_light1_spotDirection": "unity_SpotDirection[1]",
    "glstate_light2_spotDirection": "unity_SpotDirection[2]",
    "glstate_light3_spotDirection": "unity_SpotDirection[3]",
}

# 按需声明的辅助量（代码里出现才加）
HELPERS = {
    # Unity 4 以前：_World2Object 是不含缩放的逆矩阵（= s × 真逆矩阵），unity_Scale.w = 1/s，两者相乘才是物体空间；
    # 法线 M3(_Object2World) * n * unity_Scale.w 才是单位长度。Unity 6 的 unity_WorldToObject 是真逆矩阵，
    # 这里用 s = 第 0 列的长度还原这对约定（均匀缩放时与原版一致）。
    "pc_ObjectScale": "#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)",
    "pc_unity_Scale": "#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())",
    "pc_World2Object": "#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])",
    "pc_TextureMatrix0": "static const float4x4 pc_TextureMatrix0 = float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1);",
    "unity_WorldToLight": "float4x4 unity_WorldToLight;",
    "pc_mod": "#define pc_mod(x, y) ((x) - (y) * floor((x) / (y)))",
}

# 函数改名
FUNCTION_RENAMES = {
    "texture2D": "tex2D",
    "texture2DProj": "tex2Dproj",
    "textureCube": "texCUBE",
    "mix": "lerp",
    "fract": "frac",
    "inversesqrt": "rsqrt",
    "mod": "pc_mod",
    "dFdx": "ddx",
    "dFdy": "ddy",
}

# 顶点属性 → HLSL 语义
ATTRIBUTE_SEMANTICS = {
    "_glesVertex": "POSITION",
    "_glesNormal": "NORMAL",
    "_glesColor": "COLOR",
    "_glesTANGENT": "TANGENT",
    "_glesMultiTexCoord0": "TEXCOORD0",
    "_glesMultiTexCoord1": "TEXCOORD1",
    "_glesMultiTexCoord2": "TEXCOORD2",
    "_glesMultiTexCoord3": "TEXCOORD3",
}

# 旧关键字 → 预处理条件；None 表示这个关键字不产生条件
KEYWORD_CONDITIONS = {
    "LIGHTMAP_ON": "defined(LIGHTMAP_ON)",
    "LIGHTMAP_OFF": "!defined(LIGHTMAP_ON)",
    "SHADOWS_SCREEN": "defined(SHADOWS_SCREEN)",
    "SHADOWS_OFF": "!defined(SHADOWS_SCREEN)",
    "DIRLIGHTMAP_OFF": None,
    "VERTEXLIGHT_ON": "defined(VERTEXLIGHT_ON)",
}
# Unity 3.5 的方向光照贴图（双重光照贴图）没用到（m_LightmapsMode 都是 0），这些变体直接丢掉
DROPPED_KEYWORDS = {"DIRLIGHTMAP_ON"}
FORWARD_BASE_KEYWORDS = {"DIRECTIONAL", "SHADOWS_SCREEN", "SHADOWS_OFF", "VERTEXLIGHT_ON"}
FORWARD_ADD_KEYWORDS = {"POINT", "SPOT", "POINT_COOKIE", "DIRECTIONAL_COOKIE"}
LIGHTMAP_KEYWORDS = {"LIGHTMAP_ON", "LIGHTMAP_OFF"}

BINARY_OPERATORS = (" * ", " / ", " + ", " - ", " <= ", " >= ", " < ", " > ", " == ", " != ", " && ", " || ", " ? ", " : ")
IDENTIFIER_CHARS = re.compile(r"[A-Za-z0-9_]")
DECLARATION = re.compile(r"^\s*(uniform|attribute|varying)\s+(\w+)\s+(\w+)(\[\d+\])?\s*;\s*$")
LOCAL_MATRIX = re.compile(r"\bmat([234])\s+(\w+)\s*;")
# 能声明成 uniform 的 GLSL 类型；其他（旧固定管线灯光的结构体）丢掉
UNIFORM_TYPES = re.compile(r"(float|int|bool|[ib]?vec[234]|mat[234]|sampler2D|samplerCube)")


# ---- ShaderLab 层 ----

def find_matching(text, open_index, open_char="{", close_char="}"):
    """返回与 open_index 处的括号匹配的位置，跳过双引号字符串。"""
    depth = 0
    in_string = False
    index = open_index
    while index < len(text):
        char = text[index]
        if char == '"':
            in_string = not in_string
        elif not in_string:
            if char == open_char:
                depth += 1
            elif char == close_char:
                depth -= 1
                if depth == 0:
                    return index
        index += 1
    raise ValueError("unbalanced block")


def parse_subprograms(block):
    """从 Program "vp" { ... } 里取出 [(关键字列表, GLSL 代码)]。"""
    result = []
    for match in re.finditer(r'SubProgram "gles "\s*\{', block):
        body_start = match.end() - 1
        body_end = find_matching(block, body_start)
        body = block[body_start + 1:body_end]
        keywords = []
        keyword_match = re.search(r"Keywords\s*\{([^}]*)\}", body)
        if keyword_match:
            keywords = re.findall(r'"(\w+)"', keyword_match.group(1))
        code_match = re.search(r'"!!GLES(.*)"', body, re.S)
        code = code_match.group(1) if code_match else ""
        result.append((keywords, code))
    return result


def pass_light_mode(text, program_index):
    """Program 所在 Pass 的 LightMode（往前找最近的 Tags）。"""
    tags = re.findall(r'"LIGHTMODE"="(\w+)"', text[:program_index])
    if tags:
        return tags[-1]
    return ""


# ---- GLSL 层：声明 ----

def split_sections(code):
    """把一个 SubProgram 的代码拆成顶点、片元两段。"""
    vertex_start = code.index("#ifdef VERTEX")
    fragment_start = code.index("#ifdef FRAGMENT")
    vertex = code[vertex_start + len("#ifdef VERTEX"):fragment_start]
    fragment = code[fragment_start + len("#ifdef FRAGMENT"):]
    vertex = vertex[:vertex.rindex("#endif")]
    fragment = fragment[:fragment.rindex("#endif")]
    return vertex, fragment


def parse_section(section):
    """解析一段 GLSL：返回 (uniform 列表, attribute 列表, varying 列表, 函数文本)。

    列表元素是 (类型, 名字, 数组后缀)。#define、struct 定义、结构体类型的 uniform 都丢掉
    （它们是旧版固定管线灯光的别名，用到的名字由 RENAMES 直接换掉）。
    """
    uniforms = []
    attributes = []
    varyings = []
    code_lines = []
    lines = PRECISION.sub("", section).splitlines()
    index = 0
    while index < len(lines):
        line = lines[index]
        stripped = line.strip()
        if stripped.startswith("#") or stripped.startswith("precision "):
            index += 1
            continue
        if stripped.startswith("struct "):
            # 跳过整个 struct 定义
            while not lines[index].strip().startswith("};"):
                index += 1
            index += 1
            continue
        match = DECLARATION.match(line)
        if match:
            kind, type_name, name, array = match.groups()
            entry = (type_name, name, array or "")
            if kind == "uniform":
                if UNIFORM_TYPES.fullmatch(type_name):
                    uniforms.append(entry)
            elif kind == "attribute":
                attributes.append(entry)
            else:
                varyings.append(entry)
            index += 1
            continue
        code_lines.append(line)
        index += 1
    return uniforms, attributes, varyings, "\n".join(code_lines)


# ---- GLSL 层：表达式 ----

def split_top_level(text, separators):
    """按顶层（括号外）的分隔符切开，返回 (片段列表, 分隔符列表)。"""
    parts = []
    found = []
    depth = 0
    start = 0
    index = 0
    while index < len(text):
        char = text[index]
        if char in "([":
            depth += 1
        elif char in ")]":
            depth -= 1
        elif depth == 0:
            for separator in separators:
                if text.startswith(separator, index):
                    parts.append(text[start:index])
                    found.append(separator)
                    index += len(separator)
                    start = index
                    break
            else:
                index += 1
                continue
            continue
        index += 1
    parts.append(text[start:])
    return parts, found


def convert_operand(text, matrices):
    """转换一个操作数（里面的括号递归处理）。返回 (HLSL 文本, 是否矩阵)。"""
    stripped = text.strip()
    if stripped in matrices:
        return text, True
    output = []
    index = 0
    whole_group_is_matrix = None
    while index < len(text):
        char = text[index]
        if char != "(":
            output.append(char)
            index += 1
            continue
        close = find_matching(text, index, "(", ")")
        inner = text[index + 1:close]
        # 往前找函数名（hlsl2glsl 在函数名和括号之间常有一个空格）
        prefix = "".join(output).rstrip()
        name_match = re.search(r"(\w+)$", prefix)
        if name_match and not prefix.endswith(("return",)):
            name = name_match.group(1)
            arguments, _ = split_top_level(inner, (",",))
            converted = []
            for argument in arguments:
                converted.append(convert_expression(argument, matrices)[0].strip())
            output = list(prefix[:len(prefix) - len(name)])
            output.append(convert_call(name, converted))
        else:
            converted_inner, is_matrix = convert_expression(inner, matrices)
            if stripped == text[index:close + 1].strip():
                whole_group_is_matrix = is_matrix
            output.append("(" + converted_inner + ")")
        index = close + 1
    return "".join(output), bool(whole_group_is_matrix)


def convert_call(name, arguments):
    """函数调用：改名、构造函数单参数转强制转换、光照贴图采样。"""
    vector_match = re.fullmatch(r"vec([234])", name)
    if vector_match and len(arguments) == 1:
        # HLSL 不接受 float4(x) 这种单参数构造（标量扩展或截断），改成强制转换
        return "((float" + vector_match.group(1) + ")(" + arguments[0] + "))"
    if name == "texture2D" and arguments[0] == "unity_Lightmap":
        return "UNITY_SAMPLE_TEX2D(unity_Lightmap, " + arguments[1] + ")"
    if name == "atan" and len(arguments) == 2:
        return "atan2(" + ", ".join(arguments) + ")"
    new_name = FUNCTION_RENAMES.get(name, name)
    return new_name + "(" + ", ".join(arguments) + ")"


def convert_expression(text, matrices):
    """转换一个表达式：只有一个顶层 * 且有一边是矩阵时，改成 mul()。返回 (文本, 是否矩阵)。"""
    parts, operators = split_top_level(text, BINARY_OPERATORS)
    converted = []
    for part in parts:
        converted.append(convert_operand(part, matrices))
    if operators == [" * "] and (converted[0][1] or converted[1][1]):
        both = converted[0][1] and converted[1][1]
        return "mul(" + converted[0][0].strip() + ", " + converted[1][0].strip() + ")", both
    if " * " in operators:
        for operand_text, is_matrix in converted:
            if is_matrix:
                raise ValueError("matrix in chained expression: " + text)
    pieces = [converted[0][0]]
    for operator, (operand_text, _) in zip(operators, converted[1:]):
        pieces.append(operator)
        pieces.append(operand_text)
    if len(converted) == 1:
        return pieces[0], converted[0][1]
    return "".join(pieces), False


def column_swizzle(match, matrix_sizes):
    """GLSL 的 M[i]（第 i 列）→ HLSL 的列 swizzle。"""
    name = match.group(1)
    column = match.group(2)
    size = matrix_sizes[name]
    components = []
    for row in range(size):
        components.append("_m" + str(row) + column)
    return name + "." + "".join(components)


ASSIGNMENT = re.compile(r"^(\s*)([\w.\[\]]+(?:\s+\w+)?)\s*(=|\+=|-=|\*=|/=)\s*(.*);\s*$")


def convert_statement(line, matrices):
    """转换一行语句。hlsl2glsl 的输出一行一句。"""
    match = ASSIGNMENT.match(line)
    if match:
        indent, target, operator, expression = match.groups()
        return indent + target + " " + operator + " " + convert_expression(expression, matrices)[0].strip() + ";"
    return_match = re.match(r"^(\s*)return\s+(.*);\s*$", line)
    if return_match:
        return return_match.group(1) + "return " + convert_expression(return_match.group(2), matrices)[0].strip() + ";"
    if_match = re.match(r"^(\s*)(if|while)\s*\((.*)\)\s*(\{?)\s*$", line)
    if if_match:
        indent, keyword, condition, brace = if_match.groups()
        return indent + keyword + " (" + convert_expression(condition, matrices)[0].strip() + ") " + brace
    if "(" in line and not line.strip().startswith("for"):
        # 其他带括号的语句（例如单独的函数调用）
        stripped = line.strip()
        if stripped.endswith(";"):
            indent = line[:len(line) - len(line.lstrip())]
            return indent + convert_expression(stripped[:-1], matrices)[0].strip() + ";"
    return line


def convert_function_code(code, matrix_sizes):
    """转换函数文本：矩阵列下标、mul、函数改名、类型名。"""
    matrices = set(matrix_sizes)
    if matrices:
        pattern = re.compile(r"\b(" + "|".join(sorted(matrices, key=len, reverse=True)) + r")\[(\d)\]")
        code = pattern.sub(lambda m: column_swizzle(m, matrix_sizes), code)
    lines = []
    for line in code.splitlines():
        lines.append(convert_statement(line, matrices))
    code = "\n".join(lines)
    code = re.sub(r"\bvec([234])\b", r"float\1", code)
    code = re.sub(r"\bivec([234])\b", r"int\1", code)
    code = re.sub(r"\bbvec([234])\b", r"bool\1", code)
    code = re.sub(r"\bmat([234])\b", r"float\1x\1", code)
    return code


def rename_identifiers(code):
    """旧内置量换成 Unity 6 的名字。"""
    # 固定管线的环境光：gl_LightModel 是结构体别名，成员访问整体替换
    code = code.replace("gl_LightModel.ambient", "glstate_lightmodel_ambient")
    for old, new in RENAMES.items():
        code = re.sub(r"\b" + old + r"\b", new, code)
    return code


def hlsl_type(glsl_type):
    """GLSL 类型名 → HLSL 类型名。"""
    if glsl_type == "samplerCube":
        return "samplerCUBE"
    vector = re.fullmatch(r"vec([234])", glsl_type)
    if vector:
        return "float" + vector.group(1)
    matrix = re.fullmatch(r"mat([234])", glsl_type)
    if matrix:
        return "float" + matrix.group(1) + "x" + matrix.group(1)
    return glsl_type


# ---- 生成 ----

def build_variant(code, variant_index):
    """把一个 SubProgram 的 GLSL 翻成 HLSL 程序文本（不含 #if）。"""
    vertex_source, fragment_source = split_sections(code)
    vertex_uniforms, attributes, varyings, vertex_code = parse_section(vertex_source)
    fragment_uniforms, _, _, fragment_code = parse_section(fragment_source)

    # 已知的矩阵：uniform 矩阵 + 函数里声明的局部矩阵
    matrix_sizes = {}
    for type_name, name, array in vertex_uniforms + fragment_uniforms:
        size = re.fullmatch(r"mat([234])", type_name)
        if size and not array:
            matrix_sizes[name] = int(size.group(1))
    for alias in ("gl_ModelViewProjectionMatrix", "gl_ModelViewMatrix", "gl_TextureMatrix0"):
        matrix_sizes[alias] = 4
    for match in LOCAL_MATRIX.finditer(vertex_code + "\n" + fragment_code):
        matrix_sizes[match.group(2)] = int(match.group(1))

    vertex_hlsl = rename_identifiers(convert_function_code(vertex_code, matrix_sizes))
    fragment_hlsl = rename_identifiers(convert_function_code(fragment_code, matrix_sizes))

    # 入口函数：main → vert / frag，属性、varying、输出加上结构体前缀
    attribute_names = []
    for _, name, _ in attributes:
        attribute_names.append(name)
    varying_names = []
    for _, name, _ in varyings:
        varying_names.append(name)
    vertex_hlsl = prefix_names(vertex_hlsl, attribute_names, "v.")
    vertex_hlsl = prefix_names(vertex_hlsl, varying_names, "o.")
    vertex_hlsl = re.sub(r"\bgl_Position\b", "o.pos", vertex_hlsl)
    fragment_hlsl = prefix_names(fragment_hlsl, varying_names, "i.")
    fragment_hlsl = re.sub(r"\bgl_FragData\[0\]|\bgl_FragColor\b", "pc_FragColor", fragment_hlsl)
    vertex_hlsl = replace_main(vertex_hlsl, "v2f_pc vert (appdata_pc v)", "\tv2f_pc o = (v2f_pc)0;", "\treturn o;")
    fragment_hlsl = replace_main(fragment_hlsl, "float4 frag (v2f_pc i) : SV_Target", "\tfloat4 pc_FragColor = 0;", "\treturn pc_FragColor;")

    lines = []
    # uniform（顶点、片元合并去重）
    declared = set()
    for type_name, name, array in vertex_uniforms + fragment_uniforms:
        if name in declared or name in BUILTIN_UNIFORMS or name in RENAMES:
            continue
        declared.add(name)
        lines.append(hlsl_type(type_name) + " " + name + array + ";")
    body = vertex_hlsl + "\n" + fragment_hlsl
    needed = set()
    for name in HELPERS:
        if re.search(r"\b" + name + r"\b", body) and name not in declared:
            needed.add(name)
    # 缩放相关的两个宏都要用到 pc_ObjectScale
    if "pc_unity_Scale" in needed or "pc_World2Object" in needed:
        needed.add("pc_ObjectScale")
    for name, declaration in HELPERS.items():
        if name in needed:
            lines.append(declaration)
    lines.append("struct appdata_pc {")
    for type_name, name, _ in attributes:
        semantic = ATTRIBUTE_SEMANTICS[name]
        lines.append("\t" + hlsl_type(type_name) + " " + name + " : " + semantic + ";")
    lines.append("};")
    lines.append("struct v2f_pc {")
    lines.append("\tfloat4 pos : SV_POSITION;")
    for slot, (type_name, name, _) in enumerate(varyings):
        lines.append("\t" + hlsl_type(type_name) + " " + name + " : TEXCOORD" + str(slot) + ";")
    lines.append("};")
    lines.append(vertex_hlsl.strip())
    lines.append(fragment_hlsl.strip())
    return "\n".join(lines)


def prefix_names(code, names, prefix):
    """给属性 / varying 加上结构体前缀（已经带点号的成员访问不动）。"""
    for name in names:
        code = re.sub(r"(?<![.\w])" + name + r"\b", prefix + name, code)
    return code


def replace_main(code, signature, first_line, last_line):
    """把 void main () { ... } 换成入口函数，并在开头、结尾插入输出变量的声明与返回。"""
    match = re.search(r"void main \(\)\s*\{", code)
    start = match.end() - 1
    end = find_matching(code, start)
    body = code[start + 1:end]
    return code[:match.start()] + signature + "\n{\n" + first_line + body + last_line + "\n}" + code[end + 1:]


def variant_condition(keywords):
    """关键字列表 → 预处理条件（全部满足）。返回 (条件文本, 正条件个数)。"""
    conditions = []
    positives = 0
    for keyword in keywords:
        if keyword in KEYWORD_CONDITIONS:
            condition = KEYWORD_CONDITIONS[keyword]
        elif keyword.endswith("_OFF"):
            condition = "!defined(" + keyword[:-4] + "_ON)"
        else:
            condition = "defined(" + keyword + ")"
        if condition is None:
            continue
        conditions.append(condition)
        if not condition.startswith("!"):
            positives += 1
    if not conditions:
        return "1", 0
    return " && ".join(conditions), positives


def multi_compile_lines(variants, light_mode):
    """根据这些变体用到的关键字生成 #pragma multi_compile。"""
    keywords = set()
    for variant_keywords, _ in variants:
        keywords.update(variant_keywords)
    lines = []
    if light_mode == "ForwardAdd" or keywords & FORWARD_ADD_KEYWORDS:
        lines.append("#pragma multi_compile_fwdadd")
        keywords -= FORWARD_ADD_KEYWORDS | {"DIRECTIONAL"}
    elif keywords & FORWARD_BASE_KEYWORDS:
        lines.append("#pragma multi_compile_fwdbase")
        keywords -= FORWARD_BASE_KEYWORDS | LIGHTMAP_KEYWORDS | {"DIRLIGHTMAP_OFF"}
    if keywords & LIGHTMAP_KEYWORDS:
        lines.append("#pragma multi_compile _ LIGHTMAP_ON")
        keywords -= LIGHTMAP_KEYWORDS
    keywords -= {"DIRLIGHTMAP_OFF"}
    # 其余自定义关键字：从不同时出现的归为一组
    groups = []
    for keyword in sorted(keywords):
        placed = False
        for group in groups:
            exclusive = True
            for variant_keywords, _ in variants:
                if keyword in variant_keywords and group & set(variant_keywords):
                    exclusive = False
            if exclusive:
                group.add(keyword)
                placed = True
                break
        if not placed:
            groups.append({keyword})
    for group in groups:
        lines.append("#pragma multi_compile " + " ".join(sorted(group)))
    return lines


def translate_program(subprograms, light_mode, shader_name):
    """把一个 Program "vp" 的全部 SubProgram 翻成一个 CGPROGRAM 块。"""
    variants = []
    for keywords, code in subprograms:
        if set(keywords) & DROPPED_KEYWORDS:
            continue
        if "#ifdef VERTEX" not in code:
            continue
        variants.append((keywords, code))
    # 条件越具体越靠前；最后一个用 #else 兜底
    ordered = []
    for keywords, code in variants:
        condition, positives = variant_condition(keywords)
        ordered.append((positives, len(keywords), condition, keywords, code))
    ordered.sort(key=lambda item: (-item[0], -item[1]))
    # UNITY_SHADER_NO_UPGRADE：不让 Unity 的着色器升级器改写生成的代码（它会把 unity_Scale 等替换成字面量）
    lines = ["CGPROGRAM", "// UNITY_SHADER_NO_UPGRADE", "#pragma vertex vert", "#pragma fragment frag", "#pragma target 3.0"]
    lines.extend(multi_compile_lines(variants, light_mode))
    lines.append("// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译")
    for index, (_, _, condition, keywords, code) in enumerate(ordered):
        if len(ordered) > 1:
            if index == 0:
                lines.append("#if " + condition)
            elif index == len(ordered) - 1:
                lines.append("#else // " + " ".join(keywords))
            else:
                lines.append("#elif " + condition)
        lines.append(build_variant(code, index))
    if len(ordered) > 1:
        lines.append("#endif")
    lines.append("ENDCG")
    return "\n".join(lines)


def translate_shader(text):
    """翻译整个 .shader 文本：替换每个 Program "vp"，删掉对应的空 Program "fp"。"""
    shader_name = re.search(r'Shader "([^"]+)"', text).group(1)
    output = []
    cursor = 0
    while True:
        vp = text.find('Program "vp"', cursor)
        if vp < 0:
            break
        vp_open = text.index("{", vp)
        vp_close = find_matching(text, vp_open)
        subprograms = parse_subprograms(text[vp_open:vp_close + 1])
        light_mode = pass_light_mode(text, vp)
        output.append(text[cursor:vp])
        output.append(translate_program(subprograms, light_mode, shader_name))
        cursor = vp_close + 1
        # 紧跟着的 Program "fp"（GLES 下全是空的）删掉
        rest = text[cursor:]
        fp_match = re.match(r'\s*Program "fp"\s*\{', rest)
        if fp_match:
            fp_open = cursor + fp_match.end() - 1
            fp_close = find_matching(text, fp_open)
            cursor = fp_close + 1
    output.append(text[cursor:])
    return "".join(output)


def main():
    export_root = Path(sys.argv[1])
    pilot_root = Path(sys.argv[2])
    if len(sys.argv) > 3:
        relative_paths = sys.argv[3:]
    else:
        relative_paths = []
        for path in sorted(export_root.joinpath("Assets").rglob("*.shader")):
            if "!!GLES" in path.read_text(encoding="utf-8"):
                relative_paths.append(path.relative_to(export_root).as_posix())
    failed = 0
    for relative in relative_paths:
        source = export_root / relative
        target = pilot_root / relative
        try:
            translated = translate_shader(source.read_text(encoding="utf-8"))
        except (ValueError, KeyError) as error:
            print(f"[fail] {relative}: {error}")
            failed += 1
            continue
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(translated, encoding="utf-8")
        print(f"[ok] {relative}")
    print(f"translated {len(relative_paths) - failed}/{len(relative_paths)}")


if __name__ == "__main__":
    main()
