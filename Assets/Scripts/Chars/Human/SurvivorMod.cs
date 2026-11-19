using System.Collections.Generic;
using UnityEngine;

public class SurvivorMod : MonoBehaviour
{
	public enum Weapon
	{
		None,
		Default,
		BaseballBat1,
		FireAxe,
		Chainsaw,
		SurvivorPistols,
		SurvivorShotgun,
		AssaultRifle3
	}

	private const float crippledAttractiveness = 1f / 3f;

	public bool crippled;

	public Weapon weaponPrimary = Weapon.Default;

	public Weapon weaponSecondary = Weapon.Default;

	private HumanInventory.StartWeapon WeaponNameToStartWeapon(string name)
	{
		WeaponShopItem weaponShopItemByItem = Singleton.ShopData.GetWeaponShopItemByItem(name);
		PlayerData.Slots slot;
		if (weaponShopItemByItem == null)
		{
			if (name == "SurvivorShotgun")
			{
				slot = PlayerData.Slots.shotgun;
			}
			else if (name == "SurvivorPistols")
			{
				slot = PlayerData.Slots.handgun;
			}
			else
			{
				slot = PlayerData.Slots.handgun;
			}
		}
		else
		{
			slot = weaponShopItemByItem.Slot;
		}
		HumanInventory.StartWeapon startWeapon = new HumanInventory.StartWeapon();
		startWeapon.Name = name;
		startWeapon.Slot = slot;
		return startWeapon;
	}

	private void OnUnstub(GameObject obj)
	{
		if (crippled)
		{
			obj.GetComponent<Human>().baseSpeed = 2.5f;
			obj.GetComponent<Human>().attractiveness = 1f / 3f;
		}
		if (weaponPrimary == Weapon.Default)
		{
			weaponPrimary = Weapon.SurvivorShotgun;
		}
		if (weaponSecondary == Weapon.Default)
		{
			weaponSecondary = Weapon.BaseballBat1;
		}
		List<HumanInventory.StartWeapon> list = new List<HumanInventory.StartWeapon>();
		if (weaponPrimary != Weapon.None)
		{
			list.Add(WeaponNameToStartWeapon(weaponPrimary.ToString()));
		}
		if (weaponSecondary != Weapon.None)
		{
			list.Add(WeaponNameToStartWeapon(weaponSecondary.ToString()));
		}
		HumanInventory component = obj.GetComponent<HumanInventory>();
		component.startWeapons = list.ToArray();
	}
}
