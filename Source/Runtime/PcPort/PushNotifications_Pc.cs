// PC 桥接：推送通知的空实现。
// 原版的 PushNotifications_Cocoa 依赖 iOS 的 LocalNotification / RemoteNotification，
// 这些类型在新版 Unity 的非 iOS 平台上已不存在。PC 不需要推送，这里沿用同一个类名，
// 让 PushNotifications.GetInstance() 无需改动；本地通知只保存字段，不会真正弹出。
#if !UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using Glu.PushNotifications.Remote;
using UnityEngine;

public class PushNotifications_Cocoa : PushNotifications
{
	/// <summary>只保存字段的本地通知，供游戏代码照常读写。</summary>
	private class LocalNotification_Pc : ILocalNotification
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
		return new LocalNotification_Pc();
	}

	public override void UpdateRemoteNotificationsSubscription(string server, List<string> messageTypes, bool isEnabled, EventHandler<RemoteNotificationResponseEventArgs> resultHandler)
	{
	}

	public override void CreateRemoteNotification(string server, string receiverToken, string messageType, string messageText, EventHandler<RemoteNotificationResponseEventArgs> resultHandler)
	{
	}

	public override void RegisterForRemoteNotificationTypes(int notificationTypes, string urbanAirshipKey, string urbanAirshipSecret, string silentTimeStart, string silentTimeEnd, List<string> tags)
	{
		Debug.Log("[PcPort] PushNotifications: remote notifications are disabled on PC");
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
