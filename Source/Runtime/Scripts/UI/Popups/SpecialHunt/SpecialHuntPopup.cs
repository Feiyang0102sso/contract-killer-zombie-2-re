using UnityEngine;

public class SpecialHuntPopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton StartButton;

	public ShopItemWidget WeaponWidget;

	public SpriteText RewardCash;

	public SpriteText RewardXP;

	private bool _firstRun = true;

	protected override void Awake()
	{
		GUIUtils.HookButton(CloseButton, Close);
		base.Awake();
	}

	protected override void Start()
	{
		if (Singleton.PlayerData.IsSpecialHuntReceived)
		{
			Object.Destroy(gameObject);
			return;
		}
		int missionIndex = Singleton.ShopData.SpecialHunt.missionIndex;
		if (missionIndex >= Singleton.GameData.Missions.Length)
		{
			Object.Destroy(gameObject);
			return;
		}
		GameData.MissionData missionData = Singleton.GameData.Missions[missionIndex];
		int num = Mission.ExperienceBase(missionData.Type, Mission.Difficulty.Hard, true);
		int price = Mission.SoftCurrencyBase(missionData.Type, Mission.Difficulty.Hard, true);
		RewardCash.Text = GUIUtils.GetPriceStringForCurrency(price, "soft");
		RewardXP.Text = string.Format("XP {0}", num);
		WeaponWidget.SetItem(Singleton.ShopData.SpecialHunt.saleWeapon);
		base.Start();
	}

	public void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		int missionIndex = Singleton.ShopData.SpecialHunt.missionIndex;
		GameData.MissionData missionData = Singleton.GameData.Missions[missionIndex];
		Debug.Log(string.Concat("Special hunt on ", missionData.Type, "/", missionData.Difficulty, " (", missionIndex, ")"));
		GUIUtils.HookButton(StartButton, () =>
		{
			Singleton.PlayerData.IsSpecialHuntPlayed = false;
			PlayerData.Instance.CurrentLocation = mapNarrative.SelectedLocation;
			Singleton.Game.GoToGameLevel(missionData.Scene, missionData.Type);
		});
	}

	protected void Update()
	{
		if (_firstRun)
		{
			GameObject gameObject = WeaponWidget.FindChildInHierarchy("BuyButton");
			gameObject.SetActiveRecursively(false);
			GameObject gameObject2 = WeaponWidget.FindChildInHierarchy("EquippedButton");
			gameObject2.SetActiveRecursively(true);
			GameObject gameObject3 = WeaponWidget.FindChildInHierarchy("PriceLabel");
			gameObject3.SetActiveRecursively(false);
			GameObject gameObject4 = WeaponWidget.FindChildInHierarchy("DiscountPrice");
			gameObject4.SetActiveRecursively(false);
			GameObject gameObject5 = WeaponWidget.FindChildInHierarchy("StatsButton");
			gameObject5.SetActiveRecursively(false);
			_firstRun = false;
		}
	}
}
