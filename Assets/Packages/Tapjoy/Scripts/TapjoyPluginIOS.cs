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
	[DllImport("__Internal")]
	public static extern void _SetCallbackHandler(string handlerName);

	[DllImport("__Internal")]
	private static extern void _RequestTapjoyConnect(string appID, string secretKey);

	[DllImport("__Internal")]
	private static extern void _SetFlagKeyValue(string flagKey, string flagValue);

	[DllImport("__Internal")]
	private static extern void _EnableLogging(bool enable);

	[DllImport("__Internal")]
	private static extern void _ActionComplete(string actionID);

	[DllImport("__Internal")]
	private static extern void _SetUserID(string userID);

	[DllImport("__Internal")]
	private static extern void _ShowOffers();

	[DllImport("__Internal")]
	private static extern void _ShowOffersWithCurrencyID(string currencyID, bool isSelectorVisible);

	[DllImport("__Internal")]
	private static extern void _GetFullScreenAd();

	[DllImport("__Internal")]
	private static extern void _GetFullScreenAdWithCurrencyID(string currencyID);

	[DllImport("__Internal")]
	private static extern void _SetCurrencyMultiplier(float multiplier);

	[DllImport("__Internal")]
	private static extern void _ShowFullScreenAd();

	[DllImport("__Internal")]
	private static extern void _GetDailyRewardAd();

	[DllImport("__Internal")]
	private static extern void _GetDailyRewardAdWithCurrencyID(string currencyID);

	[DllImport("__Internal")]
	private static extern void _ShowDailyRewardAd();

	[DllImport("__Internal")]
	private static extern void _ShowDefaultEarnedCurrencyAlert();

	[DllImport("__Internal")]
	private static extern void _GetTapPoints();

	[DllImport("__Internal")]
	private static extern void _SpendTapPoints(int points);

	[DllImport("__Internal")]
	private static extern void _AwardTapPoints(int points);

	[DllImport("__Internal")]
	private static extern int _QueryTapPoints();

	[DllImport("__Internal")]
	private static extern void _GetDisplayAd();

	[DllImport("__Internal")]
	private static extern void _ShowDisplayAd();

	[DllImport("__Internal")]
	private static extern void _HideDisplayAd();

	[DllImport("__Internal")]
	private static extern void _GetDisplayAdWithCurrencyID(string currencyID);

	[DllImport("__Internal")]
	private static extern void _SetDisplayAdSize(string size);

	[DllImport("__Internal")]
	private static extern void _EnableDisplayAdAutoRefresh(bool enable);

	[DllImport("__Internal")]
	private static extern void _MoveDisplayAd(int x, int y);

	[DllImport("__Internal")]
	private static extern void _SetTransitionEffect(int transition);

	[DllImport("__Internal")]
	private static extern void _SendIAPEvent(string name, float price, int quantity, string currencyCode);

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
