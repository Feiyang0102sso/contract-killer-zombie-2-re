# tools

Scripts for the PC port, grouped by when you run them. Run Python with `../.venv/Scripts/python.exe` from
`ckz2_repo_pc/`; each script's docstring has its exact usage.

| Folder | What | When |
| --- | --- | --- |
| `font/` | Simplified Chinese bitmap font, plus its input data | After every change to `Assets/localization/zh-Hans.txt` |
| `art/` | Art the original does not have, drawn for the PC port | When tuning one of these images |
| `portNewUnity/asset_convert/` | Rebuilds data that Unity 6 drops or cannot read, from the untouched export or the original `.app` | Rarely: only when a conversion rule changes |
| `portNewUnity/cs_migrate/` | One-off C# edits that moved the code to Unity 6 | Never again: already applied, kept as the record of what changed |
| `test/` | Launches the built game, takes screenshots, keeps logs | Automated play tests |
| `out/` | Preview sheets written by `art/` (git-ignored) | — |

## font/

| Script | Output |
| --- | --- |
| `make_cn_font.py` | `Assets/ui/fonts/ChineseSimplified.txt` + `_0.png`, pixel-identical to the original glyphs. Windows only (GDI). Inputs are described in `font/README.md`. |

## art/

| Script | Output |
| --- | --- |
| `make_cursor.py` | Mouse cursor `Assets/Resources/PcPort/PcCursor{32,48,64}.png` |
| `make_exit_button.py` | City map exit button `PcPort/PcExitButton.png`, built from the stock button frame |
| `make_key_banners.py` | Key-hint banner sheet `PcPort/PcKeyBanners.png` (SVG rendered by headless Edge) |
| `make_pit_icons.py` | The Pit map icon candidates in `out/pit_icons/`; the chosen one goes to `PcPort/PcPitIcon.png` |

## portNewUnity/asset_convert/

Most of these read the untouched ULegacy export (`../unity_assets/ULegacy-Ripper_0.0.6`), which is outside the repo.

| Script | Output | Used by |
| --- | --- | --- |
| `gles_to_hlsl.py` | GLES shaders translated to CG/HLSL, in place in `Assets/` | Unity directly |
| `scene_lighting_extract.py` | `pc_data/lighting/<scene>.json` (lightmaps, light probes) | `PcSceneLightingRestore` |
| `navmesh_extract.py` | `pc_data/navmesh/<guid>.json` (original Detour polygons) | `PcNavMeshRebake` |
| `legacy_particles_extract.py` | `pc_data/particles/legacy_particles.json` | `PcLegacyParticleConvert` |
| `legacy_particles_size_fix.py` | SizeOverLifetime curves, written straight into scene/prefab YAML | Unity directly |
| `audio_2d_extract.py` | `Source/Runtime/PcPort/PcAudio.Generated.cs` (sounds imported as 2D) | `PcAudio` |
| `convert_intro_movie.py` | `Assets/Resources/Movies/Scene_01.mp4` (from `Payload-ori/`, needs ffmpeg) | `MoviePlayerPc` |

## portNewUnity/cs_migrate/

Applied once to the code now in `Source/`. Running them again fails or does nothing: the text they look for is gone.

| Script | What it changed |
| --- | --- |
| `replace_scripts.py` | Replaced the decompiled scripts with the restored source, keeping their GUIDs |
| `api_fixes.py` | API changes the Unity updater cannot make, one exact replacement each, with the reason |
| `bridge_externs.py` | `DllImport("__Internal")` externs forwarded to `PcNative` outside iOS |
| `nullable_xml_fix.py` | `[XmlAttribute]` on nullable fields replaced by string proxy properties |

## test/

| Script | What |
| --- | --- |
| `run_and_capture.ps1` | Starts the game with `PcBootDiagnostics` test arguments (screenshots, auto taps, mission jump, ...) and stops it afterwards |
| `scene_tour.sh` | Runs `run_and_capture.ps1` once per level from the same save backup and summarizes errors and broken paths |

The pilot log behind many of these fixes (the "day fixes" sections that comments refer to) is
`../pc_pilot_tools/README.md`, outside this repo.
