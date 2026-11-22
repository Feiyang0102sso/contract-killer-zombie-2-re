"""Draws candidate city map icons for The Pit (PC port mission, see Source/Runtime/PcPort/PcPitMission.cs).

The stock mission icons in the HD GUILocations atlas are white and get their color at run time: MapLocation
multiplies the sprite by the mission color (Survival and SpecialHunt: 255, 171, 0). The survival and special hunt
icons are the big variant (137 x 162 px in the HD atlas, 21.4 x 25.3 units on the map) with a child "Glow"
sprite (icon_glow, already orange, not tinted) centered behind them. The Pit uses the same big variant.

Steps:
  1. crop the three big icons (survival, story, special hunt) from the HD atlas;
  2. rebuild a clean frame: their frames are identical and only the glyph square differs, so each pixel takes the
     icon whose glyph does not cover it;
  3. draw each candidate glyph as a white mask (supersampled, then scaled down) into the glyph square;
  4. write every candidate as a white icon, plus a preview sheet with the stock icons for comparison and each
     candidate white, tinted with the survival orange over the glow, and at half size;
  5. write the chosen candidate (CHOSEN_CANDIDATE) to Assets/Resources/PcPort/PcPitIcon.png, the texture of the
     PcPitIcon material that PcPitMission puts on its copy of the special hunt button.

Usage (from ckz2_repo_pc):
    ../.venv/Scripts/python.exe tools/make_pit_icons.py
"""
import math
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFont

# ---- Paths ----
PROJECT_DIR = Path(__file__).resolve().parent.parent
SOURCE_ATLAS = PROJECT_DIR / "Assets" / "Resources" / "copiedtextures" / "GUILocations.png"
OUTPUT_DIR = PROJECT_DIR / "tools" / "out" / "pit_icons"
PREVIEW_PNG = OUTPUT_DIR / "preview.png"
# The candidate the game uses (picked 2026-11-21), written as the texture of the PcPitIcon material
CHOSEN_CANDIDATE = "zombie_hand"
GAME_ICON_PNG = PROJECT_DIR / "Assets" / "Resources" / "PcPort" / "PcPitIcon.png"

# ---- Source sprites: EZGUI UVs from CityMap.unity (x, y, width, height; y measured from the bottom) ----
SURVIVAL_UV = (0.40966797, 0.91748047, 0.06689453, 0.07910156)
STORY_UV = (0.9135742, 0.3022461, 0.06689453, 0.07910156)
SPECIAL_HUNT_UV = (0.34228516, 0.91748047, 0.06689453, 0.07910156)
GLOW_UV = (0.89697266, 0.0, 0.088378906, 0.099121094)

# ---- Glyph square inside the 137 x 162 icon (the stock glyphs differ only here) ----
GLYPH_LEFT = 14
GLYPH_TOP = 14
GLYPH_SIZE = 108
SUPERSAMPLE = 4
# Where all three stock glyphs overlap (the center dot), the panel is copied from this many pixels to the left
OVERLAP_PATCH_SHIFT = 10

# ---- Colors ----
# MapLocation.Survival (also used for SpecialHunt)
SURVIVAL_COLOR = (255, 171, 0)

# ---- Preview ----
PREVIEW_BACKGROUND = (38, 42, 46, 255)
PREVIEW_LABEL_COLOR = (220, 220, 220, 255)
PREVIEW_FONT = Path("C:/Windows/Fonts/arialbd.ttf")
PREVIEW_FONT_SIZE = 16
PREVIEW_CELL_PADDING = 16
PREVIEW_LABEL_HEIGHT = 28
PREVIEW_COLUMNS = 3

# ---- Text glyph ----
NUMBER_FONT = Path("C:/Windows/Fonts/impact.ttf")


# =====================================================================================================================
# Source sprites
# =====================================================================================================================

def crop_uv(atlas, uv):
    """Crops an EZGUI frame (UVs with the origin at the bottom left) from the atlas."""
    x, y, width, height = uv
    atlas_width, atlas_height = atlas.size
    left = round(x * atlas_width)
    right = round((x + width) * atlas_width)
    top = round((1.0 - y - height) * atlas_height)
    bottom = round((1.0 - y) * atlas_height)
    return atlas.crop((left, top, right, bottom))


