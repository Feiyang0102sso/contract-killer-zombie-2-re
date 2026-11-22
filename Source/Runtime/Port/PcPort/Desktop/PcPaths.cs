// PC port: every file the game writes stays next to the exe instead of C:\Users\...\AppData.
//   build:  <exe dir>\Cache\                   (replaces Application.temporaryCachePath)
//           <exe dir>\Logs\<exe name>.game.log  (the game's own Glu log, was log.txt in the temp folder)
//           <exe dir>\Logs\<exe name>.log       (Unity log, written by PcPlayerLog; Unity's Player.log is turned off)
//           (the launcher writes Logs\<exe name>.launcher.log next to these)
//   editor: <project>\Temp\PcCache\ and <project>\Logs\ckz2.game.log (both git-ignored)
// Saves are handled by StorageManager.PersistentDataPath (<exe dir>\Save\).
// The paths are captured before the first scene loads: Unity refuses Application.dataPath / temporaryCachePath
// inside MonoBehaviour constructors and field initializers (ServerTime reads its path from one).
using System.IO;
using UnityEngine;

public static class PcPaths
{
	/// <summary>Cache folder name next to the exe.</summary>
	private const string CacheDirName = "Cache";

	/// <summary>Cache folder inside the editor project's Temp folder.</summary>
	private const string EditorCacheDirName = "Temp/PcCache";

	/// <summary>Log folder: next to the exe in a build, Unity's own Logs folder in the editor.</summary>
	private const string LogDirName = "Logs";

	/// <summary>Base name of the log files in the editor.</summary>
	private const string EditorLogBaseName = "ckz2";

	/// <summary>Suffix of the "&lt;exe name&gt;_Data" folder that Application.dataPath points at.</summary>
	private const string DataFolderSuffix = "_Data";

	private static string _rootDir;

	private static string _logBaseName;

	/// <summary>Application.isEditor, captured with the paths (no engine calls later, see the header).</summary>
	private static bool _isEditor;

	/// <summary>Exe directory in a build, project root in the editor.</summary>
	public static string RootDir
	{
		get
		{
			EnsureCaptured();
			return _rootDir;
		}
	}

	/// <summary>Replacement for Application.temporaryCachePath. Created if missing.</summary>
	public static string CacheDir
	{
		get
		{
			EnsureCaptured();
			string cacheDir = Path.Combine(_rootDir, CacheDirName);
			if (_isEditor)
			{
				cacheDir = Path.Combine(_rootDir, EditorCacheDirName);
			}
			Directory.CreateDirectory(cacheDir);
			return cacheDir;
		}
	}

	/// <summary>Folder of all log files (exe dir or project root + Logs). Created if missing.</summary>
	public static string LogDir
	{
		get
		{
			EnsureCaptured();
			string logDir = Path.Combine(_rootDir, LogDirName);
			Directory.CreateDirectory(logDir);
			return logDir;
		}
	}

	/// <summary>The game's own (Glu Logging) log file.</summary>
	public static string GameLogPath
	{
		get
		{
			return Path.Combine(LogDir, _logBaseName + ".game.log");
		}
	}

	/// <summary>Unity log file written by PcPlayerLog (build only).</summary>
	public static string UnityLogPath
	{
		get
		{
			return Path.Combine(LogDir, _logBaseName + ".log");
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Capture()
	{
		string dataDir = Application.dataPath;
		_isEditor = Application.isEditor;
		_rootDir = Path.GetDirectoryName(dataDir);
		_logBaseName = EditorLogBaseName;
		if (!_isEditor)
		{
			// Builds\ckz2_Data -> ckz2
			string dataName = Path.GetFileName(dataDir);
			if (dataName.EndsWith(DataFolderSuffix))
			{
				dataName = dataName.Substring(0, dataName.Length - DataFolderSuffix.Length);
			}
			_logBaseName = dataName;
		}
	}

	private static void EnsureCaptured()
	{
		if (_rootDir == null)
		{
			Capture();
		}
	}
}
