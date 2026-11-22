#if UNITY_IOS
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Glu.PushNotifications.Remote;
using UnityEngine;

public class PushNotifications_Cocoa : PushNotifications
{
	private class LocalNotification_Cocoa : ILocalNotification
	{
		private LocalNotification notification;

		public DateTime fireDate
		{
			set
			{
				notification.fireDate = value;
			}
			get
			{
				return notification.fireDate;
			}
		}

		public string timeZone
		{
			set
			{
				notification.timeZone = value;
			}
			get
			{
				return notification.timeZone;
			}
		}

		public string alertBody
		{
			set
			{
				notification.alertBody = value;
			}
			get
			{
				return notification.alertBody;
			}
		}

		public string alertAction
		{
			set
			{
				notification.alertAction = value;
			}
			get
			{
				return notification.alertAction;
			}
		}

		public bool hasAction
		{
			set
			{
				notification.hasAction = value;
			}
			get
			{
				return notification.hasAction;
			}
		}

		public string alertLaunchImage
		{
			set
			{
				notification.alertLaunchImage = value;
			}
			get
			{
				return notification.alertLaunchImage;
			}
		}

		public int applicationIconBadgeNumber
		{
			set
			{
				notification.applicationIconBadgeNumber = value;
			}
			get
			{
				return notification.applicationIconBadgeNumber;
			}
		}

		public string soundName
		{
			set
			{
				notification.soundName = value;
			}
			get
			{
				return notification.soundName;
			}
		}

		public IDictionary userInfo
		{
			set
			{
				notification.userInfo = value;
			}
			get
			{
				return notification.userInfo;
			}
		}

		public LocalNotification_Cocoa()
		{
			notification = new LocalNotification();
		}

		public LocalNotification_Cocoa(LocalNotification ln)
		{
			notification = ln;
		}

		public LocalNotification GetLocalNotificationObject()
		{
			return notification;
		}
	}

	private class RemoteNotification_Cocoa : IRemoteNotification
	{
		private RemoteNotification notification;

		public string alertBody
		{
			get
			{
				return notification.alertBody;
			}
		}

		public bool hasAction
		{
			get
			{
				return notification.hasAction;
			}
		}

		public int applicationIconBadgeNumber
		{
			get
			{
				return notification.applicationIconBadgeNumber;
			}
		}

		public string soundName
		{
			get
			{
				return notification.soundName;
			}
		}

		public IDictionary userInfo
		{
			get
			{
				return notification.userInfo;
			}
		}

		public RemoteNotification_Cocoa(RemoteNotification rn)
		{
			notification = rn;
		}

		public RemoteNotification GetRemoteNotificationObject()
		{
			return notification;
		}
	}

	private string m_uaKey;

	private string m_uaSecret;

	private string m_silentStart;

	private string m_silentEnd;

	private List<string> tags;

	public override ILocalNotification CreateLocalNotification()
	{
		return new LocalNotification_Cocoa();
	}

	public override void CreateRemoteNotification(string server, string receiverToken, string messageType, string messageText, EventHandler<RemoteNotificationResponseEventArgs> resultHandler)
	{
		RemoteNotificationsManager.Send(server, receiverToken, messageType, messageText, resultHandler);
	}

	public override void UpdateRemoteNotificationsSubscription(string server, List<string> messageTypes, bool isEnabled, EventHandler<RemoteNotificationResponseEventArgs> resultHandler)
	{
		string deviceToken = GetDeviceToken();
		if (string.IsNullOrEmpty(deviceToken))
		{
			throw new ArgumentException("UpdateRemoteNotificationsSubscription: Initialize first");
		}
		RemoteNotificationsManager.UpdateSubscription(server, deviceToken, messageTypes, isEnabled, resultHandler);
	}

