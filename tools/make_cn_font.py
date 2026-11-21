"""Rebuilds the Simplified Chinese bitmap font from the current Chinese text.

Writes Assets/ui/fonts/ChineseSimplified.txt (BMFont text format, read by EZ GUI's SpriteFont) and
ChineseSimplified_0.png, covering every character used in Assets/localization/zh-Hans.txt.
Run it after changing Chinese texts, then build as usual (the font goes into the Fonts asset bundle).

Rendering reproduces the original font, which Glu made with AngelCode BMFont:
  - Windows GDI GetGlyphOutlineW (GGO_BITMAP, the same call BMFont uses), so Windows only;
  - Droid Sans Fallback (tools/fonts/DroidSansFallback.ttf, Android 4.4, Apache 2.0), loaded privately (not installed);
  - BMFont settings from the original header: size 66 (cell height), no smoothing, outline 2, spacing 7.
Checked against the original atlas: every glyph rendered here is pixel-identical, at the same offsets.
Kept as data, not rendered:
  - glyphs copied as images from tools/fonts/ChineseSimplified_preserved/ (PRESERVED_GLYPHS): '@' silver and
    '^' gold currency icons (the game writes prices as "@123"), and '£' 'б', which in the original came from
    another font;
  - kerning pairs from tools/fonts/ChineseSimplified_kerning.txt (see KERNING_FILE).

Atlas: 2048 x 2048 while everything fits, otherwise 4096 x 4096 (EZ GUI reads one page only); the texture's
.meta maxTextureSize is set to match, or Unity would shrink a 4096 atlas.

Usage (from ckz2_repo_pc):
    ../.venv/Scripts/python.exe tools/make_cn_font.py
"""
import ctypes
import re
from ctypes import wintypes
from pathlib import Path

from PIL import Image

# ---- Paths ----
PROJECT_DIR = Path(__file__).resolve().parent.parent
TEXT_FILE = PROJECT_DIR / "Assets" / "localization" / "zh-Hans.txt"
FONT_FILE = PROJECT_DIR / "tools" / "fonts" / "DroidSansFallback.ttf"
PRESERVED_DIR = PROJECT_DIR / "tools" / "fonts" / "ChineseSimplified_preserved"
# Kerning pairs of the original font (Latin, Cyrillic, quotes). Kept as data: this font's 'kern' table is split into
# 30 subtables with an empty first one, and GDI's GetKerningPairs only reads the first, so it returns nothing.
KERNING_FILE = PROJECT_DIR / "tools" / "fonts" / "ChineseSimplified_kerning.txt"
OUTPUT_TXT = PROJECT_DIR / "Assets" / "ui" / "fonts" / "ChineseSimplified.txt"
OUTPUT_PNG = PROJECT_DIR / "Assets" / "ui" / "fonts" / "ChineseSimplified_0.png"
OUTPUT_PNG_META = PROJECT_DIR / "Assets" / "ui" / "fonts" / "ChineseSimplified_0.png.meta"

# ---- BMFont settings (from the original header) ----
FACE_NAME = "Droid Sans Fallback"
# Positive GDI height = cell height (ascent + descent): 66 px gives lineHeight 66, base 53, em 50
CELL_HEIGHT = 66
OUTLINE = 2
SPACING = 7
ATLAS_SIZES = [2048, 4096]

# ---- Outline alpha by squared distance to the nearest glyph pixel (measured on the original atlas) ----
OUTLINE_FULL_DISTANCE2 = 4
OUTLINE_EDGE_DISTANCE2 = 5
OUTLINE_EDGE_ALPHA = 119

# ---- Character set ----
# Always included besides the Chinese text: printable ASCII, and symbols the game builds at run time
# (store prices in other currencies, rubles); all of them were in the original font.
ASCII_FIRST = 32
ASCII_LAST = 126
EXTRA_CHARS = "€￡￥ру"
# code point -> (xoffset, yoffset, xadvance); image in PRESERVED_DIR / "U+XXXX.png"
PRESERVED_GLYPHS = {
    0x40: (-10, -10, 117),
    0x5E: (-10, -10, 93),
    0xA3: (11, 15, 50),
    0x431: (1, 15, 28),
}

