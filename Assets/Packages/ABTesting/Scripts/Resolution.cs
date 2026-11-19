using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Glu.UnityBuildSystem;
using UnityEngine;

namespace Glu.ABTesting
{
	public class Resolution
	{
		public class Resolver : MonoBehaviour
		{
			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.Resolution.Resolver");
				}
			}

			private const string kGameObjectName = "Glu.ABTesting.Resolution.Resolver GameObject";

			private static Resolver m_instance = null;

			public static void Resolve(Resolution resolution)
			{
				if (!resolution.m_wasPassedToResolver)
				{
					resolution.m_wasPassedToResolver = true;
					GetInstance().StartCoroutine("DoABTesting", resolution);
				}
			}

			private static Resolver GetInstance()
			{
				if (m_instance != null)
				{
					return m_instance;
				}
				ABTesting.Init();
				GameObject gameObject = GameObject.Find("Glu.ABTesting.Resolution.Resolver GameObject");
				if (gameObject != null)
				{
					m_instance = gameObject.GetComponent<GluABTestingResolutionResolver>();
					return m_instance;
				}
				gameObject = new GameObject("Glu.ABTesting.Resolution.Resolver GameObject");
				gameObject.AddComponent<GluABTestingResolutionResolver>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				m_instance = gameObject.GetComponent<GluABTestingResolutionResolver>();
				m_instance.useGUILayout = false;
				m_instance.enabled = false;
				return m_instance;
			}

			private IEnumerator DoABTesting(Resolution resolution)
			{
				string unusedLocal1;
				string threadId = new System.Random().Next(10, 100).ToString();
				string[] urls = resolution.m_urlsToDecisionTable;
				bool isApplied = false;
				IDictionary<string, string> dict = null;
				DecisionTable decisionTable = null;
				for (int idx = 0; idx < urls.Length; idx++)
				{
					string url = urls[idx];
					string version = null;
					WWW www = null;
					url = url.Replace('\\', '/');
					if (!url.Contains("://"))
					{
						url = "file://" + url;
					}
					if (url.ToLower().EndsWith("version"))
					{
						www = new WWW(SpecializeURL(url));
						yield return StartCoroutine(WWWCoroutine(www));
						if (WWWSucceeded(www))
						{
							using (StringReader sr = new StringReader(www.text))
							{
								version = sr.ReadLine().Trim();
								if (version.Contains("<"))
								{
									version = null;
								}
							}
						}
						url = url.Remove(url.LastIndexOf("version")) + "xml";
					}
					if (!string.IsNullOrEmpty(version))
					{
						DecisionTable cachedTable = DecisionTable.Load(DefaultDecisionTableFilename);
						if (cachedTable != null)
						{
							if (!string.IsNullOrEmpty(cachedTable.m_version))
							{
								if (cachedTable.m_version.Equals(version))
								{
									decisionTable = cachedTable;
									break;
								}
							}
							else
							{
							}
						}
						else
						{
						}
					}
					www = new WWW(SpecializeURL(url));
					yield return StartCoroutine(WWWCoroutine(www));
					if (WWWSucceeded(www))
					{
						decisionTable = DecisionTable.Load(www.bytes);
						if (decisionTable != null)
						{
							if (!string.IsNullOrEmpty(version))
							{
								decisionTable.m_version = version;
								bool wasCached = decisionTable.Save(DefaultDecisionTableFilename);
							}
							break;
						}
					}
					else
					{
					}
					if (idx + 1 == urls.Length)
					{
					}
				}
				if (decisionTable != null)
				{
					ABTesting.Init(decisionTable);
					dict = ABTesting.Resolve();
				}
				if (dict == null)
				{
					dict = LoadResolutionData();
					isApplied = true;
					if (dict == null)
					{
						byte[] rawBytes = null;
						TextAsset textAsset = Resources.Load("ABTesting") as TextAsset;
						if (textAsset != null)
						{
							rawBytes = textAsset.bytes;
						}
						else
						{
							string filename = Path.Combine(Application.streamingAssetsPath, "ABTesting.xml");
							try
							{
								if (File.Exists(filename))
								{
									rawBytes = File.ReadAllBytes(filename);
								}
							}
							catch (Exception e)
							{
								Logger.Warn("Unable to read A/B testing data: {0}", e);
							}
						}
						if (rawBytes != null)
						{
							decisionTable = DecisionTable.Load(rawBytes);
							if (decisionTable != null)
							{
								ABTesting.Init(decisionTable);
								dict = ABTesting.Resolve();
							}
						}
						if (dict == null)
						{
							dict = resolution.m_dict;
						}
						isApplied = false;
					}
				}
				else
				{
					IDictionary<string, string> oldDict = LoadResolutionData();
					if (oldDict != null)
					{
						if (oldDict.Count == dict.Count)
						{
							isApplied = true;
							foreach (KeyValuePair<string, string> kvp in oldDict)
							{
								if (dict.ContainsKey(kvp.Key))
								{
									if (!kvp.Value.Equals(dict[kvp.Key]))
									{
										isApplied = false;
										break;
									}
								}
								else
								{
									isApplied = false;
									break;
								}
							}
						}
						else
						{
							isApplied = false;
						}
					}
					else
					{
						isApplied = false;
					}
				}
				resolution.m_isApplied = isApplied;
				resolution.m_dict = dict;
				resolution.m_isFinished = true;
				yield break;
			}

