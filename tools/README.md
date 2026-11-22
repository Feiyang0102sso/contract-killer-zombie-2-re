# tools

Scripts that are not part of the game build, grouped by which port they serve:

- `port_new_unity/`: moving the Unity 3.5 game to Unity 6. Platform-independent, so another port can reuse it.
- `port_pc/`: things only the PC (Windows) port needs.
- `font/`: the Chinese bitmap font, used by every port.

Run Python with `../.venv/Scripts/python.exe` from `ckz2_repo_pc/`; each script's docstring has its exact usage.
Code in `Source/` mentions these scripts by file name only, so moving a script never touches `Source/`.

| Folder | What | When |
| --- | --- | --- |
| `font/` | Simplified Chinese bitmap font, plus its input data | After every change to `Assets/localization/zh-Hans.txt` |
| `port_new_unity/asset_convert/` | Rebuilds data that Unity 6 drops or cannot read, from the untouched export | Rarely: only when a conversion rule changes |
| `port_new_unity/cs_migrate/` | One-off C# edits that moved the code to Unity 6 | Never again: already applied, kept as the record of what changed |
| `port_pc/art/` | Art the original does not have, drawn for the PC port | When tuning one of these images |
| `port_pc/asset_convert/` | Converts original assets into formats Windows can play | Rarely |
| `port_pc/cs_migrate/` | One-off C# edit for the PC platform | Never again (same as above) |
| `port_pc/test/` | Launches the built game, takes screenshots, keeps logs | Automated play tests |
| `out/` | Preview sheets written by `port_pc/art/` (git-ignored) | — |

## font/

| Script | Output |
| --- | --- |
| `make_cn_font.py` | `Assets/ui/fonts/ChineseSimplified.txt` + `_0.png`, pixel-identical to the original glyphs. Windows only (GDI). Inputs are described in `font/README.md`. |

## port_new_unity/asset_convert/

Two halves: Python reads the untouched ULegacy export (`../unity_assets/ULegacy-Ripper_0.0.6`, outside the repo) and
writes JSON into `data/`; the Unity editor scripts in `unity_editor/` read that JSON and write it into scenes and prefabs.

| Python | Output | Unity half |
| --- | --- | --- |
| `gles_to_hlsl.py` | GLES shaders translated to CG/HLSL, in place in `Assets/` | `NewUnityShaderCheck` (checks the result) |
| `scene_lighting_extract.py` | `data/lighting/<scene>.json` (lightmaps, light probes) | `NewUnitySceneLightingRestore` |
| `navmesh_extract.py` | `data/navmesh/<guid>.json` (original Detour polygons) | `NewUnityNavMeshRebake` |
| `legacy_particles_extract.py` | `data/particles/legacy_particles.json` | `NewUnityLegacyParticleConvert` |
| `legacy_particles_size_fix.py` | SizeOverLifetime curves, written straight into scene/prefab YAML | — (mirrors `NewUnityLegacyParticleConvert.ApplySizeGrow`) |
| `audio_2d_extract.py` | `Source/Runtime/PcPort/PcAudio.Generated.cs` (sounds imported as 2D) | — |
| — | — | `NewUnityCharacterControllerFix` (stepOffset over Unity 6's limit) |

### unity_editor/: optional package

`unity_editor/` is a local package (`com.ckz2.port-new-unity`, assembly `CKZ2.PortNewUnity.Editor`) that is **not** in
the manifest, so it is not compiled and adds no menus day to day. Its results are already saved in the scenes and
prefabs; enable it only to run one of them again:

1. Add to `Packages/manifest.json`, under `dependencies`:
   `"com.ckz2.port-new-unity": "file:../tools/port_new_unity/asset_convert/unity_editor",`
2. Run the step in batch mode (close the editor first), e.g.
   `Unity.exe -batchmode -quit -projectPath . -executeMethod NewUnityNavMeshRebake.RebakeAll -logFile RunLogs/rebake.log`
   Entry points: `NewUnityLegacyParticleConvert.ConvertAll`, `NewUnityNavMeshRebake.RebakeAll` / `VerifyAll`,
   `NewUnitySceneLightingRestore.RestoreAll`, `NewUnityCharacterControllerFix.Run`, `NewUnityShaderCheck.Run`.
3. Remove the line again (Unity also updates `Packages/packages-lock.json`; revert that too).

Because it is not compiled day to day, a change in `Source/Runtime` can break it unnoticed; fix it when you enable it.

## port_new_unity/cs_migrate/

Applied once to the code now in `Source/`. Running them again fails or does nothing: the text they look for is gone.

| Script | What it changed |
| --- | --- |
| `replace_scripts.py` | Replaced the decompiled scripts with the restored source, keeping their GUIDs |
| `api_fixes.py` | API changes the Unity updater cannot make, one exact replacement each, with the reason |
| `nullable_xml_fix.py` | `[XmlAttribute]` on nullable fields replaced by string proxy properties |

## port_pc/

| Script | What |
| --- | --- |
| `art/make_cursor.py` | Mouse cursor `Assets/Resources/PcPort/PcCursor{32,48,64}.png` |
| `art/make_exit_button.py` | City map exit button `PcPort/PcExitButton.png`, built from the stock button frame |
| `art/make_key_banners.py` | Key-hint banner sheet `PcPort/PcKeyBanners.png` (SVG rendered by headless Edge) |
| `art/make_pit_icons.py` | The Pit map icon candidates in `out/pit_icons/`; the chosen one goes to `PcPort/PcPitIcon.png` |
| `asset_convert/convert_intro_movie.py` | `Assets/Resources/Movies/Scene_01.mp4` from the original `.mov` (needs ffmpeg) |
| `cs_migrate/bridge_externs.py` | One-off: `DllImport("__Internal")` externs forwarded to `PcNative` outside iOS |
| `test/run_and_capture.ps1` | Starts the game with `PcBootDiagnostics` test arguments (screenshots, auto taps, mission jump, ...) and stops it afterwards |
| `test/scene_tour.sh` | Runs `run_and_capture.ps1` once per level from the same save backup and summarizes errors and broken paths |

The pilot log behind many of these fixes (the "day fixes" sections that comments refer to) is
`../pc_pilot_tools/README.md`, outside this repo.