# ---- Windows GDI ----
FR_PRIVATE = 0x10
GGO_BITMAP = 1
GDI_ERROR = 0xFFFFFFFF
NONANTIALIASED_QUALITY = 3
FW_NORMAL = 400
DEFAULT_CHARSET = 1
GGI_MARK_NONEXISTING_GLYPHS = 1
MISSING_GLYPH_INDEX = 0xFFFF


class GLYPHMETRICS(ctypes.Structure):
    _fields_ = [("gmBlackBoxX", wintypes.UINT), ("gmBlackBoxY", wintypes.UINT),
                ("gmptGlyphOrigin", wintypes.POINT), ("gmCellIncX", ctypes.c_short), ("gmCellIncY", ctypes.c_short)]


class FIXED(ctypes.Structure):
    _fields_ = [("fract", wintypes.WORD), ("value", ctypes.c_short)]


class MAT2(ctypes.Structure):
    _fields_ = [("eM11", FIXED), ("eM12", FIXED), ("eM21", FIXED), ("eM22", FIXED)]


class TEXTMETRICW(ctypes.Structure):
    _fields_ = [("tmHeight", wintypes.LONG), ("tmAscent", wintypes.LONG), ("tmDescent", wintypes.LONG),
                ("tmInternalLeading", wintypes.LONG), ("tmExternalLeading", wintypes.LONG),
                ("tmAveCharWidth", wintypes.LONG), ("tmMaxCharWidth", wintypes.LONG), ("tmWeight", wintypes.LONG),
                ("tmOverhang", wintypes.LONG), ("tmDigitizedAspectX", wintypes.LONG), ("tmDigitizedAspectY", wintypes.LONG),
                ("tmFirstChar", wintypes.WCHAR), ("tmLastChar", wintypes.WCHAR), ("tmDefaultChar", wintypes.WCHAR),
                ("tmBreakChar", wintypes.WCHAR), ("tmItalic", ctypes.c_byte), ("tmUnderlined", ctypes.c_byte),
                ("tmStruckOut", ctypes.c_byte), ("tmPitchAndFamily", ctypes.c_byte), ("tmCharSet", ctypes.c_byte)]


class Glyph:
    """One font character: its RGBA cell (glyph + outline) and BMFont metrics. Position is set by packing."""

    def __init__(self, code, image, xoffset, yoffset, xadvance):
        self.code = code
        self.image = image
        self.xoffset = xoffset
        self.yoffset = yoffset
        self.xadvance = xadvance
        self.x = 0
        self.y = 0


