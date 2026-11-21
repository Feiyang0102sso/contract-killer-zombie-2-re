// PC port: the Unity log is written live to the Logs folder next to the exe as "<exe name>.log" (Builds\Logs\ckz2.log);
// the previous run's log is kept as "<exe name>-prev.log". Unity's own Player.log (AppData\LocalLow) is turned off in the player settings
// (usePlayerLog: 0), so nothing is written to C:\Users\...\AppData.
// Everything that reaches Application.logMessageReceivedThreaded is recorded: script logs, exceptions, and the engine
// errors/warnings Unity reports through the log callback. Pure engine startup output (GPU, driver...) is not, so a short
// system header is written instead. Written as it happens, so the file survives a crash or a killed process.
using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class PcPlayerLog
{
	/// <summary>Suffix of the previous run's log ("ckz2-prev.log").</summary>
	private const string PreviousSuffix = "-prev";

	private static readonly object WriterLock = new object();

	private static StreamWriter _writer;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Install()
	{
		// The editor has its own Editor.log
		if (Application.isEditor)
		{
			return;
		}
		string logPath = PcPaths.UnityLogPath;
		if (!Open(logPath))
		{
			return;
		}
		WriteHeader();
		Application.logMessageReceivedThreaded += OnLogMessage;
		Application.quitting += Close;
		Debug.Log("[PcPlayerLog] logging to " + logPath);
	}

	/// <summary>The window only has its real size once the first scene is loaded (at startup it reports a placeholder).</summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void LogDisplay()
	{
		if (Application.isEditor)
		{
			return;
		}
		Debug.Log("[PcPlayerLog] screen " + Screen.width + "x" + Screen.height + " " + Screen.fullScreenMode);
	}

	/// <summary>Keeps the previous log as -prev and opens a fresh one. False if the file is in use (another instance).</summary>
	private static bool Open(string logPath)
	{
		string previousPath = Path.Combine(Path.GetDirectoryName(logPath),
			Path.GetFileNameWithoutExtension(logPath) + PreviousSuffix + Path.GetExtension(logPath));
		try
		{
			if (File.Exists(logPath))
			{
				File.Copy(logPath, previousPath, true);
			}
			FileStream stream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
			_writer = new StreamWriter(stream, new UTF8Encoding(false));
			_writer.AutoFlush = true;
			return true;
		}
		catch (IOException exception)
		{
			// Another running instance holds the log: carry on without a file log rather than fail the start
			Debug.LogWarning("[PcPlayerLog] cannot open " + logPath + ": " + exception.Message);
			return false;
		}
	}

	private static void WriteHeader()
	{
		StringBuilder header = new StringBuilder();
		header.AppendLine("CK Zombies 2 PC log, started " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
		header.AppendLine("Unity " + Application.unityVersion + ", " + SystemInfo.operatingSystem);
		header.AppendLine("CPU " + SystemInfo.processorType + " x" + SystemInfo.processorCount + ", RAM " + SystemInfo.systemMemorySize + " MB");
		header.AppendLine("GPU " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsDeviceVersion + "), VRAM " + SystemInfo.graphicsMemorySize + " MB");
		header.AppendLine("Command line: " + Environment.CommandLine);
		header.AppendLine();
		Write(header.ToString());
	}

	private static void OnLogMessage(string message, string stackTrace, LogType type)
	{
		StringBuilder entry = new StringBuilder();
		entry.Append(DateTime.Now.ToString("HH:mm:ss.fff"));
		entry.Append(" [");
		entry.Append(type);
		entry.Append("] ");
		entry.AppendLine(message);
		// Plain logs without their stack keep the file readable; warnings and errors keep it
		if (type != LogType.Log && !string.IsNullOrEmpty(stackTrace))
		{
			entry.AppendLine(stackTrace.TrimEnd());
		}
		Write(entry.ToString());
	}

	private static void Write(string text)
	{
		lock (WriterLock)
		{
			if (_writer != null)
			{
				_writer.Write(text);
			}
		}
	}

	private static void Close()
	{
		Write(DateTime.Now.ToString("HH:mm:ss.fff") + " [Log] quit" + Environment.NewLine);
		lock (WriterLock)
		{
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}
		}
	}
}
