using System;
using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

// PC port: partial; keyboard/mouse controls in HudController.Pc.cs
public partial class HudController : MonoBehaviourAndLog
{
	private struct ScopeData
	{
		public GameObject source;

		public GameObject instance;
	}

	private const float AimModeCooldown = 0.1f;

	private const float AfterDragAimModeCooldown = 0.7f;

	private const float MaxDragSqrDeltaToStartAimModeCooldown = 0.1f;

	private const float AutomaticFireDelay = 0.2f;

	private const float AfterDragAutomaticFireDelay = 30.7f;

	private const float NoAmmoWeaponSwitcherDelay = 2f;

	private Camera _camera;

	public float MinDistanceToFastAim = 7f;

	public float MinAbsAngleToFastAim = 40f;

	public float MinDistanceToDanger = 20f;

	public StickScript _leftStick;

	public StickScript _rightStick;

	public StickScript _fireStick;

	public GameObject _zoomModeButton;

	public GameObject _meleeButton;

	public GameObject _fireButton;

	public GameObject _centerGroup;

	public GameObject _switchButton;

	public GameObject _settingsButton;

	public UILoopedScrollList _consumablesScrollList;

	public GameObject _damageArrowPrefab;

	public GameObject _missionArrow;

	public HudWeapon _hudWeapon;

	public HudWeaponSelector _hudWeaponSelector;

	public HudRadar _hudRadar;

	private List<ScopeData> _scopesCache = new List<ScopeData>();

	public SpriteText MainInfoLabel;

	public GameObject NoAmmoLabel;

	public PackedSprite MissionInfoCheckboxBack;

	public PackedSprite MissionInfoCheckboxMark;

	public SpriteText MissionInfoLabel;

	public SpriteText AmmoLabel;

	public SpriteText BossNameLabel;

	public GameObject AmmoBack;

	public GameObject ReloadLabel;

	public SpriteText PowerupCountLabel;

	public SpriteText TipTextLabelLeft;

	public SpriteText TipTextLabelRight;

	public GameObject TipRight;

	public GameObject TipLeft;

	public GameObject TipRightFinger;

	public GameObject TipLeftFinger;

	public UIButton ConsumablesNextButton;

	public UIButton ConsumablesPrevButton;

	public UIProgressBar _scopeReloadBar;

	public UIProgressBar _healthBar;

	public UIProgressBar comboCounter;

	public SpriteText comboText;

	public HudTextPopup _notifier;

	public HudTextPopup _notifierLong;

	public HudTextPopup _info;

	public HudTextPopup _alert;

	public HudTextPopup _bonus;

	private Color ZoomModeButtonEnabledColor = new Color(1f, 1f, 1f, 1f);

	private Color ZoomModeButtonActivatedColor = new Color(1f, 0f, 0f, 1f);

	private Player _player;

	private bool _zoomMode;

	private bool _zoomModeTempPause;

	private bool _aimMode;

	private bool _isScopeShowing;

	private float _aimModeCooldown;

	private int _shootTap;

	private CountdownTimer _rightStickAutomaticFireTimer = new CountdownTimer();

	private CountdownTimer _leftStickDoubleTapTimer = new CountdownTimer();

	private CountdownTimer _tutorialRightTapTimer = new CountdownTimer();

	private CountdownTimer _tutorialLeftTapTimer = new CountdownTimer();

	private int _doubleTapFingerId = -1;

	private CountdownTimer _cursorInfoShowTimer = new CountdownTimer();

	private float _lastTimeAmmoWasNotEmpty;

	private bool _noAmmoWeaponSelectorWasShown;

	private Vector3 _tipRightFingerInitPos = Vector3.zero;

	private Vector3 _tipLeftFingerInitPos = Vector3.zero;

	private int _cachedWeaponAmmo = -1;

	private int _cachetTotalAmmo = -1;

	private int _cachedConsumableCount = -1;

	private bool _playerHasMeleeWeapon;

	private float _healthBarValue = -1f;

	private float _leftStickPressTime;

	private Vector3 _leftStickPressPoint = Vector2.zero;

	private int _lastBloodSplashOrientation = -1;

	private bool _moveButtonBlocked;

	private bool _switchButtonBlocked;

	private bool _missionArrowHidden;

	private bool _aimTip = true;

	private bool _aimTipEnabled;

	private bool _moveTip = true;

	private bool _moveTipEnabled;

	private List<float> _autofireWrongUsageTapTimes = new List<float>();

	public bool IsShowingCursorInfo()
	{
		return _cursorInfoShowTimer.IsRunning();
	}

	public bool FastAimMode { get; set; }

	public Player CurrentPlayer()
	{
		return _player;
	}

	public bool MoveButtonBlocked
	{
		get
		{
			return _moveButtonBlocked;
		}
		set
		{
			_moveButtonBlocked = value;
			_leftStick.ResetDelta();
			UpdateSticksSetup();
		}
	}

