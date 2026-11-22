"""Draws the PC exit button: a red X on the stock square button frame of the city map.

The frame and the dark teal panel come from the AchievementsButton frame in the HD GUICityMap atlas
(same 195 x 193 px size as the Facebook button it is meant to replace). The trophy is erased by
rebuilding the panel from its own edge pixels, then a red X is drawn in the same style as the
stock icons: vertical gradient fill, thin dark outline, thin light bevel line on the lower right.

The press effect needs no extra frame: UIPressableButton swaps in the CKZ2/UI-Self-Illuminated
shader and multiplies the texture by _OnPressMultiplier (1.5), so only one frame is drawn here.
A preview with both states is written next to the output.

Usage (from ckz2_repo_pc):
    ../.venv/Scripts/python.exe tools/port_pc/art/make_exit_button.py
"""
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

# ---- Paths ----
# tools/port_pc/<category>/<script>.py -> project root
PROJECT_DIR = Path(__file__).resolve().parents[3]
SOURCE_ATLAS = PROJECT_DIR / "Assets" / "Resources" / "copiedtextures" / "GUICityMap.png"
OUTPUT_PNG = PROJECT_DIR / "Assets" / "Resources" / "PcPort" / "PcExitButton.png"
PREVIEW_PNG = PROJECT_DIR / "tools" / "out" / "PcExitButton_preview.png"

# ---- Source frame ----
# AchievementsButton UVs in CommonGUI.prefab, in pixels of the 1024 x 1024 HD atlas (left, top, right, bottom)
FRAME_BOX = (336, 21, 531, 214)
# Panel area that gets rebuilt (inside the dark rim of the frame), inclusive
PANEL_LEFT = 15
PANEL_RIGHT = 179
PANEL_TOP = 15
PANEL_BOTTOM = 177
# Glyph-free column and row the panel is rebuilt from
REF_COLUMN = 20
REF_ROW = 96
# Columns closer than this to the panel side keep the side shadow; further in, the panel is flat
SIDE_SHADOW_WIDTH = 16

# ---- Red X ----
SUPERSAMPLE = 4
X_CENTER = (97.0, 96.0)
# Distance from the center to the end of each arm, along the arm
X_ARM_LENGTH = 68.0
X_THICKNESS = 30.0
RED_TOP = (255, 78, 60)
RED_BOTTOM = (160, 16, 14)
OUTLINE_COLOR = (8, 10, 10)
OUTLINE_ALPHA = 220
OUTLINE_WIDTH = 1.5
BEVEL_COLOR = (170, 168, 160)
BEVEL_ALPHA = 170
BEVEL_OFFSET = (1.0, 1.5)

# ---- Press effect (UIPressableButton._OnPressMultiplier) ----
PRESS_MULTIPLIER = 1.5
PREVIEW_BACKGROUND = (70, 70, 70, 255)
PREVIEW_GAP = 24


def rebuild_panel(frame):
    """Paints over the panel interior with a glyph-free copy built from one column and one row."""
    pixels = frame.load()
    ref_row_at_column = pixels[REF_COLUMN, REF_ROW]
    # Horizontal shading factor per column (side shadow), relative to the reference column
    column_factors = {}
    for x in range(PANEL_LEFT, PANEL_RIGHT + 1):
        distance_left = x - PANEL_LEFT
        distance_right = PANEL_RIGHT - x
        if distance_left < SIDE_SHADOW_WIDTH:
            sample_x = x
        elif distance_right < SIDE_SHADOW_WIDTH:
            sample_x = x
        else:
            # Flat middle: reuse the factor where the side shadow ends
            sample_x = PANEL_LEFT + SIDE_SHADOW_WIDTH
        sample = pixels[sample_x, REF_ROW]
        factors = []
        for channel in range(3):
            factors.append(sample[channel] / max(ref_row_at_column[channel], 1))
        column_factors[x] = factors
    # Vertical shading comes straight from the reference column
    for y in range(PANEL_TOP, PANEL_BOTTOM + 1):
        base = pixels[REF_COLUMN, y]
        for x in range(PANEL_LEFT, PANEL_RIGHT + 1):
            factors = column_factors[x]
            red = min(255, round(base[0] * factors[0]))
            green = min(255, round(base[1] * factors[1]))
            blue = min(255, round(base[2] * factors[2]))
            pixels[x, y] = (red, green, blue, 255)


