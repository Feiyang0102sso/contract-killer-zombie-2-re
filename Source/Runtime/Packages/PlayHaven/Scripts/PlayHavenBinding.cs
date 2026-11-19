using System;
using System.Collections;
using System.Runtime.InteropServices;
using LitJson;
using UnityEngine;

namespace PlayHaven
{
	public enum PurchaseResolution
	{
		Buy,
		Cancel,
		Error
	}
}

namespace PlayHaven
{
	public enum DismissType
	{
		Unknown,
		PHPublisherContentUnitTriggeredDismiss,
		PHPublisherNativeCloseButtonTriggeredDismiss,
		PHPublisherApplicationBackgroundTriggeredDismiss,
		PHPublisherNoContentTriggeredDismiss
	}
}

namespace PlayHaven
{
	public interface IPlayHavenListener
	{
		event RequestCompletedHandler OnRequestCompleted;

		event BadgeUpdateHandler OnBadgeUpdate;

		event RewardTriggerHandler OnRewardGiven;

		event PurchasePresentedTriggerHandler OnPurchasePresented;

		event SimpleDismissHandler OnDismissCrossPromotionWidget;

		event DismissHandler OnDismissContent;

		event WillDisplayContentHandler OnWillDisplayContent;

		event DidDisplayContentHandler OnDidDisplayContent;

		event SuccessHandler OnSuccessOpenRequest;

		event SuccessHandler OnSuccessPreloadRequest;

		event ErrorHandler OnErrorOpenRequest;

		event ErrorHandler OnErrorCrossPromotionWidget;

		event ErrorHandler OnErrorContentRequest;

		event ErrorHandler OnErrorMetadataRequest;

		void NotifyRequestCompleted(int requestId);

		void NotifyOpenSuccess(int requestId);

		void NotifyPreloadSuccess(int requestId);

		void NotifyOpenError(int requestId, Error error);

		void NotifyWillDisplayContent(int requestId);

		void NotifyDidDisplayContent(int requestId);

		void NotifyBadgeUpdate(int requestId, string badge);

		void NotifyRewardGiven(int requestId, Reward reward);

		void NotifyPurchasePresented(int requestId, Purchase purchase);

		void NotifyCrossPromotionWidgetDismissed();

		void NotifyCrossPromotionWidgetError(int requestId, Error error);

		void NotifyContentDismissed(int requestId, DismissType dismissType);

		void NotifyContentError(int requestId, Error error);

		void NotifyMetaDataError(int requestId, Error error);
	}
}

namespace PlayHaven
{
	public class Error
	{
		public int code;

		public string description = string.Empty;

		public override string ToString()
		{
			return "code: " + code + ", description: " + description;
		}
	}
}

namespace PlayHaven
{
	public class Reward
	{
		public string receipt;

		public string name;

		public int quantity;

		public override string ToString()
		{
			return "name: " + name + ", quantity: " + quantity + ", receipt: " + receipt;
		}
	}
}

namespace PlayHaven
{
	public class Purchase
	{
		public string productIdentifier;

		public int quantity;

		public string receipt;

		public override string ToString()
		{
			return "productIdentifier: " + productIdentifier + ", quantity: " + quantity + ", receipt: " + receipt;
		}
	}
}

namespace PlayHaven
{
	public class PlayHavenBinding : IDisposable
	{
		public enum RequestType
		{
			Open,
			Metadata,
			Content,
			Preload,
			CrossPromotionWidget
		}

		public interface IPlayHavenRequest
		{
			int HashCode { get; }

			event GeneralHandler OnWillDisplay;

			event GeneralHandler OnDidDisplay;

			event SuccessHandler OnSuccess;

			event ErrorHandler OnError;

			event DismissHandler OnDismiss;

			event RewardHandler OnReward;

			event PurchaseHandler OnPurchasePresented;

			void Send();

			void Send(bool showsOverlayImmediately);

			void TriggerEvent(string eventName, JsonData eventData);
		}

