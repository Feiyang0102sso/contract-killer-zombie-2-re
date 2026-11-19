# 工具链

本目录是构建需要的全部脚本、补丁和数据。搭环境和构建的步骤见仓库根目录的 [`README_CN.md`](../README_CN.md)；这里说明每个文件的作用、发行版里的布局，以及让 Mac 程序在 Linux 上跑起来所做的修改。

## 文件

| 文件 | 作用 |
| --- | --- |
| `setup_wsl.ps1` | Windows 侧（由 `setup.bat` 调用）：在 `.wsl\` 建发行版 `ckz2-build`，下载 Ubuntu 镜像和 Unity 安装包，再调用 `setup_distro.sh` |
| `setup_distro.sh [xcode-dmg]` | Linux 侧：装 apt 依赖（含 i386）、编 64 位和 32 位 maloader、编 Darwin 垫片、解 Unity 和 Xcode 4.5（核对 SHA-256）、自检。可重复运行，已完成的步骤会跳过 |
| `build.sh [--skip-dll]` | Linux 侧一键构建（由 `build.bat` 调用）：检查工具链后运行 `build_main.py` |
| `build_main.py` | 编排：`build_dll.py` → 回填 MVID 与时间戳并核对 SHA-256 → `build_main.sh` → 签名并放进 `bin/`。在 Windows 上直接运行时，Linux 部分经 WSL 执行 |
| `build_dll.py` | 按 Unity 的规则收集 `Assets/` 下的 `.cs`，按路径排序写成 `build/vanilla/sources.rsp`，调用 `smcs_mac.sh` |
| `smcs_mac.sh` | 在 Mac 版 Mono 上运行 smcs，编出 `Assembly-CSharp.dll`（约 25 秒） |
| `build_main.sh` | 放齐 Unity 程序集、生成 `RegisterMonoModules.cpp`、8 个程序集并行 AOT 并汇编、编原生代码、按原版顺序用 ld64 链接。`SKIP_COMPILE=1` 只重链（约 5 秒），`LOAD_FLAGS` 可覆盖 `-all_load -dead_strip` 做实验 |
| `aot.sh <dll> <out.s>` | 用原版参数和环境变量运行 mono-xcompiler；dll 所在目录要放齐它引用的程序集 |
| `build_native.sh <repo> <out_dir> [vanilla\|tuned] [文件名...]` | 在原版构建路径搭 Xcode 工程（Unity 模板 + `Assets/Plugins/iOS/`，tuned 再加 `native_stubs/`），用 Xcode 4.5 clang 编出 `.o`；给文件名时只编这些 |
| `xcode45.sh <tool> [args]` | 运行 Xcode 4.5 的工具（`clang`、`ld`、`as`、`otool`、`nm` 等），过滤 maloader 的未定义符号提示 |
| `sign_adhoc.py <输入> <输出>` | 按原版格式做 ad-hoc 签名：CodeDirectory（SHA-1）+ 空 Requirements + Entitlements |
| `register_mono_modules/` | 照 Unity 3.5.7 `MonoAOTRegistration` 的逻辑生成 `RegisterMonoModules.cpp`，用 Unity 自带的 Mono.Cecil 0.6，在 Mac 版 Mono 上运行 |
| `patches/maloader.patch` | maloader 的 3 处修正（glibc ≥ 2.32 的头文件、未定义符号上的 `N_REF_TO_WEAK`、外部重定位绑定） |
| `patches/maloader32.patch` | 32 位 maloader 的修改，见下文 |
| `darwin_shim/darwin_shim.c` | i386 程序缺的 Darwin 函数、条件变量包装，以及 Apple Libc 的 `qsort` |
| `link_order.txt` | 从原版导出：目标文件的链接顺序（调试映射的 `N_OSO` 顺序，不含静态库成员） |
| `entitlements.plist` | 从原版导出：签名里的 entitlements，字节原样保留（不做换行转换） |
| `downloads/` | 安装包：`Xcode_4.5.dmg`（自备）、`unity-3.5.7.dmg`（自动下载）。不入库 |

从原版导出的还有三个常量，写在 `build_main.py` 里：`Assembly-CSharp.dll` 的 MVID、PE 时间戳和 SHA-256。smcs 每次编译都会写入新的 MVID 和时间戳；AOT 镜像记录了 MVID，运行时会和 DLL 核对，所以这两处必须回填成原版的值。回填后整个 DLL 的 SHA-256 必须与原版相同，否则构建停止。

## 发行版里的布局

| 路径 | 内容 |
| --- | --- |
| `/work/ckz2/maloader/` | 64 位 maloader：`shinh/maloader` 提交 `464a90f` + `maloader.patch`，跑 Xcode 4.5 的工具 |
| `/work/ckz2/maloader32/` | 32 位 maloader：同一份源码再加 `maloader32.patch`，跑 mono-xcompiler 和 Mac 版 Mono |
| `/work/ckz2/darwin_shim32.so` | 垫片，经 `LD_PRELOAD` 注入 |
| `/work/ckz2/unity357/` | Unity 安装包里的 iOS 构建工具（`mono-xcompiler`）、Xcode 工程模板与 `libiPhone-lib.a`、iOS 版 `UnityEngine.dll` |
| `/work/ckz2/unity357_mono/` | Unity 编辑器自带的 Mac 版 Mono：`bin/mono`、`lib/mono/unity/`（smcs.exe 和 2.0.5.0 程序集） |
| `/work/xcode45/Xcode.app/` | Xcode 4.5 解包（约 3.5 GB） |
| `/work/ckz2/probe/` | 临时目录，用完清空 |
| `/Applications/buildAgent/work/a03c2b4a5f58e0ef/GeneratedProject_ios/` | 原版构建路径，`build_native.sh` 在这里搭工程 |

这些路径会写进目标文件的调试信息，不要改动。

从 Windows 直接调用脚本时用 `wsl -d ckz2-build -u root --exec bash <脚本>`。在 Windows 上直接运行 `build_main.py`、`build_dll.py` 时，发行版名取环境变量 `CKZ2_DISTRO`，默认 `ckz2-build`。

## 让 Mac 程序在 Linux 上跑起来

mono-xcompiler 和 Mac 版 Mono 是 i386 Mach-O。maloader 自带“实验性”的 32 位支持，实际要补这些地方：

| 问题 | 现象 | 处理 |
| --- | --- | --- |
| 32 位启动汇编把 64 位 `entry` 塞进 32 位寄存器 | `-m32` 编译报 `unsupported size for integer register` | 补丁：`(uint32_t)entry` |
| `make release` 会覆盖 `GCC_EXTRA_FLAGS` | 编出来的还是 64 位 | 直接 `make all BITS=32 "GCC_EXTRA_FLAGS=-m32 -DNOLOG -DNDEBUG"` |
| 31 个 Darwin 函数未实现（mach 线程、信号量、dyld 镜像枚举等） | `Undefined function called` | 垫片：信号量用 POSIX 实现；mach 异常线程永久阻塞；GC 停世界类函数返回失败（`GC_DONT_GC` 下不会调用） |
| `_dyld_bind_fully_image_containing_address` 的返回值 | `… failed` 后 abort | Darwin 原型返回 bool，垫片返回 1 |
| Darwin i386 的 `pthread_cond_t` 只有 28 字节，glibc 要 48 字节 | 写完 `.s` 后卡死在 `pthread_cond_broadcast` | 补丁把 `pthread_cond_*` 改名到垫片，垫片在 Darwin 条件变量里存一个堆上的 glibc 条件变量指针 |
| `dlopen(NULL)`（查 `__Internal` P/Invoke 目标） | maloader 拿 NULL 构造 `std::string`，抛 `logic_error` | 补丁：返回空导出表 |
| 老式 i386 导入桩 `__IMPORT,__jump_table` | 第一次调库函数就落在 `hlt` 上 | 补丁：按 5 字节一项写 `E9 rel32` |
| 启动时 `dlopen` CoreFoundation | maloader 找不到文件就退出 | 补丁：像真的 `dlopen` 一样返回 NULL 并设置 `dlerror` |
| `libiconv*` | 未定义符号 | 改名到 glibc 的 `iconv*`（接口相同） |
| `fcntl` 只支持 `F_GETPATH` | `System.Console` 初始化抛异常 | 补丁：支持 `F_DUPFD/F_GETFD/F_SETFD/F_GETFL/F_SETFL`，标志位按 Darwin 的值换算 |
| Darwin i386 的 `off_t`、`blkcnt_t` 是 64 位 | 写输出文件时 seek 出错 | 补丁：`__darwin_stat` 的 `st_size`、`st_blocks` 改成 64 位；`lseek`、`ftruncate` 改名到 `*64` 版 |
| `qsort` | glibc 的稳定排序与 Mac 的结果不同，DLL 的 CustomAttribute 表排列对不上 | 改名到垫片的 `darwin_bsd_qsort`：照 Xcode 4.5 iOS 6 模拟器 `libsystem_sim_c.dylib` 的 `__qsort` 反汇编转写 |
| Mac 版 Mono 下 `Directory.GetFiles` 返回空 | 生成器找不到程序集 | 生成器改为由调用者传入 DLL 列表，排序规则不变 |

改名只作用于 Mach-O 程序的导入符号，宿主自己的 pthread、qsort 不受影响。

Xcode 4.5 的 clang、ld、as 是 x86_64 Mach-O，64 位 maloader 直接能跑。解 Xcode 时 7z 会拒绝 58 个指向上级目录或绝对路径的符号链接，影响 iOS 工具链的只有 `iPhoneOS6.0.sdk/usr/include/arpa/nameser.h`，`setup_distro.sh` 会补回。
