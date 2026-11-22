// Non-iOS port: C# replacements for the iOS native plugins (DllImport("__Internal")).
// Outside iOS the extern declarations in the game sources forward to the functions of the same name here; call sites are unchanged.
// PC-specific: Glu_Localization_GetLocale returns the launcher's language (PcLaunchArgs); another port needs its own source there.
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class NonIosNative
{
	// ---------------- Localization ----------------

	/// <summary>
	/// The native side writes the locale code into buf and returns its length (0 = unavailable, the game then guesses
	/// from Application.systemLanguage). PC returns the language chosen in the launcher (PcLaunchArgs.Language), so the
	/// game loads <locale>.txt and the locale's replacement assets (fonts, loading image) exactly as on iOS.
	/// </summary>
	public static int Glu_Localization_GetLocale(int size, byte[] buf)
	{
		byte[] locale = Encoding.UTF8.GetBytes(PcLaunchArgs.Language);
		if (locale.Length > size)
		{
			return 0;
		}
		Array.Copy(locale, buf, locale.Length);
		return locale.Length;
	}

	// ---------------- Integrity ----------------

	/// <summary>Non-iOS platforms count as not jailbroken: the game then enables "trusted" analytics, allows in-app purchases, etc.</summary>
	public static bool IsJailbroken()
	{
		return false;
	}

	/// <summary>On iOS this checks whether the executable is App Store encrypted; non-iOS platforms count as genuine.</summary>
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
			UnityEngine.Debug.Log("[NonIosNative] native stub called: " + name);
		}
	}

	// ---------------- ServerTime ----------------

	/// <summary>Monotonic clock in milliseconds (the iOS native side also returns monotonic milliseconds since boot).</summary>
	public static long ServerTime_getTicks()
	{
		return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
	}
}
