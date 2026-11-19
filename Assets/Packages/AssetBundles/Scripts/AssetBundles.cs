using System;
using UnityEngine;

namespace Glu.AssetBundles
{
	public static class AssetBundles
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles");
			}
		}

		private static Version m_version = null;

		public static Version version
		{
			get
			{
				return m_version ?? (m_version = new Version(0, 9, 6));
			}
		}

		public static IndexInfo DownloadAll(params string[] urls)
		{
			var result = IndexInfo.GetInstance(urls);
			return result;
		}

		public static bool Contains(string assetPath)
		{
			bool flag = false;
			foreach (IndexInfo indexInfo in IndexInfo.Instances)
			{
				flag = indexInfo.Contains(assetPath);
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		public static UnityEngine.Object Load(string assetPath)
		{
			UnityEngine.Object obj = null;
			if (obj == null)
			{
				foreach (IndexInfo indexInfo in IndexInfo.Instances)
				{
					obj = indexInfo.Load(assetPath);
					if (obj != null)
					{
						break;
					}
				}
			}
			if (obj == null)
			{
			}
			return obj;
		}

		public static UnityEngine.Object Load(string assetPath, Type assetType)
		{
			UnityEngine.Object obj = null;
			if (obj == null)
			{
				foreach (IndexInfo indexInfo in IndexInfo.Instances)
				{
					obj = indexInfo.Load(assetPath, assetType);
					if (obj != null)
					{
						break;
					}
				}
			}
			if (obj == null)
			{
			}
			return obj;
		}

		public static void UnloadAll(bool unloadAllLoadedObjects)
		{
			foreach (IndexInfo indexInfo in IndexInfo.Instances)
			{
				indexInfo.UnloadAll(unloadAllLoadedObjects);
			}
		}
	}
}
