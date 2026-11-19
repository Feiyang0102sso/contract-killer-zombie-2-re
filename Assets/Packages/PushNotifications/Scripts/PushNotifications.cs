using System;
using System.Collections;
using System.Collections.Generic;
using Glu.PushNotifications.Remote;

public abstract class PushNotifications
{
	public interface ILocalNotification
	{
		DateTime fireDate { set; get; }

		string timeZone { set; get; }

		string alertBody { set; get; }

		string alertAction { set; get; }

		bool hasAction { set; get; }

		string alertLaunchImage { set; get; }

		int applicationIconBadgeNumber { set; get; }

		string soundName { set; get; }

		IDictionary userInfo { set; get; }
	}

	public interface IRemoteNotification
	{
		string alertBody { get; }

		bool hasAction { get; }

		int applicationIconBadgeNumber { get; }

		string soundName { get; }

		IDictionary userInfo { get; }
	}

	public const int NOTIFICATION_TYPE_NONE = 0;

	public const int NOTIFICATION_TYPE_ALERT = 1;

	public const int NOTIFICATION_TYPE_BADGE = 2;

	public const int NOTIFICATION_TYPE_SOUND = 4;

	private static PushNotifications m_instance = null;

	public static PushNotifications GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new PushNotifications_Cocoa();
		}
		return m_instance;
	}

	public abstract ILocalNotification CreateLocalNotification();

	public abstract void UpdateRemoteNotificationsSubscription(string server, List<string> messageTypes, bool isEnabled, EventHandler<RemoteNotificationResponseEventArgs> resultHandler);

	public abstract void CreateRemoteNotification(string server, string receiverToken, string messageType, string messageText, EventHandler<RemoteNotificationResponseEventArgs> resultHandler);

	public abstract void RegisterForRemoteNotificationTypes(int notificationTypes, string urbanAirshipKey, string urbanAirshipSecret, string silentTimeStart, string silentTimeEnd, List<string> tags);

	public abstract void UnregisterForRemoteNotifications();

	public abstract int GetEnabledRemoteNotificationTypes();

	public abstract string GetDeviceToken();

	public abstract string GetRegistrationError();

	public abstract int GetLocalNotificationCount();

	public abstract ILocalNotification GetLocalNotification(int index);

	public abstract ILocalNotification[] GetScheduledLocalNotifications();

	public abstract void ScheduleLocalNotification(ILocalNotification notification);

	public abstract void PresentLocalNotificationNow(ILocalNotification notification);

	public abstract void CancelLocalNotification(ILocalNotification notification);

	public abstract void CancelAllLocalNotifications();

	public abstract void ClearLocalNotifications();

	public abstract int GetRemoteNotificationCount();

	public abstract IRemoteNotification GetRemoteNotification(int index);

	public abstract void ClearRemoteNotifications();

	public abstract int GetBadgeNumber();

	public abstract void SetBadgeNumber(int number);
}
