using System;
using UnityEngine;

public class HudWeaponSelector : MonoBehaviour
{
	private const float AnyTouchClosingEnabledDelay = 2f;

	public HudAmmoRefill AmmoRefillButton;

	public GameObject Slots;

	private Player _player;

	private HudWeaponSelectorElement[] _elements;

	private bool _isShowing;

	public Action<PlayerData.Slots> OnWeaponSelected = (PlayerData.Slots s) =>
	{
	};

	private float _openTime;

	private bool _allInputDelegateSubscribed;

	private UIAnimation _controlAnimation;

	private bool _weaponWasJustSelected;

	public bool IsShowing()
	{
		return _isShowing;
	}

	private void Awake()
	{
		_elements = GetComponentsInChildren<HudWeaponSelectorElement>(true);
		AmmoRefillButton.OnRefill += new Action(OnAmmoRefill);
	}

	public void Show()
	{
		if (_isShowing)
		{
			return;
		}
		GUIUtils.FinishAllAnimations(gameObject);
		_openTime = TimeControl.GameTime;
		_weaponWasJustSelected = false;
		RefreshWeaponsInfo();
		if ((bool)AmmoRefillButton)
		{
			AmmoRefillButton.Refresh();
		}
		Singleton.TimeControl.ThisIsMassEffect();
		gameObject.SetActiveRecursively(true);
		SubscribeAllInputDelegate();
		BringIn();
		_isShowing = true;
	}

	private void SubscribeAllInputDelegate()
	{
		if (_allInputDelegateSubscribed)
		{
			return;
		}
		UIManager.instance.AddMouseTouchPtrListener(AllInputDelegate);
		_allInputDelegateSubscribed = true;
	}

	private void UnsubscribeAllInputDelegate()
	{
		if (!(_allInputDelegateSubscribed))
		{
			return;
		}
		UIManager.instance.RemoveMouseTouchPtrListener(AllInputDelegate);
		_allInputDelegateSubscribed = false;
	}

	public void Close()
	{
		if (!(_isShowing))
		{
			return;
		}
		GUIUtils.FinishAllAnimations(gameObject);
		UnsubscribeAllInputDelegate();
		Dismiss();
		_isShowing = false;
	}

	public void AllInputDelegate(POINTER_INFO info)
	{
		if (info.evt == POINTER_INFO.INPUT_EVENT.PRESS)
		{
			bool flag = (bool)AmmoRefillButton && AmmoRefillButton.IsAvailable() && info.targetObj == AmmoRefillButton.gameObject.GetComponent<UIButton>();
			if (!_controlAnimation && !flag && TimeControl.GameTime - _openTime > 2f)
			{
				Close();
			}
		}
	}

	public void RegisterPlayer(Player player)
	{
		_player = player;
		RefreshWeapons();
		gameObject.SetActiveRecursively(false);
	}

	public void RefreshWeapons()
	{
		foreach (UIButton uIButton in Slots.GetComponentsInChildren<UIButton>(true))
		{
			PlayerData.Slots slot = (PlayerData.Slots)(int)Enum.Parse(typeof(PlayerData.Slots), uIButton.name, true);
			Weapon weapon = _player.Inventory.WeaponsBySlot[(int)slot];
			uIButton.GetComponent<HudWeaponSelectorElement>().SetWeapon(slot, weapon);
			GUIUtils.HookButton(uIButton, () =>
			{
				if ((bool)weapon && !weapon.IsAmmoEmpty())
				{
					_weaponWasJustSelected = true;
					OnWeaponSelected(slot);
				}
				RefreshWeaponsInfo();
				Close();
			}, POINTER_INFO.INPUT_EVENT.PRESS, false, false);
		}
	}

	public void RefreshWeaponsInfo()
	{
		PlayerData.Slots currentSlot = Scene.Player.Inventory.GetCurrentSlot();
		foreach (HudWeaponSelectorElement hudWeaponSelectorElement in _elements)
		{
			hudWeaponSelectorElement.Refresh(hudWeaponSelectorElement.Slot == currentSlot);
		}
	}