def rebuild_frame(icons):
    """Returns the icon frame without a glyph: per pixel, the icon with the least white (glyph) there."""
    frame = icons[0].copy()
    sources = []
    for icon in icons:
        sources.append(icon.load())
    target = frame.load()
    width, height = frame.size
    for y in range(height):
        for x in range(width):
            target[x, y] = least_white(sources, x, y)
    # Inside the glyph square the panel is black; white left there means all three glyphs overlap
    # (only the center dot). The panel center is flat, so those pixels take the panel a little to the left.
    patched = 0
    for y in range(GLYPH_TOP, GLYPH_TOP + GLYPH_SIZE):
        for x in range(GLYPH_LEFT, GLYPH_LEFT + GLYPH_SIZE):
            if target[x, y][0] > 0:
                target[x, y] = least_white(sources, x - OVERLAP_PATCH_SHIFT, y)
                patched += 1
    print("frame rebuilt, overlapping glyph pixels patched: " + str(patched))
    return frame


def least_white(sources, x, y):
    """The source pixel with the lowest red value (the panel is black, glyphs are white)."""
    best = sources[0][x, y]
    for source in sources[1:]:
        pixel = source[x, y]
        if pixel[0] < best[0]:
            best = pixel
    return best


# =====================================================================================================================
# Glyph drawing
# =====================================================================================================================

class GlyphPen:
    """White-on-black mask of the glyph square, in glyph pixels (0..GLYPH_SIZE), drawn supersampled.

    ink=True paints the glyph, ink=False cuts it away (eye holes, gaps).
    """

    def __init__(self):
        size = GLYPH_SIZE * SUPERSAMPLE
        self.image = Image.new("L", (size, size), 0)
        self.draw = ImageDraw.Draw(self.image)

    @staticmethod
    def _value(ink):
        if ink:
            return 255
        return 0

    @staticmethod
    def _scale(value):
        return value * SUPERSAMPLE

    def _points(self, points):
        scaled = []
        for x, y in points:
            scaled.append((self._scale(x), self._scale(y)))
        return scaled

    def ellipse(self, cx, cy, rx, ry, ink=True):
        box = (self._scale(cx - rx), self._scale(cy - ry), self._scale(cx + rx), self._scale(cy + ry))
        self.draw.ellipse(box, fill=self._value(ink))

    def circle(self, cx, cy, radius, ink=True):
        self.ellipse(cx, cy, radius, radius, ink)

    def ring(self, cx, cy, rx, ry, width, ink=True):
        """Ellipse outline; width grows inward from the radii."""
        box = (self._scale(cx - rx), self._scale(cy - ry), self._scale(cx + rx), self._scale(cy + ry))
        self.draw.ellipse(box, outline=self._value(ink), width=round(self._scale(width)))

    def arc(self, cx, cy, rx, ry, start_degrees, end_degrees, width, ink=True):
        """Part of an ellipse outline; angles clockwise from +x (screen coordinates)."""
        box = (self._scale(cx - rx), self._scale(cy - ry), self._scale(cx + rx), self._scale(cy + ry))
        self.draw.arc(box, start_degrees, end_degrees, fill=self._value(ink), width=round(self._scale(width)))

    def polygon(self, points, ink=True):
        self.draw.polygon(self._points(points), fill=self._value(ink))

    def rounded_rect(self, left, top, right, bottom, radius, ink=True):
        box = (self._scale(left), self._scale(top), self._scale(right), self._scale(bottom))
        self.draw.rounded_rectangle(box, radius=self._scale(radius), fill=self._value(ink))

    def stroke(self, points, width, ink=True):
        """Polyline with round caps and joints."""
        self.draw.line(self._points(points), fill=self._value(ink), width=round(self._scale(width)))
        for x, y in points:
            self.circle(x, y, width / 2.0, ink)

    def wave(self, left, right, y, amplitude, wavelength, width, phase=0.0, ink=True):
        """Horizontal sine line."""
        points = []
        steps = 48
        for i in range(steps + 1):
            x = left + (right - left) * i / steps
            offset = amplitude * math.sin((x - left) / wavelength * 2.0 * math.pi + phase)
            points.append((x, y + offset))
        self.stroke(points, width, ink)

    def wave_cut_below(self, left, right, y, amplitude, wavelength, phase=0.0):
        """Cuts everything below a sine line (used to sink shapes into liquid)."""
        points = []
        steps = 48
        for i in range(steps + 1):
            x = left + (right - left) * i / steps
            offset = amplitude * math.sin((x - left) / wavelength * 2.0 * math.pi + phase)
            points.append((x, y + offset))
        points.append((right, GLYPH_SIZE + 10))
        points.append((left, GLYPH_SIZE + 10))
        self.polygon(points, ink=False)

    def text(self, cx, cy, text, font_path, height, ink=True):
        """Text centered on (cx, cy); height is the cap height in glyph pixels."""
        font = ImageFont.truetype(str(font_path), round(self._scale(height) * 1.35))
        left, top, right, bottom = self.draw.textbbox((0, 0), text, font=font)
        x = self._scale(cx) - (left + right) / 2.0
        y = self._scale(cy) - (top + bottom) / 2.0
        self.draw.text((x, y), text, fill=self._value(ink), font=font)

    def xor_polygon(self, points):
        """Inverts the mask inside a polygon: white strokes over black, black cuts through white."""
        layer = Image.new("L", self.image.size, 0)
        ImageDraw.Draw(layer).polygon(self._points(points), fill=255)
        self.image = ImageChops.difference(self.image, layer)
        self.draw = ImageDraw.Draw(self.image)

    def result(self):
        """The glyph mask at the icon's resolution."""
        return self.image.resize((GLYPH_SIZE, GLYPH_SIZE), Image.LANCZOS)


