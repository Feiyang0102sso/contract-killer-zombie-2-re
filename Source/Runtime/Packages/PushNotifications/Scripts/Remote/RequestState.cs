using System;
using System.Net;

namespace Glu.PushNotifications.Remote
{
	public class RequestState
	{
		public HttpWebRequest webRequest { private set; get; }

		public EventHandler<RemoteNotificationResponseEventArgs> handler { private set; get; }

		public RequestState(HttpWebRequest webRequest, EventHandler<RemoteNotificationResponseEventArgs> handler)
		{
			this.webRequest = webRequest;
			this.handler = handler;
		}
	}
}
