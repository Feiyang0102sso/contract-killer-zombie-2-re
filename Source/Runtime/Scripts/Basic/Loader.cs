using System;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
	public enum LoaderState
	{
		Loading,
		Ready
	}

	public delegate void CustomLoadStep();

	private LevelInfo[] _levels;

	private static HashSet<string> notScenes = new HashSet<string> { "_Start", "CityMap", "Hud", "Loading", "_Intro" };

	private LevelInfo levelToLoad;

	private CustomLoadStep[] _customLoadSteps;

	public Action OnSceneLoaded;

	private static string _currentScene;

	public static LoaderState State = LoaderState.Ready;

	public LevelInfo[] Levels
	{
		get
		{
			return _levels;
		}
	}

	public LevelInfo LevelToLoad
	{
		get
		{
			return levelToLoad;
		}
		set
		{
			levelToLoad = value;
		}
	}

	public CustomLoadStep[] CustomLoadSteps
	{
		get
		{
			return _customLoadSteps;
		}
		set
		{
			_customLoadSteps = value;
		}
	}

	private void Awake()
	{
		LevelsList component = (UnityEngine.Object.Instantiate(Resources.Load("LevelsList")) as GameObject).GetComponent<LevelsList>();
		_levels = component.levels;
		UnityEngine.Object.Destroy(component.gameObject);
	}

	public void LoadLevel(string sceneName)
	{
		LoadLevel(sceneName, null, true);
	}

	public void LoadLevel(string sceneName, CustomLoadStep[] customLoadSteps, bool withFade)
	{
		LevelInfo levelInfo = new LevelInfo();
		levelInfo.sceneName = sceneName;
		LoadLevel(levelInfo, customLoadSteps, withFade);
	}

	public void LoadLevel(LevelInfo level)
	{
		LoadLevel(level, null, true);
	}

	public void LoadLevel(LevelInfo level, CustomLoadStep[] customLoadSteps, bool withFade)
	{
		if (!(!Singleton.UI.Fader.IsFading()))
		{
			return;
		}
		// PC port: leaving a PitTrial level puts the player state back before anything else loads
		PitTrial.OnLoadLevel(level.sceneName);
		_customLoadSteps = customLoadSteps;
		Singleton.MetaMission.EndPart();
		levelToLoad = level;
		Singleton.UI.Fader.OnFadeComplete = (SpriteFader.State state) =>
		{
			Singleton.UI.Fader.OnFadeComplete = null;
			CleanUp();
			Application.LoadLevel("Loading");
		};
		if (withFade)
		{
			Singleton.UI.Fader.FadeIn(0.5f, Color.black);
		}
		else
		{
			Singleton.UI.Fader.OnFadeComplete(SpriteFader.State.FadeIn);
		}
		State = LoaderState.Loading;
	}

	public void OnLoadingFinished()
	{
		levelToLoad = null;
	}

	public static bool IsCityMapScene()
	{
		return _currentScene == "CityMap";
	}

	public static bool IsGameScene()
	{
		return IsGameScene(new LevelInfo(_currentScene));
	}

	public static bool IsGameScene(LevelInfo level)
	{
		return level.sceneName != null && level.sceneName.Length > 0 && !notScenes.Contains(level.sceneName);
	}

	private void CleanUp()
	{
		Util.ResetFind();
		Util.ResetFindChild();
		Singleton.FpsCounter.debugInfo = null;
		if (IsGameScene())
		{
			if ((bool)Util.Find("/Hud"))
			{
				UnityEngine.Object.Destroy(Util.Find("/Hud"));
			}
			if ((bool)Util.Find("/$Scene"))
			{
				UnityEngine.Object.Destroy(Util.Find("/$Scene"));
			}
		}
	}

	public void PreLoad()
	{
		_currentScene = levelToLoad.sceneName;
		Util.ClearCaches();
		GUIUtils.ClearCaches();
		TexturesReplacer.Clear();
		Singleton.UI.Clear();
		Singleton.Cache.PerStringCache.Clear();
		Singleton.Cache.PerPrefabCache.Clear();
		Singleton.Cache.PerResourceCache.Clear();
		if (IsGameScene(levelToLoad))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Hud"), Vector3.zero, Quaternion.identity) as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.RemoveClonePostfix();
			GameObject target = new GameObject("$Scene", typeof(Scene));
			UnityEngine.Object.DontDestroyOnLoad(target);
		}
	}

	public void PostLoad()
	{
		Singleton.GC();
		bool flag = IsGameScene();
		Input.multiTouchEnabled = flag;
		Singleton.PlayerData.UpdateAllowed = !flag;
		State = LoaderState.Ready;
		if (OnSceneLoaded != null)
		{
			OnSceneLoaded();
		}
		if (flag)
		{
			Invoke("PostLoadDelayed", 1f);
		}
	}

	private void PostLoadDelayed()
	{
		Singleton.FpsCounter.Reset();
	}
}
