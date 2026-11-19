using System.Collections;
using PlayHaven;
using UnityEngine;

[AddComponentMenu("PlayHaven/Content Requester")]
public class PlayHavenContentRequester : MonoBehaviour
{
	public enum WhenToRequest
	{
		Awake,
		Start,
		OnEnable,
		OnDisable,
		Manual
	}

	public enum InternetConnectivity
	{
		WiFiOnly = 0,
		CarrierNetworkOnly = 1,
		WiFiAndCarrierNetwork = 2,
		Always = 100
	}

	public enum MessageType
	{
		None,
		Send,
		Broadcast,
		Upwards
	}

	public enum ExhaustedAction
	{
		None,
		DestroySelf,
		DestroyGameObject,
		DestroyRoot
	}

	public WhenToRequest whenToRequest = WhenToRequest.Manual;

	public string placement = string.Empty;

	public WhenToRequest prefetch = WhenToRequest.Manual;

	public InternetConnectivity connectionForPrefetch;

	public bool refetchWhenUsed;

	public bool showsOverlayImmediately;

	public bool rewardMayBeDelivered;

	public MessageType rewardMessageType = MessageType.Broadcast;

	public bool useDefaultTestReward;

	public string defaultTestRewardName = string.Empty;

	public int defaultTestRewardQuantity = 1;

	public float requestDelay;

	public bool limitedUse;

	public int maxUses;

	public ExhaustedAction exhaustAction;

	private PlayHavenManager playHaven;

	private bool exhausted;

	private int uses;

	private int contentRequestId;

	private int prefetchRequestId;

	private bool requestIsInProgress;

	private bool prefetchIsInProgress;

	private bool refetch;

	private void Awake()
	{
		refetch = refetchWhenUsed;
		if (whenToRequest == WhenToRequest.Awake)
		{
			if (requestDelay > 0f)
			{
				Invoke("Request", requestDelay);
			}
			else
			{
				Request();
			}
		}
		else if (prefetch == WhenToRequest.Awake)
		{
			PreFetch();
		}
	}

	private void OnEnable()
	{
		if (whenToRequest == WhenToRequest.OnEnable)
		{
			if (requestDelay > 0f)
			{
				Invoke("Request", requestDelay);
			}
			else
			{
				Request();
			}
		}
		else if (prefetch == WhenToRequest.OnEnable)
		{
			PreFetch();
		}
	}

	private void OnDisable()
	{
		if (whenToRequest == WhenToRequest.OnDisable)
		{
			Request();
		}
		else if (prefetch == WhenToRequest.OnDisable)
		{
			PreFetch();
		}
	}

	private void OnDestroy()
	{
		if ((bool)Manager)
		{
			Manager.OnRewardGiven -= HandlePlayHavenManagerOnRewardGiven;
			Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
		}
	}

	private void Start()
	{
		if (whenToRequest == WhenToRequest.Start)
		{
			if (requestDelay > 0f)
			{
				Invoke("Request", requestDelay);
			}
			else
			{
				Request();
			}
		}
		else if (prefetch == WhenToRequest.Start)
		{
			PreFetch();
		}
	}

	private PlayHavenManager Manager
	{
		get
		{
			if (!playHaven)
			{
				playHaven = PlayHavenManager.instance;
			}
			return playHaven;
		}
	}

	public int RequestId
	{
		get
		{
			return contentRequestId;
		}
	}

	private void RequestPlayHavenContent()
	{
		if (requestDelay > 0f)
		{
			Invoke("Request", requestDelay);
		}
		else
		{
			Request();
		}
	}

	public bool IsExhausted
	{
		get
		{
			return limitedUse && uses > maxUses;
		}
	}

