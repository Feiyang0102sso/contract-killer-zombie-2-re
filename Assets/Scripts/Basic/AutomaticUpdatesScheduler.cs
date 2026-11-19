using System;
using Glu.AssetBundles;

public class AutomaticUpdatesScheduler
{
	public enum State
	{
		Unknown,
		InProgress,
		UpToDate,
		Outdated,
		Unreachable
	}

	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("App.AutomaticUpdatesScheduler");
		}
	}

	private static int m_timeInterval = 300;

	private static DateTime m_time = DateTime.UtcNow;

	private static string[] m_urls = null;

	private static IndexInfo m_indexInfo = null;

	private static UpdateInfo m_updateInfo = null;

	public static State state
	{
		get
		{
			State result = State.Unknown;
			if ((m_updateInfo != null && m_updateInfo.state == UpdateInfo.State.Pending) || (m_indexInfo != null && m_indexInfo.state == IndexInfo.State.InProgress))
			{
				result = State.InProgress;
			}
			else if (m_updateInfo != null)
			{
				switch (m_updateInfo.state)
				{
				case UpdateInfo.State.UpToDate:
					result = State.UpToDate;
					break;
				case UpdateInfo.State.Outdated:
					result = State.Outdated;
					break;
				case UpdateInfo.State.Unreachable:
					result = State.Unreachable;
					break;
				default:
					break;
				}
			}
			else if (m_indexInfo != null)
			{
				switch (m_indexInfo.state)
				{
				case IndexInfo.State.Succeeded:
					result = State.UpToDate;
					break;
				case IndexInfo.State.Failed:
					result = State.Unreachable;
					break;
				default:
					break;
				}
			}
			return result;
		}
	}

	public static int ThreadCount
	{
		set
		{
			DownloadManager.ThreadCount = value;
		}
		get
		{
			return DownloadManager.ThreadCount;
		}
	}

	public static int TimeInterval
	{
		set
		{
			m_timeInterval = value;
		}
		get
		{
			return m_timeInterval;
		}
	}

	public static IndexInfo AssetBundlesIndexInfo
	{
		get
		{
			return m_indexInfo;
		}
	}

	public static void Init(params string[] urls)
	{
		m_urls = urls;
	}

	public static bool Start()
	{
		bool result = false;
		if (state != State.InProgress)
		{
			m_indexInfo = AssetBundles.DownloadAll(m_urls);
			m_time = DateTime.UtcNow;
			result = true;
		}
		return result;
	}

	public static void CheckForUpdate()
	{
		if (m_indexInfo != null)
		{
			DateTime time = m_time;
			time.AddSeconds(TimeInterval);
			if ((state == State.Unreachable || time.Ticks < DateTime.UtcNow.Ticks) && state != State.InProgress)
			{
				m_updateInfo = m_indexInfo.CheckForUpdates();
				m_time = DateTime.UtcNow;
			}
		}
	}
}
