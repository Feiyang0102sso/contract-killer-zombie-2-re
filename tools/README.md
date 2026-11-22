# tools

- `font/`: Chinese bitmap font, shared by every port.
- `port_new_unity/`: moving the Unity 3.5 game to Unity 6, platform-independent.
- `port_pc/`: things only the PC (Windows) port needs.

## font/

| File | What |
| --- | --- |
| `make_cn_font.py` | Rebuilds `Assets/ui/fonts/ChineseSimplified.txt` + `_0.png` from the characters in `zh-Hans.txt` |
| `DroidSansFallback.ttf`, `ChineseSimplified_preserved/`, `ChineseSimplified_kerning.txt` | Its inputs, see `font/README.md` |

## port_new_unity/

| File | What |
| --- | --- |
| `asset_convert/gles_to_hlsl.py` | GLES shaders → CG/HLSL |
| `asset_convert/scene_lighting_extract.py` | Lightmaps and light probes → `data/lighting/` |
| `asset_convert/navmesh_extract.py` | Original Detour navmeshes → `data/navmesh/` |
| `asset_convert/legacy_particles_extract.py` | Legacy particles → `data/particles/` |
| `asset_convert/legacy_particles_size_fix.py` | SizeOverLifetime curves written into scene/prefab YAML |
| `asset_convert/audio_2d_extract.py` | 2D sound table → `Source/Runtime/PcPort/PcAudio.Generated.cs` |
| `asset_convert/data/` | JSON written by the scripts above, read by `unity_editor/` |
| `asset_convert/unity_editor/` | Optional package `com.ckz2.port-new-unity`, see below |
| `cs_migrate/replace_scripts.py` | One-off: decompiled scripts replaced by the restored source, GUIDs kept |
| `cs_migrate/api_fixes.py` | One-off: API changes the Unity updater cannot make |
| `cs_migrate/nullable_xml_fix.py` | One-off: `[XmlAttribute]` on nullable fields replaced by string proxies |

`unity_editor/` holds the Unity half of `asset_convert/`: `NewUnityLegacyParticleConvert`, `NewUnityNavMeshRebake`,
`NewUnitySceneLightingRestore`, `NewUnityCharacterControllerFix`, `NewUnityShaderCheck`. It is not in
`Packages/manifest.json`, so it is not compiled. To run one, add
`"com.ckz2.port-new-unity": "file:../tools/port_new_unity/asset_convert/unity_editor"` to the manifest, call it with
`-batchmode -executeMethod <Class>.<Method>`, then revert `manifest.json` and `packages-lock.json`.

## port_pc/

| File | What |
| --- | --- |
| `art/make_cursor.py` | Mouse cursor `PcCursor{32,48,64}.png` |
| `art/make_exit_button.py` | City map exit button `PcExitButton.png` |
| `art/make_key_banners.py` | Key-hint banner sheet `PcKeyBanners.png` |
| `art/make_pit_icons.py` | The Pit map icon `PcPitIcon.png` (candidates in `out/pit_icons/`) |
| `asset_convert/convert_intro_movie.py` | Intro movie `.mov` → `Scene_01.mp4` |
| `cs_migrate/bridge_externs.py` | One-off: `DllImport("__Internal")` externs forwarded to `PcNative` |
| `test/run_and_capture.ps1` | Starts the game with test arguments, takes screenshots, stops it |
| `test/scene_tour.sh` | Runs `run_and_capture.ps1` once per level and summarizes the logs |
