# Contract Killer: Zombies 2 Reverse

[English](README.md) | **中文**

## 概述

本项目在 iOS 版代码的基础上进行 PC 端重建。

游戏源码来自对 iOS 2.0.2 版 `ckzombies2` 的 1:1 还原，见 [iOS 原版还原（`ori-IOS` 分支）](https://github.com/Feiyang0102sso/contract-killer-zombie-2-re/tree/ori-IOS)。原版是 Unity 3.5.7，PC 版迁移到 **Unity 6.3 LTS（6000.3.28f1）**，按 Windows x64 构建。

## 目录

| 位置 | 内容 |
| --- | --- |
| `Assets/` | 从原版导出的素材（场景、预制体、贴图、音频、本地化文本等），只放素材 |
| `Source/` | 代码，本地包 `com.ckz2.game`（程序集 `CKZ2.Game` / `CKZ2.Game.Editor`） |
| `Source/Runtime/Scripts/`、`Packages/` | 还原出的游戏代码和 Glu 公共包 |
| `Source/Runtime/Port/` | 移植层，见下文 |
| `Source/Editor/` | 构建脚本 `PcBuild`、HUD 布局工具 |
| `Launcher/` | 原生启动器（C++，VS 工程），替换 Unity 自带的 `ckz2.exe` |
| `tools/` | 素材转换、中文字库、美术和测试脚本，见 [`tools/README.md`](tools/README.md) |

## 移植层

游戏代码原样编译，与原版不同的部分都在 [`Source/Runtime/Port/`](Source/Runtime/Port/) 里：

| 目录 | 作用 |
| --- | --- |
| `NewUnityPort/` | Unity 3.5 → 6 的行为差异：`GameObject.active` 语义、旧版粒子、物理与布娃娃关节、光照探针与光照贴图、2D 音效、XML 序列化等。与平台无关，以后移植其他平台可复用 |
| `NonIosPort/` | 替换 iOS 原生接口（`DllImport("__Internal")`、推送通知等） |
| `Offline/` | 联网、社交平台、统计改为离线空实现，Unity Analytics 也在代码里关掉 |
| `PcPort/` | PC 专用：鼠标键盘输入、按键提示、光标、帧率换算、启动参数、日志与存档路径、片头视频、极致画质 |
| `Addon/` | 原版没有的新增内容，目前是 The Pit 地图 |
| `Dev/` | 启动诊断、作弊码、编辑器保护 |

## Windows x64 构建

### 工具链

| 组件 | 版本 | 说明 |
| --- | --- | --- |
| Unity | 6000.3.28f1 | 版本以 `ProjectSettings/ProjectVersion.txt` 为准 |
| Visual Studio | 2022 或 2026 | 勾选 C++ 桌面开发；用于编译启动器 |

### 构建脚本

先关闭 Unity 编辑器，再双击根目录的脚本：

| 脚本 | 产物 | 说明 |
| --- | --- | --- |
| `build_dev.bat` | `Builds/Dev/ckz2.exe` | 开发版，带 Unity 的 Development 标志（可挂调试器，画面右下角有 Development Build 水印） |
| `build_release.bat` | `Builds/Release/ckz2.exe` | 发布版，代码与开发版相同，只去掉 Development 标志 |

## CFG配置

设置写在 EXE 同目录的 `ckz2.cfg`

| 键 | 默认 | 作用 |
| --- | --- | --- |
| `[graph] ScreenX` / `ScreenY` | 桌面分辨率 | 窗口尺寸，只在窗口模式下生效 |
| `[graph] FullScreen` | `1` | `0` 窗口，`1` 按桌面分辨率无边框全屏 |
| `[graph] VSync` | `1` | 垂直同步；为 `0` 时不限帧 |
| `[graph] GraphDetail` | `4` | 画质：`0` Lowest、`1` Low、`2` Medium、`3` High、`4` Ultra、`5` Extreme（PC 新增，高于原版各档） |
| `[game] StartDialog` | `1` | 是否显示启动窗口。关掉后只能改 cfg 找回 |
| `[game] DrawFPS` | `0` | 在画面左下角显示帧率 |
| `[game] Language` | `0` | `0` 英文，`1` 简体中文 |

## 画质选项

原版按 iOS 设备型号自动选择画质预设（`QualitySetter`），PC 版改为由 `GraphDetail` 指定，各档直接沿用原版预设：

| 档位 | 原版预设 | 原版对应 iOS 设备 | 主要内容 |
| --- | --- | --- | --- |
| `0` Lowest | `iPod4` | iPod touch 4 | 贴图降到 1/4 分辨率；同屏僵尸上限 6；关掉场景杂物、血迹、弹孔、布娃娃、体积光、复杂阴影等；低音质；关卡内容精简 |
| `1` Low | `iPhone4` | iPhone 4、iPad 1 | 全分辨率贴图；其余与 Lowest 基本相同，音质正常 |
| `2` Medium | `AndroidMedium` | 无（原版安卓中档） | 场景细节全开，同屏僵尸上限 10；有泛光和色差，但没有低血量效果，无抗锯齿 |
| `3` High | `iPhone4S` | iPhone 4S、iPad 2、iPod touch 5 | 效果全开，4× MSAA |
| `4` Ultra | `iPad3` | iPad 3、iPhone 5 | 最高着色器 LOD 与贴图档，8× MSAA |
| `5` Extreme | `iPad3` + PC 增强 | 无（PC 新增） | 在 Ultra 基础上：16× 各向异性过滤；后处理用 32 位带 MSAA 的渲染纹理（原版 RGB565、无抗锯齿）；血迹和场景杂物的显示距离从 25 m 加到 100 m |

## 键位配置

| 输入 | 操作 |
| --- | --- |
| `W` `A` `S` `D` | 移动 |
| `Left Shift` | 奔跑 |
| 移动鼠标 | 转动视角 |
| 鼠标左键 | 开火，按住连发；开镜时每次点击一发 |
| 鼠标右键 | 开镜 / 关镜 |
| `R` | 装弹 |
| `Space` | 打开 / 关闭武器轮盘 |
| `1` – `6` | 近战 / 突击 / 特殊 / 手枪 / 狙击 / 霰弹 |
| `Q` / `E` | 上一个 / 下一个道具 |
| `U` | 使用道具 |
| `P` | 拾取（弹药箱、奖励箱、任务物品） |
| `Esc` | 暂停 |

## 作弊码

游戏中直接键入即可，不需要输入框：

| 作弊码 | 效果 |
| --- | --- |
| `chxp` | 等级 +1 |

另外，PC 版开局的现金和黄金各为 999,999。

## 更新记录

### 1.0.0

- init release

## 免责声明

- 本项目与 Glu Mobile 及其关联方没有任何关系，也未获得其授权或认可。
- Contract Killer 系列的名称、商标、美术、音频、关卡等游戏内容，以及原版程序，版权均归其权利人所有。
- 本仓库包含恢复出的源码、构建脚本，以及**从原版导出的游戏素材**，仅供研究对照使用。
- 本项目仅用于学习、研究和保存目的，不得用于商业用途。
- 代码按“现状”提供，不提供任何形式的担保；使用风险自负。
