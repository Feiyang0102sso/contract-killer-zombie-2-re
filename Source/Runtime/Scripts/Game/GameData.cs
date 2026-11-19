using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameData : MonoBehaviour
{
	public enum MissionLocation
	{
		Any,
		RedLab,
		GreenLab,
		BlueLab,
		YellowArea,
		OrangeArea,
		TealArea,
		AmberArea,
		GoldenArea
	}

	public enum SceneType
	{
		Outdoor,
		Indoor,
		All
	}

	[Serializable]
	public class Act
	{
		public int Story;

		public int MaxMissions = 1;

		public int Required;

		public PoolData[] PoolData;
	}

	[Serializable]
	public class PoolData
	{
		public Mission.Type Type = Mission.Type.KillAll;

		public string Difficulty = "1";
	}

	[Serializable]
	public class MissionData
	{
		public int MinLevel = 1;

		public int MaxLevel = 1;

		public string Descriptor = string.Empty;

		public string Scene = string.Empty;

		public Mission.Type Type = Mission.Type.KillAll;

		public string Difficulty = "1";

		public MissionLocation Location;
	}

	[Serializable]
	public class ItemRecommendationRule
	{
		public string ShopItem = string.Empty;

		public int MinLevel = 1;

		public int MaxLevel = 1;

		public SceneType SceneType = SceneType.All;

		public string MissionTypes = string.Empty;
	}

	[Serializable]
	public class StorylineData
	{
		public int PlayerXP = 1;

		public string Descriptor = string.Empty;

		public string Scene = string.Empty;

		public int RewardHC;

		public MissionLocation Location;

		public string Voiceover = string.Empty;
	}

	[Serializable]
	public class MissionDescription
	{
		public string Descriptor = string.Empty;

		[Localize]
		public string Title = string.Empty;

		[Localize]
		public string Description = string.Empty;
	}

	[Serializable]
	public class CumulativeGoalData
	{
		[Localize]
		public string Title = string.Empty;

		public int Target = 1;

		public int Reward = 1;

		public CumulativeGoal Generate()
		{
			CumulativeGoal cumulativeGoal = new CumulativeGoal();
			cumulativeGoal.Target = Target;
			cumulativeGoal.Reward = Reward;
			return cumulativeGoal;
		}
	}

	[Serializable]
	public class GoalData
	{
		public Goal.Type Type;

		public int Target = 1;

		public int Reward = 1;

		public Goal Generate(int ticket)
		{
			Goal goal = new Goal();
			goal.Ticket = ticket;
			goal.GoalType = Type;
			goal.Target = Target;
			goal.Reward = Reward;
			goal.Progress = 0;
			return goal;
		}
	}

	[Serializable]
	public class PlayerLevelData
	{
		public int Level = 1;

		public int NextLevelXP = 1;

		public int GetItemsXP;

		public int GetItemsSC;

		public int KillAllXP;

		public int KillAllSC;

		public int RescueXP;

		public int RescueSC;

		public int EscortXP;

		public int EscortSC;

		public int StorylineXP;

		public int StorylineSC;

		public int LevelUpSC;

		public int UnlockedLocations;

		public int InfectionLevelPerMission;
	}

	[Serializable]
	public class WaveData
	{
		public int SmallCommon = 5;

		public int MediumCommon = 10;

		public float UncommonChance = 30f;

		public int Tier2Common = 10;

		public int Tier2Uncommon = 10;

		public int Tier2Special = 10;

		public int Statics = 20;

		public int Specials;
	}

	[Serializable]
	public class AchievementData
	{
		public int Id;

		public string GameCenterId = string.Empty;

		[Localize]
		public string Name = string.Empty;

		[Localize]
		public string Description = string.Empty;

		[Localize]
		public string EarnedDescription = string.Empty;

		public int Points = 1;

		public int Count = 1;

		public string Icon = string.Empty;
	}

	[Serializable]
	public class DailyBonusData
	{
		public string Item = string.Empty;

		public string AlternativeItem = string.Empty;

		public int SoftCurrency;

		public int HardCurrency;
	}

	[Serializable]
	public class CustomWaveData
	{
		public string Bank = string.Empty;

		public int Id;

		public float Delay;

		public int ZombieCount;

		public float Tempo = 1f;

		public int VisiblePercent;

		public float UncommonChance;

		public int Tier2Common = 10;

		public int Tier2Uncommon = 10;

		public int Tier2Special = 10;

		public float MinDistance;

		public float MaxDistance;

		public string Special = string.Empty;

		public string Survivor = string.Empty;
	}

	[Serializable]
	public class ArenaWavesData
	{
		public int Wave;

		public string Variation;

		public float Delay;

		public int CommonAmount;

		public int CommonTier;

		public string Uncommon;

		public int UncommonTier;

		public string Special;

		public int SpecialTier;

		public float Tempo;

		public float BonusChance;

		public string BonusType;
	}

	[Serializable]
	public class ArenaGoalsData
	{
		public string Id;

		public string Order;

		[Localize]
		public string Name;

		public string Save;

		public int a;

		public int b;

		public int c;

		public int d;

		public int Score;

		public int Xp;

		public int Soft;

		public int Hard;

		public float RewardB;

		public float RewardC;

		public float RewardD;
	}

	[Serializable]
	public class SceneData
	{
		public string Scene;

		public int IsOutdoor;

		public string ImagePath;

		public int UnlockLevel;

		[Localize]
		public string Title;
	}

	[Serializable]
	public class CharData
	{
		public string prefab;

		public int tier;

		public string component;

		public string field;

		public string value;
	}

	[Serializable]
	public class AudioSourcePreset
	{
		public string Sound = string.Empty;

		public int Priority;

		public float DopplerLevel = 1f;

		public AudioRolloffMode Rolloff = AudioRolloffMode.Linear;

		public float MinDistance = 5f;

		public float MaxDistance = 25f;

		public float Pitch = 1f;

		public float PanLevel = 1f;

		public void Capture(AudioSource src)
		{
			DopplerLevel = src.dopplerLevel;
			Rolloff = src.rolloffMode;
			MinDistance = src.minDistance;
			MaxDistance = src.maxDistance;
			Pitch = src.pitch;
			PanLevel = src.spatialBlend;
			Priority = src.priority;
		}

		public void Apply(AudioSource dest)
		{
			dest.dopplerLevel = DopplerLevel;
			dest.rolloffMode = Rolloff;
			dest.minDistance = MinDistance;
			dest.maxDistance = MaxDistance;
			dest.pitch = Pitch;
			dest.spatialBlend = PanLevel;
			dest.priority = Priority;
		}
	}

	public enum LoadingScreenType
	{
		Any,
		BeforeLevel,
		AfterLevel
	}

	[Serializable]
	public class LoadingScreenDescriptor
	{
		public LoadingScreenType Type;

		public bool IsMissionPredefined;

		public Mission.Type PredefinedMission;

		public string PicturePath;

		public bool ShowAtLevelUpPopup;

		public int UnlockLevel;

		[Localize]
		public string WhiteHeader;

		[Localize]
		public string WhiteText;

		[Localize]
		public string RedHeader;

		[Localize]
		public string RedText;

		public bool IsSpecial;
	}

	[Serializable]
	public class RewardInfo
	{
		public int XPPerZombie;

		public int XPPerSpecial;

		public int XPPerHeadshot;

		public int XPPerDismemberment;

		public int SCPerZombie;

		public int SCPerSpecial;

		public int SCPerHeadshot;

		public int SCPerDismemberment;
	}

	[Serializable]
	public class InfectionLevelInfo
	{
		public bool Enabled = true;

		public float Max = 8f;

		public float DecreasePerMin = 1f;

		public bool ZombieBombEnabled = true;

		public bool CanPlayWithoutBomb;

		public bool InfectionAffectsDifficulty = true;
	}

	[Serializable]
	public class StringAndFloat
	{
		public string s;

		public float f;
	}

	[Serializable]
	public class FloatAndFloat
	{
		public float a;

		public float b;
	}

	[Serializable]
	public class CollectBonus
	{
		[Serializable]
		public class BonusInfo
		{
			public StringAndFloat[] chances;

			public float[] chancesToRepeat;

			public int minLevelForXpBonus;

			public FloatAndFloat[] distribXp;

			public FloatAndFloat[] distribSoft;

			public StringAndFloat[] distribConsumables;
		}

		public StringAndFloat[] firstConsumables;

		public int hardBonus;

		public BonusInfo[] bonusInfos;
	}

	[Serializable]
	public class ArenaConf
	{
		public int scoreCommon = 10;

		public int scoreUncommon = 30;

		public int scoreSpecial = 100;

		public int overkillHead = 60;

		public int overkillDismember = 60;

		public int overkillHands = 50;

		public float overkillSecondMod = 2f;

		public int softCommon = 1;

		public int softUncommon = 3;

		public int softSpecial = 10;

		public int softHead = 5;

		public int softDismember = 5;
	}

	private class ItemRecommendationRuleChecker
	{
		private ItemRecommendationRule _rule;

		private Mission.Type[] _types;

		public bool Check(MissionData mission)
		{
			if (Singleton.PlayerData.Level > _rule.MaxLevel || Singleton.PlayerData.Level < _rule.MinLevel)
			{
				return false;
			}
			if (_types != null && !Array.Exists(_types, (Mission.Type t) => t == mission.Type))
			{
				return false;
			}
			bool flag = Singleton.GameData.GetSceneData(mission.Scene).IsOutdoor != 0;
			if ((_rule.SceneType == SceneType.Indoor && flag) || (_rule.SceneType == SceneType.Outdoor && !flag))
			{
				return false;
			}
			if (ShopItem is WeaponShopItem && (ShopItem as WeaponShopItem).IsAvailableForEquipment())
			{
				return false;
			}
			return true;
		}

		public ItemRecommendationRuleChecker(ItemRecommendationRule rule)
		{
			_rule = rule;
			if (_rule.MissionTypes.Length > 0)
			{
				string[] array = rule.MissionTypes.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				_types = new Mission.Type[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					_types[i] = (Mission.Type)(int)Enum.Parse(typeof(Mission.Type), array[i], true);
				}
			}
		}

		public ShopConfigItem ShopItem
		{
			get
			{
				return Singleton.ShopData.GetShopItem(_rule.ShopItem);
			}
		}
	}

	public Act[] Acts;

	public MissionData[] Missions;

	public StorylineData[] Storyline;

	public MissionDescription[] MissionDescriptions;

	public ItemRecommendationRule[] ItemRecommendationRules;

	public CumulativeGoalData[] CumulativeGoals;

	public GoalData[] Goals;

	public WaveData[] Waves;

	public WaveData[] WavesHard;

	public WaveData[] WavesStoryline;

	public PlayerLevelData[] PlayerLevels;

	public AchievementData[] Achievements;

	public DailyBonusData[] DailyBonus;

	public CustomWaveData[] CustomWaves;

	public ArenaWavesData[] ArenaWaves;

	public ArenaGoalsData[] ArenaGoals;

	public SceneData[] Scenes;

	public CharData[] CharConf;

	public AudioSourcePreset[] AudioSourcePresets;

	public LoadingScreenDescriptor[] LoadingScreenDescriptors;

	public int[] RateMeLevels;

	public Mission.Type Ckz1BannerShowAfterLevel = Mission.Type.Story2;

	public RewardInfo RewardUsual;

	public RewardInfo RewardSurvivalGates;

	public bool MissionPoolAffectsDifficulty = true;

	public bool DefibrillatorOnFailEnabled = true;

	public string StartConsumable = "Defibrillator";

	public int StartConsumableCount = 1;

	public string StartHandgunWeapon = "Uzi1";

	public string StartMeleeWeapon = "BaseballBat1";

	public float DamageModifierFromSpecialToSurvivor = 0.5f;

	public float DamageModifierFromSpecialToGates = 0.5f;

	public int FacebookRewardHC = 10;

	public string TutorialDemoWeaponShopId = "AssaultRifle4";

	public InfectionLevelInfo InfectionLevel;

	public CollectBonus collectBonus = new CollectBonus
	{
		firstConsumables = new StringAndFloat[2]
		{
			new StringAndFloat
			{
				s = "MedkitLarge",
				f = 2f
			},
			new StringAndFloat
			{
				s = "GrenadeBeacon",
				f = 2f
			}
		},
		hardBonus = 1,
		bonusInfos = new CollectBonus.BonusInfo[2]
		{
			new CollectBonus.BonusInfo
			{
				chances = new StringAndFloat[3]
				{
					new StringAndFloat
					{
						s = "xp",
						f = 45f
					},
					new StringAndFloat
					{
						s = "soft",
						f = 45f
					},
					new StringAndFloat
					{
						s = "consumable",
						f = 10f
					}
				},
				chancesToRepeat = new float[3] { 100f, 15f, 5f },
				minLevelForXpBonus = 4,
				distribXp = new FloatAndFloat[3]
				{
					new FloatAndFloat
					{
						a = 25f,
						b = 10f
					},
					new FloatAndFloat
					{
						a = 10f,
						b = 20f
					},
					new FloatAndFloat
					{
						a = 5f,
						b = 70f
					}
				},
				distribSoft = new FloatAndFloat[3]
				{
					new FloatAndFloat
					{
						a = 25f,
						b = 10f
					},
					new FloatAndFloat
					{
						a = 10f,
						b = 20f
					},
					new FloatAndFloat
					{
						a = 5f,
						b = 70f
					}
				},
				distribConsumables = new StringAndFloat[5]
				{
					new StringAndFloat
					{
						s = "MedkitLarge",
						f = 25f
					},
					new StringAndFloat
					{
						s = "GrenadeBeacon",
						f = 12.5f
					},
					new StringAndFloat
					{
						s = "GrenadeExplosive",
						f = 12.5f
					},
					new StringAndFloat
					{
						s = "DamageBoost",
						f = 25f
					},
					new StringAndFloat
					{
						s = "Defibrillator",
						f = 25f
					}
				}
			},
			new CollectBonus.BonusInfo()
		}
	};

	public float difficultyHardRewardXpMod = 2f;

	public float difficultyHardRewardScMod = 2f;

	public float difficultyHardCollectXpMod = 2f;

	public float difficultyHardCollectScMod = 2f;

	public ArenaConf arenaConf;

	private int _gameDataHashCode;

	private Dictionary<string, SortedList<int, List<CustomWaveData>>> _wavesMap = new Dictionary<string, SortedList<int, List<CustomWaveData>>>();

	[NonSerialized]
	public Dictionary<int, Dictionary<string, List<ArenaWavesData>>> arenaWaves = new Dictionary<int, Dictionary<string, List<ArenaWavesData>>>();

	[NonSerialized]
	public Dictionary<KeyValuePair<string, int>, Dictionary<string, Dictionary<string, string>>> charConfMap = new Dictionary<KeyValuePair<string, int>, Dictionary<string, Dictionary<string, string>>>();

	[NonSerialized]
	public Dictionary<string, ArenaGoalsData> arenaGoals = new Dictionary<string, ArenaGoalsData>();

	private ItemRecommendationRuleChecker[] _recommendationRules;

	private Dictionary<string, int> _audioSourcePresetMap = new Dictionary<string, int>();

	private void Awake()
	{
		_gameDataHashCode = GetGameDataHashCode();
		CreateWavesMap();
		CreateCharConfMap();
		PrepareRecommendationRules();
		BuildAudioSourcePresetMap();
	}

	private void CreateWavesMap()
	{
		foreach (CustomWaveData customWaveData in CustomWaves)
		{
			SortedList<int, List<CustomWaveData>> value;
			_wavesMap.TryGetValue(customWaveData.Bank, out value);
			if (value == null)
			{
				value = new SortedList<int, List<CustomWaveData>>();
				_wavesMap.Add(customWaveData.Bank, value);
			}
			List<CustomWaveData> value2;
			value.TryGetValue(customWaveData.Id, out value2);
			if (value2 == null)
			{
				value2 = new List<CustomWaveData>();
				value.Add(customWaveData.Id, value2);
			}
			value2.Add(customWaveData);
		}
		arenaWaves = ArenaWaves.GroupBy(w => w.Wave).ToDictionary(g => g.Key,
			g => g.GroupBy(v => v.Variation).ToDictionary(gg => gg.Key, gg => gg.ToList()));
		arenaGoals = ArenaGoals.ToDictionary((ArenaGoalsData g) => g.Id, (ArenaGoalsData g) => g);
	}

	public List<CustomWaveData> GetCustomWaves(string bankName, int waveId)
	{
		SortedList<int, List<CustomWaveData>> value;
		_wavesMap.TryGetValue(bankName, out value);
		if (value != null)
		{
			List<CustomWaveData> value2;
			value.TryGetValue(waveId, out value2);
			if (value2 != null)
			{
				return value2;
			}
		}
		return new List<CustomWaveData>();
	}

	private void CreateCharConfMap()
	{
		foreach (CharData charData in CharConf)
		{
			KeyValuePair<string, int> key = new KeyValuePair<string, int>(charData.prefab, charData.tier);
			Dictionary<string, Dictionary<string, string>> value;
			if (!charConfMap.TryGetValue(key, out value))
			{
				charConfMap.Add(key, value = new Dictionary<string, Dictionary<string, string>>());
			}
			Dictionary<string, string> value2;
			if (!value.TryGetValue(charData.component, out value2))
			{
				value.Add(charData.component, value2 = new Dictionary<string, string>());
			}
			if (!value2.ContainsKey(charData.field))
			{
				value2.Add(charData.field, charData.value);
			}
			else
			{
			}
		}
	}

	private void PrepareRecommendationRules()
	{
		_recommendationRules = new ItemRecommendationRuleChecker[ItemRecommendationRules.Length];
		for (int i = 0; i < ItemRecommendationRules.Length; i++)
		{
			_recommendationRules[i] = new ItemRecommendationRuleChecker(ItemRecommendationRules[i]);
		}
	}

	public ShopConfigItem GetRecommendedItem(MissionData mission, int rank)
	{
		HashSet<ShopConfigItem> hashSet = new HashSet<ShopConfigItem>();
		for (int i = 0; i < _recommendationRules.Length; i++)
		{
			if (_recommendationRules[i].Check(mission))
			{
				ShopConfigItem shopItem = _recommendationRules[i].ShopItem;
				if (hashSet.Add(shopItem))
				{
					rank--;
				}
				if (rank == 0)
				{
					return shopItem;
				}
			}
		}
		return null;
	}

	private int HashCode(string s)
	{
		int num = 0;
		for (int i = 0; i < s.Length; i++)
		{
			num += (s[i] * 31) ^ (s.Length - (i + 1));
		}
		return num;
	}

	private int HashCode(StringBuilder s)
	{
		int num = 0;
		for (int i = 0; i < s.Length; i++)
		{
			num += (s[i] * 31) ^ (s.Length - (i + 1));
		}
		return num;
	}

	private int GetGameDataHashCode()
	{
		StringBuilder stringBuilder = new StringBuilder(string.Empty);
		foreach (Act act in Acts)
		{
			stringBuilder.Append(HashCode(act.Story.ToString()));
			stringBuilder.Append(HashCode(act.Required.ToString()));
			stringBuilder.Append(HashCode(act.MaxMissions.ToString()));
			stringBuilder.Append(HashCode(stringBuilder));
		}
		foreach (MissionData missionData in Missions)
		{
			stringBuilder.Append(HashCode(missionData.MinLevel.ToString()));
			stringBuilder.Append(HashCode(missionData.MaxLevel.ToString()));
			stringBuilder.Append(HashCode(missionData.Scene));
			stringBuilder.Append(HashCode(missionData.Descriptor));
			stringBuilder.Append(HashCode(missionData.Difficulty));
			stringBuilder.Append(HashCode(missionData.Type.ToString()));
			stringBuilder.Append(HashCode(stringBuilder));
		}
		foreach (StorylineData storylineData in Storyline)
		{
			stringBuilder.Append(HashCode(storylineData.PlayerXP.ToString()));
			stringBuilder.Append(HashCode(storylineData.Descriptor));
			stringBuilder.Append(HashCode(storylineData.Scene));
			stringBuilder.Append(HashCode(stringBuilder));
		}
		foreach (MissionDescription missionDescription in MissionDescriptions)
		{
			stringBuilder.Append(HashCode(missionDescription.Descriptor));
			stringBuilder.Append(HashCode(missionDescription.Title));
			stringBuilder.Append(HashCode(missionDescription.Description));
			stringBuilder.Append(HashCode(stringBuilder));
		}
		stringBuilder.Append(HashCode(MissionPoolAffectsDifficulty.ToString()));
		stringBuilder.Append(HashCode(stringBuilder));
		return HashCode(stringBuilder);
	}

	public int GameDataHashCode
	{
		get
		{
			return _gameDataHashCode;
		}
	}

	public SceneData GetSceneData(string sceneName)
	{
		return Array.Find(Scenes, (SceneData loc) => loc.Scene == sceneName);
	}

	public MissionData GetMissionData(string missionName)
	{
		return Array.Find(Missions, (MissionData m) => m.Descriptor == missionName);
	}

	public int MaxPlayerLevel
	{
		get
		{
			return PlayerLevels.Length - 1;
		}
	}

	public int MaxPlayerXP
	{
		get
		{
			int num = 0;
			for (int i = 0; i < PlayerLevels.Length; i++)
			{
				num += PlayerLevels[i].NextLevelXP;
			}
			return num;
		}
	}

	public int GetRewardXP(int playerLevel, Mission.Type missionType, Mission.Difficulty difficulty, bool isSpecialHunt)
	{
		int num;
		switch (missionType)
		{
		case Mission.Type.GetItems:
			num = PlayerLevels[playerLevel].GetItemsXP;
			break;
		case Mission.Type.KillAll:
			num = PlayerLevels[playerLevel].KillAllXP;
			break;
		case Mission.Type.Rescue:
			num = PlayerLevels[playerLevel].RescueXP;
			break;
		case Mission.Type.Escort:
			num = PlayerLevels[playerLevel].EscortXP;
			break;
		case Mission.Type.SurvivalGates:
			num = 0;
			break;
		case Mission.Type.SurvivalArena:
			num = PlayerLevels[playerLevel].StorylineXP;
			break;
		default:
			num = PlayerLevels[playerLevel].StorylineXP;
			break;
		}
		if (difficulty == Mission.Difficulty.Hard)
		{
			num = Mathf.CeilToInt((float)num * difficultyHardRewardXpMod);
		}
		return isSpecialHunt ? 90 : num;
	}

	public int GetRewardSC(int playerLevel, Mission.Type missionType, Mission.Difficulty difficulty, bool isSpecialHunt)
	{
		int num;
		switch (missionType)
		{
		case Mission.Type.GetItems:
			num = PlayerLevels[playerLevel].GetItemsSC;
			break;
		case Mission.Type.KillAll:
			num = PlayerLevels[playerLevel].KillAllSC;
			break;
		case Mission.Type.Rescue:
			num = PlayerLevels[playerLevel].RescueSC;
			break;
		case Mission.Type.Escort:
			num = PlayerLevels[playerLevel].EscortSC;
			break;
		case Mission.Type.SurvivalGates:
			num = 0;
			break;
		case Mission.Type.SurvivalArena:
			num = PlayerLevels[playerLevel].StorylineSC;
			break;
		default:
			num = PlayerLevels[playerLevel].StorylineSC;
			break;
		}
		if (difficulty == Mission.Difficulty.Hard)
		{
			num = Mathf.CeilToInt((float)num * difficultyHardRewardScMod);
		}
		return isSpecialHunt ? num : num;
	}

	public PlayerLevelData GetPlayerLevelData(int playerLevel)
	{
		return PlayerLevels[playerLevel];
	}

	public PlayerLevelData GetCurrentPlayerLevelData()
	{
		return GetPlayerLevelData(Singleton.PlayerData.Level);
	}

	public WaveData GetCurrentPlayerLevelWaveData()
	{
		if (Scene.Mission.IsStorylineMission())
		{
			return WavesStoryline[-1 + Scene.Mission.GetStorylineNumber()];
		}
		return (!(!Scene.Mission.IsHard()) ? WavesHard : Waves)[Mathf.Clamp(-1 + Mission.GetEffectivePlayersLevel(), 0, Waves.Length - 1)];
	}

	public int GetStorylineLength()
	{
		return !(!QualitySetter.FinalMissionEnabled) ? Storyline.Length : (Storyline.Length - 1);
	}

	public string GetMissionVoiceover(int missionIndex)
	{
		if (IsStorylineMission(missionIndex))
		{
			return GetMissionStoryline(missionIndex).Voiceover;
		}
		return string.Empty;
	}

	public StorylineData GetMissionStoryline(Mission.Type missionType)
	{
		int num = (int)(missionType - 5);
		return Storyline[num];
	}

	public StorylineData GetMissionStoryline(int missionIndex)
	{
		if (IsStorylineMission(missionIndex))
		{
			int num = (int)(Missions[missionIndex].Type - 5);
			return Storyline[num];
		}
		return null;
	}

	public SceneData GetMissionSceneDescription(int missionIndex)
	{
		return GetSceneData(Missions[missionIndex].Scene);
	}

	public MissionDescription GetMissionDescription(string descriptor, int missionIndex)
	{
		for (int i = 0; i < MissionDescriptions.Length; i++)
		{
			if (descriptor == MissionDescriptions[i].Descriptor)
			{
				return MissionDescriptions[i];
			}
		}
		return null;
	}

	public static bool IsStorylineMission(Mission.Type type)
	{
		return type >= Mission.Type.Story1 && type <= Mission.Type.Story25;
	}

	public bool IsStorylineMission(int missionIndex)
	{
		return IsStorylineMission(Missions[missionIndex].Type);
	}

	public string GenerateMissionDescriptor(int missionIndex, int generationSeed)
	{
		MissionData missionData = Missions[missionIndex];
		List<string> list = new List<string>();
		for (int i = 0; i < Singleton.GameData.MissionDescriptions.Length; i++)
		{
			if (Singleton.GameData.MissionDescriptions[i].Descriptor.Contains(missionData.Descriptor))
			{
				list.Add(Singleton.GameData.MissionDescriptions[i].Descriptor);
			}
		}
		if (list.Count == 0)
		{
			Debug.Log("Invalid mission descriptor: " + missionData.Descriptor);
		}
		if (list.Count > 0)
		{
			return list[generationSeed % list.Count];
		}
		return "INVALID_DESCRIPTOR";
	}

	public Mission.Difficulty GenerateMissionDifficulty(int missionIndex, int generationSeed)
	{
		MissionData missionData = Missions[missionIndex];
		List<Mission.Difficulty> list = new List<Mission.Difficulty>();
		if (missionData.Difficulty.Contains("1"))
		{
			list.Add(Mission.Difficulty.Normal);
		}
		if (missionData.Difficulty.Contains("2") || missionData.Difficulty.Contains("3"))
		{
			list.Add(Mission.Difficulty.Hard);
		}
		if (list.Count == 0)
		{
			Debug.Log("Invalid mission difficulty: " + missionData.Difficulty);
		}
		if (list.Count > 0)
		{
			return list[generationSeed % list.Count];
		}
		return Mission.Difficulty.Normal;
	}

	public void Check()
	{
	}

	public int GetLevel(int xp)
	{
		int maxPlayerLevel = MaxPlayerLevel;
		int num = 0;
		for (int i = 0; i <= maxPlayerLevel; i++)
		{
			PlayerLevelData playerLevelData = PlayerLevels[i];
			if (xp >= num)
			{
				num += playerLevelData.NextLevelXP;
				continue;
			}
			return i - 1;
		}
		return maxPlayerLevel;
	}

	private void CheckData()
	{
		for (int i = 0; i < PlayerLevels.Length; i++)
		{
		}
		for (int j = 0; j < Missions.Length; j++)
		{
		}
		CheckRecommendedWeapons();
	}

	private void CheckRecommendedWeapons()
	{
		foreach (ItemRecommendationRule itemRecommendationRule in ItemRecommendationRules)
		{
		}
	}

	private void BuildAudioSourcePresetMap()
	{
		if (AudioSourcePresets != null)
		{
			for (int i = 0; i < AudioSourcePresets.Length; i++)
			{
				_audioSourcePresetMap.Add(AudioSourcePresets[i].Sound, i);
			}
		}
	}

	public AudioSourcePreset GetAudioSourcePreset(string sound)
	{
		int value = -1;
		if (_audioSourcePresetMap.TryGetValue(sound, out value))
		{
			return AudioSourcePresets[value];
		}
		Debug.Log("NO AUDIO SOURCE PRESET FOR " + sound);
		return null;
	}
}
