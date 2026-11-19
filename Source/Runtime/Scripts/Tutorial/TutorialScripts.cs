using System;
using Glu.Localization;
using UnityEngine;
using System.Collections.Generic;

public class HowToAimTutorial : TutorialScript
{
	private bool _taskShowed;

	private void Awake()
	{
		Singleton.UI.Hud.MissionArrowHidden = true;
		Singleton.UI.Hud.MoveButtonBlocked = true;
		Singleton.UI.Hud.SwitchButtonBlocked = true;
		Singleton.UI.Hud.AimTipEnabled = true;
		Singleton.Tutorial.Arrow.Hide();
		Singleton.UI.Hud.SetMissionInfo(string.Empty);
		Scene.Player.Health = Scene.Player.healthTotal * 0.5f;
		Scene.Mission.OnTaskChanged += new Action<Task>(OnTaskChanged);
		Scene.Camera.SetAngleYLock(true, 250f, 365f);
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.First_Mission_Started);
		if (App.FirstGameLaunch && QualitySetter.PlayHavenEnabled)
		{
			PlayHavenInterface.StartPublisherContentRequest("tutorial_start", false, false);
		}
	}

	public void OnTaskChanged(Task newTask)
	{
		if (newTask.type == Task.Type.CutScene)
		{
			Finish();
		}
	}

	private void Update()
	{
		if (!_taskShowed && Singleton.UI.Hud._rightStick.IsPressed())
		{
			_taskShowed = true;
			GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Pressed_Right_Part_To_Aim);
			Singleton.UI.Hud.SetMissionInfo(Scene.Mission.GetCurrentTask().GetText());
		}
		if (Scene.Player.Inventory.Weapon.GetAmmoInBackpack() == 0)
		{
			ResupplyAmmo.Resupply(Scene.Player.Inventory.Weapon, 1f);
		}
	}

	protected override void Finish()
	{
		Scene.Mission.OnTaskChanged -= new Action<Task>(OnTaskChanged);
		Singleton.UI.Hud.AimTipEnabled = false;
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Killed_First_Zombies);
		GoToNextScript<WatchShowTutorial>();
	}
}

public class WatchShowTutorial : TutorialScript
{
	private void Awake()
	{
		Scene.Mission.OnTaskChanged += new Action<Task>(OnTaskChanged);
		Check();
	}

	public void OnTaskChanged(Task newTask)
	{
		Check();
	}

	private void Check()
	{
		if ((bool)Scene.Mission.GetCurrentTask() && Scene.Mission.GetCurrentTask().type == Task.Type.WaitForAction)
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		Scene.Mission.OnTaskChanged -= new Action<Task>(OnTaskChanged);
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Helicopter_Explosion);
		GoToNextScript<HealSurvivorTutorial>();
	}
}

public class HealSurvivorTutorial : TutorialScript
{
	private UISpriteBlinker _healBlinker;

	private void Awake()
	{
		Scene.Player.Inventory.AddConsumable("MedkitLarge", 1, false, true);
		_healBlinker = Singleton.UI.Hud.GetComponentInChildren<HudConsumable>()._sprite.gameObject.AddComponent<UISpriteBlinker>();
		_healBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		_healBlinker.Freq = 12f;
		Singleton.Tutorial.Arrow.SetTarget(_healBlinker.gameObject.transform.position, -45f, 10f);
		Scene.Mission.OnTaskChanged += new Action<Task>(OnTaskChanged);
	}

	public void OnTaskChanged(Task newTask)
	{
		Finish();
	}

	protected override void Finish()
	{
		Singleton.Tutorial.Arrow.Hide();
		if ((bool)_healBlinker)
		{
			_healBlinker.Finish();
		}
		Scene.Mission.OnTaskChanged -= new Action<Task>(OnTaskChanged);
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Healing_Button_Tapped);
		GoToNextScript<HowToMoveTutorial>();
	}
}

public class HowToMoveTutorial : TutorialScript
{
	private Vector3 _startPos;

