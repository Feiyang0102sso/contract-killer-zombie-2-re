using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class ProposalAfterDeathPopup : UIWindow
{
	public enum ProposalType
	{
		Unknown,
		SpecialOffer,
		TooDifficult,
		ConsumablesOffer
	}

	private const string MedkitToRecommend = "MedkitLargeX3";

	private const string GrenadeToRecommend = "GrenadeBeaconX3";

	private const int SalesDiscount = 50;

	public UIButton CloseButton;

	public SpriteText DifficultText;

	public SpriteText BuyBetterWeaponText;

	public ShopItemWidget ItemWidget;

	private bool _salesSimulated;

	private ProposalType _popupType;

	public static bool ShouldBeShown()
	{
		return GetRecommendedItem() != null;
	}

	protected override void Awake()
	{
		base.Awake();
		GUIUtils.HookButton(CloseButton, Close);
	}

	protected override void Start()
	{
		base.Start();
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public void Init(string itemName, int discountPercents)
	{
		ShopConfigItem shopConfigItem = Singleton.ShopData.GetShopItem(itemName) as WeaponShopItem;
		if (shopConfigItem != null && shopConfigItem is CommonShopItem)
		{
			SimulateSale(shopConfigItem as CommonShopItem, discountPercents);
		}
		if (shopConfigItem == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		CompleteInit(shopConfigItem);
	}

	public void Init()
	{
		ShopConfigItem recommendedItem = GetRecommendedItem();
		int numMissionsFailedInRow = Singleton.PlayerData.PlayerStatistics.NumMissionsFailedInRow;
		if (recommendedItem != null && recommendedItem is CommonShopItem && numMissionsFailedInRow > 2)
		{
			SimulateSale(recommendedItem as CommonShopItem, 50);
		}
		CompleteInit(recommendedItem);
	}

	private void CompleteInit(ShopConfigItem item)
	{
		ItemWidget.SetItem(item);
		if (item is CommonShopItem && (item as CommonShopItem).IsOnSale())
		{
			DifficultText.Text = Strings.GetString("SPECIAL OFFER FIREPOWER!");
			BuyBetterWeaponText.Text = Strings.GetString("On sale just for you!");
			_popupType = ProposalType.SpecialOffer;
		}
		else if (item is ConsumableShopItem)
		{
			if (item.id == "MedkitLargeX3")
			{
				if (Singleton.PlayerData.PlayerStatistics.LastPlayedMissionWasCompletedWithLowHP)
				{
					DifficultText.Text = Strings.GetString("ALMOST GOT YOU!");
				}
				else
				{
					DifficultText.Text = Strings.GetString("BUY MORE MEDKITS!");
				}
				BuyBetterWeaponText.Text = Strings.GetString("You have run out of medkits. Replenish your supply.");
			}
			else
			{
				DifficultText.Text = Strings.GetString("COME EQUIPPED!");
				BuyBetterWeaponText.Text = Strings.GetString("Get yourself some long life insurance! Buy beacon grenade now!");
			}
			_popupType = ProposalType.ConsumablesOffer;
		}
		else
		{
			DifficultText.Text = Strings.GetString("TOO DIFFICULT?!");
			BuyBetterWeaponText.Text = Strings.GetString("Use better equipment to deal with greater challenge.");
			_popupType = ProposalType.TooDifficult;
		}
	}

	private static ShopConfigItem GetRecommendedItem()
	{
		bool lastPlayedMissionWasCompleted = Singleton.PlayerData.PlayerStatistics.LastPlayedMissionWasCompleted;
		int numMissionsFailedInRow = Singleton.PlayerData.PlayerStatistics.NumMissionsFailedInRow;
		ConsumableShopItem consumableShopItem = Singleton.ShopData.GetShopItem("MedkitLargeX3") as ConsumableShopItem;
		ConsumableShopItem consumableShopItem2 = Singleton.ShopData.GetShopItem("GrenadeBeaconX3") as ConsumableShopItem;
		if (consumableShopItem == null || consumableShopItem2 == null)
		{
			return null;
		}
		if (lastPlayedMissionWasCompleted)
		{
			if (Singleton.PlayerData.Level >= 4 && Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted % 2 == 1)
			{
				if (consumableShopItem.GetCountInPlayersInventory() == 0)
				{
					return consumableShopItem;
				}
				if (consumableShopItem2.GetCountInPlayersInventory() == 0)
				{
					return consumableShopItem2;
				}
			}
		}
		else
		{
			if (numMissionsFailedInRow == 1 && consumableShopItem.GetCountInPlayersInventory() == 0)
			{
				return consumableShopItem;
			}
			if (numMissionsFailedInRow >= 2)
			{
				return GetCheapestAvailableHCWeaponFromTypes(new List<PlayerData.Slots>(new PlayerData.Slots[4]
				{
					PlayerData.Slots.assault,
					PlayerData.Slots.shotgun,
					PlayerData.Slots.handgun,
					PlayerData.Slots.exotic
				}));
			}
		}
		return null;
	}

	private static ShopConfigItem GetCheapestAvailableHCWeaponFromTypes(List<PlayerData.Slots> types)
	{
		types.Shuffle();
		while (types.Count > 0)
		{
			WeaponShopItem cheapestAvailableHCWeaponOfType = GetCheapestAvailableHCWeaponOfType(types[0]);
			if (cheapestAvailableHCWeaponOfType != null)
			{
				return cheapestAvailableHCWeaponOfType;
			}
			types.RemoveAt(0);
		}
		return null;
	}

	private static WeaponShopItem GetCheapestAvailableHCWeaponOfType(PlayerData.Slots slot)
	{
		WeaponShopItem result = null;
		int num = int.MaxValue;
		int num2 = Mathf.RoundToInt((float)Singleton.PlayerData.HardCurrency / 0.5f + 1f);
		foreach (ShopConfigItem item in Singleton.ShopData.shopConfig.items)
		{
			WeaponShopItem weaponShopItem = item as WeaponShopItem;
			if (weaponShopItem != null)
			{
				if (weaponShopItem.slot == slot && weaponShopItem.currency == "hard" && weaponShopItem.IsAvailableForEquipment())
				{
					num2 = Math.Max(weaponShopItem.BasePrice, num2);
				}
			}
		}
		foreach (ShopConfigItem item2 in Singleton.ShopData.shopConfig.items)
		{
			WeaponShopItem weaponShopItem2 = item2 as WeaponShopItem;
			if (weaponShopItem2 != null)
			{
				if (weaponShopItem2.slot == slot && weaponShopItem2.currency == "hard" && !weaponShopItem2.IsAvailableForEquipment() && weaponShopItem2.readyToShow && weaponShopItem2.BasePrice > num2 && weaponShopItem2.BasePrice < num)
				{
					result = weaponShopItem2;
					num = weaponShopItem2.BasePrice;
				}
			}
		}
		return result;
	}

	public void SimulateSale(CommonShopItem item, int discount)
	{
		item.isSale = true;
		item.saleDiscount = discount;
		item.isSaleWithTimer = false;
		item.salePrice = (int)((float)item.BasePrice * 0.01f * (float)(100 - discount));
		_salesSimulated = true;
	}

	protected override void Deactivate()
	{
		if (_salesSimulated && Singleton.Exists())
		{
			Singleton.ShopData.ReloadConfig();
		}
		base.Deactivate();
	}

	private void Update()
	{
		if (!Hidden)
		{
			bool flag = false;
			if (ItemWidget.StoreFrontItem is ConsumableStoreFrontItemPlate && ((ItemWidget.StoreFrontItem as ConsumableStoreFrontItemPlate).ShopItem as ConsumableShopItem).GetCountInPlayersInventory() > 0)
			{
				flag = true;
			}
			else if (ItemWidget.StoreFrontItem is InventoryStoreFrontItemPlate && ((ItemWidget.StoreFrontItem as InventoryStoreFrontItemPlate).ShopItem as InventoryShopItem).IsEquipped())
			{
				flag = true;
			}
			if (flag)
			{
				Close();
			}
		}
	}

	public ProposalType PopupType
	{
		get
		{
			return _popupType;
		}
	}
}
