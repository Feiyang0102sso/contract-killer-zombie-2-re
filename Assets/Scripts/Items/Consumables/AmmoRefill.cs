using UnityEngine;

public class AmmoRefill : Consumable
{
	public override bool IsReadyToConsume(GameObject consumer)
	{
		return !IsAllWeaponsFull(consumer) && base.IsReadyToConsume(consumer);
	}

	public override void Consume(GameObject consumer)
	{
		base.Consume(consumer);
		Player component = consumer.GetComponent<Player>();
		if ((bool)component)
		{
			foreach (Weapon weapon in component.Inventory.WeaponsBySlot)
			{
				if (weapon != null)
				{
					weapon.AddTotalAmmo(weapon.GetMaxAmmo());
					weapon.AddAmmo(weapon.GetAmmoClipSize());
				}
			}
		}
		GameAnalytics.LogConsumableUsed("Refill");
	}

	public static bool IsAllWeaponsFull(GameObject consumer)
	{
		Player component = consumer.GetComponent<Player>();
		if ((bool)component)
		{
			return IsAllWeaponsFull(component);
		}
		return false;
	}

	public static bool IsAllWeaponsFull(Human human)
	{
		foreach (Weapon weapon in human.Inventory.WeaponsBySlot)
		{
			if (weapon != null && weapon.GetTotalAmmo() < weapon.GetTotalAmmoMax())
			{
				return false;
			}
		}
		return true;
	}
}
