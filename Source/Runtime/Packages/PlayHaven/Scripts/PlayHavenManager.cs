using System;
using System.Collections;
using LitJson;
using PlayHaven;
using UnityEngine;

[AddComponentMenu("PlayHaven/Manager")]
public class PlayHavenManager : MonoBehaviour, IPlayHavenListener
{
	public enum WhenToOpen
	{
		Awake,
		Start,
		Manual
	}

	public enum WhenToGetNotifications
	{
		Disabled,
		Awake,
		Start,
		OnEnable,
		Manual,
		Poll
	}

	public delegate void CancelRequestHandler(int requestId);

	public const int NO_HASH_CODE = 0;

	public static string KEY_LAUNCH_COUNT = "playhaven-launch-count";

	public string token = string.Empty;

	[HideInInspector]
	public bool lockToken;

	public string secret = string.Empty;

	[HideInInspector]
	public bool lockSecret;

	public string tokenAndroid = string.Empty;

	[HideInInspector]
	public bool lockTokenAndroid;

	public string secretAndroid = string.Empty;

	[HideInInspector]
	public bool lockSecretAndroid;

	public bool doNotDestroyOnLoad = true;

	public bool defaultShowsOverlayImmediately;

	public bool maskShowsOverlayImmediately;

	public WhenToOpen whenToSendOpen;

	public WhenToGetNotifications whenToGetNotifications = WhenToGetNotifications.Start;

	public string badgeMoreGamesPlacement = "more_games";

	public float notificationPollDelay = 1f;

	public float notificationPollRate = 15f;

	public bool cancelAllOnLevelLoad;

	private ArrayList requestsInProgress = new ArrayList(8);

	public int suppressContentRequestsForLaunches;

	public string[] suppressedPlacements;

	public string[] suppressionExceptions;

	private int launchCount;

	public bool showContentUnitsInEditor = true;

	private string badge = string.Empty;

	private string customUDID = string.Empty;

	private bool autoCallOpenUponUnpause;

	private bool networkReachable = true;

	public bool maskNetworkReachable;

	public bool isAndroidSupported;

	private static PlayHavenManager _instance;

	private static bool wasWarned;

	public static PlayHavenManager instance
	{
		get
		{
			if (!_instance)
			{
				_instance = FindInstance();
			}
			return _instance;
		}
	}

	private static PlayHavenManager FindInstance()
	{
		PlayHavenManager playHavenManager = UnityEngine.Object.FindObjectOfType(typeof(PlayHavenManager)) as PlayHavenManager;
		if (!playHavenManager)
		{
			GameObject gameObject = GameObject.Find("PlayHavenManager");
			if (gameObject != null)
			{
				playHavenManager = gameObject.GetComponent<PlayHavenManager>();
			}
		}
		if (!playHavenManager && !wasWarned)
		{
			Debug.LogWarning("unable to locate a PlayHavenManager in the scene");
			wasWarned = true;
		}
		return playHavenManager;
	}