		public class OpenRequest : IPlayHavenRequest
		{
			private int hashCode;

			private string customUDID;

#if UNITY_IOS
			[DllImport("__Internal")]
			private static extern void _PlayHavenOpenRequest(int hash, string token, string secret, string customUDID);
#else
			private static void _PlayHavenOpenRequest(int hash, string token, string secret, string customUDID)
			{
				PcNative.Stub("_PlayHavenOpenRequest");
			}
#endif

			public event SuccessHandler OnSuccess = delegate
			{
			};

			public event ErrorHandler OnError = delegate
			{
			};

			public event DismissHandler OnDismiss;

			public event RewardHandler OnReward;

			public event PurchaseHandler OnPurchasePresented;

			public event GeneralHandler OnWillDisplay;

			public event GeneralHandler OnDidDisplay;

			public OpenRequest()
			{
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}

			public OpenRequest(string customUDID)
			{
				this.customUDID = customUDID;
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: open request");
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["notification"] = new Hashtable();
						Hashtable hashtable2 = new Hashtable();
						hashtable2["data"] = hashtable;
						hashtable2["hash"] = hashCode;
						hashtable2["name"] = "success";
						string json = JsonMapper.ToJson(hashtable2);
						instance.HandleNativeEvent(json);
					}
				}
				else
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log(string.Format("PlayHaven: open request (id={0})", hashCode));
					}
					_PlayHavenOpenRequest(hashCode, token, secret, customUDID);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "success") == 0)
				{
					Debug.Log("PlayHaven: Open request success!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnSuccess(this, eventData);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					Debug.LogError("PlayHaven: Open request failed!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnError(this, eventData);
				}
			}
		}

		public class MetadataRequest : IPlayHavenRequest
		{
			protected string mPlacement;

			private int hashCode;

#if UNITY_IOS
			[DllImport("__Internal")]
			private static extern void _PlayHavenMetadataRequest(int hash, string token, string secret, string placement);
#else
			private static void _PlayHavenMetadataRequest(int hash, string token, string secret, string placement)
			{
				PcNative.Stub("_PlayHavenMetadataRequest");
			}
#endif

			public event SuccessHandler OnSuccess = delegate
			{
			};

			public event ErrorHandler OnError = delegate
			{
			};

			public event DismissHandler OnDismiss;

			public event RewardHandler OnReward;

			public event PurchaseHandler OnPurchasePresented;

			public event GeneralHandler OnWillDisplay = delegate
			{
			};

			public event GeneralHandler OnDidDisplay = delegate
			{
			};

			public MetadataRequest(string placement)
			{
				mPlacement = placement;
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: metadata request (" + mPlacement + ")");
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["type"] = "badge";
						hashtable["value"] = "1";
						Hashtable hashtable2 = new Hashtable();
						hashtable2["notification"] = hashtable;
						Hashtable hashtable3 = new Hashtable();
						hashtable3["data"] = hashtable2;
						hashtable3["hash"] = hashCode;
						hashtable3["name"] = "success";
						hashtable3["content"] = mPlacement;
						string json = JsonMapper.ToJson(hashtable3);
						instance.HandleNativeEvent(json);
					}
				}
				else
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log(string.Format("PlayHaven: metadata request (id={0}, placement={1})", hashCode, mPlacement));
					}
					_PlayHavenMetadataRequest(hashCode, token, secret, mPlacement);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "success") == 0)
				{
					Debug.Log("PlayHaven: Metadata request success!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnSuccess(this, eventData);
				}
				else if (string.Compare(eventName, "willdisplay") == 0)
				{
					OnWillDisplay(this);
				}
				else if (string.Compare(eventName, "diddisplay") == 0)
				{
					OnDidDisplay(this);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					Debug.LogError("PlayHaven: Metadata request failed!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnError(this, eventData);
				}
			}
		}

		public class ContentRequest : IPlayHavenRequest
		{
			protected string mPlacement;

			private int hashCode;

#if UNITY_IOS
			[DllImport("__Internal")]
			private static extern void _PlayHavenContentRequest(int hash, string token, string secret, string placement, bool showsOverlayImmediately);
#else
			private static void _PlayHavenContentRequest(int hash, string token, string secret, string placement, bool showsOverlayImmediately)
			{
				PcNative.Stub("_PlayHavenContentRequest");
			}
#endif

			public event SuccessHandler OnSuccess;

			public event DismissHandler OnDismiss = delegate
			{
			};

			public event ErrorHandler OnError = delegate
			{
			};

			public event RewardHandler OnReward = delegate
			{
			};

			public event PurchaseHandler OnPurchasePresented = delegate
			{
			};

			public event GeneralHandler OnWillDisplay = delegate
			{
			};

			public event GeneralHandler OnDidDisplay = delegate
			{
			};

			public ContentRequest(string placement)
			{
				mPlacement = placement;
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: content request (" + mPlacement + ")");
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["notification"] = new Hashtable();
						Hashtable hashtable2 = new Hashtable();
						hashtable2["data"] = hashtable;
						hashtable2["hash"] = hashCode;
						hashtable2["name"] = "dismiss";
						string json = JsonMapper.ToJson(hashtable2);
						instance.HandleNativeEvent(json);
					}
				}
				else
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log(string.Format("PlayHaven: content request (id={0}, placement={1})", hashCode, mPlacement));
					}
					_PlayHavenContentRequest(hashCode, token, secret, mPlacement, showsOverlayImmediately);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "reward") == 0)
				{
					Debug.Log("PlayHaven: Reward unlocked");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnReward(this, eventData);
				}
				else if (string.Compare(eventName, "purchasePresentation") == 0)
				{
					Debug.Log("PlayHaven: Purchase presented");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnPurchasePresented(this, eventData);
				}
				else if (string.Compare(eventName, "dismiss") == 0)
				{
					Debug.Log("PlayHaven: Content was dismissed!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnDismiss(this, eventData);
				}
				else if (string.Compare(eventName, "willdisplay") == 0)
				{
					OnWillDisplay(this);
				}
				else if (string.Compare(eventName, "diddisplay") == 0)
				{
					OnDidDisplay(this);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					Debug.LogError("PlayHaven: Content error!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnError(this, eventData);
				}
			}
		}

		public class ContentPreloadRequest : IPlayHavenRequest
		{
			protected string mPlacement;

			private int hashCode;

#if UNITY_IOS
			[DllImport("__Internal")]
			private static extern void _PlayHavenPreloadRequest(int hash, string token, string secret, string placement);
#else
			private static void _PlayHavenPreloadRequest(int hash, string token, string secret, string placement)
			{
				PcNative.Stub("_PlayHavenPreloadRequest");
			}
#endif

			public event SuccessHandler OnSuccess = delegate
			{
			};

			public event DismissHandler OnDismiss;

			public event ErrorHandler OnError = delegate
			{
			};

			public event RewardHandler OnReward;

			public event PurchaseHandler OnPurchasePresented;

			public event GeneralHandler OnWillDisplay;

			public event GeneralHandler OnDidDisplay;

			public ContentPreloadRequest(string placement)
			{
				mPlacement = placement;
				hashCode = GetHashCode();
				sRequests.Add(GetHashCode(), this);
			}

			public int HashCode
			{
				get
				{
					return hashCode;
				}
			}

			public void Send()
			{
				Send(false);
			}

			public void Send(bool showsOverlayImmediately)
			{
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: content preload request (" + mPlacement + ")");
					PlayHavenManager instance = PlayHavenManager.instance;
					if (instance != null)
					{
						Hashtable hashtable = new Hashtable();
						hashtable["notification"] = new Hashtable();
						Hashtable hashtable2 = new Hashtable();
						hashtable2["data"] = hashtable;
						hashtable2["hash"] = hashCode;
						hashtable2["name"] = "dismiss";
						string json = JsonMapper.ToJson(hashtable2);
						instance.HandleNativeEvent(json);
					}
				}
				else
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log(string.Format("PlayHaven: content preload request (id={0}, placement={1})", hashCode, mPlacement));
					}
					_PlayHavenPreloadRequest(hashCode, token, secret, mPlacement);
				}
			}

			public void TriggerEvent(string eventName, JsonData eventData)
			{
				if (string.Compare(eventName, "gotcontent") == 0)
				{
					Debug.Log("PlayHaven: Preloaded content");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnSuccess(this, eventData);
				}
				else if (string.Compare(eventName, "error") == 0)
				{
					Debug.LogError("PlayHaven: Content error!");
					if (Debug.isDebugBuild)
					{
						Debug.Log("JSON (trigger event): " + eventData.ToJson());
					}
					OnError(this, eventData);
				}
			}
		}

		public delegate void SuccessHandler(IPlayHavenRequest request, JsonData responseData);

		public delegate void ErrorHandler(IPlayHavenRequest request, JsonData errorData);

		public delegate void RewardHandler(IPlayHavenRequest request, JsonData rewardData);

		public delegate void PurchaseHandler(IPlayHavenRequest request, JsonData purchaseData);

		public delegate void DismissHandler(IPlayHavenRequest request, JsonData dismissData);

		public delegate void GeneralHandler(IPlayHavenRequest request);

		public static string token;

		public static string secret;

		public static IPlayHavenListener listener;

		protected static Hashtable sRequests = new Hashtable();

		private static bool optOutStatus;

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _PlayHavenCancelRequest(int hash);
#else
		private static void _PlayHavenCancelRequest(int hash)
		{
			PcNative.Stub("_PlayHavenCancelRequest");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _PlayHavenProductPurchaseResolution(int action);
#else
		private static void _PlayHavenProductPurchaseResolution(int action)
		{
			PcNative.Stub("_PlayHavenProductPurchaseResolution");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _PlayHavenIAPTrackingRequest(string token, string secret, string productId, int quantity, int resolution);
#else
		private static void _PlayHavenIAPTrackingRequest(string token, string secret, string productId, int quantity, int resolution)
		{
			PcNative.Stub("_PlayHavenIAPTrackingRequest");
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern bool _PlayHavenOptOutStatus();
#else
		private static bool _PlayHavenOptOutStatus()
		{
			PcNative.Stub("_PlayHavenOptOutStatus");
			return default(bool);
		}
#endif

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _PlayHavenSetOptOutStatus(bool yesOrNo);
#else
		private static void _PlayHavenSetOptOutStatus(bool yesOrNo)
		{
			PcNative.Stub("_PlayHavenSetOptOutStatus");
		}
#endif

		public void Dispose()
		{
		}

		public static void SetKeys(string token, string secret)
		{
			PlayHavenBinding.token = token;
			PlayHavenBinding.secret = secret;
		}

		public static bool OptOutStatus
		{
			get
			{
				if (Application.isEditor)
				{
					return optOutStatus;
				}
				return _PlayHavenOptOutStatus();
			}
			set
			{
				if (Application.isEditor)
				{
					optOutStatus = value;
				}
				else
				{
					_PlayHavenSetOptOutStatus(value);
				}
			}
		}

		public static int Open()
		{
			return SendRequest(RequestType.Open, string.Empty);
		}

		public static int Open(string customUDID)
		{
			return SendRequest(RequestType.Open, customUDID);
		}

		public static void CancelRequest(int requestId)
		{
			if (Application.isEditor)
			{
				Debug.Log("PlayHaven: cancel request for request code = " + requestId);
				PlayHavenManager instance = PlayHavenManager.instance;
				if (instance != null)
				{
					instance.RequestCancelSuccess(requestId.ToString());
				}
			}
			else
			{
				_PlayHavenCancelRequest(requestId);
			}
		}

		public static void SendProductPurchaseResolution(PurchaseResolution resolution)
		{
			if (!Application.isEditor)
			{
				_PlayHavenProductPurchaseResolution((int)resolution);
			}
		}

		public static void SendIAPTrackingRequest(Purchase purchase, PurchaseResolution resolution)
		{
			if (!Application.isEditor)
			{
				_PlayHavenIAPTrackingRequest(token, secret, purchase.productIdentifier, purchase.quantity, (int)resolution);
			}
		}

		public static int SendRequest(RequestType type, string placement)
		{
			return SendRequest(type, placement, false);
		}

		public static int SendRequest(RequestType type, string placement, bool showsOverlayImmediately)
		{
			IPlayHavenRequest playHavenRequest = null;
			switch (type)
			{
			case RequestType.Open:
				playHavenRequest = new OpenRequest(placement);
				playHavenRequest.OnSuccess += HandleOpenRequestOnSuccess;
				playHavenRequest.OnError += HandleOpenRequestOnError;
				break;
			case RequestType.Metadata:
				playHavenRequest = new MetadataRequest(placement);
				playHavenRequest.OnSuccess += HandleMetadataRequestOnSuccess;
				playHavenRequest.OnError += HandleMetadataRequestOnError;
				playHavenRequest.OnWillDisplay += HandleMetadataRequestOnWillDisplay;
				playHavenRequest.OnDidDisplay += HandleMetadataRequestOnDidDisplay;
				break;
			case RequestType.Content:
				playHavenRequest = new ContentRequest(placement);
				playHavenRequest.OnError += HandleContentRequestOnError;
				playHavenRequest.OnDismiss += HandleContentRequestOnDismiss;
				playHavenRequest.OnReward += HandleContentRequestOnReward;
				playHavenRequest.OnPurchasePresented += HandleRequestOnPurchasePresented;
				playHavenRequest.OnWillDisplay += HandleContentRequestOnWillDisplay;
				playHavenRequest.OnDidDisplay += HandleContentRequestOnDidDisplay;
				break;
			case RequestType.Preload:
				playHavenRequest = new ContentPreloadRequest(placement);
				playHavenRequest.OnError += HandleContentRequestOnError;
				playHavenRequest.OnSuccess += HandlePreloadRequestOnSuccess;
				break;
			case RequestType.CrossPromotionWidget:
				playHavenRequest = new ContentRequest("more_games");
				playHavenRequest.OnError += HandleCrossPromotionWidgetRequestOnError;
				playHavenRequest.OnDismiss += HandleCrossPromotionWidgetRequestOnDismiss;
				playHavenRequest.OnWillDisplay += HandleCrossPromotionWidgetRequestOnWillDisplay;
				playHavenRequest.OnDidDisplay += HandleCrossPromotionWidgetRequestOnDidDisplay;
				break;
			}
			if (playHavenRequest != null)
			{
				playHavenRequest.Send(showsOverlayImmediately);
				return playHavenRequest.HashCode;
			}
			return 0;
		}

		private static void HandlePreloadRequestOnSuccess(IPlayHavenRequest request, JsonData responseData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyPreloadSuccess(request.HashCode);
			}
		}

		private static Error CreateErrorFromJSON(JsonData errorData)
		{
			Error error = new Error();
			try
			{
				error.code = (int)errorData["code"];
			}
			catch (Exception ex)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex.Message);
				}
			}
			try
			{
				error.description = (string)errorData["description"];
			}
			catch (Exception ex2)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex2.Message);
				}
			}
			return error;
		}

		private static void HandleCrossPromotionWidgetRequestOnDismiss(IPlayHavenRequest request, JsonData dismissData)
		{
			if (listener != null)
			{
				listener.NotifyCrossPromotionWidgetDismissed();
			}
		}

		private static void HandleCrossPromotionWidgetRequestOnWillDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyWillDisplayContent(request.HashCode);
			}
		}

		private static void HandleCrossPromotionWidgetRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyDidDisplayContent(request.HashCode);
			}
		}

		private static void HandleCrossPromotionWidgetRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyCrossPromotionWidgetError(request.HashCode, error);
			}
		}

		private static void HandleContentRequestOnDismiss(IPlayHavenRequest request, JsonData dismissData)
		{
			DismissType dismissType = DismissType.Unknown;
			try
			{
				string value = (string)dismissData["type"];
				dismissType = (DismissType)(int)Enum.Parse(typeof(DismissType), value);
			}
			catch (Exception ex)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex.Message);
				}
			}
			if (listener != null)
			{
				listener.NotifyContentDismissed(request.HashCode, dismissType);
			}
		}

		private static void HandleContentRequestOnWillDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyWillDisplayContent(request.HashCode);
			}
		}

		private static void HandleContentRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyDidDisplayContent(request.HashCode);
			}
		}

		private static void HandleContentRequestOnReward(IPlayHavenRequest request, JsonData rewardData)
		{
			Reward reward = new Reward();
			try
			{
				reward.receipt = (string)rewardData["receipt"];
			}
			catch (Exception ex)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex.Message);
				}
			}
			try
			{
				reward.name = (string)rewardData["name"];
				reward.quantity = (int)rewardData["quantity"];
				if (listener != null)
				{
					listener.NotifyRewardGiven(request.HashCode, reward);
				}
			}
			catch (Exception ex2)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex2.Message);
				}
			}
		}

		private static void HandleRequestOnPurchasePresented(IPlayHavenRequest request, JsonData purchaseData)
		{
			Purchase purchase = new Purchase();
			try
			{
				purchase.receipt = (string)purchaseData["receipt"];
			}
			catch (Exception ex)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex.Message);
				}
			}
			try
			{
				purchase.productIdentifier = (string)purchaseData["productIdentifier"];
				purchase.quantity = (int)purchaseData["quantity"];
				if (listener != null)
				{
					listener.NotifyPurchasePresented(request.HashCode, purchase);
				}
			}
			catch (Exception ex2)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex2.Message);
				}
			}
		}

		private static void HandleContentRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyContentError(request.HashCode, error);
			}
		}

		private static void HandleMetadataRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyMetaDataError(request.HashCode, error);
			}
		}

		private static void HandleMetadataRequestOnSuccess(IPlayHavenRequest request, JsonData responseData)
		{
			string text;
			string badge;
			try
			{
				text = (string)responseData["notification"]["type"];
			}
			catch (Exception ex)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(ex.Message);
				}
				text = string.Empty;
			}
			if (text == "badge")
			{
				try
				{
					badge = (string)responseData["notification"]["value"];
					if (listener != null)
					{
						listener.NotifyBadgeUpdate(request.HashCode, badge);
					}
				}
				catch (Exception ex2)
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log(ex2.Message);
					}
				}
			}
		}


		private static void HandleMetadataRequestOnWillDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyWillDisplayContent(request.HashCode);
			}
		}

		private static void HandleMetadataRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (listener != null)
			{
				listener.NotifyDidDisplayContent(request.HashCode);
			}
		}

		private static void HandleOpenRequestOnError(IPlayHavenRequest request, JsonData errorData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				Error error = CreateErrorFromJSON(errorData);
				listener.NotifyOpenError(request.HashCode, error);
			}
		}

		private static void HandleOpenRequestOnSuccess(IPlayHavenRequest request, JsonData responseData)
		{
			if (listener != null)
			{
				listener.NotifyRequestCompleted(request.HashCode);
				listener.NotifyOpenSuccess(request.HashCode);
			}
		}

		public static IPlayHavenRequest GetRequestWithHash(int hash)
		{
			if (sRequests.ContainsKey(hash))
			{
				return (IPlayHavenRequest)sRequests[hash];
			}
			return null;
		}

		public static void ClearRequestWithHash(int hash)
		{
			if (sRequests.ContainsKey(hash))
			{
				sRequests.Remove(hash);
				if (Debug.isDebugBuild)
				{
					Debug.Log(string.Format("Cleared request (id={0})", hash));
				}
			}
		}
	}
}
