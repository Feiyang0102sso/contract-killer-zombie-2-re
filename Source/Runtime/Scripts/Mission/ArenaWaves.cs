using System;
using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

public class ArenaWaves : SpawnScenario
{
	private int currentWave = -1;

	private string currentVariation;

	private bool subComplete;

	private HashSet<string> usedWeaponsInWave = new HashSet<string>();

	private List<GameObject> bonusBoxes;

	private GameData.ArenaWavesData bonusInfo;

	private Dictionary<string, GameData.ArenaGoalsData> allGoals;

	private Dictionary<string, PlayerData.ArenaGoalInfo> currentGoals;

	private GameData.ArenaConf conf;

	private Dictionary<CountdownTimer, int> killTimers = new Dictionary<CountdownTimer, int>();

	private int OverkillHeadshots;

	private int OverkillDismembers;

	private int OverkillHands;

	private bool killForHeadshot;

	private bool killForDismember;

	private List<Survivor> mercenaries;

	private bool mercWasHired;

	private bool askForMerc = true;

	private int revives;

	[NonSerialized]
	public int scoreTotal;

	[NonSerialized]
	public int xpTotal;

	[NonSerialized]
	public int softTotal;

	[NonSerialized]
	public int hardTotal;

	private Queue<string> notifies = new Queue<string>();

	private Queue<string> notifiesLong = new Queue<string>();

	private CountdownTimer notifiesTimer = new CountdownTimer();

	public void OnRevived()
	{
		revives++;
	}

	protected override void Awake()
	{
		base.Awake();
		LogTo("Mission.Arena");
	}

