using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Serialization;
using Glu.ABTesting;
using Glu.AssetBundles;
using UnityEngine;

namespace Glu.DynamicContentPipeline
{
	public class CustomConfigs : ICustomDynamicContent
	{
		public class CheckReport
		{
			public class FileInfo
			{
				public enum Source
				{
					None,
					Online,
					Cache
				}

				public enum State
				{
					Unknown,
					Outdated,
					UpToDate
				}

				public string Filename { private set; get; }

				public string ETag { private set; get; }

				public Source source { private set; get; }

				public State state { private set; get; }

				public FileInfo(string filename, string eTag, Source source, State state)
				{
					Filename = filename;
					ETag = eTag;
					this.source = source;
					this.state = state;
				}
			}

			public FileInfo[] Files { protected set; get; }

			protected CheckReport()
			{
				Files = new FileInfo[0];
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs");
			}
		}

		private class Request
		{
			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Request");
				}
			}

			private static List<Request> m_requests = new List<Request>();

			public static Request[] EntireList
			{
				get
				{
					return m_requests.ToArray();
				}
			}

			public string Filename { private set; get; }

			public string ABTestingKey { private set; get; }

			public string[] Urls { private set; get; }

			public string LocalFilename { private set; get; }

			public bool IsDefaultFilename { private set; get; }

			public string DefaultFilename { private set; get; }

			public WWW www { set; get; }

			private Request(string filename, string abTestingKey, string[] urls, string localFilename, bool isDefaultFilename, string defaultFilename)
			{
				Filename = filename;
				ABTestingKey = abTestingKey;
				Urls = urls;
				LocalFilename = localFilename;
				IsDefaultFilename = isDefaultFilename;
				DefaultFilename = defaultFilename;
				www = null;
			}

			public static bool Create(string filename, string abTestingKey, string[] urls, string localFilename, bool isDefaultFilename, string defaultFilename)
			{
				bool flag = !string.IsNullOrEmpty(localFilename);
				foreach (Request request in m_requests)
				{
					if (!string.IsNullOrEmpty(request.LocalFilename) && !string.IsNullOrEmpty(localFilename) && request.LocalFilename.Equals(localFilename))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Request item = new Request(filename, abTestingKey, urls, localFilename, isDefaultFilename, defaultFilename);
					m_requests.Add(item);
				}
				AddObserverToCustomUpdate();
				return flag;
			}

			public static void ResetAll()
			{
				foreach (Request request in m_requests)
				{
					request.www = null;
				}
			}

			public static void RemoveAll()
			{
				m_requests.Clear();
				RemoveObserverFromCustomUpdate();
			}

			private static void AddObserverToCustomUpdate()
			{
				if (!(DynamicContent.CustomDynamicContent == null))
				{
					bool flag = false;
					foreach (ICustomDynamicContent item in DynamicContent.CustomDynamicContent)
					{
						if (item is CustomConfigs)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						DynamicContent.CustomDynamicContent.Add(new CustomConfigs());
					}
				}
			}

