using System;
using System.Collections;
using System.Collections.Generic;
using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.UnityBuildSystem;
using UnityEngine;

namespace Glu.DynamicContentPipeline
{
	public static class DynamicContent
	{
		public class CheckReport
		{
			public enum UpdateScope
			{
				All,
				ForcedUpdate,
				ABTesting,
				AssetBundles,
				CustomDynamicContent,
				Nothing
			}

			public UpdateScope updateScope { private set; get; }

			public Glu.ABTesting.Resolution ChangedABTestingResolution { private set; get; }

			public ICustomDynamicContent[] ChangedCustomDynamicContent { private set; get; }

			public CheckReport(UpdateScope updateScope)
			{
				this.updateScope = updateScope;
			}

			public CheckReport(UpdateScope updateScope, Glu.ABTesting.Resolution resolution)
				: this(updateScope)
			{
				ChangedABTestingResolution = resolution;
			}

			public CheckReport(UpdateScope updateScope, ICustomDynamicContent[] cdcs)
				: this(updateScope)
			{
				ChangedCustomDynamicContent = cdcs;
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.DynamicContent");
			}
		}

		public class Impl : MonoBehaviour
		{
			private static class FlurryEvents
			{
				private static long m_nAttempt = 0L;

				private static DateTime m_overallTime0 = DateTime.MaxValue;

				private static DateTime m_forcedUpdateTime0 = DateTime.MaxValue;

				private static DateTime m_aBTestingTime0 = DateTime.MaxValue;

				private static DateTime m_assetBundlesTime0 = DateTime.MaxValue;

				private static DateTime m_customContentTime0 = DateTime.MaxValue;

				private static long m_totalSizeInBytes = 0L;

				public static void LogDCPStarted()
				{
					m_nAttempt++;
					m_overallTime0 = DateTime.UtcNow;
					m_totalSizeInBytes = 0L;
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("No.", m_nAttempt);
					CFlurry.LogEvent("DCP_UPDATE_START", dictionary);
				}

				public static void LogDCPFinished(bool success)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("result", success ? "success" : "fail");
					double spentSeconds = GetSpentSeconds(m_overallTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					dictionary.Add("download_size_kbytes", m_totalSizeInBytes / 1024);
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_UPDATE_END", dictionary);
				}

				public static void LogForcedUpdateStarted()
				{
					m_forcedUpdateTime0 = DateTime.UtcNow;
					Dictionary<string, object> eventParams = new Dictionary<string, object>();
					CFlurry.LogEvent("DCP_FORCED_UPDATE_START", eventParams);
				}

				public static void LogForcedUpdateFinished(bool firedTimeout)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("result", ForcedUpdate.NeedToQuit() ? "needToQuit" : ForcedUpdate.NeedToUpdate() ? "needToUpdate" : "upToDate");
					dictionary.Add("fired_timeout", firedTimeout);
					double spentSeconds = GetSpentSeconds(m_forcedUpdateTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_FORCED_UPDATE_END", dictionary);
				}

				public static void LogABTestingStarted()
				{
					m_aBTestingTime0 = DateTime.UtcNow;
					Dictionary<string, object> eventParams = new Dictionary<string, object>();
					CFlurry.LogEvent("DCP_ABTESTING_RULES_START", eventParams);
				}

				public static void LogABTestingFinished(Glu.ABTesting.Resolution resolution)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("variant_applied", resolution.IsApplied);
					dictionary.Add("variant_id", resolution.VariantId);
					double spentSeconds = GetSpentSeconds(m_aBTestingTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_ABTESTING_RULES_END", dictionary);
				}

				public static void LogAssetBundlesStarted(string urls)
				{
					m_assetBundlesTime0 = DateTime.UtcNow;
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("url", urls ?? "none");
					CFlurry.LogEvent("DCP_BUNDLES_START", dictionary);
				}

