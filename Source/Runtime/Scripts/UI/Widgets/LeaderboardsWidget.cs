using System;
using Glu.AssetMapping;
using Glu.Localization;
using UnityEngine;

public class LeaderboardsWidget : MonoBehaviour
{
	private struct RecordInfo
	{
		public string UserName;

		public int Score;

		public bool IsLocalUser;

		public RecordInfo(string userName, int score, bool isLocal)
		{
			UserName = userName;
			Score = score;
			IsLocalUser = isLocal;
		}
	}

	public SpriteText UserText;

	public SpriteText ScoreText;

	private SpriteFont _font;

	private int _recordsCount;

	public float SpacingY = 7.7f;

	private void Awake()
	{
		UserText.gameObject.AddComponent<UIAnimationScale>().Setup(1.3f, 1f, 0f, 0.3f, EZAnimation.EASING_TYPE.BackIn);
		ScoreText.gameObject.AddComponent<UIAnimationScale>().Setup(1.3f, 1f, 0f, 0.3f, EZAnimation.EASING_TYPE.BackIn);
	}

	public void Populate(Leaderboards.LeaderboardType type)
	{
		int num = type == Leaderboards.LeaderboardType.Survival1 ? Singleton.PlayerData.PlayerStatistics.GetSurvivalGatesBestScore() : Singleton.PlayerData.PlayerStatistics.GetSurvivalArenaBestScore();
		_font = FontStore.GetFont(AssetMapper.Instance.MapObjectOrDefault(UserText.font, UserText.font));
		Leaderboards.LeaderboardRecord[] survivalLeaderbords = Singleton.Leaderboards.GetSurvivalLeaderbords(type);
		if (survivalLeaderbords != null && survivalLeaderbords.Length > 0)
		{
			for (int i = 0; i < survivalLeaderbords.Length; i++)
			{
				Leaderboards.LeaderboardRecord leaderboardRecord = survivalLeaderbords[i];
				int num2 = (int)leaderboardRecord.Score;
				if (leaderboardRecord.IsLocalPlayer)
				{
					num2 = Mathf.Max(num, num2);
				}
				AddRecord(leaderboardRecord.Rank, leaderboardRecord.Name, num2, leaderboardRecord.IsLocalPlayer);
			}
		}
		else
		{
			RecordInfo[] array = new RecordInfo[9]
			{
				new RecordInfo(Strings.GetString("Tr0mb"), 21840, false),
				new RecordInfo(Strings.GetString("W.Blake"), 19840, false),
				new RecordInfo(Strings.GetString("Nenastie"), 17880, false),
				new RecordInfo(Strings.GetString("Karp"), 15630, false),
				new RecordInfo(Strings.GetString("Gluer"), 7100, false),
				new RecordInfo(Strings.GetString("KKostya"), 4080, false),
				new RecordInfo(Strings.GetString("Tony"), 2020, false),
				new RecordInfo(Strings.GetString("Ignat"), 1580, false),
				new RecordInfo(Strings.GetString("Your score"), num, true)
			};
			Debug.Log(Strings.GetString("Tony").Length);
			Array.Sort(array, (RecordInfo x, RecordInfo y) => y.Score - x.Score);
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				AddRecord(num3 + 1, array[num3].UserName, array[num3].Score, array[num3].IsLocalUser);
			}
		}
	}

	public void AddRecord(int rank, string userName, int score, bool localPlayer)
	{
		SpriteText spriteText = UserText;
		SpriteText spriteText2 = ScoreText;
		if (_recordsCount > 0)
		{
			spriteText = (UnityEngine.Object.Instantiate(UserText.gameObject) as GameObject).GetComponent<SpriteText>();
			spriteText.isClone = true;
			spriteText.transform.parent = UserText.transform.parent;
			spriteText.transform.localPosition = UserText.transform.localPosition;
			spriteText.transform.Translate(0f, (float)(-_recordsCount) * SpacingY, 0f);
			spriteText.Text = string.Empty;
			spriteText2 = (UnityEngine.Object.Instantiate(ScoreText.gameObject) as GameObject).GetComponent<SpriteText>();
			spriteText2.isClone = true;
			spriteText2.transform.parent = ScoreText.transform.parent;
			spriteText2.transform.localPosition = ScoreText.transform.localPosition;
			spriteText2.transform.Translate(0f, (float)(-_recordsCount) * SpacingY, 0f);
			spriteText2.Text = string.Empty;
		}
		if (userName != null)
		{
			userName = _font.RemoveUnsupportedCharacters(userName);
		}
		if (userName == null || userName == string.Empty || userName.Trim().Length == 0)
		{
			userName = "???";
		}
		string text = (!(rank <= 0) ? string.Concat(GUIUtils.ColorYellowTitle, rank.ToString(), ".") : string.Empty);
		spriteText.Text = string.Concat(text, localPlayer ? GUIUtils.ColorYellowTitle : Color.white, userName);
		spriteText2.Text = string.Concat(localPlayer ? GUIUtils.ColorYellowTitle : Color.white, score.ToString());
		spriteText.gameObject.GetComponent<UIAnimationScale>().Delay = (float)_recordsCount * 0.1f + 0.2f;
		spriteText2.gameObject.GetComponent<UIAnimationScale>().Delay = (float)_recordsCount * 0.1f + 0.2f;
		_recordsCount++;
	}
}
