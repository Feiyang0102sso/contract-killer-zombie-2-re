using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Glu.PushNotifications.Remote
{
	public class RemoteNotificationsManager
	{
		public const string InternalError = "InternalError";

		private static void RemoteNotificationCallback(IAsyncResult result)
		{
			RequestState requestState = result.AsyncState as RequestState;
			try
			{
				HttpWebResponse httpWebResponse = requestState.webRequest.EndGetResponse(result) as HttpWebResponse;
				string result2 = string.Empty;
				using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
				{
					result2 = streamReader.ReadToEnd();
				}
				if (requestState.handler != null)
				{
					requestState.handler(null, new RemoteNotificationResponseEventArgs(result2));
				}
			}
			catch (Exception e)
			{
				if (requestState.handler != null)
				{
					requestState.handler(null, new RemoteNotificationResponseEventArgs("InternalError"));
				}
			}
		}

		private static void DoSend(string url, EventHandler<RemoteNotificationResponseEventArgs> resultCallback)
		{
			ThreadPool.QueueUserWorkItem((object state) =>
			{
				try
				{
					HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
					httpWebRequest.BeginGetResponse(RemoteNotificationCallback, new RequestState(httpWebRequest, resultCallback));
				}
				catch (Exception e)
				{
				}
			});
		}

		public static void Send(string server, string receiverToken, string messageType, string messageText, EventHandler<RemoteNotificationResponseEventArgs> resultCallback)
		{
			string unusedRequestUrl;
			if (string.IsNullOrEmpty(messageType) || string.IsNullOrEmpty(messageText) || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(receiverToken))
			{
				return;
			}
			DoSend(string.Format(server.TrimEnd('/') + "/SendNotification.php?token={0}&messageType={1}&messageText={2}", receiverToken, messageType, messageText), resultCallback);
		}

		public static void UpdateSubscription(string server, string myToken, List<string> messageTypes, bool isEnabled, EventHandler<RemoteNotificationResponseEventArgs> resultCallback)
		{
			if (messageTypes == null || messageTypes.Count == 0 || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(myToken))
			{
				return;
			}
			string unusedSubscriptionUrl;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < messageTypes.Count; i++)
			{
				stringBuilder.AppendFormat(i == messageTypes.Count - 1 ? "{0}" : "{0},", messageTypes[i]);
			}
			DoSend(string.Format(server.TrimEnd('/') + "/UpdateSubscription.php?token={0}&messageTypes={1}&isEnabled={2}", stringBuilder.ToString(), isEnabled.ToString()), resultCallback);
		}
	}
}
