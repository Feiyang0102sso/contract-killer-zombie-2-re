using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Glu.AssetBundles.Internal;
using UnityEngine;

namespace Glu.AssetBundles
{
	public class DownloadManager : MonoBehaviour
	{
		public class Info
		{
			public enum State
			{
				NotStarted,
				Pending,
				Sending,
				Receiving,
				WaitingBeforeRetry,
				Downloaded,
				Abandoned,
				DoNotDownload
			}

			public enum CacheStatus
			{
				Undefined,
				NotCached,
				Breakdown,
				JustCached,
				AlreadyCached
			}

			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.DownloadManager.Info");
				}
			}

			private Download m_download;

			public State state
			{
				get
				{
					State result = State.NotStarted;
					switch (m_download.state)
					{
					case Download.State.NotStarted:
						result = State.NotStarted;
						break;
					case Download.State.Loading:
						result = www != null
							? (www.progress > 0f ? State.Receiving :
								www.uploadProgress > 0f ? State.Sending : State.Pending)
							: State.Pending;
						break;
					case Download.State.WaitingBeforeRetry:
						result = State.WaitingBeforeRetry;
						break;
					case Download.State.Downloaded:
						result = State.Downloaded;
						break;
					case Download.State.Abandoned:
						result = State.Abandoned;
						break;
					case Download.State.DoNotDownload:
						result = State.DoNotDownload;
						break;
					default:
						break;
					}
					return result;
				}
			}

			public CacheStatus cacheStatus
			{
				get
				{
					CacheStatus result = CacheStatus.NotCached;
					switch (m_download.cacheStatus)
					{
					case Download.CacheStatus.Undefined:
						result = CacheStatus.Undefined;
						break;
					case Download.CacheStatus.NotCached:
						result = CacheStatus.NotCached;
						break;
					case Download.CacheStatus.Breakdown:
						result = CacheStatus.Breakdown;
						break;
					case Download.CacheStatus.JustCached:
						result = CacheStatus.JustCached;
						break;
					case Download.CacheStatus.AlreadyCached:
						result = CacheStatus.AlreadyCached;
						break;
					default:
						break;
					}
					return result;
				}
			}

			public string[] Urls
			{
				get
				{
					return m_download.Urls;
				}
			}

			public float SendingProgress
			{
				get
				{
					return !(www == null) ? www.uploadProgress : 0f;
				}
			}

			public float ReceivingProgress
			{
				get
				{
					return state == State.Downloaded ? 1f : (www != null ? www.progress : 0f);
				}
			}

			public string LatestUrl
			{
				get
				{
					return !(www == null) ? www.url.Replace('\\', '/') : null;
				}
			}

			public string LatestError
			{
				get
				{
					return !(www == null) ? www.error : null;
				}
			}

			public AssetBundle assetBundle
			{
				get
				{
					AssetBundle result = null;
					if (state == State.Downloaded)
					{
						if (m_download.OfflineAssetBundle != null)
						{
							result = m_download.OfflineAssetBundle;
						}
						else
						{
							result = (!(www == null) ? www.assetBundle : null);
						}
					}
					return result;
				}
			}

			public byte[] bytes
			{
				get
				{
					return !(state != State.Downloaded || version >= 0) ? www.bytes : null;
				}
			}

			public string text
			{
				get
				{
					return !(state != State.Downloaded || version >= 0) ? www.text : null;
				}
			}

			public static Info Retrieve(string guid)
			{
				Info result = null;
				Download download = FindDownloadByGuid(guid);
				if (download != null)
				{
					result = new Info(download);
				}
				return result;
			}

			public void Skip()
			{
				m_download.Skip();
			}

			private Info(Download download)
			{
				m_download = download;
			}

			private int version
			{
				get
				{
					return m_download.version;
				}
			}