	public bool SwitchButtonBlocked
	{
		get
		{
			return _switchButtonBlocked;
		}
		set
		{
			_switchButtonBlocked = value;
			UpdateSticksSetup();
		}
	}

	public bool MissionArrowHidden
	{
		get
		{
			return _missionArrowHidden;
		}
		set
		{
			_missionArrowHidden = value;
			UpdateSticksSetup();
		}
	}

	public bool AimTipEnabled
	{
		get
		{
			return _aimTipEnabled;
		}
		set
		{
			_aimTipEnabled = value;
			if (!_aimTipEnabled)
			{
				TipRight.SetActiveRecursively(_aimTip && _aimTipEnabled);
			}
		}
	}

	public bool MoveTipEnabled
	{
		get
		{
			return _moveTipEnabled;
		}
		set
		{
			_moveTipEnabled = value;
			if (!_moveTipEnabled)
			{
				TipLeft.SetActiveRecursively(_moveTip && _moveTipEnabled);
			}
		}
	}

	private void Awake()
	{
		LogTo("Hud.HudController");
		CutSceneMessageOnly(false);
		WindowManager.Instance.DisableCollider();
		Singleton.UI.Hud = this;
		FastAimMode = false;
		Singleton.GameEvents.OnEnemyDismemberment += new Action<Character, Unit, Weapon, Consumable, bool>(OnEnemyDismemberment);
		Singleton.GameEvents.OnEnemyHeadshot += new Action<Character, Unit, Weapon, Consumable>(OnEnemyHeadshot);
		Singleton.GameEvents.OnEnemyMutilated += new Action<Character, Unit, Weapon, Consumable>(OnEnemyMutilated);
	}

	private void Start()
	{
		_camera = UIManager.instance.uiCameras[0].camera;
		UpdateSticksSetup();
		_zoomModeButton.GetComponent<UIButton>().AddInputDelegate(ZoomModeButtonInputDelegate);
		_rightStick.GetComponent<UIButton>().AddInputDelegate(RightStickButtonInputDelegate);
		_fireStick.GetComponent<UIButton>().AddInputDelegate(FireStickButtonInputDelegate);
		_leftStick.GetComponent<UIButton>().AddInputDelegate(LeftStickButtonInputDelegate);
		_meleeButton.GetComponent<UIButton>().AddInputDelegate(MeleeButtonInputDelegate);
		_fireButton.GetComponent<UIButton>().AddInputDelegate(FireButtonInputDelegate);
		_switchButton.GetComponent<UIButton>().AddInputDelegate(SwitchButtonInputDelegate);
		_settingsButton.GetComponent<UIButton>().AddInputDelegate(SettingsButtonInputDelegate);
		_consumablesScrollList.OnTapDelegate = OnConsumableScrollTap;
		GUIUtils.HookButton(ConsumablesNextButton, () =>
		{
			NextConsumable(1);
		}, false);
		GUIUtils.HookButton(ConsumablesPrevButton, () =>
		{
			NextConsumable(-1);
		}, false);
		_fireButton.active = false;
		_meleeButton.active = false;
		_hudWeapon.GetComponent<UIButton>().AddInputDelegate(WeaponSwitcherInputDelegate);
		UIManager.instance.AddMouseTouchPtrListener(AllInputDelegate);
		TipTextLabelRight.Text = Strings.GetString("HOLD TO AIM\nAND MOVE CAMERA");
		TipTextLabelLeft.Text = Strings.GetString("LEFT\nSTICK\nTO MOVE");
		MissionInfoLabel.text = string.Empty;
		MoveTipEnabled = false;
		AimTipEnabled = false;
		Singleton.Settings.OnSettingsChanged += new Action(UpdateSticksSetup);
		_hudWeaponSelector.OnWeaponSelected += new Action<PlayerData.Slots>(SelectWeapon);
		// PC port: key hint labels
		CreatePcKeyHints();
		// PC port: hide touch-only controls
		HidePcTouchControls();
	}

	private void NextConsumable(int shift)
	{
		_consumablesScrollList.ScrollTo(shift);
		SoundManager.PlayExclusive("power_up_scrolled", false);
	}

	private void OnLevelWasLoaded(int level)
	{
		SetZoomMode(false);
	}

	public void CutSceneMessageOnly(bool isTrue)
	{
		Show(!isTrue);
		Vector3 localPosition = BossNameLabel.transform.localPosition;
		BossNameLabel.transform.localPosition = new Vector3(localPosition.x, localPosition.y, isTrue ? 1000 : 1);
	}

	public void Show(bool show)
	{
		transform.localPosition = new Vector3(0f, 0f, show ? 100f : (-1000f));
	}

