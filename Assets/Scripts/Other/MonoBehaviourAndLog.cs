using System.Diagnostics;
using UnityEngine;

public class MonoBehaviourAndLog : MonoBehaviour
{
	protected Logging.ILogger log;

	protected void LogTo(string loggerName)
	{
		log = Logging.GetLogger(loggerName);
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	protected void DebugCond(bool condition, string f, params object[] a)
	{
		if (condition)
		{
		}
	}

	[Conditional("LOGGING_LEVEL_DEBUG")]
	protected void Debug(string f, params object[] a)
	{
		log.Debug(gameObject, f, a);
	}

	[Conditional("LOGGING_LEVEL_INFO")]
	protected void Info(string f, params object[] a)
	{
		log.Info(gameObject, f, a);
	}

	[Conditional("LOGGING_LEVEL_WARN")]
	protected void Warn(string f, params object[] a)
	{
		log.Warn(gameObject, f, a);
	}

	[Conditional("LOGGING_LEVEL_ERROR")]
	protected void Error(string f, params object[] a)
	{
		log.Error(gameObject, f, a);
	}

	[Conditional("LOGGING_LEVEL_CRITICAL")]
	protected void Critical(string f, params object[] a)
	{
		log.Critical(gameObject, f, a);
	}
}
