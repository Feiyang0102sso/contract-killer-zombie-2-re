using UnityEngine;

public class MissionReward
{
	public int experience;

	public int softCurrency;

	public int hardCurrency;

	private bool rewardsGiven;

	public MissionReward()
	{
		hardCurrency = Scene.Mission.bonusHardCurrency;
	}

	public virtual void GiveRewards()
	{
		if (rewardsGiven)
		{
			return;
		}
		rewardsGiven = true;
		PlayerData.Instance.XP = PlayerData.Instance.XP + experience;
		PlayerData.Instance.SoftCurrency = PlayerData.Instance.SoftCurrency + softCurrency;
		PlayerData.Instance.HardCurrency = PlayerData.Instance.HardCurrency + hardCurrency;
		if (hardCurrency > 0)
		{
			GameAnalytics.LogHCReceived(hardCurrency, GameAnalytics.HCSourceType.Game);
		}
		if (PlayerData.Instance.CurrentLocation != string.Empty)
		{
			PlayerData.Instance.CompleteMission(PlayerData.Instance.CurrentLocation);
		}
		PlayerData.Instance.CurrentLocation = string.Empty;
		PlayerData.Instance.SaveOnce();
	}
}

public class BasicMissionReward : MissionReward
{
	public int experienceBase;

	public int experienceBonus;

	public int experienceKills;

	public int experienceSpecialsKills;

	public int experienceDismembered;

	public int experienceHeadshots;

	public int experienceCrates;

	public int softCurrencyBase;

	public int softCurrencyBonus;

	public int softCurrencyKills;

	public int softCurrencySpecialsKills;

	public int softCurrencyDismembered;

	public int softCurrencyHeadshots;

	public int softCurrencyCrates;

	public BasicMissionReward()
		: this(Singleton.GameData.RewardUsual)
	{
	}

	public BasicMissionReward(GameData.RewardInfo reward)
	{
		MissionStatistics lastMissionStatistics = Singleton.PlayerData.LastMissionStatistics;
		int totalNumWeaponHeadshots = lastMissionStatistics.TotalNumWeaponHeadshots;
		int totalNumWeaponDismemberments = lastMissionStatistics.TotalNumWeaponDismemberments;
		int totalNumKills = lastMissionStatistics.TotalNumKills;
		int totalNumSpecialKills = lastMissionStatistics.TotalNumSpecialKills;
		experienceBase = Scene.Mission.ExperienceBase();
		experienceKills = totalNumKills * reward.XPPerZombie;
		experienceSpecialsKills = totalNumSpecialKills * reward.XPPerSpecial;
		experienceHeadshots = totalNumWeaponHeadshots * reward.XPPerHeadshot;
		experienceDismembered = totalNumWeaponDismemberments * reward.XPPerDismemberment;
		experienceCrates = Mathf.CeilToInt((float)experienceBase * Scene.Mission.bonusExperiencePercent / 100f);
		experienceBonus = experienceSpecialsKills + experienceCrates + experienceKills + experienceHeadshots + experienceDismembered;
		experience = experienceBase + experienceBonus;
		softCurrencyBase = Scene.Mission.SoftCurrencyBase();
		softCurrencyKills = totalNumKills * reward.SCPerZombie;
		softCurrencySpecialsKills = totalNumSpecialKills * reward.SCPerSpecial;
		softCurrencyHeadshots = totalNumWeaponHeadshots * reward.SCPerHeadshot;
		softCurrencyDismembered = totalNumWeaponDismemberments * reward.SCPerDismemberment;
		softCurrencyCrates = Mathf.CeilToInt((float)softCurrencyBase * Scene.Mission.bonusSoftCurrencyPercent / 100f);
		softCurrencyBonus = softCurrencyCrates + softCurrencyDismembered + softCurrencyHeadshots;
		softCurrency = softCurrencyBase + softCurrencyBonus;
	}
}

public class SurvivalGatesMissionReward : BasicMissionReward
{
	public SurvivalGatesMissionReward()
		: base(Singleton.GameData.RewardSurvivalGates)
	{
	}
}

public class ArenaMissionReward : MissionReward
{
	public ArenaMissionReward(ArenaWaves arena)
	{
		experience = arena.xpTotal + Mathf.CeilToInt((float)Scene.Mission.ExperienceBase() * Scene.Mission.bonusExperiencePercent / 100f);
		softCurrency = arena.softTotal + Mathf.CeilToInt((float)Scene.Mission.SoftCurrencyBase() * Scene.Mission.bonusSoftCurrencyPercent / 100f);
		hardCurrency += arena.hardTotal;
	}

	public override void GiveRewards()
	{
		base.GiveRewards();
	}
}
