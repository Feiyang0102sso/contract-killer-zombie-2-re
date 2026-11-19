using System.IO;
using System.Text;
using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.DynamicContentPipeline;
using Glu.Localization;
using UnityEngine;

public class BootLoggerHandler : Logging.HandlerBase
{
	private SpriteText _text;

	public BootLoggerHandler(SpriteText text)
	{
		_text = text;
	}

	public override void Emit(ref Logging.LogRecord record)
	{
		string text = Format(ref record);
		_text.Text += '\n' + text;
	}
}

public class Boot : MonoBehaviourAndLog
{
	private const string DECORATION_FILE_NAME = "localization_decorate_strings.txt";

	public SpriteText DebugText;

	public UIProgressBar BootMeter;

	public UISingleSprite Background;

	public Camera UICamera;

	private Logging.ILogger _logger;

	private BootLoggerHandler _handler;

	private string _previousBundleState = string.Empty;

	private bool _done;

	private static bool _forcedUpdatePause = false;

	public static bool ForcedUpdatePause
	{
		get
		{
			return _forcedUpdatePause;
		}
		set
		{
			_forcedUpdatePause = value;
		}
	}

	private void Awake()
	{
		LogTo("App.Boot");
		_handler = new BootLoggerHandler(DebugText);
		_logger = Logging.GetLogger("App.Boot");
		_logger.AddHandler(_handler);
		TimeControl.Restart();
		UIWindow.Restart();
		ForcedUpdatePause = false;
		if (Singleton.Exists())
		{
			Object.DestroyImmediate(Singleton.inst.gameObject, false);
		}
		if (DecorationOfLocalizedStringsEnabled)
		{
			EnableDecorationOfLocalizedStrings(true);
		}
		if (QualitySetter.BootLogoEnabled)
		{
			Texture2D texture2D = Resources.Load("Loading_" + Strings.Locale) as Texture2D;
			if (!texture2D)
			{
				texture2D = Resources.Load("Loading_en") as Texture2D;
			}
			Background.SetTexture(texture2D);
		}
		Background.transform.position = new Vector3(0f, 0f, -1f);
	}

	private void OnDestroy()
	{
		_logger.RemoveHandler(_handler);
	}

	private void Start()
	{
		App.Instance.Create();
		BootMeter.Value = 0f;
		CustomConfigs.RegisterABTestedDownloadableFile("StoresUrls", Path.Combine(StorageManager.TemporaryCachePath, Util.GetFileNameWithBuildTag("Stores.xml")));
		CustomConfigs.RegisterABTestedDownloadableFile("OffersUrls", Path.Combine(StorageManager.TemporaryCachePath, Util.GetFileNameWithBuildTag("Offers.xml")));
		CustomConfigs.RegisterABTestedDownloadableFile("StoresOverrideUrls", Path.Combine(StorageManager.TemporaryCachePath, Util.GetFileNameWithBuildTag("StoresOverride.xml")), "Assets/Configs/StoresOverride.xml");
		DynamicContent.ThreadCount = 3;
		DynamicContent.OnForcedBinariesUpdate = () =>
		{
			PopupManager.ShowForcedUpdateAppDialog();
		};
		DynamicContent.OnAvailableBinariesUpdate = () =>
		{
			PopupManager.ShowRecommendUpdateAppDialog();
			ForcedUpdatePause = true;
			return false;
		};
		DynamicContent.OnFail = Fail;
		DynamicContent.OnSuccess = () =>
		{
			_done = true;
		};
		DynamicContent.Init("http://glusvn.s3.amazonaws.com/ckzombies2-live");
		DynamicContent.StartContentUpdate();
		GameAnalytics.LogAssetBundlesStarted();
	}

