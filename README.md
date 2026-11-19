# CKZ2 还原仓库

《Contract Killer: Zombies 2》iOS 2.0.2 的源码还原。`Assets/` 照原版 Unity 工程的路径摆放，编译出的 `Assembly-CSharp.dll` 以与原版逐字节一致为目标。

## 目录

| 目录 | 内容 | 是否原版就有 |
| --- | --- | --- |
| `Assets/` | 还原的源码，路径照原版工程：`Packages/`（Glu 公共包和 EZ GUI）、`Scripts/`（游戏代码）、`Plugins/iOS/`（已还原的 14 个 Glu 原生插件，加上 Glu 改过的模板文件 `AppController.mm`；Unity 3.5.7 只取这里的顶层文件，所以平铺放，见 `docs/native.md`）、`3rd_party/LitJSON.dll`（第三方托管库原件，目录名是推断） | 是，只放原版就有的文件 |
| `native_stubs/` | 广告、统计、联网类 11 个原生插件的同名空实现，只进 tuned 产物，见 `native_stubs/README.md` | 否 |
| `data/` | 要改的 MonoBehaviour 数据导出的 JSON，文件名用原版资源路径；内容和原版一致，不在这里改 | 否 |
| `tweaks/` | 数值调整（“防坐牢”），只写改了哪些字段、改成多少 | 否 |
| `tools/` | 构建、比对、数据导入导出脚本 | 否 |
| `docs/` | 进度页；`native.md`（原生部分的来源、构建与验证）；`link.md`（完整主程序的链接与逐函数比对） | 否 |
| `recovery/` | 本地工作区：`file_map.csv`、`notes/`、账本。不入库 | 否 |
| `build/` | 构建产物：`vanilla/` 与原版一致，`tuned/` 叠加了 `tweaks/`。不入库 | 否 |
| `bin/` | 最终产物：`ckzombies2` 和 `Data/Managed/` 下的 8 个程序集，结构与 `ckzombies2.app` 相同，整个覆盖进 `.app` 即可。主程序已带 ad-hoc 签名（entitlements 取自原版，越狱设备可直接运行）。`build_main.py` 最后一步生成，不入库 | 否 |

原版文件在上一级的 `Payload-ori/`，不复制进仓库。

## 源码路径从哪来

原版没有留下源码目录，路径按下面的证据推出来，每个类型的依据记在 `recovery/file_map.csv`（`tools/file_map.py` 生成）：

| 证据 | 含义 | 类型数 |
| --- | --- | --- |
| `po` | 本地化 PO 注释直接给出完整路径 | 63 |
| `bracketed` | MonoBehaviour 子类（文件名 = 类名），被同目录的两个锚点夹住，且与相邻的确定路径不冲突 | 64 |
| `class_name` | MonoBehaviour 子类，文件名确定，目录只知道范围 | 302 |
| `unknown` | 普通类、结构、枚举、接口，可能和别的类型同文件 | 255 |
| `delegate` | smcs 把委托排在最后，顺序不反映位置 | 20 |
| `generated` | `<Module>`、`<PrivateImplementationDetails>` | 2 |

**目录名会影响校验**：Unity 按路径字母序把文件交给 smcs，类型顺序跟着路径走。推断的目录名必须和原版排出同样的顺序，否则 DLL 对不上。反过来，只要排序相同，用的名字对不对，比对是查不出来的，所以证据等级要如实记录。

### 目录表与证据等级

`tools/layout.csv` 按 TypeDef 序号区间给每段类型指定目录，少数类型单独指定文件。`tools/import_decompiled.py` 照这张表把反编译源码导入成 579 个文件，每个类型的放置结果记在 `recovery/placement.csv`。每个类型的证据等级如下：

| 等级 | 类型数 | 含义 |
| --- | --- | --- |
| `po` | 63 | 完整路径来自 PO 注释 |
| `bracketed` | 64 | MonoBehaviour，被同目录的两个锚点夹住 |
| `anchor` | 136 | 目录是锚点给出的已知目录，文件名按类名规则或排序推定 |
| `evidence` | 1 | `Packages/ABTesting/Examples/`，DLL 里有这个路径字符串 |
| `package` | 276 | 包目录 `Packages/<包名>/` 取自日志名 `Package.<包名>` 和原版顺序；包内子目录（`Examples`、`Scripts`、`Template` 等）和 EZ GUI 的内部结构是猜的 |
| `guess` | 164 | 目录名是猜的（如 `Scripts/Camera`、`Scripts/Utils`），含 20 个委托的宿主文件 |

猜测的部分只保证排序正确：编译出的类型顺序和原版一致（逐方法比对结果与导入前完全相同），名字是否和原版一样无法验证。

