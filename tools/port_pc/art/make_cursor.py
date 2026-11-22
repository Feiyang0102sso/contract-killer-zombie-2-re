"""Draws the PC mouse cursor: a translucent cyan dart arrow with a bright rim and a soft glow.

The cyan matches the game's stock UI icons (achievements, settings). The shape is designed on a 64 x 64 grid
and rendered at every size in CURSOR_SIZES (supersampled), so small sizes stay crisp instead of being downscaled.
PcCursor picks the size closest to 32 px times the display scale (100% -> 32, 150% -> 48, 200% -> 64).

Output: Assets/Resources/PortAddon/PcCursor<size>.png, plus a preview sheet in tools/out/.
The hotspot (arrow tip) is HOTSPOT on the 64 grid; PcCursor.HotspotOnGrid must match it.

Usage (from ckz2_repo_pc):
    ../.venv/Scripts/python.exe tools/port_pc/art/make_cursor.py
"""
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

# ---- Paths ----
# tools/port_pc/<category>/<script>.py -> project root
PROJECT_DIR = Path(__file__).resolve().parents[3]
OUTPUT_DIR = PROJECT_DIR / "Assets" / "Resources" / "PortAddon"
PREVIEW_PNG = PROJECT_DIR / "tools" / "out" / "PcCursor_preview.png"

# ---- Sizes ----
CURSOR_SIZES = [32, 48, 64]
GRID = 64
SUPERSAMPLE = 4

# ---- Shape on the 64 grid: tip, right wing, notch, bottom wing ----
HOTSPOT = (6.0, 5.0)
ARROW_POINTS = [HOTSPOT, (54.0, 30.0), (31.0, 35.0), (23.0, 58.0)]
# Corner rounding radius (grid units)
CORNER_ROUNDING = 1.5

# ---- Colors (RGB) and alphas (0..255) ----
FILL_NEAR = (130, 238, 255)
FILL_FAR = (0, 110, 200)
FILL_ALPHA = 150
RIM_COLOR = (170, 248, 255)
RIM_WIDTH = 2.5
OUTLINE_COLOR = (0, 28, 44)
OUTLINE_ALPHA = 200
OUTLINE_WIDTH = 1.2
GLOW_COLOR = (0, 200, 255)
GLOW_ALPHA = 170
GLOW_SPREAD = 2.0
GLOW_BLUR = 3.5

# ---- Preview ----
PREVIEW_BACKGROUNDS = [(30, 30, 30), (200, 196, 180), (60, 90, 120)]
PREVIEW_ZOOM = 4


def to_pixels(value, size):
    """Grid units to supersampled pixels at the given cursor size."""
    return value * size / GRID * SUPERSAMPLE


def grow(mask, radius):
    """Dilates a mask by radius pixels (positive) or erodes it (negative)."""
    steps = round(abs(radius))
    if steps == 0:
        return mask
    if radius > 0:
        return mask.filter(ImageFilter.MaxFilter(steps * 2 + 1))
    return mask.filter(ImageFilter.MinFilter(steps * 2 + 1))


def arrow_mask(size):
    """The arrow shape at supersampled resolution, corners slightly rounded."""
    big = size * SUPERSAMPLE
    mask = Image.new("L", (big, big), 0)
    points = []
    for x, y in ARROW_POINTS:
        points.append((to_pixels(x, size), to_pixels(y, size)))
    ImageDraw.Draw(mask).polygon(points, fill=255)
    rounding = to_pixels(CORNER_ROUNDING, size)
    # Erode then dilate rounds the outer corners
    return grow(grow(mask, -rounding), rounding)


def scaled_alpha(mask, alpha):
    """Mask values scaled from 0..255 to 0..alpha."""
    table = []
    for value in range(256):
        table.append(value * alpha // 255)
    return mask.point(table)


def solid(size_pixels, color, mask, alpha):
    """Single-color layer cut by the mask."""
    layer = Image.new("RGBA", size_pixels, color + (0,))
    layer.putalpha(scaled_alpha(mask, alpha))
    return layer


def fill_gradient(size_pixels, mask):
    """Diagonal gradient from the tip (near) to the far corner, cut by the mask."""
    width, height = size_pixels
    layer = Image.new("RGBA", size_pixels)
    pixels = layer.load()
    for y in range(height):
        for x in range(width):
            t = (x + y) / (width + height)
            color = []
            for channel in range(3):
                color.append(round(FILL_NEAR[channel] + (FILL_FAR[channel] - FILL_NEAR[channel]) * t))
            pixels[x, y] = tuple(color) + (255,)
    layer.putalpha(scaled_alpha(mask, FILL_ALPHA))
    return layer


def draw_cursor(size):
    """Renders one cursor size."""
    mask = arrow_mask(size)
    big_size = mask.size
    outline = grow(mask, to_pixels(OUTLINE_WIDTH, size))
    rim = ImageChops.subtract(mask, grow(mask, -to_pixels(RIM_WIDTH, size)))
    glow = grow(mask, to_pixels(GLOW_SPREAD, size)).filter(ImageFilter.GaussianBlur(to_pixels(GLOW_BLUR, size)))

    image = Image.new("RGBA", big_size, (0, 0, 0, 0))
    image.alpha_composite(solid(big_size, GLOW_COLOR, glow, GLOW_ALPHA))
    image.alpha_composite(solid(big_size, OUTLINE_COLOR, outline, OUTLINE_ALPHA))
    # Clear the inside first so the translucent fill shows what is behind the cursor, not the outline color
    inside = Image.new("RGBA", big_size, (0, 0, 0, 0))
    image.paste(inside, (0, 0), mask)
    image.alpha_composite(fill_gradient(big_size, mask))
    image.alpha_composite(solid(big_size, RIM_COLOR, rim, 255))
    return image.resize((size, size), Image.LANCZOS)


def write_preview(cursors):
    """All sizes on dark, light and blue backgrounds, zoomed with nearest neighbor."""
    gap = 8
    total_width = gap
    for cursor in cursors:
        total_width += cursor.width + gap
    total_height = (GRID + gap) * len(PREVIEW_BACKGROUNDS) + gap
    sheet = Image.new("RGBA", (total_width, total_height))
    y = 0
    for background in PREVIEW_BACKGROUNDS:
        band = Image.new("RGBA", (total_width, GRID + gap * 2), background + (255,))
        sheet.alpha_composite(band, (0, y))
        x = gap
        for cursor in cursors:
            sheet.alpha_composite(cursor, (x, y + gap))
            x += cursor.width + gap
        y += GRID + gap
    sheet = sheet.resize((sheet.width * PREVIEW_ZOOM, sheet.height * PREVIEW_ZOOM), Image.NEAREST)
    PREVIEW_PNG.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(PREVIEW_PNG)
    print(f"Preview: {PREVIEW_PNG}")


def main():
    cursors = []
    for size in CURSOR_SIZES:
        cursor = draw_cursor(size)
        path = OUTPUT_DIR / f"PcCursor{size}.png"
        cursor.save(path)
        print(f"Cursor {size}: {path}")
        cursors.append(cursor)
    write_preview(cursors)


if __name__ == "__main__":
    main()
