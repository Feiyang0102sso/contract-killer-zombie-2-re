using UnityEngine;

public class HudWeapon : MonoBehaviour
{
	private GameObject _icon;

	private Weapon _weapon;

	public void SetWeapon(Weapon weapon)
	{
		if (_weapon == weapon)
		{
			return;
		}
		if ((bool)_icon)
		{
			Object.Destroy(_icon);
		}
		_icon = weapon.CreateHudIcon();
		_icon.transform.parent = transform;
		_icon.transform.localPosition = Vector3.zero;
		GUIUtils.RunAllAnimations(_icon);
		_weapon = weapon;
	}
}
