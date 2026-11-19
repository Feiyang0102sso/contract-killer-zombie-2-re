using System;
using Glu.Localization;
using UnityEngine;

public class MissionCompletePopup : UIWindow
{
	private const float topOffset = 19f;

	public UIButton OkButton;

	public SpriteText RewardCash;

	public SpriteText RewardXP;

	public SpriteText RewardHard;

	public GameStatsWidget GameStat;

	public UISingleSprite ImageRight;

	protected override void Awake()
	{
		base.Awake();
	}

	public void Init(BasicMissionReward reward)
	{
		MissionStatistics lastMissionStatistics = Singleton.PlayerData.LastMissionStatistics;
		int totalNumKills = lastMissionStatistics.TotalNumKills;
		int totalNumSpecialKills = lastMissionStatistics.TotalNumSpecialKills;
		int totalNumWeaponHeadshots = lastMissionStatistics.TotalNumWeaponHeadshots;
		int totalNumWeaponDismemberments = lastMissionStatistics.TotalNumWeaponDismemberments;
		int num = lastMissionStatistics.TotalWeaponDamage + lastMissionStatistics.TotalConsumablesDamage;
		float num2 = (int)(Singleton.PlayerData.PlayingTime - Singleton.PlayerData.PlayerStatistics.MissionStartTime);
		int bonusesCollected = Scene.Mission.bonusesCollected;
		int num3 = Mathf.Max(Scene.Mission.bonusesCollected, Scene.Mission.bonusesTotal);
		GameStat.AddStat(Strings.GetString("Zombies Killed"), totalNumKills.ToString());
		GameStat.AddStat(Strings.GetString("Special Zombies Killed"), totalNumSpecialKills);
		GameStat.AddStat(Strings.GetString("Headshots"), totalNumWeaponHeadshots);
		GameStat.AddStat(Strings.GetString("Dismembered Zombies"), totalNumWeaponDismemberments);
		GameStat.AddStat(Strings.GetString("Bonus Crates"), bonusesCollected.ToString() + " / " + num3);
		RewardXP.Text = string.Concat(Color.green, string.Empty, (reward.experienceBase + reward.experienceBonus).ToString());
		RewardCash.Text = string.Concat(Color.green, string.Empty, (reward.softCurrencyBase + reward.softCurrencyBonus).ToString());
		if (reward.hardCurrency > 0)
		{
			RewardHard.Text = string.Concat(GUIUtils.ColorYellowHard, reward.hardCurrency.ToString());
		}
		else
		{
			RewardXP.transform.Translate(25.49434f, 0f, 0f, Space.World);
			RewardCash.transform.Translate(25.49434f, 0f, 0f, Space.World);
			RewardHard.gameObject.SetActiveRecursively(false);
		}
		RewardXP.gameObject.AddComponent<UIAnimationMove>().Setup(-100f, 0f, 2f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardXP.gameObject.AddComponent<UIAnimationScale>().Setup(10f, 1f, 2f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardXP.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 2f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardCash.gameObject.AddComponent<UIAnimationMove>().Setup(100f, 0f, 3f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardCash.gameObject.AddComponent<UIAnimationScale>().Setup(10f, 1f, 3f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardCash.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 3f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardHard.gameObject.AddComponent<UIAnimationMove>().Setup(-100f, 0f, 4f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardHard.gameObject.AddComponent<UIAnimationScale>().Setup(10f, 1f, 4f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		RewardHard.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 4f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		OkButton.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 5f, 0.7f, EZAnimation.EASING_TYPE.ExponentialIn);
		GUIUtils.HookButton(OkButton, () =>
		{
			Close();
			Singleton.Game.GoToCityMap();
		});
		OnShowDelegate += (Action)(() =>
		{
			gameObject.AddComponent<UIAnimationSkipper>();
		});
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			ImageRight.SetTexture(Resources.Load("LoadingScreens/MissionCompleteBg") as Texture2D);
		}
	}

	private string BonusString(int bonus)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("(+{0})", bonus));
	}

	private string BonusStringParensless(int bonus)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("+{0}", bonus));
	}

	private string BonusXPString(int bonusXP)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("(+{0} {1})", bonusXP, string.Concat(Color.white, Strings.GetString("XP"), GUIUtils.ColorGreenUI)));
	}

	private string BonusSCString(int bonusSC)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("(+{0} {1})", bonusSC, string.Concat(Color.white, GUIUtils.GetSymbolForCurrency("soft"), GUIUtils.ColorGreenUI)));
	}
}
