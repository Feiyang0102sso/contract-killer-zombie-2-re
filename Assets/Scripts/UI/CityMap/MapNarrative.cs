using System;
using System.Collections.Generic;
using UnityEngine;

public class MapNarrative : MonoBehaviour
{
	[Serializable]
	public class MissionButton
	{
		public Mission.Type MissionType = Mission.Type.KillAll;

		public GameObject Button;
	}

	public int MaxNumGoals = 4;

	public MissionButton[] MissionButtons;

	public GameObject SpecialHuntButton;

	private string _selectedLocationName = string.Empty;

	private int _selectedMissionIndex = -1;

	private static MapNarrative _instance = null;

	private MapLocation[] GetLocations()
	{
		return GetComponentsInChildren<MapLocation>();
	}

	private MapLocation[] GetLocations(GameData.MissionLocation missionLocation)
	{
		MapLocation[] componentsInChildren = GetComponentsInChildren<MapLocation>();
		MapLocation[] array = new MapLocation[componentsInChildren.Length];
		int num = 0;
		foreach (MapLocation mapLocation in componentsInChildren)
		{
			if (mapLocation.Level <= PlayerData.Instance.Level && (mapLocation.Location == missionLocation || missionLocation == GameData.MissionLocation.Any))
			{
				array[num] = mapLocation;
				num++;
			}
		}
		Array.Resize(ref array, num);
		return array;
	}

	private MapLocation[] GetAvailableLocations(GameData.MissionLocation missionLocation)
	{
		MapLocation[] componentsInChildren = GetComponentsInChildren<MapLocation>();
		MapLocation[] array = new MapLocation[componentsInChildren.Length];
		int num = 0;
		foreach (MapLocation mapLocation in componentsInChildren)
		{
			if (!mapLocation.HasMission && mapLocation.Level <= PlayerData.Instance.Level && (mapLocation.Location == missionLocation || missionLocation == GameData.MissionLocation.Any))
			{
				array[num] = mapLocation;
				num++;
			}
		}
		Array.Resize(ref array, num);
		return array;
	}

	private MapLocation[] GetAvailableLocationsForQueuedMissions()
	{
		MapLocation[] componentsInChildren = GetComponentsInChildren<MapLocation>();
		MapLocation[] array = new MapLocation[componentsInChildren.Length];
		int num = 0;
		foreach (MapLocation mapLocation in componentsInChildren)
		{
			if (!(mapLocation.HasMission || mapLocation.Level > PlayerData.Instance.Level))
			{
				int numQueuedMissions = PlayerData.Instance.GetNumQueuedMissions();
				for (int j = 0; j < numQueuedMissions; j++)
				{
					int missionIndex = PlayerData.Instance.PopMission();
					PlayerData.Instance.QueueMission(missionIndex, false);
					GameData.MissionLocation location = GetMissionByIndex(missionIndex).Location;
					if (mapLocation.Location == location || location == GameData.MissionLocation.Any)
					{
						array[num] = mapLocation;
						num++;
						break;
					}
				}
			}
		}
		Array.Resize(ref array, num);
		return array;
	}

