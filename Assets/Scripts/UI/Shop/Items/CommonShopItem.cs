using System;
using System.Xml.Serialization;
using UnityEngine;

[XmlType("commonShopItem")]
public class CommonShopItem : ShopConfigItem
{
	[XmlAttribute("item")]
	public string item;

	[XmlAttribute("price")]
	public int? basePrice;

	[XmlAttribute("currency")]
	public string currency;

	[XmlAttribute("hidden")]
	public bool? hidden;

	[XmlAttribute("isSale")]
	public bool? isSale;

	[XmlAttribute("isSaleWithTimer")]
	public bool? isSaleWithTimer;

	[XmlAttribute("salePrice")]
	public int? salePrice;

	[XmlAttribute("saleDiscount")]
	public int? saleDiscount;

	[XmlAttribute("unlockLevel")]
	public int? unlockLevel;

	[XmlAttribute("minLevel")]
	public int? minLevel;

	[XmlAttribute("maxLevel")]
	public int? maxLevel;

	[XmlAttribute("hiddenForDonators")]
	public bool? hiddenForDonators;

	[XmlAttribute("notifyWhenAvailable")]
	public bool? notifyWhenAvailable;

	[XmlAttribute("color")]
	public string color;

	[XmlAttribute("modifier")]
	public string modifier;

	[XmlAttribute("image")]
	public string image;

	[XmlIgnore]
	public Action OnStateChange = () =>
	{
	};

	[XmlIgnore]
	public Action OnActualPurchase = () =>
	{
	};

	public int BasePrice
	{
		get
		{
			int? num = basePrice;
			return num.HasValue ? num.Value : 0;
		}
	}

	public bool Hidden
	{
		get
		{
			bool? flag = hidden;
			return flag.HasValue ? flag.Value : false;
		}
	}

	public bool IsSale
	{
		get
		{
			bool? flag = isSale;
			return flag.HasValue ? flag.Value : false;
		}
	}

	public bool IsSaleWithTimer
	{
		get
		{
			bool? flag = isSaleWithTimer;
			return flag.HasValue ? flag.Value : false;
		}
	}

	public int SalePrice
	{
		get
		{
			int? num = salePrice;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int? SaleDiscount
	{
		get
		{
			int? num = saleDiscount;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int? UnlockLevel
	{
		get
		{
			int? num = unlockLevel;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int? MinLevel
	{
		get
		{
			int? num = minLevel;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int? MaxLevel
	{
		get
		{
			int? num = maxLevel;
			return !(!num.HasValue) ? num.Value : int.MaxValue;
		}
	}

	public bool HiddenForDonators
	{
		get
		{
			bool? flag = hiddenForDonators;
			return flag.HasValue ? flag.Value : false;
		}
	}

	public bool NotifyWhenAvailable
	{
		get
		{
			bool? flag = notifyWhenAvailable;
			return flag.HasValue ? flag.Value : false;
		}
	}

	public string Color_
	{
		get
		{
			return color ?? string.Empty;
		}
	}

	public string Modifier
	{
		get
		{
			return modifier ?? string.Empty;
		}
	}

	public int Price
	{
		get
		{
			return !(!IsOnSale()) ? SalePrice : BasePrice;
		}
	}

	public override bool readyToShow
	{
		get
		{
			if (Hidden && !IsOnSale())
			{
				return false;
			}
			if (HiddenForDonators && Singleton.PlayerData.IsDonator())
			{
				return false;
			}
			return Singleton.PlayerData.Level >= MinLevel && Singleton.PlayerData.Level <= MaxLevel;
		}
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		CommonShopItem commonShopItem = other as CommonShopItem;
		OverrideValue(ref commonShopItem.item, item);
		OverrideValue(ref commonShopItem.basePrice, basePrice);
		OverrideValue(ref commonShopItem.currency, currency);
		OverrideValue(ref commonShopItem.isSale, isSale);
		OverrideValue(ref commonShopItem.isSaleWithTimer, isSaleWithTimer);
		OverrideValue(ref commonShopItem.hidden, hidden);
		OverrideValue(ref commonShopItem.salePrice, salePrice);
		OverrideValue(ref commonShopItem.saleDiscount, saleDiscount);
		OverrideValue(ref commonShopItem.unlockLevel, unlockLevel);
		OverrideValue(ref commonShopItem.minLevel, minLevel);
		OverrideValue(ref commonShopItem.maxLevel, maxLevel);
		OverrideValue(ref commonShopItem.notifyWhenAvailable, notifyWhenAvailable);
		OverrideValue(ref commonShopItem.hiddenForDonators, hiddenForDonators);
		OverrideValue(ref commonShopItem.color, color);
		OverrideValue(ref commonShopItem.image, image);
	}

	public override ShopConfigItem Clone()
	{
		CommonShopItem commonShopItem = new CommonShopItem();
		Override(commonShopItem);
		return commonShopItem;
	}

	public override string GetDescription()
	{
		string description = base.GetDescription();
		description = description + " price = " + basePrice;
		description = description + " currency = " + currency;
		description = description + " isSale = " + isSale;
		description = description + " isSaleWithTimer = " + isSaleWithTimer;
		description = description + " saleDiscount = " + saleDiscount;
		description = description + " unlockLevel = " + unlockLevel;
		description = description + " image = " + image;
		description = description + " item = " + item;
		return description;
	}

	public void OnPurchase()
	{
		OnPurchase(false);
	}

	public virtual void OnPurchase(bool isPartOfBundle)
	{
		if (item != null && currency != null)
		{
			if (!isPartOfBundle)
			{
				GameAnalytics.LogItemBuyClicked(item, BasePrice, currency);
			}
		}
		else if (!(this is BundleShopItem))
		{
			Debug.Log("OnPurchase() : invalid argument value of " + (!(item != null) ? "item" : "currency"));
		}
		Singleton.GameEvents.OnItemPurchase(item);
		OnStateChange();
		OnActualPurchase();
	}

	public virtual void Purchase()
	{
		bool flag = false;
		if (Singleton.PlayerData.IsEnoughMoney(currency, Price))
		{
			flag = Singleton.PlayerData.ChangeMoney(currency, -Price);
			SoundManager.PlayExclusive("UI/shop_item_bought", false);
		}
		else
		{
			SoundManager.PlayExclusive("UI/not_enough_money", false);
		}
		if (!flag)
		{
			Singleton.UI.ShowNotEnoughMoneyWindow(this);
		}
		if (flag)
		{
			OnPurchase();
		}
	}

	public bool IsEnoughMoney()
	{
		return Singleton.PlayerData.IsEnoughMoney(currency, Price);
	}

	public long GetSaleTimeLeft()
	{
		return overrideTimeLeft.Ticks;
	}

	public virtual bool IsOnSale()
	{
		if (IsSale)
		{
			if (!IsSaleWithTimer)
			{
				return true;
			}
			if (GetSaleTimeLeft() > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void OverrideValue<T>(ref T? otherValue, T? thisValue) where T : struct
	{
		if (thisValue.HasValue)
		{
			otherValue = thisValue;
		}
	}

	public static void OverrideValue<T>(ref T otherValue, T thisValue) where T : class
	{
		if (thisValue != null)
		{
			otherValue = thisValue;
		}
	}
}
