using Glu.Localization;
using UnityEngine;

public class ArenaGoalPanel : MonoBehaviour
{
	public UIExpandSprite Back;

	public SpriteText TitleText;

	public SpriteText ProgressText;

	public SpriteText ScoreText;

	public SpriteText DailyText;

	public PackedSprite DailyBack;

	public SpriteText CompletedText;

	public SpriteText RewardText;

	public PackedSprite RewardPic;

	public bool IsCompleted { get; private set; }

	public bool IsObsolete { get; private set; }

	public string Goal { get; private set; }

	private void Awake()
	{
	}

	public void Init(string goal, PlayerData.ArenaGoalInfo info, bool isCompleted, bool isObsolete)
	{
		IsCompleted = isCompleted;
		IsObsolete = isObsolete;
		Goal = goal;
		GameData.ArenaGoalsData arenaGoalsData = Singleton.GameData.arenaGoals[goal];
		TitleText.Text = string.Format(Strings.GetString(arenaGoalsData.Name), PlayerData.ArenaGoalInfo.LimitForType(info.type, arenaGoalsData));
		ProgressText.Text = (arenaGoalsData.Save != "+") ? string.Empty : string.Format(Strings.GetString("{0} to go"), PlayerData.ArenaGoalInfo.LimitForType(info.type, arenaGoalsData) - info.counter);
		float num = PlayerData.ArenaGoalInfo.RewardModForType(info.type, arenaGoalsData);
		ScoreText.Text = string.Format(Strings.GetString("Score: +{0}"), num * (float)arenaGoalsData.Score);
		if (isCompleted)
		{
			CompletedText.Text = Strings.GetString("GOAL COMPLETED");
		}
		else
		{
			CompletedText.Text = string.Empty;
		}
		if (info.type == "d")
		{
			DailyText.Text = Strings.GetString("DAILY");
			Back.Color = Util.ColorFromHex("#ffff00");
		}
		else
		{
			DailyText.transform.parent.Translate(0f, 0f, 10000f);
			Back.Color = Util.ColorFromHex("#5d95cb");
		}
		int num2 = arenaGoalsData.Soft != 0 ? arenaGoalsData.Soft :
			arenaGoalsData.Hard != 0 ? arenaGoalsData.Hard : arenaGoalsData.Xp;
		string text = string.Format("{0}", Mathf.CeilToInt(num * (float)num2));
		if (arenaGoalsData.Hard != 0)
		{
			RewardPic.Hide(true);
			text += "^";
		}
		if (arenaGoalsData.Xp != 0)
		{
			RewardPic.Hide(true);
			text += Strings.GetString("XP");
		}
		RewardText.Text = text;
	}

	public void SetAlpha(float alpha)
	{
		Color color = new Color(1f, 1f, 1f, alpha);
		TitleText.Color = color;
		ScoreText.Color = color;
		RewardText.Color = color;
		RewardPic.Color = color;
		if (DailyBack != null)
		{
			DailyBack.Color = color;
		}
		if (DailyText != null)
		{
			DailyText.Color = GUIUtils.NewColorWithAlpha(DailyText.Color, alpha);
		}
		CompletedText.Color = GUIUtils.NewColorWithAlpha(CompletedText.Color, alpha);
		Back.Color = GUIUtils.NewColorWithAlpha(Back.Color, alpha);
		Color color2 = ProgressText.Color;
		color2.a = alpha;
		ProgressText.Color = color2;
	}
}