	public void UpdateSticksSetup()
	{
		if (_camera == null)
		{
			return;
		}
		Vector3 vector = _camera.ScreenToWorldPoint(Vector3.zero);
		bool flag = (bool)Scene.Mission && Scene.Mission.IsSurvivalGates();
		bool flag2 = _moveButtonBlocked || flag;
		float unusedScale;
		float num = !IsZoomMode() && !flag2 ? 0.3f : 0f;
		float num2 = IsZoomMode() ? 1f : 0.7f;
		float num3 = !IsZoomMode() ? 1f : 0.5f;
		float num4 = -vector.x * 2f;
		float num5 = Mathf.Abs(vector.y) * 2f;
		_leftStick.GetComponent<UIButton>().SetSize(num * num4, num3 * num5);
		_rightStick.GetComponent<UIButton>().SetSize(num2 * num4, num5);
		TipLeft.GetComponent<UIExpandSprite>().SetSize(0.3f * num4, num5 * 0.86f);
		TipRight.GetComponent<UIExpandSprite>().SetSize(0.65f * num4, num5 * 0.86f);
		_leftStick.GetComponent<UIAlign>().UpdateAlignment();
		_rightStick.GetComponent<UIAlign>().UpdateAlignment();
		_fireStick.GetComponent<UIAlign>().UpdateAlignment();
		_rightStick.Visible = false;
		_leftStick.Visible = !IsZoomMode() && !flag2;
		_hudRadar.MissionArrow.gameObject.SetActiveRecursively(!_missionArrowHidden && !_zoomMode);
		_zoomModeButton.GetComponent<UIButton>().SetColor(IsZoomMode() ? ZoomModeButtonActivatedColor : ZoomModeButtonEnabledColor);
		if ((bool)_player)
		{
			Weapon weapon = _player.Inventory.Weapon;
			AmmoBack.SetActiveRecursively((bool)weapon && !weapon.IsMelee());
			_healthBar.transform.parent.gameObject.SetActiveRecursively(!(_player.minHealthPercent >= 1f));
			_zoomModeButton.SetActiveRecursively((bool)weapon && (weapon.ScopeModel != null || _zoomMode));
			_meleeButton.active = _playerHasMeleeWeapon && weapon.IsMelee() && !_zoomMode && (Singleton.Settings.MeleeButton && !Singleton.Settings.MiniZombieControls);
			_fireStick.gameObject.SetActiveRecursively(Singleton.Settings.MiniZombieControls && !_zoomMode && !_meleeButton.active);
			if (_player.IsDyingOrAppearing() || _player.IsMeleeing())
			{
				_switchButton.SetActiveRecursively(false);
			}
			else
			{
				_switchButton.SetActiveRecursively(!_zoomMode && !_switchButtonBlocked);
			}
		}
	}

	public void RegisterPlayer(Player player)
	{
		_player = player;
		_player.Inventory.OnInventoryChanged += new HumanInventory.InventoryChangedDelegate(InventoryChangedDelegate);
		SyncConsumablesList();
		_hudWeapon.SetWeapon(_player.Inventory.Weapon);
		_hudWeaponSelector.RegisterPlayer(_player);
		_hudRadar.RegisterPlayer(_player);
		_playerHasMeleeWeapon = _player.Inventory.MeleeWeapon != null;
		_player.RegisterOnStateChanged(OnPlayerStateChanged);
		HudArrowsController component = GetComponent<HudArrowsController>();
		if ((bool)component)
		{
			component.RegisterPlayer(_player);
		}
		SwitchScope(false);
		UpdateSticksSetup();
		if (Tutorial.IsRunning())
		{
			_rightStick.GetComponent<UIButton>().AddInputDelegate(AutofireWrongUsageInputDelegate);
		}
	}

	private void SyncConsumablesList()
	{
		_consumablesScrollList.Clear();
		foreach (KeyValuePair<string, GameObject> consumable in _player.Inventory.Consumables)
		{
			Consumable component = consumable.Value.gameObject.GetComponent<Consumable>();
			if (!(component is AmmoRefill))
			{
				GameObject gameObject = component.CreateHudIcon();
				if ((bool)gameObject)
				{
					gameObject.GetComponent<HudConsumable>().Consumable = component;
					_consumablesScrollList.AddSceneItem(gameObject);
				}
			}
		}
		SetConsumableListVisible(true);
		_consumablesScrollList.SetCurrentItem(0);
	}

	public Vector2 GetCharacterMovementInput()
	{
		var result = _leftStick.GetVector();
		// PC port: WASD overrides the stick while held, but only while the stick itself is usable: the original hides it
		// when zoomed in (and at survival gates), so the player cannot move there. Moving while zoomed would also widen
		// the sniper spread from ~0.2 to 6 degrees (Firearm.Update).
		Vector2 pcMovement = GetPcMovementInput();
		if (pcMovement != Vector2.zero && _leftStick.Visible)
		{
			result = pcMovement;
		}
		return result;
	}

