using System;
using Glu.Localization;
using UnityEngine;

public class SurvivalModeFailPopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton FacebookButton;

	public UIExpandSprite Back;

	public SpriteText RewardCash;

	public SpriteText RewardXP;

	public SpriteText LeadName;

	public SpriteText LeadScore;

	public SpriteText AdviceText;

	public GameStatsWidget GameStat;

	private ConsumableShopItem _consumableItem;

	public ShopItemWidget ReviveWidget;

	public LeaderboardsWidget LeaderboardsWidget;

	public UISingleSprite ImageLeft;

	public UISingleSprite ImageRight;

	private int _initialCountOfConsumableItems;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		Back.SetSize(GUIUtils.ScreenWidth(), GUIUtils.ScreenHeight());
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public static void FillSurvivalStats(GameStatsWidget statWidget)
	{
		MissionStatistics lastMissionStatistics = Singleton.PlayerData.LastMissionStatistics;
		int numWavesCompleted = lastMissionStatistics.NumWavesCompleted;
		int totalNumKills = lastMissionStatistics.TotalNumKills;
		int num = lastMissionStatistics.TotalNumSpawns();
		int totalNumSpecialKills = lastMissionStatistics.TotalNumSpecialKills;
		int totalNumWeaponHeadshots = lastMissionStatistics.TotalNumWeaponHeadshots;
		int totalNumWeaponDismemberments = lastMissionStatistics.TotalNumWeaponDismemberments;
		int num2 = lastMissionStatistics.TotalWeaponDamage + lastMissionStatistics.TotalConsumablesDamage;
		Gate gate = Util.FindObjectOfType<Gate>();
		int num3 = (gate == null ? 0 : ((int)gate.Health));
		int num4 = (gate == null ? 0 : ((int)gate.healthTotal));
		float num5 = (int)(Singleton.PlayerData.PlayingTime - Singleton.PlayerData.PlayerStatistics.MissionStartTime);
		statWidget.SetSpacingY(10f);
		statWidget.AddStat(Strings.GetString("Waves N"), numWavesCompleted);
		statWidget.AddStat(Strings.GetString("Zombies Killed"), totalNumKills, BonusString(lastMissionStatistics.SurvivalScoreZombies()));
		statWidget.AddStat(Strings.GetString("Special Zombies Killed"), totalNumSpecialKills, BonusString(lastMissionStatistics.SurvivalScoreSpecials()));
		statWidget.AddStat(Strings.GetString("Headshots"), totalNumWeaponHeadshots, BonusString(lastMissionStatistics.SurvivalScoreHeadshots()));
		statWidget.SetSpacingY(11f);
		statWidget.AddStat(Strings.GetString("Score"), string.Empty, string.Concat(string.Empty, GUIUtils.ColorGreenUI, lastMissionStatistics.SurvivalScoreTotal()));
		foreach (SpriteText spriteText in statWidget.LastAddedStatGroup.GetComponentsInChildren<SpriteText>())
		{
			spriteText.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
		}
	}

	public void Init(SurvivalGatesMissionReward reward)
	{
		FillSurvivalStats(GameStat);
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			ImageLeft.SetTexture(Resources.Load("LoadingScreens/loading_back_1") as Texture2D);
			ImageRight.SetTexture(Resources.Load("LoadingScreens/loading_back_r") as Texture2D);
		}
		RewardXP.Text = reward.experience.ToString();
		if (RewardCash != null)
		{
			RewardCash.Text = string.Concat(Color.white, GUIUtils.GetSymbolForCurrency("soft"), " ", GUIUtils.ColorGreyUI, reward.softCurrency);
		}
		bool defibrillatorOnFailEnabled = Singleton.GameData.DefibrillatorOnFailEnabled;
		if (defibrillatorOnFailEnabled)
		{
			AdviceText.Text = Strings.GetString("USE TOOLBOX TO REPAIR GATES\n AND CONTINUE ON MISSION");
		}
		else
		{
			AdviceText.transform.Translate(0f, -15f, 0f);
			AdviceText.Text = Strings.GetString("Equip better weapons to deal with tougher zombies, use medkits and power ups to aid you in battle, throw beacon grenades to distract enemies!");
		}
		_consumableItem = Singleton.ShopData.GetConsumableShopItemByItem("Toolbox");
		_consumableItem.basePrice = Mathf.Min(5000, Util.FibonacciNumber(1 + ++Util.FindObjectOfType<Gate>().repairedTimes));
		_initialCountOfConsumableItems = Singleton.PlayerData.GetConsumableCount(_consumableItem.item);
		ReviveWidget.SetItem(_consumableItem);
		(ReviveWidget.StoreFrontItem as ConsumableStoreFrontItemPlate).ShowUseButton = true;
		_consumableItem.OnUse += new Action(OnRevive);
		ReviveWidget.gameObject.SetActiveRecursively(defibrillatorOnFailEnabled);
		GUIUtils.HookButton(CloseButton, () =>
		{
			if ((bool)Scene.Mission)
			{
				Scene.Mission.OnRevived(false, _initialCountOfConsumableItems == 0);
			}
			Close();
		});
		MissionStatistics statistics = Singleton.PlayerData.LastMissionStatistics;
		if (QualitySetter.FacebookEnabled)
		{
			GUIUtils.HookButton(FacebookButton, () =>
			{
				if (!Util.DoRarelyReal(this, "fb_antispam", 1.5f))
				{
					return;
				}
				bool posted = FacebookManager.PostSMResultMessageToWall(statistics.SurvivalScoreTotal(), Strings.GetString("Survival Mode"));
				if (posted)
				{
					UnityEngine.Object.Destroy(FacebookButton.gameObject);
				}
			});
		}
		else
		{
			UnityEngine.Object.Destroy(FacebookButton.gameObject);
		}
		LeaderboardsWidget.Populate(Leaderboards.LeaderboardType.Survival1);
	}

	private static string BonusString(int bonus)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("(+{0})", bonus));
	}

	private void OnRevive()
	{
		Close();
		if ((bool)Scene.Mission)
		{
			Scene.Mission.OnRevived(true, _initialCountOfConsumableItems == 0);
		}
		Singleton.PlayerData.LastMissionStatistics.NumToolkitsPurchased++;
	}

	protected override void OnDestroy()
	{
		GameAnalytics.LogSurvivalWaveCompleted(Singleton.PlayerData.Level, Singleton.PlayerData.LastMissionStatistics.NumWavesCompleted, Singleton.PlayerData.LastMissionStatistics.SurvivalScoreTotal(), Singleton.PlayerData.LastMissionStatistics.NumToolkitsPurchased);
		if (_consumableItem != null)
		{
			_consumableItem.OnUse -= new Action(OnRevive);
		}
		base.OnDestroy();
	}
}
