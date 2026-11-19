using Glu;
using Glu.Localization;
using UnityEngine;

public class FacebookManager
{
	public static bool WasLoggedIn()
	{
		return Singleton.PlayerData.FacebookLoggedInOnce;
	}

	public static void Login()
	{
		if (!CheckNetworkConnection())
		{
			return;
		}
		if (!Facebook.IsLoggedIn())
		{
			Facebook.Login();
			GameAnalytics.LogFacebookLogin();
		}
	}

	public static void Chat()
	{
		if (!CheckNetworkConnection())
		{
			return;
		}
		if (Facebook.IsLoggedIn())
		{
			Application.OpenURL("http://www.facebook.com/ContractKillerZombies?fref=ts");
		}
	}

	public static void CheckRewardForFirstLogin()
	{
		if (!Facebook.IsLoggedIn())
		{
			return;
		}
		if (Singleton.PlayerData.FacebookLoggedInOnce)
		{
			return;
		}
		int facebookRewardHC = Singleton.GameData.FacebookRewardHC;
		Singleton.PlayerData.FacebookLoggedInOnce = true;
		Singleton.PlayerData.HardCurrency += facebookRewardHC;
		Singleton.UI.PopupManager.ShowFacebookRewardGivenPopup();
		GameAnalytics.LogHCReceived(facebookRewardHC, GameAnalytics.HCSourceType.Game);
	}

	public static bool PostSMResultMessageToWall(int score, string modeName)
	{
		GameAnalytics.LogFacebookSurvivalPressed();
		return PostMessageToWall(Strings.GetParticularString("Facebook", "Contract Killer: Zombies 2"), Strings.GetParticularStringFormat("Facebook", "Got  {0} score in {1}", score, modeName), Strings.GetParticularString("Facebook", "Sneak your way in, shoot your way out!"));
	}

	public static bool PostLevelUpMessageToWall(int level)
	{
		GameAnalytics.LogFacebookLevelUpPressed();
		return PostMessageToWall(Strings.GetParticularString("Facebook", "Contract Killer: Zombies 2"), Strings.GetParticularStringFormat("Facebook", "Completed a mission and reached Level {0}", level), Strings.GetParticularString("Facebook", "Sneak your way in, shoot your way out!"));
	}

	private static bool ChallengeFriends(string request, string notification, string dataFirst, string dataSecond)
	{
		request = string.Format(request, dataFirst, dataSecond);
		notification = string.Format(notification, dataFirst, dataSecond);
		Facebook.SendRequestToFriend(request, notification, true);
		return Facebook.IsLoggedIn();
	}

	private static bool PostMessageToWall(string name, string caption, string description)
	{
		if (!CheckNetworkConnection())
		{
			return false;
		}
		Facebook.PostLinkToWall("http://www.glu.com/game/contract-killer-zombies-2", "https://s3.amazonaws.com/glu-facebook/CKZ2_1.1_512icon.png", name, caption, description, true);
		return Facebook.IsLoggedIn();
	}

	private static bool CheckNetworkConnection()
	{
		bool flag = App.Instance.GetNetworkReachability() != NetworkReachability.NotReachable;
		if (!flag)
		{
			Singleton.UI.PopupManager.ShowInfoPopup(Strings.GetString("Unable to connect to Facebook. Please check your connection and try again."));
		}
		return flag;
	}
}
