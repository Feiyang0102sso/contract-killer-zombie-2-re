using System.Xml;
using System;
using System.Xml.Serialization;
using Glu.Localization;
using UnityEngine;

[XmlType("inventoryItem")]
public class InventoryShopItem : CommonShopItem
{
	[XmlIgnore]
	public PlayerData.Slots? slot;

	/// <summary>New Unity port: XML read/write proxy for slot (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
	[XmlAttribute("slot")]
	public string slot_XmlText
	{
		get
		{
			if (!slot.HasValue)
			{
				return null;
			}
			return slot.Value.ToString();
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				slot = null;
				return;
			}
			slot = (PlayerData.Slots)System.Enum.Parse(typeof(PlayerData.Slots), value);
		}
	}

	[XmlAttribute("lockedTextId")]
	public string lockedTextId;

	public PlayerData.Slots Slot
	{
		get
		{
			PlayerData.Slots? slots = slot;
			return slots.HasValue ? slots.Value : PlayerData.Slots.handgun;
		}
	}

	public override bool readyToShow
	{
		get
		{
			if (IsAvailableForEquipment())
			{
				return true;
			}
			return base.readyToShow;
		}
	}

	public InventoryShopItem()
	{
		if (Application.isPlaying)
		{
			Singleton.PlayerData.OnWeaponEquipped += new PlayerData.OnWeaponEquippedDelegate(OnItemEquipped);
		}
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		InventoryShopItem inventoryShopItem = other as InventoryShopItem;
		CommonShopItem.OverrideValue(ref inventoryShopItem.slot, slot);
		CommonShopItem.OverrideValue(ref inventoryShopItem.lockedTextId, lockedTextId);
	}

	public override ShopConfigItem Clone()
	{
		InventoryShopItem inventoryShopItem = new InventoryShopItem();
		Override(inventoryShopItem);
		return inventoryShopItem;
	}

	public override string GetDescription()
	{
		var result = base.GetDescription();
		return result;
	}

	public override void OnPurchase(bool isPartOfBundle)
	{
		bool flag = Price == 0;
		if (!isPartOfBundle)
		{
			if (currency.Equals("soft"))
			{
				GameAnalytics.LogSCDurable(item, Price, currency, PlayerData.Session.Level);
			}
			else if (currency.Equals("hard"))
			{
				GameAnalytics.LogHCDurable(item, Price, currency, PlayerData.Session.Level);
			}
		}
		if (!Singleton.PlayerData.HasWeapon(item))
		{
			Singleton.PlayerData.AddWeapon(item);
		}
		if (currency.Equals("hard") && !flag && !isPartOfBundle)
		{
			Singleton.PlayerData.AddConsumable("AmmoRefill", 1);
			Singleton.UI.PopupManager.ShowInfoPopup(Strings.GetString("You received a free ammo refill for buying this weapon!"));
		}
		base.OnPurchase(isPartOfBundle);
		Equip();
	}

	public virtual void Equip()
	{
		Singleton.PlayerData.EquipWeapon(Slot, item);
		SoundManager.PlayExclusive("UI/shop_item_equiped", false);
	}

	public virtual bool IsEquipped()
	{
		return Singleton.PlayerData.IsEquipped(Slot, item);
	}

	public virtual bool IsAvailableForEquipment()
	{
		return Singleton.PlayerData.HasWeapon(item);
	}

	public void OnItemEquipped(string item)
	{
		OnStateChange();
	}

	private void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.PlayerData.OnWeaponEquipped -= new PlayerData.OnWeaponEquippedDelegate(OnItemEquipped);
		}
	}

	public override bool IsOnSale()
	{
		return base.IsOnSale() && !IsAvailableForEquipment();
	}
}