	private void Awake()
	{
		Singleton.UI.Hud.MoveButtonBlocked = false;
		Singleton.UI.Hud.MoveTipEnabled = true;
		Singleton.UI.Hud._leftStick.IsDynamic = false;
		_startPos = Scene.Player.transform.position;
		Singleton.UI.Hud.SetMissionInfo(Strings.GetString("Move to next area"));
		Scene.Camera.SetAngleYLock(false);
	}

	private void Update()
	{
		if ((_startPos - Scene.Player.transform.position).sqrMagnitude > 1f)
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		Singleton.UI.Hud.MoveTipEnabled = false;
		Singleton.UI.Hud._leftStick.IsDynamic = true;
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Pressed_Left_Part_To_Move);
		GoToNextScript<NavigationTutorial>();
	}
}

public class NavigationTutorial : TutorialScript
{
	private UISpriteBlinker _navigationBlinker;

	private void Awake()
	{
		Singleton.UI.Hud.MissionArrowHidden = false;
		Scene.Mission.OnTaskChanged += new Action<Task>(OnTaskChanged);
		_navigationBlinker = Singleton.UI.Hud._hudRadar.MissionArrow.transform.GetChild(0).gameObject.AddComponent<UISpriteBlinker>();
		_navigationBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		_navigationBlinker.AnimateScale = true;
	}

	public void OnTaskChanged(Task newTask)
	{
		if ((bool)newTask.GetComponentInChildren<WaitForAction>())
		{
			Finish();
		}
		else
		{
			GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Second_Zombies_Appeared);
		}
	}

	private void Update()
	{
		if (Scene.Player.Inventory.Weapon.GetAmmoInBackpack() == 0)
		{
			ResupplyAmmo.Resupply(Scene.Player.Inventory.Weapon, 1f);
		}
	}

	protected override void Finish()
	{
		_navigationBlinker.Finish();
		Scene.Mission.OnTaskChanged -= new Action<Task>(OnTaskChanged);
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Killed_Second_Zombies);
		GoToNextScript<SwitchTutorial>();
	}
}

public class SwitchTutorial : TutorialScript
{
	private UISpriteBlinker _switchBlinker;

	private UISpriteBlinker _weaponBlinker;

	private void Awake()
	{
		Singleton.UI.Hud.SwitchButtonBlocked = false;
		Singleton.UI.Hud.MissionArrowHidden = true;
		_switchBlinker = Singleton.UI.Hud._switchButton.AddComponent<UISpriteBlinker>();
		_switchBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		_switchBlinker.Freq = 12f;
		_weaponBlinker = Singleton.UI.Hud._hudWeaponSelector.Slots.gameObject.FindChild("Melee").FindChildWithComponent<UISingleSprite>().gameObject.AddComponent<UISpriteBlinker>();
		_weaponBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		_weaponBlinker.Freq = 12f;
		Scene.Mission.OnTaskChanged += new Action<Task>(OnTaskChanged);
		Singleton.Tutorial.Arrow.Hide();
	}

	private void Update()
	{
		bool flag = _weaponBlinker.gameObject.active;
		_switchBlinker.Play(!flag);
		if (flag)
		{
			Singleton.Tutorial.Arrow.SetTarget(_weaponBlinker.transform.position, 45f, 10f);
		}
		else
		{
			Singleton.Tutorial.Arrow.SetTarget(_switchBlinker.GetComponent<UIButton>(), 225f, 5f);
		}
	}

	public void OnTaskChanged(Task newTask)
	{
		Finish();
	}

	protected override void Finish()
	{
		Singleton.Tutorial.Arrow.Hide();
		_switchBlinker.Finish();
		_weaponBlinker.Finish();
		Scene.Mission.OnTaskChanged -= new Action<Task>(OnTaskChanged);
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Switched_To_The_Bat);
		GoToNextScript<MeleeTutorial>();
		Singleton.Tutorial.Arrow.Hide();
	}
}

public class MeleeTutorial : TutorialScript
{
	private UIButton _meleeButton;

	private bool _changedWeapon;

	private void Awake()
	{
		_meleeButton = Singleton.UI.Hud._meleeButton.GetComponent<UIButton>();
		Singleton.UI.Hud.MissionArrowHidden = false;
	}

