// PC bridge: stand-ins for iOS / mobile-only APIs, compiled only in PC builds.
// The API updater turned old APIs such as iPhone.generation into UnityEngine.iOS.Device and friends; those types exist
// in the editor and on iOS but not on Windows. Same-named types are added here so the game source stays unchanged.
// In the editor Unity's own types are used, hence !UNITY_EDITOR to avoid duplicate definitions.
#if !UNITY_EDITOR && !UNITY_IOS && !UNITY_ANDROID
namespace UnityEngine.iOS
{
	/// <summary>Only the device generations the game code uses.</summary>
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
		/// PC reports an iPad 3: every feature the game disables per device stays on and the iPad high-res UI textures
		/// are used. The quality preset is not taken from this on PC: QualitySetter uses the launcher's GraphDetail.
		/// </summary>
		public static DeviceGeneration generation
		{
			get
			{
				return DeviceGeneration.iPad3Gen;
			}
		}

		/// <summary>iCloud backup flag, meaningless on PC.</summary>
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
		/// <summary>Mobile full-screen movie. Not played on PC yet, treated as finished (VideoPlayer later).</summary>
		public static bool PlayFullScreenMovie(string path, Color bgColor, FullScreenMovieControlMode controlMode, FullScreenMovieScalingMode scalingMode)
		{
			Debug.Log("[PcPort] Handheld.PlayFullScreenMovie skipped on PC: " + path);
			return false;
		}
	}
}
#endif
