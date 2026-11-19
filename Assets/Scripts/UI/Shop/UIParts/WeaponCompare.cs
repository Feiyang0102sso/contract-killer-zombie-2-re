using UnityEngine;

public class WeaponCompare : MonoBehaviour
{
	public GameObject Weapon1;

	public GameObject Weapon2;

	public GameObject WeaponCenter;

	public GameObject Arrow;

	private void Awake()
	{
	}

	public void Init(string weaponId)
	{
		WeaponShopItem weaponShopItem = Singleton.ShopData.GetShopItem(weaponId) as WeaponShopItem;
		WeaponShopItem weaponShopItem2 = Singleton.ShopData.GetCompareItemForSlot(weaponShopItem.Slot) as WeaponShopItem;
		WeaponInfoPlate component = Util.CreateGameObject("WeaponInfoPlate").GetComponent<WeaponInfoPlate>();
		if (weaponShopItem2 != null && weaponShopItem != weaponShopItem2)
		{
			component.transform.parent = Weapon1.transform;
			component.transform.localPosition = new Vector3(0f, 0f, 0f);
			component.Init(weaponShopItem2, null);
			WeaponInfoPlate component2 = Util.CreateGameObject("WeaponInfoPlate").GetComponent<WeaponInfoPlate>();
			component2.transform.parent = Weapon2.transform;
			component2.transform.localPosition = new Vector3(0f, 0f, 0f);
			component2.Init(weaponShopItem, weaponShopItem2);
		}
		else
		{
			Arrow.SetActiveRecursively(false);
			component.transform.parent = WeaponCenter.transform;
			component.transform.localPosition = new Vector3(0f, 0f, 0f);
			component.Init(weaponShopItem, null);
		}
	}
}
