using System.Xml;
using System;
using System.Xml.Serialization;

[XmlType("consumableItem")]
public class ConsumableShopItem : CommonShopItem
{
	[XmlIgnore]
	public Action OnUse = () =>
	{
	};

	[XmlIgnore]
	public int? count;

	/// <summary>New Unity port: XML read/write proxy for count (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
	[XmlAttribute("count")]
	public string count_XmlText
	{
		get
		{
			if (!count.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(count.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				count = null;
				return;
			}
			count = XmlConvert.ToInt32(value);
		}
	}

	public int Count
	{
		get
		{
			int? num = count;
			return !(!num.HasValue) ? num.Value : 1;
		}
	}

	public override ShopConfigItem Clone()
	{
		ConsumableShopItem consumableShopItem = new ConsumableShopItem();
		Override(consumableShopItem);
		return consumableShopItem;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ConsumableShopItem consumableShopItem = other as ConsumableShopItem;
		CommonShopItem.OverrideValue(ref consumableShopItem.count, count);
	}

	public override void OnPurchase(bool isPartOfBundle)
	{
		if (!isPartOfBundle)
		{
			if (currency.Equals("soft"))
			{
				GameAnalytics.LogSCConsumable(item, Price, currency, PlayerData.Session.Level);
			}
			else if (currency.Equals("hard"))
			{
				GameAnalytics.LogHCConsumable(item, Price, currency, PlayerData.Session.Level);
			}
		}
		Singleton.PlayerData.AddConsumable(item, Count);
		if (Scene.Exists() && Scene.Player != null)
		{
			Scene.Player.Inventory.AddConsumable(item, Count, true);
		}
		base.OnPurchase(isPartOfBundle);
	}

	public void Use()
	{
		Singleton.PlayerData.ChangeConsumableCount(item, -1);
		OnUse();
	}

	public int GetCountInPlayersInventory()
	{
		return Singleton.PlayerData.GetConsumableCount(item);
	}
}
