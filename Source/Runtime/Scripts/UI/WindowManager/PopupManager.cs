using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
	private Queue<UIWindow> _queue = new Queue<UIWindow>();

	private UIWindow _currentPopup;

	public UIWindow ShowPopup(PopupInfo info)
	{
		return ShowPopup(info, false);
	}

	public UIWindow ShowPopup(PopupInfo info, bool withPause)
	{
		return ShowPopup(info, withPause, true);
	}

	public UIWindow ShowPopup(PopupInfo info, bool withPause, bool queued)
	{
		GameObject gameObject = Util.CreateGameObject("InfoPopup");
		PopupControl component = gameObject.GetComponent<PopupControl>();
		if (withPause)
		{
			component.WithPause();
		}
		component.WithFade();
		component.SetInfo(info);
		if (queued)
		{
			AddPopup(component);
		}
		else
		{
			component.Show();
		}
		return component;
	}

	public static void ShowPopupNow(PopupInfo info)
	{
		GameObject gameObject = Util.CreateGameObject("InfoPopup");
		PopupControl component = gameObject.GetComponent<PopupControl>();
		component.SetInfo(info);
		component.WithFade();
		component.Show();
	}

	public void ShowMissionLockedPopup(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		MissionLockedPopup component = Util.CreateGameObject("MissionLockedPopup").GetComponent<MissionLockedPopup>();
		component.Init(mapNarrative, mapLocation);
		component.WithFade();
		component.Show();
	}

	public void ShowInfectionLevelPopup(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		InfectionLevelPopup component = Util.CreateGameObject("InfectionLevelPopup").GetComponent<InfectionLevelPopup>();
		component.Init(mapNarrative, mapLocation);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowMissionInfoPopup(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		Mission.Type type = mapNarrative.SelectedMission.Type;
		string resourceName = "UsualMissionInfoPopup";
		if (Mission.IsSurvivalGates(type))
		{
			resourceName = "SurvivalGatesInfoPopup";
		}
		else if (Mission.IsSurvivalArena(type))
		{
			resourceName = "SurvivalArenaInfoPopup";
		}
		else if (Mission.IsUsualMission(type) || Mission.IsStorylineMission(type))
		{
			resourceName = "UsualMissionInfoPopup";
		}
		else
		{
		}
		MissionInfoPopup component = Util.CreateGameObject(resourceName).GetComponent<MissionInfoPopup>();
		component.Init(mapNarrative, mapLocation);
		component.WithFade();
		component.Show();
	}

	public void ShowSpecialHuntPopup(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		SpecialHuntPopup component = Util.CreateGameObject("SpecialHuntPopup").GetComponent<SpecialHuntPopup>();
		component.Init(mapNarrative, mapLocation);
		component.WithFade();
		component.Show();
	}

	public void ShowTutorialShotgunSellPopup(MapLocation location)
	{
		GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Shotgun_Screen_Loaded);
		WeaponSellPopup component = Util.CreateGameObject("WeaponSellPopup").GetComponent<WeaponSellPopup>();
		component.Init(location, Strings.GetString("GET A SHOTGUN"), Strings.GetString("Take this SHOTGUN.\nYou're going to need it to take out more zombie. If you need a better one, I'm sure we can make a deal."), "Shotgun1", "Shotgun3", "Shotgun5", false);
		component.SimulateSale(100, 0, 0);
		AddPopup(component);
	}

	public void ShowTutorialSniperRifleSellPopup(MapLocation location)
	{
		WeaponSellPopup component = Util.CreateGameObject("WeaponSellPopup").GetComponent<WeaponSellPopup>();
		component.Init(location, Strings.GetString("BUY A SNIPER RIFLE"), Strings.GetString("Get a SNIPER RIFLE.\nYou're going to need it to snipe more zombie. If you need a better one, I'm sure we can make a deal."), "SniperRifle1", "SniperRifle2", "SniperRifle6", true);
		component.Show();
	}

	public void ShowMissionCompletePopup(BasicMissionReward reward)
	{
		MissionCompletePopup component = Util.CreateGameObject("MissionCompletePopup").GetComponent<MissionCompletePopup>();
		component.Init(reward);
		component.WithPause();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowMissionFailPopup(GameEvents.MissionFailure failure, BasicMissionReward reward)
	{
		MissionFailPopup component = Util.CreateGameObject("MissionFailPopup").GetComponent<MissionFailPopup>();
		component.Init(failure, reward);
		component.WithPause();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowSurvivalModeFailPopup(SurvivalGatesMissionReward reward)
	{
		SurvivalModeFailPopup component = Util.CreateGameObject("SurvivalModeFailPopup").GetComponent<SurvivalModeFailPopup>();
		component.Init(reward);
		component.WithPause();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowSurvivalModeBetweenPopup(SurvivalGatesMissionReward reward)
	{
		SurvivalModeBetweenPopup component = Util.CreateGameObject("SurvivalModeBetweenPopup").GetComponent<SurvivalModeBetweenPopup>();
		component.Init(reward);
		component.WithPause();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowArenaModeFinishPopup(ArenaMissionReward reward)
	{
		ArenaModeFinishPopup component = Util.CreateGameObject("ArenaModeFinishPopup").GetComponent<ArenaModeFinishPopup>();
		component.Init(reward);
		component.WithPause();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowArenaRevivePopup(ArenaMissionReward reward)
	{
		ArenaRevivePopup component = Util.CreateGameObject("ArenaRevivePopup").GetComponent<ArenaRevivePopup>();
		component.Init(reward);
		component.WithPause();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowLevelUpPopup()
	{
		LevelUpPopup component = Util.CreateGameObject("LevelUpPopup").GetComponent<LevelUpPopup>();
		AddPopup(component, false);
	}

	public void ShowRateMePopup()
	{
		RateMePopup component = Util.CreateGameObject("RateMePopup").GetComponent<RateMePopup>();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowGoalAnimationPopup(CommonGUI commonGUI)
	{
		GameObject gameObject = new GameObject("GoalAnimation");
		gameObject.layer = Const.LayerGui;
		GoalAnimationPopup goalAnimationPopup = gameObject.AddComponent<GoalAnimationPopup>();
		goalAnimationPopup.Init(commonGUI);
		AddPopup(goalAnimationPopup);
	}

	public void ShowDailyBonusPopup(int day)
	{
		DailyBonusPopup component = Util.CreateGameObject("DailyBonusPopup").GetComponent<DailyBonusPopup>();
		component.Init(day);
		AddPopup(component);
	}

	public void ShowProposalAfterDeathPopup(string itemName, int discountPercents)
	{
		ProposalAfterDeathPopup component = Util.CreateGameObject("ProposalAfterDeathPopup").GetComponent<ProposalAfterDeathPopup>();
		component.Init(itemName, discountPercents);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowProposalAfterDeathPopup()
	{
		ProposalAfterDeathPopup component = Util.CreateGameObject("ProposalAfterDeathPopup").GetComponent<ProposalAfterDeathPopup>();
		component.Init();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowLimitedTimeOfferWelcomePopup()
	{
		LimitedTimeOfferPopup component = Util.CreateGameObject("LimitedTimeOfferPopup").GetComponent<LimitedTimeOfferPopup>();
		int price = 0;
		BundleShopItem rewardBundle = Singleton.ShopData.LimitedTimeOffer.GetRewardBundle(out price);
		component.Init(true, rewardBundle, price);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowLimitedTimeOfferRewardPopup(BundleShopItem item)
	{
		LimitedTimeOfferPopup component = Util.CreateGameObject("LimitedTimeOfferPopup").GetComponent<LimitedTimeOfferPopup>();
		component.Init(false, item, 0);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowCkz1BannerPopup()
	{
		Ckz1BannerPopup component = Util.CreateGameObject("Ckz1BannerPopup").GetComponent<Ckz1BannerPopup>();
		component.WithFade();
		AddPopup(component);
	}

	public void ShowICloudPopup()
	{
		ICloud iCloud = new ICloud();
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("ICloud");
		GameAnalytics.LogICloudButtonPressed();
		if (!iCloud.IsSupported)
		{
			popupInfo.Title = Strings.GetString("Warning");
			popupInfo.Message = Strings.GetString("iCloud is not supported on this device.");
			popupInfo.AddButton(Strings.GetString("Ok"), () =>
			{
			});
		}
		else if (!iCloud.IsAvailable)
		{
			popupInfo.Title = Strings.GetString("Warning");
			popupInfo.Message = Strings.GetString("Please, log in to iCloud in the device settings.");
			popupInfo.AddButton(Strings.GetString("Ok"), () =>
			{
			});
		}
		else
		{
			popupInfo.CloseButtonEnabled = true;
			popupInfo.Message = Strings.GetString("Would you like to save or load you game to/from your iCloud account?");
			popupInfo.AddButton(Strings.GetString("Save"), () =>
			{
				GameAnalytics.LogICloudSaveButtonPressed();
				PopupInfo popupInfo2 = new PopupInfo();
				popupInfo2.Title = Strings.GetString("Save");
				popupInfo2.Message = Strings.GetString("Are you sure you want to save game to iCloud server?");
				popupInfo2.AddButton(Strings.GetString("Yes"), () =>
				{
					UIWindow waitPopup = ShowUnclosablePopup(Strings.GetString("Please Wait"), Strings.GetString("Your save file has been queued to be uploaded to the iCloud Service."));
					if (App.Instance.GetNetworkReachability() == NetworkReachability.NotReachable)
					{
						ShowAssetBundlesFailDialog();
					}
					else
					{
						iCloud.Save(Const.SaveFileName, (string fileName, bool success) =>
						{
							waitPopup.Close();
							if (success)
							{
								ShowInfoPopup(Strings.GetString("Save Complete"), Strings.GetString("Save complete."));
							}
							else
							{
								ShowInfoPopup(Strings.GetString("Error"), Strings.GetString("Unable to store save file in the iCloud Service."));
							}
						});
					}
				});
				popupInfo2.AddButton(Strings.GetString("No"), null);
				ShowPopup(popupInfo2);
			}, true);
			popupInfo.AddButton(Strings.GetString("Load"), () =>
			{
				GameAnalytics.LogICloudLoadButtonPressed();
				PopupInfo popupInfo2 = new PopupInfo();
				popupInfo2.Title = Strings.GetString("Save");
				popupInfo2.Message = Strings.GetString("Are you sure you want to load game form iCloud server?");
				popupInfo2.AddButton(Strings.GetString("Yes"), () =>
				{
					if (App.Instance.GetNetworkReachability() == NetworkReachability.NotReachable)
					{
						ShowAssetBundlesFailDialog();
					}
					else
					{
						UIWindow waitPopup = ShowUnclosablePopup(Strings.GetString("Please Wait"), Strings.GetString("Your save file has been queued to be loaded from the iCloud Service."));
						iCloud.Load(Const.SaveFileName, (string fileName, byte[] data) =>
						{
							waitPopup.Close();
							if (data != null)
							{
								StorageManager.Write(Const.SaveFileName, data);
								ShowInfoPopup(Strings.GetString("Load Complete"), Strings.GetString("Your save data has been successfully loaded from the iCloud Service."), () =>
								{
									Application.LoadLevel(0);
								});
							}
							else
							{
								ShowInfoPopup(Strings.GetString("Error"), Strings.GetString("Unable to find save file on the iCloud Service."));
							}
						});
					}
				});
				popupInfo2.AddButton(Strings.GetString("No"), null);
				ShowPopup(popupInfo2);
			}, true);
		}
		ShowPopup(popupInfo).WithFade();
	}

	public void ShowMapWindowAnimationController()
	{
		MapWindowAnimationController popup = new GameObject("MapWindowAnimationController").AddComponent<MapWindowAnimationController>();
		AddPopup(popup);
	}

	public void ShowPlayhavenPopup()
	{
		PlayhavenPopup popup = new GameObject("PlayhavenPopup").AddComponent<PlayhavenPopup>();
		AddPopup(popup);
	}

	public void ShowNewItemAvailablePopup(CommonShopItem item)
	{
		NewItemAvailablePopup component = Util.CreateGameObject("NewItemAvailablePopup").GetComponent<NewItemAvailablePopup>();
		component.Init(item);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowRestartForUpdateAppDialog()
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Info");
		popupInfo.Message = Strings.GetString("New updates are ready, the game will be restarted.");
		popupInfo.AddButton(Strings.GetString("Ok"), () =>
		{
			Application.LoadLevel(0);
		});
		ShowPopup(popupInfo);
	}

	public static void ShowForcedUpdateAppDialog()
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Please Update");
		popupInfo.Message = Strings.GetString("The version you currently have installed is not up to date! Please install the latest version of the game.");
		popupInfo.AddButton(Strings.GetString("Ok"), () =>
		{
			Application.OpenURL(ForcedUpdate.GetUpdateURL());
		}, false);
		ShowPopupNow(popupInfo);
	}

	public static void ShowRecommendUpdateAppDialog()
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Please Update");
		popupInfo.Message = Strings.GetString("The version you currently have installed is not up to date! Please install the latest version of the game.");
		popupInfo.AddButton(Strings.GetString("Ok"), () =>
		{
			Application.OpenURL(ForcedUpdate.GetUpdateURL());
		}, false);
		popupInfo.AddButton(Strings.GetString("Cancel"), () =>
		{
			Boot.ForcedUpdatePause = false;
		});
		ShowPopupNow(popupInfo);
	}

	public void ShowAssetBundlesFailDialog()
	{
		ShowInfoPopup(Strings.GetString("Unable to connect to the server. 3G, Edge or Wi-Fi internet connection is required."));
	}

	public void ShowIAPFailedPopup()
	{
		ShowInfoPopup(Strings.GetString("Your purchase failed. Please try again later."));
	}

	public void ShowIAPCanceledPopup()
	{
		ShowInfoPopup(Strings.GetString("Your purchase cancelled."));
	}

	public void ShowIAPNoNetworkPopup()
	{
		ShowInfoPopup(Strings.GetString("Unable to connect to store. Please check your connection and try again."));
	}

	public void ShowIAPBoughtPopup(IAPShopItem iap)
	{
		ShowInfoPopup(string.Format(Strings.GetString("You successfully purchased the following product: {0}."), GUIUtils.GetPriceStringForCurrency(iap.Amount, iap.currency)));
	}

	public void ShowFacebookLoginForRewardPopup()
	{
		GameObject gameObject = Util.CreateGameObject("FacebookRewardPopup");
		PopupControl component = gameObject.GetComponent<PopupControl>();
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Facebook");
		popupInfo.Message = Strings.GetString("Login to your Facebook account and receive free reward.");
		popupInfo.AddButton(Strings.GetString("Continue"), FacebookManager.Login);
		popupInfo.CloseButtonEnabled = true;
		component.SetInfo(popupInfo);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowFacebookRewardGivenPopup()
	{
		GameObject gameObject = Util.CreateGameObject("FacebookRewardPopup");
		PopupControl component = gameObject.GetComponent<PopupControl>();
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Congratulations");
		popupInfo.Message = Strings.GetString("You received your reward for first login to Facebook!");
		popupInfo.AddButton(Strings.GetString("Ok"), null);
		component.SetInfo(popupInfo);
		component.WithFade();
		AddPopup(component);
	}

	public void ShowQuitConfirmationPopup()
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Quit?");
		popupInfo.Message = Strings.GetString("Are you sure you want to quit mission? (All rewards will be lost).");
		popupInfo.AddButton(Strings.GetString("Yes"), () =>
		{
			Scene.Mission.ExitMission();
		});
		popupInfo.AddButton(Strings.GetString("No"), () =>
		{
			Singleton.UI.ShowPauseWindow();
		});
		ShowPopup(popupInfo);
	}

	/// <summary>PC port: asked by the city map exit button. Same strings as the original ShopOrExit popup.</summary>
	public void ShowExitGameConfirmationPopup()
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Quit?");
		popupInfo.Message = Strings.GetString("Wanna quit?");
		popupInfo.AddButton(Strings.GetString("Quit"), () =>
		{
			Debug.Log("Exit button: quitting the game");
			Application.Quit();
		});
		popupInfo.AddButton(Strings.GetString("Continue"), () =>
		{
		});
		ShowPopup(popupInfo);
	}

	public UIWindow ShowInfoPopup(string message)
	{
		return ShowInfoPopup(Strings.GetString("Info"), message);
	}

	public UIWindow ShowInfoPopup(string title, string message)
	{
		return ShowInfoPopup(title, message, null);
	}

	public UIWindow ShowInfoPopup(string title, string message, Action okAction)
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = title;
		popupInfo.Message = message;
		popupInfo.AddButton(Strings.GetString("Ok"), okAction);
		return ShowPopup(popupInfo, true, false);
	}

	public UIWindow ShowUnclosablePopup(string title, string message)
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = title;
		popupInfo.Message = message;
		return ShowPopup(popupInfo, true, false);
	}

	public void Clear()
	{
		_currentPopup = null;
		_queue.Clear();
	}

	public bool IsQueued<UIWindowType>()
	{
		if ((bool)_currentPopup && _currentPopup is UIWindowType)
		{
			return true;
		}
		foreach (UIWindow item in _queue)
		{
			if (item is UIWindowType)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearQueue()
	{
		_queue.Clear();
	}

	public void AddPopup(UIWindow popup)
	{
		AddPopup(popup, true);
	}

	private void AddPopup(UIWindow popup, bool withAnimation)
	{
		popup.OnDestroyDelegate += new Action(ShowNextPopup);
		if (withAnimation)
		{
			popup.OnShowDelegate += (Action)(() =>
			{
				Array.ForEach(popup.gameObject.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
				{
					c.RunAnimation();
				});
			});
		}
		if (_queue.Count == 0 && _currentPopup == null)
		{
			ShowQueuedPopup(popup);
		}
		else
		{
			popup.Hide();
			_queue.Enqueue(popup);
		}
	}

	private void ShowNextPopup()
	{
		_currentPopup = null;
		if (_queue.Count > 0)
		{
			ShowQueuedPopup(_queue.Dequeue());
		}
	}

	private void ShowQueuedPopup(UIWindow popup)
	{
		_currentPopup = popup;
		if (Loader.IsGameScene())
		{
			_currentPopup.WithPause();
		}
		_currentPopup.Show();
	}
}