			private string SpecializeURL(string url)
			{
				return url;
			}

			private IEnumerator WWWCoroutine(WWW www)
			{
				double unusedLocal1;
				DateTime dateTime = DateTime.UtcNow;
				float progress = www.progress;
				while (true)
				{
					if (www.isDone || !string.IsNullOrEmpty(www.error))
					{
						break;
					}
					if (www.progress > progress)
					{
						dateTime = DateTime.UtcNow;
						progress = www.progress;
					}
					else if ((DateTime.UtcNow - dateTime).TotalSeconds > 5.0)
					{
						break;
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

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.Resolution");
			}
		}

		private const string kDefaultDecisionTableFilename = "ABTestingDecisionTable.dat";

		private const string kDefaultResolutionDataFilename = "ABTestingResolutionData.dat";

		private string[] m_urlsToDecisionTable;

		private bool m_wasPassedToResolver;

		private bool m_isFinished;

		private bool m_isApplied;

		private IDictionary<string, string> m_dict;

		public bool Ready
		{
			get
			{
				return m_isFinished;
			}
		}

		public bool IsApplied
		{
			get
			{
				return Ready ? m_isApplied : false;
			}
		}

		public IDictionary<string, string> Data
		{
			get
			{
				return Ready ? m_dict : null;
			}
		}

		public string VariantId
		{
			get
			{
				string value = null;
				if (Data != null && !Data.TryGetValue("VariantId", out value))
				{
					value = null;
				}
				return value;
			}
		}

		public static Resolution Retrieve(IDictionary<string, string> initialResolutionData, params string[] urlsToDecisionTable)
		{
			Resolution resolution = null;
			if (urlsToDecisionTable != null && urlsToDecisionTable.Length > 0)
			{
				resolution = new Resolution(urlsToDecisionTable, initialResolutionData);
				Resolver.Resolve(resolution);
			}
			return resolution;
		}

		public bool Commit()
		{
			bool result = false;
			if (Ready)
			{
				if (!IsApplied && SaveResolutionData(Data))
				{
					m_isApplied = true;
				}
				result = IsApplied;
			}
			return result;
		}

		private static string DefaultPathname
		{
			get
			{
				return !(!string.IsNullOrEmpty(BuildInfo.buildTag)) ? Application.temporaryCachePath : Path.Combine(Application.temporaryCachePath, BuildInfo.buildTag);
			}
		}

		private Resolution(string[] urlsToDecisionTable, IDictionary<string, string> defaultResolutionData)
		{
			m_urlsToDecisionTable = urlsToDecisionTable;
			m_wasPassedToResolver = false;
			m_isFinished = false;
			m_isApplied = false;
			m_dict = defaultResolutionData;
		}

		private static string DefaultDecisionTableFilename
		{
			get
			{
				return Path.Combine(DefaultPathname, "ABTestingDecisionTable.dat");
			}
		}

		private static string DefaultResolutionDataFilename
		{
			get
			{
				return Path.Combine(DefaultPathname, "ABTestingResolutionData.dat");
			}
		}

		private static IDictionary<string, string> LoadResolutionData()
		{
			IDictionary<string, string> dictionary = null;
			try
			{
				IList<KeyValuePair<string, string>> list = StorageManager.ReadXmlFromLocation(DefaultResolutionDataFilename, typeof(List<KeyValuePair<string, string>>)) as List<KeyValuePair<string, string>>;
				if (list != null)
				{
					dictionary = new Dictionary<string, string>();
					dictionary.Clear();
					foreach (KeyValuePair<string, string> item in list)
					{
						dictionary.Add(item.Key, item.Value);
					}
				}
			}
			catch (Exception e)
			{
			}
			return dictionary;
		}

		private static bool SaveResolutionData(IDictionary<string, string> dict)
		{
			bool result = false;
			if (dict != null)
			{
				IList<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
				list.Clear();
				foreach (KeyValuePair<string, string> item in dict)
				{
					list.Add(item);
				}
				try
				{
					string directoryName = Path.GetDirectoryName(DefaultResolutionDataFilename);
					if (!Directory.Exists(directoryName))
					{
						try
						{
							Directory.CreateDirectory(directoryName);
						}
						catch (Exception)
						{
						}
					}
					StorageManager.WriteXmlToLocation(DefaultResolutionDataFilename, DefaultResolutionDataFilename, list);
					iPhone.SetNoBackupFlag(DefaultResolutionDataFilename);
					string text = DefaultResolutionDataFilename + ".check";
					if (File.Exists(text))
					{
						iPhone.SetNoBackupFlag(text);
					}
					result = true;
				}
				catch (Exception e)
				{
				}
			}
			else if (File.Exists(DefaultResolutionDataFilename))
			{
				try
				{
					File.Delete(DefaultResolutionDataFilename);
					File.Delete(DefaultResolutionDataFilename + ".check");
				}
				catch (Exception e)
				{
				}
			}
			return result;
		}
	}
}
