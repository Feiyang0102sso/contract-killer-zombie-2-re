using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PlayHaven;
using UnityEngine;

public static class PlayHavenInterface
{
	public delegate void DismissHandler(string placement);

	public delegate void ErrorHandler(string placement, string errorDescription);

	public delegate void PurchaseInitiatedHandler(string productId);

	public delegate void RewardGivenHandler(string name, int quantity);

	private static string _activePlacement = string.Empty;

	private static int _lastContentRequestHash = 0;

	private static bool _showAlert = false;

	private static bool _isPrecachedModal = false;

	private static HashSet<string> _badgePlacements = new HashSet<string>();

	public static event DismissHandler OnDismiss;

	public static event ErrorHandler OnError;

	public static event RewardGivenHandler OnRewarded;

	public static event PurchaseInitiatedHandler OnPurchaseInitiated;

	public static void Init(string appToken, string secretCode)
	{
		Component component = UnityEngine.Object.FindObjectOfType(typeof(PlayHavenManager)) as Component;
		if (component == null)
		{
			GameObject gameObject = new GameObject("PlayHavenManager");
			gameObject.active = false;
			PlayHavenManager playHavenManager = gameObject.AddComponent<PlayHavenManager>();
			playHavenManager.token = appToken;
			playHavenManager.secret = secretCode;
			gameObject.active = true;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			PlayHavenManager.instance.cancelAllOnLevelLoad = true;
			PlayHavenManager.instance.OnErrorContentRequest += ContentErrorHandler;
			PlayHavenManager.instance.OnErrorCrossPromotionWidget += ContentErrorHandler;
			PlayHavenManager.instance.OnDismissContent += ContentDismissHandler;
			PlayHavenManager.instance.OnDismissCrossPromotionWidget += OldDismissHandler;
			PlayHavenManager.instance.OnPurchasePresented += PurchaseTriggered;
			PlayHavenManager.instance.OnSuccessPreloadRequest += PreloadSuccessHandler;
			PlayHavenManager.instance.OnRewardGiven += RewardHandler;
		}
	}

	public static void Destroy()
	{
		Component component = UnityEngine.Object.FindObjectOfType(typeof(PlayHavenManager)) as Component;
		if (component != null)
		{
			UnityEngine.Object.Destroy(component.gameObject);
			PlayHavenManager.instance.OnErrorContentRequest -= ContentErrorHandler;
			PlayHavenManager.instance.OnErrorCrossPromotionWidget -= ContentErrorHandler;
			PlayHavenManager.instance.OnDismissContent -= ContentDismissHandler;
			PlayHavenManager.instance.OnDismissCrossPromotionWidget -= OldDismissHandler;
			PlayHavenManager.instance.OnPurchasePresented -= PurchaseTriggered;
			PlayHavenManager.instance.OnSuccessPreloadRequest -= PreloadSuccessHandler;
			PlayHavenManager.instance.OnRewardGiven -= RewardHandler;
		}
	}

	public static void StartPublisherContentRequest(string placement, bool showOverlayImmediately, bool showErrorMessage)
	{
		if (!(_lastContentRequestHash != 0 && PlayHavenBinding.GetRequestWithHash(_lastContentRequestHash) != null))
		{
			_activePlacement = placement;
			_showAlert = showErrorMessage;
			_lastContentRequestHash = PlayHavenManager.instance.ContentRequest(placement, showOverlayImmediately);
			if (_lastContentRequestHash == 0)
			{
				if (OnError != null)
				{
					OnError(placement, "No network connection");
				}
				if (_showAlert)
				{
					DisplayAlert("PlayHaven error", "Network connection is required to use this feature");
				}
			}
		}
	}

	public static void StartPublisherContentRequest(string placement, bool showOverlayImmediately)
	{
		StartPublisherContentRequest(placement, showOverlayImmediately, false);
	}