	private void Update()
	{
		if (!_changedWeapon && (bool)Scene.Player && Scene.Player.Inventory.GetCurrentSlot() != PlayerData.Slots.melee)
		{
			_changedWeapon = true;
			GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Switched_Back_To_Firearms_1);
		}
		Singleton.UI.Hud.AimTipEnabled = !Singleton.Settings.MeleeButton && Singleton.UI.Hud.FastAimMode && Scene.Player.Inventory.Weapon.IsMelee();
		if (Singleton.Settings.MeleeButton && Singleton.UI.Hud.FastAimMode && _meleeButton.gameObject.active)
		{
			Singleton.Tutorial.Arrow.SetTarget(_meleeButton, 225f, 15f);
		}
		else
		{
			Singleton.Tutorial.Arrow.Hide();
		}
		if (!Scene.Player.Inventory.Weapon.IsMelee() && Scene.Player.Inventory.Weapon.GetAmmoInBackpack() == 0)
		{
			ResupplyAmmo.Resupply(Scene.Player.Inventory.Weapon, 1f);
		}
	}

	protected override void Finish()
	{
		Singleton.Tutorial.Arrow.Hide();
		UnityEngine.Object.Destroy(this);
	}
}

public class EnterTheStore : TutorialScript
{
	private UISpriteBlinker _shopBlinker;

	private void Start()
	{
		_shopBlinker = Util.Find("ShopButton").AddComponent<UISpriteBlinker>();
		_shopBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		Singleton.Tutorial.Arrow.SetTarget(_shopBlinker.GetComponent<UIButton>(), 135f, 5f + _shopBlinker.GetComponent<UIButton>().width * 0.5f);
		Singleton.Tutorial.Tip.SetTip(Strings.GetString("Go to the Store and buy a medkit"), new Vector3(GUIUtils.ScreenWidth() * -0.082f, -88.5f, _shopBlinker.transform.position.z));
		Array.ForEach(Util.Find("MapWindow").GetComponentsInChildren<MapLocation>(), (MapLocation o) =>
		{
			if ((bool)o.Button)
			{
				o.Button.gameObject.AddComponent<UIButtonBlocker>();
			}
		});
	}

	private void Update()
	{
		if (StoreScreen.ShopIsOpen)
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		Singleton.Tutorial.Arrow.Hide();
		Singleton.Tutorial.Tip.Hide();
		_shopBlinker.Finish();
		GoToNextScript<OpenConsumables>();
	}
}

public class OpenConsumables : TutorialScript
{
	private UISpriteBlinker _selectBlinker;

	private UISpriteBlinker _consumableBlinker;

	private StoreGroupContainer _groupContainer;

	private void Awake()
	{
		_groupContainer = Singleton.UI.Store.GetComponentInChildren<StoreGroupContainer>();
		_selectBlinker = _groupContainer.SelectedGroupButton.GetComponentInChildren<UIButton>().gameObject.AddComponent<UISpriteBlinker>();
		_selectBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		Singleton.UI.Store.BlockBackButton(true);
		Singleton.UI.Store.BlockAddCurrencyButtons(true);
	}

	private void Update()
	{
		if (_groupContainer.GetState() == StoreGroupContainer.State.Folding)
		{
			_selectBlinker.Play(true);
			Singleton.Tutorial.Arrow.SetTarget(_selectBlinker.GetComponent<UIButton>(), 0f, 0f);
		}
		else
		{
			if (_consumableBlinker == null)
			{
				foreach (StoreGroupButton groupButton in _groupContainer.groupButtons)
				{
					if (groupButton.name == "consumables")
					{
						_consumableBlinker = groupButton.GetComponentInChildren<UIButton>().gameObject.AddComponent<UISpriteBlinker>();
						_consumableBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
						_consumableBlinker.SetOriginalColor(Color.white);
					}
					else
					{
						groupButton.GetComponentInChildren<UIButton>().gameObject.AddComponent<UIButtonBlocker>();
					}
				}
			}
			_consumableBlinker.Play(true);
			_selectBlinker.Play(false);
			Singleton.Tutorial.Arrow.SetTarget(_consumableBlinker.GetComponent<UIButton>(), 45f, 5f);
		}
		if (_groupContainer.GetSelectedGroup().id == "consumables")
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		_selectBlinker.Finish();
		if ((bool)_consumableBlinker)
		{
			UnityEngine.Object.Destroy(_consumableBlinker);
		}
		GoToNextScript<BuyGrenade>();
	}
}

