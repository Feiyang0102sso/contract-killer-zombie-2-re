using System.Xml;
using System.Xml.Serialization;
using PlayHaven;
using UnityEngine;

[XmlType("iapShopItem")]
public class IAPShopItem : IAPShopConfigItem
{
	private const bool IsIAPTrackingEnabled = true;

	[XmlIgnore]
	public int? amount;

	/// <summary>PC 移植：amount 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("amount")]
	public string amount_XmlText
	{
		get
		{
			if (!amount.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(amount.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				amount = null;
				return;
			}
			amount = XmlConvert.ToInt32(value);
		}
	}

	[XmlAttribute("currency")]
	public string currency;

	[XmlAttribute("image")]
	public string image;

	[XmlAttribute("bonusTextId")]
	public string bonusTextId;

	public bool Delivered;

	[XmlIgnore]
	private bool _isVGPBuy;

	public int Amount
	{
		get
		{
			int? num = amount;
			return num.HasValue ? num.Value : 0;
		}
	}

	public decimal DefaultPrice
	{
		get
		{
			decimal? num = defaultPrice;
			return !(!num.HasValue) ? num.Value : 0m;
		}
	}

	public override bool readyToShow
	{
		get
		{
			return true;
		}
	}

	public IAPShopItem()
	{
		amount = -1;
		currency = string.Empty;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		IAPShopItem iAPShopItem = other as IAPShopItem;
		CommonShopItem.OverrideValue(ref iAPShopItem.amount, amount);
		CommonShopItem.OverrideValue(ref iAPShopItem.currency, currency);
		CommonShopItem.OverrideValue(ref iAPShopItem.image, image);
		CommonShopItem.OverrideValue(ref iAPShopItem.bonusTextId, bonusTextId);
	}

	public override ShopConfigItem Clone()
	{
		IAPShopItem iAPShopItem = new IAPShopItem();
		Override(iAPShopItem);
		return iAPShopItem;
	}

	public void Buy(bool isVGPBuy)
	{
		_isVGPBuy = isVGPBuy;
		GameAnalytics.LogIAPBuyClicked(productId, Amount, currency, PlayerData.Session.Level, DefaultPrice);
		Delivered = false;
		Singleton.UI.ShowIAPBuyWindow(this);
		base.Buy();
	}

	public override void Buy()
	{
		Buy(false);
	}

	public override void Deliver()
	{
		Delivered = true;
		if (Integrity.IsJailbroken())
		{
			GameAnalytics.LogJBIAPCompleted(productId, Amount, currency, PlayerData.Session.Level, DefaultPrice, Singleton.ShopData.LimitedTimeOffer.IsRunning());
		}
		else
		{
			GameAnalytics.LogIAPCompleted(productId, Amount, currency, PlayerData.Session.Level, DefaultPrice, Singleton.ShopData.LimitedTimeOffer.IsRunning());
		}
		Singleton.GameEvents.OnIAPPurchase(currency.Equals("hard"), Amount, GetPrice());
		if (!Integrity.IsJailbroken())
		{
			if (QualitySetter.PlayHavenEnabled)
			{
				PlayHavenInterface.CancelCurrentContentRequest();
				if (_isVGPBuy)
				{
					PlayHavenInterface.SendVgpResolutionRequest(PurchaseResolution.Buy);
				}
				PlayHavenInterface.SendIapTrackingRequest(productId, PurchaseResolution.Buy);
			}
		}
		else
		{
			Debug.Log("Revenue tracking cancelled (jailbroken device)");
		}
		if (currency.Equals("soft"))
		{
			GameAnalytics.SetNextSCSourceType(GameAnalytics.SCSourceType.IAP);
			Singleton.PlayerData.SoftCurrency += Amount;
		}
		else if (currency.Equals("hard"))
		{
			GameAnalytics.LogHCReceived(Amount, GameAnalytics.HCSourceType.IAP);
			Singleton.PlayerData.HardCurrency += Amount;
		}
		else
		{
		}
		if (Singleton.ShopData.LimitedTimeOffer.IsRunning())
		{
			Singleton.ShopData.LimitedTimeOffer.GiveRewardAndFinish();
		}
		Singleton.PlayerData.SaveOnce();
		Singleton.UI.PopupManager.ShowIAPBoughtPopup(this);
	}

	public void OnPurchaseCanceled()
	{
		if (!Integrity.IsJailbroken())
		{
			if (QualitySetter.PlayHavenEnabled)
			{
				PlayHavenInterface.CancelCurrentContentRequest();
			}
		}
		else
		{
			Debug.Log("Revenue tracking cancelled (jailbroken device)");
		}
		if (!Integrity.IsJailbroken())
		{
			if (QualitySetter.PlayHavenEnabled && !Loader.IsGameScene())
			{
				PlayHavenInterface.StartPublisherContentRequest("iap_cancelled_in_store", false, false);
			}
		}
		else
		{
			Debug.Log("Revenue tracking cancelled (jailbroken device)");
		}
		if (!Integrity.IsJailbroken())
		{
			if (QualitySetter.PlayHavenEnabled)
			{
				if (_isVGPBuy)
				{
					PlayHavenInterface.SendVgpResolutionRequest(PurchaseResolution.Cancel);
				}
				PlayHavenInterface.SendIapTrackingRequest(productId, PurchaseResolution.Cancel);
			}
		}
		else
		{
			Debug.Log("Revenue tracking cancelled (jailbroken device)");
		}
		GameAnalytics.LogIAPPurchaseCancelled(productId, Amount, currency, Singleton.PlayerData.Level, DefaultPrice);
	}

	public void OnPurchaseFailed(GameAnalytics.IAPFailed reason)
	{
		if (!Integrity.IsJailbroken())
		{
			if (QualitySetter.PlayHavenEnabled)
			{
				PlayHavenInterface.CancelCurrentContentRequest();
				if (_isVGPBuy)
				{
					PlayHavenInterface.SendVgpResolutionRequest(PurchaseResolution.Error);
				}
				PlayHavenInterface.SendIapTrackingRequest(productId, PurchaseResolution.Error);
			}
		}
		else
		{
			Debug.Log("Revenue tracking cancelled (jailbroken device)");
		}
		GameAnalytics.LogIAPFailed(productId, Amount, currency, Singleton.PlayerData.Level, DefaultPrice, reason);
	}
}