			private static void RemoveObserverFromCustomUpdate()
			{
				if (!(DynamicContent.CustomDynamicContent == null || DynamicContent.CustomDynamicContent.Count <= 0))
				{
					for (int num = DynamicContent.CustomDynamicContent.Count - 1; num >= 0; num--)
					{
						if (DynamicContent.CustomDynamicContent[num] is CustomConfigs)
						{
							DynamicContent.CustomDynamicContent.RemoveAt(num);
						}
					}
				}
			}
		}

		public class Impl : MonoBehaviour, ICustomDynamicContent
		{
			private enum Task
			{
				None,
				Update,
				Check
			}

			public static class AmazonResponseTester
			{
				private class Logger : LoggerSingleton<Logger>
				{
					public Logger()
					{
						LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Impl.AmazonResponseTester");
					}
				}

				[XmlType("Error")]
				public class AmazonError
				{
					private class Logger : LoggerSingleton<Logger>
					{
						public Logger()
						{
							LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Impl.AmazonResponseTester.AmazonError");
						}
					}

					[XmlAttribute("Code")]
					public string m_code;

					[XmlAttribute("Message")]
					public string m_message;

					[XmlAttribute("Key")]
					public string m_key;

					[XmlAttribute("RequestId")]
					public string m_requestId;

					[XmlAttribute("HostId")]
					public string m_hostId;

					private static XmlSerializer m_xmlSerializer = null;

					public static AmazonError LoadInstance(Stream stream)
					{
						AmazonError amazonError = null;
						try
						{
							amazonError = xmlSerializer.Deserialize(stream) as AmazonError;
						}
						catch (Exception e)
						{
							amazonError = null;
						}
						return amazonError;
					}

					public AmazonError()
					{
						m_code = null;
						m_message = null;
						m_key = null;
						m_requestId = null;
						m_hostId = null;
					}

					private static XmlSerializer xmlSerializer
					{
						get
						{
							if (m_xmlSerializer != null)
							{
								return m_xmlSerializer;
							}
							m_xmlSerializer = new XmlSerializer(typeof(AmazonError));
							return m_xmlSerializer;
						}
					}
				}

				public static bool IsJunk(byte[] bytes)
				{
					bool result = false;
					try
					{
						using (Stream stream = new MemoryStream(bytes, false))
						{
							result = AmazonError.LoadInstance(stream) != null;
						}
					}
					catch (Exception e)
					{
					}
					return result;
				}
			}

			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Impl");
				}
			}

			private class CheckReportWriter : CheckReport
			{
				private List<FileInfo> m_fileInfo;

				public CheckReportWriter()
				{
					m_fileInfo = new List<FileInfo>();
				}

				public void Add(FileInfo fileInfo)
				{
					m_fileInfo.Add(fileInfo);
					Files = m_fileInfo.ToArray();
				}
			}

			private class Source
			{
				private static MD5 m_md5 = null;

				public byte[] Bytes { private set; get; }

				public string ETag { private set; get; }

				public Source InnerSource { private set; get; }

				public bool BytesEqual(Source src)
				{
					bool result = false;
					byte[] bytes = Bytes;
					byte[] bytes2 = src.Bytes;
					if (bytes == null && bytes2 == null)
					{
						result = true;
					}
					if (bytes != null && bytes2 != null && bytes.Length == bytes2.Length)
					{
						result = true;
						for (int i = 0; i < bytes.Length; i++)
						{
							if (bytes[i] != bytes2[i])
							{
								result = false;
								break;
							}
						}
					}
					return result;
				}

				protected Source(byte[] bytes)
					: this(bytes, (bytes != null) ? ComputeHash(bytes) : null)
				{
				}

				protected Source(byte[] bytes, string eTag)
				{
					Bytes = bytes;
					ETag = StripETag(eTag);
					InnerSource = null;
				}

				protected Source(Source realSource)
					: this(realSource.Bytes, realSource.ETag)
				{
					InnerSource = realSource;
				}

				public bool ETagEquals(string eTag)
				{
					eTag = StripETag(eTag);
					return !string.IsNullOrEmpty(ETag) && !string.IsNullOrEmpty(eTag) && ETag.ToLower().Equals(eTag.ToLower());
				}

				private static MD5 md5
				{
					get
					{
						return m_md5 ?? (m_md5 = new MD5CryptoServiceProvider());
					}
				}

				private static string BytesToString(byte[] bytes)
				{
					string text = string.Empty;
					foreach (byte b in bytes)
					{
						text += b.ToString("x2").ToLower();
					}
					return text;
				}

				private static string ComputeHash(byte[] bytes)
				{
					return BytesToString(md5.ComputeHash(bytes));
				}

				private static string StripETag(string eTag)
				{
					if (!string.IsNullOrEmpty(eTag) && eTag.Contains("\""))
					{
						try
						{
							int num = eTag.IndexOf("\"") + 1;
							int num2 = eTag.LastIndexOf("\"");
							int length = num2 - num;
							eTag = eTag.Substring(num, length);
						}
						catch (Exception)
						{
						}
					}
					return eTag;
				}
			}

			private class Online : Source
			{
				public Online(byte[] bytes, string eTag)
					: base(bytes, eTag)
				{
				}

				public Online(Source realSource)
					: base(realSource)
				{
				}
			}

			private class Cache : Source
			{
				public Cache(byte[] bytes)
					: base(bytes)
				{
				}

				public Cache(byte[] bytes, string eTag)
					: base(bytes, eTag)
				{
				}
			}

			private class Default : Source
			{
				public Default(byte[] bytes)
					: base(bytes)
				{
				}
			}

			private class HttpHeadRequestData
			{
				private object m_locker = new object();

				private string m_url;

				private bool m_succeeded;

				private string m_eTag;

				public string Url
				{
					private set
					{
						lock (m_locker)
						{
							m_url = value;
						}
					}
					get
					{
						lock (m_locker)
						{
							return m_url;
						}
					}
				}

				public bool Succeeded
				{
					set
					{
						lock (m_locker)
						{
							m_succeeded = value;
						}
					}
					get
					{
						lock (m_locker)
						{
							return m_succeeded;
						}
					}
				}

				public string ETag
				{
					set
					{
						lock (m_locker)
						{
							m_eTag = value;
						}
					}
					get
					{
						lock (m_locker)
						{
							return m_eTag;
						}
					}
				}

				public HttpHeadRequestData(string url)
				{
					Url = url;
					Succeeded = false;
					ETag = null;
				}
			}

			private const string kGameObjectName = "Glu.DynamicContentPipeline.CustomConfigs GameObject";

			private static Impl m_instance;

			private static int m_threadCount = 1;

			private static Task m_task;

			private static Glu.ABTesting.Resolution m_resolution;

			private static CheckReportWriter m_report;

			private static bool m_finished = true;

			private static bool m_result;

			private static long m_lastUpdateSize;

			private static int m_runCoroutinesCount;

			void ICustomDynamicContent.StartContentUpdate(Glu.ABTesting.Resolution resolution)
			{
				if (m_task == Task.None)
				{
					m_task = Task.Update;
					m_resolution = resolution;
					m_finished = false;
					m_result = true;
					m_lastUpdateSize = 0L;
					StartCoroutine(ManageRequests());
				}
			}

			void ICustomDynamicContent.CheckForUpdates(Glu.ABTesting.Resolution resolution)
			{
				if (m_task == Task.None)
				{
					m_task = Task.Check;
					m_resolution = resolution;
					m_finished = false;
					m_result = false;
					m_lastUpdateSize = 0L;
					StartCoroutine(ManageRequests());
				}
			}

			float ICustomDynamicContent.Progress
			{
				get
				{
					return 0f;
				}
			}

			bool ICustomDynamicContent.IsInProgress
			{
				get
				{
					return !m_finished;
				}
			}

			bool ICustomDynamicContent.Result
			{
				get
				{
					return m_result;
				}
			}

			long ICustomDynamicContent.LastUpdateSize
			{
				get
				{
					return m_lastUpdateSize;
				}
			}

			void ICustomDynamicContent.InvalidateLastCheckReport()
			{
				LastCheckReport = null;
			}

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

			public static Impl GetInstance()
			{
				if (m_instance != null)
				{
					return m_instance;
				}
				GameObject gameObject = GameObject.Find("Glu.DynamicContentPipeline.CustomConfigs GameObject");
				if (gameObject != null)
				{
					m_instance = gameObject.GetComponent<GluDynamicContentPipelineCustomConfigsImpl>();
					return m_instance;
				}
				gameObject = new GameObject("Glu.DynamicContentPipeline.CustomConfigs GameObject");
				gameObject.AddComponent<GluDynamicContentPipelineCustomConfigsImpl>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				m_instance = gameObject.GetComponent<GluDynamicContentPipelineCustomConfigsImpl>();
				m_instance.useGUILayout = false;
				m_instance.enabled = false;
				return m_instance;
			}

			public CheckReport LastCheckReport { private set; get; }

			private IEnumerator ManageRequests()
			{
				m_report = new CheckReportWriter();
				m_runCoroutinesCount = 0;
				Request.ResetAll();
				Request[] requests = Request.EntireList;
				int idx = 0;
				bool warnedAboutDelay = false;
				while (idx < requests.Length || m_runCoroutinesCount > 0)
				{
					if (idx < requests.Length)
					{
						Request request = requests[idx];
						if (ThreadCount > m_runCoroutinesCount)
						{
							bool processRequest = (m_task == Task.Update && LastCheckReport == null) || m_task == Task.Check;
							if (m_task == Task.Update && LastCheckReport != null)
							{
								foreach (CheckReport.FileInfo fileInfo in LastCheckReport.Files)
								{
									if (fileInfo.Filename.Equals(request.LocalFilename) && fileInfo.state == CheckReport.FileInfo.State.Outdated)
									{
										processRequest = true;
										break;
									}
								}
							}
							if (processRequest)
							{
								m_runCoroutinesCount++;
								StartCoroutine(DoProcess(request));
							}
							else
							{
							}
							idx++;
							warnedAboutDelay = false;
						}
						else if (!warnedAboutDelay)
						{
							warnedAboutDelay = true;
						}
					}
					yield return null;
				}
				if ((m_task == Task.Update && m_result) || m_task == Task.Check)
				{
					LastCheckReport = m_report;
				}
				m_task = Task.None;
				m_resolution = null;
				m_report = null;
				m_finished = true;
				yield break;
			}

			private IEnumerator DoProcess(Request request)
			{
				double unusedLocal1;
				try
				{
					Source source = null;
					Online online = null;
					Cache cache = null;
					Default @default = null;
					List<string> urls = new List<string>();
					if (!string.IsNullOrEmpty(request.Filename))
					{
						throw new NotImplementedException();
					}
					if (!string.IsNullOrEmpty(request.ABTestingKey))
					{
						if (m_resolution != null)
						{
							if (m_resolution.Data != null && m_resolution.Data.ContainsKey(request.ABTestingKey))
							{
								urls.AddRange(m_resolution.Data[request.ABTestingKey].Split(new string[3] { " ", ",", ";" }, StringSplitOptions.RemoveEmptyEntries));
							}
							else
							{
							}
						}
						else
						{
						}
					}
					else if (request.Urls != null && request.Urls.Length > 0)
					{
						urls.AddRange(request.Urls);
					}
					try
					{
						byte[] bytes = StorageManager.ReadFromLocation(request.LocalFilename);
						string eTag = null;
						try
						{
							using (StreamReader sr = new StreamReader(request.LocalFilename + ".version"))
							{
								eTag = sr.ReadLine();
							}
						}
						catch (Exception e)
						{
							Logger.Warn("Unable to read the cached version: {0}", e);
						}
						if (bytes != null)
						{
							if (!string.IsNullOrEmpty(eTag))
							{
								cache = new Cache(bytes, eTag);
							}
							else
							{
								cache = new Cache(bytes);
							}
						}
					}
					catch (Exception e)
					{
						Logger.Warn("Unable to load the configuration cache: {0}", e);
					}
					if (cache == null && request.IsDefaultFilename)
					{
						TextAsset textAsset = null;
						if (string.IsNullOrEmpty(Path.GetExtension(request.DefaultFilename)))
						{
							textAsset = Resources.Load(request.DefaultFilename) as TextAsset;
						}
						if (textAsset == null && request.DefaultFilename.ToLower().StartsWith("assets"))
						{
							textAsset = Glu.AssetBundles.AssetBundles.Load(request.DefaultFilename) as TextAsset;
						}
						if (textAsset != null)
						{
							@default = new Default(textAsset.bytes);
						}
						else
						{
						}
					}
					else
					{
					}
					bool wasHttpHeadRequestSuccessful = false;
					if (!wasHttpHeadRequestSuccessful)
					{
						string eTag = null;
						if (urls.Count > 0)
						{
							for (int idx = 0; idx < urls.Count; idx++)
							{
								HttpHeadRequestData headRequestData = new HttpHeadRequestData(urls[idx]);
								Thread thread = new Thread((object obj) =>
								{
									try
									{
										HttpHeadRequestData httpHeadRequestData = (HttpHeadRequestData)obj;
										HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpHeadRequestData.Url);
										httpWebRequest.Method = "HEAD";
										try
										{
											using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
											{
												try
												{
													switch (httpWebResponse.StatusCode)
													{
													case HttpStatusCode.OK:
													case HttpStatusCode.Created:
													case HttpStatusCode.Accepted:
													case HttpStatusCode.NonAuthoritativeInformation:
													case HttpStatusCode.NoContent:
													case HttpStatusCode.ResetContent:
													case HttpStatusCode.PartialContent:
														httpHeadRequestData.Succeeded = true;
														break;
													default:
														httpHeadRequestData.Succeeded = false;
														break;
													}
													httpHeadRequestData.ETag = httpWebResponse.Headers[HttpResponseHeader.ETag];
												}
												catch (Exception e)
												{
												}
											}
										}
										catch (Exception e)
										{
										}
									}
									catch (Exception e)
									{
									}
								});
								thread.Name = string.Format("Package.DynamicContentPipeline.CustomConfigs.Impl.DoProcess #{0}", Guid.NewGuid().ToString());
								thread.IsBackground = true;
								thread.Start(headRequestData);
								bool firedTimeout = false;
								DateTime dateTime = DateTime.UtcNow;
								while (thread.IsAlive)
								{
									if ((DateTime.UtcNow - dateTime).TotalSeconds > 5.0)
									{
										firedTimeout = true;
										break;
									}
									yield return null;
								}
								if (!firedTimeout)
								{
									wasHttpHeadRequestSuccessful = headRequestData.Succeeded;
									if (wasHttpHeadRequestSuccessful)
									{
										eTag = headRequestData.ETag;
									}
								}
								else
								{
								}
								if (!string.IsNullOrEmpty(eTag))
								{
									break;
								}
								yield return null;
							}
						}
						if (wasHttpHeadRequestSuccessful && !string.IsNullOrEmpty(eTag))
						{
							if (cache != null && cache.ETagEquals(eTag))
							{
								source = new Online(cache);
							}
							else if (@default != null && @default.ETagEquals(eTag))
							{
								source = new Online(@default);
							}
							else
							{
								online = new Online(null, eTag);
							}
						}
					}
					if (source == null && wasHttpHeadRequestSuccessful && urls.Count > 0)
					{
						for (int idx = 0; idx < urls.Count; idx++)
						{
							request.www = new WWW(urls[idx]);
							yield return StartCoroutine(WWWCoroutine(request.www));
							if (WWWSucceeded(request.www))
							{
								byte[] bytes = request.www.bytes;
								string eTag = null;
								try
								{
									eTag = request.www.responseHeaders["ETAG"];
								}
								catch (Exception e)
								{
									Logger.Warn("Unable to read the response ETag: {0}", e);
								}
								if (string.IsNullOrEmpty(eTag) && online != null && !string.IsNullOrEmpty(online.ETag))
								{
									eTag = online.ETag;
								}
								online = new Online(bytes, eTag);
								source = online;
								request.www = null;
								break;
							}
							request.www = null;
						}
					}
					else
					{
					}
					if (source == null && cache != null)
					{
						source = cache;
					}
					if (source == null && @default != null)
					{
						source = @default;
					}
					if (request.IsDefaultFilename)
					{
					}
					if (source != null && source is Online && source.InnerSource == null)
					{
						m_lastUpdateSize += source.Bytes.Length;
					}
					string fiETag = source != null ? source.ETag : null;
					CheckReport.FileInfo.Source fiSource = ((source != null) ? ((source is Online) ? CheckReport.FileInfo.Source.Online : CheckReport.FileInfo.Source.Cache) : CheckReport.FileInfo.Source.None);
					CheckReport.FileInfo.State fiState = ((source != null && source is Online) ? ((source.InnerSource == null) ? CheckReport.FileInfo.State.Outdated : CheckReport.FileInfo.State.UpToDate) : CheckReport.FileInfo.State.Unknown);
					m_report.Add(new CheckReport.FileInfo(request.LocalFilename, fiETag, fiSource, fiState));
					if (source != null)
					{
						switch (m_task)
						{
						case Task.Update:
							DoContentUpdate(request, source);
							break;
						case Task.Check:
							DoCheckForUpdates(request, source);
							break;
						default:
							break;
						}
					}
				}
				finally
				{
					m_runCoroutinesCount--;
				}
				yield break;
			}

			private void DoContentUpdate(Request request, Source source)
			{
				if (source != null)
				{
					bool flag = true;
					bool flag2 = true;
					if (source.InnerSource != null)
					{
						flag = !source.BytesEqual(source.InnerSource);
						flag2 = !source.ETagEquals(source.InnerSource.ETag);
						source = source.InnerSource;
					}
					if (!(source is Cache))
					{
						try
						{
							try
							{
								string directoryName = Path.GetDirectoryName(request.LocalFilename);
								if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
								{
									Directory.CreateDirectory(directoryName);
								}
							}
							catch (Exception e)
							{
							}
							if (flag)
							{
								StorageManager.WriteToLocation(request.LocalFilename, request.LocalFilename, source.Bytes);
							}
							if (flag2)
							{
								try
								{
									string path = request.LocalFilename + ".version";
									if (!string.IsNullOrEmpty(source.ETag))
									{
										using (StreamWriter streamWriter = new StreamWriter(path))
										{
											streamWriter.WriteLine(source.ETag);
										}
									}
									else if (File.Exists(path))
									{
										File.Delete(path);
									}
								}
								catch (Exception e)
								{
								}
							}
						}
						catch (Exception e)
						{
							m_result = false;
						}
					}
				}
				else
				{
					m_result = false;
				}
			}

			private void DoCheckForUpdates(Request request, Source source)
			{
				if (source != null)
				{
					bool flag = true;
					if (source.InnerSource != null)
					{
						flag = !source.BytesEqual(source.InnerSource);
						source = source.InnerSource;
					}
					if (!(source is Cache) && flag)
					{
						m_result = true;
					}
				}
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
					else if (www.progress < 1f && (DateTime.UtcNow - dateTime).TotalSeconds > 5.0)
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
					bool flag2 = false;
					try
					{
						Dictionary<string, string> responseHeaders = www.responseHeaders;
						string text = responseHeaders["STATUS"];
						string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (array[0].ToUpper().Contains("HTTP"))
						{
							int num = (int)Convert.ChangeType(array[1], typeof(int));
							flag = num < 400;
							flag2 = true;
						}
					}
					catch (Exception e)
					{
					}
					if (!flag2)
					{
						bool flag3 = AmazonResponseTester.IsJunk(www.bytes);
						flag = !flag3;
						if (flag3)
						{
						}
					}
				}
				return flag;
			}
		}

		void ICustomDynamicContent.StartContentUpdate(Glu.ABTesting.Resolution resolution)
		{
			((ICustomDynamicContent)Impl.GetInstance()).StartContentUpdate(resolution);
		}

		void ICustomDynamicContent.CheckForUpdates(Glu.ABTesting.Resolution resolution)
		{
			((ICustomDynamicContent)Impl.GetInstance()).CheckForUpdates(resolution);
		}

		float ICustomDynamicContent.Progress
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).Progress;
			}
		}

		bool ICustomDynamicContent.IsInProgress
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).IsInProgress;
			}
		}

		bool ICustomDynamicContent.Result
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).Result;
			}
		}

		long ICustomDynamicContent.LastUpdateSize
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).LastUpdateSize;
			}
		}

		private CustomConfigs()
		{
		}

		void ICustomDynamicContent.InvalidateLastCheckReport()
		{
			((ICustomDynamicContent)Impl.GetInstance()).InvalidateLastCheckReport();
		}

		public static int ThreadCount
		{
			set
			{
				Impl.ThreadCount = value;
			}
			get
			{
				return Impl.ThreadCount;
			}
		}

		public static bool RegisterABTestedDownloadableFile(string abTestingKey, string localFilename)
		{
			var result = Request.Create(null, abTestingKey, null, localFilename, false, null);
			return result;
		}

		public static bool RegisterABTestedDownloadableFile(string abTestingKey, string localFilename, string defaultFilename)
		{
			var result = Request.Create(null, abTestingKey, null, localFilename, true, defaultFilename);
			return result;
		}

		public static bool RegisterDownloadableFile(string[] urls, string localFilename)
		{
			var result = Request.Create(null, null, urls, localFilename, false, null);
			return result;
		}

		public static bool RegisterDownloadableFile(string[] urls, string localFilename, string defaultFilename)
		{
			var result = Request.Create(null, null, urls, localFilename, true, defaultFilename);
			return result;
		}

		public static void UnregisterAll()
		{
			Request.RemoveAll();
		}

		public CheckReport LastCheckReport
		{
			get
			{
				return Impl.GetInstance().LastCheckReport;
			}
		}
	}
}
