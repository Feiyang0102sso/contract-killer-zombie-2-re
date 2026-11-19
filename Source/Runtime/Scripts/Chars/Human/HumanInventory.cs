using System;
using System.Collections.Generic;
using UnityEngine;

public class HumanInventory : MonoBehaviourAndLog
{
	[Serializable]
	public class StartWeapon
	{
		public PlayerData.Slots Slot;

		public string Name;
	}

	public delegate void InventoryChangedDelegate();

	public StartWeapon[] startWeapons;

	private Weapon[] _equippables = new Weapon[Enum.GetValues(typeof(PlayerData.Slots)).Length];

	private Dictionary<string, GameObject> _consumables = new Dictionary<string, GameObject>();

	private PlayerData.Slots _currentSlot;

	private Weapon _currentWeapon;

	private MeleeWeapon _currentMeleeWeapon;

	public InventoryChangedDelegate OnInventoryChanged = () =>
	{
	};

	public Weapon[] WeaponsBySlot
	{
		get
		{
			return _equippables;
		}
	}

	public List<GameObject> Weapons
	{
		get
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Weapon weapon in _equippables)
			{
				if ((bool)weapon)
				{
					list.Add(weapon.gameObject);
				}
			}
			return list;
		}
	}

	public Dictionary<string, GameObject> Consumables
	{
		get
		{
			return _consumables;
		}
	}

	public PlayerData.Slots GetCurrentSlot()
	{
		return _currentSlot;
	}

	public Weapon Weapon
	{
		get
		{
			return _currentWeapon;
		}
	}

	public MeleeWeapon MeleeWeapon
	{
		get
		{
			return _currentMeleeWeapon;
		}
	}

	private void Awake()
	{
		LogTo("Char.Human.Inventory");
	}

	private void Start()
	{
	}

	public void SetCurrentWeapon(string itemName)
	{
	}

	public bool SetCurrentSlot(PlayerData.Slots slot)
	{
		if (_equippables[(int)slot] == null)
		{
			return false;
		}
		_currentWeapon = _equippables[(int)slot].GetComponent<Weapon>();
		_currentSlot = slot;
		RefreshInventoryActivity();
		if ((bool)Scene.Mission)
		{
			Scene.Mission.ReportAction(WaitForAction.Type.ChangeWeapon, slot.ToString());
		}
		gameObject.SendMessage("OnSwitchWeapon", Weapon);
		return true;
	}

	public Consumable GetConsumableByItemName(string itemName)
	{
		GameObject value;
		_consumables.TryGetValue(itemName, out value);
		if ((bool)value)
		{
			return value.GetComponent<Consumable>();
		}
		return null;
	}

	public Consumable TryConsume(string itemName)
	{
		Consumable consumableByItemName = GetConsumableByItemName(itemName);
		if (!consumableByItemName)
		{
			return null;
		}
		if (consumableByItemName.IsReadyToConsume(gameObject))
		{
			consumableByItemName.Consume(gameObject);
			if ((bool)Scene.Mission)
			{
				Scene.Mission.ReportAction(WaitForAction.Type.UsePowerUp, itemName);
			}
			if (consumableByItemName.Count <= 0)
			{
				RemoveConsumable(consumableByItemName.name);
			}
			return consumableByItemName;
		}
		return null;
	}

	public void AddEquippable(string itemName, PlayerData.Slots slot)
	{
		GameObject gameObject = LoadItem(itemName, "Assets/Prefabs/Items/Weapons");
		if (gameObject == null)
		{
			return;
		}
		Weapon component = gameObject.GetComponent<Weapon>();
		component.SetOwner(base.gameObject);
		if (component is MeleeWeapon)
		{
			_currentMeleeWeapon = component as MeleeWeapon;
		}
		if (_equippables[(int)slot] != null)
		{
			UnityEngine.Object.Destroy(_equippables[(int)slot].gameObject);
		}
		_equippables[(int)slot] = component;
		OnInventoryChanged();
	}

	public void AddConsumable(string itemName, int count, bool syncWithPlayer)
	{
		AddConsumable(itemName, count, syncWithPlayer, false);
	}

	public void AddConsumable(string itemName, int count, bool syncWithPlayer, bool collectedInGame)
	{
		if (count == 0)
		{
			return;
		}
		if (_consumables.ContainsKey(itemName))
		{
			_consumables[itemName].GetComponent<Consumable>().Stack(count);
			return;
		}
		GameObject gameObject = LoadItem(itemName, "Assets/Prefabs/Items/Consumables");
		if (gameObject == null)
		{
			return;
		}
		Consumable component = gameObject.GetComponent<Consumable>();
		if (!component.IsUsedInGame())
		{
			if (collectedInGame)
			{
				Singleton.PlayerData.AddConsumable(itemName, count);
			}
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		component.SyncWithPlayer = syncWithPlayer;
		component.Count = count;
		if (collectedInGame)
		{
			component.Sync();
		}
		_consumables[itemName] = gameObject;
		OnInventoryChanged();
	}

	private GameObject LoadItem(string item, string path)
	{
		GameObject gameObject = Util.CreateGameObject(item, path);
		if (!gameObject)
		{
			return null;
		}
		gameObject.RemoveClonePostfix();
		gameObject.transform.parent = Util.FindOrCreate(base.gameObject, "Inventory").transform;
		gameObject.transform.position = Vector3.zero;
		gameObject.active = false;
		return gameObject;
	}

	public void RemoveConsumable(string itemName)
	{
		if (_consumables.ContainsKey(itemName))
		{
			UnityEngine.Object.Destroy(_consumables[itemName]);
			_consumables.Remove(itemName);
		}
		OnInventoryChanged();
	}

	public void SetAnyCurrentWeapon()
	{
		foreach (PlayerData.Slots value in Enum.GetValues(typeof(PlayerData.Slots)))
		{
			if (_equippables[(int)value] != null)
			{
				SetCurrentSlot(value);
				return;
			}
		}
	}

	private void OnActivate()
	{
		RefreshInventoryActivity();
	}

	private void RefreshInventoryActivity()
	{
		foreach (Weapon weapon in _equippables)
		{
			if ((bool)weapon)
			{
				weapon.gameObject.active = false;
			}
		}
		foreach (KeyValuePair<string, GameObject> consumable in _consumables)
		{
			consumable.Value.active = false;
		}
		if ((bool)_currentWeapon)
		{
			_currentWeapon.gameObject.transform.position = Vector3.zero;
			_currentWeapon.gameObject.active = true;
		}
	}

	public void SwitchWeapon()
	{
	}

	public static PlayerData.Slots SlotOfWeapon(string weapon)
	{
		WeaponShopItem weaponShopItem = Singleton.ShopData.GetShopItem(weapon) as WeaponShopItem;
		return weaponShopItem.Slot;
	}
}
