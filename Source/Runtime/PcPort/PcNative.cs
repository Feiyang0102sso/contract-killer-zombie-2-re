// PC bridge: C# replacements for the iOS native plugins (DllImport("__Internal")).
// On PC the extern declarations in the game sources forward to the functions of the same name here; call sites are unchanged.
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class PcNative
{
	// ---------------- Localization ----------------

	/// <summary>
	/// The native side returns the system language code; 0 means unavailable and the game falls back to Application.systemLanguage.
	/// PC simply takes the game's own fallback.
	/// </summary>
	public static int Glu_Localization_GetLocale(int size, byte[] buf)
	{
		return 0;
	}

	// ---------------- Integrity ----------------

	/// <summary>PC counts as not jailbroken: the game then enables "trusted" analytics, allows in-app purchases, etc.</summary>
	public static bool IsJailbroken()
	{
		return false;
	}

	/// <summary>On iOS this checks whether the executable is App Store encrypted; PC counts as genuine.</summary>
	public static bool IsEncrypted()
	{
		return true;
	}

	// ---------------- UnityBuildSystem / BuildInfo ----------------

	/// <summary>Same as the original Info.plist.</summary>
	private const string BundleId = "com.glu.ckzombies2";

	private const string BundleVersion = "2.0.2";

	public static void GetBundleId(StringBuilder sb, int size)
	{
		sb.Append(BundleId);
	}

	public static void GetBundleVersion(StringBuilder sb, int size)
	{
		sb.Append(BundleVersion);
	}

	// ---------------- Stubs for the remaining plugins log once ----------------

	private static readonly System.Collections.Generic.HashSet<string> LoggedStubs = new System.Collections.Generic.HashSet<string>();

	/// <summary>Logs the first call of each stub, to show which native interfaces the game actually uses.</summary>
	public static void Stub(string name)
	{
		if (LoggedStubs.Add(name))
		{
			UnityEngine.Debug.Log("[PcPort] native stub called: " + name);
		}
	}

	// ---------------- ServerTime ----------------

	/// <summary>Monotonic clock in milliseconds (the iOS native side also returns monotonic milliseconds since boot).</summary>
	public static long ServerTime_getTicks()
	{
		return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
	}
}
