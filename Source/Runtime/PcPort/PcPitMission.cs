// PC port: makes The Pit (mission THEPIT, unused by the original) a regular city map mission.
//   - Spot: the map building Storage3, which has no MapLocation in the original. Setup gives it one with its own
//     area (GameData.MissionLocation.PcPitArea), so no other mission can be placed there.
//   - Like the survival missions it is always on the map from level MinLevel on: MapNarrative.Start re-adds it after
//     it was completed or after the plot reset (a finished story mission clears every spot).
//   - It is not counted as a regular mission (PlayerData.GetNumRegularActiveMissions), so the act's other missions stay as many.
//   - Briefing, rewards and story progress go through the game's normal mission code (KillAll, Hard).
//   - The GameData entry is left as shipped (MinLevel 99), so the act mission pools never pick it.
//   - Icon: a copy of the special hunt button (the big icon with the glow, tinted with the survival orange like it)
//     with its own white glyph, a zombie hand rising from the pool (Resources/PcPort/PcPitIcon, drawn by
//     tools/make_pit_icons.py). MapNarrative.GetMissionButton returns it for The Pit, and MapLocation.GetMissionColor
//     gives it the survival orange instead of the hard-mission yellow.
using UnityEngine;

public static class PcPitMission
{
	/// <summary>GameData.Missions entry of The Pit.</summary>
	private const string MissionDescriptor = "THEPIT";

	/// <summary>Map building (child of the map scroller) that holds the mission.</summary>
	private const string LocationName = "Storage3";

	/// <summary>First player level the mission appears at.</summary>
	public const int MinLevel = 2;

	/// <summary>Material with the icon texture (the whole texture is one icon).</summary>
	private const string IconMaterialPath = "PcPort/PcPitIcon";

	/// <summary>Index into GameData.Missions, -1 until looked up.</summary>
	private static int _missionIndex = -1;

	/// <summary>Inactive button template on the current city map, cloned by MapLocation like the stock ones.</summary>
	public static GameObject Button;

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
		// Before the MapLocation below: its Awake already asks for the button
		Button = CreateButton(narrative.SpecialHuntButton);
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

	/// <summary>
	/// Copies the special hunt button (frame, glow child, size, colors) and gives it the Pit icon texture.
	/// The copy is made under an inactive holder, so EZGUI's Awake only runs once MapLocation activates a clone of
	/// it, after the material and the frame UVs are changed. The template stays inactive next to the original.
	/// </summary>
	private static GameObject CreateButton(GameObject specialHuntButton)
	{
		Material material = Resources.Load<Material>(IconMaterialPath);
		if (material == null)
		{
			throw new System.Exception("[PcPitMission] material missing: Resources/" + IconMaterialPath);
		}
		GameObject holder = new GameObject("PcPitButtonHolder");
		holder.SetActive(false);
		GameObject button = Object.Instantiate(specialHuntButton, holder.transform);
		button.name = "PcPitButton";
		button.SetActive(false);
		button.transform.SetParent(specialHuntButton.transform.parent, false);
		button.transform.localPosition = specialHuntButton.transform.localPosition;
		button.transform.localRotation = specialHuntButton.transform.localRotation;
		button.transform.localScale = specialHuntButton.transform.localScale;
		Object.Destroy(holder);

		button.GetComponent<Renderer>().sharedMaterial = material;
		UIButton uiButton = button.GetComponent<UIButton>();
		foreach (TextureAnim state in uiButton.states)
		{
			foreach (CSpriteFrame frame in state.spriteFrames)
			{
				frame.uvs = new Rect(0f, 0f, 1f, 1f);
			}
		}
		Debug.Log("[PcPitMission] icon button created from " + specialHuntButton.name);
		return button;
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
