"""Builds the key-hint banner sprite sheet (red strips behind PC key hints such as SPACE / R / P).

Every banner is drawn as SVG by its own function (tunable values at the top of each function), the sheet is
rendered to PNG by headless Edge, and the result replaces Assets/Resources/PcPort/PcKeyBanners.png.

Sheet layout: 4 columns x 5 rows, each cell 512 x 256 px, banner N (1-based) in reading order.
Each banner is drawn centered in its cell; the game maps the whole cell onto the banner quad
(PcKeyHints.BannerIndex picks the cell), so keep shapes inside the cell with a small margin.

Usage (from ckz2_repo_pc):
    ../.venv/Scripts/python.exe tools/art/make_key_banners.py
"""
import shutil
import struct
import subprocess
import tempfile
import time
from pathlib import Path

# ---- Output ----
# tools/<category>/<script>.py -> project root
PROJECT_DIR = Path(__file__).resolve().parents[2]
OUTPUT_PNG = PROJECT_DIR / "Assets" / "Resources" / "PcPort" / "PcKeyBanners.png"
EDGE_EXE = Path(r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe")
# Seconds to wait for Edge to write the screenshot
RENDER_TIMEOUT = 30

# ---- Sheet layout (must match PcKeyHints.BannerSheetColumns / BannerSheetRows) ----
CELL_WIDTH = 512
CELL_HEIGHT = 256
COLUMNS = 4
ROWS = 5
SHEET_WIDTH = CELL_WIDTH * COLUMNS
SHEET_HEIGHT = CELL_HEIGHT * ROWS
# Cell center, where every banner is drawn around
CX = CELL_WIDTH / 2
CY = CELL_HEIGHT / 2

# ---- Shared colors ----
RED_TOP = "#ff5040"
RED_MIDDLE = "#e01212"
RED_BOTTOM = "#860404"
RED_DARK = "#a00808"
HIGHLIGHT = "#ff8070"

# ---- Shared drop shadow ----
SHADOW_OFFSET_Y = 4
SHADOW_BLUR = 3
SHADOW_OPACITY = 0.65


# ======================================================================
# Shared SVG pieces
# ======================================================================

def shared_defs():
    """Gradients and the drop shadow used by several banners."""
    return f"""
  <defs>
    <linearGradient id="red" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="{RED_TOP}"/><stop offset="0.4" stop-color="{RED_MIDDLE}"/><stop offset="1" stop-color="{RED_BOTTOM}"/>
    </linearGradient>
    <linearGradient id="fade" x1="0" y1="0" x2="1" y2="0">
      <stop offset="0" stop-color="{RED_MIDDLE}" stop-opacity="1"/><stop offset="0.6" stop-color="{RED_MIDDLE}" stop-opacity="0.9"/><stop offset="1" stop-color="{RED_BOTTOM}" stop-opacity="0"/>
    </linearGradient>
    <linearGradient id="gloss" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#ffffff" stop-opacity="0.45"/><stop offset="1" stop-color="#ffffff" stop-opacity="0"/>
    </linearGradient>
    <filter id="shadow" x="-20%" y="-40%" width="140%" height="190%">
      <feDropShadow dx="0" dy="{SHADOW_OFFSET_Y}" stdDeviation="{SHADOW_BLUR}" flood-color="#000" flood-opacity="{SHADOW_OPACITY}"/>
    </filter>
  </defs>"""


def rough_filter(filter_id, frequency_x, frequency_y, roughness, seed):
    """Ragged edges: displaces the shape by fractal noise. roughness = max displacement in px."""
    return f"""<filter id="{filter_id}" filterUnits="userSpaceOnUse" x="0" y="0" width="{CELL_WIDTH}" height="{CELL_HEIGHT}">
  <feTurbulence type="fractalNoise" baseFrequency="{frequency_x} {frequency_y}" numOctaves="3" seed="{seed}" result="noise"/>
  <feDisplacementMap in="SourceGraphic" in2="noise" scale="{roughness}" xChannelSelector="R" yChannelSelector="G"/>
</filter>"""


def brush_filter(filter_id, edge_frequency_x, edge_frequency_y, edge_roughness,
                 streak_frequency_x, streak_frequency_y, streak_strength, base_opacity, seed):
    """
    Brush stroke: ragged edges plus horizontal bristle streaks.
    streak_frequency_x small = long streaks; streak_frequency_y large = thin, dense streaks.
    streak_strength: contrast of the streaks; base_opacity: opacity of the paint between streaks.
    """
    streak_offset = streak_strength * 0.72
    return f"""<filter id="{filter_id}" filterUnits="userSpaceOnUse" x="0" y="0" width="{CELL_WIDTH}" height="{CELL_HEIGHT}">
  <feTurbulence type="fractalNoise" baseFrequency="{edge_frequency_x} {edge_frequency_y}" numOctaves="3" seed="{seed}" result="noise"/>
  <feDisplacementMap in="SourceGraphic" in2="noise" scale="{edge_roughness}" xChannelSelector="R" yChannelSelector="G" result="shape"/>
  <feTurbulence type="fractalNoise" baseFrequency="{streak_frequency_x} {streak_frequency_y}" numOctaves="2" seed="{seed + 7}" result="streakNoise"/>
  <feColorMatrix in="streakNoise" type="matrix" values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 {-streak_strength} {streak_offset}" result="streakAlpha"/>
  <feComposite in="shape" in2="streakAlpha" operator="in" result="streaks"/>
  <feComponentTransfer in="shape" result="base"><feFuncA type="linear" slope="{base_opacity}"/></feComponentTransfer>
  <feMerge><feMergeNode in="base"/><feMergeNode in="streaks"/></feMerge>
</filter>"""


def points_text(points):
    """[(x, y), ...] -> 'x,y x,y ...' for polygon points."""
    parts = []
    for point in points:
        parts.append(f"{point[0]:.1f},{point[1]:.1f}")
    return " ".join(parts)


def droplets(drops, fill):
    """Small circles around a splat; drops = [(dx, dy, radius), ...] relative to the cell center."""
    parts = []
    for drop in drops:
        parts.append(f'<circle cx="{CX + drop[0]}" cy="{CY + drop[1]}" r="{drop[2]}" fill="{fill}"/>')
    return "".join(parts)


# ======================================================================
# Banners: one function each, returns SVG markup in cell space (0..512 x 0..256)
# ======================================================================

def banner_01_long_brush():
    """Long brush stroke."""
    length = 470
    height = 56
    edge_roughness = 22
    streak_strength = 2.2
    base_opacity = 0.55
    seed = 3

    x = CX - length / 2
    y = CY - height / 2
    return (brush_filter("b01", 0.014, 0.11, edge_roughness, 0.003, 0.5, streak_strength, base_opacity, seed)
            + f'<g filter="url(#shadow)"><rect x="{x}" y="{y}" width="{length}" height="{height}" fill="url(#red)" filter="url(#b01)"/></g>')


def banner_02_slanted_strip():
    """Slightly tilted strip with jagged ends and a highlight along the top."""
    length = 450
    height = 62
    tilt_degrees = -3
    end_jag = 14
    edge_roughness = 11
    highlight_height = 11
    highlight_opacity = 0.45
    seed = 2

    x0 = CX - length / 2
    x1 = CX + length / 2
    y0 = CY - height / 2
    y1 = CY + height / 2
    outline = [
        (x0 + end_jag, y0), (x1 - end_jag, y0),
        (x1, y0 + height * 0.15), (x1 - end_jag * 0.6, y0 + height * 0.35), (x1 + end_jag * 0.3, y0 + height * 0.55),
        (x1 - end_jag * 0.8, y0 + height * 0.78), (x1 - end_jag * 0.2, y1),
        (x0 + end_jag * 0.5, y1),
        (x0 - end_jag * 0.3, y0 + height * 0.8), (x0 + end_jag * 0.6, y0 + height * 0.55),
        (x0 - end_jag * 0.5, y0 + height * 0.35), (x0 + end_jag * 0.3, y0 + height * 0.12),
    ]
    return (rough_filter("r02", 0.025, 0.16, edge_roughness, seed)
            + f'<g filter="url(#shadow)" transform="rotate({tilt_degrees} {CX} {CY})">'
            + f'<polygon points="{points_text(outline)}" fill="url(#red)" filter="url(#r02)"/>'
            + f'<rect x="{x0 + end_jag}" y="{y0 + 4}" width="{length - end_jag * 2}" height="{highlight_height}" fill="{HIGHLIGHT}" opacity="{highlight_opacity}" filter="url(#r02)"/>'
            + '</g>')


def banner_03_short_brush():
    """Short, thick brush stroke (the one used in game)."""
    length = 440
    height = 128
    edge_frequency_x = 0.011
    edge_frequency_y = 0.075
    edge_roughness = 24
    streak_frequency_x = 0.0022
    streak_frequency_y = 0.3
    streak_strength = 2.4
    base_opacity = 0.5
    seed = 11

    x = CX - length / 2
    y = CY - height / 2
    return (brush_filter("b03", edge_frequency_x, edge_frequency_y, edge_roughness,
                         streak_frequency_x, streak_frequency_y, streak_strength, base_opacity, seed)
            + f'<g filter="url(#shadow)"><rect x="{x}" y="{y}" width="{length}" height="{height}" fill="url(#red)" filter="url(#b03)"/></g>')


def banner_04_capsule():
    """Clean pill with a gloss band."""
    length = 460
    height = 84
    gloss_inset = 22
    gloss_height = 30

    x = CX - length / 2
    y = CY - height / 2
    radius = height / 2
    return (f'<g filter="url(#shadow)">'
            + f'<rect x="{x}" y="{y}" width="{length}" height="{height}" rx="{radius}" fill="url(#red)"/>'
            + f'<rect x="{x + gloss_inset}" y="{y + 4}" width="{length - gloss_inset * 2}" height="{gloss_height}" rx="{gloss_height / 2}" fill="url(#gloss)"/>'
            + '</g>')


def banner_05_circle():
    """Round disc with slightly ragged edge."""
    radius = 100
    edge_roughness = 14
    seed = 2

    return (rough_filter("r05", 0.02, 0.12, edge_roughness, seed)
            + f'<g filter="url(#shadow)"><circle cx="{CX}" cy="{CY}" r="{radius}" fill="url(#red)" filter="url(#r05)"/></g>')


def banner_06_circle_splat():
    """Round splat with a few droplets."""
    radius = 80
    splat_roughness = 45
    splat_frequency = 0.03
    seed = 5
    # (dx, dy, radius) from the cell center
    drops = [(110, -45, 13), (-105, 62, 11), (125, 58, 8), (-125, -60, 8)]

    return (rough_filter("r06", splat_frequency, splat_frequency, splat_roughness, seed)
            + f'<g filter="url(#shadow)"><g filter="url(#r06)">'
            + f'<circle cx="{CX}" cy="{CY}" r="{radius}" fill="url(#red)"/>'
            + droplets(drops, RED_MIDDLE)
            + '</g></g>')


def banner_07_rounded_rect():
    """Rounded rectangle with a thin inner border."""
    width = 320
    height = 110
    corner_radius = 20
    border_inset = 6
    border_width = 3
    border_opacity = 0.7

    x = CX - width / 2
    y = CY - height / 2
    return (f'<g filter="url(#shadow)">'
            + f'<rect x="{x}" y="{y}" width="{width}" height="{height}" rx="{corner_radius}" fill="url(#red)"/>'
            + f'<rect x="{x + border_inset}" y="{y + border_inset}" width="{width - border_inset * 2}" height="{height - border_inset * 2}" '
            + f'rx="{corner_radius - border_inset / 2}" fill="none" stroke="#ff7262" stroke-width="{border_width}" opacity="{border_opacity}"/>'
            + '</g>')


def banner_08_parallelogram():
    """Slanted parallelogram with a top highlight."""
    length = 420
    height = 76
    slant = 64
    highlight_height = 14
    highlight_opacity = 0.4

    x0 = CX - length / 2
    y0 = CY - height / 2
    y1 = CY + height / 2
    half_slant = slant / 2
    body = [(x0 - half_slant, y1), (x0 + half_slant, y0), (x0 + length + half_slant, y0), (x0 + length - half_slant, y1)]
    # Highlight follows the top edge
    step = slant * highlight_height / height
    highlight = [(x0 + half_slant - step + 4, y0 + 4 + highlight_height), (x0 + half_slant + 4, y0 + 4),
                 (x0 + length + half_slant - 6, y0 + 4), (x0 + length + half_slant - 6 - step, y0 + 4 + highlight_height)]
    return (f'<g filter="url(#shadow)">'
            + f'<polygon points="{points_text(body)}" fill="url(#red)"/>'
            + f'<polygon points="{points_text(highlight)}" fill="{HIGHLIGHT}" opacity="{highlight_opacity}"/>'
            + '</g>')


def banner_09_tapered_stroke():
    """Brush stroke, thick on the left, thin on the right."""
    length = 480
    thick_end = 64
    thin_end = 10
    edge_roughness = 22
    seed = 3

    x0 = CX - length / 2
    x1 = CX + length / 2
    path = (f"M{x0},{CY - thick_end / 2} "
            f"C{x0 + length * 0.15},{CY - thick_end * 0.8} {x1 - length * 0.2},{CY - thin_end} {x1},{CY} "
            f"C{x1 - length * 0.2},{CY + thin_end} {x0 + length * 0.25},{CY + thick_end * 0.7} {x0},{CY + thick_end / 2} Z")
    return (brush_filter("b09", 0.014, 0.11, edge_roughness, 0.003, 0.5, 2.2, 0.55, seed)
            + f'<g filter="url(#shadow)"><path d="{path}" fill="url(#red)" filter="url(#b09)"/></g>')


def banner_10_dripping_bar():
    """Bar with blood drips hanging from the bottom edge."""
    length = 450
    height = 58
    # Bar sits above center so bar + drips stay centered
    bar_center_offset = -30
    edge_roughness = 10
    seed = 2
    # (x as a fraction of the length, drip length, drip width)
    drips = [(0.12, 56, 12), (0.33, 30, 10), (0.6, 72, 14), (0.88, 38, 10)]

    x0 = CX - length / 2
    y0 = CY + bar_center_offset - height / 2
    y1 = y0 + height
    parts = []
    parts.append(f'<rect x="{x0}" y="{y0}" width="{length}" height="{height}"/>')
    for drip in drips:
        drip_x = x0 + length * drip[0]
        drip_length = drip[1]
        drip_width = drip[2]
        # Teardrop: narrow neck at the bar, round tip at the bottom
        parts.append(f'<path d="M{drip_x - drip_width / 2},{y1 - 4} Q{drip_x - drip_width / 3},{y1 + drip_length * 0.6} '
                     f'{drip_x - drip_width / 2},{y1 + drip_length - drip_width / 2} '
                     f'A{drip_width / 2},{drip_width / 2} 0 0 0 {drip_x + drip_width / 2},{y1 + drip_length - drip_width / 2} '
                     f'Q{drip_x + drip_width / 3},{y1 + drip_length * 0.6} {drip_x + drip_width / 2},{y1 - 4} Z"/>')
    return (rough_filter("r10", 0.025, 0.16, edge_roughness, seed)
            + f'<g filter="url(#shadow)" fill="url(#red)"><g filter="url(#r10)">' + "".join(parts) + '</g></g>')


def banner_11_swallowtail_ribbon():
    """Ribbon with V notches at both ends."""
    length = 470
    height = 64
    notch_depth = 30
    highlight_height = 10
    highlight_opacity = 0.4

    x0 = CX - length / 2
    x1 = CX + length / 2
    y0 = CY - height / 2
    y1 = CY + height / 2
    body = [(x0, y0), (x1, y0), (x1 - notch_depth, CY), (x1, y1), (x0, y1), (x0 + notch_depth, CY)]
    return (f'<g filter="url(#shadow)">'
            + f'<polygon points="{points_text(body)}" fill="url(#red)"/>'
            + f'<rect x="{x0 + notch_depth * 0.6}" y="{y0 + 5}" width="{length - notch_depth * 1.2}" height="{highlight_height}" fill="{HIGHLIGHT}" opacity="{highlight_opacity}"/>'
            + '</g>')


def banner_12_thin_scratch():
    """Thin, long brush scratch."""
    length = 480
    thickness = 26
    edge_roughness = 22
    seed = 11

    x0 = CX - length / 2
    x1 = CX + length / 2
    path = (f"M{x0},{CY - thickness * 0.2} C{x0 + length * 0.3},{CY - thickness * 0.7} {x1 - length * 0.3},{CY - thickness * 0.5} {x1},{CY - thickness * 0.3} "
            f"C{x1 - length * 0.3},{CY + thickness * 0.4} {x0 + length * 0.3},{CY + thickness * 0.6} {x0},{CY + thickness * 0.4} Z")
    return (brush_filter("b12", 0.02, 0.14, edge_roughness, 0.004, 0.55, 2.4, 0.5, seed)
            + f'<g filter="url(#shadow)"><path d="{path}" fill="url(#red)" filter="url(#b12)"/></g>')


def banner_13_ring_badge():
    """Disc inside a ragged ring."""
    disc_radius = 84
    ring_radius = 104
    ring_width = 9
    ring_roughness = 12
    seed = 2

    return (rough_filter("r13", 0.025, 0.16, ring_roughness, seed)
            + f'<g filter="url(#shadow)">'
            + f'<circle cx="{CX}" cy="{CY}" r="{disc_radius}" fill="url(#red)"/>'
            + f'<circle cx="{CX}" cy="{CY}" r="{ring_radius}" fill="none" stroke="#d01010" stroke-width="{ring_width}" filter="url(#r13)"/>'
            + '</g>')


def banner_14_blood_splat():
    """Wide blood splat with droplets."""
    blob_width = 280
    blob_height = 110
    splat_roughness = 45
    splat_frequency = 0.03
    seed = 5
    # (dx, dy, radius) from the cell center
    drops = [(185, -35, 14), (205, 25, 8), (-180, 22, 12), (-200, -35, 7), (75, 85, 9)]

    half_width = blob_width / 2
    half_height = blob_height / 2
    blob = (f"M{CX - half_width},{CY} "
            f"C{CX - half_width},{CY - half_height * 1.1} {CX - half_width * 0.3},{CY - half_height * 1.2} {CX + half_width * 0.05},{CY - half_height * 0.9} "
            f"C{CX + half_width * 0.6},{CY - half_height * 1.2} {CX + half_width * 1.05},{CY - half_height * 0.8} {CX + half_width},{CY} "
            f"C{CX + half_width * 1.05},{CY + half_height * 1.0} {CX + half_width * 0.5},{CY + half_height * 1.1} {CX},{CY + half_height * 0.9} "
            f"C{CX - half_width * 0.5},{CY + half_height * 1.2} {CX - half_width},{CY + half_height * 1.0} {CX - half_width},{CY} Z")
    return (rough_filter("r14", splat_frequency, splat_frequency, splat_roughness, seed)
            + f'<g filter="url(#shadow)"><g filter="url(#r14)">'
            + f'<path d="{blob}" fill="url(#red)"/>'
            + droplets(drops, RED_MIDDLE)
            + '</g></g>')


def banner_15_keycap():
    """Keyboard key cap: dark base, red face, gloss on top."""
    size = 180
    corner_radius = 24
    face_inset = 12
    face_drop = 22
    gloss_height = 56

    x = CX - size / 2
    y = CY - size / 2
    face_width = size - face_inset * 2
    face_height = size - face_drop - 6
    return (f'<g filter="url(#shadow)">'
            + f'<rect x="{x}" y="{y}" width="{size}" height="{size}" rx="{corner_radius}" fill="#7a0303"/>'
            + f'<rect x="{x + face_inset}" y="{y + 4}" width="{face_width}" height="{face_height}" rx="{corner_radius * 0.75}" fill="url(#red)"/>'
            + f'<rect x="{x + face_inset + 8}" y="{y + 10}" width="{face_width - 16}" height="{gloss_height}" rx="{corner_radius * 0.6}" fill="url(#gloss)"/>'
            + '</g>')


def banner_16_double_stroke():
    """Two overlapping brush strokes."""
    length = 420
    height = 50
    offset_x = 28
    offset_y = 26
    back_tilt_degrees = -2
    front_tilt_degrees = 1.5
    edge_roughness = 22

    back_x = CX - length / 2 - offset_x / 2
    back_y = CY - height / 2 - offset_y / 2
    front_x = back_x + offset_x
    front_y = back_y + offset_y
    return (brush_filter("b16a", 0.02, 0.14, edge_roughness, 0.004, 0.55, 2.4, 0.5, 11)
            + brush_filter("b16b", 0.014, 0.11, edge_roughness, 0.003, 0.5, 2.2, 0.55, 3)
            + f'<g filter="url(#shadow)">'
            + f'<rect x="{back_x}" y="{back_y}" width="{length}" height="{height}" fill="{RED_DARK}" filter="url(#b16a)" transform="rotate({back_tilt_degrees} {CX} {back_y + height / 2})"/>'
            + f'<rect x="{front_x}" y="{front_y}" width="{length}" height="{height}" fill="url(#red)" filter="url(#b16b)" transform="rotate({front_tilt_degrees} {CX} {front_y + height / 2})"/>'
            + '</g>')


def banner_17_diamond():
    """Diamond with a highlight on the upper right edge."""
    half_size = 104
    highlight_width = 14
    highlight_opacity = 0.45

    body = [(CX, CY - half_size), (CX + half_size, CY), (CX, CY + half_size), (CX - half_size, CY)]
    highlight = [(CX, CY - half_size + 10), (CX + half_size - 10, CY), (CX + half_size - 10 - highlight_width, CY),
                 (CX, CY - half_size + 10 + highlight_width)]
    return (f'<g filter="url(#shadow)">'
            + f'<polygon points="{points_text(body)}" fill="url(#red)"/>'
            + f'<polygon points="{points_text(highlight)}" fill="{HIGHLIGHT}" opacity="{highlight_opacity}"/>'
            + '</g>')


def banner_18_half_capsule_torn():
    """Round on the left, torn on the right."""
    length = 430
    height = 72
    tear_depth = 18
    gloss_height = 22

    radius = height / 2
    x0 = CX - length / 2 + radius
    x1 = CX + length / 2
    y0 = CY - height / 2
    y1 = CY + height / 2
    path = (f"M{x0},{y0} L{x1},{y0} "
            f"L{x1 - tear_depth * 0.6},{y0 + height * 0.2} L{x1 + tear_depth * 0.4},{y0 + height * 0.37} "
            f"L{x1 - tear_depth * 0.5},{y0 + height * 0.58} L{x1 + tear_depth * 0.3},{y0 + height * 0.79} "
            f"L{x1 - tear_depth * 0.3},{y1} L{x0},{y1} A{radius},{radius} 0 0 1 {x0},{y0} Z")
    return (f'<g filter="url(#shadow)">'
            + f'<path d="{path}" fill="url(#red)"/>'
            + f'<rect x="{x0 - radius * 0.7}" y="{y0 + 5}" width="{length - radius - tear_depth}" height="{gloss_height}" rx="{gloss_height / 2}" fill="url(#gloss)"/>'
            + '</g>')


def banner_19_fading_bar():
    """Brush bar fading out to the right (no shadow)."""
    length = 480
    height = 60
    edge_roughness = 22
    seed = 3

    x = CX - length / 2
    y = CY - height / 2
    return (brush_filter("b19", 0.014, 0.11, edge_roughness, 0.003, 0.5, 2.2, 0.55, seed)
            + f'<rect x="{x}" y="{y}" width="{length}" height="{height}" fill="url(#fade)" filter="url(#b19)"/>')


def banner_20_arrow_tag():
    """Bar with a pointed right end."""
    length = 470
    height = 64
    point_length = 56
    highlight_height = 10
    highlight_opacity = 0.4

    x0 = CX - length / 2
    x1 = CX + length / 2
    y0 = CY - height / 2
    y1 = CY + height / 2
    body = [(x0, y0), (x1 - point_length, y0), (x1, CY), (x1 - point_length, y1), (x0, y1)]
    highlight = [(x0 + 5, y0 + 5), (x1 - point_length - 2, y0 + 5),
                 (x1 - point_length - 2 + point_length * highlight_height * 2 / height, y0 + 5 + highlight_height), (x0 + 5, y0 + 5 + highlight_height)]
    return (f'<g filter="url(#shadow)">'
            + f'<polygon points="{points_text(body)}" fill="url(#red)"/>'
            + f'<polygon points="{points_text(highlight)}" fill="{HIGHLIGHT}" opacity="{highlight_opacity}"/>'
            + '</g>')


# Sheet order: banner N goes to cell N - 1 (reading order). Append new banners at the end; the sheet has 20 cells.
BANNERS = [
    banner_01_long_brush,
    banner_02_slanted_strip,
    banner_03_short_brush,
    banner_04_capsule,
    banner_05_circle,
    banner_06_circle_splat,
    banner_07_rounded_rect,
    banner_08_parallelogram,
    banner_09_tapered_stroke,
    banner_10_dripping_bar,
    banner_11_swallowtail_ribbon,
    banner_12_thin_scratch,
    banner_13_ring_badge,
    banner_14_blood_splat,
    banner_15_keycap,
    banner_16_double_stroke,
    banner_17_diamond,
    banner_18_half_capsule_torn,
    banner_19_fading_bar,
    banner_20_arrow_tag,
]


# ======================================================================
# Sheet building and rendering
# ======================================================================

def build_sheet_svg():
    """Whole sheet as one SVG; each banner in a nested <svg> clipped to its cell, transparent background."""
    if len(BANNERS) > COLUMNS * ROWS:
        raise ValueError(f"{len(BANNERS)} banners don't fit in {COLUMNS} x {ROWS} cells")
    parts = []
    parts.append(f'<svg xmlns="http://www.w3.org/2000/svg" width="{SHEET_WIDTH}" height="{SHEET_HEIGHT}" viewBox="0 0 {SHEET_WIDTH} {SHEET_HEIGHT}">')
    parts.append(shared_defs())
    for index, make_banner in enumerate(BANNERS):
        cell_x = (index % COLUMNS) * CELL_WIDTH
        cell_y = (index // COLUMNS) * CELL_HEIGHT
        parts.append(f'  <!-- {index + 1:02d} {make_banner.__name__} -->')
        parts.append(f'  <svg x="{cell_x}" y="{cell_y}" width="{CELL_WIDTH}" height="{CELL_HEIGHT}" '
                     f'viewBox="0 0 {CELL_WIDTH} {CELL_HEIGHT}" overflow="hidden">{make_banner()}</svg>')
    parts.append('</svg>')
    return "\n".join(parts)


def export_svg(svg_path):
    """Writes the sheet SVG (for inspecting or editing in a vector tool). Not called by main."""
    svg_path = Path(svg_path)
    svg_path.write_text(build_sheet_svg(), encoding="utf-8")
    print(f"[make_key_banners] wrote {svg_path}")


def read_png_size(png_path):
    """(width, height) from the PNG IHDR chunk."""
    with open(png_path, "rb") as png_file:
        header = png_file.read(24)
    if header[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(f"not a PNG: {png_path}")
    width, height = struct.unpack(">II", header[16:24])
    return width, height


def render_png(svg_text, png_path):
    """Renders the SVG with headless Edge (transparent background) into png_path."""
    # Edge's background processes can still hold files in its profile when we clean up; leave those behind
    with tempfile.TemporaryDirectory(prefix="key_banners_", ignore_cleanup_errors=True) as work_dir:
        work_dir = Path(work_dir)
        svg_path = work_dir / "sheet.svg"
        shot_path = work_dir / "sheet.png"
        svg_path.write_text(svg_text, encoding="utf-8")
        command = [
            str(EDGE_EXE),
            "--headless",
            "--disable-gpu",
            "--hide-scrollbars",
            "--default-background-color=00000000",
            "--force-device-scale-factor=1",
            f"--user-data-dir={work_dir / 'edge_profile'}",
            f"--window-size={SHEET_WIDTH},{SHEET_HEIGHT}",
            f"--screenshot={shot_path}",
            svg_path.as_uri(),
        ]
        print("[make_key_banners] rendering with Edge")
        subprocess.run(command, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        # Edge may return before the file is flushed
        deadline = time.time() + RENDER_TIMEOUT
        while not shot_path.exists():
            if time.time() > deadline:
                raise TimeoutError("Edge did not write the screenshot")
            time.sleep(0.2)
        size = read_png_size(shot_path)
        if size != (SHEET_WIDTH, SHEET_HEIGHT):
            raise ValueError(f"rendered size {size}, expected {(SHEET_WIDTH, SHEET_HEIGHT)}")
        png_path.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(shot_path, png_path)
    print(f"[make_key_banners] wrote {png_path} ({SHEET_WIDTH}x{SHEET_HEIGHT}, {len(BANNERS)} banners)")


def main():
    render_png(build_sheet_svg(), OUTPUT_PNG)


if __name__ == "__main__":
    main()
