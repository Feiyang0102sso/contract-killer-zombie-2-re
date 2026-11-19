using UnityEngine;

public class SurvivalModeBetweenPopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton NextButton;

	public UIExpandSprite Back;

	public SpriteText RewardCash;

	public SpriteText RewardXP;

	public ShopItemWidget RecWeapon1Widget;

	public ShopItemWidget RecWeapon2Widget;

	public UISingleSprite ImageLeft;

	public UISingleSprite ImageRight;

	public GameStatsWidget GameStat;

	protected override void Awake()
	{
		base.Awake();
		GUIUtils.HookButton(CloseButton, Close);
	}

	protected override void Start()
	{
		base.Start();
		Back.SetSize(GUIUtils.ScreenWidth(), GUIUtils.ScreenHeight());
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public void Init(SurvivalGatesMissionReward reward)
	{
		SurvivalModeFailPopup.FillSurvivalStats(GameStat);
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
		RecWeapon1Widget.SetItem("GrenadeBeacon");
		RecWeapon2Widget.SetItem("DamageBoost");
		GUIUtils.DelayButtonFunctionality((RecWeapon1Widget.StoreFrontItem as ConsumableStoreFrontItemPlate).BuyButton, 1.3f);
		GUIUtils.DelayButtonFunctionality((RecWeapon2Widget.StoreFrontItem as ConsumableStoreFrontItemPlate).BuyButton, 1.3f);
		GUIUtils.HookButton(NextButton, Close);
		GameAnalytics.LogSurvivalWaveCompleted(Singleton.PlayerData.Level, Singleton.PlayerData.LastMissionStatistics.NumWavesCompleted, Singleton.PlayerData.LastMissionStatistics.SurvivalScoreTotal(), Singleton.PlayerData.LastMissionStatistics.NumToolkitsPurchased);
	}
}