public class BuyGrenade : TutorialScript
{
	private UISpriteBlinker _buyBlinker;

	private ConsumableShopItem _item;

	private void Start()
	{
		Singleton.UI.Store.BlockGroupButton(true);
		foreach (ConsumableStoreFrontItemPlate consumableStoreFrontItemPlate in Singleton.UI.Store.GetComponentsInChildren<ConsumableStoreFrontItemPlate>())
		{
			if (consumableStoreFrontItemPlate.ShopItem.id == "MedkitLargeX3")
			{
				_item = consumableStoreFrontItemPlate.ShopItem as ConsumableShopItem;
				_item.OnStateChange += new Action(OnItemStateChange);
				_buyBlinker = consumableStoreFrontItemPlate.BuyButton.gameObject.AddComponent<UISpriteBlinker>();
				_buyBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
				_buyBlinker.SetOriginalColor(Color.white);
				break;
			}
		}
	}

	private void Update()
	{
		if (_buyBlinker != null)
		{
			Singleton.Tutorial.Arrow.SetTarget(_buyBlinker.transform.position, 0f, 10f);
		}
	}

	private void OnItemStateChange()
	{
		Finish();
	}

	protected override void Finish()
	{
		Array.ForEach(Util.Find("MapWindow").GetComponentsInChildren<UIButtonBlocker>(), (UIButtonBlocker b) =>
		{
			b.Unblock();
		});
		_item.OnStateChange -= new Action(OnItemStateChange);
		Singleton.UI.Store.BlockGroupButton(false);
		Singleton.UI.Store.BlockBackButton(false);
		Singleton.UI.Store.BlockAddCurrencyButtons(false);
		UnityEngine.Object.Destroy(_buyBlinker);
	}
}

public class ChoiceMissionIcon : TutorialScript
{
	private UISpriteBlinker _backBlinker;

	private UISpriteBlinker _missionBlinker;

	private bool _popupOpenedEventSent;

	private void Start()
	{
		TutorialScript.CheckHardCurrencySpent();
	}

	private void Update()
	{
		if (!Singleton.Exists())
		{
			return;
		}
		if (_missionBlinker == null)
		{
			MapLocation mapLocation = Array.Find(Util.Find("MapWindow").GetComponentsInChildren<MapLocation>(), (MapLocation o) => o.MissionData != null && o.MissionData.Type == Mission.Type.Story2);
			if (mapLocation != null)
			{
				_missionBlinker = mapLocation.Button.AddComponent<UISpriteBlinker>();
				_missionBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
			}
		}
		MissionInfoPopup missionInfoPopup = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is MissionInfoPopup) as MissionInfoPopup;
		if ((bool)missionInfoPopup)
		{
			if (!_popupOpenedEventSent)
			{
				GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Second_Mission_Popup_Opened);
				_popupOpenedEventSent = true;
			}
			Singleton.Tutorial.Tip.Hide();
			Singleton.Tutorial.Arrow.SetTarget(missionInfoPopup.StartButton, 135f, 8f);
		}
		else if (StoreScreen.ShopIsOpen)
		{
			Singleton.Tutorial.Tip.Hide();
			if (_backBlinker == null)
			{
				_backBlinker = Singleton.UI.Store.gameObject.GetComponentInChildren<CommonGUI>().BackButton.gameObject.AddComponent<UISpriteBlinker>();
				_backBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
			}
			Singleton.Tutorial.Arrow.SetTarget(_backBlinker.GetComponent<UIButton>(), 135f, 10f);
		}
		else
		{
			Singleton.Tutorial.Tip.Hide();
			if ((bool)_missionBlinker)
			{
				Singleton.Tutorial.Arrow.SetTarget(_missionBlinker.GetComponent<UIButton>(), 45f, 7f);
			}
		}
	}

	protected override void Finish()
	{
		UnityEngine.Object.Destroy(_backBlinker);
		UnityEngine.Object.Destroy(this);
	}
}