def draw_skull(pen, cx, cy, scale, grow=0.0, ink=True):
    """Front-facing skull: cranium, cheek bones, jaw; eye sockets, nose and teeth gaps are cut.

    (cx, cy) is the cranium center; grow > 0 draws only a fattened silhouette (to cut a gap around the skull).
    """
    pen.ellipse(cx, cy, 30 * scale + grow, 27 * scale + grow, ink)
    pen.rounded_rect(cx - 19 * scale - grow, cy + 8 * scale - grow,
                     cx + 19 * scale + grow, cy + 36 * scale + grow, 7 * scale + grow, ink)
    if grow > 0 or not ink:
        return
    # Eye sockets
    pen.ellipse(cx - 12 * scale, cy + 7 * scale, 8.5 * scale, 9 * scale, ink=False)
    pen.ellipse(cx + 12 * scale, cy + 7 * scale, 8.5 * scale, 9 * scale, ink=False)
    # Nose
    pen.polygon([(cx, cy + 15 * scale), (cx - 4.5 * scale, cy + 23 * scale), (cx + 4.5 * scale, cy + 23 * scale)], ink=False)
    # Teeth: three gaps from the mouth line down
    for offset in (-8.0, 0.0, 8.0):
        pen.stroke([(cx + offset * scale, cy + 28 * scale), (cx + offset * scale, cy + 37 * scale)], 2.6 * scale, ink=False)
    pen.stroke([(cx - 13 * scale, cy + 28.5 * scale), (cx + 13 * scale, cy + 28.5 * scale)], 2.4 * scale, ink=False)


def glyph_skull_pool():
    """Skull sinking into a toxic pool, bubbles around it."""
    pen = GlyphPen()
    draw_skull(pen, 54, 34, 1.0)
    pen.wave_cut_below(0, GLYPH_SIZE, 72, 3.0, 27.0)
    pen.wave(4, 104, 79, 3.0, 27.0, 5.0)
    pen.wave(14, 94, 93, 3.0, 27.0, 5.0, phase=math.pi)
    pen.wave(30, 78, 105, 2.0, 24.0, 4.0)
    pen.ring(14, 52, 5, 5, 2.6)
    pen.ring(95, 44, 6, 6, 2.6)
    pen.circle(89, 60, 2.6)
    pen.circle(20, 36, 2.2)
    return pen.result()


