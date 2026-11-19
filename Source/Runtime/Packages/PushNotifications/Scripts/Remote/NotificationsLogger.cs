namespace Glu.PushNotifications.Remote
{
	public sealed class NotificationsLogger : LoggerSingleton<NotificationsLogger>
	{
		public NotificationsLogger()
		{
			LoggerSingleton<NotificationsLogger>.SetLoggerName("Package.PushNotifications.Remote");
		}
	}
}