	private void Update()
	{
		BootMeter.Value = DynamicContent.Progress;
		string text = null;
		if (ForcedUpdate.IsCheckInProgress())
		{
			text = "Checking for new version...";
		}
		else if (ForcedUpdate.NeedToQuit())
		{
			text = "Forced update";
		}
		else
		{
			Glu.ABTesting.Resolution aBTestingResolution = DynamicContent.ABTestingResolution;
			if (aBTestingResolution == null)
			{
				text = "Waiting for ABTesting...";
			}
			else
			{
				GameAnalytics.LogABTesting(aBTestingResolution.VariantId);
				if (!aBTestingResolution.Ready)
				{
					text = "ABTesting...";
				}
				else
				{
					IndexInfo assetBundlesIndexInfo = DynamicContent.AssetBundlesIndexInfo;
					if (assetBundlesIndexInfo != null)
					{
						text = "Checking for asset bundles...";
						if (assetBundlesIndexInfo.state == IndexInfo.State.InProgress)
						{
							if (assetBundlesIndexInfo.downloadInfo == null)
							{
								text = "Preparing to start to download index...";
							}
							else if (assetBundlesIndexInfo.downloadInfo.state != DownloadManager.Info.State.Downloaded)
							{
								text = "Loading index file...";
							}
							else if (assetBundlesIndexInfo.downloadInfo.state == DownloadManager.Info.State.Downloaded)
							{
								AssetBundleInfo[] assetBundleInfo = assetBundlesIndexInfo.assetBundleInfo;
								foreach (AssetBundleInfo assetBundleInfo2 in assetBundleInfo)
								{
									if (assetBundleInfo2.downloadInfo.state == DownloadManager.Info.State.Receiving)
									{
										text = "Loading asset bundle " + assetBundleInfo2.Filename + "...";
									}
								}
							}
						}
						if (assetBundlesIndexInfo.state == IndexInfo.State.Failed)
						{
							text = "Cannot load asset bundles";
						}
						if (assetBundlesIndexInfo.state == IndexInfo.State.Succeeded)
						{
							text = "Asset bundles are loaded";
						}
					}
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "Unknown state";
		}
		if (_previousBundleState != text)
		{
			_previousBundleState = text;
		}
		if (_done && !ForcedUpdatePause)
		{
			Done();
		}
	}

	private void Fail()
	{
		Singleton.UI.PopupManager.ShowAssetBundlesFailDialog();
		GameAnalytics.LogAssetBundlesCompleted(false);
	}

	private void Done()
	{
		Object.DestroyImmediate(UIManager.instance.gameObject);
		GameAnalytics.LogAssetBundlesCompleted(true);
		Singleton.Tutorial.Init();
		if (Tutorial.IsRunning() && Singleton.Tutorial.State.AutoStartMission() != null)
		{
			Singleton.Game.GoToGameLevel(Singleton.Tutorial.State.AutoStartMission(), false);
		}
		else
		{
			Singleton.Game.GoToCityMap(false);
		}
		_done = false;
	}

	public static bool DecorationOfLocalizedStringsEnabled
	{
		get
		{
			return File.Exists(DecorationFilePath);
		}
	}

	public static void EnableDecorationOfLocalizedStrings(bool decorate)
	{
		SetLocalizedStringsDecorator(decorate);
		string decorationFilePath = DecorationFilePath;
		if (decorate)
		{
			File.WriteAllText(decorationFilePath, string.Empty);
		}
		else if (File.Exists(decorationFilePath))
		{
			File.Delete(decorationFilePath);
		}
	}

	private static void SetLocalizedStringsDecorator(bool decorate)
	{
		LocalizationManager manager = Strings.Manager;
		if (decorate)
		{
			manager.StringDecorator = (string context, string id, string returnString) =>
			{
				for (int i = 0; i < returnString.Length; i++)
				{
					if (returnString[i] != '%')
					{
						return ReplaceStringContentsWithChar(returnString, ' ');
					}
				}
				return returnString;
			};
			manager.UntranslatedStringHandler = (string context, string id) => ReplaceStringContentsWithChar(id, ' ');
		}
		else
		{
			manager.StringDecorator = null;
			manager.UntranslatedStringHandler = null;
		}
	}

	private static string ReplaceStringContentsWithChar(string str, char ch)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ch, (str ?? string.Empty).Length);
		return stringBuilder.ToString();
	}

	private static string DecorationFilePath
	{
		get
		{
			return Path.Combine(StorageManager.PersistentDataPath, "localization_decorate_strings.txt");
		}
	}
}