	private bool SurvivalIsAssigned(Mission.Type survivalType)
	{
		MapLocation[] locations = GetLocations();
		foreach (MapLocation mapLocation in locations)
		{
			if (PlayerData.Instance.HasMission(mapLocation.gameObject.name))
			{
				int mission = PlayerData.Instance.GetMission(mapLocation.gameObject.name);
				if (GetMissionByIndex(mission).Type == survivalType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool SpecialHuntIsAssigned()
	{
		if (PlayerData.Instance.IsSpecialHuntReceived)
		{
			return false;
		}
		MapLocation[] locations = GetLocations();
		foreach (MapLocation mapLocation in locations)
		{
			if (PlayerData.Instance.HasMission(mapLocation.gameObject.name))
			{
				int mission = PlayerData.Instance.GetMission(mapLocation.gameObject.name);
				if (mission == Singleton.ShopData.SpecialHunt.missionIndex)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void AssignStorylineMissions()
	{
		for (int num = PlayerData.Instance.GetNumQueuedMissions(); num > 0; num--)
		{
			int missionIndex = PlayerData.Instance.PopMission();
			if (GetMissionByIndex(missionIndex).Type >= Mission.Type.Story1)
			{
				MapLocation[] availableLocations = GetAvailableLocations(GetMissionByIndex(missionIndex).Location);
				if (availableLocations.Length == 0)
				{
					availableLocations = GetLocations(GetMissionByIndex(missionIndex).Location);
					int num2 = UnityEngine.Random.Range(0, availableLocations.Length);
					PlayerData.Instance.RequeueMission(availableLocations[num2].gameObject.name);
					availableLocations[num2].PlotUpdate(true);
					Debug.Log("(Plot) Cleanup location " + availableLocations[num2].gameObject.name + " for storyline mission!");
					availableLocations = GetAvailableLocations(GetMissionByIndex(missionIndex).Location);
				}
				if (availableLocations.Length > 0)
				{
					int num3 = UnityEngine.Random.Range(0, availableLocations.Length);
					PlayerData.Instance.AddMission(availableLocations[num3].gameObject.name, missionIndex, UnityEngine.Random.Range(0, int.MaxValue));
					availableLocations[num3].PlotUpdate(true);
					break;
				}
				PlayerData.Instance.QueueMission(missionIndex, false);
			}
			else
			{
				PlayerData.Instance.QueueMission(missionIndex, false);
			}
		}
	}

	private void AssignSurvivalMission(Mission.Type survivalType)
	{
		for (int num = PlayerData.Instance.GetNumQueuedMissions(); num > 0; num--)
		{
			int missionIndex = PlayerData.Instance.PopMission();
			if (GetMissionByIndex(missionIndex).Type == survivalType)
			{
				MapLocation[] availableLocations = GetAvailableLocations(GetMissionByIndex(missionIndex).Location);
				if (availableLocations.Length > 0)
				{
					int num2 = UnityEngine.Random.Range(0, availableLocations.Length);
					PlayerData.Instance.AddMission(availableLocations[num2].gameObject.name, missionIndex, UnityEngine.Random.Range(0, int.MaxValue));
					availableLocations[num2].PlotUpdate(true);
					break;
				}
				PlayerData.Instance.QueueMission(missionIndex, false);
			}
			else
			{
				PlayerData.Instance.QueueMission(missionIndex, false);
			}
		}
	}

	private void AssignSpecialHuntMission()
	{
		if (PlayerData.Instance.IsSpecialHuntReceived)
		{
			return;
		}
		for (int num = PlayerData.Instance.GetNumQueuedMissions(); num > 0; num--)
		{
			int num2 = PlayerData.Instance.PopMission();
			if (num2 == Singleton.ShopData.SpecialHunt.missionIndex)
			{
				MapLocation[] availableLocations = GetAvailableLocations(GetMissionByIndex(num2).Location);
				if (availableLocations.Length > 0)
				{
					int num3 = UnityEngine.Random.Range(0, availableLocations.Length);
					PlayerData.Instance.AddMission(availableLocations[num3].gameObject.name, num2, UnityEngine.Random.Range(0, int.MaxValue));
					availableLocations[num3].PlotUpdate(true);
					break;
				}
				PlayerData.Instance.QueueMission(num2, false);
			}
			else
			{
				PlayerData.Instance.QueueMission(num2, false);
			}
		}
	}

	private void PrintQueue(string title)
	{
		int numQueuedMissions = PlayerData.Instance.GetNumQueuedMissions();
		string text = title + " : ";
		for (int i = 0; i < numQueuedMissions; i++)
		{
			int num = PlayerData.Instance.PopMission();
			text += string.Format("{0} ", num);
			PlayerData.Instance.QueueMission(num, false);
		}
		Debug.Log(text);
	}

	private void AssignStackedMissions()
	{
		if (!SurvivalIsAssigned(Mission.Type.SurvivalGates))
		{
			AssignSurvivalMission(Mission.Type.SurvivalGates);
		}
		if (!SurvivalIsAssigned(Mission.Type.SurvivalArena))
		{
			AssignSurvivalMission(Mission.Type.SurvivalArena);
		}
		if (!SpecialHuntIsAssigned())
		{
			AssignSpecialHuntMission();
		}
		AssignStorylineMissions();
		int maxMissions = Singleton.GameData.Acts[PlayerData.Instance.Act].MaxMissions;
		int num = Mathf.Min(PlayerData.Instance.GetNumQueuedMissions(), maxMissions - PlayerData.Instance.GetNumRegularActiveMissions());
		Debug.Log("Assigning " + num + " queued mission(s)...");
		for (; num > 0; num--)
		{
			Mission.Type rarestMissionType = PlayerData.Instance.RarestMissionType;
			int num2 = -1;
			for (int num3 = PlayerData.Instance.GetNumQueuedMissions(); num3 > 0; num3--)
			{
				int num4 = PlayerData.Instance.PopMission();
				Mission.Type type = Singleton.GameData.Missions[num4].Type;
				if (Mission.IsSurvival(type))
				{
					PlayerData.Instance.QueueMission(num4, false);
				}
				else if (!PlayerData.Instance.HasMission(num4))
				{
					MapLocation[] availableLocations = GetAvailableLocations(GetMissionByIndex(num4).Location);
					if (availableLocations.Length > 0)
					{
						if (type == rarestMissionType || num2 == -1)
						{
							if (num2 != -1)
							{
								PlayerData.Instance.QueueMission(num2, false);
							}
							num2 = num4;
						}
						else
						{
							Debug.Log("Failed to assign mission " + GetMissionByIndex(num4).Descriptor + " (type rareness check)");
							PlayerData.Instance.QueueMission(num4, false);
						}
					}
					else
					{
						Debug.Log("Failed to assign mission " + GetMissionByIndex(num4).Descriptor + " (no available locations)");
						PlayerData.Instance.QueueMission(num4, false);
					}
				}
				else
				{
					Debug.Log("Failed to assign mission " + GetMissionByIndex(num4).Descriptor + " (index already in use)");
					PlayerData.Instance.QueueMission(num4, false);
				}
			}
			if (num2 != -1)
			{
				MapLocation[] availableLocations2 = GetAvailableLocations(GetMissionByIndex(num2).Location);
				int num5 = UnityEngine.Random.Range(0, availableLocations2.Length);
				PlayerData.Instance.AddMission(availableLocations2[num5].gameObject.name, num2, UnityEngine.Random.Range(0, int.MaxValue));
				availableLocations2[num5].PlotUpdate(true);
				Debug.Log("Assignment successfull: " + GetMissionByIndex(num2).Descriptor + " to " + availableLocations2[num5].gameObject.name);
			}
			else
			{
				Debug.LogWarning("Failed to assign any mission from queue!");
				break;
			}
		}
		Debug.Log("Act " + PlayerData.Instance.Act + ", max missions " + maxMissions + ", assigned " + PlayerData.Instance.GetNumRegularActiveMissions() + ", queued " + PlayerData.Instance.GetNumQueuedMissions());
	}

	private void RemoveSurvivalFromQueue(Mission.Type survivalType)
	{
		for (int num = PlayerData.Instance.GetNumQueuedMissions(); num > 0; num--)
		{
			int missionIndex = PlayerData.Instance.PopMission();
			if (GetMissionByIndex(missionIndex).Type == survivalType)
			{
				break;
			}
			PlayerData.Instance.QueueMission(missionIndex, false);
		}
	}

	private bool SurvivalIsQueued(Mission.Type survivalType)
	{
		for (int num = PlayerData.Instance.GetNumQueuedMissions(); num > 0; num--)
		{
			int missionIndex = PlayerData.Instance.PopMission();
			if (GetMissionByIndex(missionIndex).Type == survivalType)
			{
				PlayerData.Instance.QueueMission(missionIndex, false);
				return true;
			}
			PlayerData.Instance.QueueMission(missionIndex, false);
		}
		return false;
	}

	private bool SurvivalIsEnabled()
	{
		int maxMissionIndex = GetMaxMissionIndex();
		for (int i = 0; i <= maxMissionIndex; i++)
		{
			if (Mission.IsSurvival(GetMissionByIndex(i).Type))
			{
				return true;
			}
		}
		return false;
	}

	private bool SpecialHuntIsQueued()
	{
		if (PlayerData.Instance.IsSpecialHuntReceived)
		{
			return false;
		}
		for (int num = PlayerData.Instance.GetNumQueuedMissions(); num > 0; num--)
		{
			int num2 = PlayerData.Instance.PopMission();
			if (num2 == Singleton.ShopData.SpecialHunt.missionIndex)
			{
				PlayerData.Instance.QueueMission(num2, false);
				return true;
			}
			PlayerData.Instance.QueueMission(num2, false);
		}
		return false;
	}

	private bool SpecialHuntIsEnabled()
	{
		if (PlayerData.Instance.IsSpecialHuntReceived)
		{
			return false;
		}
		int maxMissionIndex = GetMaxMissionIndex();
		if (Singleton.ShopData.SpecialHunt.missionIndex >= 0 && Singleton.ShopData.SpecialHunt.missionIndex < maxMissionIndex)
		{
			return true;
		}
		return false;
	}

	private void GenerateSurvival(Mission.Type survivalType)
	{
		GameData.Act act = Singleton.GameData.Acts[PlayerData.Instance.Act];
		int maxMissionIndex = GetMaxMissionIndex();
		for (int i = 0; i <= maxMissionIndex; i++)
		{
			GameData.MissionData missionByIndex = GetMissionByIndex(i);
			if (missionByIndex.Type == survivalType && Singleton.PlayerData.Level >= missionByIndex.MinLevel && Singleton.PlayerData.Level <= missionByIndex.MaxLevel)
			{
				PlayerData.Instance.QueueMission(i, false);
			}
		}
		AssignSurvivalMission(survivalType);
	}

	private void GenerateSpecialHunt()
	{
		int maxMissionIndex = GetMaxMissionIndex();
		int missionIndex = Singleton.ShopData.SpecialHunt.missionIndex;
		if (missionIndex >= 0 && missionIndex < maxMissionIndex && !PlayerData.Instance.HasMission(missionIndex))
		{
			PlayerData.Instance.QueueMission(missionIndex, false);
		}
		AssignSpecialHuntMission();
	}

	private void GenerateStoryline()
	{
		if (!(PlayerData.Instance.StorylineTicket >= Singleton.GameData.GetStorylineLength()))
		{
			Mission.Type type = (Mission.Type)(5 + PlayerData.Instance.StorylineTicket);
			int num = -1;
			for (int num2 = Singleton.GameData.Missions.Length - 1; num2 >= 0; num2--)
			{
				if (Singleton.GameData.Missions[num2].Type == type)
				{
					num = num2;
					break;
				}
			}
			if (num >= 0 && !PlayerData.Instance.HasMission(num))
			{
				PlayerData.Instance.QueueMission(num, false);
			}
		}
	}

	private void GenerateMissionPool()
	{
		GameData.Act act = Singleton.GameData.Acts[PlayerData.Instance.Act];
		for (int i = 0; i < act.PoolData.Length; i++)
		{
			GameData.PoolData poolData = act.PoolData[i];
			List<int> list = new List<int>();
			for (int j = 0; j < Singleton.GameData.Missions.Length; j++)
			{
				if (!PlayerData.Instance.IsMissionQueued(j) && !PlayerData.Instance.IsMissionLocked(j))
				{
					GameData.MissionData missionData = Singleton.GameData.Missions[j];
					if (Singleton.PlayerData.Level >= missionData.MinLevel && Singleton.PlayerData.Level <= missionData.MaxLevel && missionData.Type == poolData.Type && missionData.Difficulty == poolData.Difficulty && missionData.Type < Mission.Type.Story1)
					{
						list.Add(j);
					}
				}
			}
			if (list.Count > 0)
			{
				int missionIndex = list[0];
				float num = PlayerData.Instance.GetQueuedLocationRarenessForMission(missionIndex);
				for (int k = 1; k < list.Count; k++)
				{
					float queuedLocationRarenessForMission = PlayerData.Instance.GetQueuedLocationRarenessForMission(list[k]);
					if (queuedLocationRarenessForMission < num)
					{
						missionIndex = list[k];
						num = queuedLocationRarenessForMission;
					}
				}
				PlayerData.Instance.QueueMission(missionIndex, true);
			}
			else
			{
			}
		}
	}

	private void GeneratePlot(bool continueCurrentAct)
	{
		bool flag = PlayerData.Instance.GetNumQueuedMissions() == 0;
		bool flag2 = !PlayerData.Instance.IsUpToDate;
		if (!continueCurrentAct || flag)
		{
			if (!continueCurrentAct)
			{
				PlayerData.Instance.CleanupActiveMissions();
				PlayerData.Instance.CleanupQueue();
			}
			MapLocation[] locations = GetLocations();
			foreach (MapLocation mapLocation in locations)
			{
				mapLocation.PlotUpdate(true);
			}
			int num = 0;
			foreach (GameData.Act act in Singleton.GameData.Acts)
			{
				if (act.Story == PlayerData.Instance.StorylineTicket)
				{
					PlayerData.Instance.Act = num;
					if ((Singleton.PlayerData.StorylineProgress == 0 && (!continueCurrentAct || flag2)) || PlayerData.Instance.StorylineTicket == Singleton.GameData.GetStorylineLength())
					{
						GenerateMissionPool();
					}
					break;
				}
				num++;
			}
		}
		if (PlayerData.Instance.IsReadyForStorylineMission())
		{
			GenerateStoryline();
		}
		AssignStackedMissions();
	}

	private int GetPendingTicketReadyToUse()
	{
		for (int i = 0; i < PlayerData.Instance.NumPendingTickets; i++)
		{
			int pendingTicket = PlayerData.Instance.GetPendingTicket(i);
			GameData.GoalData goalData = Singleton.GameData.Goals[pendingTicket];
			if (!PlayerData.Instance.HasGoal(goalData.Type))
			{
				return pendingTicket;
			}
		}
		return -1;
	}

	private void GenerateGoals()
	{
		while (PlayerData.Instance.GetNumGoals() < MaxNumGoals)
		{
			int pendingTicketReadyToUse = GetPendingTicketReadyToUse();
			if (pendingTicketReadyToUse == -1)
			{
				break;
			}
			GameData.GoalData goalData = Singleton.GameData.Goals[pendingTicketReadyToUse];
			PlayerData.Instance.AddGoal(goalData.Generate(pendingTicketReadyToUse));
			PlayerData.Instance.RemovePendingTicket(pendingTicketReadyToUse);
			Debug.Log(string.Concat("New goal: ", goalData.Type, " Total goals: ", PlayerData.Instance.GetNumGoals()));
		}
		while (PlayerData.Instance.GetNumGoals() < MaxNumGoals && PlayerData.Instance.Ticket < Singleton.GameData.Goals.Length)
		{
			GameData.GoalData goalData2 = Singleton.GameData.Goals[PlayerData.Instance.Ticket];
			if (!PlayerData.Instance.HasGoal(goalData2.Type))
			{
				PlayerData.Instance.AddGoal(goalData2.Generate(PlayerData.Instance.Ticket));
				PlayerData.Instance.Ticket++;
				Debug.Log(string.Concat("New goal: ", goalData2.Type, " Total goals: ", PlayerData.Instance.GetNumGoals()));
			}
			else
			{
				Debug.Log(string.Concat("Pending ticket: ", PlayerData.Instance.Ticket, "(goal: ", goalData2.Type, ")"));
				PlayerData.Instance.AddPendingTicket(PlayerData.Instance.Ticket);
				PlayerData.Instance.Ticket++;
			}
		}
		CommonGUI componentInChildren = GetComponentInChildren<CommonGUI>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SendMessage("UpdatePlayerInfo");
		}
	}

	private void GenerateCumulativeGoal()
	{
		while (PlayerData.Instance.CumulativeTicket < Singleton.GameData.CumulativeGoals.Length)
		{
			GameData.CumulativeGoalData cumulativeGoalData = Singleton.GameData.CumulativeGoals[PlayerData.Instance.CumulativeTicket];
			PlayerData.Instance.SetCumulativeGoal(cumulativeGoalData.Generate());
			PlayerData.Instance.CumulativeTicket++;
			break;
		}
		CommonGUI componentInChildren = GetComponentInChildren<CommonGUI>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SendMessage("UpdatePlayerInfo");
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		MapLocation[] availableLocationsForQueuedMissions = GetAvailableLocationsForQueuedMissions();
		bool flag = !PlayerData.Instance.IsUpToDate;
		bool flag2 = PlayerData.Instance.Act == -1;
		bool flag3 = !flag2 && Singleton.GameData.Acts[PlayerData.Instance.Act].Story < PlayerData.Instance.StorylineTicket;
		bool flag4 = PlayerData.Instance.GetNumQueuedMissions() > 0 && availableLocationsForQueuedMissions.Length > 0;
		bool flag5 = !flag3 && PlayerData.Instance.GetNumQueuedMissions() == 0 && PlayerData.Instance.GetNumRegularActiveMissions() == 0;
		if (PlayerData.Instance.Act >= 0 && PlayerData.Instance.Act < Singleton.GameData.Acts.Length)
		{
			int maxMissions = Singleton.GameData.Acts[PlayerData.Instance.Act].MaxMissions;
			flag4 = PlayerData.Instance.GetNumRegularActiveMissions() < maxMissions;
		}
		bool continueCurrentAct = false;
		if (!flag2 && !flag3)
		{
			continueCurrentAct = true;
		}
		bool flag6 = false;
		if (flag || flag2 || flag3 || flag4 || flag5)
		{
			GeneratePlot(continueCurrentAct);
			PlayerData.Instance.IsUpToDate = true;
			flag6 = true;
		}
		if (!SurvivalIsAssigned(Mission.Type.SurvivalGates) && SurvivalIsEnabled())
		{
			GenerateSurvival(Mission.Type.SurvivalGates);
		}
		if (!SurvivalIsAssigned(Mission.Type.SurvivalArena) && SurvivalIsEnabled())
		{
			GenerateSurvival(Mission.Type.SurvivalArena);
		}
		if (!SpecialHuntIsAssigned() && !SpecialHuntIsQueued() && SpecialHuntIsEnabled())
		{
			GenerateSpecialHunt();
		}
		if (PlayerData.Instance.IsReadyForStorylineMission())
		{
			GenerateStoryline();
			AssignStorylineMissions();
		}
		bool flag7 = PlayerData.Instance.GetNumQueuedMissions() > 0;
		if (!flag6 && flag4 && flag7)
		{
			int numQueuedMissions = PlayerData.Instance.GetNumQueuedMissions();
			AssignStackedMissions();
			int numQueuedMissions2 = PlayerData.Instance.GetNumQueuedMissions();
			Debug.Log("Assigned " + (numQueuedMissions2 - numQueuedMissions) + " queued missions.");
		}
		bool flag8 = PlayerData.Instance.GetNumGoals() <= MaxNumGoals && PlayerData.Instance.Ticket < Singleton.GameData.Goals.Length;
		if (flag2 || flag8)
		{
			GenerateGoals();
		}
		bool flag9 = !PlayerData.Instance.HasCumulativeGoal() && PlayerData.Instance.CumulativeTicket < Singleton.GameData.CumulativeGoals.Length;
		if (flag2 || flag9)
		{
			GenerateCumulativeGoal();
		}
		Debug.Log(string.Format("LEVEL: {0}, XP:{1}, NEXT:{2}, STORYLINE: {3}", PlayerData.Instance.Level, PlayerData.Instance.XP, PlayerData.Instance.NextLevelXP, PlayerData.Instance.StorylineTicket));
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	public int GetMaxMissionIndex()
	{
		return Singleton.GameData.Missions.Length - 1;
	}

	public GameData.MissionData GetMissionByIndex(int missionIndex)
	{
		return Singleton.GameData.Missions[missionIndex];
	}

	public GameObject GetMissionButton(int missionIndex)
	{
		if (!PlayerData.Instance.IsSpecialHuntReceived && missionIndex == Singleton.ShopData.SpecialHunt.missionIndex)
		{
			return SpecialHuntButton;
		}
		Mission.Type type = GetMissionByIndex(missionIndex).Type;
		return GetMissionButton(type);
	}

	public GameObject GetMissionButton(Mission.Type missionType)
	{
		if (Mission.IsStorylineMission(missionType))
		{
			missionType = Mission.Type.Story1;
		}
		foreach (MissionButton missionButton in MissionButtons)
		{
			if (missionButton.MissionType == missionType)
			{
				return missionButton.Button;
			}
		}
		return null;
	}

	public Mission.Difficulty SelectedMissionDifficulty
	{
		get
		{
			return Singleton.PlayerData.GetMissionDifficulty(_selectedLocationName);
		}
	}

	public int SelectedMissionIndex
	{
		get
		{
			return _selectedMissionIndex;
		}
	}

	public GameData.MissionData SelectedMission
	{
		get
		{
			return GetMissionByIndex(_selectedMissionIndex);
		}
	}

	public GameData.MissionDescription SelectedMissionDescription
	{
		get
		{
			string missionDescriptor = Singleton.PlayerData.GetMissionDescriptor(_selectedLocationName);
			return Singleton.GameData.GetMissionDescription(missionDescriptor, _selectedMissionIndex);
		}
	}

	public GameData.SceneData SelectedMissionSceneDescription
	{
		get
		{
			return Singleton.GameData.GetMissionSceneDescription(_selectedMissionIndex);
		}
	}

	public string SelectedLocation
	{
		get
		{
			return _selectedLocationName;
		}
	}

	public void ShowMissionDialogBox(MapLocation location, int missionIndex)
	{
		BroadcastMessage("RollupAllGoals");
		_selectedLocationName = location.gameObject.name;
		_selectedMissionIndex = missionIndex;
		GameData.MissionData missionByIndex = GetMissionByIndex(missionIndex);
		bool flag = Mission.IsStorylineMission(missionByIndex.Type);
		if (flag && Singleton.PlayerData.StorylineProgress < Singleton.GameData.Acts[Singleton.PlayerData.Act].Required)
		{
			Debug.Log(string.Format("Storyline mission #{0} is locked; it is required {1} missions to complete while player has only {2}", Singleton.PlayerData.StorylineTicket, Singleton.PlayerData.StorylineProgress, Singleton.GameData.Acts[Singleton.PlayerData.Act].Required));
			Singleton.UI.PopupManager.ShowMissionLockedPopup(this, location);
		}
		else if (Singleton.GameData.InfectionLevel.Enabled && Singleton.GameData.InfectionLevel.ZombieBombEnabled && Singleton.PlayerData.IsInfectionLevelCritical() && !flag)
		{
			Singleton.UI.PopupManager.ShowInfectionLevelPopup(this, location);
		}
		else if (!Singleton.PlayerData.IsSpecialHuntReceived && missionIndex == Singleton.ShopData.SpecialHunt.missionIndex)
		{
			Singleton.UI.PopupManager.ShowSpecialHuntPopup(this, location);
		}
		else
		{
			Singleton.UI.PopupManager.ShowMissionInfoPopup(this, location);
		}
	}

	public static MapNarrative Instance
	{
		get
		{
			return _instance;
		}
	}
}
