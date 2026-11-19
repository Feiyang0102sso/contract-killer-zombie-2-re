using UnityEngine;

public class AppGame : MonoBehaviourAndLog
{
	public bool PlayerWasInGame;

	public bool PlayHavenMissionEnd;

	public int LastPlayedMissionNumber = -1;

	private void Awake()
	{
		LogTo("AppGame.AppGame");
	}

	public void GoToGameLevel(GameData.MissionData mission, bool withFade)
	{
		GoToGameLevel(mission.Scene, mission.Type, withFade);
	}

	public void GoToGameLevel(string sceneName, Mission.Type missionType)
	{
		GoToGameLevel(sceneName, missionType, true);
	}

	public void GoToGameLevel(string sceneName, Mission.Type missionType, bool withFade)
	{
		Singleton.MetaMission.NewMission(missionType);
		PlayerWasInGame = true;
		PlayHavenMissionEnd = true;
		LastPlayedMissionNumber = Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted + 1;
		if (!Mission.IsUsualMission(missionType))
		{
			Singleton.PlayerData.InfectionLevel += Singleton.GameData.GetCurrentPlayerLevelData().InfectionLevelPerMission;
		}
		Singleton.Loader.LoadLevel(sceneName, new Loader.CustomLoadStep[2]
		{
			() =>
			{
				TexturesReplacer.ReplaceTexturesAccordingToQuality();
			},
			() =>
			{
				Shader.WarmupAllShaders();
			}
		}, withFade);
	}

	public void GoToCityMap()
	{
		GoToCityMap(true);
	}

	public void GoToCityMap(bool withFade)
	{
		GUIUtils.InputEnabled = false;
		Singleton.Loader.LoadLevel("CityMap", new Loader.CustomLoadStep[6]
		{
			() =>
			{
				Singleton.UI.PreloadWindows();
			},
			() =>
			{
				Singleton.ShopData.PreloadItemPrefabs();
			},
			() =>
			{
				TexturesReplacer.ReplaceTexturesAccordingToQuality();
			},
			() =>
			{
				Singleton.UI.Preinstantiate("GoalPanel", 8);
			},
			() =>
			{
				Singleton.UI.Preinstantiate("CumulativeGoalPanel", 2);
			},
			() =>
			{
				Shader.WarmupAllShaders();
			}
		}, withFade);
	}
}
