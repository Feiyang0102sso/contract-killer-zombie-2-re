// PC port: makes The Pit (mission THEPIT, unused by the original) a regular city map mission.
//   - Spot: the map building Storage3, which has no MapLocation in the original. Setup gives it one with its own
//     area (GameData.MissionLocation.PcPitArea), so no other mission can be placed there.
//   - Like the survival missions it is always on the map from level MinLevel on: MapNarrative.Start re-adds it after
//     it was completed or after the plot reset (a finished story mission clears every spot).
//   - It is not counted as a regular mission (PlayerData.GetNumRegularActiveMissions), so the act's other missions stay as many.
//   - Briefing, rewards and story progress go through the game's normal mission code (KillAll, Hard).
//   - The GameData entry is left as shipped (MinLevel 99), so the act mission pools never pick it.
//   - Icon: the special hunt icon as a placeholder (MapNarrative.GetMissionButton).
using UnityEngine;

public static class PcPitMission
{
	/// <summary>GameData.Missions entry of The Pit.</summary>
	private const string MissionDescriptor = "THEPIT";

	/// <summary>Map building (child of the map scroller) that holds the mission.</summary>
	private const string LocationName = "Storage3";

	/// <summary>First player level the mission appears at.</summary>
	public const int MinLevel = 2;

	/// <summary>Index into GameData.Missions, -1 until looked up.</summary>
	private static int _missionIndex = -1;

	/// <summary>True when missionIndex is The Pit.</summary>
	public static bool IsPit(int missionIndex)
	{
		return missionIndex >= 0 && missionIndex == GetMissionIndex();
	}

	/// <summary>Index of THEPIT in GameData.Missions (-1 when missing).</summary>
	public static int GetMissionIndex()
	{
		if (_missionIndex >= 0)
		{
			return _missionIndex;
		}
		GameData.MissionData[] missions = Singleton.GameData.Missions;
		for (int i = 0; i < missions.Length; i++)
		{
			if (missions[i].Descriptor == MissionDescriptor)
			{
				_missionIndex = i;
				break;
			}
		}
		return _missionIndex;
	}

	/// <summary>MapNarrative.Awake hook: adds the MapLocation of The Pit to its building.</summary>
	public static void Setup(MapNarrative narrative)
	{
		MapScroller scroller = narrative.GetComponentInChildren<MapScroller>();
		Transform building = scroller.transform.Find(LocationName);
		if (building == null)
		{
			Debug.Log("[PcPitMission] map building " + LocationName + " not found, The Pit stays off the map");
			return;
		}
		// MapLocation.Awake runs here and shows the mission if the save already has it on this spot
		MapLocation location = building.gameObject.AddComponent<MapLocation>();
		location.Level = MinLevel;
		location.Location = GameData.MissionLocation.PcPitArea;
	}

	/// <summary>MapNarrative.Start hook (after the survival missions): puts The Pit on its spot when it is missing.</summary>
	public static void EnsureAssigned(MapNarrative narrative)
	{
		PlayerData player = PlayerData.Instance;
		int missionIndex = GetMissionIndex();
		if (missionIndex < 0 || player.Level < MinLevel || player.HasMission(missionIndex))
		{
			return;
		}
		MapScroller scroller = narrative.GetComponentInChildren<MapScroller>();
		Transform building = scroller.transform.Find(LocationName);
		if (building == null || player.HasMission(LocationName))
		{
			Debug.Log("[PcPitMission] spot " + LocationName + " missing or taken, The Pit not placed");
			return;
		}
		player.AddMission(LocationName, missionIndex, Random.Range(0, int.MaxValue));
		building.GetComponent<MapLocation>().PlotUpdate(true);
		Debug.Log("[PcPitMission] The Pit placed on " + LocationName);
	}
}
