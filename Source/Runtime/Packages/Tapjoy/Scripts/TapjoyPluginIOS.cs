using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum TapjoyTransition
{
	TJCTransitionBottomToTop = 0,
	TJCTransitionTopToBottom = 1,
	TJCTransitionLeftToRight = 2,
	TJCTransitionRightToLeft = 3,
	TJCTransitionFadeEffect = 4,
	TJCTransitionNormalToBottom = 5,
	TJCTransitionNormalToTop = 6,
	TJCTransitionNormalToLeft = 7,
	TJCTransitionNormalToRight = 8,
	TJCTransitionFadeEffectReverse = 9,
	TJCTransitionExpand = 10,
	TJCTransitionShrink = 11,
	TJCTransitionFlip = 12,
	TJCTransitionFlipReverse = 13,
	TJCTransitionPageCurl = 14,
	TJCTransitionPageCurlReverse = 15,
	TJCTransitionNoEffect = -1
}

public class TapjoyPluginIOS : MonoBehaviour
{
#if UNITY_IOS
	[DllImport("__Internal")]
	public static extern void _SetCallbackHandler(string handlerName);
#else
	public static void _SetCallbackHandler(string handlerName)
	{
		PcNative.Stub("_SetCallbackHandler");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _RequestTapjoyConnect(string appID, string secretKey);
#else
	private static void _RequestTapjoyConnect(string appID, string secretKey)
	{
		PcNative.Stub("_RequestTapjoyConnect");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SetFlagKeyValue(string flagKey, string flagValue);
#else
	private static void _SetFlagKeyValue(string flagKey, string flagValue)
	{
		PcNative.Stub("_SetFlagKeyValue");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _EnableLogging(bool enable);
#else
	private static void _EnableLogging(bool enable)
	{
		PcNative.Stub("_EnableLogging");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ActionComplete(string actionID);
#else
	private static void _ActionComplete(string actionID)
	{
		PcNative.Stub("_ActionComplete");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SetUserID(string userID);
#else
	private static void _SetUserID(string userID)
	{
		PcNative.Stub("_SetUserID");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ShowOffers();
#else
	private static void _ShowOffers()
	{
		PcNative.Stub("_ShowOffers");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ShowOffersWithCurrencyID(string currencyID, bool isSelectorVisible);
#else
	private static void _ShowOffersWithCurrencyID(string currencyID, bool isSelectorVisible)
	{
		PcNative.Stub("_ShowOffersWithCurrencyID");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetFullScreenAd();
#else
	private static void _GetFullScreenAd()
	{
		PcNative.Stub("_GetFullScreenAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetFullScreenAdWithCurrencyID(string currencyID);
#else
	private static void _GetFullScreenAdWithCurrencyID(string currencyID)
	{
		PcNative.Stub("_GetFullScreenAdWithCurrencyID");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SetCurrencyMultiplier(float multiplier);
#else
	private static void _SetCurrencyMultiplier(float multiplier)
	{
		PcNative.Stub("_SetCurrencyMultiplier");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ShowFullScreenAd();
#else
	private static void _ShowFullScreenAd()
	{
		PcNative.Stub("_ShowFullScreenAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetDailyRewardAd();
#else
	private static void _GetDailyRewardAd()
	{
		PcNative.Stub("_GetDailyRewardAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetDailyRewardAdWithCurrencyID(string currencyID);
#else
	private static void _GetDailyRewardAdWithCurrencyID(string currencyID)
	{
		PcNative.Stub("_GetDailyRewardAdWithCurrencyID");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ShowDailyRewardAd();
#else
	private static void _ShowDailyRewardAd()
	{
		PcNative.Stub("_ShowDailyRewardAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ShowDefaultEarnedCurrencyAlert();
#else
	private static void _ShowDefaultEarnedCurrencyAlert()
	{
		PcNative.Stub("_ShowDefaultEarnedCurrencyAlert");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetTapPoints();
#else
	private static void _GetTapPoints()
	{
		PcNative.Stub("_GetTapPoints");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SpendTapPoints(int points);
#else
	private static void _SpendTapPoints(int points)
	{
		PcNative.Stub("_SpendTapPoints");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _AwardTapPoints(int points);
#else
	private static void _AwardTapPoints(int points)
	{
		PcNative.Stub("_AwardTapPoints");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int _QueryTapPoints();
#else
	private static int _QueryTapPoints()
	{
		PcNative.Stub("_QueryTapPoints");
		return default(int);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetDisplayAd();
#else
	private static void _GetDisplayAd()
	{
		PcNative.Stub("_GetDisplayAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _ShowDisplayAd();
#else
	private static void _ShowDisplayAd()
	{
		PcNative.Stub("_ShowDisplayAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _HideDisplayAd();
#else
	private static void _HideDisplayAd()
	{
		PcNative.Stub("_HideDisplayAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _GetDisplayAdWithCurrencyID(string currencyID);
#else
	private static void _GetDisplayAdWithCurrencyID(string currencyID)
	{
		PcNative.Stub("_GetDisplayAdWithCurrencyID");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SetDisplayAdSize(string size);
#else
	private static void _SetDisplayAdSize(string size)
	{
		PcNative.Stub("_SetDisplayAdSize");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _EnableDisplayAdAutoRefresh(bool enable);
#else
	private static void _EnableDisplayAdAutoRefresh(bool enable)
	{
		PcNative.Stub("_EnableDisplayAdAutoRefresh");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _MoveDisplayAd(int x, int y);
#else
	private static void _MoveDisplayAd(int x, int y)
	{
		PcNative.Stub("_MoveDisplayAd");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SetTransitionEffect(int transition);
#else
	private static void _SetTransitionEffect(int transition)
	{
		PcNative.Stub("_SetTransitionEffect");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SendIAPEvent(string name, float price, int quantity, string currencyCode);
#else
	private static void _SendIAPEvent(string name, float price, int quantity, string currencyCode)
	{
		PcNative.Stub("_SendIAPEvent");
	}
#endif

	public static void SetCallbackHandler(string handlerName)
	{
		_SetCallbackHandler(handlerName);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey)
	{
		_RequestTapjoyConnect(appID, secretKey);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey, Dictionary<string, string> flags)
	{
		if (flags != null)
		{
			foreach (KeyValuePair<string, string> flag in flags)
			{
				_SetFlagKeyValue(flag.Key, flag.Value);
			}
		}
		_RequestTapjoyConnect(appID, secretKey);
	}

	public static void EnableLogging(bool enable)
	{
		_EnableLogging(enable);
	}

	public static void ActionComplete(string actionID)
	{
		_ActionComplete(actionID);
	}

	public static void SetUserID(string userID)
	{
		_SetUserID(userID);
	}

	public static void ShowOffers()
	{
		_ShowOffers();
	}

	public static void ShowOffersWithCurrencyID(string currencyID, bool isSelectorVisible)
	{
		_ShowOffersWithCurrencyID(currencyID, isSelectorVisible);
	}

	public static void GetFullScreenAd()
	{
		_GetFullScreenAd();
	}

	public static void GetFullScreenAdWithCurrencyID(string currencyID)
	{
		_GetFullScreenAdWithCurrencyID(currencyID);
	}

	public static void SetCurrencyMultiplier(float multiplier)
	{
		_SetCurrencyMultiplier(multiplier);
	}

	public static void ShowFullScreenAd()
	{
		_ShowFullScreenAd();
	}

	public static void GetDailyRewardAd()
	{
		_GetDailyRewardAd();
	}

	public static void GetDailyRewardAdWithCurrencyID(string currencyID)
	{
		_GetDailyRewardAdWithCurrencyID(currencyID);
	}

	public static void ShowDailyRewardAd()
	{
		_ShowDailyRewardAd();
	}

	public static void ShowDefaultEarnedCurrencyAlert()
	{
		_ShowDefaultEarnedCurrencyAlert();
	}

	public static void GetTapPoints()
	{
		_GetTapPoints();
	}

	public static void SpendTapPoints(int points)
	{
		_SpendTapPoints(points);
	}

	public static void AwardTapPoints(int points)
	{
		_AwardTapPoints(points);
	}

	public static int QueryTapPoints()
	{
		return _QueryTapPoints();
	}

	public static void GetDisplayAd()
	{
		_GetDisplayAd();
	}

	public static void ShowDisplayAd()
	{
		_ShowDisplayAd();
	}

	public static void HideDisplayAd()
	{
		_HideDisplayAd();
	}

	public static void GetDisplayAdWithCurrencyID(string currencyID)
	{
		_GetDisplayAdWithCurrencyID(currencyID);
	}

	public static void SetDisplayAdSize(string size)
	{
		_SetDisplayAdSize(size);
	}

	public static void EnableDisplayAdAutoRefresh(bool enable)
	{
		_EnableDisplayAdAutoRefresh(enable);
	}

	public static void MoveDisplayAd(int x, int y)
	{
		_MoveDisplayAd(x, y);
	}

	public static void SetTransitionEffect(int transition)
	{
		_SetTransitionEffect(transition);
	}

	public static void SendShutDownEvent()
	{
	}

	public static void SendIAPEvent(string name, float price, int quantity, string currencyCode)
	{
		_SendIAPEvent(name, price, quantity, currencyCode);
	}
}
