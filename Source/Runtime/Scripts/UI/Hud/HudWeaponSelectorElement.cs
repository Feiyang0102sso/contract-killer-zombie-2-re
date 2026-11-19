using UnityEngine;

public class HudWeaponSelectorElement : MonoBehaviour
{
	private Weapon _weapon;

	private PlayerData.Slots _slot;

	private UIButton _button;

	private static Color NoWeaponColor = new Color(0f, 133f / 256f, 93f / 128f, 1f);

	private static Color WithWeaponColor = new Color(7f / 64f, 133f / 256f, 17f / 32f, 1f);

	private static Color WithEmptyWeaponColor = new Color(7f / 64f, 133f / 256f, 17f / 32f, 0.5f);

	private static Color SelectedWeaponColor = new Color(0f, 183f / 256f, 255f / 256f, 1f);

	private bool _isSelected;

	private HudBulletAmmo _hudBulletAmmo;

	public PlayerData.Slots Slot
	{
		get
		{
			return _slot;
		}
	}

	public Vector3 OriginalPosition { get; private set; }

	public bool IsSelected()
	{
		return _isSelected;
	}

	public Weapon GetWeapon()
	{
		return _weapon;
	}

	public Color GetColor()
	{
		if (_weapon == null)
		{
			return NoWeaponColor;
		}
		if (_weapon.IsAmmoEmpty())
		{
			return WithEmptyWeaponColor;
		}
		if (_isSelected)
		{
			return SelectedWeaponColor;
		}
		return WithWeaponColor;
	}

	private void Awake()
	{
		OriginalPosition = transform.localPosition;
		_button = GetComponent<UIButton>();
	}

	public void SetWeapon(PlayerData.Slots slot, Weapon weapon)
	{
		if (_weapon != null)
		{
			return;
		}
		_weapon = weapon;
		_slot = slot;
		float num = 1f;
		Texture2D texture2D = null;
		if (_weapon != null)
		{
			texture2D = _weapon.ShopIconTexture;
			num = 0.6f;
			foreach (SpriteText spriteText in GetComponentsInChildren<SpriteText>(true))
			{
				spriteText.GetComponent<Renderer>().enabled = false;
			}
			if (!_weapon.IsMelee())
			{
				_hudBulletAmmo = Util.CreateGameObject("HudBulletAmmo").GetComponent<HudBulletAmmo>();
				_hudBulletAmmo.Init(_weapon);
				_hudBulletAmmo.transform.parent = transform;
				_hudBulletAmmo.transform.localPosition = new Vector3(-10f, -10f, -2f);
				_hudBulletAmmo.gameObject.SetActiveRecursively(false);
			}
		}
		else
		{
			texture2D = GetEmptyTextureForSlot(slot);
			num = 1.2f;
		}
		UISingleSprite uISingleSprite = GetComponentsInChildren<UISingleSprite>(true)[0];
		uISingleSprite.GetComponent<Renderer>().enabled = true;
		uISingleSprite.SetTexture(texture2D);
		uISingleSprite.SetSize(GUIUtils.Pixels2GUIUnits(texture2D.width) * num, GUIUtils.Pixels2GUIUnits(texture2D.height) * num);
		Refresh(false);
	}

	public void Refresh(bool isSelected)
	{
		_isSelected = isSelected;
		_button.Color = GetColor();
		if (_hudBulletAmmo != null)
		{
			_hudBulletAmmo.Refresh();
		}
	}

	private static Texture2D GetEmptyTextureForSlot(PlayerData.Slots slot)
	{
		string path = "WeaponSlots/" + slot.ToString();
		return Resources.Load(path) as Texture2D;
	}
}
