using System.Collections.Generic;
using System.Xml.Serialization;
using Glu.Localization;
using UnityEngine;

[XmlType("bundleItem")]
public class BundleShopItem : CommonShopItem
{
	[XmlAttribute("shopItem1")]
	public string shopItem1;

	[XmlAttribute("shopItem2")]
	public string shopItem2;

	[XmlAttribute("shopItem3")]
	public string shopItem3;

	[XmlAttribute("shopItem4")]
	public string shopItem4;

	[XmlAttribute("bonusTextId")]
	public string bonusTextId;

	public override bool readyToShow
	{
		get
		{
			bool flag = true;
			bool flag2 = false;
			foreach (KeyValuePair<CommonShopItem, int> item in ToItemsList())
			{
				InventoryShopItem inventoryShopItem = item.Key as InventoryShopItem;
				if (inventoryShopItem == null)
				{
					continue;
				}
				flag2 = true;
				if (!inventoryShopItem.IsAvailableForEquipment())
				{
					flag = false;
				}
			}
			return base.readyToShow && !(flag2 && flag);
		}
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		BundleShopItem bundleShopItem = other as BundleShopItem;
		CommonShopItem.OverrideValue(ref bundleShopItem.shopItem1, shopItem1);
		CommonShopItem.OverrideValue(ref bundleShopItem.shopItem2, shopItem2);
		CommonShopItem.OverrideValue(ref bundleShopItem.shopItem3, shopItem3);
		CommonShopItem.OverrideValue(ref bundleShopItem.shopItem4, shopItem4);
		CommonShopItem.OverrideValue(ref bundleShopItem.bonusTextId, bonusTextId);
	}

	private void AddItemInDict(Dictionary<CommonShopItem, int> dict, string item)
	{
		if (item == null)
		{
			return;
		}
		string[] array = item.Trim().Split(":");
		CommonShopItem key = Singleton.ShopData.GetShopItem(array[0]) as CommonShopItem;
		dict[key] = array.Length > 1 ? int.Parse(array[1]) : 1;
	}

	public Dictionary<CommonShopItem, int> ToItemsList()
	{
		Dictionary<CommonShopItem, int> dictionary = new Dictionary<CommonShopItem, int>();
		AddItemInDict(dictionary, shopItem1);
		AddItemInDict(dictionary, shopItem2);
		AddItemInDict(dictionary, shopItem3);
		AddItemInDict(dictionary, shopItem4);
		return dictionary;
	}

	public override ShopConfigItem Clone()
	{
		BundleShopItem bundleShopItem = new BundleShopItem();
		Override(bundleShopItem);
		return bundleShopItem;
	}

	public override string GetDescription()
	{
		var result = base.GetDescription();
		return result;
	}

	public override void OnPurchase(bool isPartOfBundle)
	{
		if (!isPartOfBundle)
		{
			GameAnalytics.LogHCBundle(id, Price, currency, PlayerData.Session.Level);
		}
		foreach (KeyValuePair<CommonShopItem, int> item in ToItemsList())
		{
			for (int i = 0; i < item.Value; i++)
			{
				item.Key.OnPurchase(true);
			}
		}
		base.OnPurchase(isPartOfBundle);
	}

	public List<InventoryShopItem> GetItemsWeAlreadyHave()
	{
		List<InventoryShopItem> list = new List<InventoryShopItem>();
		foreach (KeyValuePair<CommonShopItem, int> item in ToItemsList())
		{
			InventoryShopItem inventoryShopItem = item.Key as InventoryShopItem;
			if (inventoryShopItem == null)
			{
				continue;
			}
			if (inventoryShopItem.IsAvailableForEquipment())
			{
				list.Add(inventoryShopItem);
			}
		}
		return list;
	}

	public override void Purchase()
	{
		List<InventoryShopItem> itemsWeAlreadyHave = GetItemsWeAlreadyHave();
		if (itemsWeAlreadyHave.Count > 0)
		{
			string text = Strings.GetString("You already own the following items from this bundle:");
			foreach (InventoryShopItem item in itemsWeAlreadyHave)
			{
				text += "\n" + GUIUtils.ColorYellowTitle + Strings.GetString(item.nameId);
			}
			text += "\n" + Color.white + Strings.GetString("Do you still want to purchase the bundle?");
			PopupInfo popupInfo = new PopupInfo();
			popupInfo.Title = string.Concat(GUIUtils.ColorYellowTitle, Strings.GetString(nameId));
			popupInfo.Message = text;
			popupInfo.AddButton(Strings.GetString("Yes"), DoActualPuchase);
			popupInfo.AddButton(Strings.GetString("No"), null);
			Singleton.UI.PopupManager.ShowPopup(popupInfo, true, false);
		}
		else
		{
			DoActualPuchase();
		}
	}

	public void DoActualPuchase()
	{
		base.Purchase();
		if (StoreScreen.ShopIsOpen)
		{
			Singleton.UI.Store.Refresh();
		}
	}

	private void OnDestroy()
	{
	}

	public override bool IsOnSale()
	{
		return base.IsOnSale();
	}
}
