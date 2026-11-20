using System;
using System.Collections.Generic;
using System.IO;
using Glu.RuntimeDataProtection;
using UnityEngine;

public class PlayerData : MonoBehaviourAndLog
{
	public enum Slots
	{
		handgun,
		shotgun,
		assault,
		sniper,
		exotic,
		melee
	}

	public class ArenaGoalInfo
	{
		public string type;

		public int counter;

		public static int LimitForType(string type, GameData.ArenaGoalsData goal)
		{
			return type == "a" ? goal.a
				: type == "b" ? goal.b
				: type == "c" ? goal.c
				: type == "d" ? goal.d
				: -1;
		}

		public static float RewardModForType(string type, GameData.ArenaGoalsData goal)
		{
			return type == "a" ? 1f
				: type == "b" ? goal.RewardB
				: type == "c" ? goal.RewardC
				: type == "d" ? goal.RewardD
				: -1f;
		}
	}

	public class Data
	{
		public int GameDataHashCode;

		public Encrypted<int> HardCurrency = new Encrypted<int>(0);

		public Encrypted<int> SoftCurrency = new Encrypted<int>(0);

		public Encrypted<int> Xp = new Encrypted<int>(0);

		public Encrypted<int> LevelUpState = new Encrypted<int>(0);

		public Encrypted<int> Act = new Encrypted<int>(-1);

		public Encrypted<int> Ticket = new Encrypted<int>(0);

		public Encrypted<int> CumulativeTicket = new Encrypted<int>(0);

		public Encrypted<int> StorylineTicket = new Encrypted<int>(0);

		public Encrypted<int> TutorialState = new Encrypted<int>(0);

		public XmlDictionary<string, int> Consumables = new XmlDictionary<string, int>();

		public XmlDictionary<string, int> Weapons = new XmlDictionary<string, int>();

		public XmlDictionary<Slots, string> EquippedWeapons = new XmlDictionary<Slots, string>();

		public XmlDictionary<string, int> ActiveMissions = new XmlDictionary<string, int>();

		public XmlDictionary<int, int> ActiveSeeds = new XmlDictionary<int, int>();

		public List<int> QueuedMissions = new List<int>();

		public XmlDictionary<Goal.Type, Goal> Goals = new XmlDictionary<Goal.Type, Goal>();

		public List<CumulativeGoal> CumulativeGoals = new List<CumulativeGoal>();

		public List<int> PendingTickets = new List<int>();

		public PlayerStatistics Statistics = new PlayerStatistics();

		public HashSet<int> Achievements = new HashSet<int>();

		public XmlDictionary<string, ArenaGoalInfo> ArenaGoals = new XmlDictionary<string, ArenaGoalInfo>();

		public HashSet<string> CompletedArenaGoals = new HashSet<string>();

		public List<GameAnalytics.AfterTutorialEvent> AfterTutorialEvents = new List<GameAnalytics.AfterTutorialEvent>();

		public DateTime LastArenaDailyGoalReceived = DateTime.MinValue;

		public int LastPlayedMission = -1;

		public Slots RecentlyEquipped;

		public float InfectionLevel;

		public long LastInfectionLevelUpdateTime;

		public int StartingBonusCollectedNumber;

		public float PlayingTime;

		public float LevelTime;

		public bool PlayerReceivedTutorialHardCurrency;

		public int LastStartedLimitedTimeOfferVersion = -1;

		public bool IsLimitedTimeOfferReceived;

		public DateTime LimitedTimeOfferStartTime = DateTime.MinValue;

		public bool IsLimitedTimeOfferPopupShown;

		public int LastStartedSpecialHuntVersion = -1;

		public bool IsSpecialHuntReceived = true;

		public DateTime SpecialHuntStartTime = DateTime.MinValue;

		public bool RateMeAccepted;

		public int TutorialAnalyticsStep = -1;

		public bool FacebookLoggedInOnce;

		public int StorylineProgress;
	}

	public struct SessionStatistics
	{
		public int Level;

		public int MissionsCompleted;

		public SessionStatistics(bool flag)
		{
			Level = 0;
			MissionsCompleted = 0;
		}
	}

	public delegate void OnCurrencyChangedDelegate(int valueBefore, int valueNow);

	public delegate void OnXPChangedDelegate(int valueBefore, int valueNow);

	public delegate void OnWeaponEquippedDelegate(string item);

	private bool _updateAllowed = true;

	private bool _saveOnce;

	private Data _data = new Data();

	private bool _flush;

	private MissionStatistics _missionStatistics = new MissionStatistics();

	private Dictionary<Goal.Type, int> _missionProgress = new Dictionary<Goal.Type, int>();

	private string _currentLocation = string.Empty;

	public string startFrom = string.Empty;

	private bool _isSpecialHuntPlayed;

	private string _lastCompletedMissionLocation = string.Empty;

	private int _lastCompletedMission = -1;

	private int _lastCompletedMissionSeed;

	private bool _lastCompletedMissionIsSpecialHunt;

	private static PlayerData _instance = null;

	public OnCurrencyChangedDelegate OnHardCurrencyChanged = delegate
	{
	};

	public OnCurrencyChangedDelegate OnSoftCurrencyChanged = delegate
	{
	};

	public OnXPChangedDelegate OnXPChanged = delegate
	{
	};

	private int currLevel = -1;

	public OnWeaponEquippedDelegate OnWeaponEquipped = delegate
	{
	};

	public Dictionary<string, ArenaGoalInfo> LastCompletedArenaGoals = new Dictionary<string, ArenaGoalInfo>();