	public static void StartPublisherContentRequestWithPreload(string placement, bool showOverlayImmediately, bool showErrorMessage)
	{
		if (!(_lastContentRequestHash != 0 && PlayHavenBinding.GetRequestWithHash(_lastContentRequestHash) != null))
		{
			_activePlacement = placement;
			_isPrecachedModal = showOverlayImmediately;
			_showAlert = showErrorMessage;
			_lastContentRequestHash = PlayHavenManager.instance.ContentPreloadRequest(placement);
			if (_lastContentRequestHash == 0)
			{
				if (OnError != null)
				{
					OnError(placement, "No network connection");
				}
				if (_showAlert)
				{
					DisplayAlert("PlayHaven error", "Network connection is required to use this feature");
				}
			}
		}
	}

	public static void StartPublisherContentRequestWithPreload(string placement, bool showOverlayImmediately)
	{
		StartPublisherContentRequestWithPreload(placement, showOverlayImmediately, false);
	}

	public static void SendIapTrackingRequest(string productId, PurchaseResolution resolution)
	{
		if (!Integrity.IsJailbroken())
		{
			Purchase purchase = new Purchase();
			purchase.productIdentifier = productId;
			purchase.quantity = 1;
			PlayHavenManager.instance.ProductPurchaseTrackingRequest(purchase, resolution);
		}
	}

	public static void SendVgpResolutionRequest(PurchaseResolution resolution)
	{
		PlayHavenManager.instance.ProductPurchaseResolutionRequest(resolution);
	}

	private static void PurchaseTriggered(int requestId, Purchase purchase)
	{
		if (OnPurchaseInitiated != null)
		{
			OnPurchaseInitiated(purchase.productIdentifier);
		}
	}

	public static void CancelCurrentContentRequest()
	{
		if (_lastContentRequestHash != 0)
		{
			PlayHavenBinding.CancelRequest(_lastContentRequestHash);
			_lastContentRequestHash = 0;
		}
	}

	public static int GetBadgeNumber(string placement)
	{
		if (!_badgePlacements.Contains(placement))
		{
			_badgePlacements.Add(placement);
		}
		return Convert.ToInt32("0" + PlayHavenManager.instance.Badge);
	}

	public static int GetBadgeNumber()
	{
		return GetBadgeNumber("more_games");
	}

	private static void ContentErrorHandler(int requestId, Error error)
	{
		_lastContentRequestHash = 0;
		if (OnError != null)
		{
			OnError(_activePlacement, error.description);
		}
		if (_showAlert)
		{
			DisplayAlert("PlayHaven error", "Network connection is required to use this feature");
		}
	}

	private static void ContentDismissHandler(int requestId, DismissType type)
	{
		_lastContentRequestHash = 0;
		if (_badgePlacements.Contains(_activePlacement) && Convert.ToInt32("0" + PlayHavenManager.instance.Badge) != 0)
		{
			PlayHavenManager.instance.BadgeRequest(_activePlacement);
		}
		if (OnDismiss != null)
		{
			OnDismiss(_activePlacement);
		}
	}

	private static void PreloadSuccessHandler(int requestId)
	{
		_lastContentRequestHash = 0;
		StartPublisherContentRequest(_activePlacement, _isPrecachedModal, _showAlert);
	}

	private static void OldDismissHandler()
	{
		_lastContentRequestHash = 0;
		if (_badgePlacements.Contains(_activePlacement) && Convert.ToInt32("0" + PlayHavenManager.instance.Badge) != 0)
		{
			PlayHavenManager.instance.BadgeRequest(_activePlacement);
		}
		if (OnDismiss != null)
		{
			OnDismiss(_activePlacement);
		}
	}

	private static void RewardHandler(int requestId, Reward reward)
	{
		if (OnRewarded != null)
		{
			OnRewarded(reward.name, reward.quantity);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void DisplayAlert(string title, string text);
#else
	private static void DisplayAlert(string title, string text)
	{
		PcNative.Stub("DisplayAlert");
	}
#endif
}
