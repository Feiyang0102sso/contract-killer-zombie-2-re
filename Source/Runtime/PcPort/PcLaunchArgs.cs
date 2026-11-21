// PC port: settings chosen in the launcher (Launcher/ckz2.exe, table in Launcher/PcConfig.h).
// The launcher reads ckz2.cfg and passes the values on the command line; the game never reads the cfg itself.
// Resolution and window mode go through Unity's own -screen-* arguments; this class reads the three -ckz2-* ones.
// Without the arguments (editor Play) the defaults below apply.
using System;
using System.Globalization;
using UnityEngine;

public static class PcLaunchArgs
{
	// ---- Argument names (must match PcConfig.h) ----
	private const string GraphDetailArgument = "-ckz2-graph-detail";
	private const string VSyncArgument = "-ckz2-vsync";
	private const string DrawFpsArgument = "-ckz2-draw-fps";

	// ---- Defaults when an argument is missing (same as the cfg defaults in PcConfig.h) ----
	private const int DefaultGraphDetail = 4;
	private const bool DefaultVSync = true;
	private const bool DefaultDrawFps = false;

	/// <summary>
	/// Original quality preset (QualitySettings.asset) behind each GraphDetail level 0..4:
	/// Lowest, Low, Medium, High, Ultra. Lowest (iPod4) also trims level content (QualitySetter.TrimmedLevels).
	/// </summary>
	private static readonly string[] GraphDetailPresets = new string[] { "iPod4", "iPhone4", "AndroidMedium", "iPhone4S", "iPad3" };

	/// <summary>Quality preset name chosen in the launcher.</summary>
	public static string QualityPresetName { get; private set; }

	public static bool VSync { get; private set; }

	public static bool DrawFps { get; private set; }

	static PcLaunchArgs()
	{
		string[] arguments = Environment.GetCommandLineArgs();
		int graphDetail = ReadInt(arguments, GraphDetailArgument, DefaultGraphDetail, 0, GraphDetailPresets.Length - 1);
		QualityPresetName = GraphDetailPresets[graphDetail];
		VSync = ReadBool(arguments, VSyncArgument, DefaultVSync);
		DrawFps = ReadBool(arguments, DrawFpsArgument, DefaultDrawFps);
		Debug.Log("[PcLaunchArgs] graph detail " + graphDetail + " (" + QualityPresetName + "), vsync " + VSync + ", draw fps " + DrawFps);
	}

	/// <summary>
	/// No frame cap; VSync on or off. Call after QualitySettings.SetQualityLevel, which resets vSyncCount to the preset's.
	/// </summary>
	public static void ApplyFrameSettings()
	{
		int vSyncCount = 0;
		if (VSync)
		{
			vSyncCount = 1;
		}
		QualitySettings.vSyncCount = vSyncCount;
		Application.targetFrameRate = -1;
		Debug.Log("[PcLaunchArgs] vSyncCount " + vSyncCount + ", no frame cap");
	}

	/// <summary>Value after the argument name, or the default when missing or out of range.</summary>
	private static int ReadInt(string[] arguments, string name, int defaultValue, int minValue, int maxValue)
	{
		for (int i = 0; i + 1 < arguments.Length; i++)
		{
			if (arguments[i] != name)
			{
				continue;
			}
			int value;
			bool parsed = int.TryParse(arguments[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
			if (parsed && value >= minValue && value <= maxValue)
			{
				return value;
			}
			Debug.LogWarning("[PcLaunchArgs] invalid " + name + " " + arguments[i + 1] + ", using " + defaultValue);
			return defaultValue;
		}
		return defaultValue;
	}

	private static bool ReadBool(string[] arguments, string name, bool defaultValue)
	{
		int defaultNumber = 0;
		if (defaultValue)
		{
			defaultNumber = 1;
		}
		return ReadInt(arguments, name, defaultNumber, 0, 1) == 1;
	}
}
