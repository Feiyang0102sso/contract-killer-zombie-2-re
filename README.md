# Contract Killer: Zombies 2 Reverse

**English** | [中文](README_CN.md)

## Overview

This project rebuilds the game for PC on top of the iOS code.

The game source is a 1:1 restoration of `ckzombies2` for iOS 2.0.2, see [the iOS restoration (`ori-IOS` branch)](https://github.com/Feiyang0102sso/contract-killer-zombie-2-re/tree/ori-IOS). The original runs on Unity 3.5.7; the PC version is ported to **Unity 6.3 LTS (6000.3.28f1)** and built for Windows x64.

## Layout

| Path | Contents |
| --- | --- |
| `Assets/` | Assets exported from the original game (scenes, prefabs, textures, audio, localization text, ...), assets only |
| `Source/` | Code, local package `com.ckz2.game` (assemblies `CKZ2.Game` / `CKZ2.Game.Editor`) |
| `Source/Runtime/Scripts/`, `Packages/` | Restored game code and Glu's shared packages |
| `Source/Runtime/Port/` | Porting layer, see below |
| `Source/Editor/` | Build script `PcBuild`, HUD layout tool |
| `Launcher/` | Native launcher (C++, VS project) that replaces Unity's own `ckz2.exe` |
| `tools/` | Asset conversion, Chinese font, art and test scripts, see [`tools/README.md`](tools/README.md) |

## Porting layer

The game code is compiled as is. Everything that differs from the original lives in [`Source/Runtime/Port/`](Source/Runtime/Port/):

| Folder | Purpose |
| --- | --- |
| `NewUnityPort/` | Unity 3.5 → 6 behavior differences: `GameObject.active` semantics, legacy particles, physics and ragdoll joints, light probes and lightmaps, 2D sounds, XML serialization, ... Platform-independent, reusable for future ports |
| `NonIosPort/` | Replaces iOS native interfaces (`DllImport("__Internal")`, push notifications, ...) |
| `Offline/` | Networking, social platforms and telemetry become offline stubs; Unity Analytics is also turned off in code |
| `PcPort/` | PC only: mouse and keyboard input, key hints, cursor, frame-rate scaling, launch arguments, log and save paths, intro movie, Extreme quality |
| `Addon/` | Content the original doesn't have; currently The Pit map |
| `Dev/` | Boot diagnostics, cheat codes, editor safeguards |

## Windows x64 build

### Toolchain

| Component | Version | Notes |
| --- | --- | --- |
| Unity | 6000.3.28f1 | `ProjectSettings/ProjectVersion.txt` is authoritative |
| Visual Studio | 2022 or 2026 | With "Desktop development with C++"; builds the launcher |

### Build scripts

Close the Unity editor first, then double-click a script in the root folder:

| Script | Output | Notes |
| --- | --- | --- |
| `build_dev.bat` | `Builds/Dev/ckz2.exe` | Development build with Unity's Development flag (debugger attachable, "Development Build" watermark in the bottom-right corner) |
| `build_release.bat` | `Builds/Release/ckz2.exe` | Release build, same code as the dev build, only without the Development flag |

## CFG

Settings live in `ckz2.cfg` next to the EXE.

| Key | Default | Effect |
| --- | --- | --- |
| `[graph] ScreenX` / `ScreenY` | Desktop resolution | Window size, windowed mode only |
| `[graph] FullScreen` | `1` | `0` windowed, `1` borderless fullscreen at desktop resolution |
| `[graph] VSync` | `1` | Vertical sync; no FPS cap when `0` |
| `[graph] GraphDetail` | `4` | Quality: `0` Lowest, `1` Low, `2` Medium, `3` High, `4` Ultra, `5` Extreme (PC only, above every original preset) |
| `[game] StartDialog` | `1` | Show the start dialog. Once turned off, only editing the cfg brings it back |
| `[game] DrawFPS` | `0` | FPS counter in the bottom-left corner |
| `[game] Language` | `0` | `0` English, `1` Simplified Chinese |

## Quality levels

The original picks a quality preset from the iOS device model (`QualitySetter`). The PC version takes it from `GraphDetail` instead, and each level reuses an original preset:

| Level | Original preset | Original iOS devices | Contents |
| --- | --- | --- | --- |
| `0` Lowest | `iPod4` | iPod touch 4 | Textures at 1/4 resolution; at most 6 zombies on screen; scene clutter, blood, bullet holes, ragdolls, volumetric light, complex shadows etc. off; low-quality audio; trimmed level content |
| `1` Low | `iPhone4` | iPhone 4, iPad 1 | Full-resolution textures; otherwise much like Lowest, with normal audio |
| `2` Medium | `AndroidMedium` | None (original Android medium) | All scene detail on, at most 10 zombies on screen; bloom and chromatic aberration, but no low-health effect and no anti-aliasing |
| `3` High | `iPhone4S` | iPhone 4S, iPad 2, iPod touch 5 | All effects on, 4× MSAA |
| `4` Ultra | `iPad3` | iPad 3, iPhone 5 | Highest shader LOD and texture tier, 8× MSAA |
| `5` Extreme | `iPad3` + PC upgrades | None (PC only) | On top of Ultra: 16× anisotropic filtering; post effects render to a 32-bit MSAA render texture (original: RGB565, no anti-aliasing); blood and scene clutter stay visible up to 100 m instead of 25 m |

## Controls

| Input | Action |
| --- | --- |
| `W` `A` `S` `D` | Move |
| `Left Shift` | Run |
| Mouse movement | Look around |
| Left mouse button | Fire, hold for automatic fire; one shot per click while zoomed |
| Right mouse button | Zoom in / out |
| `R` | Reload |
| `Space` | Open / close the weapon wheel |
| `1` – `6` | Melee / Assault / Exotic / Handgun / Sniper / Shotgun |
| `Q` / `E` | Previous / next item |
| `U` | Use item |
| `P` | Pick up (ammo crates, bonus boxes, mission items) |
| `Esc` | Pause |

## Cheat codes

Type them anywhere in the game, no text field needed:

| Code | Effect |
| --- | --- |
| `chxp` | Level +1 |

Also, the PC version starts with 999,999 cash and 999,999 gold.

## Changelog

### 1.0.0

- init release

## Disclaimer

- This project is not affiliated with, authorized or endorsed by Glu Mobile or its affiliates.
- The Contract Killer names, trademarks, art, audio, levels and other game content, as well as the original program, belong to their respective owners.
- This repository contains restored source code, build scripts and **game assets exported from the original game**, for research and comparison only.
- This project is for learning, research and preservation only, and must not be used commercially.
- The code is provided "as is", without warranty of any kind; use at your own risk.
