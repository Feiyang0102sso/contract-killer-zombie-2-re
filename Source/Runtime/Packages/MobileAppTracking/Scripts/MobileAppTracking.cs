using System.Runtime.InteropServices;

namespace Glu.MobileAppTracking
{
	public class MobileAppTrackingLogger : LoggerSingleton<MobileAppTrackingLogger>
	{
		public MobileAppTrackingLogger()
		{
			LoggerSingleton<MobileAppTrackingLogger>.SetLoggerName("Package.MobileAppTracking");
		}
	}
}

namespace Glu.MobileAppTracking
{
	public class MobileAppTracking
	{
		private const string DLLIMPORT_NAME = "__Internal";

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void MAT_initialize(string advertiserId, string key);
#else
		private static void MAT_initialize(string advertiserId, string key)
		{
			NonIosNative.Stub("MAT_initialize");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void MAT_trackAction(string eventName, string itemName, float itemPrice, string currencyCode);
#else
		private static void MAT_trackAction(string eventName, string itemName, float itemPrice, string currencyCode)
		{
			NonIosNative.Stub("MAT_trackAction");
		}
#endif

		public static void Init(string advertiserId, string appKey)
		{
			MAT_initialize(advertiserId, appKey);
			TrackAction("open");
		}

		public static void TrackAction(string eventName)
		{
			TrackAction(eventName, null, 0f, null);
		}

		public static void TrackAction(string eventName, string itemName, float itemPrice, string currencyCode)
		{
			if (Integrity.IsJailbroken() && currencyCode != null)
			{
				return;
			}
			if (itemName != null)
			{
			}
			else
			{
			}
			MAT_trackAction(eventName, itemName, itemPrice, currencyCode);
		}
	}
}
