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

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void CFlurry_SetAppVersion(string version);
#else
		private static void CFlurry_SetAppVersion(string version)
		{
			PcNative.Stub("CFlurry_SetAppVersion");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern string CFlurry_GetFlurryAgentVersion();
#else
		private static string CFlurry_GetFlurryAgentVersion()
		{
			PcNative.Stub("CFlurry_GetFlurryAgentVersion");
			return default(string);
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetShowErrorInLogEnabled(bool value);
#else
		private static void CFlurry_SetShowErrorInLogEnabled(bool value)
		{
			PcNative.Stub("CFlurry_SetShowErrorInLogEnabled");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetDebugLogEnabled(bool value);
#else
		private static void CFlurry_SetDebugLogEnabled(bool value)
		{
			PcNative.Stub("CFlurry_SetDebugLogEnabled");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetSessionContinueSeconds(int seconds);
#else
		private static void CFlurry_SetSessionContinueSeconds(int seconds)
		{
			PcNative.Stub("CFlurry_SetSessionContinueSeconds");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetSecureTransportEnabled(bool value);
#else
		private static void CFlurry_SetSecureTransportEnabled(bool value)
		{
			PcNative.Stub("CFlurry_SetSecureTransportEnabled");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern IntPtr CFlurry_GetUserID();
#else
		private static IntPtr CFlurry_GetUserID()
		{
			PcNative.Stub("CFlurry_GetUserID");
			return default(IntPtr);
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void CFlurry_StartSession(string apiKey, bool trusted, string[] keys, string[] values);
#else
		private static void CFlurry_StartSession(string apiKey, bool trusted, string[] keys, string[] values)
		{
			PcNative.Stub("CFlurry_StartSession");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal", CharSet = CharSet.Ansi)]
		private static extern void CFlurry_LogEvent(string eventTypeId, string[] keys, string[] values);
#else
		private static void CFlurry_LogEvent(string eventTypeId, string[] keys, string[] values)
		{
			PcNative.Stub("CFlurry_LogEvent");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose);
#else
		private static void CFlurry_SetSessionReportsOnCloseEnabled(bool sendSessionReportsOnClose)
		{
			PcNative.Stub("CFlurry_SetSessionReportsOnCloseEnabled");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled);
#else
		private static void CFlurry_SetSessionReportsOnPauseEnabled(bool setSessionReportsOnPauseEnabled)
		{
			PcNative.Stub("CFlurry_SetSessionReportsOnPauseEnabled");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void CFlurry_SetEventLoggingEnabled(bool value);
#else
		private static void CFlurry_SetEventLoggingEnabled(bool value)
		{
			PcNative.Stub("CFlurry_SetEventLoggingEnabled");
		}
#endif
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
