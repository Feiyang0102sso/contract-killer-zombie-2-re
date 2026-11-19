using System;
using System.Collections.Generic;
using Glu.MobileAppTracking;
using UnityEngine;

public sealed class TutorialLogger : LoggerSingleton<TutorialLogger>
{
	public TutorialLogger()
	{
		LoggerSingleton<TutorialLogger>.SetLoggerName("Other.Tutorial");
	}
}

public class Tutorial : MonoBehaviour
{
	public enum AnalyticEvents
	{
		First_Mission_Started,
		Pressed_Right_Part_To_Aim,
		Killed_First_Zombies,
		Helicopter_Explosion,
		Healing_Button_Tapped,
		Pressed_Left_Part_To_Move,
		Second_Zombies_Appeared,
		Killed_Second_Zombies,
		Switched_To_The_Bat,
		Switched_Back_To_Firearms_1,
		First_Mission_Completed,
		City_Map_Loaded,
		Second_Mission_Popup_Opened,
		Second_Mission_Started,
		Pressed_Left_Part_To_Move_2nd_Mission,
		Pressed_Right_Part_To_Aim_2nd_Mission,
		Ammo_Crate_Collected,
		Bonus_Crate_Collected,
		Zombies_Horde_Appeared,
		Beacon_Grenade_Thrown,
		Second_Mission_Completed,
		Tutorial_Completed,
		Shotgun_Screen_Loaded,
		Buy_Shotgun_1,
		Buy_Shotgun_2,
		Buy_Shotgun_3,
		Next_Mission_Started
	}

	public class TutorialState
	{
		protected Tutorial tutorial
		{
			get
			{
				return Singleton.Tutorial;
			}
		}

		public virtual void OnStart()
		{
		}

		public virtual void OnFinish()
		{
		}

		public virtual GameData.MissionData AutoStartMission()
		{
			return null;
		}

		public virtual void ShowCityMapPopup()
		{
		}
	}

	private class RunMission : TutorialState
	{
		public override void OnStart()
		{
			Singleton.Loader.OnSceneLoaded += new Action(OnSceneLoaded);
		}

		private void OnSceneLoaded()
		{
			if (!(!Loader.IsGameScene()))
			{
				tutorial.gameObject.AddComponent<HowToAimTutorial>();
				Scene.Mission.OnCompleted += (Action<Mission>)delegate
				{
					if ((bool)tutorial)
					{
						tutorial.NextState();
					}
				};
			}
		}

		public override void OnFinish()
		{
			Singleton.Loader.OnSceneLoaded -= new Action(OnSceneLoaded);
		}

		public override GameData.MissionData AutoStartMission()
		{
			return Array.Find(Singleton.GameData.Missions, (GameData.MissionData m) => m.Type == Mission.Type.Story1);
		}
	}

	private class Shop : TutorialState
	{
		private CommonShopItem _grenade;

		public override void OnStart()
		{
			Singleton.Loader.OnSceneLoaded += new Action(OnSceneLoaded);
			_grenade = Singleton.ShopData.GetShopItem("MedkitLargeX3") as CommonShopItem;
			_grenade.OnStateChange += new Action(OnStateChange);
		}

		private void OnStateChange()
		{
			if (Singleton.PlayerData.GetConsumableCount(_grenade.item) > 0)
			{
				tutorial.NextState();
			}
		}

		private void OnSceneLoaded()
		{
			tutorial.NextState();
		}

		public override void OnFinish()
		{
			Singleton.Loader.OnSceneLoaded -= new Action(OnSceneLoaded);
			_grenade.OnStateChange -= new Action(OnStateChange);
		}
	}

	private class SecondMission : TutorialState
	{
		public override void OnStart()
		{
			Singleton.Loader.OnSceneLoaded += new Action(OnSceneLoaded);
			OnSceneLoaded();
		}

		private void OnSceneLoaded()
		{
			if (Loader.IsCityMapScene() && Loader.State == Loader.LoaderState.Ready && tutorial.gameObject.GetComponent<ChoiceMissionIcon>() == null)
			{
				tutorial.gameObject.AddComponent<ChoiceMissionIcon>();
			}
			else
			{
				if (!(!Loader.IsGameScene() || !Scene.Mission))
				{
					tutorial.gameObject.AddComponent<HowToMove2Tutorial>();
					Scene.Mission.OnCompleted += (Action<Mission>)delegate
					{
						tutorial.NextState();
					};
					Scene.Mission.OnFailed += (Action<Mission>)delegate
					{
						foreach (TutorialScript obj in tutorial.GetComponents<TutorialScript>())
						{
							UnityEngine.Object.Destroy(obj);
						}
					};
				}
			}
		}

		public override void OnFinish()
		{
			Singleton.Loader.OnSceneLoaded -= new Action(OnSceneLoaded);
		}
	}

	public class SurvivalModeIntroduction : TutorialState
	{
		private bool _playerWasInSurvivalMode;

		private bool _playerWasOnCityMap;

		public override void OnStart()
		{
			Singleton.Loader.OnSceneLoaded += new Action(OnSceneLoaded);
			OnSceneLoaded();
		}

		private void OnSceneLoaded()
		{
			if (Loader.IsCityMapScene() && Loader.State == Loader.LoaderState.Ready && tutorial.gameObject.GetComponent<RunSurvivalMode>() == null)
			{
				if (_playerWasInSurvivalMode)
				{
					tutorial.NextState();
				}
				else
				{
					tutorial.gameObject.AddComponent<RunSurvivalMode>();
					_playerWasOnCityMap = true;
				}
			}
			else if (Loader.IsGameScene() && (bool)Scene.Mission)
			{
				if (_playerWasOnCityMap)
				{
					GameAnalytics.LogTutorialStepCompleted(AnalyticEvents.Next_Mission_Started);
				}
				if (Scene.Mission.IsSurvivalGates())
				{
					tutorial.gameObject.AddComponent<IAmASniperTutorial>();
					_playerWasInSurvivalMode = true;
				}
			}
		}

