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

	[DllImport("__Internal")]
	private static extern int NetworkStatus();

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

	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern int MT_GetCurrentMemoryBytes();
}
