using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class CFlurry
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.Flurry");
		}
	}

	private static class Impl
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.Flurry.Impl");
			}
		}

		private const string kExternalFilename = "__Internal";

		public static void SetAppVersion(string version)
		{
			CFlurry_SetAppVersion(version);
		}

		public static string GetFlurryAgentVersion()
		{
			return CFlurry_GetFlurryAgentVersion();
		}

		public static void SetShowErrorInLogEnabled(bool value)
		{
			CFlurry_SetShowErrorInLogEnabled(value);
		}

		public static void SetDebugLogEnabled(bool value)
		{
			CFlurry_SetDebugLogEnabled(value);
		}

		public static void SetSessionContinueSeconds(int seconds)
		{
			CFlurry_SetSessionContinueSeconds(seconds);
		}

		public static void SetSecureTransportEnabled(bool value)
		{
			CFlurry_SetSecureTransportEnabled(value);
		}

		public static string GetUserID()
		{
			return Marshal.PtrToStringAnsi(CFlurry_GetUserID());
		}

		public static void StartSession(string apiKey)
		{
			bool trusted = !Integrity.IsJailbroken();
			Dictionary<string, string> details = Integrity.GetDetails();
			string[] array = new string[details.Keys.Count];
			string[] array2 = new string[details.Values.Count];
			details.Keys.CopyTo(array, 0);
			details.Values.CopyTo(array2, 0);
			CFlurry_StartSession(apiKey, trusted, array, array2);
		}

		public static void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
		{
			string[] array = new string[eventParams.Keys.Count];
			eventParams.Keys.CopyTo(array, 0);
			string[] array2 = new string[eventParams.Values.Count];
			int num = 0;
			foreach (object value in eventParams.Values)
			{
				array2[num] = (!(value != null) ? string.Empty : value.ToString());
				num++;
			}
			CFlurry_LogEvent(eventTypeId, array, array2);
		}

		public static void SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose)
		{
			CFlurry_SetSessionReportsOnCloseEnabled(sendSessionReportsOnClose);
		}

		public static void SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled)
		{
			CFlurry_SetSessionReportsOnPauseEnabled(setSessionReportsOnPauseEnabled);
		}

		public static void SetEventLoggingEnabled(bool value)
		{
			CFlurry_SetEventLoggingEnabled(value);
		}

		public static void StopSession()
		{
		}

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void CFlurry_SetAppVersion(string version);

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern string CFlurry_GetFlurryAgentVersion();

		[DllImport("__Internal")]
		private static extern void CFlurry_SetShowErrorInLogEnabled(bool value);

		[DllImport("__Internal")]
		private static extern void CFlurry_SetDebugLogEnabled(bool value);

		[DllImport("__Internal")]
		private static extern void CFlurry_SetSessionContinueSeconds(int seconds);

		[DllImport("__Internal")]
		private static extern void CFlurry_SetSecureTransportEnabled(bool value);

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern IntPtr CFlurry_GetUserID();

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void CFlurry_StartSession(string apiKey, bool trusted, string[] keys, string[] values);

		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void CFlurry_LogEvent(string eventTypeId, string[] keys, string[] values);

		[DllImport("__Internal")]
		private static extern void CFlurry_SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose);

		[DllImport("__Internal")]
		private static extern void CFlurry_SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled);

		[DllImport("__Internal")]
		private static extern void CFlurry_SetEventLoggingEnabled(bool value);
	}

	public static void SetAppVersion(string version)
	{
		Impl.SetAppVersion(version);
	}

	public static string GetFlurryAgentVersion()
	{
		var result = Impl.GetFlurryAgentVersion();
		return result;
	}

	public static void SetShowErrorInLogEnabled(bool value)
	{
		Impl.SetShowErrorInLogEnabled(value);
	}

	public static void SetDebugLogEnabled(bool value)
	{
		Impl.SetDebugLogEnabled(value);
	}

	public static void SetSessionContinueSeconds(int seconds)
	{
		Impl.SetSessionContinueSeconds(seconds);
	}

	public static void SetSecureTransportEnabled(bool value)
	{
		Impl.SetSecureTransportEnabled(value);
	}

	public static string GetUserID()
	{
		var result = Impl.GetUserID() ?? "Undefined";
		return result;
	}

	public static void StartSession(string apiKey)
	{
		Impl.StartSession(apiKey);
	}

	public static void StopSession()
	{
		Impl.StopSession();
	}

	public static void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
	{
		string text = string.Empty;
		if (eventParams != null && eventParams.Count > 0)
		{
			foreach (KeyValuePair<string, object> eventParam in eventParams)
			{
				text += string.Format("\n\t{0}: \"{1}\"", eventParam.Key, eventParam.Value ?? "null");
			}
		}
		else
		{
			text = "<empty>";
		}
		Impl.LogEvent(eventTypeId, eventParams);
	}

	public static void SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose)
	{
		Impl.SetSessionReportsOnCloseEnabled(sendSessionReportsOnClose);
	}

	public static void SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled)
	{
		Impl.SetSessionReportsOnPauseEnabled(setSessionReportsOnPauseEnabled);
	}

	public static void SetEventLoggingEnabled(bool value)
	{
		Impl.SetEventLoggingEnabled(value);
	}
}
