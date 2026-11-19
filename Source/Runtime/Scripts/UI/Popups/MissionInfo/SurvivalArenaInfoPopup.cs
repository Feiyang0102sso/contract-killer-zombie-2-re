using Glu.Localization;
using UnityEngine;

public class SurvivalArenaInfoPopup : MissionInfoPopup
{
	public ArenaGoalsWidget ArenaGoalsWidget;

	public LeaderboardsWidget LeaderboardsWidget;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		base.Init(mapNarrative, mapLocation);
		ArenaGoalsWidget.Init();
		LeaderboardsWidget.Populate(Leaderboards.LeaderboardType.Survival2);
		TitleText.Text = Strings.GetString("ARENA");
		TitleText.Color = Color.white;
		TitleLines.Color = Color.white;
	}
}