				public static void LogAssetBundlesFinished(IndexInfo indexInfo)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("result", indexInfo.state == IndexInfo.State.Succeeded ? "success" : "fail");
					dictionary.Add("source", indexInfo.source);
					dictionary.Add("loaded_tag", indexInfo.BuildTag ?? "none");
					dictionary.Add("cached_hash", indexInfo.CachedContentHash ?? "none");
					dictionary.Add("loaded_hash", indexInfo.ContentHash ?? "none");
					double spentSeconds = GetSpentSeconds(m_assetBundlesTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					long num = 0L;
					if (indexInfo.assetBundleInfo != null)
					{
						foreach (AssetBundleInfo assetBundleInfo2 in indexInfo.assetBundleInfo)
						{
							if (assetBundleInfo2.downloadInfo.cacheStatus == DownloadManager.Info.CacheStatus.JustCached)
							{
								num += assetBundleInfo2.Size;
							}
						}
						m_totalSizeInBytes += num;
					}
					dictionary.Add("download_size_kbytes", num / 1024);
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_BUNDLES_END", dictionary);
				}

				public static void LogCustomContentStarted()
				{
					if (CustomDynamicContent.Count > 0)
					{
						m_customContentTime0 = DateTime.UtcNow;
						Dictionary<string, object> eventParams = new Dictionary<string, object>();
						CFlurry.LogEvent("DCP_CONFIGS_START", eventParams);
					}
				}

				public static void LogCustomContentFinished(bool success, params ICustomDynamicContent[] customDynamicContentsToLog)
				{
					ICustomDynamicContent[] array = customDynamicContentsToLog ?? CustomDynamicContent.ToArray();
					if (array.Length > 0)
					{
						Dictionary<string, object> dictionary = new Dictionary<string, object>();
						dictionary.Add("result", success ? "success" : "fail");
						double spentSeconds = GetSpentSeconds(m_customContentTime0);
						dictionary.Add("time_seconds", Round(spentSeconds));
						dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
						long num = 0L;
						foreach (ICustomDynamicContent customDynamicContent in array)
						{
							num += customDynamicContent.LastUpdateSize;
						}
						m_totalSizeInBytes += num;
						dictionary.Add("download_size_kbytes", num / 1024);
						dictionary.Add("network_type", GetNetworkType());
						CFlurry.LogEvent("DCP_CONFIGS_END", dictionary);
					}
				}

				private static double GetSpentSeconds(DateTime dateTimeFrom)
				{
					double result = 0.0;
					DateTime utcNow = DateTime.UtcNow;
					if (dateTimeFrom <= utcNow)
					{
						result = (DateTime.UtcNow - dateTimeFrom).TotalSeconds;
					}
					return result;
				}

				private static int Round(double seconds)
				{
					int result = -1;
					try
					{
						result = Convert.ToInt32(seconds);
					}
					catch (Exception)
					{
					}
					return result;
				}

				private static string GetTimeInterval(double seconds)
				{
					string text = null;
					int[] array = new int[10] { 0, 2, 5, 10, 30, 60, 120, 180, 240, 300 };
					for (int i = 1; i < array.Length; i++)
					{
						if (seconds < (double)array[i])
						{
							text = string.Format("{0}:{1}", array[i - 1], array[i]);
							break;
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						text = string.Format("{0}+", array[array.Length - 1]);
					}
					return text;
				}

				private static string GetNetworkType()
				{
					string text = null;
					switch (Application.internetReachability)
					{
					case NetworkReachability.NotReachable:
						text = "Offline";
						break;
					case NetworkReachability.ReachableViaCarrierDataNetwork:
						text = "Cell";
						break;
					case NetworkReachability.ReachableViaLocalAreaNetwork:
						text = "Wi-Fi";
						break;
					default:
						text = Application.internetReachability.ToString();
						break;
					}
					return text;
				}
			}

			private const string kGameObjectName = "Glu.DynamicContentPipeline.DynamicContent GameObject";

			private const double kForcedUpdateTimeOut = 5.0;

			private static Impl m_instance = null;

