using System.Xml;
using System;
using System.Xml.Serialization;
using UnityEngine;

[XmlType("commonShopItem")]
public class CommonShopItem : ShopConfigItem
{
	[XmlAttribute("item")]
	public string item;

	[XmlIgnore]
	public int? basePrice;

	/// <summary>PC 移植：basePrice 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("price")]
	public string basePrice_XmlText
	{
		get
		{
			if (!basePrice.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(basePrice.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				basePrice = null;
				return;
			}
			basePrice = XmlConvert.ToInt32(value);
		}
	}

	[XmlAttribute("currency")]
	public string currency;

	[XmlIgnore]
	public bool? hidden;

	/// <summary>PC 移植：hidden 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("hidden")]
	public string hidden_XmlText
	{
		get
		{
			if (!hidden.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(hidden.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				hidden = null;
				return;
			}
			hidden = PcXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public bool? isSale;

	/// <summary>PC 移植：isSale 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("isSale")]
	public string isSale_XmlText
	{
		get
		{
			if (!isSale.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(isSale.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				isSale = null;
				return;
			}
			isSale = PcXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public bool? isSaleWithTimer;

	/// <summary>PC 移植：isSaleWithTimer 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("isSaleWithTimer")]
	public string isSaleWithTimer_XmlText
	{
		get
		{
			if (!isSaleWithTimer.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(isSaleWithTimer.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				isSaleWithTimer = null;
				return;
			}
			isSaleWithTimer = PcXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public int? salePrice;

	/// <summary>PC 移植：salePrice 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("salePrice")]
	public string salePrice_XmlText
	{
		get
		{
			if (!salePrice.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(salePrice.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				salePrice = null;
				return;
			}
			salePrice = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? saleDiscount;

	/// <summary>PC 移植：saleDiscount 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("saleDiscount")]
	public string saleDiscount_XmlText
	{
		get
		{
			if (!saleDiscount.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(saleDiscount.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				saleDiscount = null;
				return;
			}
			saleDiscount = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? unlockLevel;

	/// <summary>PC 移植：unlockLevel 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("unlockLevel")]
	public string unlockLevel_XmlText
	{
		get
		{
			if (!unlockLevel.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(unlockLevel.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				unlockLevel = null;
				return;
			}
			unlockLevel = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? minLevel;

	/// <summary>PC 移植：minLevel 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("minLevel")]
	public string minLevel_XmlText
	{
		get
		{
			if (!minLevel.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(minLevel.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				minLevel = null;
				return;
			}
			minLevel = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? maxLevel;

	/// <summary>PC 移植：maxLevel 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("maxLevel")]
	public string maxLevel_XmlText
	{
		get
		{
			if (!maxLevel.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(maxLevel.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				maxLevel = null;
				return;
			}
			maxLevel = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public bool? hiddenForDonators;

	/// <summary>PC 移植：hiddenForDonators 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("hiddenForDonators")]
	public string hiddenForDonators_XmlText
	{
		get
		{
			if (!hiddenForDonators.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(hiddenForDonators.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				hiddenForDonators = null;
				return;
			}
			hiddenForDonators = PcXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public bool? notifyWhenAvailable;

	/// <summary>PC 移植：notifyWhenAvailable 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("notifyWhenAvailable")]
	public string notifyWhenAvailable_XmlText
	{
		get
		{
			if (!notifyWhenAvailable.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(notifyWhenAvailable.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				notifyWhenAvailable = null;
				return;
			}
			notifyWhenAvailable = PcXml.ParseBool(value);
		}
	}

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
