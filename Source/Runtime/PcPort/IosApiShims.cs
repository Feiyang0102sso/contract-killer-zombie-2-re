// PC 桥接：iOS / 移动端专有 API 的替身，只在打 PC 包时编译。
// API 更新器把 iPhone.generation 等旧 API 改成了 UnityEngine.iOS.Device 等，
// 这些类型在编辑器和 iOS 平台存在，在 Windows 平台不存在。这里补上同名类型，游戏源码不用改。
// 编辑器里由 Unity 自带的类型提供，因此用 !UNITY_EDITOR 排除，避免重复定义。
#if !UNITY_EDITOR && !UNITY_IOS && !UNITY_ANDROID
namespace UnityEngine.iOS
{
	/// <summary>只保留游戏代码用到的设备代号。</summary>
	public enum DeviceGeneration
	{
		Unknown,
		iPhone4,
		iPhone4S,
		iPhone5,
		iPhoneUnknown,
		iPodTouch4Gen,
		iPodTouch5Gen,
		iPodTouchUnknown,
		iPad1Gen,
		iPad2Gen,
		iPad3Gen,
		iPadUnknown
	}

	public static class Device
	{
		/// <summary>
		/// PC 报告为 iPad 3 代：游戏会选最高画质（"iPad3"），开启所有按设备关闭的功能，
		/// 并使用 iPad 的高分辨率界面贴图。
		/// </summary>
		public static DeviceGeneration generation
		{
			get
			{
				return DeviceGeneration.iPad3Gen;
			}
		}

		/// <summary>iCloud 备份标记，PC 上没有意义。</summary>
		public static void SetNoBackupFlag(string path)
		{
		}

		public static void ResetNoBackupFlag(string path)
		{
		}
	}
}

namespace UnityEngine
{
	public enum FullScreenMovieControlMode
	{
		Full,
		Minimal,
		CancelOnInput,
		Hidden
	}

	public enum FullScreenMovieScalingMode
	{
		None,
		AspectFit,
		AspectFill,
		Fill
	}

	public static class Handheld
	{
		/// <summary>移动端全屏播片。PC 暂不播放，直接视为播完（以后改用 VideoPlayer）。</summary>
		public static bool PlayFullScreenMovie(string path, Color bgColor, FullScreenMovieControlMode controlMode, FullScreenMovieScalingMode scalingMode)
		{
			Debug.Log("[PcPort] Handheld.PlayFullScreenMovie skipped on PC: " + path);
			return false;
		}
	}
}
#endif