def x_mask(size):
    """Returns the X shape as an L mask at supersampled resolution."""
    mask = Image.new("L", size, 0)
    draw = ImageDraw.Draw(mask)
    center_x = X_CENTER[0] * SUPERSAMPLE
    center_y = X_CENTER[1] * SUPERSAMPLE
    # Half length / half width of each bar, rotated 45 degrees: unit vectors along and across the bar
    half_length = X_ARM_LENGTH * SUPERSAMPLE
    half_width = X_THICKNESS * SUPERSAMPLE / 2
    diagonal = 0.70710678
    for direction in (1, -1):
        along = (diagonal, diagonal * direction)
        across = (-along[1], along[0])
        corners = []
        for sign_along, sign_across in ((1, 1), (1, -1), (-1, -1), (-1, 1)):
            corner_x = center_x + along[0] * half_length * sign_along + across[0] * half_width * sign_across
            corner_y = center_y + along[1] * half_length * sign_along + across[1] * half_width * sign_across
            corners.append((corner_x, corner_y))
        draw.polygon(corners, fill=255)
    return mask


def grow(mask, pixels):
    """Dilates a supersampled mask by the given number of output pixels."""
    radius = round(pixels * SUPERSAMPLE)
    return mask.filter(ImageFilter.MaxFilter(radius * 2 + 1))


def shift(mask, offset):
    """Moves a supersampled mask by an offset given in output pixels."""
    return ImageChops.offset(mask, round(offset[0] * SUPERSAMPLE), round(offset[1] * SUPERSAMPLE))


def solid_layer(size, color, mask, alpha):
    """Returns a single-color RGBA layer whose alpha is the mask scaled by alpha / 255."""
    layer = Image.new("RGBA", size, color + (0,))
    scaled = mask.point(_alpha_table(alpha))
    layer.putalpha(scaled)
    return layer


def _alpha_table(alpha):
    """Lookup table that scales 0..255 mask values to 0..alpha."""
    table = []
    for value in range(256):
        table.append(value * alpha // 255)
    return table


def gradient_layer(size, mask):
    """Returns the red fill: a vertical gradient over the glyph's height, cut by the mask."""
    top, bottom = mask.getbbox()[1], mask.getbbox()[3]
    layer = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)
    for y in range(size[1]):
        if y <= top:
            t = 0.0
        elif y >= bottom:
            t = 1.0
        else:
            t = (y - top) / (bottom - top)
        color = []
        for channel in range(3):
            color.append(round(RED_TOP[channel] + (RED_BOTTOM[channel] - RED_TOP[channel]) * t))
        draw.line([(0, y), (size[0], y)], fill=tuple(color) + (255,))
    layer.putalpha(mask)
    return layer


def draw_x(frame):
    """Draws the red X onto the frame, supersampled then scaled down."""
    big_size = (frame.width * SUPERSAMPLE, frame.height * SUPERSAMPLE)
    mask = x_mask(big_size)
    outline = grow(mask, OUTLINE_WIDTH)
    # Light bevel only shows where the shifted outline sticks out below / right of the dark outline
    bevel = ImageChops.subtract(shift(outline, BEVEL_OFFSET), outline)

    glyph = Image.new("RGBA", big_size, (0, 0, 0, 0))
    glyph.alpha_composite(solid_layer(big_size, BEVEL_COLOR, bevel, BEVEL_ALPHA))
    glyph.alpha_composite(solid_layer(big_size, OUTLINE_COLOR, outline, OUTLINE_ALPHA))
    glyph.alpha_composite(gradient_layer(big_size, mask))
    glyph = glyph.resize(frame.size, Image.LANCZOS)
    frame.alpha_composite(glyph)


def pressed(image):
    """Mimics CKZ2/UI-Self-Illuminated: every RGBA channel times _OnPressMultiplier, clamped."""
    table = []
    for value in range(256):
        table.append(min(255, round(value * PRESS_MULTIPLIER)))
    return image.point(table * 4)


def write_preview(button):
    """Saves normal and pressed states side by side, at full size and at half size (in-game scale)."""
    states = [button, pressed(button)]
    half_size = (button.width // 2, button.height // 2)
    width = (button.width + half_size[0]) * 2 + PREVIEW_GAP * 5
    height = button.height + PREVIEW_GAP * 2
    sheet = Image.new("RGBA", (width, height), PREVIEW_BACKGROUND)
    x = PREVIEW_GAP
    for state in states:
        sheet.alpha_composite(state, (x, PREVIEW_GAP))
        x += button.width + PREVIEW_GAP
    for state in states:
        small = state.resize(half_size, Image.LANCZOS)
        sheet.alpha_composite(small, (x, PREVIEW_GAP))
        x += half_size[0] + PREVIEW_GAP
    PREVIEW_PNG.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(PREVIEW_PNG)
    print(f"Preview: {PREVIEW_PNG}")


def main():
    atlas = Image.open(SOURCE_ATLAS).convert("RGBA")
    frame = atlas.crop(FRAME_BOX)
    print(f"Frame {FRAME_BOX} from {SOURCE_ATLAS.name}: {frame.size}")
    rebuild_panel(frame)
    draw_x(frame)
    OUTPUT_PNG.parent.mkdir(parents=True, exist_ok=True)
    frame.save(OUTPUT_PNG)
    print(f"Button: {OUTPUT_PNG}")
    write_preview(frame)


if __name__ == "__main__":
    main()