def glyph_zombie_hand():
    """Clawed hand rising out of the pool."""
    pen = GlyphPen()
    # Forearm and palm
    pen.rounded_rect(41, 60, 69, 100, 5)
    pen.rounded_rect(36, 40, 72, 70, 9)
    # Fingers, bent at the tips like claws
    pen.stroke([(42, 44), (38, 24), (31, 14)], 8.5)
    pen.stroke([(52, 42), (51, 16), (46, 5)], 8.5)
    pen.stroke([(62, 43), (66, 18), (63, 8)], 8.5)
    pen.stroke([(70, 50), (80, 32), (79, 22)], 7.5)
    pen.stroke([(38, 64), (25, 54), (20, 42)], 8.5)
    # Gaps between the fingers where they meet the palm
    pen.stroke([(47, 40), (47, 33)], 1.8, ink=False)
    pen.stroke([(57, 40), (58, 33)], 1.8, ink=False)
    pen.stroke([(67, 44), (70, 38)], 1.8, ink=False)
    # Torn sleeve: a ragged cuff a bit wider than the forearm, separated from the wrist by a zigzag gap
    pen.polygon([(37, 82), (73, 82), (73, 100), (37, 100)])
    pen.stroke([(37, 80), (43, 84), (49, 79), (55, 85), (61, 79), (67, 84), (73, 80)], 2.4, ink=False)
    # Pool
    pen.wave_cut_below(0, GLYPH_SIZE, 90, 2.5, 30.0)
    pen.wave(2, 106, 95, 2.5, 30.0, 5.0)
    pen.wave(16, 92, 105, 2.0, 26.0, 4.0, phase=math.pi)
    pen.ring(16, 80, 5, 5, 2.6)
    pen.circle(92, 74, 3.0)
    pen.circle(84, 85, 2.0)
    return pen.result()


def almond_points(cx, cy, half_width, half_height):
    """Eye outline: two sine arcs meeting at sharp corners."""
    points = []
    steps = 40
    for i in range(steps + 1):
        t = i / steps
        points.append((cx - half_width + 2 * half_width * t, cy - half_height * math.sin(math.pi * t)))
    for i in range(steps + 1):
        t = i / steps
        points.append((cx + half_width - 2 * half_width * t, cy + half_height * math.sin(math.pi * t)))
    return points


def glyph_watcher_eye():
    """Slit-pupil eye watching over the pit."""
    pen = GlyphPen()
    pen.polygon(almond_points(54, 40, 50, 27))
    pen.polygon(almond_points(54, 40, 43, 20.5), ink=False)
    pen.circle(54, 40, 17)
    pen.ellipse(54, 40, 4.5, 13, ink=False)
    pen.circle(61, 32, 3, ink=False)
    # The pit below, in perspective
    pen.ring(54, 92, 46, 13, 4.5)
    pen.ring(54, 94, 29, 7.5, 3.5)
    pen.ellipse(54, 95, 13, 3)
    return pen.result()


def glyph_biohazard():
    """Biohazard symbol: three crescents around a ring."""
    pen = GlyphPen()
    cx = 54.0
    cy = 58.0
    crescent_distance = 21.0
    crescent_radius = 24.0
    opening_distance = 29.0
    opening_radius = 16.5
    angles = (-90.0, 30.0, 150.0)
    # Ring, separated from the crescents by a thin gap
    pen.ring(cx, cy, 27, 27, 5.5)
    for angle in angles:
        dx = math.cos(math.radians(angle))
        dy = math.sin(math.radians(angle))
        pen.circle(cx + dx * crescent_distance, cy + dy * crescent_distance, crescent_radius + 2.5, ink=False)
    for angle in angles:
        dx = math.cos(math.radians(angle))
        dy = math.sin(math.radians(angle))
        pen.circle(cx + dx * crescent_distance, cy + dy * crescent_distance, crescent_radius)
    for angle in angles:
        dx = math.cos(math.radians(angle))
        dy = math.sin(math.radians(angle))
        pen.circle(cx + dx * opening_distance, cy + dy * opening_distance, opening_radius, ink=False)
    # Center hole and the three slots from it
    pen.circle(cx, cy, 7.0, ink=False)
    for angle in angles:
        dx = math.cos(math.radians(angle))
        dy = math.sin(math.radians(angle))
        pen.stroke([(cx, cy), (cx + dx * 13.0, cy + dy * 13.0)], 2.6, ink=False)
    return pen.result()