public class HowToMove2Tutorial : TutorialScript
{
	private Vector3 _startPos;

	private bool _weaponGiven;

	private void Awake()
	{
		Scene.Player.minHealthPercent = 0.1f;
		Singleton.UI.Hud.MoveTipEnabled = true;
		Singleton.UI.Hud._leftStick.IsDynamic = false;
		_startPos = Scene.Player.transform.position;
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Second_Mission_Started);
	}

	private void Update()
	{
		if (!_weaponGiven)
		{
			WeaponShopItem weaponShopItem = Singleton.ShopData.GetShopItem(Singleton.GameData.TutorialDemoWeaponShopId) as WeaponShopItem;
			if (weaponShopItem != null && Scene.Player.Inventory.WeaponsBySlot[(int)weaponShopItem.Slot] == null)
			{
				Scene.Player.Inventory.AddEquippable(weaponShopItem.item, weaponShopItem.Slot);
				Singleton.UI.Hud.SelectWeapon(weaponShopItem.Slot);
			}
			_weaponGiven = true;
		}
		if ((_startPos - Scene.Player.transform.position).sqrMagnitude > 2f)
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		Singleton.UI.Hud.MoveTipEnabled = false;
		Singleton.UI.Hud._leftStick.IsDynamic = true;
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Pressed_Left_Part_To_Move_2nd_Mission);
		GoToNextScript<HowToAim2Tutorial>();
	}
}

public class HowToAim2Tutorial : TutorialScript
{
	private bool _aim;

	private int _showCount;

	private void Awake()
	{
	}

	private void Update()
	{
		bool fastAimMode = Singleton.UI.Hud.FastAimMode;
		Singleton.UI.Hud.AimTipEnabled = fastAimMode;
		if (_aim != fastAimMode)
		{
			_showCount++;
		}
		_aim = fastAimMode;
		if (_showCount >= 2)
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Pressed_Right_Part_To_Aim_2nd_Mission);
		GoToNextScript<BeaconTutorial>();
	}
}

public class BeaconTutorial : TutorialScript
{
	private enum State
	{
		Pre,
		Slow,
		Stop
	}

	private int quantityBefore;

	private State state;

	private CountdownTimer timer = new CountdownTimer();

	private void Update()
	{
		string unusedLocal1;
		Dictionary<string, GameObject> consumables = Scene.Player.Inventory.Consumables;
		if (state == State.Pre && Scene.Mission.GetCurrentTask().gameObject.name.Contains("BeaconTutorial"))
		{
			state = State.Slow;
			Scene.Camera.SetAngleXLock(true, Scene.Camera.transform.rotation.x, Scene.Camera.transform.rotation.x);
			Scene.Camera.SetAngleYLock(true, Scene.Camera.transform.rotation.y - 95f, Scene.Camera.transform.rotation.y - 95f);
			Singleton.UI.Hud.MoveButtonBlocked = true;
			Singleton.UI.Hud.SwitchButtonBlocked = true;
			timer.Run(1f);
			TimeControl.TimeScale(0.2f);
			Singleton.Tutorial.Arrow.SetTarget(Singleton.UI.Hud._consumablesScrollList.transform.position, -45f, 12f);
			if (!consumables.ContainsKey("GrenadeBeacon") || consumables["GrenadeBeacon"].GetComponent<Consumable>().Count < 1)
			{
				Scene.Player.Inventory.AddConsumable("GrenadeBeacon", 1, false, true);
			}
			quantityBefore = consumables["GrenadeBeacon"].GetComponent<Consumable>().Count;
			UILoopedScrollList consumablesScrollList = Singleton.UI.Hud._consumablesScrollList;
			consumablesScrollList.SetCurrentItem(consumablesScrollList.Items.FindIndex((GameObject o) => o.GetComponent<HudConsumable>().Consumable.name == "GrenadeBeacon"));
			GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Zombies_Horde_Appeared);
		}
		if (state == State.Slow && !timer.IsRunning())
		{
			state = State.Stop;
			TimeControl.TimeScale(1f);
			UILoopedScrollList consumablesScrollList2 = Singleton.UI.Hud._consumablesScrollList;
			consumablesScrollList2.SetCurrentItem(consumablesScrollList2.Items.FindIndex((GameObject o) => o.GetComponent<HudConsumable>().Consumable.name == "GrenadeBeacon"));
			TimeControl.Pause(true, TimeControl.PauseTypes.CutScene);
		}
		if ((state == State.Slow || state == State.Stop) && (!consumables.ContainsKey("GrenadeBeacon") || consumables["GrenadeBeacon"].GetComponent<Consumable>().Count < quantityBefore))
		{
			Finish();
		}
	}

	protected override void Finish()
	{
		Singleton.Tutorial.Arrow.Hide();
		Scene.Camera.SetAngleXLock(false, 0f, 0f);
		Scene.Camera.SetAngleYLock(false, 0f, 0f);
		Singleton.UI.Hud.MoveButtonBlocked = false;
		Singleton.UI.Hud.SwitchButtonBlocked = false;
		TimeControl.Pause(false, TimeControl.PauseTypes.CutScene);
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Beacon_Grenade_Thrown);
		UnityEngine.Object.Destroy(this);
	}
}