		public override void OnFinish()
		{
			Singleton.Loader.OnSceneLoaded -= new Action(OnSceneLoaded);
		}

		public override void ShowCityMapPopup()
		{
			if (!(!HasPlayerAnyWeaponFromGroup("shotgun")))
			{
				return;
			}
			Singleton.UI.PopupManager.ShowTutorialShotgunSellPopup(null);
		}
	}

	private class TapjoyDemo : TutorialState
	{
		private bool _arrowShown;

		public override void OnStart()
		{
			Singleton.Loader.OnSceneLoaded += new Action(OnSceneLoaded);
			OnSceneLoaded();
		}

		private void OnSceneLoaded()
		{
			if (_arrowShown)
			{
				tutorial.NextState();
				return;
			}
			bool readyForDemo = Singleton.PlayerData.Level >= 4 && PlayerData.Instance.LevelUpState == 0;
			if (readyForDemo && Loader.IsCityMapScene() && Loader.State == Loader.LoaderState.Ready)
			{
				tutorial.gameObject.AddComponent<TryTapjoy>();
				_arrowShown = true;
			}
		}

		public override void OnFinish()
		{
			Singleton.Loader.OnSceneLoaded -= new Action(OnSceneLoaded);
		}
	}

	private class Completed : TutorialState
	{
		public override void OnStart()
		{
			UnityEngine.Object.Destroy(tutorial);
		}
	}

	public const string ItemNameToBuy1 = "MedkitLargeX3";

	public const string ItemNameToHeal = "MedkitLarge";

	public const string ShopGroupName = "consumables";

	public const string ItemNameToBuy2 = "Shotgun1";

	public const string ItemNameToBuy2Better = "Shotgun3";

	public const string ItemNameToBuy2Best = "Shotgun5";

	public const string ItemNameToBuy3 = "SniperRifle1";

	public const string ItemNameToBuy3Better = "SniperRifle2";

	public const string ItemNameToBuy3Best = "SniperRifle6";

	private List<TutorialState> _states = new List<TutorialState>(16);

	private TutorialState _currentState;

	private TutorialTip _tip;

	private TutorialArrow _arrow;

	private void Awake()
	{
		_states.Add(new RunMission());
		_states.Add(new Shop());
		_states.Add(new SecondMission());
		_states.Add(new SurvivalModeIntroduction());
		_states.Add(new TapjoyDemo());
		_states.Add(new Completed());
	}

	public bool TutorialStateIs<T>() where T : TutorialState
	{
		if (!Singleton.PlayerData.TutorialState.InSegment(0, _states.Count - 1))
		{
			return false;
		}
		return _states[Singleton.PlayerData.TutorialState] is T;
	}

	public void Init()
	{
		Init(-1);
	}

	public void Init(int forceState)
	{
		int state = Singleton.PlayerData.TutorialState;
		if (forceState >= 0)
		{
			state = forceState;
		}
		SetState(state);
	}

	private void SetState(int state)
	{
		if (_currentState != null)
		{
			_currentState.OnFinish();
		}
		foreach (TutorialScript obj in GetComponents<TutorialScript>())
		{
			UnityEngine.Object.Destroy(obj);
		}
		state = Mathf.Clamp(state, 0, _states.Count - 1);
		TutorialState currentState = _states[state];
		_currentState = currentState;
		Singleton.PlayerData.TutorialState = state;
		_currentState.OnStart();
	}

	public void NextState()
	{
		SetState(Singleton.PlayerData.TutorialState + 1);
		switch (Singleton.PlayerData.TutorialState)
		{
		case 1:
			GameAnalytics.LogTutorialStepCompleted(AnalyticEvents.First_Mission_Completed);
			break;
		case 2:
			GameAnalytics.LogTutorialStepCompleted(AnalyticEvents.City_Map_Loaded);
			break;
		case 3:
			GameAnalytics.LogTutorialStepCompleted(AnalyticEvents.Second_Mission_Completed);
			break;
		}
		if (TutorialStateIs<SurvivalModeIntroduction>())
		{
			MobileAppTracking.TrackAction("tutorial_complete");
		}
	}

	public void Finish()
	{
		SetState(_states.Count - 1);
	}

	public TutorialState State
	{
		get
		{
			return _currentState;
		}
	}

	public static bool IsRunning()
	{
		return (bool)Singleton.Tutorial && Singleton.Tutorial.State != null;
	}

	private void OnDestroy()
	{
		if (_arrow != null)
		{
			UnityEngine.Object.Destroy(_arrow.gameObject);
		}
	}

	public TutorialTip Tip
	{
		get
		{
			if (_tip == null)
			{
				_tip = Util.CreateGameObject("TutorialTip").GetComponent<TutorialTip>();
			}
			return _tip;
		}
	}

	public TutorialArrow Arrow
	{
		get
		{
			if (_arrow == null)
			{
				_arrow = Util.CreateGameObject("TutorialArrow").GetComponent<TutorialArrow>();
			}
			return _arrow;
		}
	}

	public static bool HasPlayerAnyWeaponFromGroup(string groupName)
	{
		foreach (ShopConfigGroup.Reference itemRef in Singleton.ShopData.shopConfig.FindGroup(groupName).itemRefs)
		{
			ShopConfigItem shopItem = Singleton.ShopData.GetShopItem(itemRef.itemId);
			if (shopItem is WeaponShopItem && (shopItem as WeaponShopItem).IsAvailableForEquipment())
			{
				return true;
			}
		}
		return false;
	}
}
