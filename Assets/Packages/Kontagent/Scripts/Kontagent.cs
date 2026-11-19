using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Glu.UnityBuildSystem;

namespace Glu.Kontagent
{
	public static class Kontagent
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.Kontagent");
			}
		}

		private static Regex _alphaNumericSubRegex = new Regex("^[A-Za-z0-9_-]*$");

		private static bool _initialized = false;

		[DllImport("__Internal")]
		private static extern void KontagentStartSession(string apiKey, bool testMode, bool trusted, string integrityDetails, string appVersion);

		[DllImport("__Internal")]
		private static extern void KontagentCustomEvent(string name, string st1, string st2, string st3, string level, string val, string data);

		[DllImport("__Internal")]
		private static extern void KontagentRevenueTracking(int val);

		[DllImport("__Internal")]
		private static extern IntPtr KontagentGetUserID();

		private static string KontagentGetUserIDString()
		{
			return Marshal.PtrToStringAnsi(KontagentGetUserID());
		}

		private static string DictionaryToJSON(Dictionary<string, string> dict)
		{
			if (dict == null)
			{
				return null;
			}
			string text = "{";
			foreach (KeyValuePair<string, string> item in dict)
			{
				if (text != "{")
				{
					text += ",";
				}
				text += "\"";
				text += item.Key;
				text += "\":\"";
				text += item.Value;
				text += "\"";
			}
			text += "}";
			if (text == "{}")
			{
				text = null;
			}
			return text;
		}

		private static void CheckStringParam(string paramName, string param)
		{
			if (!_alphaNumericSubRegex.IsMatch(param))
			{
			}
			if (param.Length > 32)
			{
			}
		}

		public static void StartSession(string apiKey)
		{
			StartSession(apiKey, false);
		}

		public static void StartSession(string apiKey, bool testMode)
		{
			KontagentStartSession(apiKey, testMode, !Integrity.IsJailbroken(), DictionaryToJSON(Integrity.GetDetails()), BuildInfo.bundleVersion);
			_initialized = true;
		}

		public static void LogEvent(string name, string st1, string st2, string st3, int? level, int? val)
		{
			LogEvent(name, st1, st2, st3, level, val, null);
		}

		public static void LogEvent(string name, string st1, string st2, string st3, int? level, int? val, Dictionary<string, string> data)
		{
			if (!_initialized)
			{
				return;
			}
			CheckStringParam("name", name);
			CheckStringParam("st1", st1);
			if (st2 != null)
			{
				CheckStringParam("st2", st2);
			}
			if (st3 != null)
			{
				CheckStringParam("st3", st3);
			}
			string text = level.HasValue ? level.Value.ToString() : null;
			if (text == null || level.Value < 0 || level.Value > 255)
			{
			}
			string text2 = val.HasValue ? val.Value.ToString() : null;
			if (text2 != null)
			{
			}
			KontagentCustomEvent(name, st1, st2, st3, text, text2, DictionaryToJSON(data));
		}

		public static void RevenueTracking(int val)
		{
			if (!_initialized)
			{
				return;
			}
			if (!Integrity.IsJailbroken())
			{
				KontagentRevenueTracking(val);
			}
		}

		public static string GetUserID()
		{
			return KontagentGetUserIDString();
		}
	}
}