	private static SessionStatistics _session = new SessionStatistics(true);

	public bool UpdateAllowed
	{
		get
		{
			return _updateAllowed;
		}
		set
		{
			_updateAllowed = value;
		}
	}

	public void SaveOnce()
	{
		_saveOnce = true;
	}

	public static PlayerData Instance
	{
		get
		{
			return _instance;
		}
	}

	public bool IsUpToDate
	{
		get
		{
			int gameDataHashCode = _data.GameDataHashCode;
			int gameDataHashCode2 = Singleton.GameData.GameDataHashCode;
			return gameDataHashCode == gameDataHashCode2;
		}
		set
		{
			if (value)
			{
				_data.GameDataHashCode = Singleton.GameData.GameDataHashCode;
			}
			else
			{
				_data.GameDataHashCode = 0;
			}
			_flush = true;
		}
	}

	public int HardCurrency
	{
		get
		{
			return _data.HardCurrency;
		}
		set
		{
			int valueBefore = _data.HardCurrency;
			_data.HardCurrency = value;
			OnHardCurrencyChanged(valueBefore, _data.HardCurrency);
			_flush = true;
		}
	}

	public int SoftCurrency
	{
		get
		{
			return _data.SoftCurrency;
		}
		set
		{
			int num = _data.SoftCurrency;
			_data.SoftCurrency = value;
			OnSoftCurrencyChanged(num, _data.SoftCurrency);
			if ((int)_data.SoftCurrency > num)
			{
				GameAnalytics.LogSCReceived((int)_data.SoftCurrency - num);
			}
			_flush = true;
		}
	}

	public int MoneyDelta(string currency, int money)
	{
		if (currency == "hard")
		{
			return HardCurrency - money;
		}
		if (currency == "soft")
		{
			return SoftCurrency - money;
		}
		return -1;
	}

	public bool IsEnoughMoney(string currency, int money)
	{
		return MoneyDelta(currency, money) >= 0;
	}

	public bool ChangeMoney(string currency, int money)
	{
		if (currency == "hard")
		{
			if (HardCurrency + money >= 0)
			{
				HardCurrency += money;
				return true;
			}
			return false;
		}
		if (currency == "soft")
		{
			if (SoftCurrency + money >= 0)
			{
				SoftCurrency += money;
				return true;
			}
			return false;
		}
		return false;
	}

	public int XP
	{
		get
		{
			return _data.Xp;
		}
		set
		{
			int num = _data.Xp;
			int level = Level;
			_data.Xp = Mathf.Clamp(value, 0, Singleton.GameData.MaxPlayerXP);
			currLevel = GetLevel(_data.Xp);
			if (level < currLevel)
			{
				_data.LevelUpState = 2;
				_session.Level = currLevel;
				SoftCurrency += Singleton.GameData.GetPlayerLevelData(currLevel).LevelUpSC;
				GameAnalytics.LogLevelAchieved(Level, _data.SoftCurrency, _data.HardCurrency);
				GameAnalytics.LogLevelTime(Level, _data.LevelTime);
				_data.LevelTime = 0f;
				_data.InfectionLevel = 0f;
			}
			_flush = true;
			UnityEngine.Debug.Log("XP earned:" + ((int)_data.Xp - num));
			OnXPChanged(num, _data.Xp);
		}
	}

	public int Level
	{
		get
		{
			return !(currLevel == -1) ? currLevel : (currLevel = GetLevel(XP));
		}
	}

	public int NextLevelXP
	{
		get
		{
			return GetLevelXP(Level + 1);
		}
	}

	public int PrevLevelXP
	{
		get
		{
			return GetLevelXP(Level - 1);
		}
	}

	public int CurrLevelXP
	{
		get
		{
			return GetLevelXP(Level);
		}
	}

	public int GetLevelXP(int level)
	{
		if (level > Singleton.GameData.MaxPlayerLevel)
		{
			return int.MaxValue;
		}
		int num = 0;
		for (level--; level > 0; level--)
		{
			num += Singleton.GameData.PlayerLevels[level].NextLevelXP;
		}
		return num;
	}

	public int GetLevel(int xp)
	{
		int maxPlayerLevel = Singleton.GameData.MaxPlayerLevel;
		int num = 0;
		for (int i = 0; i <= maxPlayerLevel; i++)
		{
			GameData.PlayerLevelData playerLevelData = Singleton.GameData.PlayerLevels[i];
			if (xp >= num)
			{
				num += playerLevelData.NextLevelXP;
				continue;
			}
			return i - 1;
		}
		return maxPlayerLevel;
	}

	public int LevelUpState
	{
		get
		{
			return _data.LevelUpState;
		}
		set
		{
			_data.LevelUpState = value;
			_flush = true;
		}
	}

	public bool HasWeapon(string item)
	{
		return _data.Weapons.ContainsKey(item);
	}

	public void AddWeapon(string item)
	{
		if (_data.Weapons.ContainsKey(item))
		{
		}
		else
		{
			_data.Weapons.Add(item, 1);
		}
		_flush = true;
	}

	public Slots RecentlyEquipped
	{
		get
		{
			return _data.RecentlyEquipped;
		}
		set
		{
			_data.RecentlyEquipped = value;
		}
	}

	public void EquipWeapon(Slots slot, string weapon)
	{
		if (!(HasWeapon(weapon)))
		{
			return;
		}
		_data.EquippedWeapons[slot] = weapon;
		_data.RecentlyEquipped = slot;
		OnWeaponEquipped(weapon);
		_flush = true;
	}

