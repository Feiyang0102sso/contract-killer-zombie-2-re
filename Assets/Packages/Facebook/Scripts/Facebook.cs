using System;
using System.Runtime.InteropServices;

namespace Glu
{
	public static class Facebook
	{
		private class MonoPInvokeCallbackAttribute : Attribute
		{
			private Type type;

			public MonoPInvokeCallbackAttribute(Type t)
			{
				type = t;
			}
		}

		private static class platform
		{
			public static void Init()
			{
				FacebookInitialize(LoginHandler, UserInfoHandler);
			}

			public static bool IsLoggedIn()
			{
				return FacebookUserLoggedIn();
			}

			public static void Login(string permissions)
			{
				FacebookLogin(permissions);
			}

			public static void Logout()
			{
				FacebookLogout();
			}

			public static void GetUserInfo()
			{
				FacebookRequestUserInfo();
			}

			public static void SendRequestToFriend(string requestText, string notificationText)
			{
				FacebookAppRequestToFriends(requestText, notificationText);
			}

			public static void PostMessageToWall(string messageText)
			{
				FacebookPostMessageToFeed(messageText);
			}

			public static void PostLinkToWall(string linkUrl, string pictureUrl, string name, string caption, string descriptionText)
			{
				FacebookPostLinkToFeed(linkUrl, pictureUrl, name, caption, descriptionText);
			}

			public static void PostHighScoreToFeed(int highscore)
			{
				FacebookPublishScore(highscore);
			}

			[DllImport("__Internal")]
			private static extern void FacebookInitialize(LoginCallback callback, UserInfoCallback callback2);

			[DllImport("__Internal")]
			private static extern bool FacebookUserLoggedIn();

			[DllImport("__Internal")]
			private static extern void FacebookLogin(string permissions);

			[DllImport("__Internal")]
			private static extern void FacebookLogout();

			[DllImport("__Internal")]
			private static extern void FacebookRequestUserInfo();

			[DllImport("__Internal", CharSet = CharSet.Ansi)]
			private static extern void FacebookPostMessageToFeed(string postData);

			[DllImport("__Internal", CharSet = CharSet.Ansi)]
			private static extern void FacebookPostLinkToFeed(string link, string picture, string name, string caption, string description);

			[DllImport("__Internal", CharSet = CharSet.Ansi)]
			private static extern void FacebookAppRequestToFriends(string requestText, string notificationText);

			[DllImport("__Internal")]
			private static extern void FacebookPublishScore(int score);
		}

		private delegate void LoginCallback(bool success, string token);

		private delegate void UserInfoCallback(bool success, ulong ID, string name, string firstName);

		private static Action loginDelayedCall;

		private static Action userInfoDelayedCall;

		private static Action<string, string> userInfoCallback;

		public static ulong? UserID { get; private set; }

		public static string UserName { get; private set; }

		public static string UserFirstName { get; private set; }

		public static string AccessToken { get; private set; }

		static Facebook()
		{
			platform.Init();
		}

		public static bool IsLoggedIn()
		{
			return platform.IsLoggedIn();
		}

		public static void Login()
		{
			if (!platform.IsLoggedIn())
			{
				platform.Login("email");
			}
		}

		public static void Logout()
		{
			if (platform.IsLoggedIn())
			{
				platform.Logout();
			}
		}

		public static void GetUserInfo(Action<string, string> callback)
		{
			if (platform.IsLoggedIn())
			{
				userInfoCallback = callback;
				platform.GetUserInfo();
			}
		}

		public static void GetUserInfo(Action<string, string> callback, bool loginIfNeeded)
		{
			if (platform.IsLoggedIn())
			{
				userInfoCallback = callback;
				platform.GetUserInfo();
			}
			else if (loginIfNeeded)
			{
				userInfoCallback = callback;
				Login();
			}
		}

