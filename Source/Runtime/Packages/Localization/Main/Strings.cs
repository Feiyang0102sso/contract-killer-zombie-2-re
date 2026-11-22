using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Glu.Localization.Config;
using Glu.Localization.Internal;
using Glu.Localization.StreamGetters;
using Glu.Localization.Utils;
using UnityEngine;

namespace Glu.Localization
{
	public static class Strings
	{
		private static bool initialized;

		private static string locale;

		private static LocalizationManager manager;

		private static LocaleListenerManager managers;

		private static LocaleListenerManager listeners;

		public static string Locale
		{
			get
			{
				Initialize();
				return locale;
			}
			set
			{
				locale = value;
				Initialize();
				managers.NotifyListeners(locale);
				listeners.NotifyListeners(locale);
			}
		}

		public static LocalizationManager Manager
		{
			get
			{
				InitializeMainLocalizationManager();
				return manager;
			}
			set
			{
				manager = value;
			}
		}

		public static string GetString(string id)
		{
			InitializeMainLocalizationManager();
			return manager.GetString(id);
		}

		public static string GetParticularString(string context, string id)
		{
			InitializeMainLocalizationManager();
			return manager.GetParticularString(context, id);
		}

		public static string GetStringFormat(string id, params object[] args)
		{
			InitializeMainLocalizationManager();
			return string.Format(manager.GetString(id), args);
		}

		public static string GetParticularStringFormat(string context, string id, params object[] args)
		{
			InitializeMainLocalizationManager();
			return string.Format(manager.GetParticularString(context, id), args);
		}

		[Conditional("o_O")]
		public static void MarkString(string id)
		{
		}

		[Conditional("o_O")]
		public static void MarkParticularString(string context, string id)
		{
		}

		public static void Initialize()
		{
			if (initialized)
			{
				return;
			}
			if (string.IsNullOrEmpty(locale))
			{
				locale = DetectLocale();
			}
			managers = new LocaleListenerManager();
			listeners = new LocaleListenerManager();
			initialized = true;
		}

		public static void Destroy()
		{
			manager = null;
			locale = null;
			listeners = null;
			managers = null;
			initialized = false;
			RuntimeConfigManager.ReplaceConfig(null);
		}

		public static void RegisterLocaleListener(ILocaleListener listener)
		{
			Initialize();
			LocaleListenerManager localeListenerManager = (listener is LocalizationManager ? managers : listeners);
			localeListenerManager.RegisterListener(listener);
		}

		public static void UnregisterLocaleListener(ILocaleListener listener)
		{
			Initialize();
			LocaleListenerManager localeListenerManager = (listener is LocalizationManager ? managers : listeners);
			localeListenerManager.UnregisterListener(listener);
		}

		private static void InitializeMainLocalizationManager()
		{
			if (!(manager == null))
			{
				return;
			}
			Initialize();
			RuntimeConfig instance = RuntimeConfigManager.Instance;
			LocalizationManager localizationManager = new LocalizationManager();
			localizationManager.StreamGetter = CreateGroupStreamGetter(instance.Locations);
			localizationManager.HandleLocaleChanged(locale);
			RegisterLocaleListener(localizationManager);
			manager = localizationManager;
		}

		private static IStreamGetter CreateGroupStreamGetter(IList<PoLocation> locations)
		{
			GroupStreamGetter groupStreamGetter = new GroupStreamGetter();
			int count = locations.Count;
			for (int i = 0; i < count; i++)
			{
				PoLocation location = locations[i];
				IStreamGetter streamGetter = CreateStreamGetter(location);
				groupStreamGetter.AddChild(streamGetter);
			}
			return groupStreamGetter;
		}

		private static IStreamGetter CreateStreamGetter(PoLocation location)
		{
			string baseDirectory = location.BaseDirectory;
			string dataPath;
			switch (location.Source)
			{
			case ResourceSource.StreamingAssets:
			{
				dataPath = Application.dataPath;
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					baseDirectory = dataPath + "/Raw";
				}
				if (Application.platform == RuntimePlatform.Android)
				{
					baseDirectory = string.Format("jar:file://{0}!/assets", dataPath);
					throw new NotSupportedException("Have to use WWW in stream getter");
				}
				baseDirectory = dataPath + "/StreamingAssets";
				return new FileStreamGetter
				{
					BaseDir = baseDirectory
				};
			}
			case ResourceSource.AssetBundles:
			{
				return new AssetBundlesStreamGetter
				{
					BaseDir = baseDirectory
				};
			}
			case ResourceSource.Resources:
			{
				return new ResourcesStreamGetter
				{
					BaseDir = baseDirectory
				};
			}
			default:
				break;
			}
			throw new NotSupportedException(string.Format("Not supported source {0}", location.Source));
		}

		private static string DetectLocale()
		{
			int unusedLocal1;
			byte[] array = new byte[32];
			int num = Glu_Localization_GetLocale(array.Length, array);
			if (num > 0)
			{
				return Encoding.UTF8.GetString(array, 0, num);
			}
			return LanguageTable.GetIso639FromLanguage(Application.systemLanguage);
		}

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern int Glu_Localization_GetLocale(int size, [Out][MarshalAs(UnmanagedType.LPArray)] byte[] buf);
#else
		private static int Glu_Localization_GetLocale(int size, byte[] buf)
		{
			return NonIosNative.Glu_Localization_GetLocale(size, buf);
		}
#endif
	}
}
