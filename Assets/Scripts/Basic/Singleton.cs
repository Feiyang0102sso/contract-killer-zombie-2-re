using System;
using System.Collections;
using Glu.AssetBundles;
using Glu.AssetMapping;
using Glu.Localization;
using UnityEngine;

public class Singleton : MonoBehaviour
{
	public delegate void GCCompletedDelegate();

	private static Singleton instance = null;

	private static OnScreenLog log;

	private TimeControl _time;

	private GameObjectsCache _gameObjectsCache;

	private UI _ui;

	private Loader _loader;

	private AppGame _game;

	private Settings _settings;

	private QualitySetter _quality;

	private Const _const;

	private MetaMission _metaMission;

	private Visual _visual;

	private PlayerData _playerData;

	private ShopData _shopData;

	private GameData _gameData;

	private Achievements _achievements;

	private Leaderboards _leaderboards;

	private Tutorial _tutorial;

	private GameEvents _gameEvents;

	private MainTheme _mainTheme;

	private FpsCounter fpsCounter;

	private PerformanceMeter _perfMeter;

	public static bool Exists()
	{
		return instance != null;
	}

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private static void Create()
	{
		if ((bool)instance)
		{
			Debug.LogWarning("singleton already created");
			return;
		}
		GameObject gameObject = new GameObject(typeof(Singleton).Name);
		gameObject.name = "$Singleton";
		instance = gameObject.AddComponent<Singleton>();
		InitAssetMapping();
		ServerTime.Init(true);
		DailyBonus.Init();
		instance._time = gameObject.AddComponent<TimeControl>();
		instance._loader = gameObject.AddComponent<Loader>();
		instance._game = gameObject.AddComponent<AppGame>();
		instance._settings = gameObject.AddComponent<Settings>();
		instance._quality = gameObject.AddComponent<QualitySetter>();
		instance._const = gameObject.AddComponent<Const>();
		instance._metaMission = gameObject.AddComponent<MetaMission>();
		instance._visual = gameObject.AddComponent<Visual>();
		instance._tutorial = gameObject.AddComponent<Tutorial>();
		instance._gameEvents = gameObject.AddComponent<GameEvents>();
		instance._mainTheme = gameObject.AddComponent<MainTheme>();
		instance._perfMeter = gameObject.AddComponent<PerformanceMeter>();
		AudioSettings.outputSampleRate = (!(!instance._quality.HighQualityAudio) ? 44100 : 22050);
		GameObject gameObject2 = Util.CreateGameObject("GameData", "Assets/Prefabs/Basic");
		gameObject2.transform.parent = instance.transform;
		instance._gameData = gameObject2.GetComponent<GameData>();
		instance._playerData = gameObject.AddComponent<PlayerData>();
		instance._shopData = gameObject.AddComponent<ShopData>();
		instance._achievements = gameObject.AddComponent<Achievements>();
		instance._leaderboards = gameObject.AddComponent<Leaderboards>();
		GameObject gameObject3 = new GameObject("GameObjectsCache");
		gameObject3.transform.parent = gameObject.transform;
		instance._gameObjectsCache = gameObject3.AddComponent<GameObjectsCache>();
		GameObject gameObject4 = UnityEngine.Object.Instantiate(Resources.Load("UI")) as GameObject;
		gameObject4.transform.parent = gameObject.transform;
		gameObject4.RemoveClonePostfix();
		instance._ui = gameObject4.GetComponent<UI>();
		instance.fpsCounter = gameObject4.GetComponent<FpsCounter>();
		instance._gameData.Check();
	}

	public static void MakeSureItExists()
	{
		if (!instance)
		{
			Create();
		}
	}

	public static Singleton inst
	{
		get
		{
			MakeSureItExists();
			return instance;
		}
	}

	public static TimeControl TimeControl
	{
		get
		{
			return inst._time;
		}
	}

	public static Loader Loader
	{
		get
		{
			return inst._loader;
		}
	}

	public static AppGame Game
	{
		get
		{
			return inst._game;
		}
	}

	public static Settings Settings
	{
		get
		{
			return inst._settings;
		}
	}

	public static QualitySetter Quality
	{
		get
		{
			return inst._quality;
		}
	}

	public static UI UI
	{
		get
		{
			return inst._ui;
		}
	}

	public static GameObjectsCache Cache
	{
		get
		{
			return inst._gameObjectsCache;
		}
	}

	public static MetaMission MetaMission
	{
		get
		{
			return inst._metaMission;
		}
	}

	public static Visual Visual
	{
		get
		{
			return inst._visual;
		}
	}

	public static PlayerData PlayerData
	{
		get
		{
			return inst._playerData;
		}
	}

	public static ShopData ShopData
	{
		get
		{
			return inst._shopData;
		}
	}

	public static GameData GameData
	{
		get
		{
			return inst._gameData;
		}
	}

	public static Achievements Achievements
	{
		get
		{
			return inst._achievements;
		}
	}

	public static Leaderboards Leaderboards
	{
		get
		{
			return inst._leaderboards;
		}
	}

	public static Tutorial Tutorial
	{
		get
		{
			return inst._tutorial;
		}
	}

	public static GameEvents GameEvents
	{
		get
		{
			return inst._gameEvents;
		}
	}

	public static MainTheme MainTheme
	{
		get
		{
			return inst._mainTheme;
		}
	}

	public static FpsCounter FpsCounter
	{
		get
		{
			return inst.fpsCounter;
		}
	}

	public static void GC()
	{
		inst.StartGC(null);
	}

	public static void GC(GCCompletedDelegate onGCCompleted)
	{
		inst.StartGC(onGCCompleted);
	}

	private void StartGC(GCCompletedDelegate onGCCompleted)
	{
		StartCoroutine(ProcessGarbageCollection(onGCCompleted));
	}

	private IEnumerator ProcessGarbageCollection(GCCompletedDelegate onGCCompleted)
	{
		AsyncOperation async = Resources.UnloadUnusedAssets();
		yield return async;
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		if (onGCCompleted != null)
		{
			onGCCompleted();
		}
	}

	public static void InitAssetMapping()
	{
		string unusedLocal1;
		Strings.Locale = Strings.Locale;
		string text = string.Format("Assets/Localization/Replacement/{0}.asset", "en");
		AssetGroup assetGroup = (AssetGroup)AssetBundles.Load(text, typeof(AssetGroup));
		if (assetGroup == null)
		{
			Debug.LogError(string.Format("No assets for group '{0}'", text));
		}
		string text2 = string.Format("Assets/Localization/Replacement/{0}.asset", Strings.Locale);
		if (!AssetBundles.Contains(text2))
		{
			text2 = text;
		}
		AssetMapper.Instance.Reset();
		AssetMapper.Instance.TagGroup(assetGroup);
		AssetMapper.Instance.AddGroup(new AssetBundleGroupLoader(text2));
	}
}