class GdiFont:
    """Droid Sans Fallback selected into a memory DC, loaded privately for this process."""

    def __init__(self):
        self.gdi32 = ctypes.WinDLL("gdi32", use_last_error=True)
        gdi32 = self.gdi32
        gdi32.AddFontResourceExW.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, ctypes.c_void_p]
        gdi32.RemoveFontResourceExW.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, ctypes.c_void_p]
        gdi32.CreateFontW.restype = wintypes.HANDLE
        gdi32.CreateCompatibleDC.restype = wintypes.HANDLE
        gdi32.SelectObject.restype = wintypes.HANDLE
        gdi32.SelectObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        gdi32.GetTextMetricsW.argtypes = [wintypes.HANDLE, ctypes.POINTER(TEXTMETRICW)]
        gdi32.GetGlyphOutlineW.restype = wintypes.DWORD
        gdi32.GetGlyphOutlineW.argtypes = [wintypes.HANDLE, wintypes.UINT, wintypes.UINT, ctypes.POINTER(GLYPHMETRICS),
                                           wintypes.DWORD, ctypes.c_void_p, ctypes.POINTER(MAT2)]
        gdi32.GetGlyphIndicesW.argtypes = [wintypes.HANDLE, wintypes.LPCWSTR, ctypes.c_int,
                                           ctypes.POINTER(wintypes.WORD), wintypes.DWORD]

        added = gdi32.AddFontResourceExW(str(FONT_FILE), FR_PRIVATE, None)
        if added == 0:
            raise RuntimeError("cannot load font " + str(FONT_FILE))
        font = gdi32.CreateFontW(CELL_HEIGHT, 0, 0, 0, FW_NORMAL, 0, 0, 0, DEFAULT_CHARSET, 0, 0,
                                 NONANTIALIASED_QUALITY, 0, FACE_NAME)
        self.dc = gdi32.CreateCompatibleDC(None)
        gdi32.SelectObject(self.dc, font)
        metrics = TEXTMETRICW()
        gdi32.GetTextMetricsW(self.dc, ctypes.byref(metrics))
        self.ascent = metrics.tmAscent
        self.line_height = metrics.tmHeight
        self.identity = MAT2(FIXED(0, 1), FIXED(0, 0), FIXED(0, 0), FIXED(0, 1))
        print("[font] " + FACE_NAME + ": line height " + str(self.line_height) + ", base " + str(self.ascent))

    def has_glyph(self, code):
        index = wintypes.WORD()
        self.gdi32.GetGlyphIndicesW(self.dc, chr(code), 1, ctypes.byref(index), GGI_MARK_NONEXISTING_GLYPHS)
        return index.value != MISSING_GLYPH_INDEX

    def render(self, code):
        """Glyph bitmap ('L', 255 = ink) trimmed to its ink, ink offset from the pen (x, y down from line top), advance."""
        metrics = GLYPHMETRICS()
        size = self.gdi32.GetGlyphOutlineW(self.dc, code, GGO_BITMAP, ctypes.byref(metrics), 0, None,
                                           ctypes.byref(self.identity))
        if size == GDI_ERROR:
            raise RuntimeError("GetGlyphOutlineW failed for U+%04X" % code)
        width = metrics.gmBlackBoxX
        height = metrics.gmBlackBoxY
        bitmap = Image.new("L", (width, height), 0)
        if size > 0:
            buffer = ctypes.create_string_buffer(size)
            self.gdi32.GetGlyphOutlineW(self.dc, code, GGO_BITMAP, ctypes.byref(metrics), size, buffer,
                                        ctypes.byref(self.identity))
            raw = buffer.raw
            # 1 bit per pixel, most significant bit first, rows padded to 4 bytes
            stride = ((width + 31) // 32) * 4
            pixels = bitmap.load()
            for y in range(height):
                for x in range(width):
                    if raw[y * stride + x // 8] & (0x80 >> (x % 8)):
                        pixels[x, y] = 255
        left = metrics.gmptGlyphOrigin.x
        top = self.ascent - metrics.gmptGlyphOrigin.y
        # BMFont trims empty rows and columns of the black box
        box = bitmap.getbbox()
        if box is None:
            return None, 0, 0, metrics.gmCellIncX
        trimmed = bitmap.crop(box)
        return trimmed, left + box[0], top + box[1], metrics.gmCellIncX

    def close(self):
        self.gdi32.RemoveFontResourceExW(str(FONT_FILE), FR_PRIVATE, None)


def collect_characters():
    """Code points to put in the font: ASCII, every msgstr character of the Chinese text, EXTRA_CHARS, PRESERVED_GLYPHS."""
    text = TEXT_FILE.read_text(encoding="utf-8")
    codes = set(range(ASCII_FIRST, ASCII_LAST + 1))
    for block in text.split("\n\n"):
        found = re.search(r'msgstr ((?:".*"\s*)+)', block)
        if found is None:
            continue
        for piece in re.findall(r'"(.*)"', found.group(1)):
            # PO escapes (\n, \", \\) are not characters on screen
            piece = piece.replace("\\n", "").replace('\\"', '"').replace("\\\\", "\\").replace("\\t", "")
            for character in piece:
                if ord(character) >= ASCII_FIRST:
                    codes.add(ord(character))
    for character in EXTRA_CHARS:
        codes.add(ord(character))
    for code in PRESERVED_GLYPHS:
        codes.add(code)
    return sorted(codes)


def add_outline(bitmap):
    """RGBA cell: white glyph in RGB, glyph plus OUTLINE px border in alpha (BMFont outline, alphaChnl=1 redChnl=0)."""
    width = bitmap.width + OUTLINE * 2
    height = bitmap.height + OUTLINE * 2
    cell = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    ink = bitmap.load()
    out = cell.load()
    ink_points = []
    for y in range(bitmap.height):
        for x in range(bitmap.width):
            if ink[x, y] > 0:
                ink_points.append((x + OUTLINE, y + OUTLINE))
    # Alpha from the nearest ink pixel within the outline radius
    alpha = []
    for _ in range(height):
        alpha.append([0] * width)
    for ix, iy in ink_points:
        for dy in range(-OUTLINE, OUTLINE + 1):
            for dx in range(-OUTLINE, OUTLINE + 1):
                distance2 = dx * dx + dy * dy
                value = 0
                if distance2 <= OUTLINE_FULL_DISTANCE2:
                    value = 255
                elif distance2 == OUTLINE_EDGE_DISTANCE2:
                    value = OUTLINE_EDGE_ALPHA
                if value > alpha[iy + dy][ix + dx]:
                    alpha[iy + dy][ix + dx] = value
    for y in range(height):
        for x in range(width):
            out[x, y] = (0, 0, 0, alpha[y][x])
    for ix, iy in ink_points:
        out[ix, iy] = (255, 255, 255, 255)
    return cell


def build_glyphs(font, codes):
    glyphs = []
    missing = []
    for code in codes:
        if code in PRESERVED_GLYPHS:
            image = Image.open(PRESERVED_DIR / ("U+%04X.png" % code)).convert("RGBA")
            xoffset, yoffset, xadvance = PRESERVED_GLYPHS[code]
            glyphs.append(Glyph(code, image, xoffset, yoffset, xadvance))
            continue
        if not font.has_glyph(code):
            missing.append(code)
            continue
        bitmap, left, top, advance = font.render(code)
        if bitmap is None:
            # Blank character (space): a 1 x 1 transparent cell of its own, so its UVs point at an empty spot
            glyphs.append(Glyph(code, Image.new("RGBA", (1, 1), (0, 0, 0, 0)), 0, 0, advance))
            continue
        glyphs.append(Glyph(code, add_outline(bitmap), left - OUTLINE, top - OUTLINE, advance))
    return glyphs, missing


def cell_size_key(glyph):
    """Sort key for packing: tallest, then widest first."""
    return (glyph.image.height, glyph.image.width)


def code_key(glyph):
    """Sort key for the .txt: by code point."""
    return glyph.code


def pack(glyphs, size):
    """Shelf packing, tallest first, SPACING px between cells. Returns False when the atlas is too small."""
    cells = []
    for glyph in glyphs:
        if glyph.image is not None:
            cells.append(glyph)
    cells.sort(key=cell_size_key, reverse=True)
    x = 0
    y = 0
    shelf_height = 0
    for glyph in cells:
        width = glyph.image.width
        height = glyph.image.height
        if x + width > size:
            x = 0
            y += shelf_height + SPACING
            shelf_height = 0
        if y + height > size:
            return False
        glyph.x = x
        glyph.y = y
        x += width + SPACING
        if height > shelf_height:
            shelf_height = height
    return True


def write_atlas(glyphs, size):
    atlas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    for glyph in glyphs:
        if glyph.image is not None:
            atlas.paste(glyph.image, (glyph.x, glyph.y))
    atlas.save(OUTPUT_PNG)


def write_text(glyphs, size, font, kerning):
    lines = [
        'info face="%s" size=%d bold=0 italic=0 charset="" unicode=1 stretchH=100 smooth=0 aa=1 padding=0,0,0,0 '
        'spacing=%d,%d outline=%d' % (FACE_NAME, CELL_HEIGHT, SPACING, SPACING, OUTLINE),
        "common lineHeight=%d base=%d scaleW=%d scaleH=%d pages=1 packed=0 alphaChnl=1 redChnl=0 greenChnl=0 "
        "blueChnl=0" % (font.line_height, font.ascent, size, size),
        'page id=0 file="%s"' % OUTPUT_PNG.name,
        "chars count=%d" % len(glyphs),
    ]
    for glyph in sorted(glyphs, key=code_key):
        width = 0
        height = 0
        if glyph.image is not None:
            width = glyph.image.width
            height = glyph.image.height
        lines.append("char id=%-4d x=%-5d y=%-5d width=%-5d height=%-5d xoffset=%-5d yoffset=%-5d xadvance=%-5d "
                     "page=0  chnl=15" % (glyph.code, glyph.x, glyph.y, width, height, glyph.xoffset, glyph.yoffset,
                                         glyph.xadvance))
    lines.append("kernings count=%d" % len(kerning))
    for first, second, amount in kerning:
        lines.append("kerning first=%-4d second=%-4d amount=%d" % (first, second, amount))
    OUTPUT_TXT.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def set_meta_max_size(size):
    """Unity shrinks textures above maxTextureSize; keep it equal to the atlas size."""
    meta = OUTPUT_PNG_META.read_text(encoding="utf-8")
    updated = re.sub(r"maxTextureSize: \d+", "maxTextureSize: %d" % size, meta)
    if updated != meta:
        OUTPUT_PNG_META.write_text(updated, encoding="utf-8", newline="")
        print("[meta] maxTextureSize set to " + str(size))


def read_kerning(included):
    """Pairs from KERNING_FILE whose two characters are both in the font, in file order."""
    pattern = r"^kerning first=(\d+)\s+second=(\d+)\s+amount=(-?\d+)"
    kerning = []
    for match in re.finditer(pattern, KERNING_FILE.read_text(encoding="utf-8"), re.M):
        first = int(match.group(1))
        second = int(match.group(2))
        if first in included and second in included:
            kerning.append((first, second, int(match.group(3))))
    return kerning


def read_previous_codes():
    if not OUTPUT_TXT.exists():
        return set()
    previous = set()
    for code in re.findall(r"^char id=(\d+)", OUTPUT_TXT.read_text(encoding="utf-8"), re.M):
        previous.add(int(code))
    return previous


def main():
    codes = collect_characters()
    previous = read_previous_codes()
    print("[chars] " + str(len(codes)) + " characters (" + str(len(previous)) + " in the previous font)")
    font = GdiFont()
    try:
        glyphs, missing = build_glyphs(font, codes)
    finally:
        font.close()
    included = set()
    for glyph in glyphs:
        included.add(glyph.code)
    kerning = read_kerning(included)
    if missing:
        names = []
        for code in missing:
            names.append("U+%04X %s" % (code, chr(code)))
        print("[warning] not in " + FONT_FILE.name + ", left out (would not show in game): " + ", ".join(names))

    size = None
    for candidate in ATLAS_SIZES:
        if pack(glyphs, candidate):
            size = candidate
            break
    if size is None:
        raise RuntimeError("glyphs do not fit in %d x %d" % (ATLAS_SIZES[-1], ATLAS_SIZES[-1]))

    write_atlas(glyphs, size)
    write_text(glyphs, size, font, kerning)
    set_meta_max_size(size)

    added = sorted(included - previous)
    removed = sorted(previous - included)
    print("[atlas] %d x %d, %d glyphs, %d kerning pairs" % (size, size, len(glyphs), len(kerning)))
    print("[diff] added %d, removed %d compared with the previous font" % (len(added), len(removed)))
    print("[done] " + str(OUTPUT_TXT) + " and " + OUTPUT_PNG.name)


main()
