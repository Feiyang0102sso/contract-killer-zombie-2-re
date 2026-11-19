# native_stubs/：广告、统计、联网类原生插件的空实现

原版 `Libraries/` 里有 25 个 Glu 原生插件。其中 14 个已经逐函数还原，放在 `Assets/Plugins/iOS/`；这里的 11 个是第三方 SDK 的胶水代码（Flurry、Facebook、内购、Kontagent、MAT、PlayHaven、Tapjoy），按项目决定不还原。

- **只进 `tuned` 产物**：`toolchain/build_native.sh <repo> <out> tuned` 才会把这里的文件放进 `Libraries/`；`vanilla` 不用它们，也不参与逐字节校验。
- **文件名与原版相同**，一个文件对应原版一个编译单元。原版只在单元内部使用的文件（`CFlurry_Cocoa_mm.mm`、`MATInterface.mm`、`VerificationController.m`）保留为空文件。
- **只提供 C# 用 `DllImport("__Internal")` 调用的 77 个入口**（C# 一共声明 97 个，另外 20 个由已还原的插件提供）。Mono 全量 AOT 时，这些调用会编译成对同名 C 符号的直接调用，所以一个都不能少；入口清单按 C# 声明逐个核对过。
- **C# 侧的调用 1:1 保留**，这里只决定返回什么：
  - 布尔量返回 0，C# 走“SDK 不可用”的分支（例如 `IsIAPInitialized()` 为假时，内购界面不发起任何请求）。
  - `IntPtr` 返回静态空串 `""`：C# 用 `Marshal.PtrToStringAnsi` 读取，得到空串而不是 null。
  - 返回类型为 `string` 的入口返回 `strdup("")`：Mono 封送后会释放这个指针，必须是堆上的拷贝。
  - 回调从不触发。
- `ServerTime` 不在这里：它只是读本机的单调时钟（`mach_absolute_time`），不联网，已按原版还原。
