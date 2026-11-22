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

	/// <summary>New Unity port: XML read/write proxy for basePrice (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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

	/// <summary>New Unity port: XML read/write proxy for hidden (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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
			hidden = NewUnityXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public bool? isSale;

	/// <summary>New Unity port: XML read/write proxy for isSale (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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
			isSale = NewUnityXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public bool? isSaleWithTimer;

	/// <summary>New Unity port: XML read/write proxy for isSaleWithTimer (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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
			isSaleWithTimer = NewUnityXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public int? salePrice;

	/// <summary>New Unity port: XML read/write proxy for salePrice (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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

	/// <summary>New Unity port: XML read/write proxy for saleDiscount (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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

	/// <summary>New Unity port: XML read/write proxy for unlockLevel (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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

	/// <summary>New Unity port: XML read/write proxy for minLevel (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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

	/// <summary>New Unity port: XML read/write proxy for maxLevel (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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

	/// <summary>New Unity port: XML read/write proxy for hiddenForDonators (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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
			hiddenForDonators = NewUnityXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public bool? notifyWhenAvailable;

	/// <summary>New Unity port: XML read/write proxy for notifyWhenAvailable (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
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
			notifyWhenAvailable = NewUnityXml.ParseBool(value);
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
