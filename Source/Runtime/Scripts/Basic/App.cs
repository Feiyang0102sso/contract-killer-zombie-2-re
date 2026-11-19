using System;
using System.Collections.Generic;
using Glu;
using Glu.Kontagent;
using Glu.Localization;
using Glu.MobileAppTracking;
using UnityEngine;

public class App : MonoBehaviour
{
	private static App _instance = null;

	private bool _suspended;

	private float _appStartTime;

	private float _gameStartTime;

	private bool _flushCachedAnalytics;

	private CountdownTimer _flushTimer;

	private static bool _playHavenGameLaunch;

	public static bool FirstGameLaunch;

	public static bool NoNetworkWarningWasShown;

	private bool _tapjoyInterfaceOpened;

	private bool _isIAPBuyInProcess;

	private bool _facebookLoginCompleted;

	private bool _waitDeviceToken;

	private NetworkReachability _cachedNetworkReachability;

	private int _updatesTillNetworkReachabilityUpdate;

	public static bool Exists()
	{
		return _instance != null;
	}

	public static App Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("App");
				_instance = gameObject.AddComponent<App>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return _instance;
		}
	}

	public void Create()
	{
	}

	public NetworkReachability GetNetworkReachability()
	{
		if (_updatesTillNetworkReachabilityUpdate <= 0)
		{
			_cachedNetworkReachability = Application.internetReachability;
			_updatesTillNetworkReachabilityUpdate = 3;
		}
		return _cachedNetworkReachability;
	}

	private void Awake()
	{
		_appStartTime = Time.time;
		_gameStartTime = Time.time;
		CFlurry.StartSession("ZA1BTLN37AZJ25USG9YC");
		MobileAppTracking.Init("2376", "533273abed4546b082352289881f70d4");
		Kontagent.StartSession("7a6da10d46fd4f469fa488bbf1c51a06");
		if (Integrity.IsJailbroken())
		{
			GameAnalytics.LogJailbrokenAppStart();
		}
		else
		{
			GameAnalytics.LogAppStart();
		}
		GameAnalytics.LogJailbroken(Integrity.IsJailbroken() ? GameAnalytics.IntegrityType.Jailbroken : GameAnalytics.IntegrityType.Genuine);
		GameAnalytics.LogGameStart();
		if (QualitySetter.PlayHavenEnabled)
		{
			PlayHavenInterface.Init("a63d65d508d24eccbe9706766d6ee594", "aa8d8a9604454629a3a3c74183781e30");
			PlayHavenInterface.OnPurchaseInitiated += (string productId) =>
			{
				if (Singleton.Exists())
				{
					Debug.Log(productId);
					IAPShopItem iAPShopItemByProductId = Singleton.ShopData.GetIAPShopItemByProductId(productId);
					if (iAPShopItemByProductId != null && iAPShopItemByProductId.state == IAPShopConfigItem.State.AvailableForPurchase)
					{
						iAPShopItemByProductId.Buy(true);
					}
				}
			};
		}
		Tapjoy.Initialize("c631fb4d-a728-4f34-ac42-42f5cb6b9e80", "MCw1DV6xmrO81ah15AAl", QualitySetter.TapjoyVideoEnabled);
		Tapjoy.onOfferwallStateChanged += (Tapjoy.OfferwallState state) =>
		{
			if (state == Tapjoy.OfferwallState.Failed)
			{
				Singleton.UI.PopupManager.ShowAssetBundlesFailDialog();
			}
		};
		PushNotifications.GetInstance().RegisterForRemoteNotificationTypes(7, "KqYZlpA3QnGPhuAZh-gMqg", "2WpCQ-X1R_y_ozFwXIxVXg", null, null, null);
		_waitDeviceToken = true;
		Social.localUser.Authenticate((bool success) =>
		{
		});
	}

	private void OnSocialAuthentication(bool success)
	{
		Debug.Log("Social authentication: " + success);
	}

	private void OnApplicationPause()
	{
		_suspended = !_suspended;
		if (_suspended)
		{
			GameAnalytics.LogGameStop(PlayerData.Session.Level, Time.time - _gameStartTime, PlayerData.Session.MissionsCompleted);
			OnSuspend();
		}
		else
		{
			_gameStartTime = Time.time;
			GameAnalytics.LogGameStart();
			OnRestore();
		}
	}

	private void Update()
	{
		if (_updatesTillNetworkReachabilityUpdate > 0)
		{
			_updatesTillNetworkReachabilityUpdate--;
		}
		if (_flushCachedAnalytics && !_flushTimer.IsRunning())
		{
			GameAnalytics.FlushCachedEvents(1);
			_flushTimer.Run(0.5f);
		}
		if (_tapjoyInterfaceOpened && !Tapjoy.isOfferwallOpen)
		{
			if (QualitySetter.PlayHavenEnabled)
			{
				PlayHavenInterface.StartPublisherContentRequest("tj_closed", false, false);
			}
			_tapjoyInterfaceOpened = false;
			if (Singleton.Exists())
			{
				Singleton.PlayerData.TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.ReturnedFromTJOfferwall);
			}
		}
		if (_waitDeviceToken)
		{
			if (PushNotifications.GetInstance().GetDeviceToken() != null)
			{
				GameAnalytics.LogPushToken(PushNotifications.GetInstance().GetDeviceToken(), PushNotifications.GetInstance().GetEnabledRemoteNotificationTypes() != 0);
				_waitDeviceToken = false;
			}
			else if (PushNotifications.GetInstance().GetRegistrationError() != null)
			{
				_waitDeviceToken = false;
			}
		}
		if (!_facebookLoginCompleted && Facebook.IsLoggedIn())
		{
			_facebookLoginCompleted = true;
			GameAnalytics.LogFacebookLoginCompleted();
		}
	}

	private void SafeSchedule<T>(SortedDictionary<DateTime, T> schedule, DateTime dateTime, T val)
	{
		while (schedule.ContainsKey(dateTime))
		{
			dateTime = dateTime.AddSeconds(1.0);
		}
		schedule.Add(dateTime, val);
	}

	private void ScheduleNewDailyBonusAvailable(SortedDictionary<DateTime, PushNotifications.ILocalNotification> schedule, DateTime fireDate)
	{
		PushNotifications.ILocalNotification localNotification = PushNotifications.GetInstance().CreateLocalNotification();
		localNotification.fireDate = fireDate;
		localNotification.alertBody = Strings.GetString("New daily bonus available! Complete any mission to claim it!");
		localNotification.alertAction = Strings.GetString(string.Empty);
		SafeSchedule(schedule, localNotification.fireDate, localNotification);
		Debug.Log("ScheduleNewDailyBonusAvailable " + fireDate);
	}

	private void ScheduleDailyBonusIsAboutToExpire(SortedDictionary<DateTime, PushNotifications.ILocalNotification> schedule, DateTime fireDate)
	{
		PushNotifications.ILocalNotification localNotification = PushNotifications.GetInstance().CreateLocalNotification();
		localNotification.fireDate = fireDate;
		localNotification.alertBody = Strings.GetString("You daily bonus is about to expire! Return and comoplete any mission to claim it!");
		localNotification.alertAction = Strings.GetString(string.Empty);
		SafeSchedule(schedule, localNotification.fireDate, localNotification);
		Debug.Log("ScheduleDailyBonusIsAboutToExpire " + fireDate);
	}

	private void ScheduleLocalNotifications()
	{
		if (!Singleton.Exists())
		{
			return;
		}
		if (!Singleton.Settings.Notifications)
		{
			return;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		num = DailyBonus.GetTotalConsecutiveDays();
		num2 = DailyBonus.GetUnclaimedConsecutiveDays();
		if (num == 1 && num2 == 1)
		{
			num = 0;
		}
		if (num > 0)
		{
			flag = DailyBonus.GetLastRewardedLocalTime().Date == DateTime.Now.Date;
		}
		Debug.Log("DailyBonus consecutive days: " + num + " aknowledged today: " + flag);
		SortedDictionary<DateTime, PushNotifications.ILocalNotification> sortedDictionary = new SortedDictionary<DateTime, PushNotifications.ILocalNotification>();
		PushNotifications.ILocalNotification localNotification;
		if (Singleton.GameData.InfectionLevel.Enabled && Singleton.GameData.InfectionLevel.InfectionAffectsDifficulty && PlayerData.Instance.IsInfectionLevelCritical())
		{
			float num3 = PlayerData.Instance.MinutesLeftTillInfectionLevel(PlayerData.Instance.InfectionLevel);
			localNotification = PushNotifications.GetInstance().CreateLocalNotification();
			localNotification.fireDate = DateTime.Now.AddSeconds(num3 * 60f);
			localNotification.alertBody = Strings.GetString("Infection level is low again! Return to the game to blast more zombie heads!");
			localNotification.alertAction = Strings.GetString(string.Empty);
			SafeSchedule(sortedDictionary, localNotification.fireDate, localNotification);
		}
		if (flag)
		{
			ScheduleNewDailyBonusAvailable(sortedDictionary, DateTime.Now.Date.AddHours(41.0));
			ScheduleDailyBonusIsAboutToExpire(sortedDictionary, DateTime.Now.Date.AddHours(45.0));
		}
		else if (num > 0)
		{
			if (DateTime.Now < DateTime.Now.Date.AddHours(17.0))
			{
				ScheduleNewDailyBonusAvailable(sortedDictionary, DateTime.Now.Date.AddHours(17.0));
			}
			if (DateTime.Now < DateTime.Now.Date.AddHours(21.0))
			{
				ScheduleDailyBonusIsAboutToExpire(sortedDictionary, DateTime.Now.Date.AddHours(21.0));
			}
			else if (DateTime.Now < DateTime.Now.Date.AddHours(23.0).AddMinutes(45.0))
			{
				TimeSpan timeSpan = DateTime.Now.Date.AddHours(23.0).AddMinutes(59.0) - DateTime.Now;
				int num4 = timeSpan.Hours * 60 + timeSpan.Minutes;
				ScheduleDailyBonusIsAboutToExpire(sortedDictionary, DateTime.Now.AddMinutes(num4 / 2));
			}
		}
		else if (DateTime.Now < DateTime.Now.Date.AddHours(17.0))
		{
			ScheduleNewDailyBonusAvailable(sortedDictionary, DateTime.Now.Date.AddHours(17.0));
		}
		else
		{
			ScheduleNewDailyBonusAvailable(sortedDictionary, DateTime.Now.Date.AddHours(41.0));
		}
		if (Singleton.Exists() && !Singleton.PlayerData.IsSpecialHuntPlayed && DateTime.Now < Singleton.PlayerData.SpecialHuntStartTime.AddHours(Singleton.ShopData.SpecialHunt.availabilityInHours / 2))
		{
			localNotification = PushNotifications.GetInstance().CreateLocalNotification();
			localNotification.fireDate = Singleton.PlayerData.SpecialHuntStartTime.AddHours(Singleton.ShopData.SpecialHunt.availabilityInHours / 2);
			localNotification.alertBody = Strings.GetString("Special hunt is about to expire! Return and test a sample of \"") + Strings.GetString(Singleton.ShopData.GetShopItem(Singleton.ShopData.SpecialHunt.saleWeapon).nameId) + Strings.GetString("\" for FREE!");
			localNotification.alertAction = Strings.GetString(string.Empty);
			SafeSchedule(sortedDictionary, localNotification.fireDate, localNotification);
		}
		int num5 = 1;
		foreach (KeyValuePair<DateTime, PushNotifications.ILocalNotification> item in sortedDictionary)
		{
			localNotification = item.Value;
			localNotification.applicationIconBadgeNumber = num5;
			PushNotifications.GetInstance().ScheduleLocalNotification(localNotification);
			num5++;
		}
	}

	private void OnSuspend()
	{
		Debug.Log("App::OnSuspend");
		ScheduleLocalNotifications();
		if (Singleton.Exists() && !Scene.Exists())
		{
			_isIAPBuyInProcess = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is IAPBuyWindow) != null;
		}
		else
		{
			_isIAPBuyInProcess = false;
		}
	}

	private void OnRestore()
	{
		Debug.Log("App::Restore");
		if (Singleton.Exists() && Loader.IsGameScene() && !TimeControl.IsPaused())
		{
			Singleton.UI.ShowPauseWindow();
		}
		NoNetworkWarningWasShown = false;
		if (!_isIAPBuyInProcess && !_tapjoyInterfaceOpened)
		{
			_playHavenGameLaunch = false;
		}
		if (Singleton.Exists() && !Scene.Exists() && !Singleton.UI.PopupManager.IsQueued<PlayhavenPopup>() && !Tapjoy.isOfferwallOpen)
		{
			Singleton.UI.PopupManager.ShowPlayhavenPopup();
		}
		if (QualitySetter.FacebookEnabled)
		{
			FacebookManager.CheckRewardForFirstLogin();
		}
		PushNotifications.ILocalNotification[] scheduledLocalNotifications = PushNotifications.GetInstance().GetScheduledLocalNotifications();
		if (scheduledLocalNotifications != null && scheduledLocalNotifications.Length > 0)
		{
			for (int i = 0; i < scheduledLocalNotifications.Length; i++)
			{
				PushNotifications.GetInstance().CancelLocalNotification(scheduledLocalNotifications[i]);
			}
		}
		if (PushNotifications.GetInstance().GetLocalNotificationCount() > 0)
		{
			PushNotifications.GetInstance().ClearLocalNotifications();
		}
		if (PushNotifications.GetInstance().GetRemoteNotificationCount() > 0)
		{
			PushNotifications.GetInstance().ClearRemoteNotifications();
		}
		PushNotifications.GetInstance().SetBadgeNumber(-1);
		PushNotifications.GetInstance().SetBadgeNumber(-1);
	}

	private void OnDestroy()
	{
		_instance = null;
		GameAnalytics.LogAppStop(PlayerData.Session.Level, Time.time - _appStartTime, PlayerData.Session.MissionsCompleted);
		CFlurry.StopSession();
		if (QualitySetter.PlayHavenEnabled)
		{
			PlayHavenInterface.Destroy();
		}
	}

	public void GetMoreGames()
	{
		if (QualitySetter.PlayHavenEnabled)
		{
			PlayHavenInterface.StartPublisherContentRequest("more_games_only", true, true);
		}
	}

	public void GetFreeGold()
	{
		if (Singleton.Exists())
		{
			Singleton.PlayerData.TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.TapjoyButtonTapped);
		}
		Tapjoy.OpenOfferwall();
		_tapjoyInterfaceOpened = true;
	}

	public static bool PlayHavenGameLaunch
	{
		get
		{
			return _playHavenGameLaunch;
		}
		set
		{
			_playHavenGameLaunch = value;
		}
	}

	public bool FlushCachedAnalytics
	{
		get
		{
			return _flushCachedAnalytics;
		}
		set
		{
			if (value)
			{
				_flushTimer = new CountdownTimer(0.5f);
			}
			else
			{
				if (GameAnalytics.NumCachedEvents > 0)
				{
					GameAnalytics.FlushCachedEvents(GameAnalytics.NumCachedEvents);
				}
				_flushTimer = null;
			}
			_flushCachedAnalytics = value;
		}
	}
}
