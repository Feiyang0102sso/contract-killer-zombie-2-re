using Glu.Localization;
using UnityEngine;

public class UsualMissionInfoPopup : MissionInfoPopup
{
	protected bool _voiceOverEnabled = true;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		base.Init(mapNarrative, mapLocation);
		GameData.MissionData selectedMission = mapNarrative.SelectedMission;
		Mission.Type type = selectedMission.Type;
		Mission.Difficulty missionDifficulty = mapLocation.MissionDifficulty;
		bool flag = Mission.IsStorylineMission(type);
		GameData.StorylineData storylineData = flag ? Singleton.GameData.GetMissionStoryline(type) : null;
		if (ObjectiveText2 != null)
		{
			ObjectiveText2.Text = string.Empty;
		}
		bool isSpecialHunt = !Singleton.PlayerData.IsSpecialHuntReceived && PlayerData.Instance.CurrentMissionIndex == Singleton.ShopData.SpecialHunt.missionIndex;
		int num = Mission.ExperienceBase(type, missionDifficulty, isSpecialHunt);
		int num2 = Mission.SoftCurrencyBase(type, missionDifficulty, isSpecialHunt);
		int num3 = ((storylineData != null) ? storylineData.RewardHC : 0);
		RewardCash.Text = GUIUtils.FormatCurrencyString(num2, string.Empty, null);
		RewardHard.Text = GUIUtils.FormatCurrencyString(num3, string.Empty, null);
		RewardXP.Text = string.Format("{0}", num);
		if (flag && _voiceOverEnabled)
		{
			_voiceOver = Singleton.GameData.GetMissionVoiceover(mapNarrative.SelectedMissionIndex);
		}
		bool flag2 = num3 > 0;
		RewardHard.transform.parent.gameObject.SetActiveRecursively(flag2);
		bool flag3 = num2 > 0;
		RewardCash.transform.parent.gameObject.SetActiveRecursively(flag3);
		if (RewardsGrid != null)
		{
			int num4 = 0;
			if (flag3)
			{
				RewardsGrid.AddItem(RewardCash.transform.parent, num4++, 0);
			}
			if (flag2)
			{
				RewardsGrid.AddItem(RewardHard.transform.parent, num4++, 0);
			}
			RewardsGrid.AddItem(RewardXP.transform.parent, num4++, 0);
		}
		if (RecWeapon1Widget != null)
		{
			RecWeapon1Widget.SetItem(Singleton.GameData.GetRecommendedItem(selectedMission, 1));
		}
		if (RecWeapon2Widget != null)
		{
			RecWeapon2Widget.SetItem(Singleton.GameData.GetRecommendedItem(selectedMission, 2));
		}
		Color storyline;
		string text = null;
		if (flag)
		{
			storyline = MapLocation.Storyline;
			text = Strings.GetString("STORY MISSION");
		}
		else
		{
			bool flag4 = missionDifficulty == Mission.Difficulty.Hard;
			storyline = flag4 ? MapLocation.Hard : MapLocation.Normal;
			text = flag4 ? Strings.GetString("HARD MISSION") : Strings.GetString("EASY MISSION");
		}
		storyline = Color.white;
		// PC port: The Pit shows its own mission name ("The Pit Fighter") instead of the difficulty, upper case like the other titles
		if (PitMission.IsPit(mapNarrative.SelectedMissionIndex))
		{
			text = Strings.GetString(mapNarrative.SelectedMissionDescription.Title).ToUpperInvariant();
		}
		if (TitleText != null)
		{
			TitleText.Text = text;
			TitleText.Color = storyline;
		}
		if (TitleLines != null)
		{
			TitleLines.Color = storyline;
		}
		bool flag5 = missionDifficulty == Mission.Difficulty.Normal && !flag;
		int num5 = Mission.AmountForTag("Bonus", type, missionDifficulty);
		// PC port: the count above comes from the mission type only; The Pit's scene has no bonus crate spots
		if (PitMission.IsPit(mapNarrative.SelectedMissionIndex))
		{
			num5 = 0;
		}
		for (int i = 0; i < num5; i++)
		{
			GameObject item = Util.CreateGameObject(flag5 ? "BonusCrateUsual" : "BonusCrateGold");
			if (BonusGrid != null)
			{
				BonusGrid.AddItem(item, i, 0);
			}
		}
	}
}
