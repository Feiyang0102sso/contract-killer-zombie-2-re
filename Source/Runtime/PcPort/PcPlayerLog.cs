// PC port: when the game quits, copy this run's player log next to the exe as "<exe name>.log" (Builds\ckz2.log).
// Unity itself writes the log to %USERPROFILE%\AppData\LocalLow\<company>\<product>\Player.log; the copy is just
// easier to find. Lines Unity writes after Application.quitting (engine shutdown) are not in the copy.
// A crash skips Application.quitting, so after a crash read Player.log instead.
using System.IO;
using UnityEngine;

public static class PcPlayerLog
{
	/// <summary>Suffix of the "<exe name>_Data" folder that Application.dataPath points at.</summary>
	private const string DataFolderSuffix = "_Data";

	/// <summary>Extension of the copied log.</summary>
	private const string LogExtension = ".log";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Install()
	{
		// The editor has its own Editor.log and no exe next to the data folder
		if (Application.isEditor)
		{
			return;
		}
		Application.quitting += CopyLogNextToExe;
		Debug.Log("[PcPlayerLog] will copy " + Application.consoleLogPath + " to " + GetTargetPath() + " on quit");
	}

	/// <summary>Builds\ckz2_Data -> Builds\ckz2.log</summary>
	private static string GetTargetPath()
	{
		string dataDir = Application.dataPath;
		string rootDir = Path.GetDirectoryName(dataDir);
		string exeName = Path.GetFileName(dataDir);
		if (exeName.EndsWith(DataFolderSuffix))
		{
			exeName = exeName.Substring(0, exeName.Length - DataFolderSuffix.Length);
		}
		return Path.Combine(rootDir, exeName + LogExtension);
	}

	private static void CopyLogNextToExe()
	{
		string sourcePath = Application.consoleLogPath;
		if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
		{
			Debug.LogWarning("[PcPlayerLog] no player log to copy (started with -nolog?)");
			return;
		}
		string targetPath = GetTargetPath();
		Debug.Log("[PcPlayerLog] copying log to " + targetPath);
		// Unity keeps Player.log open for writing, so share ReadWrite instead of File.Copy
		using (FileStream input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			using (FileStream output = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
			{
				input.CopyTo(output);
			}
		}
	}
}