		public static void SendRequestToFriend(string requestText, string notificationText, bool loginIfNeeded)
		{
			if (requestText == null)
			{
				throw new ArgumentNullException("Request text must not be null.");
			}
			if (requestText.Equals(string.Empty))
			{
				throw new ArgumentException("Request text must not be empty.");
			}
			if (platform.IsLoggedIn())
			{
				platform.SendRequestToFriend(requestText, notificationText);
			}
			else if (loginIfNeeded)
			{
				loginDelayedCall = () =>
				{
					platform.SendRequestToFriend(requestText, notificationText);
				};
				Login();
			}
		}

		public static void SendRequestToFriend(string requestText, string notificationText)
		{
			SendRequestToFriend(requestText, notificationText, false);
		}

		public static void PostMessageToWall(string messageText, bool loginIfNeeded)
		{
			if (messageText == null)
			{
				throw new ArgumentNullException("Message text must not be null.");
			}
			if (messageText.Equals(string.Empty))
			{
				throw new ArgumentException("Message text must not be empty.");
			}
			if (platform.IsLoggedIn())
			{
				platform.PostMessageToWall(messageText);
			}
			else if (loginIfNeeded)
			{
				loginDelayedCall = () =>
				{
					platform.PostMessageToWall(messageText);
				};
				Login();
			}
		}

		public static void PostMessageToWall(string messageText)
		{
			PostMessageToWall(messageText, false);
		}

		public static void PostLinkToWall(string linkUrl, string pictureUrl, string name, string caption, string descriptionText, bool loginIfNeeded)
		{
			if (linkUrl == null)
			{
				throw new ArgumentNullException("Link URL must not be null.");
			}
			if (linkUrl.Equals(string.Empty))
			{
				throw new ArgumentException("Link URL must not be empty.");
			}
			if (platform.IsLoggedIn())
			{
				platform.PostLinkToWall(linkUrl, pictureUrl, name, caption, descriptionText);
			}
			else if (loginIfNeeded)
			{
				loginDelayedCall = () =>
				{
					platform.PostLinkToWall(linkUrl, pictureUrl, name, caption, descriptionText);
				};
				Login();
			}
		}

		public static void PostLinkToWall(string linkUrl, string pictureUrl, string name, string caption, string descriptionText)
		{
			PostLinkToWall(linkUrl, pictureUrl, name, caption, descriptionText, false);
		}

		public static void PublishScoreToFeed(int score, bool loginIfNeeded)
		{
			if (platform.IsLoggedIn())
			{
				platform.PostHighScoreToFeed(score);
			}
			else if (loginIfNeeded)
			{
				userInfoDelayedCall = () =>
				{
					platform.PostHighScoreToFeed(score);
				};
				platform.Login("publish_actions");
			}
		}

		public static void PublishScoreToFeed(int score)
		{
			PublishScoreToFeed(score, false);
		}

		[MonoPInvokeCallback(typeof(LoginCallback))]
		private static void LoginHandler(bool success, string token)
		{
			if (success)
			{
				AccessToken = token;
				if (loginDelayedCall != null)
				{
					loginDelayedCall();
				}
				platform.GetUserInfo();
			}
			else
			{
				AccessToken = null;
				userInfoDelayedCall = null;
				if (!platform.IsLoggedIn())
				{
					UserInfoHandler(false, 0uL, null, null);
				}
			}
			loginDelayedCall = null;
		}

		[MonoPInvokeCallback(typeof(UserInfoCallback))]
		private static void UserInfoHandler(bool success, ulong ID, string name, string firstName)
		{
			if (success)
			{
				UserID = ID;
				UserName = name;
				UserFirstName = firstName;
				if (userInfoDelayedCall != null)
				{
					userInfoDelayedCall();
				}
			}
			else
			{
				UserID = null;
				UserName = null;
				UserFirstName = null;
			}
			userInfoDelayedCall = null;
			if (userInfoCallback != null)
			{
				userInfoCallback(UserName, UserFirstName);
				userInfoCallback = null;
			}
		}
	}
}
