// PC port test maps: gives the player a test loadout when a test map starts.
// Lives as an object in the test map scene (Assets/Scenes/_TestMaps), so the lists are edited in the Inspector.
// Grants go to this run's inventory only. Consumables are detached from the save first, so using or stacking
// them in a test map never changes the counts stored in PlayerData.
using System;
using System.Collections;
using UnityEngine;

public class PcTestLoadout : MonoBehaviour
{
	[Serializable]
	public class ConsumableGrant
	{
		/// <summary>Consumable prefab name (Assets/Prefabs/Items/Consumables), e.g. MedkitLarge.</summary>
		public string Name;
		public int Count;
	}

	/// <summary>Weapon prefab names (Assets/Prefabs/Items/Weapons); the slot comes from the shop data.</summary>
	public string[] Weapons = new string[] { "Katana2", "Pistols6", "AssaultRifle6", "Shotgun5", "SniperRifle6", "RocketLauncher" };

	public ConsumableGrant[] Consumables = new ConsumableGrant[]
	{
		new ConsumableGrant { Name = "MedkitLarge", Count = 5 },
		new ConsumableGrant { Name = "GrenadeExplosive", Count = 5 },
		new ConsumableGrant { Name = "GrenadeBeacon", Count = 3 },
		new ConsumableGrant { Name = "DamageBoost", Count = 3 },
	};

	private IEnumerator Start()
	{
		// Player.Start builds the inventory from PlayerData; wait for it, then one more frame for the consumables
		while (Scene.Player == null || Scene.Player.Inventory == null || Scene.Player.Inventory.Weapons.Count == 0)
		{
			yield return null;
		}
		yield return null;

		HumanInventory inventory = Scene.Player.Inventory;
		foreach (GameObject item in inventory.Consumables.Values)
		{
			item.GetComponent<Consumable>().SyncWithPlayer = false;
		}

		foreach (string weapon in Weapons)
		{
			PlayerData.Slots slot = HumanInventory.SlotOfWeapon(weapon);
			inventory.AddEquippable(weapon, slot);
			Debug.Log("[PcTestLoadout] weapon " + weapon + " -> " + slot);
		}
		// Re-select the current slot: its old weapon may have been replaced
		if (!inventory.SetCurrentSlot(inventory.GetCurrentSlot()))
		{
			inventory.SetAnyCurrentWeapon();
		}

		foreach (ConsumableGrant grant in Consumables)
		{
			inventory.AddConsumable(grant.Name, grant.Count, false);
			Debug.Log("[PcTestLoadout] consumable " + grant.Name + " +" + grant.Count);
		}
	}
}
