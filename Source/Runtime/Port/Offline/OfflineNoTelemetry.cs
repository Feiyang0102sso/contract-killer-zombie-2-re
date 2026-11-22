// Offline: turns off Unity's built-in analytics before the first scene loads, on every platform.
// The project carries com.unity.analytics (required by com.unity.purchasing), whose engine service writes its state to
// AppData\LocalLow\<company>\<product>\Unity\...\Analytics on every start and reports to Unity's servers.
// The game's own analytics (Flurry/Kontagent/...) are native stubs outside iOS already. Disabling the services in the
// project settings alone did not stop it, so it is switched off here at startup.
using UnityEngine;
using UnityEngine.Analytics;

public static class OfflineNoTelemetry
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Disable()
	{
		Analytics.initializeOnStartup = false;
		Analytics.enabled = false;
		Analytics.deviceStatsEnabled = false;
		Analytics.limitUserTracking = true;
		Debug.Log("[OfflineNoTelemetry] Unity analytics disabled");
	}
}