几个不是一类型一文件的情况：
- 日志类、接口、枚举等放不进自己的文件时，并入下一个文件的开头（`attached_forward`，26 个）。
- 其余放不进的，追加到上一个文件（`attached_back`，16 个）。
- `Tutorial/TutorialScripts.cs` 是 PO 锚点里的多类型文件，按顺序放进 17 个教程类。

## 工具

| 命令 | 作用 |
| --- | --- |
| `python tools/build_dll.py [--windows]` | 按 Unity 的规则收集 `Assets/` 下的 .cs，按路径排序，用 smcs 2.6.5 和 Unity 原始参数编译到 `build/vanilla/`。默认在 WSL `ckre` 里用 Unity 的 Mac 版 Mono 编（`toolchain/smcs_mac.sh`，约 25 秒），产物除 MVID 和时间戳外与原版逐字节相同，门禁用它；`--windows` 用 Windows 版 mono（约 1 秒），方法 IL 相同，但 mscorlib 版本和同一目标上多个特性的排列不同，只适合快速试写法 |
| `python tools/il_compare.py [dll]` | 和原版逐方法比对 IL，报告写到 `recovery/il_compare/`（`methods.csv`、`diffs.txt`） |
| `python tools/gate.py <轮名> [--advance]` | 正式门禁：编译 + 比对 + 回退锁（`recovery/gate/lock.json`）+ 每轮报告 `recovery/rounds/` + 刷新饼图 |
| `python tools/il_triage.py [dll]` | 差异分诊：每个方法归类（编号错位、局部变量、写法不同……），并找出编译器生成名编号从哪个类型开始错位 |
| `python tools/status_page.py` | 生成饼图页 `recovery/status/progress.cn.html`：上面是 IL 逐方法比对，下面是主程序逐函数比对（有 `recovery/link/summary.json` 时） |
| `python tools/build_main.py [--skip-dll]` | 链出完整主程序 `build/tuned/ckzombies2`：编 DLL → 回填 MVID 并核对与原版逐字节相同 → 生成 `RegisterMonoModules.cpp`（`tools/register_mono_modules/`）→ WSL 里 `toolchain/build_main.sh` 做 AOT、原生编译、链接 → 签名并复制到 `bin/`。见 `docs/link.md` |
| `python tools/sign_adhoc.py <输入> <输出>` | 给主程序加 ad-hoc 签名，identifier 和 entitlements 取自原版；不签名就没有钥匙串权限，启动会弹“无法连接服务器”。`build_main.py` 自动调用 |
| `python tools/link_compare.py [产物]` | 链接产物与原版逐函数比对（L1，地址相关字段按指向的东西比，带比较器自测），报告写到 `recovery/link/` |
| `python tools/link_datadiff.py [产物]` | 数据比对（L2）：代码以外的所有节，指针按指向的东西比，只在原版里有的顺着指针归属到 SDK / 胶水；依赖 L1 的 `functions.csv` |
| `python tools/link_layout.py [产物]` | 结构核对（L3）：Mach-O 头、加载命令、段与节、dylib、入口、导入、符号可见性、函数与数据的排列顺序 |
| `python tools/native_compare.py <.o 或目录>... [--show 函数名]` | 原生代码与原版主程序逐函数比对（复用 skill 的 `macho_objdiff.py`，补上 ARM 模式的数据引用核对和链接器分支岛）；`.o` 由 `toolchain/build_native.sh` 编出 |
| `python tools/native_disasm.py <源文件名>...` | 导出原版某个编译单元的带注释反汇编（selector、字符串、类引用、GOT 都解析出名字）和 ObjC 类、分类元数据，写到 `recovery/native/disasm/` |
| `python tools/file_map.py` | 重新生成 `recovery/file_map.csv` |
| `python tools/import_decompiled.py` | 按 `tools/layout.csv` 一次性导入，自动删除接口代理方法、修正 using 歧义；`Assets/` 已有源码时拒绝运行 |

Python 用上一级的 `.venv`：`../.venv/Scripts/python.exe`。

## 两套产物

- `vanilla`：只用 `Assets/` 和原版资源，逐字节校验。`Assembly-CSharp.dll` 已做到除 MVID 和时间戳外与原版逐字节相同（`d013-mac-build`）。同一目标上多个特性的源码顺序要按 Mac 构建定，不要为 `--windows` 构建调换。
- `tuned`：在 `vanilla` 上叠加 `tweaks/`。只改数据时，不用重新 AOT，也不用重新链接，换资源包、重新签名即可。

数值写死在 C# 里的，改动放在单独的分支或补丁里，主线源码保持原样。