	public bool IsEquipped(Slots slot, string item)
	{
		return _data.EquippedWeapons.ContainsKey(slot) && _data.EquippedWeapons[slot] == item;
	}

	public string GetEquippedItem(Slots slot)
	{
		if (!_data.EquippedWeapons.ContainsKey(slot))
		{
			return null;
		}
		return _data.EquippedWeapons[slot];
	}

	public void AddConsumable(string item, int count)
	{
		if (_data.Consumables.ContainsKey(item))
		{
			_data.Consumables[item] += count;
		}
		else
		{
			_data.Consumables.Add(item, count);
		}
		_flush = true;
	}

	public void ChangeConsumableCount(string item, int delta)
	{
		int consumableCount = GetConsumableCount(item);
		consumableCount += delta;
		if (consumableCount < 0)
		{
			consumableCount = 0;
		}
		SetConsumableCount(item, consumableCount);
	}

	public void SetConsumableCount(string item, int count)
	{
		if (_data.Consumables.ContainsKey(item))
		{
			_data.Consumables[item] = count;
		}
		else
		{
			_data.Consumables.Add(item, count);
		}
		_flush = true;
	}

	public int GetConsumableCount(string item)
	{
		return _data.Consumables.ContainsKey(item) ? _data.Consumables[item] : 0;
	}

	public XmlDictionary<string, int> GetConsumables()
	{
		return _data.Consumables;
	}

	public int GetNumQueuedMissions()
	{
		return _data.QueuedMissions.Count;
	}

	public int PopMission()
	{
		if (GetNumQueuedMissions() > 0)
		{
			int result = _data.QueuedMissions[0];
			_data.QueuedMissions.RemoveAt(0);
			_flush = true;
			return result;
		}
		return -1;
	}

