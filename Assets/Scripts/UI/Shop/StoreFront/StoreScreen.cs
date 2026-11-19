using System;
using System.Collections;
using UnityEngine;

public class StoreScreen : UIWindow
{
	private const float BackBlockedTime = 0.5f;

	public static bool ShopIsOpen = false;

	private string _currentGroup;

	private float _startTime;

	public DVStoreFrontItemContainer _itemContainer;

	private WeaponCompare _weaponCompare;

	private StoreFrontView _storeFrontView;

	private bool _compareMode;

	private bool _justStarted = true;

	private bool _inited;

	private bool _autoOpenGroups;

	private CommonGUI _commonGUI;

	private CommonGUI commonGUI
	{
		get
		{
			if (_commonGUI == null)
			{
				_commonGUI = transform.GetComponentInChildren<CommonGUI>();
			}
			return _commonGUI;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Singleton.UI.Store = this;
		_storeFrontView = GetComponentInChildren<StoreFrontView>();
		Singleton.ShopData.ReloadConfig();
		OnDestroyDelegate += (Action)(() =>
		{
			if (Singleton.Exists())
			{
				_itemContainer.Clear();
			}
			App.NoNetworkWarningWasShown = false;
		});
		ShopIsOpen = true;
	}

	protected override void Start()
	{
		InitStore();
		gameObject.FindChildInHierarchy("internal_UIScrollList").layer = Const.LayerGui;
		GUIUtils.OverHookButton(commonGUI.BackButton, OnBackButton);
		GUIUtils.OverHookButton(commonGUI.LeftButton, () =>
		{
			SwitchWeaponCompareMode(false);
		});
		if (_weaponCompare == null)
		{
			SelectCurrentGroup();
		}
		commonGUI.ShowGoals(false);
		commonGUI.ShowBottomControls(true);
		_startTime = Time.realtimeSinceStartup;
		if (Singleton.PlayerData.XP >= 200 && _currentGroup != "currency" && QualitySetter.PlayHavenEnabled)
		{
			PlayHavenInterface.StartPublisherContentRequestWithPreload("store_launch", false, false);
		}
		base.Start();
		if (!_compareMode && _autoOpenGroups)
		{
			StartCoroutine(OpenGroupsAnim());
		}
	}

	protected override void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.UI.Store = null;
		}
		base.OnDestroy();
		ShopIsOpen = false;
	}

	private void OnBackButton()
	{
		if (!(_started && !(Time.realtimeSinceStartup - _startTime < 0.5f)))
		{
			return;
		}
		Close();
	}

	public void InitStore()
	{
		_inited = true;
		_storeFrontView.Init(Singleton.ShopData.shopConfig);
		if (_currentGroup == null)
		{
			SelectDefaultGroup();
			_autoOpenGroups = true;
		}
		else
		{
			SelectGroup(_currentGroup);
		}
	}

	private void SelectDefaultGroup()
	{
		for (int i = 0; i < Singleton.ShopData.shopConfig.groups.Count; i++)
		{
			ShopConfigGroup shopConfigGroup = Singleton.ShopData.shopConfig.groups[i];
			if (!Singleton.ShopData.IsShopGroupEmpty(shopConfigGroup))
			{
				SelectGroup(shopConfigGroup.id);
				break;
			}
		}
	}

	public void SelectGroup(string groupName)
	{
		_currentGroup = groupName;
		SelectCurrentGroup();
	}

	public void Refresh()
	{
		ShopConfigGroup selectedGroup = _storeFrontView.groupContainer.GetSelectedGroup();
		if (selectedGroup != null)
		{
			SelectGroup(selectedGroup.id);
		}
		else
		{
			SelectCurrentGroup();
		}
	}

	private void SelectCurrentGroup()
	{
		if (!(_inited))
		{
			return;
		}
		SwitchWeaponCompareMode(false);
		ShopConfigGroup shopConfigGroup = Singleton.ShopData.shopConfig.FindGroup(_currentGroup);
		if (shopConfigGroup != null && !Singleton.ShopData.IsShopGroupEmpty(shopConfigGroup))
		{
			_storeFrontView.groupContainer.SelectGroup(shopConfigGroup);
		}
		else
		{
			SelectDefaultGroup();
		}
	}

	public void SwitchWeaponCompareMode(bool on)
	{
		if (!on && !_started)
		{
			return;
		}
		if (on == _compareMode && !_justStarted)
		{
			return;
		}
		_justStarted = false;
		_compareMode = on;
		float num = 1.2f * GUIUtils.ScreenWidth();
		commonGUI.LeftButton.gameObject.SetActiveRecursively(on);
		if (on)
		{
			commonGUI.LeftButton.gameObject.AddComponent<UIAnimationAlpha>().Setup(on ? 0f : 1f, on ? 1f : 0f, 0f, 0.7f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
		}
		commonGUI.LeftButton.controlIsEnabled = on;
		if ((bool)_weaponCompare)
		{
			_weaponCompare.transform.localPosition = new Vector3(on ? 0f : num, 0f, -2f);
			UIAnimation uIAnimation = _weaponCompare.gameObject.AddComponent<UIAnimationMove>().Setup(num * (on ? 1f : -1f), 0f, 0f, 0.7f, EZAnimation.EASING_TYPE.ExponentialOut);
			_weaponCompare.gameObject.AddComponent<UIAnimationAlpha>().Setup(on ? 0f : 1f, on ? 1f : 0f, 0f, 0.7f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
			uIAnimation.DestroyObjectOnFinish = !on;
			uIAnimation.RunAnimation();
		}
		_itemContainer.transform.localPosition = new Vector3(on ? -num : 0f, 0f, -2f);
		_itemContainer.gameObject.AddComponent<UIAnimationMove>().Setup(num * (on ? 1f : -1f), 0f, 0f, 0.7f, EZAnimation.EASING_TYPE.ExponentialOut).RunAnimation();
		if (Singleton.Quality.AdvancedUIAnimations)
		{
			_itemContainer.gameObject.AddComponent<UIAnimationAlpha>().Setup(on ? 1f : 0f, on ? 0f : 1f, 0f, 0.7f, EZAnimation.EASING_TYPE.Linear, false, true).RunAnimation();
		}
	}

	public void CompareWeapon(string weaponId)
	{
		if (_weaponCompare != null)
		{
			return;
		}
		_weaponCompare = Util.CreateGameObject("WeaponCompare").GetComponent<WeaponCompare>();
		_weaponCompare.transform.parent = transform;
		_weaponCompare.transform.localPosition = new Vector3(0f, 0f, -2f);
		_weaponCompare.Init(weaponId);
		SwitchWeaponCompareMode(true);
	}

	public void SelectCurrencyGroup()
	{
		SelectGroup("currency");
	}

	public void BlockBackButton(bool block)
	{
		UIButton backButton = commonGUI.BackButton;
		GUIUtils.BlockButton(backButton, block);
	}

	public void BlockGroupButton(bool block)
	{
		UIButton componentInChildren = GetComponentInChildren<StoreGroupContainer>().SelectedGroupButton.GetComponentInChildren<UIButton>();
		GUIUtils.BlockButton(componentInChildren, block);
	}

	public void BlockAddCurrencyButtons(bool block)
	{
		GUIUtils.BlockButton(commonGUI.AddButton1, block);
		GUIUtils.BlockButton(commonGUI.AddButton2, block);
	}

	public IEnumerator OpenGroupsAnim()
	{
		yield return new WaitForSeconds(0.3f);
		_storeFrontView.groupContainer.Unfold();
	}
}
