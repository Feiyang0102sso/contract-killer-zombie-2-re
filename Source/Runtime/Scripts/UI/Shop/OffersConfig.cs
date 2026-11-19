using System;
using System.IO;
using System.Xml.Serialization;

[XmlType("Offers")]
public class OffersConfig
{
	public class LimitedTimeOffer
	{
		[XmlAttribute("version")]
		public int version;

		[XmlAttribute("minXP")]
		public int minXP;

		[XmlAttribute("hours")]
		public int hours;

		[XmlAttribute("mainBundle")]
		public string mainBundle;

		[XmlAttribute("mainBundlePrice")]
		public int mainBundlePrice;

		[XmlAttribute("alternativeBundle")]
		public string alternativeBundle;

		[XmlAttribute("alternativeBundlePrice")]
		public int alternativeBundlePrice;

		private LimitedTimeOffer()
		{
		}

		public bool IsRunning()
		{
			return Singleton.PlayerData.LastStartedLimitedTimeOfferVersion == version && !Singleton.PlayerData.IsLimitedTimeOfferReceived && GetTimeLeft().Ticks > 0;
		}

		public TimeSpan GetTimeLeft()
		{
			return Singleton.PlayerData.LimitedTimeOfferStartTime + new TimeSpan(hours, 0, 0) - DateTime.Now;
		}

		public void UpdateState()
		{
			if (version > Singleton.PlayerData.LastStartedLimitedTimeOfferVersion && Singleton.PlayerData.XP >= minXP)
			{
				Singleton.PlayerData.IsLimitedTimeOfferReceived = false;
				Singleton.PlayerData.LimitedTimeOfferStartTime = DateTime.Now;
				Singleton.PlayerData.LastStartedLimitedTimeOfferVersion = version;
				Singleton.PlayerData.IsLimitedTimeOfferPopupShown = false;
			}
		}

		public void GiveRewardAndFinish()
		{
			if (!(IsRunning()))
			{
				return;
			}
			int price = 0;
			BundleShopItem rewardBundle = GetRewardBundle(out price);
			if (rewardBundle != null)
			{
				rewardBundle.OnPurchase();
			}
			Singleton.PlayerData.IsLimitedTimeOfferReceived = true;
			Singleton.UI.PopupManager.ShowLimitedTimeOfferRewardPopup(rewardBundle);
		}

		public BundleShopItem GetRewardBundle(out int price)
		{
			ShopConfigItem shopItem = Singleton.ShopData.GetShopItem(mainBundle);
			ShopConfigItem shopItem2 = Singleton.ShopData.GetShopItem(alternativeBundle);
			if (shopItem != null && shopItem is BundleShopItem && (shopItem as BundleShopItem).GetItemsWeAlreadyHave().Count == 0)
			{
				price = mainBundlePrice;
				return shopItem as BundleShopItem;
			}
			if (shopItem2 != null && shopItem2 is BundleShopItem)
			{
				price = alternativeBundlePrice;
				return shopItem2 as BundleShopItem;
			}
			price = 0;
			return null;
		}
	}

	public class SpecialHunt
	{
		public enum UpdateStateResult
		{
			HuntContinued,
			HuntStarted,
			HuntStopped
		}

		[XmlAttribute("version")]
		public int version;

		[XmlAttribute("minXP")]
		public int minXP;

		[XmlAttribute("missionIndex")]
		public int missionIndex;

		[XmlAttribute("availabilityInHours")]
		public int availabilityInHours;

		[XmlAttribute("pushNotification")]
		public int pushNotification;

		[XmlAttribute("saleWeapon")]
		public string saleWeapon;

		[XmlAttribute("discountPercents")]
		public int discountPercents;

		private SpecialHunt()
		{
		}

		public UpdateStateResult UpdateState()
		{
			if (version > Singleton.PlayerData.LastStartedSpecialHuntVersion && Singleton.PlayerData.XP >= minXP && !Singleton.PlayerData.HasWeapon(saleWeapon))
			{
				Singleton.PlayerData.IsSpecialHuntReceived = false;
				Singleton.PlayerData.IsSpecialHuntPlayed = false;
				Singleton.PlayerData.SpecialHuntStartTime = DateTime.Now;
				Singleton.PlayerData.LastStartedSpecialHuntVersion = version;
				return UpdateStateResult.HuntStarted;
			}
			if (!Singleton.PlayerData.IsSpecialHuntReceived && (Singleton.PlayerData.SpecialHuntStartTime.AddHours(availabilityInHours) < DateTime.Now || Singleton.PlayerData.HasWeapon(saleWeapon)))
			{
				Singleton.PlayerData.IsSpecialHuntReceived = true;
				Singleton.PlayerData.IsSpecialHuntPlayed = true;
				return UpdateStateResult.HuntStopped;
			}
			return UpdateStateResult.HuntContinued;
		}
	}

	public LimitedTimeOffer limitedTimeOffer;

	public SpecialHunt specialHunt;

	public static OffersConfig Load(MemoryStream stream)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(OffersConfig));
		return (OffersConfig)xmlSerializer.Deserialize(stream);
	}
}
