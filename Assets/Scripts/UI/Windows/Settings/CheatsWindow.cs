using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class CheatsWindow : UIWindow
{
	private delegate string TextSetter();

	private const int MaxButtons = 21;

	public UIButton SourceButton;

	public UIButton CheatsButton;

	public UIButton DebugButton;

	public UIButton MissionsButton;

	public UIButton CloseButton;

	private int _buttonsCount;

	private List<GameObject> _createdObjects = new List<GameObject>();

	protected override void Start()
	{
		base.Start();
		SourceButton.gameObject.SetActiveRecursively(false);
		Close();
	}

	private void OpenCheats()
	{
		Clear();
		bool isGameScene = Loader.IsGameScene();
		if (isGameScene)
		{
			AddCheatButton(() => string.Format("Invulnerable (O): {0}", Singleton.Settings.InvulnerablePlayer), () =>
			{
				Cheat.InvulnerablePlayer();
			});
			AddCheatButton(() => string.Format("Invuln. survivor (P): {0}", Singleton.Settings.InvulnerableSurvivor), () =>
			{
				Cheat.InvulnerableSurvivor();
			});
			AddCheatButton(() => "Kill enemies (K)", () =>
			{
				Cheat.KillEnemies();
				Close();
			});
			AddCheatButton(() => "Complete mission (M)", () =>
			{
				Cheat.CompleteMission();
				Close();
			});
			AddCheatButton(() => "Complete task (N)", () =>
			{
				Cheat.CompleteTask();
				Close();
			});
			AddCheatButton(() => "Resupply ammo (,)", () =>
			{
				Cheat.Resupply();
				Close();
			});
		}
		else
		{
			AddCheatButton(() => Singleton.PlayerData.InfectionLevel > 0f ? "Disinfect!" : "Infect!", () =>
			{
				Singleton.PlayerData.InfectionLevel = Singleton.PlayerData.InfectionLevel > 0f ? 0f : Singleton.GameData.InfectionLevel.Max;
			});
		}
		AddCheatButton(() => "Add 1kk cash", () =>
		{
			Singleton.PlayerData.SoftCurrency += 1000000;
		});
		AddCheatButton(() => "Add 1kk hard", () =>
		{
			Singleton.PlayerData.HardCurrency += 1000000;
		});
		AddCheatButton(() => "+1 Level", () =>
		{
			Singleton.PlayerData.XP += Singleton.PlayerData.NextLevelXP - Singleton.PlayerData.XP;
		});
		AddCheatButton(() => "-1 Level", () =>
		{
			Singleton.PlayerData.XP += Singleton.PlayerData.PrevLevelXP - Singleton.PlayerData.XP;
		});
		AddCheatButton(() => "Finish tutorial", () =>
		{
			if ((bool)Singleton.Tutorial)
			{
				Singleton.Tutorial.Finish();
				if (isGameScene)
				{
					Cheat.CompleteMission();
					Close();
				}
			}
		});
		AddCheatButton(() => "Reset progress", () =>
		{
			Singleton.PlayerData.ResetProgress();
		});
		AddCheatButton(() => "Restart app", () =>
		{
			Application.LoadLevel(0);
		});
		AddCheatButton(() => "Complete goal (.)", () =>
		{
			Cheat.CompleteGoal();
			Close();
		});
		AddCheatButton(() => "Simplify achievements", () =>
		{
			foreach (GameData.AchievementData achievementData in Singleton.GameData.Achievements)
			{
				achievementData.Count = 5;
			}
		});
	}

	private void OpenDebug()
	{
		Clear();
		bool flag = Loader.IsGameScene();
		AddCheatButton(() => string.Format("Show full fps stats: {0}", Singleton.FpsCounter.showFullFpsInfo), () =>
		{
			Util.Invert(ref Singleton.FpsCounter.showFullFpsInfo);
		});
		AddCheatButton(() => string.Format("Show fps: {0}", Singleton.FpsCounter.enabled), () =>
		{
			Singleton.FpsCounter.enabled = !Singleton.FpsCounter.enabled;
		});
		AddCheatButton(() => string.Format("Show debug info: {0}", Singleton.FpsCounter.showDebugInfo), () =>
		{
			Util.Invert(ref Singleton.FpsCounter.showDebugInfo);
		});
		AddCheatButton(() => "Fill memory", () =>
		{
			if (!Util.FindObjectOfType<MemoryFiller>())
			{
				new GameObject("MemoryFiller", typeof(MemoryFiller));
			}
			Close();
		});
		AddCheatButton(() => string.Format("{0} strings decoration", Boot.DecorationOfLocalizedStringsEnabled ? "Disable" : "Enable"), () =>
		{
			Boot.EnableDecorationOfLocalizedStrings(!Boot.DecorationOfLocalizedStringsEnabled);
		});
		AddCheatButton(() => "Quality Setup...", OpenQuality);
		AddCheatButton(() => string.Format("Language: {0}", Strings.Locale), () =>
		{
			Strings.Locale = Const.supportedLocales[(Const.supportedLocales.IndexOf(Strings.Locale) + 1) % Const.supportedLocales.Count];
			Singleton.InitAssetMapping();
		});
		AddCheatButton(() => "Load arena stage...", OpenArenaStages);
		AddCheatButton(() => "Complete all arena goals", () =>
		{
			ArenaWaves.CompleteAllCurrentGoals();
		});
		if (flag)
		{
			AddCheatButton(() => string.Format("Change visual effect: {0}", Scene.Camera.CameraEffect), () =>
			{
				Scene.Camera.CameraEffect++;
				if (Scene.Camera.CameraEffect >= MiniZombieCamera.Effect.Count)
				{
					Scene.Camera.CameraEffect = MiniZombieCamera.Effect.Normal;
				}
				Close();
			});
			AddCheatButton(() => string.Format("Combo Counter: {0}", Scene.inst.GetComponent<ComboCounter>() != null), () =>
			{
				if (!Scene.inst.GetComponent<ComboCounter>())
				{
					Scene.inst.gameObject.AddComponent<ComboCounter>();
				}
				else
				{
					UnityEngine.Object.Destroy(Scene.inst.GetComponent<ComboCounter>());
				}
			});
			AddCheatButton(() => string.Format("Zombie Update: {0}", Singleton.Settings.ZombieUpdate), () =>
			{
				Util.Invert(ref Singleton.Settings.ZombieUpdate);
				Scene.Spawner.UpdateZombieSettings();
			});
			AddCheatButton(() => string.Format("Zombie Render: {0}", Singleton.Settings.ZombieRender), () =>
			{
				Util.Invert(ref Singleton.Settings.ZombieRender);
				Scene.Spawner.UpdateZombieSettings();
			});
			AddCheatButton(() => string.Format("Zombie Animations: {0}", Singleton.Settings.ZombieAnimations), () =>
			{
				Util.Invert(ref Singleton.Settings.ZombieAnimations);
				Scene.Spawner.UpdateZombieSettings();
			});
			AddCheatButton(() => "Launch wave of zombies (L)", () =>
			{
				Cheat.LaunchWave();
				Close();
			});
			AddCheatButton(() => "Launch specific wave...", OpenWaves);
		}
		else
		{
		}
	}

	private void OpenArenaStages()
	{
		Clear();
		foreach (KeyValuePair<int, Dictionary<string, List<GameData.ArenaWavesData>>> arenaWave in Singleton.GameData.arenaWaves)
		{
			int wave = arenaWave.Key;
			AddCheatButton(() => string.Format("wave {0}", wave), () =>
			{
				PlayerData.Instance.CurrentLocation = string.Empty;
				PlayerData.Instance.startFrom = wave.ToString();
				Singleton.Game.GoToGameLevel("Indoor_Survival_grey", Mission.Type.SurvivalArena);
			});
		}
	}

	private void OpenWaves()
	{
		Clear();
		GameObject list = Util.Find("/WavesList");
		if (!list)
		{
			list = UnityEngine.Object.Instantiate(Resources.Load("WavesList")) as GameObject;
			list.name = "WavesList";
		}
		foreach (GameObject gameObject in list.GetComponent<WavesList>().waves)
		{
			if (!gameObject)
			{
				continue;
			}
			GameObject wavePrefab = gameObject;
			AddCheatButton(() => wavePrefab.name, () =>
			{
				GameObject gameObject2 = list.QuickFindChild(wavePrefab.name);
				if (!gameObject2)
				{
					gameObject2 = UnityEngine.Object.Instantiate(wavePrefab) as GameObject;
					gameObject2.transform.parent = list.transform;
					gameObject2.RemoveClonePostfix();
				}
				gameObject2.transform.position = Scene.Player.transform.position + Scene.MainCamera.transform.forward * 3f;
				gameObject2.SendMessage("OnActivate");
				Close();
			});
		}
	}

	private void OpenMissions()
	{
		OpenMissions(0);
	}

	private void OpenMissions(int page)
	{
		Clear();
		int count = 20;
		int num = count * page;
		for (int i = num; i < Singleton.GameData.Missions.Length; i++)
		{
			if (i >= num + count)
			{
				AddCheatButton(() => string.Format("Next Page ({0}-{1})", (page + 1) * count + 1, Mathf.Min((page + 2) * count, Singleton.GameData.Missions.Length)), () =>
				{
					OpenMissions(page + 1);
				});
				break;
			}
			GameData.MissionData data = Singleton.GameData.Missions[i];
			AddCheatButton(() => data.Descriptor.Replace("_", "-"), () =>
			{
				PlayerData.Instance.CurrentLocation = string.Empty;
				Singleton.Game.GoToGameLevel(data.Scene, data.Type);
			});
		}
	}

	private void OpenQuality()
	{
		Clear();
		AddCheatButton(() => "Preset:" + QualitySettings.names[QualitySettings.GetQualityLevel()], () =>
		{
			QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length, true);
			Singleton.Quality.UpdateQualitySetup();
			OpenQuality();
		});
		AddCheatButton(() => "CustomTextureQuality:" + Singleton.Quality.CustomTextureQuality.ToString(), () =>
		{
			Singleton.Quality.CustomTextureQuality = IterateEnum(Singleton.Quality.CustomTextureQuality);
		});
		AddCheatButton(() => "BloomEffect:" + Singleton.Quality.BloomEffect.ToString(), () =>
		{
			Singleton.Quality.BloomEffect = !Singleton.Quality.BloomEffect;
			Scene.Camera.SetEffect(PostEffectManager.PostEffect.Normal, -1f);
		});
		AddCheatButton(() => "LowHealthEffect:" + Singleton.Quality.LowHealthEffect.ToString(), () =>
		{
			Singleton.Quality.LowHealthEffect = !Singleton.Quality.LowHealthEffect;
			Scene.Camera.SetEffect(PostEffectManager.PostEffect.Normal, -1f);
		});
		AddCheatButton(() => "VolumeLightOnScene:" + Singleton.Quality.VolumeLightOnScene.ToString(), () =>
		{
			Singleton.Quality.VolumeLightOnScene = !Singleton.Quality.VolumeLightOnScene;
		});
		AddCheatButton(() => "GarbageOnScene:" + Singleton.Quality.GarbageOnScene.ToString(), () =>
		{
			Singleton.Quality.GarbageOnScene = !Singleton.Quality.GarbageOnScene;
		});
		AddCheatButton(() => "MiscObjectsOnScene:" + Singleton.Quality.MiscObjectsOnScene.ToString(), () =>
		{
			Singleton.Quality.MiscObjectsOnScene = !Singleton.Quality.MiscObjectsOnScene;
		});
		AddCheatButton(() => "DecalsOnScene:" + Singleton.Quality.DecalsOnScene.ToString(), () =>
		{
			Singleton.Quality.DecalsOnScene = !Singleton.Quality.DecalsOnScene;
		});
		AddCheatButton(() => "HitEffectsOnScene:" + Singleton.Quality.HitEffectsOnScene.ToString(), () =>
		{
			Singleton.Quality.HitEffectsOnScene = !Singleton.Quality.HitEffectsOnScene;
		});
		AddCheatButton(() => "ShellEffectsOnScene:" + Singleton.Quality.ShellEffectsOnScene.ToString(), () =>
		{
			Singleton.Quality.ShellEffectsOnScene = !Singleton.Quality.ShellEffectsOnScene;
		});
		AddCheatButton(() => "RagdollsOnScene:" + Singleton.Quality.RagdollsOnScene.ToString(), () =>
		{
			Singleton.Quality.RagdollsOnScene = !Singleton.Quality.RagdollsOnScene;
		});
		AddCheatButton(() => "ComplexShadows:" + Singleton.Quality.ComplexShadows.ToString(), () =>
		{
			Singleton.Quality.ComplexShadows = !Singleton.Quality.ComplexShadows;
		});
		AddCheatButton(() => "HitSoundsOnScene:" + Singleton.Quality.HitSoundsOnScene.ToString(), () =>
		{
			Singleton.Quality.HitSoundsOnScene = !Singleton.Quality.HitSoundsOnScene;
		});
		AddCheatButton(() => "NewZombies:" + Singleton.Quality.NewZombies.ToString(), () =>
		{
			Singleton.Quality.NewZombies = !Singleton.Quality.NewZombies;
		});
		AddCheatButton(() => "ZombiesLimit:" + Singleton.Quality.ZombiesLimit.ToString(), () =>
		{
			Singleton.Quality.ZombiesLimit = ++Singleton.Quality.ZombiesLimit % 16;
		});
		AddCheatButton(() => "HudBloodSplashes:" + Singleton.Quality.HudBloodSplashes.ToString(), () =>
		{
			Singleton.Quality.HudBloodSplashes = !Singleton.Quality.HudBloodSplashes;
		});
		AddCheatButton(() => "AdvancedUIAnimations:" + Singleton.Quality.AdvancedUIAnimations.ToString(), () =>
		{
			Singleton.Quality.AdvancedUIAnimations = !Singleton.Quality.AdvancedUIAnimations;
		});
		AddCheatButton(() => "Shader LOD:" + Shader.globalMaximumLOD, () =>
		{
			NextShaderLODLevel();
		});
		AddCheatButton(() => "AnisotropicFiltering:" + QualitySettings.anisotropicFiltering, () =>
		{
			QualitySettings.anisotropicFiltering = IterateEnum(QualitySettings.anisotropicFiltering);
		});
		AddCheatButton(() => "AntiAliasing:" + QualitySettings.antiAliasing, () =>
		{
			QualitySettings.antiAliasing = QualitySettings.antiAliasing == 8 ? 0 : (int)Mathf.Pow(2f, QualitySettings.antiAliasing / 2 + 1);
		});
		AddCheatButton(() => "MasterTextureLimit:" + QualitySettings.masterTextureLimit, () =>
		{
			QualitySettings.masterTextureLimit = QualitySettings.masterTextureLimit > 10 ? 0 : QualitySettings.masterTextureLimit + 1;
		});
		AddCheatButton(() => "BlendWeights:" + QualitySettings.blendWeights, () =>
		{
			QualitySettings.blendWeights = IterateEnum(QualitySettings.blendWeights);
		});
	}

	private void Clear()
	{
		foreach (GameObject createdObject in _createdObjects)
		{
			UnityEngine.Object.Destroy(createdObject);
		}
		_createdObjects.Clear();
		_buttonsCount = 0;
	}

	private void AddCheatButton(TextSetter textSetter, Action action)
	{
		float unusedLocal1;
		float unusedLocal2;
		UIButton button = (UnityEngine.Object.Instantiate(SourceButton.gameObject) as GameObject).GetComponent<UIButton>();
		button.spriteText.isClone = true;
		_createdObjects.Add(button.gameObject);
		button.gameObject.SetActiveRecursively(true);
		button.transform.parent = SourceButton.transform.parent;
		button.transform.localPosition = new Vector3((float)(_buttonsCount % 3 - 1) * 85f, (float)(-(_buttonsCount / 3)) * 22f, 0f);
		button.spriteText.Text = textSetter();
		GUIUtils.HookButton(button, () =>
		{
			action();
			button.spriteText.Text = textSetter();
		});
		_buttonsCount++;
	}

	private void NextShaderLODLevel()
	{
		int globalMaximumLOD = Shader.globalMaximumLOD;
		if (globalMaximumLOD < 600)
		{
			globalMaximumLOD += 100;
		}
		else if (globalMaximumLOD == 600)
		{
			globalMaximumLOD = 650;
		}
		else
		{
			globalMaximumLOD = 200;
		}
		Shader.globalMaximumLOD = globalMaximumLOD;
	}

	private T IterateEnum<T>(T e) where T : struct, IConvertible
	{
		T[] array = (T[])Enum.GetValues(e.GetType());
		return array[(Array.IndexOf(array, e) + 1) % array.Length];
	}
}