def glyph_glass_watcher():
    """The psycho behind his glass: head and shoulders, crossed by glare stripes."""
    pen = GlyphPen()
    pen.ellipse(54, 102, 44, 32)
    pen.rounded_rect(46, 50, 62, 74, 4)
    pen.ellipse(54, 40, 17, 21)
    # Gap between head and shoulders
    pen.arc(54, 70, 20, 8, 200, 340, 2.4, ink=False)
    # Glare stripes right of the head: white over the dark panel, black through the shoulder
    pen.xor_polygon([(86, 0), (96, 0), (61, GLYPH_SIZE), (51, GLYPH_SIZE)])
    pen.xor_polygon([(101, 0), (105, 0), (70, GLYPH_SIZE), (66, GLYPH_SIZE)])
    return pen.result()


def glyph_kill_100():
    """Small skull over the kill count of the level."""
    pen = GlyphPen()
    draw_skull(pen, 54, 22, 0.62)
    pen.text(54, 79, "100", NUMBER_FONT, 34)
    return pen.result()


def glyph_skull_crossbones():
    """Skull over crossed bones."""
    pen = GlyphPen()
    bones = (((16, 64), (92, 102)), ((92, 64), (16, 102)))
    for start, end in bones:
        pen.stroke([start, end], 8.0)
        length = math.hypot(end[0] - start[0], end[1] - start[1])
        nx = -(end[1] - start[1]) / length
        ny = (end[0] - start[0]) / length
        for x, y in (start, end):
            pen.circle(x + nx * 4.5, y + ny * 4.5, 5.5)
            pen.circle(x - nx * 4.5, y - ny * 4.5, 5.5)
    draw_skull(pen, 54, 34, 0.95, grow=3.0, ink=False)
    draw_skull(pen, 54, 34, 0.95)
    return pen.result()


def glyph_psycho_grin():
    """The psycho's face reduced to angry eyes and a wide toothy grin ("Impress me, Evelyn!")."""
    pen = GlyphPen()
    # Grin: a crescent, the lower part of one ellipse minus a higher one
    pen.ellipse(54, 58, 47, 42)
    pen.ellipse(54, 38, 54, 36, ink=False)
    # Teeth: vertical gaps and the line between the two rows
    for x in (22, 32, 43, 54, 65, 76, 86):
        pen.stroke([(x, 60), (x, 102)], 2.4, ink=False)
    pen.arc(54, 60, 44, 28, 15, 165, 2.4, ink=False)
    # Eyes slanting down toward the nose (drawn last: the grin's cut ellipse covers them)
    pen.polygon([(12, 14), (46, 28), (43, 40), (17, 30)])
    pen.polygon([(96, 14), (62, 28), (65, 40), (91, 30)])
    return pen.result()


# Candidate name -> glyph function, in preview order
CANDIDATES = (
    ("skull_pool", glyph_skull_pool),
    ("zombie_hand", glyph_zombie_hand),
    ("watcher_eye", glyph_watcher_eye),
    ("biohazard", glyph_biohazard),
    ("glass_watcher", glyph_glass_watcher),
    ("kill_100", glyph_kill_100),
    ("skull_crossbones", glyph_skull_crossbones),
    ("psycho_grin", glyph_psycho_grin),
)


# =====================================================================================================================
# Composition and preview
# =====================================================================================================================

def compose_icon(frame, glyph_mask):
    """White glyph over the clean frame."""
    icon = frame.copy()
    white = Image.new("RGBA", (GLYPH_SIZE, GLYPH_SIZE), (255, 255, 255, 255))
    white.putalpha(glyph_mask)
    icon.alpha_composite(white, (GLYPH_LEFT, GLYPH_TOP))
    return icon


