using Glu.Localization;
using UnityEngine;

public class SurvivalGatesInfoPopup : MissionInfoPopup
{
	public LeaderboardsWidget LeaderboardsWidget;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		base.Init(mapNarrative, mapLocation);
		ObjectiveText2.Text = string.Format(Strings.GetString("Receive XP and {0} for each survived wave and saved survivor"), GUIUtils.GetSymbolForCurrency("soft"));
		RewardCash.Text = string.Empty;
		RewardXP.Text = string.Empty;
		LeaderboardsWidget.Populate(Leaderboards.LeaderboardType.Survival1);
		TitleText.Text = Strings.GetString("SURVIVAL MODE");
		TitleText.Color = Color.red;
		TitleLines.Color = Color.red;
	}
}