			public static void StartContentUpdate()
			{
				if (m_isLoadingInProgress)
				{
					m_isLoadingInProgress = false;
					Instance.StopCoroutine("DoContentUpdate");
				}
				if (m_isCheckingInProgress)
				{
					m_isCheckingInProgress = false;
					Instance.StopCoroutine("DoCheckForUpdates");
				}
				Reset(true);
				m_isLoadingInProgress = true;
				Instance.StartCoroutine("DoContentUpdate");
			}

			public static bool IsUpdateInProgress()
			{
				var result = m_isLoadingInProgress;
				return result;
			}

			public static void CheckForUpdates()
			{
				if (!m_isLoadingInProgress && !m_isCheckingInProgress)
				{
					Reset(false);
					m_isCheckingInProgress = true;
					Instance.StartCoroutine("DoCheckForUpdates");
				}
			}

			private static Impl Instance
			{
				get
				{
					if (m_instance != null)
					{
						return m_instance;
					}
					GameObject gameObject = GameObject.Find("Glu.DynamicContentPipeline.DynamicContent GameObject");
					if (gameObject != null)
					{
						m_instance = gameObject.GetComponent<GluDynamicContentPipelineDynamicContentImpl>();
						return m_instance;
					}
					gameObject = new GameObject("Glu.DynamicContentPipeline.DynamicContent GameObject");
					gameObject.AddComponent<GluDynamicContentPipelineDynamicContentImpl>();
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					m_instance = gameObject.GetComponent<GluDynamicContentPipelineDynamicContentImpl>();
					m_instance.useGUILayout = false;
					m_instance.enabled = false;
					return m_instance;
				}
			}

			private static string BuildTag
			{
				get
				{
					return BuildInfo.buildTag;
				}
			}

			private static string Platform
			{
				get
				{
					string result = string.Empty;
					switch (Application.platform)
					{
					case RuntimePlatform.Android:
						result = "android";
						break;
					case RuntimePlatform.IPhonePlayer:
						result = "ios";
						break;
					case RuntimePlatform.OSXPlayer:
						result = "mac";
						break;
					case RuntimePlatform.WindowsPlayer:
						result = "win";
						break;
					default:
						break;
					}
					return result;
				}
			}

			private static string[] UrlsToForcedUpdate
			{
				get
				{
					string[] array = new string[m_baseUrls.Length];
					for (int i = 0; i < m_baseUrls.Length; i++)
					{
						array[i] = string.Format("{0}/{1}/ForcedUpdate.xml", m_baseUrls[i], Platform);
					}
					return array;
				}
			}

			private static string[] UrlsToABTestingDecisionTable
			{
				get
				{
					string[] array = new string[m_baseUrls.Length];
					for (int i = 0; i < m_baseUrls.Length; i++)
					{
						array[i] = string.Format("{0}/{1}/{2}/ABTesting.xml", m_baseUrls[i], Platform, BuildTag);
					}
					return array;
				}
			}

			private static string[] UrlsToDefaultAssetBundles
			{
				get
				{
					string[] array = new string[m_baseUrls.Length];
					for (int i = 0; i < m_baseUrls.Length; i++)
					{
						array[i] = string.Format("{0}/{1}/{2}/AssetBundles/{3}.version", m_baseUrls[i], Platform, BuildTag, AssetBundlesIndexName);
					}
					return array;
				}
			}

			private static void Reset(bool resetAll)
			{
				if (resetAll)
				{
					if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ABTesting)
					{
						m_resolution = null;
					}
					if (LastCheckReport.updateScope <= CheckReport.UpdateScope.AssetBundles)
					{
						m_isIndexInfo = false;
					}
				}
				m_updateInfo = null;
			}

