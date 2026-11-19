// PC 桥接：离线的 Social 平台（替代 iOS 的 Game Center）。
// 非 iOS 平台上 Social.Active 默认是 Unity 自带的本地实现：登录总是成功，游戏随后上报成就和排行榜，
// 本地实现不认识这些 ID，于是刷出 "Achievement ID not found" / "Leaderboard not found" 错误（开发版会弹出控制台）。
// PC 版彻底断网，这里让登录失败（等同 iOS 上没有登录 Game Center）。游戏所有上报都先判断 Social.localUser.authenticated，
// 所以什么都不会发出去；万一被直接调用，回调一律返回失败或空结果。
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PcSocialPlatform : ISocialPlatform
{
	private readonly PcLocalUser _localUser = new PcLocalUser();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Install()
	{
		Social.Active = new PcSocialPlatform();
		Debug.Log("[PcPort] offline social platform installed");
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

	/// <summary>永远没有登录的本地用户。</summary>
	private class PcLocalUser : ILocalUser
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