	public override void RegisterForRemoteNotificationTypes(int notificationTypes, string urbanAirshipKey, string urbanAirshipSecret, string silentTimeStart, string silentTimeEnd, List<string> tags)
	{
		m_uaKey = urbanAirshipKey;
		m_uaSecret = urbanAirshipSecret;
		m_silentStart = silentTimeStart;
		m_silentEnd = silentTimeEnd;
		if (tags != null)
		{
			this.tags = new List<string>(tags);
		}
		NotificationServices.RegisterForRemoteNotificationTypes(IntToRemoteNotificationType(notificationTypes));
		if (m_uaKey != null && m_uaSecret != null)
		{
			new Thread(WaitDeviceToken).Start();
		}
	}

	public override void UnregisterForRemoteNotifications()
	{
		NotificationServices.UnregisterForRemoteNotifications();
	}

	public override int GetEnabledRemoteNotificationTypes()
	{
		return RemoteNotificationTypeToInt(NotificationServices.enabledRemoteNotificationTypes);
	}

	public override string GetDeviceToken()
	{
		return ByteArrayToHexString(NotificationServices.deviceToken);
	}

	public override string GetRegistrationError()
	{
		return NotificationServices.registrationError;
	}

	public override int GetLocalNotificationCount()
	{
		return NotificationServices.localNotificationCount;
	}

	public override ILocalNotification GetLocalNotification(int index)
	{
		return new LocalNotification_Cocoa(NotificationServices.GetLocalNotification(index));
	}

	public override ILocalNotification[] GetScheduledLocalNotifications()
	{
		LocalNotification[] scheduledLocalNotifications = NotificationServices.scheduledLocalNotifications;
		if (scheduledLocalNotifications.GetLength(0) > 0)
		{
			LocalNotification_Cocoa[] array = new LocalNotification_Cocoa[scheduledLocalNotifications.GetLength(0)];
			for (int i = 0; i < scheduledLocalNotifications.GetLength(0); i++)
			{
				array[i] = new LocalNotification_Cocoa(NotificationServices.scheduledLocalNotifications[i]);
			}
			return array;
		}
		return null;
	}

	public override void ScheduleLocalNotification(ILocalNotification notification)
	{
		NotificationServices.ScheduleLocalNotification(((LocalNotification_Cocoa)notification).GetLocalNotificationObject());
	}

	public override void PresentLocalNotificationNow(ILocalNotification notification)
	{
		NotificationServices.PresentLocalNotificationNow(((LocalNotification_Cocoa)notification).GetLocalNotificationObject());
	}

	public override void CancelLocalNotification(ILocalNotification notification)
	{
		NotificationServices.CancelLocalNotification(((LocalNotification_Cocoa)notification).GetLocalNotificationObject());
	}

	public override void CancelAllLocalNotifications()
	{
		NotificationServices.CancelAllLocalNotifications();
	}

	public override void ClearLocalNotifications()
	{
		NotificationServices.ClearLocalNotifications();
	}

	public override int GetRemoteNotificationCount()
	{
		return NotificationServices.remoteNotificationCount;
	}

	public override IRemoteNotification GetRemoteNotification(int index)
	{
		return new RemoteNotification_Cocoa(NotificationServices.GetRemoteNotification(index));
	}

	public override void ClearRemoteNotifications()
	{
		NotificationServices.ClearRemoteNotifications();
	}

	public override int GetBadgeNumber()
	{
		return PushNotificationsGetBadgeNumber();
	}

	public override void SetBadgeNumber(int number)
	{
		PushNotificationsSetBadgeNumber(number);
	}

