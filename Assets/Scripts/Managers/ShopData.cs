using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShopData : MonoBehaviour, IShopConfigProvider
{
	private ShopConfig _shopConfig;

	private OffersConfig _offersConfig;

	public ShopConfig shopConfig
	{
		get
		{
			return _shopConfig;
		}
	}

	public OffersConfig offersConfig
	{
		get
		{
			return _offersConfig;
		}
	}

	public OffersConfig.LimitedTimeOffer LimitedTimeOffer
	{
		get
		{
			return _offersConfig.limitedTimeOffer;
		}
	}

	public OffersConfig.SpecialHunt SpecialHunt
	{
		get
		{
			return _offersConfig.specialHunt;
		}
	}

	private void Awake()
	{
		ReloadConfig();
		gameObject.AddComponent<IAPProcessor>();
		GetComponent<IAPProcessor>().Init(this, "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAiDe5FBbiqA55lvMO+FnsGzdtZpLsbhEMg2IiAIow4RUSDBHLnBf6m2BbxnusxZS47dpG0hq3Daqw1WkMxHj6UTjOQc9V+GYwvAX/h55nT4gFpMTzzjx+yECekyXUVZ/dEDl2Nfil3rTRlwiURbpA1CIObiuFQmP2vH8pRkXnvUaj60EGtrqZydGzsuR8QVX8fa7eOzopBNsUqU5kqrAsggi52bws08OBfYZ2ehlIrBxts5UipYQVwFuFKtmc53XI1jb4sQOs5UdT9wvJ87Cg+C8vI85Z1sDjBCn8XSkJsN3Y3TAdv4H2gA92pUza3K+kdIHUa24h+zQxwWe8gdQiNwIDAQAB");
		GetComponent<IAPProcessor>().ResumeProductRetrival();
		CheckData();
	}

	public void ReloadConfig()
	{
		using (MemoryStream stream = new MemoryStream(Util.LoadConfigFile("Assets/Configs/Stores.xml")))
		{
			_shopConfig = ShopConfig.Load(stream);
		}
		byte[] array = Util.LoadConfigFile("Assets/Configs/StoresOverride.xml");
		if (array != null)
		{
			using (MemoryStream stream2 = new MemoryStream(array, false))
			{
				ShopOverride shopOverride = ShopOverride.Load(stream2);
				if (shopOverride.overrides.Count > 0)
				{
					_shopConfig = _shopConfig.Override(shopOverride);
				}
			}
		}
		using (MemoryStream stream3 = new MemoryStream(Util.LoadConfigFile("Assets/Configs/Offers.xml")))
		{
			_offersConfig = OffersConfig.Load(stream3);
		}
	}

	public void ToggleIAPProcessor(bool toggle)
	{
		if (toggle)
		{
			GetComponent<IAPProcessor>().ResumeProductRetrival();
		}
		else
		{
			GetComponent<IAPProcessor>().StopProductRetrival();
		}
	}

	public bool IsShopGroupEmpty(ShopConfigGroup group)
	{
		foreach (ShopConfigGroup.Reference itemRef in group.itemRefs)
		{
			if (itemRef.item.readyToShow)
			{
				return false;
			}
		}
		return true;
	}

	public ShopConfigItem GetShopItem(string shopItemId)
	{
		return _shopConfig.items.Find((ShopConfigItem shopItem) =>
		{
			if (shopItem.id == shopItemId)
			{
				return true;
			}
			return false;
		});
	}

	public IAPShopItem GetIAPShopItemByProductId(string productId)
	{
		return _shopConfig.items.Find((ShopConfigItem shopItem) =>
		{
			if (shopItem is IAPShopItem)
			{
				return (shopItem as IAPShopItem).productId == productId;
			}
			return false;
		}) as IAPShopItem;
	}

	public CommonShopItem GetCommonShopItemByItem(string item)
	{
		return _shopConfig.items.Find((ShopConfigItem shopItem) =>
		{
			if (shopItem is CommonShopItem)
			{
				return (shopItem as CommonShopItem).item == item;
			}
			return false;
		}) as CommonShopItem;
	}

	public WeaponShopItem GetWeaponShopItemByItem(string item)
	{
		CommonShopItem commonShopItemByItem = GetCommonShopItemByItem(item);
		if (commonShopItemByItem != null)
		{
			return commonShopItemByItem as WeaponShopItem;
		}
		return null;
	}

	public ConsumableShopItem GetConsumableShopItemByItem(string item)
	{
		CommonShopItem commonShopItemByItem = GetCommonShopItemByItem(item);
		if (commonShopItemByItem != null)
		{
			return commonShopItemByItem as ConsumableShopItem;
		}
		return null;
	}

	public List<ShopConfigItem> GetItemsUnlockedOnLevel(int level)
	{
		return _shopConfig.items.FindAll((ShopConfigItem shopItem) =>
		{
			if (shopItem is CommonShopItem)
			{
				return (shopItem as CommonShopItem).unlockLevel == level;
			}
			return false;
		});
	}

	public InventoryShopItem GetCompareItemForSlot(PlayerData.Slots slot)
	{
		string equippedItem = Singleton.PlayerData.GetEquippedItem(slot);
		if (equippedItem == null)
		{
			return null;
		}
		return GetCommonShopItemByItem(equippedItem) as InventoryShopItem;
	}

	public ShopConfigGroup FindGroupWithItem(ShopConfigItem item)
	{
		foreach (ShopConfigGroup group in _shopConfig.groups)
		{
			if (group.itemRefs.Find((ShopConfigGroup.Reference obj) => obj.itemId == item.id) != null)
			{
				return group;
			}
		}
		return null;
	}

	public int GetItemsCountOnSaleWithTimer()
	{
		int num = 0;
		foreach (ShopConfigGroup group in _shopConfig.groups)
		{
			foreach (ShopConfigGroup.Reference itemRef in group.itemRefs)
			{
				CommonShopItem commonShopItem = itemRef.item as CommonShopItem;
				if (commonShopItem != null && commonShopItem.readyToShow && commonShopItem.IsOnSale() && commonShopItem.IsSaleWithTimer)
				{
					num++;
				}
			}
		}
		return num;
	}

	private void CheckData()
	{
	}

	public void PreloadItemPrefabs()
	{
		ReloadConfig();
		foreach (ShopConfigItem item in _shopConfig.items)
		{
			GameObject gameObject = Singleton.Cache.PerStringCache.Load(item.id, item.prefab);
			StoreFrontItem component = gameObject.GetComponent<StoreFrontItem>();
			component.Init(item);
			Singleton.Cache.PerStringCache.Release(item.id);
		}
	}

	public CommonShopItem GetNewAvailableItemToNotify()
	{
		int level = Singleton.PlayerData.Level;
		foreach (ShopConfigItem item in _shopConfig.items)
		{
			CommonShopItem commonShopItem = item as CommonShopItem;
			if (commonShopItem != null && commonShopItem.readyToShow && commonShopItem.NotifyWhenAvailable && commonShopItem.MinLevel == level)
			{
				return commonShopItem;
			}
		}
		return null;
	}
}
