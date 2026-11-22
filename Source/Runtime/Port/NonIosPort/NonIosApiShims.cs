// Non-iOS port: stand-ins for iOS / mobile-only APIs, compiled only in desktop player builds (not editor, iOS or Android).
// The API updater turned old APIs such as iPhone.generation into UnityEngine.iOS.Device and friends; those types exist
// in the editor, on iOS and on Android, but not on desktop platforms. Same-named types are added here so the game source stays unchanged.
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
		/// Non-iOS platforms report an iPad 3: every feature the game disables per device stays on and the iPad high-res UI textures
		/// are used. The quality preset is not taken from this on PC: QualitySetter uses the launcher's GraphDetail.
		/// </summary>
		public static DeviceGeneration generation
		{
			get
			{
				return DeviceGeneration.iPad3Gen;
			}
		}

		/// <summary>iCloud backup flag, meaningless outside iOS.</summary>
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
		/// <summary>Mobile full-screen movie. Not played here, treated as finished (the PC port plays movies with MoviePlayerPc instead).</summary>
		public static bool PlayFullScreenMovie(string path, Color bgColor, FullScreenMovieControlMode controlMode, FullScreenMovieScalingMode scalingMode)
		{
			Debug.Log("[NonIosApiShims] Handheld.PlayFullScreenMovie skipped: " + path);
			return false;
		}
	}
}
#endif