	private void WaitDeviceToken()
	{
		while (GetDeviceToken() == null && GetRegistrationError() == null)
		{
			Debug.Log("Thread - wait device token");
			Thread.Sleep(5000);
		}
		try
		{
			if (GetRegistrationError() == null)
			{
				string requestUriString = "https://go.urbanairship.com/api/device_tokens/" + GetDeviceToken();
				CredentialCache credentialCache = new CredentialCache();
				credentialCache.Add(new Uri("https://go.urbanairship.com"), "Basic", new NetworkCredential(m_uaKey, m_uaSecret));
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				httpWebRequest.Method = "PUT";
				httpWebRequest.Credentials = credentialCache;
				bool flag = true;
				StringBuilder stringBuilder = new StringBuilder("{");
				if (tags != null && tags.Count > 0)
				{
					flag = false;
					stringBuilder.Append("\"tags\": [");
					for (int i = 0; i < tags.Count; i++)
					{
						stringBuilder.Append("\"" + tags[i] + "\"" + (i == tags.Count - 1 ? string.Empty : ","));
					}
					stringBuilder.Append("]");
				}
				if (m_silentStart != null && m_silentEnd != null)
				{
					if (!flag)
					{
						stringBuilder.Append(",");
					}
					StringBuilder stringBuilder2 = new StringBuilder(256);
					PushNotificationsGetTimezoneOlson(stringBuilder2, stringBuilder2.Capacity);
					if (stringBuilder2.Length > 0)
					{
						httpWebRequest.ContentType = "application/json";
						stringBuilder.AppendFormat("\"quiettime\":{{\"start\":\"{0}\",\"end\":\"{1}\"}},\"tz\":\"{2}\"", m_silentStart, m_silentEnd, stringBuilder2.ToString());
					}
					flag = false;
				}
				if (!flag)
				{
					stringBuilder.Append("}");
					using (Stream stream = httpWebRequest.GetRequestStream())
					{
						using (StreamWriter streamWriter = new StreamWriter(stream))
						{
							streamWriter.Write(stringBuilder.ToString());
						}
					}
				}
				httpWebRequest.BeginGetResponse(DidReceiveUrbanAirshipResponse, httpWebRequest);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("WaitDeviceToken exception " + ex.ToString());
			throw;
		}
	}

	private void DidReceiveUrbanAirshipResponse(IAsyncResult result)
	{
		try
		{
			HttpWebResponse httpWebResponse = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
			using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
			{
				string text = streamReader.ReadToEnd();
				Debug.Log("HTTP response - " + text);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("DidReceiveUrbanAirshipResponse exception " + ex.ToString());
			throw;
		}
	}

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern int PushNotificationsGetBadgeNumber();
#else
	private static int PushNotificationsGetBadgeNumber()
	{
		NonIosNative.Stub("PushNotificationsGetBadgeNumber");
		return default(int);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern void PushNotificationsSetBadgeNumber(int val);
#else
	private static void PushNotificationsSetBadgeNumber(int val)
	{
		NonIosNative.Stub("PushNotificationsSetBadgeNumber");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern void PushNotificationsGetTimezoneOlson(StringBuilder dest, int n);
#else
	private static void PushNotificationsGetTimezoneOlson(StringBuilder dest, int n)
	{
		NonIosNative.Stub("PushNotificationsGetTimezoneOlson");
	}
#endif

	private int RemoteNotificationTypeToInt(RemoteNotificationType rnt)
	{
		int num = 0;
		if ((rnt & RemoteNotificationType.Alert) != 0)
		{
			num |= 1;
		}
		if ((rnt & RemoteNotificationType.Badge) != 0)
		{
			num |= 2;
		}
		if ((rnt & RemoteNotificationType.Sound) != 0)
		{
			num |= 4;
		}
		return num;
	}

	private RemoteNotificationType IntToRemoteNotificationType(int t)
	{
		RemoteNotificationType remoteNotificationType = RemoteNotificationType.None;
		if ((t & 1) != 0)
		{
			remoteNotificationType |= RemoteNotificationType.Alert;
		}
		if ((t & 2) != 0)
		{
			remoteNotificationType |= RemoteNotificationType.Badge;
		}
		if ((t & 4) != 0)
		{
			remoteNotificationType |= RemoteNotificationType.Sound;
		}
		return remoteNotificationType;
	}

	private string ByteArrayToHexString(byte[] src)
	{
		if (src != null)
		{
			char[] array = new char[src.Length * 2];
			byte b;
			for (int i = 0; i < src.Length; i++)
			{
				b = (byte)(src[i] >> 4);
				array[i * 2] = (char)(b > 9 ? b + 55 : b + 48);
				b = (byte)(src[i] & 0xF);
				array[i * 2 + 1] = (char)(b > 9 ? b + 55 : b + 48);
			}
			return new string(array);
		}
		return null;
	}
}

#endif