	private void Animate(bool appear)
	{
		float unusedLocal1;
		foreach (HudWeaponSelectorElement hudWeaponSelectorElement in _elements)
		{
			hudWeaponSelectorElement.gameObject.active = true;
			if (appear)
			{
				hudWeaponSelectorElement.transform.localPosition = hudWeaponSelectorElement.OriginalPosition;
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationMove>().Setup(-hudWeaponSelectorElement.transform.localPosition.x, -hudWeaponSelectorElement.transform.localPosition.y, 0f, 0.3f, EZAnimation.EASING_TYPE.ExponentialOut);
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, hudWeaponSelectorElement.GetColor().a, 0f, 0.060000002f, EZAnimation.EASING_TYPE.Linear, false);
			}
			else if (hudWeaponSelectorElement.IsSelected() && _weaponWasJustSelected)
			{
				Transform transform = hudWeaponSelectorElement.transform;
				transform.localPosition = hudWeaponSelectorElement.OriginalPosition;
				Vector3 position = transform.position;
				hudWeaponSelectorElement.transform.position = Singleton.UI.Hud._hudWeapon.transform.position;
				Vector3 vector = position - hudWeaponSelectorElement.transform.position;
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationMove>().Setup(vector.x, vector.y, 0.25f, 0.4f, EZAnimation.EASING_TYPE.ExponentialIn);
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationAlpha>().Setup(hudWeaponSelectorElement.GetColor().a, 0f, 0.45f, 0.2f, EZAnimation.EASING_TYPE.Linear, false);
			}
			else
			{
				hudWeaponSelectorElement.transform.localPosition = new Vector3(0f, 0f, hudWeaponSelectorElement.OriginalPosition.z);
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationMove>().Setup(hudWeaponSelectorElement.OriginalPosition.x, hudWeaponSelectorElement.OriginalPosition.y, 0f, 0.3f, EZAnimation.EASING_TYPE.Linear);
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationAlpha>().Setup(hudWeaponSelectorElement.GetColor().a, 0f, 0.15f, 0.120000005f, EZAnimation.EASING_TYPE.Linear, false);
			}
			hudWeaponSelectorElement.SendMessage("RunAnimation");
		}
		_controlAnimation = gameObject.AddComponent<UIAnimationMove>().Setup(0f, 0.7f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
		if (AmmoRefillButton != null)
		{
			if (appear)
			{
				AmmoRefillButton.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, AmmoRefillButton.GetAlpha(), 0f, 0.15f, EZAnimation.EASING_TYPE.Linear, false);
			}
			else
			{
				AmmoRefillButton.gameObject.AddComponent<UIAnimationAlpha>().Setup(AmmoRefillButton.GetAlpha(), 0f, 0f, 0.27f, EZAnimation.EASING_TYPE.Linear, false);
			}
			AmmoRefillButton.SendMessage("RunAnimation", SendMessageOptions.DontRequireReceiver);
		}
		if (!appear)
		{
			_controlAnimation.OnFinishDelegate = () =>
			{
				if (!_isShowing)
				{
					gameObject.SetActiveRecursively(false);
				}
			};
		}
	}

	private void OnAmmoRefill()
	{
		foreach (HudWeaponSelectorElement hudWeaponSelectorElement in _elements)
		{
			if (hudWeaponSelectorElement.gameObject.active && (bool)hudWeaponSelectorElement.GetWeapon() && !hudWeaponSelectorElement.GetWeapon().IsMelee())
			{
				hudWeaponSelectorElement.gameObject.AddComponent<UIAnimationScale>().Setup(1.5f, 1f, 0f, 0.5f, EZAnimation.EASING_TYPE.BackOut).RunAnimation();
			}
		}
		RefreshWeaponsInfo();
	}

	private void BringIn()
	{
		Animate(true);
	}

	private void Dismiss()
	{
		Animate(false);
	}

	private void OnDestroy()
	{
		if (Singleton.Exists())
		{
			UnsubscribeAllInputDelegate();
		}
	}
}