	public void QueueMission(int missionIndex, bool shuffle)
	{
		if (shuffle && _data.QueuedMissions.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, _data.QueuedMissions.Count);
			_data.QueuedMissions.Insert(index, missionIndex);
		}
		else
		{
			_data.QueuedMissions.Add(missionIndex);
		}
		_flush = true;
	}

	public bool IsMissionQueued(int missionIndex)
	{
		foreach (int queuedMission in _data.QueuedMissions)
		{
			if (queuedMission == missionIndex)
			{
				return true;
			}
		}
		return false;
	}

	public float GetQueuedLocationRarenessForMission(int missionIndex)
	{
		GameData.MissionLocation location = Singleton.GameData.Missions[missionIndex].Location;
		int num = 0;
		foreach (int queuedMission in _data.QueuedMissions)
		{
			GameData.MissionLocation location2 = Singleton.GameData.Missions[queuedMission].Location;
			if (location2 == location)
			{
				num++;
			}
		}
		return (float)num / (float)_data.QueuedMissions.Count;
	}

	public void CleanupQueue()
	{
		_data.QueuedMissions.Clear();
		_flush = true;
	}

	public bool IsAchievementCompleted(GameData.AchievementData achievement)
	{
		return _data.Achievements.Contains(achievement.Id);
	}

	public void CompleteAchievement(GameData.AchievementData achievement)
	{
		_data.Achievements.Add(achievement.Id);
		_flush = true;
	}

	public int LastStartedLimitedTimeOfferVersion
	{
		get
		{
			return _data.LastStartedLimitedTimeOfferVersion;
		}
		set
		{
			_data.LastStartedLimitedTimeOfferVersion = value;
			_flush = true;
		}
	}

	public bool IsLimitedTimeOfferReceived
	{
		get
		{
			return _data.IsLimitedTimeOfferReceived;
		}
		set
		{
			_data.IsLimitedTimeOfferReceived = value;
			_flush = true;
		}
	}

	public bool IsLimitedTimeOfferPopupShown
	{
		get
		{
			return _data.IsLimitedTimeOfferPopupShown;
		}
		set
		{
			_data.IsLimitedTimeOfferPopupShown = value;
			_flush = true;
		}
	}

	public DateTime LimitedTimeOfferStartTime
	{
		get
		{
			return _data.LimitedTimeOfferStartTime;
		}
		set
		{
			_data.LimitedTimeOfferStartTime = value;
			_flush = true;
		}
	}

	public bool RateMeAccepted
	{
		get
		{
			return _data.RateMeAccepted;
		}
		set
		{
			_data.RateMeAccepted = value;
			_flush = true;
		}
	}

	public int TutorialAnalyticsStep
	{
		get
		{
			return _data.TutorialAnalyticsStep;
		}
		set
		{
			_data.TutorialAnalyticsStep = value;
			_flush = true;
		}
	}

	public bool FacebookLoggedInOnce
	{
		get
		{
			return _data.FacebookLoggedInOnce;
		}
		set
		{
			_data.FacebookLoggedInOnce = value;
			_flush = true;
		}
	}

	public int LastStartedSpecialHuntVersion
	{
		get
		{
			return _data.LastStartedSpecialHuntVersion;
		}
		set
		{
			_data.LastStartedSpecialHuntVersion = value;
			_flush = true;
		}
	}

	public bool IsSpecialHuntPlayed
	{
		get
		{
			return _isSpecialHuntPlayed;
		}
		set
		{
			_isSpecialHuntPlayed = value;
		}
	}

	public bool IsSpecialHuntReceived
	{
		get
		{
			return _data.IsSpecialHuntReceived;
		}
		set
		{
			_data.IsSpecialHuntReceived = value;
			_flush = true;
		}
	}

	public DateTime SpecialHuntStartTime
	{
		get
		{
			return _data.SpecialHuntStartTime;
		}
		set
		{
			_data.SpecialHuntStartTime = value;
			_flush = true;
		}
	}

	public int TutorialState
	{
		get
		{
			return _data.TutorialState;
		}
		set
		{
			_data.TutorialState = value;
			_flush = true;
		}
	}

	public int Act
	{
		get
		{
			return _data.Act;
		}
		set
		{
			_data.Act = value;
			_flush = true;
		}
	}

	public int StorylineProgress
	{
		get
		{
			return _data.StorylineProgress;
		}
		set
		{
			_data.StorylineProgress = value;
			_flush = true;
		}
	}

	public int LastPlayedMissionIndex
	{
		get
		{
			return _data.LastPlayedMission;
		}
		set
		{
			_data.LastPlayedMission = value;
		}
	}

	public string CurrentLocation
	{
		get
		{
			return _currentLocation;
		}
		set
		{
			_currentLocation = value;
		}
	}

	public int CurrentMissionIndex
	{
		get
		{
			return !(!HasMission(CurrentLocation)) ? GetMission(CurrentLocation) : (-1);
		}
	}

	public Mission.Type CurrentMissionType
	{
		get
		{
			return !(!HasMission(CurrentLocation)) ? GetCurrentMissionInfo().Type : Mission.Type.KillAll;
		}
	}

	public string CurrentMissionDescriptor
	{
		get
		{
			return !(!HasMission(CurrentLocation)) ? GetMissionDescriptor(CurrentLocation) : string.Empty;
		}
	}

	public Mission.Difficulty CurrentMissionDifficulty
	{
		get
		{
			return HasMission(CurrentLocation) ? GetMissionDifficulty(CurrentLocation) : Mission.Difficulty.Normal;
		}
	}

	public int StorylineTicket
	{
		get
		{
			return _data.StorylineTicket;
		}
		set
		{
			_data.StorylineTicket = Mathf.Clamp(value, 0, Singleton.GameData.GetStorylineLength());
			_flush = true;
		}
	}

	public int StartingBonusCollectedNumber
	{
		get
		{
			return _data.StartingBonusCollectedNumber;
		}
	}

	public float InfectionLevel
	{
		get
		{
			return _data.InfectionLevel;
		}
		set
		{
			_data.InfectionLevel = Mathf.Clamp(value, 0f, Singleton.GameData.InfectionLevel.Max);
		}
	}

	public bool PlayerReceivedTutorialHardCurrency
	{
		get
		{
			return _data.PlayerReceivedTutorialHardCurrency;
		}
		set
		{
			_data.PlayerReceivedTutorialHardCurrency = value;
			_flush = true;
		}
	}

	public float MinutesLeftTillInfectionLevel(float infectionLevel)
	{
		return Mathf.Max(0f, (_data.InfectionLevel - infectionLevel) * (1f / Singleton.GameData.InfectionLevel.DecreasePerMin));
	}

	public bool IsInfectionLevelCritical()
	{
		return _data.InfectionLevel + (float)Singleton.GameData.GetCurrentPlayerLevelData().InfectionLevelPerMission >= Singleton.GameData.InfectionLevel.Max;
	}

	public Mission.Type RarestMissionType
	{
		get
		{
			Dictionary<Mission.Type, int> dictionary = new Dictionary<Mission.Type, int>();
			foreach (Mission.Type value in Enum.GetValues(typeof(Mission.Type)))
			{
				if (dictionary.ContainsKey(value))
				{
					continue;
				}
				if (Mission.IsUsualMission(value))
				{
					dictionary.Add(value, 0);
				}
			}
			foreach (KeyValuePair<string, int> activeMission in _data.ActiveMissions)
			{
				Mission.Type type2 = Singleton.GameData.Missions[activeMission.Value].Type;
				if (dictionary.ContainsKey(type2))
				{
					dictionary[type2] = dictionary[type2] + 1;
				}
			}
			int num = int.MaxValue;
			Mission.Type result = Mission.Type.Story25;
			foreach (KeyValuePair<Mission.Type, int> item in dictionary)
			{
				if (item.Value < num)
				{
					num = item.Value;
					result = item.Key;
				}
			}
			return result;
		}
	}

	public bool IsMissionLocked(int missionIndex)
	{
		return false;
	}

	public void AddMission(string location, int missionIndex, int generationSeed)
	{
		if (HasMission(location))
		{
		}
		else
		{
			_data.ActiveMissions.Add(location, missionIndex);
			_data.ActiveSeeds.Add(missionIndex, generationSeed);
			_flush = true;
		}
	}

	public void RequeueMission(string location)
	{
		if (HasMission(location))
		{
			QueueMission(_data.ActiveMissions[location], false);
			_data.ActiveSeeds.Remove(_data.ActiveMissions[location]);
			_data.ActiveMissions.Remove(location);
			_flush = true;
		}
		else
		{
		}
	}

	public void RemoveMission(string location)
	{
		if (HasMission(location))
		{
			_data.ActiveSeeds.Remove(_data.ActiveMissions[location]);
			_data.ActiveMissions.Remove(location);
			_flush = true;
		}
		else
		{
		}
	}

	public bool IsReadyForStorylineMission()
	{
		if (StorylineTicket >= Singleton.GameData.GetStorylineLength())
		{
			return false;
		}
		return true;
	}

	public bool HasMission(string location)
	{
		return _data.ActiveMissions.ContainsKey(location);
	}

	public bool HasMission(int missionIndex)
	{
		foreach (KeyValuePair<string, int> activeMission in _data.ActiveMissions)
		{
			if (activeMission.Value == missionIndex)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMission(string location)
	{
		if (HasMission(location))
		{
			return _data.ActiveMissions[location];
		}
		return -1;
	}

	public int GetMissionGenerationSeed(string location)
	{
		if (HasMission(location))
		{
			return _data.ActiveSeeds[_data.ActiveMissions[location]];
		}
		return 0;
	}

	public string GetMissionDescriptor(string location)
	{
		if (HasMission(location))
		{
			int num = _data.ActiveMissions[location];
			int generationSeed = _data.ActiveSeeds[num];
			return Singleton.GameData.GenerateMissionDescriptor(num, generationSeed);
		}
		return string.Empty;
	}

	public Mission.Difficulty GetMissionDifficulty(int missionIndex, int generationSeed)
	{
		if (Singleton.GameData.InfectionLevel.Enabled && Singleton.GameData.InfectionLevel.InfectionAffectsDifficulty && Singleton.PlayerData.IsInfectionLevelCritical())
		{
			return Mission.Difficulty.Hard;
		}
		if (Singleton.GameData.MissionPoolAffectsDifficulty)
		{
			return Singleton.GameData.GenerateMissionDifficulty(missionIndex, generationSeed);
		}
		return Mission.Difficulty.Normal;
	}

	public Mission.Difficulty GetMissionDifficulty(string location)
	{
		if (HasMission(location))
		{
			int num = _data.ActiveMissions[location];
			int generationSeed = _data.ActiveSeeds[num];
			return GetMissionDifficulty(num, generationSeed);
		}
		return Mission.Difficulty.Normal;
	}

	public GameData.MissionData GetCurrentMissionInfo()
	{
		if (!HasMission(CurrentLocation))
		{
			return null;
		}
		return Singleton.GameData.Missions[GetMission(CurrentLocation)];
	}

	public GameData.SceneData GetCurrentSceneData()
	{
		GameData.MissionData currentMissionInfo = Singleton.PlayerData.GetCurrentMissionInfo();
		return !(currentMissionInfo == null) ? Singleton.GameData.GetSceneData(currentMissionInfo.Scene) : null;
	}

	public void CompleteMission(string location)
	{
		if (HasMission(location))
		{
			ApplyLocalProgress();
			_lastCompletedMissionLocation = location;
			_lastCompletedMission = _data.ActiveMissions[location];
			_lastCompletedMissionSeed = _data.ActiveSeeds[_data.ActiveMissions[location]];
			_lastCompletedMissionIsSpecialHunt = !IsSpecialHuntReceived && _lastCompletedMission == Singleton.ShopData.SpecialHunt.missionIndex;
			_data.ActiveSeeds.Remove(_data.ActiveMissions[location]);
			_data.ActiveMissions.Remove(location);
			_flush = true;
			_session.MissionsCompleted++;
		}
		else
		{
		}
	}

	public int GetNumActiveMissions()
	{
		return _data.ActiveMissions.Count;
	}

	public int GetNumRegularActiveMissions()
	{
		int num = GetNumActiveMissions();
		foreach (KeyValuePair<string, int> activeMission in _data.ActiveMissions)
		{
			Mission.Type type = Singleton.GameData.Missions[activeMission.Value].Type;
			if (Mission.IsSurvival(type) || Mission.IsStorylineMission(type))
			{
				num--;
			}
		}
		return num;
	}

	public void CleanupActiveMissions()
	{
		_data.ActiveMissions.Clear();
		_data.ActiveSeeds.Clear();
		_flush = true;
	}

	public string LastCompletedMissionLocation
	{
		get
		{
			return _lastCompletedMissionLocation;
		}
	}

	public int LastCompletedMission
	{
		get
		{
			return _lastCompletedMission;
		}
	}

	public int LastCompletedMissionSeed
	{
		get
		{
			return _lastCompletedMissionSeed;
		}
	}

	public bool LastCompletedMissionIsSpecialHunt
	{
		get
		{
			return _lastCompletedMissionIsSpecialHunt;
		}
	}

	public void CleanupLastCompletedMissionInfo()
	{
		_lastCompletedMissionLocation = string.Empty;
		_lastCompletedMission = -1;
		_lastCompletedMissionSeed = 0;
		_lastCompletedMissionIsSpecialHunt = false;
	}

	public int CumulativeTicket
	{
		get
		{
			return _data.CumulativeTicket;
		}
		set
		{
			_data.CumulativeTicket = value;
			_flush = true;
		}
	}

	public bool HasCumulativeGoal()
	{
		return _data.CumulativeGoals.Count > 0;
	}

	public CumulativeGoal GetCumulativeGoal()
	{
		if (HasCumulativeGoal())
		{
			return _data.CumulativeGoals[0];
		}
		return null;
	}

	public void SetCumulativeGoal(CumulativeGoal cumulativeGoal)
	{
		_data.CumulativeGoals.Clear();
		_data.CumulativeGoals.Add(cumulativeGoal);
		_flush = true;
	}

	public void ProgressCumulativeGoal(int progression)
	{
		if (HasCumulativeGoal())
		{
			CumulativeGoal cumulativeGoal = _data.CumulativeGoals[0];
			cumulativeGoal.Progress += progression;
			_data.CumulativeGoals[0] = cumulativeGoal;
			_flush = true;
		}
		else
		{
			throw new Exception(string.Format("Player has no cumulative goal"));
		}
	}

	public void CleanupCumulativeGoal()
	{
		_data.CumulativeGoals.Clear();
		_flush = true;
	}

	public int NumPendingTickets
	{
		get
		{
			return _data.PendingTickets.Count;
		}
	}

	public int GetPendingTicket(int index)
	{
		return _data.PendingTickets[index];
	}

	public void AddPendingTicket(int ticket)
	{
		bool flag = _data.PendingTickets.Remove(ticket);
		_data.PendingTickets.Add(ticket);
		_flush = true;
	}

	public bool RemovePendingTicket(int ticket)
	{
		var result = _data.PendingTickets.Remove(ticket);
		return result;
	}

	public int Ticket
	{
		get
		{
			return _data.Ticket;
		}
		set
		{
			_data.Ticket = value;
			_flush = true;
		}
	}

	public bool HasGoals()
	{
		return _data.Goals.Count > 0;
	}

	public int GetNumGoals()
	{
		return _data.Goals.Count;
	}

	public bool HasGoal(Goal.Type goalType)
	{
		return _data.Goals.ContainsKey(goalType);
	}

	public Goal GetGoal(Goal.Type goalType)
	{
		if (HasGoal(goalType))
		{
			return _data.Goals[goalType];
		}
		throw new Exception(string.Format("Player has no goal {0}", goalType));
	}

	public Goal.Type[] GetGoals()
	{
		if (HasGoals())
		{
			Goal.Type[] array = new Goal.Type[_data.Goals.Count];
			Dictionary<Goal.Type, Goal>.Enumerator enumerator = _data.Goals.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num] = enumerator.Current.Key;
				num++;
			}
			return array;
		}
		return null;
	}

	public void AddGoal(Goal goal)
	{
		if (!HasGoal(goal.GoalType))
		{
			_data.Goals[goal.GoalType] = goal;
			_flush = true;
		}
		else
		{
			throw new Exception(string.Format("Player already has goal {0}", goal.GoalType));
		}
	}

	public void RemoveGoal(Goal.Type goalType)
	{
		if (HasGoal(goalType))
		{
			_data.Goals.Remove(goalType);
			_flush = true;
		}
	}

	public void CleanupGoals()
	{
		_data.Goals.Clear();
		_flush = true;
	}

	public void CleanupLocalProgress()
	{
		_missionProgress.Clear();
	}

	public void NotifyGoalRewardReceived(Goal.Type goalType, int reward)
	{
		if (HasGoal(goalType))
		{
			Goal goal = GetGoal(goalType);
			goal.Reward -= reward;
			goal.Reward = (!(goal.Reward >= 0) ? 0 : goal.Reward);
			_data.Goals[goalType] = goal;
			_flush = true;
		}
	}

	public void ProgressGoal(Goal.Type goalType)
	{
		ProgressGoal(goalType, 1);
	}

	public void ProgressGoal(Goal.Type goalType, int progression)
	{
		if (_missionProgress.ContainsKey(goalType))
		{
			_missionProgress[goalType] = _missionProgress[goalType] + progression;
		}
		else
		{
			_missionProgress.Add(goalType, progression);
		}
	}

	private void ApplyLocalProgress()
	{
		foreach (KeyValuePair<Goal.Type, int> item in _missionProgress)
		{
			if (HasGoal(item.Key))
			{
				Goal goal = GetGoal(item.Key);
				goal.Progress += item.Value;
				goal.Progress = (!(goal.Progress <= goal.Target) ? goal.Target : goal.Progress);
				_data.Goals[item.Key] = goal;
			}
		}
		CleanupLocalProgress();
		_flush = true;
	}

	public Dictionary<string, ArenaGoalInfo> ArenaGoals
	{
		get
		{
			return _data.ArenaGoals;
		}
	}

	public DateTime LastArenaDailyGoalReceived
	{
		get
		{
			return _data.LastArenaDailyGoalReceived;
		}
		set
		{
			_data.LastArenaDailyGoalReceived = value;
		}
	}

	public HashSet<string> CompletedArenaGoals
	{
		get
		{
			return _data.CompletedArenaGoals;
		}
	}

	private void Awake()
	{
		LogTo("Player.PlayerData");
		_instance = this;
		Load();
		Save();
		Singleton.GameEvents.OnWeaponShot += PlayerStatistics.OnWeaponShot;
		Singleton.GameEvents.OnConsumableUsed += PlayerStatistics.OnConsumableUsed;
		Singleton.GameEvents.OnEnemyHit += PlayerStatistics.OnEnemyHit;
		Singleton.GameEvents.OnEnemyKill += PlayerStatistics.OnEnemyKill;
		Singleton.GameEvents.OnEnemySpawned += PlayerStatistics.OnEnemySpawned;
		Singleton.GameEvents.OnEnemyHeadshot += PlayerStatistics.OnEnemyHeadshot;
		Singleton.GameEvents.OnEnemyDismemberment += PlayerStatistics.OnEnemyDismemberment;
		Singleton.GameEvents.OnEnemyMutilated += PlayerStatistics.OnEnemyMutilated;
		Singleton.GameEvents.OnTaskCompleted += PlayerStatistics.OnTaskCompleted;
		Singleton.GameEvents.OnSupplyGathered += PlayerStatistics.OnSupplyGathered;
		Singleton.GameEvents.OnAmmoResupplied += PlayerStatistics.OnAmmoResupplied;
		Singleton.GameEvents.OnBonusCollected += PlayerStatistics.OnBonusCollected;
		Singleton.GameEvents.OnPlayerKill += PlayerStatistics.OnPlayerKill;
		Singleton.GameEvents.OnMissionStarted += PlayerStatistics.OnMissionStarted;
		Singleton.GameEvents.OnMissionCompleted += PlayerStatistics.OnMissionCompleted;
		Singleton.GameEvents.OnMissionFailed += PlayerStatistics.OnMissionFailed;
		Singleton.GameEvents.OnIAPPurchase += PlayerStatistics.OnIAPPurchase;
		Singleton.GameEvents.OnItemPurchase += PlayerStatistics.OnItemPurchase;
		Singleton.GameEvents.OnIAPPurchase += (Action<bool, int, decimal>)((bool isHC, int amount, decimal price) =>
		{
			_data.StartingBonusCollectedNumber = PlayerStatistics.NumBonusCollected;
		});
		Singleton.GameEvents.OnEnemyHeadshot += (Action<Character, Unit, Weapon, Consumable>)((Character s, Unit x, Weapon y, Consumable z) =>
		{
			if (s is Player)
			{
				ProgressGoal(Goal.Type.Headshot);
			}
		});
		Singleton.GameEvents.OnEnemyDismemberment += (Action<Character, Unit, Weapon, Consumable, bool>)((Character s, Unit x, Weapon y, Consumable z, bool zz) =>
		{
			if (s is Player)
			{
				ProgressGoal(Goal.Type.Dismember);
			}
		});
		Singleton.GameEvents.OnWeaponShot += (Action<Character, Weapon, int>)((Character source, Weapon weapon, int amount) =>
		{
			if (!(source is Player))
			{
				return;
			}
			if (weapon is Firearm)
			{
				switch ((weapon as Firearm).Ammunition)
				{
				case Weapon.AmmoType.PistolCartridge:
					ProgressGoal(Goal.Type.HandgunShots);
					break;
				case Weapon.AmmoType.ShotgunShell:
					ProgressGoal(Goal.Type.ShotgunShots);
					break;
				case Weapon.AmmoType.SMGCartridge:
				case Weapon.AmmoType.AssaultRifleCartridge:
				case Weapon.AmmoType.MGCartridge:
					ProgressGoal(Goal.Type.ASRShots);
					break;
				case Weapon.AmmoType.SniperRifleCartridge:
					ProgressGoal(Goal.Type.SniperShots);
					break;
				}
			}
		});
		Singleton.GameEvents.OnEnemyKill += (Action<Character, Unit, Weapon, Consumable>)((Character source, Unit unit, Weapon weapon, Consumable consumable) =>
		{
			if (!(source is Player))
			{
				return;
			}
			if ((bool)weapon)
			{
				if (weapon is MeleeWeapon)
				{
					ProgressGoal(Goal.Type.MeleeKill);
				}
				else if (weapon is Firearm)
				{
					switch ((weapon as Firearm).Ammunition)
					{
					case Weapon.AmmoType.PistolCartridge:
					case Weapon.AmmoType.SMGCartridge:
						ProgressGoal(Goal.Type.HandgunKill);
						break;
					case Weapon.AmmoType.ShotgunShell:
						ProgressGoal(Goal.Type.ShotgunKill);
						break;
					case Weapon.AmmoType.AssaultRifleCartridge:
						ProgressGoal(Goal.Type.ASRKill);
						break;
					case Weapon.AmmoType.SniperRifleCartridge:
						ProgressGoal(Goal.Type.SniperKill);
						break;
					case Weapon.AmmoType.Grenade:
						ProgressGoal(Goal.Type.ExplodeKill);
						break;
					}
				}
				else if (weapon is Launcher)
				{
					ProgressGoal(Goal.Type.ExplodeKill);
				}
			}
			else if ((bool)consumable && consumable is Grenade)
			{
				ProgressGoal(Goal.Type.ExplodeKill);
			}
		});
		Singleton.GameEvents.OnMissionStarted += (Action<string>)((string locationName) =>
		{
			if (!HasMission(locationName))
			{
				return;
			}
			int mission = GetMission(locationName);
			Mission.Type type = Singleton.GameData.Missions[mission].Type;
			bool isSpecialHunt = !IsSpecialHuntReceived && mission == Singleton.ShopData.SpecialHunt.missionIndex;
			if (isSpecialHunt)
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.SpecialHuntStarted);
			}
			else if (Mission.IsUsualMission(type))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.FirstPoolMissionStarted);
			}
			else if (Mission.IsSurvivalGates(type))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.SurvivalModeStarted);
			}
			else if (Mission.IsSurvivalArena(type))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.ArenaModeStarted);
			}
		});
		Singleton.GameEvents.OnMissionCompleted += (Action<string>)((string locationName) =>
		{
			if (!HasMission(locationName))
			{
				return;
			}
			int mission = GetMission(locationName);
			Mission.Type type = Singleton.GameData.Missions[mission].Type;
			bool isSpecialHunt = !IsSpecialHuntReceived && mission == Singleton.ShopData.SpecialHunt.missionIndex;
			if (isSpecialHunt)
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.SpecialHuntCompleted);
			}
			else if (Mission.IsUsualMission(type))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.FirstPoolMissionCompleted);
			}
			else if (Mission.IsSurvivalGates(type))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.SurvivalModeCompleted);
			}
			else if (Mission.IsSurvivalArena(type))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.ArenaModeCompleted);
			}
		});
		Singleton.GameEvents.OnMissionFailed += (Action<string, GameEvents.MissionFailure, Mission.Type>)((string locationName, GameEvents.MissionFailure failureType, Mission.Type missionType) =>
		{
			if (!HasMission(locationName))
			{
				return;
			}
			if (Mission.IsSurvivalGates(missionType))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.SurvivalModeCompleted);
			}
			else if (Mission.IsSurvivalArena(missionType))
			{
				TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent.ArenaModeCompleted);
			}
		});
		_session.Level = Level;
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Update()
	{
		if (Time.timeScale > 0f)
		{
			_data.PlayingTime += Time.deltaTime / Time.timeScale;
			_data.LevelTime += Time.deltaTime / Time.timeScale;
		}
		if (_updateAllowed || _saveOnce)
		{
			long ticks = DateTime.Now.Ticks;
			double num = TimeSpan.FromTicks(ticks - _data.LastInfectionLevelUpdateTime).TotalMinutes;
			if (num < 0.0)
			{
				num = 0.0;
			}
			InfectionLevel -= (float)(num * (double)Singleton.GameData.InfectionLevel.DecreasePerMin);
			_data.LastInfectionLevelUpdateTime = ticks;
			if (_flush || _data.Statistics.Flush || _saveOnce)
			{
				_flush = false;
				_data.Statistics.Flush = false;
				_saveOnce = false;
				Save();
			}
		}
	}

	public void ResetProgress()
	{
		File.Delete(Path.Combine(StorageManager.PersistentDataPath, Const.SaveFileName));
		Application.LoadLevel(0);
	}

	private void FirstStart()
	{
		// PC port: start with 999,999 of both currencies (original: HardCurrency = 3, SoftCurrency = GetPlayerLevelData(1).LevelUpSC, which is 0).
		// Write the save fields directly: FirstStart runs inside PlayerData.Awake, before Singleton._playerData is assigned,
		// and the SoftCurrency setter logs any increase through GameAnalytics.GetPlayerLevel() -> Singleton.PlayerData.Level,
		// which throws a NullReferenceException there and aborts the rest of FirstStart (no XP, weapons or consumables).
		// The setters' other side effects are not needed here: nothing is subscribed to the change events yet,
		// and _flush is set at the end of this method.
		_data.HardCurrency = 999999;
		_data.SoftCurrency = 999999;
		XP = 1;
		AddConsumable(Singleton.GameData.StartConsumable, Singleton.GameData.StartConsumableCount);
		AddWeapon(Singleton.GameData.StartHandgunWeapon);
		AddWeapon(Singleton.GameData.StartMeleeWeapon);
		EquipWeapon(Slots.melee, Singleton.GameData.StartMeleeWeapon);
		EquipWeapon(Slots.handgun, Singleton.GameData.StartHandgunWeapon);
		App.FirstGameLaunch = true;
		GameAnalytics.LogFirstStart(Singleton.GameData.StartMeleeWeapon, Singleton.GameData.StartHandgunWeapon, Singleton.GameData.StartConsumable, Singleton.GameData.StartConsumableCount);
		_flush = true;
	}

	private void Load()
	{
		bool flag = true;
		try
		{
			_data = (Data)StorageManager.ReadXml(Const.SaveFileName, _data.GetType());
		}
		catch (Exception)
		{
			flag = false;
		}
		if (!flag)
		{
			FirstStart();
		}
		if (!IsUpToDate)
		{
			CleanupActiveMissions();
			CleanupQueue();
		}
		if (_data.AfterTutorialEvents.Count == 0)
		{
			Array values = Enum.GetValues(typeof(GameAnalytics.AfterTutorialEvent));
			foreach (GameAnalytics.AfterTutorialEvent item in values)
			{
				if (!GameAnalytics.IsRepeatableAfterTutorialEvent(item))
				{
					_data.AfterTutorialEvents.Add(item);
				}
			}
		}
	}

	private void Save()
	{
		StorageManager.WriteXml(Const.SaveFileName, _data);
	}

	public float PlayingTime
	{
		get
		{
			return _data.PlayingTime;
		}
	}

	public PlayerStatistics PlayerStatistics
	{
		get
		{
			return _data.Statistics;
		}
	}

	public MissionStatistics LastMissionStatistics
	{
		get
		{
			return _data.Statistics.MissionStatistics;
		}
	}

	public bool IsDonator()
	{
		return _data.Statistics.HCPurchases.Count > 0 || _data.Statistics.SCPurchases.Count > 0;
	}

	public void TryLogAfterTutorialEvent(GameAnalytics.AfterTutorialEvent afterTutorialEvent)
	{
		bool flag = GameAnalytics.IsRepeatableAfterTutorialEvent(afterTutorialEvent);
		bool flag2 = _data.AfterTutorialEvents.Exists((GameAnalytics.AfterTutorialEvent x) => x == afterTutorialEvent);
		if (flag || flag2)
		{
			if (flag2)
			{
				int index = _data.AfterTutorialEvents.FindIndex((GameAnalytics.AfterTutorialEvent x) => x == afterTutorialEvent);
				_data.AfterTutorialEvents.RemoveAt(index);
			}
			GameAnalytics.LogAfterTutorial(afterTutorialEvent);
			UnityEngine.Debug.Log(string.Concat("ATE Logged: ", afterTutorialEvent, ", isRepeatable: ", flag, ", isScheduled: ", flag2));
		}
		else
		{
			UnityEngine.Debug.Log("ATE Declined: " + afterTutorialEvent);
		}
	}

	public static SessionStatistics Session
	{
		get
		{
			return _session;
		}
	}
}
