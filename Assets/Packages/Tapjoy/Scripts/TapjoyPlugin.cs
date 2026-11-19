using System.ComponentModel;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum TapjoyDisplayAdSize
{
	[Description("320x50")]
	SIZE_320X50,
	[Description("640x100")]
	SIZE_640X100,
	[Description("768x90")]
	SIZE_768X90
}

public enum TapjoyViewType
{
	OFFERWALL,
	FULL_SCREEN_AD,
	DAILY_REWARD_AD,
	VIDEO_AD
}

public class TapjoyPlugin : MonoBehaviour
{
	public const string MacAddressOptionOn = "0";

	public const string MacAddressOptionOffWithVersionCheck = "1";

	public const string MacAddressOptionOff = "2";

	public static event Action connectCallSucceeded;

	public static event Action connectCallFailed;

	public static event Action<int> getTapPointsSucceeded;

	public static event Action getTapPointsFailed;

	public static event Action<int> spendTapPointsSucceeded;

	public static event Action spendTapPointsFailed;

	public static event Action awardTapPointsSucceeded;

	public static event Action awardTapPointsFailed;

	public static event Action<int> tapPointsEarned;

	public static event Action getFullScreenAdSucceeded;

	public static event Action getFullScreenAdFailed;

	public static event Action getDailyRewardAdSucceeded;

	public static event Action getDailyRewardAdFailed;

	public static event Action getDisplayAdSucceeded;

	public static event Action getDisplayAdFailed;

	public static event Action videoAdStarted;

	public static event Action videoAdFailed;

	public static event Action videoAdCompleted;

	public static event Action showOffersFailed;

	public static event Action<TapjoyViewType> viewOpened;

	public static event Action<TapjoyViewType> viewClosed;

