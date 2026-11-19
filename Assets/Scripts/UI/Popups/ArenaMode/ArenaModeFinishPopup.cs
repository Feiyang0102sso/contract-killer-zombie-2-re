using Glu.Localization;
using UnityEngine;

public class ArenaModeFinishPopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton FacebookButton;

	public UIExpandSprite Back;

	public SpriteText LeadName;

	public SpriteText LeadScore;

	public SpriteText RewardXp;

	public SpriteText RewardSoft;

	public SpriteText RewardHard;

	private ConsumableShopItem _consumableItem;

	public ArenaGoalsWidget ArenaGoalsWidget;

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

	public void Init(ArenaMissionReward reward)
	{
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			ImageLeft.SetTexture(Resources.Load("LoadingScreens/loading_back_1") as Texture2D);
			ImageRight.SetTexture(Resources.Load("LoadingScreens/loading_back_r") as Texture2D);
		}
		if (reward.experience != 0)
		{
			RewardXp.Text = string.Concat(string.Empty, Color.white, reward.experience);
		}
		else
		{
			RewardXp.gameObject.SetActiveRecursively(false);
		}
		RewardSoft.Text = string.Concat(string.Empty, Color.white, reward.softCurrency);
		if (reward.hardCurrency != 0)
		{
			RewardHard.Text = string.Concat(string.Empty, Color.white, reward.hardCurrency);
		}
		else
		{
			RewardHard.gameObject.SetActiveRecursively(false);
		}
		_consumableItem = Singleton.ShopData.GetConsumableShopItemByItem("Defibrillator");
		_initialCountOfConsumableItems = Singleton.PlayerData.GetConsumableCount(_consumableItem.item);
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
				bool posted = FacebookManager.PostSMResultMessageToWall(statistics.ArenaScoreTotal(), Strings.GetString("Arena Mode"));
				if (posted)
				{
					Object.Destroy(FacebookButton.gameObject);
				}
			});
		}
		else
		{
			Object.Destroy(FacebookButton.gameObject);
		}
		ArenaGoalsWidget.Init();
		LeaderboardsWidget.Populate(Leaderboards.LeaderboardType.Survival2);
	}
}
