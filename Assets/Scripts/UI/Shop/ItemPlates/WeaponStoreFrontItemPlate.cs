using Glu.Localization;
using UnityEngine;

public class WeaponStoreFrontItemPlate : InventoryStoreFrontItemPlate
{
	protected WeaponShopItem _weaponShopItem;

	public SpriteText PowerText;

	public SpriteText ModifierText;

	public GameObject ArrowUp;

	public GameObject ArrowDown;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Init(ShopConfigItem item)
	{
		_weaponShopItem = item as WeaponShopItem;
		if (PowerText != null)
		{
			PowerText.Text = string.Concat(Strings.GetString("POWER:"), " ", GUIUtils.ColorYellowTitle, _weaponShopItem.power);
		}
		string[] array = new string[9]
		{
			Strings.GetString("RAPID FIRE"),
			Strings.GetString("BEST RIFLE"),
			Strings.GetString("BEST GUN"),
			Strings.GetString("FULL AUTO"),
			Strings.GetString("STEADY AIM"),
			Strings.GetString("HEADSHOT"),
			Strings.GetString("BIG BOOM"),
			Strings.GetString("HACK N SLASH"),
			Strings.GetString("INSTAKILL")
		};
		if (ModifierText != null)
		{
			string text = _weaponShopItem.Modifier.TrimStart('+');
			string text2 = new string('+', _weaponShopItem.Modifier.Length - text.Length);
			ModifierText.transform.parent.gameObject.SetActiveRecursively(_weaponShopItem.Modifier != string.Empty);
			ModifierText.Text = "[#ffea00]" + text2 + Strings.GetString(text.ToUpper());
		}
		base.Init(item);
	}

	protected override void UpdateState()
	{
		if (_weaponShopItem == null)
		{
			return;
		}
		WeaponShopItem weaponShopItem = Singleton.ShopData.GetCompareItemForSlot(_weaponShopItem.Slot) as WeaponShopItem;
		if (ArrowUp != null && ArrowDown != null)
		{
			ArrowUp.SetActiveRecursively(false);
			ArrowDown.SetActiveRecursively(false);
			if (weaponShopItem != null && weaponShopItem != _weaponShopItem)
			{
				if (weaponShopItem.power > _weaponShopItem.power)
				{
					ArrowDown.SetActiveRecursively(true);
				}
				else
				{
					ArrowUp.SetActiveRecursively(true);
				}
			}
		}
		base.UpdateState();
	}
}