public class RunSurvivalMode : TutorialScript
{
	private UISpriteBlinker _missionBlinker;

	private void Start()
	{
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Tutorial_Completed);
		TutorialScript.CheckHardCurrencySpent();
		MapLocation[] componentsInChildren = Singleton.UI.MapWindow.GetComponentsInChildren<MapLocation>();
		if (componentsInChildren == null)
		{
			return;
		}
		MapLocation location = Array.Find(componentsInChildren, (MapLocation o) => o.MissionData != null);
		if (Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted == 1 && location != null)
		{
			_missionBlinker = location.Button.AddComponent<UISpriteBlinker>();
			_missionBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		}
		else
		{
			location = Array.Find(componentsInChildren, (MapLocation o) => o.MissionData != null && Mission.IsSurvivalGates(o.MissionData.Type));
			if (location != null)
			{
				_missionBlinker = location.Button.AddComponent<UISpriteBlinker>();
				_missionBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
				Singleton.UI.MapWindow.ShowTip(Strings.GetString("Challenge with your friends in Survival\nmode and receive a cash reward."));
				GUIUtils.HookButton(location.Button.GetComponent<UIButton>(), () =>
				{
					Singleton.UI.MapWindow.HideTip();
					if (!Tutorial.HasPlayerAnyWeaponFromGroup("sniper"))
					{
						Singleton.UI.PopupManager.ShowTutorialSniperRifleSellPopup(location);
					}
				});
			}
		}
	}

	private void Update()
	{
		if (!Singleton.Exists())
		{
			return;
		}
		if (_missionBlinker == null)
		{
			return;
		}
		MissionInfoPopup missionInfoPopup = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is MissionInfoPopup) as MissionInfoPopup;
		do
		{
			if (missionInfoPopup)
			{
				break;
			}
			if (_missionBlinker)
			{
				Singleton.Tutorial.Arrow.SetTarget(_missionBlinker.GetComponent<UIButton>(), 45f, 7f);
			}
		}
		while (false);
	}

	protected override void Finish()
	{
		UnityEngine.Object.Destroy(this);
	}
}

public class IAmASniperTutorial : TutorialScript
{
	private UISpriteBlinker _switchBlinker;

	private UISpriteBlinker _weaponBlinker;

	private void Awake()
	{
		_switchBlinker = Singleton.UI.Hud._switchButton.AddComponent<UISpriteBlinker>();
		_switchBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		_switchBlinker.Freq = 12f;
		_weaponBlinker = Singleton.UI.Hud._hudWeaponSelector.Slots.gameObject.FindChild("Sniper").FindChildWithComponent<UISingleSprite>().gameObject.AddComponent<UISpriteBlinker>();
		_weaponBlinker.BlinkColor = TutorialScript.UIBlinkColor1;
		_weaponBlinker.Freq = 12f;
		Singleton.GameEvents.OnEnemyKill += new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
	}

