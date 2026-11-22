// Offline: Social platform that never signs in (replaces iOS Game Center), installed on every platform.
// Outside iOS, Social.Active defaults to Unity's local implementation: sign-in always succeeds, the game then reports
// achievements and leaderboards, and the local implementation does not know those IDs, so it logs
// "Achievement ID not found" / "Leaderboard not found" (a development build pops up the console).
// The port is fully offline, so sign-in fails here (as on iOS without a Game Center login). Every report in the game
// checks Social.localUser.authenticated first, so nothing is sent; a direct call gets failure or empty results.
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class OfflineSocialPlatform : ISocialPlatform
{
	private readonly OfflineLocalUser _localUser = new OfflineLocalUser();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Install()
	{
		Social.Active = new OfflineSocialPlatform();
		Debug.Log("[OfflineSocialPlatform] installed");
	}

	public ILocalUser localUser
	{
		get
		{
			return _localUser;
		}
	}

	public void Authenticate(ILocalUser user, Action<bool> callback)
	{
		if (callback != null)
		{
			callback(false);
		}
	}

	public void Authenticate(ILocalUser user, Action<bool, string> callback)
	{
		if (callback != null)
		{
			callback(false, "offline");
		}
	}

	public void LoadFriends(ILocalUser user, Action<bool> callback)
	{
		if (callback != null)
		{
			callback(false);
		}
	}

	public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback)
	{
		if (callback != null)
		{
			callback(new IUserProfile[0]);
		}
	}

	public void ReportProgress(string achievementID, double progress, Action<bool> callback)
	{
		if (callback != null)
		{
			callback(false);
		}
	}

	public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback)
	{
		if (callback != null)
		{
			callback(new IAchievementDescription[0]);
		}
	}

	public void LoadAchievements(Action<IAchievement[]> callback)
	{
		if (callback != null)
		{
			callback(new IAchievement[0]);
		}
	}

	public IAchievement CreateAchievement()
	{
		return null;
	}

	public void ReportScore(long score, string board, Action<bool> callback)
	{
		if (callback != null)
		{
			callback(false);
		}
	}

	public void LoadScores(string leaderboardID, Action<IScore[]> callback)
	{
		if (callback != null)
		{
			callback(new IScore[0]);
		}
	}

	public ILeaderboard CreateLeaderboard()
	{
		return new UnityEngine.SocialPlatforms.Impl.Leaderboard();
	}

	public void LoadScores(ILeaderboard board, Action<bool> callback)
	{
		if (callback != null)
		{
			callback(false);
		}
	}

	public bool GetLoading(ILeaderboard board)
	{
		return false;
	}

	public void ShowAchievementsUI()
	{
	}

	public void ShowLeaderboardUI()
	{
	}

	/// <summary>Local user that is never signed in.</summary>
	private class OfflineLocalUser : ILocalUser
	{
		public void Authenticate(Action<bool> callback)
		{
			if (callback != null)
			{
				callback(false);
			}
		}

		public void Authenticate(Action<bool, string> callback)
		{
			if (callback != null)
			{
				callback(false, "offline");
			}
		}

		public void LoadFriends(Action<bool> callback)
		{
			if (callback != null)
			{
				callback(false);
			}
		}

		public IUserProfile[] friends
		{
			get
			{
				return new IUserProfile[0];
			}
		}

		public bool authenticated
		{
			get
			{
				return false;
			}
		}

		public bool underage
		{
			get
			{
				return false;
			}
		}

		public string userName
		{
			get
			{
				return "";
			}
		}

		public string id
		{
			get
			{
				return "0";
			}
		}

		public string gameId
		{
			get
			{
				return "0";
			}
		}

		public bool isFriend
		{
			get
			{
				return false;
			}
		}

		public UserState state
		{
			get
			{
				return UserState.Offline;
			}
		}

		public Texture2D image
		{
			get
			{
				return null;
			}
		}
	}
}
