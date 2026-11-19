using UnityEngine;

public class ShopItemWidget : MonoBehaviour
{
	public StoreFrontItem StoreFrontItem { get; private set; }

	public void SetItem(string shopItemId)
	{
		ShopConfigItem item = Singleton.ShopData.shopConfig.FindItemById(shopItemId);
		SetItem(item);
	}

	public void SetItem(ShopConfigItem item)
	{
		SetItem(item, true);
	}

	public void SetItem(ShopConfigItem item, bool minimizeBundle)
	{
		if (item == null)
		{
			return;
		}
		string path = item.prefab;
		if (item is BundleShopItem && minimizeBundle)
		{
			path = "NewBundleItemPlate";
		}
		Object original = Resources.Load(path);
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		StoreFrontItem = gameObject.GetComponent<StoreFrontItem>();
		StoreFrontItem.Init(item);
		Vector3 vector = GUIUtils.FindSizeOfGameObjectUsingSpriteRoot(gameObject);
		vector.y = -vector.y;
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = -vector / 2f;
	}

	public void SetWeaponInfoItem(string shopItemId, string compareShopItemId)
	{
		WeaponShopItem item = Singleton.ShopData.shopConfig.FindItemById(shopItemId) as WeaponShopItem;
		WeaponShopItem compareItem = Singleton.ShopData.shopConfig.FindItemById(compareShopItemId) as WeaponShopItem;
		SetWeaponInfoItem(item, compareItem);
	}

	public void SetWeaponInfoItem(WeaponShopItem item, WeaponShopItem compareItem)
	{
		Object original = Resources.Load("WeaponInfoPlate");
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		WeaponInfoPlate component = gameObject.GetComponent<WeaponInfoPlate>();
		component.Init(item, compareItem);
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = Vector3.zero;
	}
}