	public Vector2 GetCameraRotationDeltaAndReset()
	{
		Vector2 delta = _rightStick.GetDelta();
		_rightStick.ResetDelta();
		if (_fireStick.IsPressed())
		{
			delta = _fireStick.GetDelta();
			_fireStick.ResetDelta();
		}
		// PC port: add mouse look
		delta += GetPcMouseLookDelta();
		if (Singleton.Settings.InvertYAxis)
		{
			delta.y = -delta.y;
		}
		return delta;
	}

	public bool IsAimMode()
	{
		return _aimMode;
	}

	public bool IsZoomMode()
	{
		return _zoomMode;
	}

	public bool IsScopeShowing()
	{
		return _isScopeShowing;
	}

	public void SetBossName(string value)
	{
		BossNameLabel.Text = value;
		BossNameLabel.Hide(string.IsNullOrEmpty(value));
	}

	public void FadeBossName(bool fadeOut, float time)
	{
		UIAnimationAlpha component = BossNameLabel.GetComponent<UIAnimationAlpha>();
		if (component != null)
		{
			if (fadeOut)
			{
				component.From = 1f;
				component.To = 0f;
			}
			else
			{
				component.From = 0f;
				component.To = 1f;
			}
			component.Duration = time;
			component.RunAnimation();
		}
	}

	private void SetZoomMode(bool zoomMode)
	{
		_zoomMode = zoomMode;
		_fireButton.active = _zoomMode;
		if (zoomMode)
		{
			_meleeButton.active = false;
		}
		SetConsumableListVisible(!_zoomMode);
		_fireStick.gameObject.SetActiveRecursively(!_zoomMode);
		_hudRadar.gameObject.SetActiveRecursively(!_zoomMode);
		UpdateSticksSetup();
		if (zoomMode && (bool)Scene.Mission)
		{
			Scene.Mission.ReportAction(WaitForAction.Type.EnterZoomMode);
		}
		SwitchScope(zoomMode);
	}

	private void SetConsumableListVisible(bool visible)
	{
		_consumablesScrollList.gameObject.SetActiveRecursively(visible && _consumablesScrollList.Items.Count > 0);
		ConsumablesNextButton.gameObject.SetActiveRecursively(visible && _consumablesScrollList.Items.Count > 1);
		ConsumablesPrevButton.gameObject.SetActiveRecursively(visible && _consumablesScrollList.Items.Count > 1);
	}

	public void SwitchScope(bool on)
	{
		if (!on)
		{
			SetScope(null);
			if (_player != null)
			{
				_player.Hidden = false;
			}
			_isScopeShowing = false;
		}
		else
		{
			GameObject scope = null;
			if (_player != null)
			{
				scope = _player.Inventory.Weapon.ScopeModel;
			}
			SetScope(scope);
		}
		if ((bool)Scene.Camera)
		{
			Scene.Camera.ChangeFovInstant();
		}
	}

	private void SetScope(GameObject newScopeSource)
	{
		bool flag = false;
		foreach (ScopeData item2 in _scopesCache)
		{
			item2.instance.active = item2.source == newScopeSource;
			if (item2.source == newScopeSource)
			{
				flag = true;
			}
		}
		if (!flag && newScopeSource != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(newScopeSource) as GameObject;
			gameObject.layer = Const.LayerGui;
			gameObject.transform.parent = transform;
			gameObject.transform.localPosition = new Vector3(0f, 0f, -5f);
			float z = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.z;
			float num = 200f / z;
			gameObject.transform.localScale = new Vector3(num, num, num);
			ScopeData item;
			item.instance = gameObject;
			item.source = newScopeSource;
			_scopesCache.Add(item);
			flag = true;
		}
		if ((bool)_player)
		{
			_player.Hidden = flag;
		}
		_isScopeShowing = flag;
		_scopeReloadBar.gameObject.SetActiveRecursively(flag);
	}

	public void SelectWeapon(PlayerData.Slots slot)
	{
		_player.SetCurrentSlot(slot);
		_hudWeapon.SetWeapon(_player.Inventory.Weapon);
		if (_zoomMode)
		{
			SetZoomMode(false);
		}
		UpdateSticksSetup();
	}

	public bool ConsumeShootTap()
	{
		if (_shootTap > 0)
		{
			_shootTap--;
			return true;
		}
		return false;
	}

	public bool IsAutomaticFiring()
	{
		return (_rightStick.IsPressed() || _fireStick.IsPressed()) && !_rightStickAutomaticFireTimer.IsRunning();
	}

