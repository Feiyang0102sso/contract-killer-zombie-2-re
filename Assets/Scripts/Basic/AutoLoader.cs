using UnityEngine;

public class AutoLoader : MonoBehaviour
{
	public string LoadOtherScene = string.Empty;

	private bool simulateLoader;

	private void Awake()
	{
		Singleton.MakeSureItExists();
		if (enabled && gameObject.active)
		{
			if (Application.loadedLevelName == "Hud")
			{
				Object.DestroyImmediate(Util.Find("/Hud"));
				Object.DestroyImmediate(Util.Find("/UI"));
			}
			if (LoadOtherScene != string.Empty)
			{
				Singleton.Loader.LoadLevel(LoadOtherScene);
				Object.DestroyImmediate(gameObject);
			}
			else
			{
				if (Singleton.Loader.LevelToLoad == null)
				{
					LevelInfo levelInfo = new LevelInfo();
					levelInfo.sceneName = Application.loadedLevelName;
					Singleton.Loader.LevelToLoad = levelInfo;
					Singleton.Loader.PreLoad();
					simulateLoader = true;
					if (Singleton.Tutorial != null)
					{
						if (Application.loadedLevelName.ToLower().Contains("green_tutorial") || Application.loadedLevelName.ToLower().Contains("outdoor_store_tutorial"))
						{
							Singleton.Tutorial.Init(0);
						}
						else if (Application.loadedLevelName.ToLower().Contains("outdoor_gasstation_day_story2"))
						{
							Singleton.Tutorial.Init(2);
						}
						else
						{
							Singleton.Tutorial.Finish();
						}
					}
				}
			}
		}
	}

	private void Start()
	{
		if (Loader.IsGameScene())
		{
			Scene.inst.Init();
		}
	}

	private void Update()
	{
		if (simulateLoader)
		{
			Singleton.Loader.PostLoad();
		}
		Object.DestroyImmediate(gameObject);
	}
}