def tint(image, color):
    """Multiplies RGB by color, like the Sprite Vertex Colored shader with the mission color."""
    red, green, blue, alpha = image.split()
    red = red.point(make_multiply_table(color[0]))
    green = green.point(make_multiply_table(color[1]))
    blue = blue.point(make_multiply_table(color[2]))
    return Image.merge("RGBA", (red, green, blue, alpha))


def make_multiply_table(factor):
    table = []
    for value in range(256):
        table.append(value * factor // 255)
    return table


def on_map(icon, glow):
    """Icon tinted with the survival color over its glow, as MapLocation shows it."""
    canvas = Image.new("RGBA", glow.size, (0, 0, 0, 0))
    canvas.alpha_composite(glow)
    offset_x = (glow.width - icon.width) // 2
    offset_y = (glow.height - icon.height) // 2
    canvas.alpha_composite(tint(icon, SURVIVAL_COLOR), (offset_x, offset_y))
    return canvas


def build_preview(entries, glow):
    """One cell per entry: white icon, tinted icon with glow, tinted at half size; label on top."""
    font = ImageFont.truetype(str(PREVIEW_FONT), PREVIEW_FONT_SIZE)
    half_width = glow.width // 2
    half_height = glow.height // 2
    cell_width = PREVIEW_CELL_PADDING * 4 + entries[0][1].width + glow.width + half_width
    cell_height = PREVIEW_LABEL_HEIGHT + glow.height + PREVIEW_CELL_PADDING
    rows = (len(entries) + PREVIEW_COLUMNS - 1) // PREVIEW_COLUMNS
    sheet = Image.new("RGBA", (cell_width * PREVIEW_COLUMNS, cell_height * rows + PREVIEW_CELL_PADDING), PREVIEW_BACKGROUND)
    draw = ImageDraw.Draw(sheet)
    for index, (label, icon) in enumerate(entries):
        cell_x = (index % PREVIEW_COLUMNS) * cell_width
        cell_y = (index // PREVIEW_COLUMNS) * cell_height + PREVIEW_CELL_PADDING
        draw.text((cell_x + PREVIEW_CELL_PADDING, cell_y), label, fill=PREVIEW_LABEL_COLOR, font=font)
        top = cell_y + PREVIEW_LABEL_HEIGHT
        x = cell_x + PREVIEW_CELL_PADDING
        sheet.alpha_composite(icon, (x, top + (glow.height - icon.height) // 2))
        x += icon.width + PREVIEW_CELL_PADDING
        shown = on_map(icon, glow)
        sheet.alpha_composite(shown, (x, top))
        x += glow.width + PREVIEW_CELL_PADDING
        small = shown.resize((half_width, half_height), Image.LANCZOS)
        sheet.alpha_composite(small, (x, top + (glow.height - half_height) // 2))
    return sheet


def main():
    atlas = Image.open(SOURCE_ATLAS).convert("RGBA")
    survival = crop_uv(atlas, SURVIVAL_UV)
    story = crop_uv(atlas, STORY_UV)
    special_hunt = crop_uv(atlas, SPECIAL_HUNT_UV)
    glow = crop_uv(atlas, GLOW_UV)
    print("icon " + str(survival.size) + ", glow " + str(glow.size))
    frame = rebuild_frame([survival, story, special_hunt])

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    entries = [("stock: survival", survival), ("stock: special hunt (current Pit)", special_hunt)]
    for number, (name, draw_glyph) in enumerate(CANDIDATES, start=1):
        icon = compose_icon(frame, draw_glyph())
        label = str(number) + ". " + name
        path = OUTPUT_DIR / ("PcPitIcon_" + str(number) + "_" + name + ".png")
        icon.save(path)
        print("wrote " + str(path.relative_to(PROJECT_DIR)))
        if name == CHOSEN_CANDIDATE:
            icon.save(GAME_ICON_PNG)
            print("wrote " + str(GAME_ICON_PNG.relative_to(PROJECT_DIR)) + " (game icon)")
            label = label + "  [in game]"
        entries.append((label, icon))
    preview = build_preview(entries, glow)
    preview.save(PREVIEW_PNG)
    print("wrote " + str(PREVIEW_PNG.relative_to(PROJECT_DIR)))


if __name__ == "__main__":
    main()
