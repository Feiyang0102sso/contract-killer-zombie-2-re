using System;
using Glu.Localization;
using UnityEngine;

public class InfectionLevelPopup : UIWindow
{
	public UIButton CloseButton;

	public SpriteText DescriptionText;

	public GameObject ItemPlacer;

	private ConsumableShopItem _consumableItem;

	private MapNarrative _mapNarrative;

	private MapLocation _mapLocation;

	private bool _logIfCancelled = true;

	protected override void Awake()
	{
		base.Awake();
		GUIUtils.HookButton(CloseButton, () =>
		{
			if (Singleton.GameData.InfectionLevel.CanPlayWithoutBomb)
			{
				Singleton.UI.PopupManager.ShowMissionInfoPopup(_mapNarrative, _mapLocation);
			}
			Close();
		});
	}

	protected override void Start()
	{
		base.Start();
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		_mapNarrative = mapNarrative;
		_mapLocation = mapLocation;
		float num = Singleton.PlayerData.MinutesLeftTillInfectionLevel(Singleton.GameData.InfectionLevel.Max - (float)Singleton.GameData.GetCurrentPlayerLevelData().InfectionLevelPerMission);
		DescriptionText.Text = string.Format(
			Strings.GetString("Wait {0} until the infection will get lower or use zombie bomb to decrease infection immediately!"),
			new DateTime(TimeSpan.FromMinutes(num).Ticks).ToString("mm:ss"));
		_consumableItem = Singleton.ShopData.GetConsumableShopItemByItem("ZombieBomb");
		int consumableCount = Singleton.PlayerData.GetConsumableCount(_consumableItem.item);
		UnityEngine.Object original = Resources.Load(_consumableItem.prefab);
		GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
		ConsumableStoreFrontItemPlate component = gameObject.GetComponent<ConsumableStoreFrontItemPlate>();
		component.Init(_consumableItem);
		Vector3 vector = GUIUtils.FindSizeOfGameObjectUsingSpriteRoot(gameObject);
		vector.y = -vector.y;
		gameObject.transform.parent = ItemPlacer.transform;
		gameObject.transform.localPosition = -vector / 2f;
		component.ShowUseButton = true;
		component.ShowCount = false;
		_consumableItem.OnUse += new Action(OnUseZombieBomb);
	}

	private void OnUseZombieBomb()
	{
		Singleton.PlayerData.InfectionLevel = 0f;
		GameAnalytics.LogInfectionLevelPopup(Singleton.PlayerData.Level, true, Singleton.PlayerData.HardCurrency);
		_logIfCancelled = false;
		Close();
	}

	protected override void OnDestroy()
	{
		if (_consumableItem != null)
		{
			_consumableItem.OnUse -= new Action(OnUseZombieBomb);
		}
		if (_logIfCancelled)
		{
			GameAnalytics.LogInfectionLevelPopup(Singleton.PlayerData.Level, false, Singleton.PlayerData.HardCurrency);
		}
		base.OnDestroy();
	}
}