	public void PreFetch()
	{
		bool flag = true;
		switch (connectionForPrefetch)
		{
		case InternetConnectivity.WiFiOnly:
			flag = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
			break;
		case InternetConnectivity.CarrierNetworkOnly:
			flag = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
			break;
		case InternetConnectivity.WiFiAndCarrierNetwork:
			flag = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
			break;
		}
		if (!flag)
		{
			return;
		}
		if (prefetchIsInProgress)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("prefetch request is in progress; not making another request");
			}
			return;
		}
		if (Manager)
		{
			if (placement.Length > 0)
			{
				prefetchIsInProgress = true;
				Manager.OnSuccessPreloadRequest += HandleManagerOnSuccessPreloadRequest;
				if (Debug.isDebugBuild)
				{
					Debug.Log("Making content preload request for placement: " + placement);
				}
				prefetchRequestId = Manager.ContentPreloadRequest(placement);
			}
			else if (Debug.isDebugBuild)
			{
				Debug.LogError("placement value not set in PlayHaventContentRequester");
			}
		}
	}

	private void HandleManagerOnSuccessPreloadRequest(int requestId)
	{
		if (requestId == prefetchRequestId)
		{
			prefetchIsInProgress = false;
			if (Debug.isDebugBuild)
			{
				Debug.Log("prefetch of placement successful: " + placement);
			}
		}
	}

	public void Request()
	{
		Request(refetchWhenUsed);
	}

	public void Request(bool refetch)
	{
		StartCoroutine(_Request(refetch));
	}

	private IEnumerator _Request(bool refetch)
	{
		if (whenToRequest == WhenToRequest.Manual && requestDelay > 0f)
		{
			yield return new WaitForSeconds(requestDelay);
		}
		bool doRequest = true;
		if (requestIsInProgress)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("request is in progress; not making another request");
			}
			doRequest = false;
		}
		if (exhausted)
		{
			if (Application.isEditor)
			{
				Debug.LogWarning("content requester has been exhausted");
			}
			doRequest = false;
		}
		if (doRequest)
		{
			this.refetch = refetch;
			if ((bool)Manager)
			{
				if (placement.Length > 0)
				{
					Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
					Manager.OnDismissContent += HandlePlayHavenManagerOnDismissContent;
					if (rewardMayBeDelivered)
					{
						Manager.OnRewardGiven -= HandlePlayHavenManagerOnRewardGiven;
						Manager.OnRewardGiven += HandlePlayHavenManagerOnRewardGiven;
					}
					requestIsInProgress = true;
					contentRequestId = Manager.ContentRequest(placement, showsOverlayImmediately);
				}
				else if (Debug.isDebugBuild)
				{
					Debug.LogError("placement value not set in PlayHaventContentRequester");
				}
			}
			uses++;
			if (limitedUse && !rewardMayBeDelivered && uses >= maxUses)
			{
				Exhaust();
			}
		}
	}

	private void Exhaust()
	{
		exhausted = true;
		switch (exhaustAction)
		{
		case ExhaustedAction.DestroySelf:
			Object.Destroy(this);
			break;
		case ExhaustedAction.DestroyGameObject:
			Object.Destroy(gameObject);
			break;
		case ExhaustedAction.DestroyRoot:
			Object.Destroy(transform.root.gameObject);
			break;
		}
	}

	private void HandlePlayHavenManagerOnDismissContent(int hashCode, DismissType dismissType)
	{
		if (contentRequestId == hashCode)
		{
			requestIsInProgress = false;
			if ((bool)Manager)
			{
				Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
			}
			switch (rewardMessageType)
			{
			case MessageType.Broadcast:
				BroadcastMessage("OnPlayHavenContentDismissed", dismissType, SendMessageOptions.DontRequireReceiver);
				break;
			case MessageType.Send:
				SendMessage("OnPlayHavenContentDismissed", dismissType, SendMessageOptions.DontRequireReceiver);
				break;
			case MessageType.Upwards:
				SendMessageUpwards("OnPlayHavenContentDismissed", dismissType, SendMessageOptions.DontRequireReceiver);
				break;
			}
			if (!exhausted && limitedUse && uses > maxUses)
			{
				Exhaust();
			}
			else if (refetch)
			{
				PreFetch();
			}
		}
	}

	public void HandlePlayHavenManagerOnRewardGiven(int hashCode, Reward reward)
	{
		if (contentRequestId == hashCode)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Reward given -> " + reward);
			}
			switch (rewardMessageType)
			{
			case MessageType.Broadcast:
				BroadcastMessage("OnPlayHavenRewardGiven", reward);
				break;
			case MessageType.Send:
				SendMessage("OnPlayHavenRewardGiven", reward);
				break;
			case MessageType.Upwards:
				SendMessageUpwards("OnPlayHavenRewardGiven", reward);
				break;
			}
		}
	}
}
