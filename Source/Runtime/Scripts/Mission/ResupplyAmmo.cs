using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class ResupplyAmmo : MonoBehaviour
{
	public enum ResupplyWhat
	{
		Current,
		Recommended,
		All,
		Balanced
	}

	public ResupplyWhat resupply = ResupplyWhat.All;

	public Weapon.AmmoType resupplySpecific = (Weapon.AmmoType)(-1);

	public float amount = 0.5f;

	private void OnUse()
	{
		Player player = Scene.Player;
		Scene.Mission.ReportAction(WaitForAction.Type.CollectAmmo, string.Empty);
		if (resupplySpecific >= Weapon.AmmoType.PistolCartridge)
		{
			foreach (GameObject weapon2 in player.Inventory.Weapons)
			{
				if (weapon2.GetComponent<Weapon>().Ammunition == resupplySpecific)
				{
					Resupply(weapon2.GetComponent<Weapon>(), amount);
				}
			}
		}
		else if (resupply == ResupplyWhat.Current || resupply == ResupplyWhat.Recommended)
		{
			Resupply(player.Inventory.Weapon.GetComponent<Weapon>(), amount);
		}
		else if (resupply == ResupplyWhat.All)
		{
			ResupplyAll(player.Inventory, amount);
		}
		else if (resupply == ResupplyWhat.Balanced)
		{
			int num = 0;
			foreach (Weapon weapon in player.Inventory.WeaponsBySlot)
			{
				if ((bool)weapon && !(weapon is MeleeWeapon) && weapon.GetMaxAmmo() > 0)
				{
					num++;
				}
			}
			float unusedLocal1;
			float num2 = 1f;
			if (num > 0)
			{
				num2 = 1f / Mathf.Pow(num, 1.5f);
			}
			Debug.Log(string.Format("Resupply targets: {0} resupply coefficient {1}", num, amount * num2));
			ResupplyAll(player.Inventory, amount * num2);
		}
		SoundManager.Play("resupply", false);
		string text = resupply == ResupplyWhat.All ? Strings.GetString("Ammo resupplied for all weapons!") : Strings.GetString("Ammo resupplied!");
		Singleton.UI.Hud.ShowInfo(text);
		Singleton.GameEvents.OnAmmoResupplied(gameObject);
		Mission.Type missionType = Mission.Type.Story1;
		int num3 = -1;
		if (Singleton.PlayerData.HasMission(Singleton.PlayerData.CurrentLocation))
		{
			num3 = Singleton.PlayerData.GetMission(Singleton.PlayerData.CurrentLocation);
			missionType = Singleton.GameData.Missions[num3].Type;
		}
		GameAnalytics.CacheLogAmmoCrateFound(missionType, num3, Singleton.PlayerData.Level);
		Invoke("Used", 0f);
	}

	private void Used()
	{
		if (!GetComponent<TimerFromTrigger>())
		{
			GetComponent<ActivateItem>().Disappear();
		}
	}

	private void OnActivate()
	{
	}

	public static void Resupply(Weapon weapon, float amountPercent)
	{
		weapon.AddTotalAmmo(Mathf.CeilToInt((float)weapon.GetMaxAmmo() * amountPercent));
	}

	public static void ResupplyClip(Weapon weapon, float amountPercent)
	{
		weapon.AddAmmo(Mathf.CeilToInt((float)weapon.GetAmmoClipSize() * amountPercent));
	}

	public static void ResupplyAll(HumanInventory inventory, float amountPercent)
	{
		HashSet<Weapon.AmmoType> hashSet = new HashSet<Weapon.AmmoType>();
		foreach (GameObject weapon in inventory.Weapons)
		{
			Weapon component = weapon.GetComponent<Weapon>();
			if (!(!hashSet.Contains(component.Ammunition)))
			{
				continue;
			}
			Resupply(component, amountPercent);
			ResupplyClip(component, amountPercent);
			hashSet.Add(component.Ammunition);
		}
	}
}