	private void Awake()
	{
		_instance = FindInstance();
		DetectNetworkReachable();
		if (token.Length == 0)
		{
			Debug.LogError("PlayHaven token has not been specified in the PlayerHavenManager");
		}
		if (secret.Length == 0)
		{
			Debug.LogError("PlayHaven secret has not been specified in the PlayerHavenManager");
		}
		gameObject.name = GetType().ToString();
		if (doNotDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		PlayHavenBinding.SetKeys(token, secret);
		PlayHavenBinding.listener = this;
		if (suppressContentRequestsForLaunches > 0)
		{
			launchCount = PlayerPrefs.GetInt(KEY_LAUNCH_COUNT, 0);
			launchCount++;
			PlayerPrefs.SetInt(KEY_LAUNCH_COUNT, launchCount);
			PlayerPrefs.Save();
			if (Debug.isDebugBuild)
			{
				Debug.Log("Launch count: " + launchCount);
			}
		}
		if (whenToSendOpen == WhenToOpen.Awake)
		{
			OpenNotification();
		}
		if (whenToGetNotifications == WhenToGetNotifications.Awake)
		{
			BadgeRequest(badgeMoreGamesPlacement);
		}
	}

	private void OnEnable()
	{
		if (whenToGetNotifications == WhenToGetNotifications.OnEnable)
		{
			BadgeRequest(badgeMoreGamesPlacement);
		}
	}

	private void Start()
	{
		if (whenToSendOpen == WhenToOpen.Start)
		{
			OpenNotification();
		}
		if (whenToGetNotifications == WhenToGetNotifications.Start)
		{
			BadgeRequest(badgeMoreGamesPlacement);
		}
		else if (whenToGetNotifications == WhenToGetNotifications.Poll)
		{
			PollForBadgeRequests();
		}
		autoCallOpenUponUnpause = true;
	}

	public event RequestCompletedHandler OnRequestCompleted;

	public event BadgeUpdateHandler OnBadgeUpdate;

	public event RewardTriggerHandler OnRewardGiven;

	public event PurchasePresentedTriggerHandler OnPurchasePresented;

	public event SimpleDismissHandler OnDismissCrossPromotionWidget;

	public event DismissHandler OnDismissContent;

	public event WillDisplayContentHandler OnWillDisplayContent;

	public event DidDisplayContentHandler OnDidDisplayContent;

	public event SuccessHandler OnSuccessOpenRequest;

	public event SuccessHandler OnSuccessPreloadRequest;

	public event ErrorHandler OnErrorOpenRequest;

	public event ErrorHandler OnErrorCrossPromotionWidget;

	public event ErrorHandler OnErrorContentRequest;

	public event ErrorHandler OnErrorMetadataRequest;

	public event CancelRequestHandler OnSuccessCancelRequest;

	public event CancelRequestHandler OnErrorCancelRequest;

	private void DetectNetworkReachable()
	{
		networkReachable = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		networkReachable &= !maskNetworkReachable;
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			DetectNetworkReachable();
			if (autoCallOpenUponUnpause)
			{
				OpenNotification();
			}
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		if (cancelAllOnLevelLoad)
		{
			CancelAllPendingRequests();
		}
	}

	public string CustomUDID
	{
		get
		{
			return customUDID;
		}
		set
		{
			customUDID = value;
		}
	}

	public bool OptOutStatus
	{
		get
		{
			return PlayHavenBinding.OptOutStatus;
		}
		set
		{
			PlayHavenBinding.OptOutStatus = value;
		}
	}

	public static bool IsAndroidSupported
	{
		get
		{
			PlayHavenManager playHavenManager = instance;
			if (playHavenManager != null)
			{
				return playHavenManager.isAndroidSupported;
			}
			return false;
		}
	}

	public bool IsPlacementSuppressed(string placement)
	{
		if (suppressContentRequestsForLaunches > 0 && launchCount < suppressContentRequestsForLaunches)
		{
			if (suppressedPlacements != null && suppressedPlacements.Length > 0)
			{
				foreach (string text in suppressedPlacements)
				{
					if (text == placement)
					{
						return true;
					}
				}
				return false;
			}
			if (suppressionExceptions != null && suppressionExceptions.Length > 0)
			{
				foreach (string text2 in suppressionExceptions)
				{
					if (text2 == placement)
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}
		return false;
	}

	public int OpenNotification(string customUDID)
	{
		if (networkReachable)
		{
			CustomUDID = customUDID;
			int num = PlayHavenBinding.Open(customUDID);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int OpenNotification()
	{
		if (networkReachable)
		{
			int num = PlayHavenBinding.Open(CustomUDID);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public void CancelAllPendingRequests()
	{
		foreach (int item in requestsInProgress)
		{
			PlayHavenBinding.CancelRequest(item);
		}
		requestsInProgress.Clear();
	}

	public void ProductPurchaseResolutionRequest(PurchaseResolution resolution)
	{
		PlayHavenBinding.SendProductPurchaseResolution(resolution);
	}

	public void ProductPurchaseTrackingRequest(Purchase purchase, PurchaseResolution resolution)
	{
		PlayHavenBinding.SendIAPTrackingRequest(purchase, resolution);
	}

	public int ContentPreloadRequest(string placement)
	{
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Preload, placement);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int ContentRequest(string placement)
	{
		if (IsPlacementSuppressed(placement))
		{
			return 0;
		}
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Content, placement, defaultShowsOverlayImmediately);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int ContentRequest(string placement, bool showsOverlayImmediately)
	{
		if (IsPlacementSuppressed(placement))
		{
			return 0;
		}
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Content, placement, showsOverlayImmediately && !maskShowsOverlayImmediately);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	[Obsolete("This method is obsolete; it assumes that you will have a placement called more_games; instead, simply use ContentRequest() but with the relevant placement.", false)]
	public int ShowCrossPromotionWidget()
	{
		if (networkReachable)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.CrossPromotionWidget, string.Empty, defaultShowsOverlayImmediately);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	public int BadgeRequest(string placement)
	{
		if (networkReachable && whenToGetNotifications != WhenToGetNotifications.Disabled)
		{
			int num = PlayHavenBinding.SendRequest(PlayHavenBinding.RequestType.Metadata, placement);
			requestsInProgress.Add(num);
			return num;
		}
		return 0;
	}

	[Obsolete("This method is obsolete; it assumes that you will have a placement called more_games; instead, simply use BadgeRequest() but with the relevant placement.", false)]
	public int BadgeRequest()
	{
		return BadgeRequest("more_games");
	}

	public void PollForBadgeRequests()
	{
		CancelInvoke("BadgeRequestPolled");
		if (notificationPollRate > 0f)
		{
			if (!string.IsNullOrEmpty(badgeMoreGamesPlacement))
			{
				InvokeRepeating("BadgeRequestPolled", notificationPollDelay, notificationPollRate);
			}
			else if (Debug.isDebugBuild)
			{
				Debug.LogError("A more games badge placement is not defined.");
			}
		}
		else
		{
			Debug.LogError("cannot have a notification poll rate <= 0");
		}
	}

	private void BadgeRequestPolled()
	{
		BadgeRequest(badgeMoreGamesPlacement);
	}

	public void NotifyRequestCompleted(int requestId)
	{
		requestsInProgress.Remove(requestId);
		if (OnRequestCompleted != null)
		{
			OnRequestCompleted(requestId);
		}
	}

	public void NotifyOpenSuccess(int requestId)
	{
		if (OnSuccessOpenRequest != null)
		{
			OnSuccessOpenRequest(requestId);
		}
	}

	public void NotifyOpenError(int requestId, Error error)
	{
		if (OnErrorOpenRequest != null)
		{
			OnErrorOpenRequest(requestId, error);
		}
	}

	public void NotifyWillDisplayContent(int requestId)
	{
		if (OnWillDisplayContent != null)
		{
			OnWillDisplayContent(requestId);
		}
	}

	public void NotifyDidDisplayContent(int requestId)
	{
		if (OnDidDisplayContent != null)
		{
			OnDidDisplayContent(requestId);
		}
	}

	public void NotifyPreloadSuccess(int requestId)
	{
		if (OnSuccessPreloadRequest != null)
		{
			OnSuccessPreloadRequest(requestId);
		}
	}

	public void NotifyBadgeUpdate(int requestId, string badge)
	{
		this.badge = badge;
		if (OnBadgeUpdate != null)
		{
			OnBadgeUpdate(requestId, badge);
		}
	}

	public void NotifyRewardGiven(int requestId, Reward reward)
	{
		if (OnRewardGiven != null)
		{
			OnRewardGiven(requestId, reward);
		}
	}

	public void NotifyPurchasePresented(int requestId, Purchase purchase)
	{
		if (OnPurchasePresented != null)
		{
			OnPurchasePresented(requestId, purchase);
		}
	}

	public void NotifyCrossPromotionWidgetDismissed()
	{
		if (OnDismissCrossPromotionWidget != null)
		{
			OnDismissCrossPromotionWidget();
		}
	}

	public void NotifyCrossPromotionWidgetError(int requestId, Error error)
	{
		if (OnErrorCrossPromotionWidget != null)
		{
			OnErrorCrossPromotionWidget(requestId, error);
		}
	}

	public void NotifyContentDismissed(int requestId, DismissType dismissType)
	{
		if (OnDismissContent != null)
		{
			OnDismissContent(requestId, dismissType);
		}
	}

	public void NotifyContentError(int requestId, Error error)
	{
		if (OnErrorContentRequest != null)
		{
			OnErrorContentRequest(requestId, error);
		}
	}

	public void NotifyMetaDataError(int requestId, Error error)
	{
		if (OnErrorMetadataRequest != null)
		{
			OnErrorMetadataRequest(requestId, error);
		}
	}

	public string Badge
	{
		get
		{
			return badge;
		}
	}

	public void ClearBadge()
	{
		badge = string.Empty;
	}

	public void HandleNativeEvent(string json)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("JSON (native event): " + json);
		}
		JsonData jsonData = JsonMapper.ToObject(json);
		int num = (int)jsonData["hash"];
		PlayHavenBinding.IPlayHavenRequest requestWithHash = PlayHavenBinding.GetRequestWithHash(num);
		if (requestWithHash != null)
		{
			string text = (string)jsonData["name"];
			if (Debug.isDebugBuild)
			{
				Debug.Log(string.Format("PlayHaven event={0} (id={1})", text, num));
			}
			JsonData eventData = jsonData["data"];
			requestWithHash.TriggerEvent(text, eventData);
			if (requestWithHash is PlayHavenBinding.ContentRequest && text == "reward")
			{
				StartCoroutine(DelayedClearRequestWithHash(1f, num));
			}
			else if (text != "willdisplay" && text != "diddisplay" && text != "gotcontent")
			{
				PlayHavenBinding.ClearRequestWithHash(num);
			}
		}
		else if (Debug.isDebugBuild)
		{
			Debug.LogError("Unable to locate request with id=" + num);
		}
	}

	private IEnumerator DelayedClearRequestWithHash(float delay, int hash)
	{
		yield return new WaitForSeconds(delay);
		PlayHavenBinding.ClearRequestWithHash(hash);
	}

	public void RequestCancelSuccess(string hashCodeString)
	{
		int num = Convert.ToInt32(hashCodeString);
		PlayHavenBinding.ClearRequestWithHash(num);
		if (OnSuccessCancelRequest != null)
		{
			OnSuccessCancelRequest(num);
		}
	}

	public void RequestCancelFailed(string hashCodeString)
	{
		if (OnErrorCancelRequest != null)
		{
			int requestId = Convert.ToInt32(hashCodeString);
			OnErrorCancelRequest(requestId);
		}
	}
}

namespace PlayHaven
{
	public delegate void RequestCompletedHandler(int requestId);
}

namespace PlayHaven
{
	public delegate void BadgeUpdateHandler(int requestId, string badge);
}

namespace PlayHaven
{
	public delegate void RewardTriggerHandler(int requestId, Reward reward);
}

namespace PlayHaven
{
	public delegate void PurchasePresentedTriggerHandler(int requestId, Purchase purchase);
}

namespace PlayHaven
{
	public delegate void SuccessHandler(int requestId);
}

namespace PlayHaven
{
	public delegate void WillDisplayContentHandler(int requestId);
}

namespace PlayHaven
{
	public delegate void DidDisplayContentHandler(int requestId);
}

namespace PlayHaven
{
	public delegate void DismissHandler(int requestId, DismissType dismissType);
}

namespace PlayHaven
{
	public delegate void SimpleDismissHandler();
}

namespace PlayHaven
{
	public delegate void ErrorHandler(int requestId, Error error);
}
