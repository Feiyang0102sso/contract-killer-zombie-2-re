"""Converts the original intro movie (Data/Raw/Movies/Scene_01.mov) into an MP4 the Windows video player can read.

The original is H.264 Main 1080p with 8-bit unsigned PCM audio (22050 Hz mono) in a QuickTime container.
Windows Media Foundation, which Unity's VideoPlayer uses on Windows, reliably reads H.264 + AAC in MP4,
so the video stream is copied unchanged (no re-encode) and only the audio is converted to AAC.

Output: Assets/Resources/Movies/Scene_01.mp4, loaded by MoviePlayerPc as Resources "Movies/Scene_01".

Usage (from ckz2_repo_pc):
    ../.venv/Scripts/python.exe tools/port_pc/asset_convert/convert_intro_movie.py [path to ffmpeg.exe]
"""
import subprocess
import sys
from pathlib import Path

# ---- Paths ----
# tools/port_pc/<category>/<script>.py -> project root
PROJECT_DIR = Path(__file__).resolve().parents[3]
SOURCE_MOVIE = PROJECT_DIR.parent / "Payload-ori" / "ckzombies2.app" / "Data" / "Raw" / "Movies" / "Scene_01.mov"
OUTPUT_MOVIE = PROJECT_DIR / "Assets" / "Resources" / "Movies" / "Scene_01.mp4"
DEFAULT_FFMPEG = Path(r"E:\ffmpeg-6.1.1-full_build\bin\ffmpeg.exe")

# ---- Audio ----
AUDIO_BITRATE = "96k"


def main():
    ffmpeg = DEFAULT_FFMPEG
    if len(sys.argv) > 1:
        ffmpeg = Path(sys.argv[1])
    OUTPUT_MOVIE.parent.mkdir(parents=True, exist_ok=True)
    command = [
        str(ffmpeg),
        "-y",
        "-i", str(SOURCE_MOVIE),
        # Video and audio only: the original also has a QuickTime data track the player does not need
        "-map", "0:v:0",
        "-map", "0:a:0",
        "-c:v", "copy",
        "-c:a", "aac",
        "-b:a", AUDIO_BITRATE,
        # Index at the front so the player can start without reading the whole file
        "-movflags", "+faststart",
        # The original video carries a timecode tag; without this the muxer adds it back as a data track
        "-write_tmcd", "0",
        str(OUTPUT_MOVIE),
    ]
    print("Running: " + " ".join(command))
    subprocess.run(command, check=True)
    print(f"Movie: {OUTPUT_MOVIE} ({OUTPUT_MOVIE.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