	private void Update()
	{
		bool flag = Scene.Player.Inventory.GetCurrentSlot() == PlayerData.Slots.sniper;
		bool flag2 = _weaponBlinker.gameObject.active;
		bool flag3 = Singleton.UI.Hud.IsZoomMode();
		_switchBlinker.Play(!flag2 && !flag);
		if (flag3)
		{
			Singleton.Tutorial.Arrow.SetTarget(Singleton.UI.Hud._fireButton.GetComponent<UIButton>(), 255f, 10f);
		}
		else if (flag)
		{
			if ((bool)Scene.Player.Inventory.Weapon && Scene.Player.Inventory.Weapon.ScopeModel != null)
			{
				Singleton.Tutorial.Arrow.SetTarget(Singleton.UI.Hud._zoomModeButton.GetComponent<UIButton>(), 255f, 10f);
			}
			else
			{
				Singleton.Tutorial.Arrow.Hide();
			}
		}
		else if (flag2)
		{
			Singleton.Tutorial.Arrow.SetTarget(_weaponBlinker.transform.position, 45f, 10f);
		}
		else
		{
			Singleton.Tutorial.Arrow.SetTarget(_switchBlinker.GetComponent<UIButton>(), 225f, 5f);
		}
	}

	public void OnEnemyKill(Character s, Unit u, Weapon w, Consumable c)
	{
		if (!this)
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
			return;
		}
		if (w != null)
		{
			WeaponShopItem weaponShopItemByItem = Singleton.ShopData.GetWeaponShopItemByItem(w.name);
			if (weaponShopItemByItem != null && weaponShopItemByItem.Slot == PlayerData.Slots.sniper)
			{
				Finish();
			}
		}
	}

	protected override void Finish()
	{
		Singleton.Tutorial.Arrow.Hide();
		_switchBlinker.Finish();
		_weaponBlinker.Finish();
		Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
		Singleton.Tutorial.Arrow.Hide();
		UnityEngine.Object.Destroy(this);
	}
}

public class TryTapjoy : TutorialScript
{
	private bool _pressed = true;

	private bool _seen;

	private void Start()
	{
		UIButton getFreeGoldButton = Singleton.UI.MapWindow.GetComponentInChildren<CommonGUI>().GetFreeGoldButton;
		Singleton.UI.MapWindow.ShowTip(Strings.GetString("Get free gold to buy weapons and consumables."));
		Singleton.Tutorial.Arrow.SetTarget(getFreeGoldButton, 225f, 15f);
		GUIUtils.HookButton(getFreeGoldButton, () =>
		{
			_pressed = true;
			Finish();
		});
	}

	private void Update()
	{
		LevelUpPopup levelUpPopup = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is LevelUpPopup) as LevelUpPopup;
		if ((bool)levelUpPopup)
		{
			_seen = false;
			return;
		}
		RateMePopup rateMePopup = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is RateMePopup) as RateMePopup;
		if ((bool)rateMePopup)
		{
			_seen = false;
			return;
		}
		if (StoreScreen.ShopIsOpen && _seen)
		{
			_pressed = false;
			Finish();
			return;
		}
		MissionInfoPopup missionInfoPopup = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is MissionInfoPopup) as MissionInfoPopup;
		if ((bool)missionInfoPopup && _seen)
		{
			_pressed = false;
			Finish();
			return;
		}
		PauseWindow pauseWindow = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is PauseWindow) as PauseWindow;
		if ((bool)pauseWindow && _seen)
		{
			_pressed = false;
			Finish();
			return;
		}
		AchievementsWindow achievementsWindow = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is AchievementsWindow) as AchievementsWindow;
		if ((bool)achievementsWindow && _seen)
		{
			_pressed = false;
			Finish();
			return;
		}
		_seen = true;
	}

	protected override void Finish()
	{
		if (this == null)
		{
			return;
		}
		Singleton.Tutorial.Arrow.Hide();
		Singleton.UI.MapWindow.HideTip();
		Singleton.UI.MapWindow.BroadcastMessage("ShowGetFreeTutorialIcon", false);
		UnityEngine.Object.Destroy(this);
	}
}
