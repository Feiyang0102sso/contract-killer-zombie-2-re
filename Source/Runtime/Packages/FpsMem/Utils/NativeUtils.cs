using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeUtils
{
	public enum NetworkType
	{
		NetNone,
		NetWiFi,
		Net3G
	}

	private const string m_ExternalFileName = "__Internal";

	public static bool IsNetworkAvailable()
	{
		int num = NetworkStatus();
		Debug.Log("NetworkStatus = " + num);
		return num != 0;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int NetworkStatus();
#else
	private static int NetworkStatus()
	{
		NonIosNative.Stub("NetworkStatus");
		return default(int);
	}
#endif

	public static void ReportMemory(string eventName)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log(eventName + " " + MT_GetCurrentMemoryBytes());
		}
	}

	public static int GetCurrentMemoryBytes()
	{
		return MT_GetCurrentMemoryBytes();
	}

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern int MT_GetCurrentMemoryBytes();
#else
	private static int MT_GetCurrentMemoryBytes()
	{
		NonIosNative.Stub("MT_GetCurrentMemoryBytes");
		return default(int);
	}
#endif
}