	protected override void Start()
	{
		base.Start();
		if (Singleton.PlayerData.startFrom != string.Empty)
		{
			currentWave = int.Parse(Singleton.PlayerData.startFrom);
			Singleton.PlayerData.startFrom = string.Empty;
		}
		allGoals = Singleton.GameData.arenaGoals;
		currentGoals = Singleton.PlayerData.ArenaGoals;
		conf = Singleton.GameData.arenaConf;
		RefreshGoals();
		Scene.Player.OnHurtExternal += new Action<DamageInfo>(OnHurtPlayer);
		Singleton.GameEvents.OnWeaponShot += new Action<Character, Weapon, int>(OnWeaponShot);
		Singleton.GameEvents.OnEnemyHit += new Action<Character, Unit, Weapon, Consumable, float>(OnEnemyHit);
		Singleton.GameEvents.OnEnemyKill += new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
		Singleton.GameEvents.OnEnemyHeadshot += new Action<Character, Unit, Weapon, Consumable>(OnEnemyHeadshot);
		Singleton.GameEvents.OnEnemyDismemberment += new Action<Character, Unit, Weapon, Consumable, bool>(OnEnemyDismemberment);
		Singleton.GameEvents.OnEnemyMutilated += new Action<Character, Unit, Weapon, Consumable>(OnEnemyMutilated);
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => "goals: " + string.Join(", ", currentGoals.Select((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => string.Format("{0}~{1} ({2}/{3})", p.Key, p.Value.type, p.Value.counter, PlayerData.ArenaGoalInfo.LimitForType(p.Value.type, allGoals[p.Key]))).ToArray()));
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("overkill head: {0}, dismember: {1}, hand: {2}", OverkillHeadshots, OverkillDismembers, OverkillHands));
	}

	public bool IsBonusRewardHard()
	{
		return bonusInfo != null ? bonusInfo.BonusType == "hard" : false;
	}

	public string GetText()
	{
		return Strings.GetString("Wave") + " " + currentWave;
	}

	public void OnActivate()
	{
		bonusBoxes = Scene.Mission.FindCustomChildren((GameObject o) => o.name == "BonusBox_");
		bonusBoxes.ForEach((GameObject b) =>
		{
			b.SetActiveRecursively(false);
		});
		mercenaries = Util.FindObjectsOfType<Survivor>().ToList();
		mercenaries.ForEach((Survivor s) =>
		{
			s.gameObject.SetActiveRecursively(false);
		});
		Generate();
	}

	private void SuggestMerc()
	{
		ConsumableShopItem mercItem = Singleton.ShopData.GetConsumableShopItemByItem("Mercenary");
		MercenaryHiringPopup popup = Util.CreateGameObject("MercenaryHiringPopup").GetComponent<MercenaryHiringPopup>();
		popup.YesButtonLabel.Text = string.Format("{0} (^{1})", Strings.GetString("Yes"), mercItem.Price);
		GUIUtils.HookButton(popup.YesButton, () =>
		{
			mercWasHired = true;
			mercItem.Purchase();
		});
		Action purchased = null;
		purchased = () =>
		{
			popup.Close();
			mercItem.Use();
			mercenaries.ForEach((Survivor s) =>
			{
				((GameObject)UnityEngine.Object.Instantiate(s.gameObject, s.transform.position, s.transform.rotation)).RemoveClonePostfix().SetActiveRecursively(true);
			});
			mercItem.OnActualPurchase -= purchased;
		};
		mercItem.OnActualPurchase += purchased;
		GUIUtils.HookButton(popup.NoButton, () =>
		{
			popup.Close();
			askForMerc = false;
			mercItem.OnActualPurchase -= purchased;
		});
		Singleton.UI.PopupManager.AddPopup(popup);
	}

	protected override void OnDestroy()
	{
		if (Scene.Exists() && (bool)Scene.Player)
		{
			Scene.Player.OnHurtExternal -= new Action<DamageInfo>(OnHurtPlayer);
		}
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnWeaponShot -= new Action<Character, Weapon, int>(OnWeaponShot);
			Singleton.GameEvents.OnEnemyHit -= new Action<Character, Unit, Weapon, Consumable, float>(OnEnemyHit);
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
			Singleton.GameEvents.OnEnemyHeadshot -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyHeadshot);
			Singleton.GameEvents.OnEnemyDismemberment -= new Action<Character, Unit, Weapon, Consumable, bool>(OnEnemyDismemberment);
			Singleton.GameEvents.OnEnemyMutilated -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyMutilated);
		}
		if (Singleton.Exists())
		{
			new Dictionary<string, PlayerData.ArenaGoalInfo>(currentGoals).Where((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => allGoals[p.Key].Save != "+").ToList().ForEach((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) =>
			{
				currentGoals[p.Key].counter = 0;
			});
		}
		base.OnDestroy();
	}

	private void NotifyOverkill(string text, int score)
	{
		Notify(false, text, score, 0, 0, 0);
	}

	private void NotifyWave(string text)
	{
		Notify(true, text, 0, 0, 0, 0);
	}

	private void NotifyGoal(string text, int score, int xp, int soft, int hard)
	{
		Notify(true, text, score, xp, soft, hard);
	}

	private void Notify(bool showLong, string text, int score, int soft, int xp, int hard)
	{
		string text2 = string.Empty;
		if (score != 0)
		{
			text2 += string.Format("{0}{1} +{2}", text2 != string.Empty ? ", " : string.Empty, Strings.GetString("Score"), score);
		}
		if (soft != 0)
		{
			text2 += string.Format("{0}{1} +{2}", text2 != string.Empty ? ", " : string.Empty, "@", soft);
		}
		if (xp != 0)
		{
			text2 += string.Format("{0}{1} +{2}", text2 != string.Empty ? ", " : string.Empty, Strings.GetString("XP"), xp);
		}
		if (hard != 0)
		{
			text2 += string.Format("{0}{1} +{2}", text2 != string.Empty ? ", " : string.Empty, "^", hard);
		}
		if (text != string.Empty && text2 != string.Empty)
		{
			text2 = "\n" + text2;
		}
		if (showLong)
		{
			notifiesLong.Enqueue(text + text2);
		}
		else
		{
			notifies.Enqueue(text + text2);
		}
		scoreTotal += Mathf.FloorToInt((float)score * Scene.Mission.GetComponent<ComboCounter>().Ratio());
		xpTotal += xp;
		softTotal += soft;
		hardTotal += hard;
	}

	private void Score(int score)
	{
		scoreTotal += Mathf.FloorToInt((float)score * Scene.Mission.GetComponent<ComboCounter>().Ratio());
	}

	private void ScoreSoft(int soft)
	{
		softTotal += soft;
	}

	protected override void Generate()
	{
		Dictionary<int, Dictionary<string, List<GameData.ArenaWavesData>>> arenaWaves = Singleton.GameData.arenaWaves;
		bool flag = false;
		if (arenaWaves.Count((KeyValuePair<int, Dictionary<string, List<GameData.ArenaWavesData>>> p) => p.Key >= currentWave) == 0)
		{
			flag = true;
			while (!arenaWaves.ContainsKey(currentWave))
			{
				currentWave--;
			}
		}
		while (!arenaWaves.ContainsKey(currentWave))
		{
			currentWave++;
		}
		string key = Util.RandomFrom(arenaWaves[currentWave].Select((KeyValuePair<string, List<GameData.ArenaWavesData>> p) => p.Key));
		subComplete = false;
		if (!flag)
		{
			NotifyWave(string.Format(Strings.GetString("Wave {0} incoming!"), currentWave));
		}
		killTimers.Add(new CountdownTimer(120f), 0);
		UnityEngine.Object.Destroy(Util.Find("/BonusBoxCurrent"));
		float unusedBonusValue;
		List<GameData.ArenaWavesData> list = (from w in arenaWaves[currentWave].SelectMany((KeyValuePair<string, List<GameData.ArenaWavesData>> p) => p.Value)
			where w.BonusChance != 0f
			select w).ToList();
		bonusInfo = list.Count() > 0 ? list[0] : null;
		if (bonusInfo != null && Util.Chance(bonusInfo.BonusChance))
		{
			GameObject gameObject = Util.RandomFrom((IEnumerable<GameObject>)bonusBoxes);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
			gameObject2.name = "BonusBoxCurrent";
			gameObject2.SetParent(null);
			gameObject2.SetActiveRecursively(true);
		}
		else
		{
			bonusInfo = null;
		}
		if (Scene.Survivors.Count == 0 && askForMerc)
		{
			SuggestMerc();
		}
		foreach (GameData.ArenaWavesData wave in arenaWaves[currentWave][key])
		{
			CustomInterpolators.EASING_TYPE unusedEasing;
			float tempo = wave.Tempo != 0f ? wave.Tempo : 2f;
			AddSpawnSequence(wave.CommonAmount, Scene.Spawner.CommonOrUncommonZombie(0f, (wave.CommonTier > 1) ? 100 : 0, 0f), null, null, Spawner.PointDesc.AnyVisible(), wave.Delay, tempo, CustomInterpolators.EASING_TYPE.Linear);
			{
				List<string> uncommons = Util.MakeChoice(wave.Uncommon);
				int index = 0;
				AddSpawnSequence(uncommons.Count, () => Scene.Spawner.UncommonZombie((Spawner.UncommonType)(int)Enum.Parse(typeof(Spawner.UncommonType), uncommons[index++], true), (wave.UncommonTier > 1) ? 100 : 0)(), null, null, Spawner.PointDesc.AnyInvisible(), wave.Delay, tempo, CustomInterpolators.EASING_TYPE.Linear);
			}
			{
				List<string> specials = Util.MakeChoice(wave.Special);
				int index = 0;
				AddSpawnSequence(specials.Count, () => Scene.Spawner.SpecialZombie((Spawner.SpecialType)(int)Enum.Parse(typeof(Spawner.SpecialType), specials[index++], true), (wave.SpecialTier > 1) ? 100 : 0)(), null, null, Spawner.PointDesc.AnyInvisible(), wave.Delay, tempo, CustomInterpolators.EASING_TYPE.Linear);
			}
		}
		GameAnalytics.LogAMFinishedWave(Singleton.PlayerData.Level, currentWave, scoreTotal, revives, mercWasHired ? 1 : 0);
		base.Generate();
	}

	protected override void Complete()
	{
		subComplete = true;
	}

	protected override void Update()
	{
		int unusedInt1;
		int unusedInt2;
		if (notifies.Count > 0 && !notifiesTimer.IsRunning())
		{
			Singleton.UI.Hud.ShowNotifier(notifies.Dequeue());
			notifiesTimer.Run(1f);
		}
		if (notifiesLong.Count > 0 && !notifiesTimer.IsRunning())
		{
			Singleton.UI.Hud.ShowNotifierLong(notifiesLong.Dequeue());
			notifiesTimer.Run(3f);
		}
		if (Util.DoRarely(this, "goals_recheck", 0.5f))
		{
			new Dictionary<string, PlayerData.ArenaGoalInfo>(currentGoals).Where((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Value.counter >= PlayerData.ArenaGoalInfo.LimitForType(p.Value.type, allGoals[p.Key])).ToList().ForEach((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) =>
			{
				CompleteGoal(p.Key, p.Value.type);
			});
		}
		if (killForHeadshot)
		{
			OverkillHeadshots = 0;
			killForHeadshot = false;
		}
		if (killForDismember)
		{
			OverkillDismembers = 0;
			killForDismember = false;
		}
		if (OverkillHeadshots == 3)
		{
			NotifyOverkill(Strings.GetString("3 HEADS OFF!"), conf.overkillHead);
			OverkillHeadshots++;
		}
		if (OverkillHeadshots == 6)
		{
			NotifyOverkill(Strings.GetString("BEHEADER!"), Mathf.CeilToInt((float)conf.overkillHead * conf.overkillSecondMod));
			OverkillHeadshots = 0;
		}
		if (OverkillDismembers == 3)
		{
			NotifyOverkill(Strings.GetString("TRIPLE MUTILATION!"), conf.overkillDismember);
			OverkillDismembers++;
		}
		if (OverkillDismembers == 6)
		{
			NotifyOverkill(Strings.GetString("SPLATTERHOUSE!"), Mathf.CeilToInt((float)conf.overkillDismember * conf.overkillSecondMod));
			OverkillDismembers = 0;
		}
		if (OverkillHands == 3)
		{
			NotifyOverkill(Strings.GetString("3 HANDS DETACHED!"), conf.overkillHands);
			OverkillHands++;
		}
		if (OverkillHands == 6)
		{
			NotifyOverkill(Strings.GetString("HANDS OFF ME!"), Mathf.CeilToInt((float)conf.overkillHands * conf.overkillSecondMod));
			OverkillHands = 0;
		}
		if (Util.DoRarely(this, "completion_recheck", 0.5f) && subComplete && Scene.AttackingEnemies.Count == 0)
		{
			Increment("survive");
			IEnumerable<IGrouping<PlayerData.Slots, string>> source = usedWeaponsInWave.GroupBy(w => HumanInventory.SlotOfWeapon(w));
			string text = source.Count() == 1 ? source.First().Key.ToString() : string.Empty;
			if (text == "handgun")
			{
				Increment("only_handgun");
			}
			if (text == "shotgun")
			{
				Increment("only_shotgun");
			}
			if (text == "assault")
			{
				Increment("only_assault");
			}
			if (text == "melee")
			{
				Increment("only_melee");
			}
			usedWeaponsInWave.Clear();
			currentWave++;
			Generate();
			return;
		}
		base.Update();
	}

	public static bool IsObsolete(string goal)
	{
		Dictionary<string, PlayerData.ArenaGoalInfo> arenaGoals = Singleton.PlayerData.ArenaGoals;
		return arenaGoals.ContainsKey(goal) && arenaGoals[goal].type == "d" && DateTime.Now >= Singleton.PlayerData.LastArenaDailyGoalReceived + new TimeSpan(1, 0, 0, 0);
	}

	public static Dictionary<string, PlayerData.ArenaGoalInfo> RefreshGoals()
	{
		Dictionary<string, PlayerData.ArenaGoalInfo> arenaGoals = Singleton.PlayerData.ArenaGoals;
		Dictionary<string, GameData.ArenaGoalsData> arenaGoals2 = Singleton.GameData.arenaGoals;
		Dictionary<string, PlayerData.ArenaGoalInfo> dictionary = new Dictionary<string, PlayerData.ArenaGoalInfo>();
		Singleton.PlayerData.LastCompletedArenaGoals.Clear();
		if (DateTime.Now >= Singleton.PlayerData.LastArenaDailyGoalReceived + new TimeSpan(1, 0, 0, 0))
		{
			foreach (string item in (from p in arenaGoals
				where p.Value.type == "d"
				select p.Key).ToList())
			{
				arenaGoals.Remove(item);
			}
			if (arenaGoals.Count((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Value.type == "d") == 0)
			{
				Singleton.PlayerData.LastArenaDailyGoalReceived = DateTime.Now;
				string key = arenaGoals2.Select((KeyValuePair<string, GameData.ArenaGoalsData> p) => p.Key).ToList().Shuffled()
					.First();
				arenaGoals[key] = new PlayerData.ArenaGoalInfo
				{
					type = "d"
				};
				dictionary[key] = arenaGoals[key];
			}
		}
		int unusedLocal1;
		while (arenaGoals.Count((KeyValuePair<string, PlayerData.ArenaGoalInfo> p) => p.Value.type != "d") < 3)
		{
			bool flag = false;
			foreach (KeyValuePair<string, string> item2 in QueryAllGoals())
			{
				string key2 = item2.Key;
				string value = item2.Value;
				if (arenaGoals.ContainsKey(key2))
				{
					continue;
				}
				if (Singleton.PlayerData.CompletedArenaGoals.Contains(key2 + "~" + value))
				{
					continue;
				}
				arenaGoals[key2] = new PlayerData.ArenaGoalInfo
				{
					type = value
				};
				dictionary[key2] = arenaGoals[key2];
				flag = true;
				break;
			}
			if (!flag)
			{
				break;
			}
		}
		return dictionary;
	}

	public static IEnumerable<KeyValuePair<string, string>> QueryAllGoals()
	{
		Dictionary<string, GameData.ArenaGoalsData> allGoals = Singleton.GameData.arenaGoals;
		foreach (string type in new string[3] { "a", "b", "c" })
		{
			foreach (string goal in allGoals.Select((KeyValuePair<string, GameData.ArenaGoalsData> p) => p.Key).ToList().Shuffled()
				.OrderBy((string g) => allGoals[g].Order, new Util.NaturalComparer()))
			{
				yield return new KeyValuePair<string, string>(goal, type);
			}
		}
	}

	private void CompleteGoal(string goal, string type)
	{
		NotifyGoal(string.Format(Strings.GetString("Challenge completed:") + " " + Strings.GetString(allGoals[goal].Name), PlayerData.ArenaGoalInfo.LimitForType(currentGoals[goal].type, allGoals[goal])), allGoals[goal].Score, allGoals[goal].Soft, allGoals[goal].Xp, allGoals[goal].Hard);
		Singleton.PlayerData.CompletedArenaGoals.Add(goal + "~" + type);
		Singleton.PlayerData.LastCompletedArenaGoals[goal] = currentGoals[goal];
		currentGoals.Remove(goal);
	}

	public static void CompleteAllCurrentGoals()
	{
		Dictionary<string, PlayerData.ArenaGoalInfo> arenaGoals = Singleton.PlayerData.ArenaGoals;
		foreach (KeyValuePair<string, PlayerData.ArenaGoalInfo> item in new Dictionary<string, PlayerData.ArenaGoalInfo>(arenaGoals))
		{
			string key = item.Key;
			string type = item.Value.type;
			Singleton.PlayerData.CompletedArenaGoals.Add(key + "~" + type);
			Singleton.PlayerData.LastCompletedArenaGoals[key] = arenaGoals[key];
			arenaGoals.Remove(key);
		}
	}

	private void Increment(string goal)
	{
		if (currentGoals.ContainsKey(goal))
		{
			currentGoals[goal].counter++;
		}
		if (currentGoals.ContainsKey(goal + "_x"))
		{
			currentGoals[goal + "_x"].counter++;
		}
	}

	private void Set(string goal, int value)
	{
		if (currentGoals.ContainsKey(goal))
		{
			currentGoals[goal].counter = value;
		}
		if (currentGoals.ContainsKey(goal + "_x"))
		{
			currentGoals[goal + "_x"].counter = value;
		}
	}

	private void OnHurtPlayer(DamageInfo info)
	{
		Set("kill_unharmed", 0);
	}

	private void OnWeaponShot(Character character, Weapon weapon, int pellets)
	{
		if (character is Player)
		{
			usedWeaponsInWave.Add(weapon.name);
		}
	}

	private void OnEnemyHit(Character character, Unit unit, Weapon weapon, Consumable consumable, float damage)
	{
	}

	private void OnEnemyKill(Character character, Unit unit, Weapon weapon, Consumable consumable)
	{
		if (unit is Zombie && !(unit is Uncommon))
		{
			Increment("kill_common");
			Score(conf.scoreCommon);
			ScoreSoft(conf.softCommon);
		}
		if (unit is Uncommon)
		{
			Increment("kill_uncommon");
			Score(conf.scoreUncommon);
			ScoreSoft(conf.softUncommon);
		}
		if (unit.name == "Uncommon1")
		{
			Increment("kill_uncommon_1");
		}
		if (unit.name == "Uncommon2")
		{
			Increment("kill_uncommon_2");
		}
		if (unit.name == "Uncommon3")
		{
			Increment("kill_uncommon_3");
		}
		if (unit.name == "PiperMinion")
		{
			Increment("kill_piperminion");
		}
		if (unit is Special && !(unit is PiperMinion))
		{
			Increment("kill_special");
			Score(conf.scoreSpecial);
			ScoreSoft(conf.softSpecial);
		}
		if (unit.name == "Piper" || unit.name == "PiperBoss")
		{
			Increment("kill_piper");
		}
		if (unit.name == "Thrower")
		{
			Increment("kill_thrower");
		}
		if (unit.name == "Charger")
		{
			Increment("kill_charger");
		}
		if (unit.name == "Bomber")
		{
			Increment("kill_bomber");
		}
		if (unit.name == "Spitter")
		{
			Increment("kill_spitter");
		}
		Increment("kill_unharmed");
		foreach (KeyValuePair<CountdownTimer, int> item in new Dictionary<CountdownTimer, int>(killTimers))
		{
			if (!item.Key.IsRunning())
			{
				killTimers.Remove(item.Key);
				continue;
			}
			killTimers[item.Key]++;
		}
		Set("kill_time", (killTimers.Count > 0) ? killTimers.Select((KeyValuePair<CountdownTimer, int> p) => p.Value).ToList().Max() : 0);
		string text;
		if (character is Player)
		{
			text = weapon != null ? HumanInventory.SlotOfWeapon(weapon.name).ToString() : string.Empty;
			if (text == "handgun")
			{
				Increment("weapon_handgun");
			}
			if (text == "shotgun")
			{
				Increment("weapon_shotgun");
			}
			if (text == "assault")
			{
				Increment("weapon_assault");
			}
			if (text == "sniper")
			{
				Increment("weapon_sniper");
			}
			if (text == "melee")
			{
				Increment("weapon_melee");
			}
			if (text == "exotic" || consumable != null)
			{
				Increment("weapon_explosion");
			}
			if (unit is Special && !(unit is PiperMinion) && text == "melee")
			{
				Increment("with_melee_special");
			}
			if ((bool)character.GetComponent<Powerup>())
			{
				Increment("with_powerup");
			}
			if (unit.NumDismemberedParts == 0)
			{
				OverkillHands = 0;
			}
			killForHeadshot = (killForDismember = true);
		}
	}

	private void OnEnemyHeadshot(Character character, Unit unit, Weapon weapon, Consumable consumable)
	{
		if (character is Player)
		{
			ScoreSoft(conf.softHead);
			Increment("headshot");
			OverkillHeadshots++;
			killForHeadshot = false;
		}
	}

	private void OnEnemyDismemberment(Character character, Unit unit, Weapon weapon, Consumable consumable, bool isFatal)
	{
		if (character is Player)
		{
			if (isFatal)
			{
				ScoreSoft(conf.softDismember);
				Increment("dismemberment");
				OverkillDismembers++;
				killForDismember = false;
			}
			if (unit.NumDismemberedParts == 0)
			{
				Increment("shoot_hand");
				OverkillHands++;
			}
		}
	}

	private void OnEnemyMutilated(Character character, Unit unit, Weapon weapon, Consumable consumable)
	{
		if (character is Player)
		{
			ScoreSoft(conf.softDismember);
			Increment("dismemberment");
			if (unit.NumDismemberedParts == 0)
			{
				Increment("shoot_hand");
			}
			OverkillDismembers++;
			killForDismember = false;
		}
	}
}
