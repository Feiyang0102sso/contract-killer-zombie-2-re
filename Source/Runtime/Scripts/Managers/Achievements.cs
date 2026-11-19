using System;
using UnityEngine;

public class Achievements : MonoBehaviourAndLog
{
	private Func<int>[] checkFunctions = new Func<int>[32];

	private void Awake()
	{
		LogTo("Other.Achievements");
		PlayerStatistics playerStats = Singleton.PlayerData.PlayerStatistics;
		checkFunctions[1] = () => playerStats.NumMissionsCompleted;
		checkFunctions[2] = () => playerStats.NumSurvivorsRescued;
		checkFunctions[3] = () => playerStats.NumSuppliesGathered;
		checkFunctions[4] = () => playerStats.TotalNumKills;
		checkFunctions[5] = () => playerStats.TotalNumSpecialKills;
		checkFunctions[6] = () => playerStats.TotalNumWeaponDismemberments;
		checkFunctions[7] = () => playerStats.NumBonusCollected;
		checkFunctions[8] = () => playerStats.TotalNumWeaponHeadshots;
		checkFunctions[9] = () => playerStats.GetEnemyKillsBySlot(PlayerData.Slots.assault);
		checkFunctions[10] = () => playerStats.TotalNumConsumableKills + playerStats.GetEnemyKillsBySlot(PlayerData.Slots.exotic);
		checkFunctions[11] = () => playerStats.GetEnemyKillsBySlot(PlayerData.Slots.melee);
		checkFunctions[12] = () => playerStats.GetEnemyKillsBySlot(PlayerData.Slots.handgun);
		checkFunctions[13] = () => playerStats.GetEnemyKillsBySlot(PlayerData.Slots.shotgun);
		checkFunctions[14] = () => playerStats.GetEnemyKillsBySlot(PlayerData.Slots.sniper);
		checkFunctions[15] = () => playerStats.GetWeaponShotsBySlot(PlayerData.Slots.handgun);
		checkFunctions[16] = () => playerStats.GetWeaponShotsBySlot(PlayerData.Slots.shotgun);
		checkFunctions[17] = () => playerStats.GetWeaponShotsBySlot(PlayerData.Slots.sniper);
		checkFunctions[18] = () => playerStats.GetWeaponShotsBySlot(PlayerData.Slots.assault);
		checkFunctions[19] = () => playerStats.GetWeaponShotsBySlot(PlayerData.Slots.exotic);
		checkFunctions[20] = () => playerStats.NumWeaponsPurchased;
	}

	public bool IsImplemented(GameData.AchievementData achievement)
	{
		return checkFunctions[achievement.Id] != null;
	}

	public float GetAchievementValue(GameData.AchievementData achievement)
	{
		if (!IsImplemented(achievement))
		{
			return 0f;
		}
		return checkFunctions[achievement.Id]();
	}

	public float GetAchievementProgress(GameData.AchievementData achievement)
	{
		return Mathf.Clamp01(GetAchievementValue(achievement) / (float)achievement.Count);
	}

	public void CheckAchievements()
	{
		bool authenticated = Social.localUser.authenticated;
		foreach (GameData.AchievementData achievementData in Singleton.GameData.Achievements)
		{
			float achievementProgress = GetAchievementProgress(achievementData);
			if (authenticated)
			{
				Social.ReportProgress(achievementData.GameCenterId, achievementProgress * 100f, (bool result) =>
				{
				});
			}
			if (Singleton.PlayerData.IsAchievementCompleted(achievementData))
			{
				continue;
			}
			if (achievementProgress >= 1f)
			{
				Complete(achievementData);
			}
		}
	}

	private void Complete(GameData.AchievementData achievement)
	{
		GameAnalytics.LogAchievementCompleted(achievement.GameCenterId);
		Singleton.PlayerData.CompleteAchievement(achievement);
		AchievementPlate.PopupComplete(achievement);
	}
}