			private IEnumerator DoContentUpdate()
			{
				FlurryEvents.LogDCPStarted();
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ForcedUpdate)
				{
					FlurryEvents.LogForcedUpdateStarted();
					ForcedUpdate.Init(UrlsToForcedUpdate[0], BuildTag);
					ForcedUpdate.CheckUpdateStatus();
					DateTime dateTime0 = DateTime.UtcNow;
					bool firedTimeout = false;
					while (ForcedUpdate.IsCheckInProgress())
					{
						if ((DateTime.UtcNow - dateTime0).TotalSeconds > 5.0)
						{
							firedTimeout = true;
							break;
						}
						yield return null;
					}
					FlurryEvents.LogForcedUpdateFinished(firedTimeout);
					if (ForcedUpdate.NeedToQuit())
					{
						LastCheckReport = new CheckReport(CheckReport.UpdateScope.ForcedUpdate);
						if (OnForcedBinariesUpdate != null)
						{
							try
							{
								OnForcedBinariesUpdate();
							}
							catch (Exception e)
							{
								Logger.Warn("Forced update callback failed: {0}", e);
							}
						}
						m_isLoadingInProgress = false;
						yield break;
					}
					if (ForcedUpdate.NeedToUpdate())
					{
						if (OnAvailableBinariesUpdate != null)
						{
							try
							{
								if (OnAvailableBinariesUpdate())
								{
									LastCheckReport = new CheckReport(CheckReport.UpdateScope.ForcedUpdate);
									m_isLoadingInProgress = false;
									yield break;
								}
							}
							catch (Exception e)
							{
								Logger.Warn("Dynamic content update callback failed: {0}", e);
							}
						}
					}
					else
					{
					}
				}
				else
				{
				}
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ABTesting)
				{
					if (LastCheckReport.ChangedABTestingResolution == null)
					{
						Dictionary<string, string> initialVariantData = new Dictionary<string, string>();
						initialVariantData.Add("VariantId", "DefaultOffline");
						string str = null;
						foreach (string url in UrlsToDefaultAssetBundles)
						{
							str = str != null ? str + ";" + url : url;
						}
						initialVariantData.Add("AssetBundlesUrls", str);
						FlurryEvents.LogABTestingStarted();
						m_resolution = Glu.ABTesting.Resolution.Retrieve(initialVariantData, UrlsToABTestingDecisionTable);
						while (!m_resolution.Ready)
						{
							yield return null;
						}
					}
					else
					{
						m_resolution = LastCheckReport.ChangedABTestingResolution;
					}
					FlurryEvents.LogABTestingFinished(m_resolution);
				}
				else
				{
				}
				bool success = true;
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.AssetBundles)
				{
					if (m_indexInfo != null)
					{
						m_indexInfo.UnloadAll(true);
					}
					if (m_resolution.Data != null && m_resolution.Data.ContainsKey("AssetBundlesUrls"))
					{
						FlurryEvents.LogAssetBundlesStarted(m_resolution.Data["AssetBundlesUrls"]);
						m_indexInfo = Glu.AssetBundles.AssetBundles.DownloadAll(m_resolution.Data["AssetBundlesUrls"].Split(new string[3] { " ", ",", ";" }, StringSplitOptions.RemoveEmptyEntries));
						m_isIndexInfo = true;
						while (m_indexInfo.state == IndexInfo.State.InProgress)
						{
							yield return null;
						}
						FlurryEvents.LogAssetBundlesFinished(m_indexInfo);
						switch (m_indexInfo.state)
						{
						case IndexInfo.State.Failed:
							success = false;
							break;
						case IndexInfo.State.Succeeded:
							success = true;
							break;
						default:
							break;
						}
					}
					else
					{
						m_indexInfo = null;
						success = true;
					}
				}
				else
				{
				}
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.CustomDynamicContent && success)
				{
					ICustomDynamicContent[] cdcsToUpdate = LastCheckReport.ChangedCustomDynamicContent ?? CustomDynamicContent.ToArray();
					if (cdcsToUpdate.Length > 0)
					{
						FlurryEvents.LogCustomContentStarted();
						foreach (ICustomDynamicContent customDynamicContent in cdcsToUpdate)
						{
							if (customDynamicContent.IsInProgress)
							{
								success = false;
								foreach (ICustomDynamicContent temp in CustomDynamicContent)
								{
									temp.InvalidateLastCheckReport();
								}
								break;
							}
							customDynamicContent.StartContentUpdate(m_resolution);
							while (customDynamicContent.IsInProgress)
							{
								yield return null;
							}
							success &= customDynamicContent.Result;
						}
						FlurryEvents.LogCustomContentFinished(success, cdcsToUpdate);
					}
					else
					{
						success = true;
					}
				}
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ABTesting && success)
				{
					success = m_resolution.Commit();
				}
				FlurryEvents.LogDCPFinished(success);
				if (success)
				{
					LastCheckReport = new CheckReport(CheckReport.UpdateScope.Nothing);
					if (OnSuccess != null)
					{
						try
						{
							OnSuccess();
						}
						catch (Exception e)
						{
							Logger.Warn("Success callback failed: {0}", e);
						}
					}
				}
				else if (OnFail != null)
				{
					try
					{
						OnFail();
					}
					catch (Exception e)
					{
						Logger.Warn("Failure callback failed: {0}", e);
					}
				}
				m_isLoadingInProgress = false;
				yield break;
			}

			private IEnumerator DoCheckForUpdates()
			{
				yield return null;
				CheckReport report = new CheckReport(CheckReport.UpdateScope.Nothing);
				Glu.ABTesting.Resolution resolution = null;
				List<ICustomDynamicContent> cdcsToUpdate = new List<ICustomDynamicContent>();
				ForcedUpdate.CheckUpdateStatus();
				DateTime dateTime0 = DateTime.UtcNow;
				while (ForcedUpdate.IsCheckInProgress())
				{
					if ((DateTime.UtcNow - dateTime0).TotalSeconds > 5.0)
					{
						break;
					}
					yield return null;
				}
				if (ForcedUpdate.NeedToQuit())
				{
					report = new CheckReport(CheckReport.UpdateScope.ForcedUpdate);
				}
				else
				{
					resolution = Glu.ABTesting.Resolution.Retrieve(null, UrlsToABTestingDecisionTable);
					while (!resolution.Ready)
					{
						yield return null;
					}
					if (!resolution.IsApplied)
					{
						report = new CheckReport(CheckReport.UpdateScope.ABTesting, resolution);
					}
					else
					{
						if (m_indexInfo != null)
						{
							m_updateInfo = m_indexInfo.CheckForUpdates();
							while (m_updateInfo.state == UpdateInfo.State.Pending)
							{
								yield return null;
							}
							if (m_updateInfo.state == UpdateInfo.State.Outdated)
							{
								report = new CheckReport(CheckReport.UpdateScope.AssetBundles);
							}
						}
						else
						{
						}
						if (report.updateScope == CheckReport.UpdateScope.Nothing && CustomDynamicContent != null && CustomDynamicContent.Count > 0)
						{
							foreach (ICustomDynamicContent customDynamicContent in CustomDynamicContent)
							{
								if (customDynamicContent.IsInProgress)
								{
									cdcsToUpdate.Clear();
									foreach (ICustomDynamicContent temp in CustomDynamicContent)
									{
										temp.InvalidateLastCheckReport();
									}
									break;
								}
								customDynamicContent.CheckForUpdates(resolution);
								while (customDynamicContent.IsInProgress)
								{
									yield return null;
								}
								if (customDynamicContent.Result)
								{
									cdcsToUpdate.Add(customDynamicContent);
								}
							}
							if (cdcsToUpdate.Count > 0)
							{
								report = new CheckReport(CheckReport.UpdateScope.CustomDynamicContent, cdcsToUpdate.ToArray());
							}
						}
						else
						{
						}
					}
				}
				if (report.updateScope < CheckReport.UpdateScope.CustomDynamicContent)
				{
					foreach (ICustomDynamicContent customDynamicContent in CustomDynamicContent)
					{
						customDynamicContent.InvalidateLastCheckReport();
					}
				}
				LastCheckReport = report;
				yield return new WaitForSeconds(TimeInterval);
				m_isCheckingInProgress = false;
				yield break;
			}
		}

		public delegate void OnVoidEvent();

		public delegate bool OnBoolEvent();

		private static Version m_version;

		private static int m_timeInterval;

		private static string m_assetBundlesIndexName;

		private static List<ICustomDynamicContent> m_customDynamicContent;

		private static string[] m_baseUrls;

		private static bool m_isLoadingInProgress;

		private static bool m_isCheckingInProgress;

		private static Glu.ABTesting.Resolution m_resolution;

		private static bool m_isIndexInfo;

		private static IndexInfo m_indexInfo;

		private static UpdateInfo m_updateInfo;

		public static Version version
		{
			get
			{
				return m_version ?? (m_version = new Version(1, 1, 6));
			}
		}

		public static int ThreadCount
		{
			set
			{
				DownloadManager.ThreadCount = value;
				CustomConfigs.ThreadCount = value;
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

		public static string AssetBundlesIndexName
		{
			set
			{
				m_assetBundlesIndexName = value;
			}
			get
			{
				return m_assetBundlesIndexName;
			}
		}

		public static float Progress
		{
			get
			{
				float result = 0f;
				if (m_isIndexInfo && m_indexInfo != null && m_indexInfo.downloadInfo != null && m_indexInfo.downloadInfo.state == DownloadManager.Info.State.Downloaded)
				{
					AssetBundleInfo[] assetBundleInfo = m_indexInfo.assetBundleInfo;
					long num = 0L;
					float num2 = 0f;
					foreach (AssetBundleInfo assetBundleInfo2 in assetBundleInfo)
					{
						num += assetBundleInfo2.Size;
						num2 += assetBundleInfo2.downloadInfo.ReceivingProgress * (float)assetBundleInfo2.Size;
					}
					result = (!(num <= 0) ? (num2 / (float)num) : 0f);
				}
				return result;
			}
		}

		public static IndexInfo AssetBundlesIndexInfo
		{
			get
			{
				return m_isIndexInfo ? m_indexInfo : null;
			}
		}

		public static Glu.ABTesting.Resolution ABTestingResolution
		{
			get
			{
				return m_resolution;
			}
		}

		public static OnVoidEvent OnForcedBinariesUpdate { set; get; }

		public static OnBoolEvent OnAvailableBinariesUpdate { set; get; }

		public static OnVoidEvent OnFail { set; get; }

		public static OnVoidEvent OnSuccess { set; get; }

		public static List<ICustomDynamicContent> CustomDynamicContent
		{
			get
			{
				return m_customDynamicContent;
			}
		}

		public static void Init(params string[] baseUrls)
		{
			if (baseUrls != null && baseUrls.Length > 0)
			{
				for (int i = 0; i < baseUrls.Length; i++)
				{
					while (baseUrls[i].EndsWith("/"))
					{
						baseUrls[i] = baseUrls[i].Remove(baseUrls[i].Length - 1);
					}
				}
				m_baseUrls = baseUrls;
			}
			else
			{
				m_baseUrls = new string[0];
			}
		}

		static DynamicContent()
		{
			m_version = null;
			m_timeInterval = 300;
			m_assetBundlesIndexName = "index";
			m_customDynamicContent = new List<ICustomDynamicContent>();
			m_baseUrls = null;
			m_isLoadingInProgress = false;
			m_isCheckingInProgress = false;
			m_resolution = null;
			m_isIndexInfo = false;
			m_indexInfo = null;
			m_updateInfo = null;
			LastCheckReport = new CheckReport(CheckReport.UpdateScope.All);
		}

		public static void StartContentUpdate()
		{
			Impl.StartContentUpdate();
		}

		public static bool IsUpdateInProgress()
		{
			var result = Impl.IsUpdateInProgress();
			return result;
		}

		public static bool CheckForUpdates()
		{
			Impl.CheckForUpdates();
			return LastCheckReport.updateScope < CheckReport.UpdateScope.Nothing;
		}

		public static CheckReport LastCheckReport { private set; get; }
	}
}