			private WWW www
			{
				get
				{
					return m_download.www;
				}
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.DownloadManager");
			}
		}

		private class Download
		{
			public enum State
			{
				NotStarted,
				Loading,
				WaitingBeforeRetry,
				Downloaded,
				Abandoned,
				DoNotDownload
			}

			public enum CacheStatus
			{
				Undefined,
				NotCached,
				Breakdown,
				JustCached,
				AlreadyCached
			}

			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.DownloadManager.Download");
				}
			}

			private string m_guid;

			private List<string> m_urls;

			private int m_version;

			private OnDownloadCallbackType m_onDownloadCallback;

			private WWW m_www;

			private AssetBundle m_offlineAssetBundle;

			private State m_state;

			private CacheStatus m_cacheStatus;

			public string GUID
			{
				get
				{
					return m_guid;
				}
			}

			public string[] Urls
			{
				get
				{
					return m_urls.ToArray();
				}
			}

			public int version
			{
				get
				{
					return m_version;
				}
			}

			public OnDownloadCallbackType onDownloadCallback
			{
				get
				{
					return m_onDownloadCallback;
				}
			}

			public WWW www
			{
				set
				{
					m_www = value;
				}
				get
				{
					return m_www;
				}
			}

			public AssetBundle OfflineAssetBundle
			{
				set
				{
					m_offlineAssetBundle = value;
				}
				get
				{
					return m_offlineAssetBundle;
				}
			}

			public State state
			{
				set
				{
					m_state = value;
				}
				get
				{
					return m_state;
				}
			}

			public CacheStatus cacheStatus
			{
				set
				{
					m_cacheStatus = value;
				}
				get
				{
					return m_cacheStatus;
				}
			}

			public Download(string[] urls, int version, OnDownloadCallbackType onDownloadCallback)
			{
				m_guid = Guid.NewGuid().ToString();
				m_urls = new List<string>(urls.Length);
				m_urls.Clear();
				foreach (string item in urls)
				{
					m_urls.Add(item);
				}
				m_version = version;
				m_onDownloadCallback = onDownloadCallback;
				m_www = null;
				m_offlineAssetBundle = null;
				m_state = State.NotStarted;
				m_cacheStatus = CacheStatus.Undefined;
			}

			public void Skip()
			{
				if (state == State.NotStarted)
				{
					m_state = State.DoNotDownload;
				}
			}
		}

		public delegate bool OnDownloadCallbackType(Info downloadInfo);

		public const int VERSION_FORCE_REDOWNLOAD = int.MinValue;

		private const string kGameObjectName = "Glu.AssetBundles.DownloadManager GameObject";

		private static DownloadManager m_instance = null;

		private static int m_threadCount = 1;

		private static int m_triesCount = 1;

		private static int m_delayBeforeRetry = 30;

		private static List<Download> m_downloads = new List<Download>();

		private static Dictionary<int, bool> m_downloadCoroutines = new Dictionary<int, bool>();

		public static int ThreadCount
		{
			set
			{
				m_threadCount = value;
			}
			get
			{
				return m_threadCount;
			}
		}

		public static int ActiveThreads
		{
			get
			{
				return m_downloadCoroutines.Count;
			}
		}

		public static int TriesCount
		{
			set
			{
				m_triesCount = value;
			}
			get
			{
				return m_triesCount;
			}
		}

		public static int DelayBeforeRetry
		{
			set
			{
				m_delayBeforeRetry = value;
			}
			get
			{
				return m_delayBeforeRetry;
			}
		}

		public static Info LoadFromCacheOrDownloadAsync(string[] urls, int version, OnDownloadCallbackType onDownloadCallback)
		{
			foreach (string text in urls)
			{
			}
			string text2 = string.Empty;
			foreach (string arg in urls)
			{
				text2 += string.Format("{0}{1}", string.IsNullOrEmpty(text2) ? string.Empty : "\n", arg);
			}
			Info info = null;
			if (urls != null && urls.Length > 0)
			{
				for (int k = 0; k < urls.Length; k++)
				{
					urls[k] = urls[k].Replace('\\', '/');
				}
				Download download = FindDownloadByUrl(urls[0]);
				if (download == null)
				{
					download = new Download(urls, version, onDownloadCallback);
					m_downloads.Add(download);
				}
				else
				{
				}
				info = Info.Retrieve(download.GUID);
				EnsureDownloadCoroutines();
			}
			if (info == null)
			{
			}
			return info;
		}

		public static bool IsCached(string filenameOrUrl, int version)
		{
			bool result = false;
			if (version >= 0)
			{
				result = Caching.IsVersionCached(filenameOrUrl, version);
			}
			return result;
		}

		private static DownloadManager GetInstance()
		{
			if (m_instance != null)
			{
				return m_instance;
			}
			GameObject gameObject = GameObject.Find("Glu.AssetBundles.DownloadManager GameObject");
			if (gameObject != null)
			{
				m_instance = gameObject.GetComponent<GluAssetBundlesDownloadManager>();
				return m_instance;
			}
			gameObject = new GameObject("Glu.AssetBundles.DownloadManager GameObject");
			gameObject.AddComponent<GluAssetBundlesDownloadManager>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			m_instance = gameObject.GetComponent<GluAssetBundlesDownloadManager>();
			m_instance.useGUILayout = false;
			m_instance.enabled = false;
			return m_instance;
		}

		private static void EnsureDownloadCoroutines()
		{
			for (int i = 0; i < ThreadCount; i++)
			{
				bool value;
				if (m_downloadCoroutines.TryGetValue(i, out value))
				{
					if (!value)
					{
						m_downloadCoroutines.Remove(i);
					}
				}
				else
				{
					value = false;
				}
				if (!value)
				{
					m_downloadCoroutines.Add(i, true);
					GetInstance().StartCoroutine(GetInstance().DownloadCoroutine(i));
				}
			}
		}

		private static string HackUrl(string url)
		{
			return url;
		}

		private static bool ShouldAssetBundleBeTriedAsUncompressed(string filename)
		{
			var result = true;
			return result;
		}

		private static Download FindDownloadByGuid(string guid)
		{
			Download result = null;
			for (int num = m_downloads.Count - 1; num >= 0; num--)
			{
				Download download = m_downloads[num];
				if (download.GUID.Equals(guid))
				{
					result = download;
					break;
				}
			}
			return result;
		}

		private static Download FindDownloadByUrl(string url)
		{
			Download download = null;
			for (int num = m_downloads.Count - 1; num >= 0; num--)
			{
				Download download2 = m_downloads[num];
				foreach (string text in download2.Urls)
				{
					if (text.Equals(url))
					{
						download = download2;
						break;
					}
				}
				if (download != null)
				{
					break;
				}
			}
			return download;
		}

		private IEnumerator DownloadCoroutine(int threadNumber)
		{
			Logger.Debug("Starting download coroutine {0}", threadNumber);
			yield return null;
			int idx = 0;
			while (idx < m_downloads.Count)
			{
				Download download = m_downloads[idx];
				if (download.state == Download.State.NotStarted)
				{
					bool downloaded = false;
					bool cacheBreakdown = false;
					int urlIndex = 0;
					for (urlIndex = 0; urlIndex < download.Urls.Length; urlIndex++)
					{
						int attempt = 0;
						for (attempt = 0; attempt < TriesCount; attempt++)
						{
							if (attempt > 0)
							{
								yield return new WaitForSeconds(DelayBeforeRetry);
							}
							if (RTUtils.UncompressedAssetBundlesAllowed)
							{
								try
								{
									string filename = download.Urls[urlIndex].Substring(download.Urls[urlIndex].IndexOf("://") + 3);
									if (download.version >= 0 && File.Exists(filename) && ShouldAssetBundleBeTriedAsUncompressed(filename))
									{
										download.OfflineAssetBundle = AssetBundle.LoadFromFile(filename);
										downloaded = download.OfflineAssetBundle != null;
										if (downloaded)
										{
											download.cacheStatus = Download.CacheStatus.NotCached;
											break;
										}
									}
								}
								catch (Exception e)
								{
									Logger.Warn("Unable to inspect the asset bundle cache: {0}", e);
								}
							}
							if (download.version >= 0)
							{
								download.state = Download.State.Loading;
								bool warned = false;
								warned = false;
								while (!Caching.ready)
								{
									if (!warned)
									{
										warned = true;
									}
									yield return null;
								}
								bool alreadyCached = Caching.IsVersionCached(download.Urls[urlIndex], download.version);
								download.www = WWW.LoadFromCacheOrDownload(download.Urls[urlIndex], download.version);
								yield return StartCoroutine(WWWCoroutine(download.www));
								if (WWWSucceeded(download.www))
								{
									if (Caching.IsVersionCached(download.Urls[urlIndex], download.version))
									{
										download.cacheStatus = alreadyCached ? Download.CacheStatus.AlreadyCached : Download.CacheStatus.JustCached;
										downloaded = true;
										break;
									}
									download.cacheStatus = Download.CacheStatus.Breakdown;
									cacheBreakdown = true;
								}
								else
								{
									download.cacheStatus = Download.CacheStatus.NotCached;
								}
								try
								{
									AssetBundle assetBundle = download.www.assetBundle;
									if (assetBundle != null)
									{
										assetBundle.Unload(true);
									}
									else
									{
									}
								}
								catch (Exception e)
								{
									Logger.Warn("Unable to unload the asset bundle: {0}", e);
								}
							}
							else if (download.version == int.MinValue)
							{
								download.state = Download.State.Loading;
								download.www = new WWW(HackUrl(download.Urls[urlIndex]));
								yield return StartCoroutine(WWWCoroutine(download.www));
								if (WWWSucceeded(download.www))
								{
									downloaded = true;
									break;
								}
							}
							else
							{
							}
							if (download.www != null)
							{
								try
								{
									download.www.Dispose();
								}
								catch (Exception e)
								{
									Logger.Warn("Unable to dispose the download: {0}", e);
								}
								finally
								{
									download.www = null;
								}
							}
							download.state = Download.State.WaitingBeforeRetry;
							if (cacheBreakdown)
							{
								attempt++;
								break;
							}
						}
						if (downloaded)
						{
							download.state = Download.State.Downloaded;
							if (download.onDownloadCallback != null)
							{
								downloaded = download.onDownloadCallback(Info.Retrieve(download.GUID));
							}
							if (downloaded)
							{
								break;
							}
						}
						if (!downloaded)
						{
							download.state = Download.State.WaitingBeforeRetry;
						}
						if (cacheBreakdown)
						{
							urlIndex++;
							break;
						}
					}
					if (!downloaded)
					{
						download.state = Download.State.Abandoned;
						if (download.onDownloadCallback != null)
						{
							download.onDownloadCallback(Info.Retrieve(download.GUID));
						}
					}
					m_downloads.Remove(download);
					idx = 0;
				}
				else if (download.state == Download.State.DoNotDownload)
				{
					m_downloads.RemoveAt(idx);
				}
				else
				{
					idx++;
				}
				if (threadNumber >= ThreadCount)
				{
					break;
				}
				yield return null;
			}
			m_downloadCoroutines.Remove(threadNumber);
		}

		private IEnumerator WWWCoroutine(WWW www)
		{
			double unusedLocal1;
			DateTime dateTime = DateTime.Now;
			float progress = www.progress;
			while (true)
			{
				if (www.isDone || !string.IsNullOrEmpty(www.error))
				{
					break;
				}
				if (!www.url.ToLower().StartsWith("file://"))
				{
					if (www.progress > progress)
					{
						dateTime = DateTime.Now;
						progress = www.progress;
					}
					else if (www.progress < 1f && (DateTime.Now - dateTime).TotalSeconds > 5.0)
					{
						break;
					}
				}
				yield return null;
			}
		}

		private bool WWWSucceeded(WWW www)
		{
			bool flag = www.isDone && string.IsNullOrEmpty(www.error);
			if (flag)
			{
				try
				{
					Dictionary<string, string> responseHeaders = www.responseHeaders;
					string text = responseHeaders["STATUS"];
					string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (array[0].ToUpper().Contains("HTTP"))
					{
						int num = (int)Convert.ChangeType(array[1], typeof(int));
						flag = num < 400;
					}
				}
				catch (Exception e)
				{
				}
			}
			return flag;
		}
	}
}
