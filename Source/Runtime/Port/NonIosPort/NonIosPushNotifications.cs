// Non-iOS port: empty push notification implementation.
// The original PushNotifications_Cocoa depends on iOS LocalNotification / RemoteNotification, which do not exist on
// non-iOS platforms in newer Unity. The class keeps the original name PushNotifications_Cocoa so that
// PushNotifications.GetInstance() stays unchanged; local notifications only store their fields and never fire.
#if !UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using Glu.PushNotifications.Remote;
using UnityEngine;

public class PushNotifications_Cocoa : PushNotifications
{
	/// <summary>Local notification that only stores its fields, so game code can read and write them as usual.</summary>
	private class NonIosLocalNotification : ILocalNotification
	{
		public DateTime fireDate { get; set; }

		public string timeZone { get; set; }

		public string alertBody { get; set; }

		public string alertAction { get; set; }

		public bool hasAction { get; set; }

		public string alertLaunchImage { get; set; }

		public int applicationIconBadgeNumber { get; set; }

		public string soundName { get; set; }

		public IDictionary userInfo { get; set; }
	}

	public override ILocalNotification CreateLocalNotification()
	{
		return new NonIosLocalNotification();
	}

	public override void UpdateRemoteNotificationsSubscription(string server, List<string> messageTypes, bool isEnabled, EventHandler<RemoteNotificationResponseEventArgs> resultHandler)
	{
	}

	public override void CreateRemoteNotification(string server, string receiverToken, string messageType, string messageText, EventHandler<RemoteNotificationResponseEventArgs> resultHandler)
	{
	}

	public override void RegisterForRemoteNotificationTypes(int notificationTypes, string urbanAirshipKey, string urbanAirshipSecret, string silentTimeStart, string silentTimeEnd, List<string> tags)
	{
		Debug.Log("[NonIosPushNotifications] remote notifications are disabled");
	}

	public override void UnregisterForRemoteNotifications()
	{
	}

	public override int GetEnabledRemoteNotificationTypes()
	{
		return NOTIFICATION_TYPE_NONE;
	}

	public override string GetDeviceToken()
	{
		return null;
	}

	public override string GetRegistrationError()
	{
		return null;
	}

	public override int GetLocalNotificationCount()
	{
		return 0;
	}

	public override ILocalNotification GetLocalNotification(int index)
	{
		return null;
	}

	public override ILocalNotification[] GetScheduledLocalNotifications()
	{
		return new ILocalNotification[0];
	}

	public override void ScheduleLocalNotification(ILocalNotification notification)
	{
	}

	public override void PresentLocalNotificationNow(ILocalNotification notification)
	{
	}

	public override void CancelLocalNotification(ILocalNotification notification)
	{
	}

	public override void CancelAllLocalNotifications()
	{
	}

	public override void ClearLocalNotifications()
	{
	}

	public override int GetRemoteNotificationCount()
	{
		return 0;
	}

	public override IRemoteNotification GetRemoteNotification(int index)
	{
		return null;
	}

	public override void ClearRemoteNotifications()
	{
	}

	public override int GetBadgeNumber()
	{
		return 0;
	}

	public override void SetBadgeNumber(int number)
	{
	}
}
#endif
