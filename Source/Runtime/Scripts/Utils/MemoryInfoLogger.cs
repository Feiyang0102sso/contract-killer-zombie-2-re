using System.IO;
using UnityEngine;

public class MemoryInfoLogger : LoggerSingleton<MemoryInfoLogger>
{
	public MemoryInfoLogger()
	{
		LoggerSingleton<MemoryInfoLogger>.SetLoggerName("MemoryInfoLogger");
		for (int i = 0; i < LoggerSingleton<MemoryInfoLogger>.GetHandlerCount(); i++)
		{
			LoggerSingleton<MemoryInfoLogger>.RemoveHandler(LoggerSingleton<MemoryInfoLogger>.GetHandler(i));
		}
		string temporaryCachePath = Application.temporaryCachePath;
		string path = Path.Combine(temporaryCachePath, "memory_log.txt");
		Logging.IHandler handler = new Logging.FileHandler(path, FileMode.Create);
		Logging.SimpleFormatter formatter = new Logging.SimpleFormatter();
		formatter.MessageFormat = "{0}";
		handler.SetFormatter(formatter);
		LoggerSingleton<MemoryInfoLogger>.Propagate = false;
		LoggerSingleton<MemoryInfoLogger>.AddHandler(handler);
		LoggerSingleton<MemoryInfoLogger>.SetLevel(20);
	}
}
