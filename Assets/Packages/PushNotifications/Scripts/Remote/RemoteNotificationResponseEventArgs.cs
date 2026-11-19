using System;

namespace Glu.PushNotifications.Remote
{
	public class RemoteNotificationResponseEventArgs : EventArgs
	{
		public string result { get; private set; }

		public RemoteNotificationResponseEventArgs(string result)
		{
			this.result = result;
		}
	}
}