	public void ZoomModeButtonInputDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (!_hudWeaponSelector.IsShowing())
			{
				SetZoomMode(!_zoomMode);
			}
			break;
		}
	}

	public void MeleeButtonInputDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_shootTap++;
			break;
		}
	}

	public void AutofireWrongUsageInputDelegate(ref POINTER_INFO info)
	{
		float unusedTapTime;
		int unusedTapCount;
		float realtimeSinceStartup;
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (!_zoomMode && FastAimMode && !Singleton.Settings.MiniZombieControls)
			{
				realtimeSinceStartup = Time.realtimeSinceStartup;
				_autofireWrongUsageTapTimes.Add(realtimeSinceStartup);
				while (_autofireWrongUsageTapTimes.Count > 0 && realtimeSinceStartup - _autofireWrongUsageTapTimes[0] > 1f)
				{
					_autofireWrongUsageTapTimes.RemoveAt(0);
				}
				if (_autofireWrongUsageTapTimes.Count >= 2)
				{
					GameAnalytics.CacheLogAutofireWrongUsage(Singleton.PlayerData.Level);
					_autofireWrongUsageTapTimes.Clear();
				}
			}
			break;
		}
	}

	public void RightStickButtonInputDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			_aimTip = true;
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_rightStickAutomaticFireTimer.Run(0.2f);
			if (!_zoomMode)
			{
				_aimModeCooldown = 0.1f;
			}
			_aimTip = false;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (info.inputDelta.sqrMagnitude > 0.1f)
			{
				if (!_aimMode)
				{
					_aimModeCooldown = 0.7f;
				}
				if (_rightStickAutomaticFireTimer.IsRunning())
				{
					_rightStickAutomaticFireTimer.Run(30.7f);
				}
			}
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			_aimTip = true;
			break;
		}
	}

	public void FireStickButtonInputDelegate(ref POINTER_INFO info)
	{
		if (info.evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
		}
		else if (info.evt == POINTER_INFO.INPUT_EVENT.PRESS)
		{
			_rightStickAutomaticFireTimer.Run(0.2f);
		}
		else if (info.evt == POINTER_INFO.INPUT_EVENT.RELEASE)
		{
		}
	}

	public void LeftStickButtonInputDelegate(ref POINTER_INFO info)
	{
		float unusedSwipeTime;
		float unusedSwipeDistance;
		float horizontalDelta;
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (_zoomMode)
			{
				return;
			}
			if (_leftStickDoubleTapTimer.IsRunning() && info.id == _doubleTapFingerId)
			{
				_leftStickDoubleTapTimer.Reset();
				Scene.Camera.AutoRotate180();
			}
			else
			{
				_doubleTapFingerId = info.id;
				_leftStickDoubleTapTimer.Run(0.3f);
			}
			_moveTip = true;
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_moveTip = false;
			if (_zoomModeTempPause)
			{
				_zoomModeTempPause = false;
			}
			_leftStickPressTime = Time.realtimeSinceStartup;
			_leftStickPressPoint = info.ray.origin;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
		{
			_moveTip = true;
			horizontalDelta = info.ray.origin.x - _leftStickPressPoint.x;
			if (Singleton.Settings.UselessSwipe && Time.realtimeSinceStartup - _leftStickPressTime < 0.4f && Mathf.Abs(horizontalDelta) > 20f)
			{
				Scene.Camera.AutoRotate(horizontalDelta > 0f ? 90f : -90f, null);
			}
			break;
		}
		}
	}

	public void FireButtonInputDelegate(ref POINTER_INFO info)
	{
		if (info.evt == POINTER_INFO.INPUT_EVENT.PRESS && _zoomMode)
		{
			_shootTap++;
		}
	}

	public void SwitchButtonInputDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_hudWeaponSelector.Show();
			UpdateSticksSetup();
			break;
		}
	}

	public void SettingsButtonInputDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			Singleton.UI.ShowPauseWindow();
			break;
		}
	}

	public void OnConsumableScrollTap()
	{
		GameObject currentItem = _consumablesScrollList.GetCurrentItem();
		if (!((bool)currentItem))
		{
			return;
		}
		Consumable consumable = currentItem.GetComponent<HudConsumable>().Consumable;
		if ((bool)_player && !_player.IsDying())
		{
			_player.TryConsume(consumable.name);
		}
	}

	public void WeaponSwitcherInputDelegate(ref POINTER_INFO info)
	{
		if (info.evt == POINTER_INFO.INPUT_EVENT.TAP && _player != null)
		{
			_player.Reload();
		}
	}

	public void AllInputDelegate(POINTER_INFO info)
	{
		if (_player == null)
		{
			return;
		}
		if (!TimeControl.IsPaused() && info.evt == POINTER_INFO.INPUT_EVENT.PRESS && (bool)Scene.MainCamera)
		{
			Ray ray = Scene.MainCamera.ScreenPointToRay(info.devicePos);
			RaycastHit hitInfo;
			bool hit = Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 1 << Const.LayerGui3d);
			if (hit)
			{
				hitInfo.collider.SendMessageUpwards("OnUiTap", hitInfo.collider, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void InventoryChangedDelegate()
	{
		SyncConsumablesList();
		_hudWeaponSelector.RefreshWeapons();
	}

	public void ShowCursorInfo(string text)
	{
		MainInfoLabel.Text = text;
		_cursorInfoShowTimer.Run(0.2f);
	}

	public void ClearCursorInfo()
	{
		_cursorInfoShowTimer.Reset();
	}

	public void ShowNotifier(string text)
	{
		_notifier.Run(text);
	}

	public void ShowNotifierLong(string text)
	{
		_notifierLong.Run(text);
	}

	public void ShowInfo(string text)
	{
		_info.Run(text);
	}

	public void ShowBonus(string text, Texture2D icon)
	{
		_bonus.Run(text.TrimEnd('\n'), icon);
	}

	public void ShowAlert(string text)
	{
		_alert.Run(text);
	}

	public void SetMissionInfo(string text)
	{
		SetMissionInfo(text, true);
	}

	public void SetMissionInfo(string text, bool animate)
	{
		if (text != null && MissionInfoLabel.Text == text)
		{
			return;
		}
		if (MissionInfoLabel.Text == string.Empty)
		{
			animate = false;
		}
		new List<MonoBehaviour>
		{
			MissionInfoCheckboxBack,
			MissionInfoCheckboxMark,
			MissionInfoLabel
		}.ForEach((MonoBehaviour s) =>
		{
			s.gameObject.GetComponents<UIAnimation>().ToList().ForEach((UIAnimation a) =>
			{
				UnityEngine.Object.Destroy(a);
			});
		});
		if (animate)
		{
			MissionInfoCheckboxMark.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 0f, 1f, EZAnimation.EASING_TYPE.CubicOut).RunAnimation()
				.OnFinishDelegate = () =>
			{
				MissionInfoCheckboxMark.gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 0f, 0f, 1f, EZAnimation.EASING_TYPE.CubicIn).RunAnimation();
			};
			MissionInfoLabel.gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 0f, 1f, 1f, EZAnimation.EASING_TYPE.CubicIn).RunAnimation()
				.OnFinishDelegate = () =>
			{
				if (text != null)
				{
					MissionInfoLabel.Text = text;
				}
				MissionInfoLabel.gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 1f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation()
					.gameObject.AddComponent<UIAnimationScale>().Setup(1f, 1f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation()
					.gameObject.AddComponent<UIAnimationRotate>().Setup(0f, 0f, Vector3.right, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation()
					.gameObject.AddComponent<UIAnimationTypingText>().Setup(15f, 0f, 0f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
			};
		}
		else
		{
			if (text != null)
			{
				MissionInfoLabel.Text = text;
			}
			MissionInfoLabel.gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 1f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation()
				.gameObject.AddComponent<UIAnimationScale>().Setup(1f, 1f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation()
				.gameObject.AddComponent<UIAnimationRotate>().Setup(0f, 0f, Vector3.right, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
			MissionInfoCheckboxMark.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 0f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear).RunAnimation();
		}
	}

	public void ClearInfo()
	{
		_cursorInfoShowTimer.Reset();
	}

	public void OnPlayerStateChanged(Character.State fromState, Character.State toState)
	{
		if (fromState == Character.State.Reloading)
		{
			if (_zoomModeTempPause)
			{
				SetZoomMode(true);
				_zoomModeTempPause = false;
			}
		}
		else if (fromState == Character.State.Appearing)
		{
			UpdateSticksSetup();
		}
		if (toState == Character.State.Reloading)
		{
			if (IsZoomMode())
			{
				SetZoomMode(false);
				_zoomModeTempPause = true;
			}
		}
		else if (toState == Character.State.Dying && IsZoomMode())
		{
			SetZoomMode(false);
		}
	}

	private void Update()
	{
		if (_player == null)
		{
			return;
		}
		if (_player.Inventory == null)
		{
			return;
		}
		Weapon weapon = _player.Inventory.Weapon;
		_healthBarValue = !_player.IsDyingOrAppearing() ? _player.Health / _player.healthTotal : 0f;
		if (_healthBar.Value != _healthBarValue)
		{
			_healthBar.Value = _healthBarValue;
		}
		if (Singleton.Settings.MiniZombieControls)
		{
			_aimMode = _fireStick.IsPressed();
		}
		else if (_rightStick.IsPressed())
		{
			if (FastAimMode)
			{
				_aimMode = true;
			}
			else if (!_leftStick.IsPressed() && !_aimMode)
			{
				if (_aimModeCooldown > 0f)
				{
					_aimModeCooldown -= Time.deltaTime;
				}
				if (_aimModeCooldown <= 0f)
				{
					_aimMode = true;
				}
			}
		}
		else
		{
			_aimMode = false;
		}
		// PC port: keyboard/mouse controls (may override _aimMode)
		UpdatePcControls();
		if (MoveTipEnabled)
		{
			if (_moveTip)
			{
				TipLeft.SetActiveRecursively(!_tutorialLeftTapTimer.IsRunning());
			}
			else
			{
				_tutorialLeftTapTimer.Run(2f);
				TipLeft.SetActiveRecursively(false);
			}
		}
		if (AimTipEnabled)
		{
			if (_aimTip)
			{
				TipRight.SetActiveRecursively(!_tutorialRightTapTimer.IsRunning());
			}
			else
			{
				_tutorialRightTapTimer.Run(2f);
				TipRight.SetActiveRecursively(false);
				AimTipEnabled = false;
			}
		}
		UpdateAnimation();
		if (_isScopeShowing)
		{
			_scopeReloadBar.Value = 1f - weapon.GetTimeToNextShot() * weapon.GetRateOfFire();
		}
		bool flag = _cursorInfoShowTimer.IsRunning();
		MainInfoLabel.gameObject.active = flag;
		if (flag)
		{
			MainInfoLabel.SetColor(new Color(1f, 0f, 0f, 1f - _cursorInfoShowTimer.Progress()));
		}
		if ((bool)weapon)
		{
			bool flag2 = weapon.IsAmmoEmpty();
			NoAmmoLabel.active = flag2;
			if (_cachedWeaponAmmo != weapon.GetAmmoInClip() || _cachetTotalAmmo != weapon.GetAmmoInBackpack())
			{
				AmmoLabel.Text = string.Empty + weapon.GetAmmoInClip() + (weapon.GetMaxAmmo() > 0 ? " / " + weapon.GetAmmoInBackpack() : string.Empty);
				_cachedWeaponAmmo = weapon.GetAmmoInClip();
				_cachetTotalAmmo = weapon.GetAmmoInBackpack();
			}
			if (_zoomMode && flag2)
			{
				SetZoomMode(false);
			}
			ReloadLabel.active = (float)_cachedWeaponAmmo < (float)weapon.GetAmmoClipSize() * 0.4f;
			float realTime = TimeControl.RealTime;
			if (flag2)
			{
				if (!_hudWeaponSelector.IsShowing() && !_noAmmoWeaponSelectorWasShown && realTime - _lastTimeAmmoWasNotEmpty > 2f)
				{
					_hudWeaponSelector.Show();
					_noAmmoWeaponSelectorWasShown = true;
				}
			}
			else
			{
				_lastTimeAmmoWasNotEmpty = realTime;
				_noAmmoWeaponSelectorWasShown = false;
			}
		}
		else
		{
			AmmoLabel.Text = string.Empty;
		}
		Task currentTask = Scene.Mission.GetCurrentTask();
		HintArrow hintArrow = currentTask != null ? currentTask.GetTargetHintArrow() : null;
		if (hintArrow != null && hintArrow.gameObject.active && _player.IsMissionTargetVisible())
		{
			Vector3 vector = Scene.MainCamera.WorldToViewportPoint(hintArrow.OriginalPosition);
			Vector3 vector2 = hintArrow.OriginalPosition - Scene.MainCamera.transform.position;
			bool flag3 = vector.z > 0f;
			if (vector.x > 0f && vector.x < 1f && flag3)
			{
				Singleton.UI.Arrow.Hide();
			}
			else
			{
				float unusedLocal1;
				float num = GUIUtils.ScreenWidth() * 0.5f - 10f;
				float y = 0f;
				float z = Singleton.UI.Hud.transform.position.z - 10f;
				bool flag4 = vector.x < 0f;
				if (!flag3)
				{
					flag4 = !flag4;
				}
				Singleton.UI.Arrow.SetTarget(new Vector3(flag4 ? -num : num, y, z), flag4 ? 90f : 270f, 0f, 100000f);
			}
		}
		else
		{
			Singleton.UI.Arrow.Hide();
		}
		GameObject currentItem = _consumablesScrollList.GetCurrentItem();
		Consumable consumable = currentItem ? currentItem.GetComponent<HudConsumable>().Consumable : null;
		if ((bool)consumable)
		{
			if (_cachedConsumableCount != consumable.Count)
			{
				PowerupCountLabel.Text = string.Empty + consumable.Count;
				_cachedConsumableCount = consumable.Count;
			}
		}
		else
		{
			PowerupCountLabel.Text = string.Empty;
		}
	}

	private void AutoSelectWeapon()
	{
		SelectWeapon(BestSuitableWeapon());
	}

	private bool CheckSlot(PlayerData.Slots slot, float ammoPercent)
	{
		Weapon weapon = _player.Inventory.WeaponsBySlot[(int)slot];
		return weapon != null && weapon.GetAmmoLeftPercent() > ammoPercent;
	}

	private PlayerData.Slots BestSuitableWeapon()
	{
		Weapon[] weaponsBySlot = _player.Inventory.WeaponsBySlot;
		if (CheckSlot(PlayerData.Slots.assault, 20f))
		{
			return PlayerData.Slots.assault;
		}
		if (CheckSlot(PlayerData.Slots.shotgun, 20f))
		{
			return PlayerData.Slots.shotgun;
		}
		if (CheckSlot(PlayerData.Slots.handgun, 20f))
		{
			return PlayerData.Slots.handgun;
		}
		if (CheckSlot(PlayerData.Slots.assault, 0f))
		{
			return PlayerData.Slots.assault;
		}
		if (CheckSlot(PlayerData.Slots.shotgun, 0f))
		{
			return PlayerData.Slots.shotgun;
		}
		if (CheckSlot(PlayerData.Slots.handgun, 0f))
		{
			return PlayerData.Slots.handgun;
		}
		if (CheckSlot(PlayerData.Slots.sniper, 0f))
		{
			return PlayerData.Slots.sniper;
		}
		return PlayerData.Slots.melee;
	}

	private void OnEnemyDismemberment(Character source, Unit unit, Weapon weapon, Consumable consumable, bool isFatal)
	{
		SprayBlood(unit, source);
	}

	private void OnEnemyHeadshot(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		SprayBlood(unit, source);
	}

	private void OnEnemyMutilated(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		SprayBlood(unit, source);
	}

	private void SprayBlood(Unit unit, Character destination)
	{
		if (destination is Player && (destination.transform.position - unit.transform.position).sqrMagnitude < 6.25f)
		{
			CreateBloodSplash();
		}
	}

	public void CreateBloodSplash()
	{
		if (!Singleton.Quality.HudBloodSplashes)
		{
			return;
		}
		if (HudSplash.CurrentSplashCount >= 2)
		{
			return;
		}
		string text = Util.Chance(50f) ? "HudBloodSplash1" : "HudBloodSplash2";
		GameObject gameObject = Singleton.Cache.PerPrefabCache.Instantiate(Singleton.Cache.PerResourceCache.Load(text), 8);
		gameObject.transform.parent = transform;
		_lastBloodSplashOrientation = (_lastBloodSplashOrientation + UnityEngine.Random.Range(0, 3) + 1) % 4;
		gameObject.GetComponent<HudSplash>().Init(_lastBloodSplashOrientation);
	}

	public HudArrow CreateMissionArrow()
	{
		return _missionArrow.GetComponent<HudArrow>();
	}

	public GameObject CreateDamageArrow()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_damageArrowPrefab) as GameObject;
		Util.MarkAllChildrenAsClone(gameObject);
		gameObject.transform.parent = _centerGroup.transform;
		gameObject.transform.localPosition = Vector3.zero;
		return gameObject;
	}

	private void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.Settings.OnSettingsChanged -= new Action(UpdateSticksSetup);
			Singleton.GameEvents.OnEnemyDismemberment -= new Action<Character, Unit, Weapon, Consumable, bool>(OnEnemyDismemberment);
			Singleton.GameEvents.OnEnemyHeadshot -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyHeadshot);
			Singleton.GameEvents.OnEnemyMutilated -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyMutilated);
		}
		if (UIManager.Exists())
		{
			UIManager.instance.RemoveMouseTouchPtrListener(AllInputDelegate);
		}
		// PC port: release the cursor when leaving the level
		SetPcCursorLocked(false);
	}

	private void UpdateAnimation()
	{
		float unusedLocal1;
		float unusedLocal2;
		if (MoveTipEnabled)
		{
			if ((bool)TipLeftFinger.GetComponent<UIAlign>())
			{
				UnityEngine.Object.Destroy(TipLeftFinger.GetComponent<UIAlign>());
				_tipLeftFingerInitPos = TipLeftFinger.transform.localPosition;
			}
			Vector3 localPosition = _tipLeftFingerInitPos + Vector3.right * 10f + Vector3.right * 10f * Mathf.Sin(5f * Time.time);
			TipLeftFinger.transform.localPosition = localPosition;
		}
		if (AimTipEnabled)
		{
			if ((bool)TipRightFinger.GetComponent<UIAlign>())
			{
				UnityEngine.Object.Destroy(TipRightFinger.GetComponent<UIAlign>());
				_tipRightFingerInitPos = TipRightFinger.transform.localPosition;
			}
			Vector3 localPosition2 = _tipRightFingerInitPos - Vector3.right * 10f - Vector3.right * 10f * Mathf.Sin(5f * Time.time);
			TipRightFinger.transform.localPosition = localPosition2;
		}
	}
}
