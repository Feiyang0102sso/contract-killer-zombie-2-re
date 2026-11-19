using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Leaderboards : MonoBehaviour
{
	public enum LeaderboardType
	{
		Survival1,
		Survival2,
		Count
	}

	public struct LeaderboardRecord
	{
		public int Rank;

		public string PlayerId;

		public string Name;

		public long Score;

		public bool IsLocalPlayer;

		public bool IsFriend;

		public void Fill(IScore record, bool isFriend)
		{
			Rank = record.rank;
			PlayerId = record.userID;
			Score = record.value;
			IsFriend = isFriend;
		}
	}

	private const int MaxScoresCount = 10;

	public IScore[] SurvivalScores;

	private LeaderboardRecord[][] _survivalLeaderboards = new LeaderboardRecord[2][];

	public LeaderboardRecord[] GetSurvivalLeaderbords(LeaderboardType type)
	{
		return _survivalLeaderboards[(int)type];
	}

	public void UpdateLeaderboards()
	{
		if (Social.localUser.authenticated)
		{
			Social.ReportScore(Singleton.PlayerData.PlayerStatistics.TotalNumKills, "CKZOMBIES2_BOARD_ZOMBIES_KILLED", (bool result) =>
			{
			});
			Social.ReportScore(Singleton.PlayerData.PlayerStatistics.SurvivalGatesBestScore, "CKZOMBIES2_BOARD_SURVIVAL_SCORE", (bool result) =>
			{
			});
			Social.ReportScore(Singleton.PlayerData.PlayerStatistics.SurvivalArenaBestScore, "CKZOMBIES2_BOARD_ARENA_SCORE", (bool result) =>
			{
			});
			UpdateLeaderboard(LeaderboardType.Survival1, "CKZOMBIES2_BOARD_SURVIVAL_SCORE");
			UpdateLeaderboard(LeaderboardType.Survival2, "CKZOMBIES2_BOARD_ARENA_SCORE");
		}
	}

	private void UpdateLeaderboard(LeaderboardType type, string leaderboardId)
	{
		ILeaderboard leaderboard = Social.CreateLeaderboard();
		leaderboard.id = leaderboardId;
		leaderboard.userScope = UserScope.FriendsOnly;
		leaderboard.LoadScores((bool success) =>
		{
			if (leaderboard.scores == null || leaderboard.scores.Length <= 1)
			{
				leaderboard.userScope = UserScope.Global;
				leaderboard.LoadScores((bool secondSuccess) =>
				{
					_survivalLeaderboards[(int)type] = TakeLeaderboards(leaderboard.scores, leaderboardId, false);
				});
			}
			else
			{
				_survivalLeaderboards[(int)type] = TakeLeaderboards(leaderboard.scores, leaderboardId, true);
			}
		});
	}

	private LeaderboardRecord[] TakeLeaderboards(IScore[] records, string leaderboardId, bool isFriends)
	{
		if (records == null)
		{
			return null;
		}
		MonoBehaviour.print("Taking lbs:" + isFriends);
		foreach (IScore score in records)
		{
			MonoBehaviour.print("Rank:" + score.rank);
			MonoBehaviour.print("FormattedScore:" + score.formattedValue);
			MonoBehaviour.print("PlayerID:" + score.userID);
			MonoBehaviour.print("Score:" + score.value);
		}
		LeaderboardRecord[] leaderboardRecords = new LeaderboardRecord[Mathf.Min(10, records.Length)];
		for (int j = 0; j < leaderboardRecords.Length; j++)
		{
			leaderboardRecords[j].Fill(records[j], isFriends);
		}
		if (leaderboardRecords.Length == 0)
		{
			return null;
		}
		ILeaderboard leaderboard = Social.CreateLeaderboard();
		leaderboard.id = leaderboardId;
		leaderboard.userScope = (isFriends ? UserScope.FriendsOnly : UserScope.Global);
		leaderboard.LoadScores((bool success) =>
		{
			if (success)
			{
				IScore localUserScore = leaderboard.localUserScore;
				MonoBehaviour.print("Local score:");
				MonoBehaviour.print("Rank:" + localUserScore.rank);
				MonoBehaviour.print("FormattedScore:" + localUserScore.formattedValue);
				MonoBehaviour.print("PlayerID:" + localUserScore.userID);
				MonoBehaviour.print("Score:" + localUserScore.value);
				int num = leaderboardRecords.Length - 1;
				for (int k = 0; k < leaderboardRecords.Length; k++)
				{
					leaderboardRecords[k].IsLocalPlayer = false;
					if (leaderboardRecords[k].PlayerId == localUserScore.userID)
					{
						num = k;
					}
				}
				leaderboardRecords[num].Fill(localUserScore, true);
				leaderboardRecords[num].IsLocalPlayer = true;
			}
			LoadNames(leaderboardRecords);
		});
		return leaderboardRecords;
	}

	private void LoadNames(LeaderboardRecord[] leaderboardRecords)
	{
		string[] array = new string[leaderboardRecords.Length];
		for (int i = 0; i < leaderboardRecords.Length; i++)
		{
			array[i] = leaderboardRecords[i].PlayerId;
		}
		Social.LoadUsers(array, (IUserProfile[] records) =>
		{
			if (records == null)
			{
				return;
			}
			MonoBehaviour.print("player names loaded");
			foreach (IUserProfile userProfile in records)
			{
				MonoBehaviour.print("id:" + userProfile.id);
				MonoBehaviour.print("userName:" + userProfile.userName);
			}
			for (int k = 0; k < leaderboardRecords.Length && k < records.Length; k++)
			{
				leaderboardRecords[k].Name = records[k].userName;
			}
		});
	}
}