	private void Awake()
	{
		gameObject.name = GetType().ToString();
		SetCallbackHandler(gameObject.name);
		MonoBehaviour.print("UnitySendMessage directs to " + gameObject.name);
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void TapjoyConnectSuccess(string message)
	{
		if (connectCallSucceeded != null)
		{
			connectCallSucceeded();
		}
	}

	public void TapjoyConnectFail(string message)
	{
		if (connectCallFailed != null)
		{
			connectCallFailed();
		}
	}

	public void TapPointsLoaded(string message)
	{
		if (getTapPointsSucceeded != null)
		{
			getTapPointsSucceeded(int.Parse(message));
		}
	}

	public void TapPointsLoadedError(string message)
	{
		if (getTapPointsFailed != null)
		{
			getTapPointsFailed();
		}
	}

	public void TapPointsSpent(string message)
	{
		if (spendTapPointsSucceeded != null)
		{
			spendTapPointsSucceeded(int.Parse(message));
		}
	}

	public void TapPointsSpendError(string message)
	{
		if (spendTapPointsFailed != null)
		{
			spendTapPointsFailed();
		}
	}

	public void TapPointsAwarded(string message)
	{
		if (awardTapPointsSucceeded != null)
		{
			awardTapPointsSucceeded();
		}
	}

	public void TapPointsAwardError(string message)
	{
		if (awardTapPointsFailed != null)
		{
			awardTapPointsFailed();
		}
	}

	public void CurrencyEarned(string message)
	{
		if (tapPointsEarned != null)
		{
			tapPointsEarned(int.Parse(message));
		}
	}

	public void FullScreenAdLoaded(string message)
	{
		if (getFullScreenAdSucceeded != null)
		{
			getFullScreenAdSucceeded();
		}
	}

	public void FullScreenAdError(string message)
	{
		if (getFullScreenAdFailed != null)
		{
			getFullScreenAdFailed();
		}
	}

	public void DailyRewardAdLoaded(string message)
	{
		if (getDailyRewardAdSucceeded != null)
		{
			getDailyRewardAdSucceeded();
		}
	}

	public void DailyRewardAdError(string message)
	{
		if (getDailyRewardAdFailed != null)
		{
			getDailyRewardAdFailed();
		}
	}

	public void DisplayAdLoaded(string message)
	{
		if (getDisplayAdSucceeded != null)
		{
			getDisplayAdSucceeded();
		}
	}

	public void DisplayAdError(string message)
	{
		if (getDisplayAdFailed != null)
		{
			getDisplayAdFailed();
		}
	}

	public void VideoAdStart(string message)
	{
		if (videoAdStarted != null)
		{
			videoAdStarted();
		}
	}

	public void VideoAdError(string message)
	{
		if (videoAdFailed != null)
		{
			videoAdFailed();
		}
	}

	public void VideoAdComplete(string message)
	{
		if (videoAdCompleted != null)
		{
			videoAdCompleted();
		}
	}

	public void ShowOffersError(string message)
	{
		if (showOffersFailed != null)
		{
			showOffersFailed();
		}
	}

	public void ViewOpened(string message)
	{
		if (viewOpened != null)
		{
			int obj = int.Parse(message);
			viewOpened((TapjoyViewType)obj);
		}
	}

	public void ViewClosed(string message)
	{
		if (viewClosed != null)
		{
			int obj = int.Parse(message);
			viewClosed((TapjoyViewType)obj);
		}
	}

	public static void SetCallbackHandler(string handlerName)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.SetCallbackHandler(handlerName);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.RequestTapjoyConnect(appID, secretKey);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey, Dictionary<string, string> flags)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.RequestTapjoyConnect(appID, secretKey, flags);
	}

	public static void EnableLogging(bool enable)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.EnableLogging(enable);
	}

	public static void ActionComplete(string actionID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ActionComplete(actionID);
	}

	public static void SetUserID(string userID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.SetUserID(userID);
	}

	public static void ShowOffers()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowOffers();
	}

	public static void GetTapPoints()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetTapPoints();
	}

	public static void SpendTapPoints(int points)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.SpendTapPoints(points);
	}

	public static void AwardTapPoints(int points)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.AwardTapPoints(points);
	}

	public static int QueryTapPoints()
	{
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			return 0;
		}
		return TapjoyPluginIOS.QueryTapPoints();
	}

	public static void ShowDefaultEarnedCurrencyAlert()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowDefaultEarnedCurrencyAlert();
	}

	public static void GetDisplayAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDisplayAd();
	}

	public static void ShowDisplayAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowDisplayAd();
	}

	public static void HideDisplayAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.HideDisplayAd();
	}

	[Obsolete("SetDisplayAdContentSize is deprecated. Please use SetDisplayAdSize.")]
	public static void SetDisplayAdContentSize(int size)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		SetDisplayAdSize((TapjoyDisplayAdSize)size);
	}

	public static void SetDisplayAdSize(TapjoyDisplayAdSize size)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		string displayAdSize = "320x50";
		if (size == TapjoyDisplayAdSize.SIZE_640X100)
		{
			displayAdSize = "640x100";
		}
		if (size == TapjoyDisplayAdSize.SIZE_768X90)
		{
			displayAdSize = "768x90";
		}
		TapjoyPluginIOS.SetDisplayAdSize(displayAdSize);
	}

	[Obsolete("RefreshDisplayAd(bool enable) is deprecated. Please use EnableDisplayAdAutoRefresh.")]
	public static void RefreshDisplayAd(bool enable)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.EnableDisplayAdAutoRefresh(enable);
	}

	public static void EnableDisplayAdAutoRefresh(bool enable)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.EnableDisplayAdAutoRefresh(enable);
	}

	[Obsolete("RefreshDisplayAd() is deprecated. Please use GetDisplayAd.")]
	public static void RefreshDisplayAd()
	{
		TapjoyPluginIOS.GetDisplayAd();
	}

	public static void MoveDisplayAd(int x, int y)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.MoveDisplayAd(x, y);
	}

	[Obsolete("SetUserDefinedColorWithIntValue is deprecated. There is no navigation bar for iOS in v9.x.x+.")]
	public static void SetUserDefinedColorWithIntValue(int color)
	{
	}

	public static void SetTransitionEffect(int transition)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.SetTransitionEffect(transition);
	}

	[Obsolete("GetFeaturedApp is deprecated, please use GetFullScreenAd instead.")]
	public static void GetFeaturedApp()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetFullScreenAd();
	}

	[Obsolete("SetFeaturedAppDisplayCount is deprecated.")]
	public static void SetFeaturedAppDisplayCount(int displayCount)
	{
	}

	[Obsolete("ShowFeaturedAppFullScreenAd is deprecated, please use ShowFullScreenAd instead.")]
	public static void ShowFeaturedAppFullScreenAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowFullScreenAd();
	}

	public static void GetFullScreenAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetFullScreenAd();
	}

	public static void ShowFullScreenAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowFullScreenAd();
	}

	[Obsolete("GetReEngagementAd is deprecated, please use GetDailyRewardAd instead.")]
	public static void GetReEngagementAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDailyRewardAd();
	}

	[Obsolete("ShowReEngagementAd is deprecated, please use ShowDailyRewardAd instead.")]
	public static void ShowReEngagementAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowDailyRewardAd();
	}

	public static void GetDailyRewardAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDailyRewardAd();
	}

	public static void ShowDailyRewardAd()
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowDailyRewardAd();
	}

	[Obsolete("InitVideoAd is deprecated, video is now controlled via your Tapjoy Dashboard.")]
	public static void InitVideoAd()
	{
	}

	[Obsolete("SetVideoCacheCount is deprecated, video is now controlled via your Tapjoy Dashboard.")]
	public static void SetVideoCacheCount(int cacheCount)
	{
	}

	[Obsolete("EnableVideoCache is deprecated, video is now controlled via your Tapjoy Dashboard.")]
	public static void EnableVideoCache(bool enable)
	{
	}

	public static void SendShutDownEvent()
	{
		TapjoyPluginIOS.SendShutDownEvent();
	}

	public static void SendIAPEvent(string name, float price, int quantity, string currencyCode)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.SendIAPEvent(name, price, quantity, currencyCode);
	}

	public static void ShowOffersWithCurrencyID(string currencyID, bool selector)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.ShowOffersWithCurrencyID(currencyID, selector);
	}

	public static void GetDisplayAdWithCurrencyID(string currencyID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDisplayAdWithCurrencyID(currencyID);
	}

	[Obsolete("RefreshDisplayAdWithCurrencyID is deprecated, please use GetDisplayAdWithCurrencyID instead.")]
	public static void RefreshDisplayAdWithCurrencyID(string currencyID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDisplayAdWithCurrencyID(currencyID);
	}

	[Obsolete("GetFeaturedAppWithCurrencyID is deprecated, please use GetFullScreenAdWithCurrencyID instead.")]
	public static void GetFeaturedAppWithCurrencyID(string currencyID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetFullScreenAdWithCurrencyID(currencyID);
	}

	public static void GetFullScreenAdWithCurrencyID(string currencyID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetFullScreenAdWithCurrencyID(currencyID);
	}

	public static void SetCurrencyMultiplier(float multiplier)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.SetCurrencyMultiplier(multiplier);
	}

	[Obsolete("GetReEngagementAdWithCurrencyID is deprecated, please use GetDailyRewardAdWithCurrencyID instead.")]
	public static void GetReEngagementAdWithCurrencyID(string currencyID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDailyRewardAdWithCurrencyID(currencyID);
	}

	public static void GetDailyRewardAdWithCurrencyID(string currencyID)
	{
		if (!(Application.platform != RuntimePlatform.OSXEditor))
		{
			return;
		}
		TapjoyPluginIOS.GetDailyRewardAdWithCurrencyID(currencyID);
	}
}
