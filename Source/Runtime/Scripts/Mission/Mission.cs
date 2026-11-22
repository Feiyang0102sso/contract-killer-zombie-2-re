using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

public class Mission : MonoBehaviourAndLog
{
	public enum Type
	{
		GetItems,
		KillAll,
		Rescue,
		Escort,
		SurvivalGates,
		Story1,
		Story2,
		Story3,
		Story4,
		Story5,
		Story6,
		Story7,
		Story8,
		Story9,
		Story10,
		Story11,
		Story12,
		Story13,
		Story14,
		Story15,
		Story16,
		Story17,
		Story18,
		Story19,
		Story20,
		Story21,
		Story22,
		Story23,
		Story24,
		Story25,
		SurvivalArena
	}

	public enum Difficulty
	{
		Normal,
		Hard
	}

	public class NodeInfo
	{
		public List<GameObject> previous = new List<GameObject>();

		public List<GameObject> next = new List<GameObject>();
	}

	private class TreeProcessor
	{
		private StringBuilder text = new StringBuilder();

		private Dictionary<GameObject, string> nodeIds = new Dictionary<GameObject, string>();

		private int nodeIdGen;

		private int tabOffset;

		private Dictionary<GameObject, NodeInfo> tree = new Dictionary<GameObject, NodeInfo>();

		private GameObject firstNode;

		public string GenerateGraph(GameObject root)
		{
			ProcessTree(root);
			return text.ToString();
		}

		public void GenerateTree(GameObject root, out Dictionary<GameObject, NodeInfo> tree_, out GameObject firstNode_)
		{
			ProcessTree(root);
			tree_ = tree;
			firstNode_ = firstNode;
		}

		public void ProcessTree(GameObject root)
		{
			text.Append("digraph Mission\n{\n\tnode [style=filled,color=lightgray] ;\n");
			tabOffset++;
			Process(root, null);
			text.Append("}\n");
		}

		private string GetId(GameObject node)
		{
			if (!nodeIds.ContainsKey(node))
			{
				nodeIds[node] = (++nodeIdGen).ToString();
			}
			return nodeIds[node];
		}

		public string Tab(int shift)
		{
			return new string('\t', tabOffset + shift);
		}

		public string Tab()
		{
			return Tab(0);
		}

		public void Node(GameObject node)
		{
			if (!tree.ContainsKey(node))
			{
				tree[node] = new NodeInfo();
			}
			string text = IsTask(node) ? "[color=lightblue,shape=box]"
				: IsAction(node) ? "[color=orange]"
				: IsTrigger(node) ? "[color=green]"
				: IsSelector(node) ? "[color=violet]" : string.Empty;
			this.text.Append(string.Format("{0}{1} [label=\"{2}\"]{3} ;\n", new string('\t', tabOffset), GetId(node), node.name, text != string.Empty ? " " + text : string.Empty));
		}

		public void Link(GameObject from, GameObject to)
		{
			tree[from].next.Add(to);
			if (!tree.ContainsKey(to))
			{
				tree[to] = new NodeInfo();
			}
			tree[to].previous.Add(from);
			text.Append(string.Format("{0}{1} -> {2} ;\n", Tab(), GetId(from), GetId(to)));
		}

		public void ClusterBegin(GameObject node)
		{
			text.Append(string.Format("{0}subgraph cluster_{2}\n{0}{{{0}\t\n{1}label=\"{3}\";\n", Tab(), Tab(1), GetId(node), node.name));
			tabOffset++;
		}

		public void ClusterEnd()
		{
			tabOffset--;
			text.Append(string.Format("{0}}}\n", Tab()));
		}

		private void Process(GameObject node, GameObject nextNode)
		{
			if (IsTask(node) || IsTrigger(node) || IsAction(node) || IsInit(node))
			{
				if (nextNode != null)
				{
					Link(node, nextNode);
				}
				return;
			}
			if (IsGroup(node))
			{
				ClusterBegin(node);
			}
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in node.transform)
			{
				list.Add(item.gameObject);
			}
			list.Sort((GameObject a, GameObject b) => Util.CompareNatural(a.name, b.name));
			for (int num = 0; num < list.Count; num++)
			{
				GameObject gameObject = list[num];
				GameObject gameObject2 = (!(num == list.Count - 1) ? list[num + 1] : null);
				if (firstNode == null)
				{
					firstNode = gameObject;
				}
				Node(gameObject);
				if (IsSelector(node))
				{
					Link(node, gameObject);
					Process(gameObject, nextNode);
					continue;
				}
				bool flag = firstNode == gameObject;
				if (num == 0 && !flag)
				{
					Link(node, gameObject);
				}
				Process(gameObject, gameObject2 != null ? gameObject2 : nextNode);
			}
			if (IsGroup(node))
			{
				ClusterEnd();
			}
			if (list.Count == 0 && nextNode != null)
			{
				Link(node, nextNode);
			}
		}
	}

	private enum FailureType
	{
		PlayerKilled,
		SurvivorKilled
	}

	public const Type TypeLast = Type.Story25;

	[SerializeField]
	private Type missionType;

	public GameObject startWith;

	[NonSerialized]
	public Difficulty difficulty;

	[NonSerialized]
	public int bonusesTotal;

	[NonSerialized]
	public int bonusesCollected;

	[NonSerialized]
	public float bonusExperiencePercent;

	[NonSerialized]
	public float bonusSoftCurrencyPercent;

	[NonSerialized]
	public int bonusHardCurrency;

	private Dictionary<GameObject, NodeInfo> tree;

	private GameObject firstNode;

	private Task currentTask;

	private string currentTaskId;

	private HashSet<GameObject> parentsOfStart = new HashSet<GameObject>();

	private List<GameObject> missionRoute = new List<GameObject>();

	public Action<Task> OnTaskChanged = (Task obj) =>
	{
	};

	public Action<Mission> OnCompleted = (Mission obj) =>
	{
	};

	public Action<Mission> OnFailed = (Mission obj) =>
	{
	};

	private bool finished;

	[NonSerialized]
	public int killQuantityLeft;

	private MissionConfig config;

	public static bool IsUsualMission(Type type)
	{
		return type >= Type.GetItems && type <= Type.Escort;
	}

	public static bool IsStorylineMission(Type type)
	{
		return type >= Type.Story1 && type <= Type.Story25;
	}

	public static bool IsSurvival(Type type)
	{
		return type == Type.SurvivalGates || type == Type.SurvivalArena;
	}

	public static bool IsSurvivalGates(Type type)
	{
		return type == Type.SurvivalGates;
	}

	public static bool IsSurvivalArena(Type type)
	{
		return type == Type.SurvivalArena;
	}

	public bool IsStorylineMission()
	{
		return IsStorylineMission(missionType);
	}

	public bool IsSurvival()
	{
		return IsSurvival(missionType);
	}

	public bool IsSurvivalGates()
	{
		return IsSurvivalGates(missionType);
	}

	public bool IsSurvivalArena()
	{
		return IsSurvivalArena(missionType);
	}

	public int GetStorylineNumber()
	{
		return (int)(missionType - 5 + 1);
	}

	public static bool IsHard(Difficulty difficulty)
	{
		return difficulty == Difficulty.Hard;
	}

	public bool IsHard()
	{
		return IsHard(difficulty);
	}

	public MissionConfig Config
	{
		get
		{
			return config;
		}
	}

	public static int GetEffectivePlayersLevel()
	{
		return Singleton.PlayerData.StorylineTicket;
	}

	public static GameObject GetSpecialTarget()
	{
		if ((bool)Scene.Mission && Scene.Mission.IsSurvivalGates())
		{
			return Util.FindObjectOfType<Gate>().gameObject;
		}
		return Scene.Player.gameObject;
	}

	private void Awake()
	{
		LogTo("Mission.Base");
	}

	public void Init()
	{
		Util.DoForHierarchy(base.gameObject, false, (GameObject node) =>
		{
			node.active = false;
			return true;
		});
		if (Singleton.MetaMission.missionType >= Type.GetItems)
		{
			missionType = Singleton.MetaMission.missionType;
		}
		difficulty = Singleton.PlayerData.CurrentMissionDifficulty;
		TreeProcessor treeProcessor = new TreeProcessor();
		treeProcessor.GenerateTree(base.gameObject, out tree, out firstNode);
		startWith = null;
		if (startWith != null)
		{
		}
		if (startWith != null)
		{
			Queue<GameObject> queue = new Queue<GameObject>();
			queue.Enqueue(startWith);
			while (true)
			{
				if (queue.Count == 0)
				{
					break;
				}
				GameObject gameObject = queue.Dequeue();
				parentsOfStart.Add(gameObject);
				if (gameObject.GetParent() != null)
				{
					queue.Enqueue(gameObject.GetParent());
				}
			}
		}
		GameObject gameObject2 = firstNode;
		while (true)
		{
			missionRoute.Add(gameObject2);
			GameObject gameObject3 = PickNextNode(gameObject2);
			if (gameObject3 == null)
			{
				break;
			}
			gameObject2 = gameObject3;
		}
		HashSet<GameObject> hashSet = new HashSet<GameObject>(missionRoute);
		foreach (GameObject item in missionRoute)
		{
			GameObject gameObject4 = item;
			while (gameObject4 != null)
			{
				if (!hashSet.Contains(gameObject4))
				{
					hashSet.Add(gameObject4);
				}
				gameObject4 = gameObject4.GetParent();
			}
		}
		foreach (KeyValuePair<GameObject, NodeInfo> item2 in tree)
		{
			GameObject key = item2.Key;
			if (key != null && !hashSet.Contains(key))
			{
				UnityEngine.Object.DestroyImmediate(key);
			}
		}
		missionRoute.ForEach((GameObject node) =>
		{
			Util.DoForHierarchy(node, (GameObject n) =>
			{
				foreach (KeyValuePair<string, string> item3 in CustomTags())
				{
					string key3 = item3.Key;
					string text2 = "{Tag:" + item3.Value + "}";
					if (n.name.IndexOf(text2) >= 0)
					{
						continue;
					}
					if (n.name == key3 || n.name.StartsWith(key3 + " "))
					{
						n.name += " " + text2;
					}
				}
				return true;
			});
		});
		Dictionary<string, List<GameObject>> dictionary = new Dictionary<string, List<GameObject>>();
		HashSet<GameObject> missionRouteFull = new HashSet<GameObject>();
		foreach (GameObject item4 in missionRoute)
		{
			Util.DoForHierarchy(item4, (GameObject n) => missionRouteFull.Add(n));
		}
		foreach (GameObject item5 in missionRouteFull)
		{
			string unusedTag;
			if (item5.name.Contains("{Tag:"))
			{
				string text = item5.name.Split("{Tag:")[1].Split("}")[0];
				if (!dictionary.ContainsKey(text))
				{
					dictionary.Add(text, new List<GameObject>());
				}
				dictionary[text].Add(item5);
				log.Debug(item5, "found unique node '{0}', name '{1}'", item5.name, text);
			}
		}
		Dictionary<string, List<GameObject>> dictionary2 = new Dictionary<string, List<GameObject>>();
		foreach (KeyValuePair<string, List<GameObject>> item6 in dictionary)
		{
			string key2 = item6.Key;
			List<GameObject> value = item6.Value;
			int num = AmountForTag(key2, missionType, difficulty);
			List<GameObject> list = new List<GameObject>();
			Dictionary<GameObject, float> dictionary3 = value.ToDictionary((GameObject obj) => obj, (GameObject obj) =>
			{
				string unusedWeightText;
				float result;
				if (obj.name.Contains("{Weight:") && float.TryParse(obj.name.Split("{Weight:")[1].Split("}")[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				{
					return 1f / result;
				}
				return 1f;
			});
			for (int num2 = 0; num2 < num; num2++)
			{
				if (dictionary3.Count == 0)
				{
					break;
				}
				GameObject gameObject5 = Util.RandomByWeights(dictionary3);
				dictionary3.Remove(gameObject5);
				list.Add(gameObject5);
			}
			log.Info("from {0} unique nodes '{1}' picked {2}: {3}", value.Count, key2, list.Count, list.Implode(" and ", (GameObject k) => "'" + k.name + "'"));
			dictionary2[key2] = new List<GameObject>(list);
			foreach (GameObject item7 in value)
			{
				if (!list.Contains(item7))
				{
					Util.DoForHierarchy(item7, (GameObject sub) =>
					{
						if (missionRoute.Contains(sub))
						{
							missionRoute.Remove(sub);
						}
						return true;
					});
					UnityEngine.Object.Destroy(item7);
				}
			}
		}
		bonusesTotal = (dictionary2.ContainsKey("Bonus") ? dictionary2["Bonus"].Count : 0);
		if (IsStorylineMission(missionType))
		{
			GameData.StorylineData missionStoryline = Singleton.GameData.GetMissionStoryline(missionType);
			bonusHardCurrency += missionStoryline.RewardHC;
		}
		config = base.gameObject.GetComponent<MissionConfig>();
		if (!config)
		{
			config = base.gameObject.AddComponent<MissionConfig>();
		}
		missionRoute.ForEach((GameObject node) =>
		{
			Util.DoForHierarchy(node, false, (GameObject n) =>
			{
				MissionConfig component = n.GetComponent<MissionConfig>();
				if ((bool)component)
				{
					config.spawnStatics = component.spawnStatics;
				}
				return false;
			});
		});
		killQuantityLeft = 0;
		missionRoute.ForEach((GameObject node) =>
		{
			Util.DoForHierarchy(node, false, (GameObject n) =>
			{
				KillQuantity component = n.GetComponent<KillQuantity>();
				if ((bool)component)
				{
					killQuantityLeft += component.PreCalcQuantity();
				}
				return false;
			});
		});
		Singleton.GameEvents.OnMissionStarted(PlayerData.Instance.CurrentLocation);
		SoundManager.Play("Mission/mission_started", false);
		ActivateItem.terminalNumber = 0;
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("mission: {0}, difficulty: {1}\n", missionType.ToString(), difficulty.ToString()) + (currentTask ? string.Format("task: {0} {1}", currentTaskId, currentTask.DebugInfo()) : "task: none"));
		ProceedToNextTask();
		if (currentTask == null)
		{
		}
	}

	public void PreWarmSpawn()
	{
		int num = 0;
		int num2 = 0;
		foreach (GameObject item in missionRoute)
		{
			if (!IsTask(item) && !IsAction(item) && !IsTrigger(item))
			{
				continue;
			}
			foreach (ScenarioWave wave in item.GetComponentsInChildren<ScenarioWave>(true))
			{
				Scene.Spawner.AddToHell(() => Scene.Spawner.CommonOrUncommonZombie()(), wave.GetQuantity());
				Scene.Spawner.AddToHell(() => Scene.Spawner.SpecialZombie(wave.specialType)(), wave.GetSpecialQuantity());
				num += wave.GetQuantity();
				num2 += wave.GetSpecialQuantity();
				log.Debug(wave.gameObject, "added to hell from {0}: {1} zombies{2}", TaskIdFromNode(wave.gameObject), wave.GetQuantity(), (wave.specialType == Spawner.SpecialType.Any) ? string.Empty : string.Format(", {0} {1}", wave.GetSpecialQuantity(), wave.specialType.ToString().ToLower()));
			}
		}
	}

	private GameObject PickNextNode(GameObject node)
	{
		if (tree[node].next.Count == 0)
		{
			return null;
		}
		GameObject gameObject;
		if (tree[node].next.Count > 1)
		{
			Dictionary<GameObject, float> dictionary = new Dictionary<GameObject, float>();
			Dictionary<GameObject, float> dictionary2 = new Dictionary<GameObject, float>();
			foreach (GameObject item in tree[node].next)
			{
				bool flag = true;
				bool flag2 = true;
				bool flag3 = true;
				float result = 1f;
				if (item.name.Contains("{Type:"))
				{
					flag = false;
				}
				if (item.name.Contains("{Flag:"))
				{
					flag3 = false;
				}
				string[] array = item.name.Split("{", "}");
				if (array.Length % 2 != 1)
				{
					log.Error(item, "unparsable: '{0}'", item.name);
					continue;
				}
				bool flag4 = false;
				for (int i = 1; array.Length > 1 && i < array.Length; i += 2)
				{
					string str = array[i];
					if (Util.TryRemovePrefix(ref str, "Type:"))
					{
						flag4 = true;
						Type type;
						try
						{
							type = (Type)(int)Enum.Parse(typeof(Type), str);
						}
						catch (Exception)
						{
							log.Error(item, "unknown type: '{0}'", str);
							continue;
						}
						if (type == missionType)
						{
							flag = true;
						}
					}
					else if (Util.TryRemovePrefix(ref str, "Level"))
					{
						flag4 = true;
						int result2;
						string text = null;
						foreach (string text2 in new string[4] { "<=", ">=", "<", ">" })
						{
							if (Util.TryRemovePrefix(ref str, text2))
							{
								text = text2;
								break;
							}
						}
						if (text == null)
						{
							log.Error(item, "unknown operation: '{0}'", str);
							continue;
						}
						if (!int.TryParse(str, out result2))
						{
							log.Error(item, "unparsable level: '{0}'", str);
							continue;
						}
						int level = Singleton.PlayerData.Level;
						if ((text == "<=" && level > result2) || (text == ">=" && level < result2) || (text == "<" && level >= result2) || (text == ">" && level <= result2))
						{
							flag2 = false;
						}
					}
					else if (Util.TryRemovePrefix(ref str, "Flag:"))
					{
						flag4 = true;
						if (str == "NewZombies" && Singleton.Quality.NewZombies)
						{
							flag3 = true;
						}
					}
					else if (Util.TryRemovePrefix(ref str, "Weight:"))
					{
						if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
						{
							log.Error(item, "unparsable weight: '{0}'", str);
							continue;
						}
					}
					else if (Util.TryRemovePrefix(ref str, "Tag:"))
					{
					}
					else
					{
						log.Error(item, "unknown condition: '{0}'", str);
						continue;
					}
				}
				bool flag5 = flag && flag2 && flag3;
				if (flag5)
				{
					(flag4 ? dictionary : dictionary2)[item] = result;
				}
				log.Debug(item, "{0} '{1}' (weight {2})", flag5 ? "suits" : "avoiding", item.name, result);
			}
			Dictionary<GameObject, float> dictionary3 = dictionary.Count > 0 ? dictionary : dictionary2;
			Dictionary<GameObject, float> dictionary4 = new Dictionary<GameObject, float>(dictionary3);
			if (startWith != null)
			{
				foreach (KeyValuePair<GameObject, float> item2 in dictionary3)
				{
					if (!parentsOfStart.Contains(item2.Key))
					{
						dictionary4.Remove(item2.Key);
					}
				}
			}
			if (dictionary4.Count > 0)
			{
				if (dictionary3.Count != dictionary4.Count)
				{
					log.Debug(node, "dropped {0} choices due to start-with", dictionary3.Count - dictionary4.Count);
				}
				dictionary3 = dictionary4;
			}
			else
			{
				dictionary4.Clear();
				foreach (GameObject item3 in tree[node].next)
				{
					if (parentsOfStart.Contains(item3))
					{
						dictionary4.Add(item3, 1f);
					}
				}
				if (dictionary4.Count > 0)
				{
					if (dictionary3.Count != dictionary4.Count)
					{
						log.Debug(node, "dropped {0} choices due to start-with, while ignoring conditions", dictionary3.Count - dictionary4.Count);
					}
					dictionary3 = dictionary4;
				}
			}
			gameObject = Util.RandomByWeights(dictionary3);
			log.Debug(node, "picked randomly next '{0}' from {1} choices", gameObject.name, dictionary3.Count);
		}
		else
		{
			gameObject = tree[node].next[0];
		}
		return gameObject;
	}

	private void ProceedToNextTask()
	{
		GameObject gameObject = currentTask ? currentTask.gameObject : null;
		while (true)
		{
			if (gameObject == null)
			{
				gameObject = firstNode;
			}
			else
			{
				if (startWith == null || IsInit(gameObject))
				{
					DeactivateNode(gameObject);
				}
				int num = missionRoute.IndexOf(gameObject) + 1;
				GameObject gameObject2 = num < missionRoute.Count ? missionRoute[num] : null;
				if (gameObject2 == null)
				{
					log.Debug(gameObject, "leaving last node '{0}'", gameObject);
					currentTask = null;
					return;
				}
				log.Info(gameObject2, "going from '{0}' to '{1}'", gameObject.name, gameObject2.name);
				gameObject = gameObject2;
			}
			if (startWith == gameObject)
			{
				startWith = null;
				log.Info("starting mission with '{0}'", gameObject.name);
			}
			if (startWith == null || IsInit(gameObject))
			{
				ActivateNode(gameObject);
				if (IsTask(gameObject))
				{
					log.Info(gameObject, "current task is '{0}'", gameObject.name);
					break;
				}
			}
			else if (startWith != null)
			{
				log.Info(gameObject, "skipping '{0}'", gameObject.name);
			}
		}
		if ((bool)currentTask && currentTask.soundWhenCompleted)
		{
			SoundManager.Play(IsSurvival() ? "task_finished" : "Mission/task_changed", false);
		}
		currentTask = gameObject.GetComponent<Task>();
		currentTaskId = TaskIdFromNode(currentTask.gameObject);
		Player player = Scene.Player ?? Util.FindObjectOfType<Player>();
		if (player != null)
		{
			player.OnTaskChanged(currentTask);
		}
		OnTaskChanged(currentTask);
	}

	public Task GetCurrentTask()
	{
		return currentTask;
	}

	public float GetCurrentTaskProgress()
	{
		return currentTask ? currentTask.GetProgress() : (-1f);
	}

	private void ActivateNode(GameObject node)
	{
		node.active = true;
		if (IsAction(node) || IsTask(node) || IsTrigger(node) || IsInit(node))
		{
			Util.DoForHierarchy(node, false, (GameObject subNode) =>
			{
				subNode.active = true;
				return true;
			});
			if (!IsTrigger(node))
			{
				node.BroadcastMessage("OnActivate");
			}
		}
		if (IsInit(node))
		{
			if ((bool)node.FindChild("Player"))
			{
				node.FindChild("Player").transform.parent = null;
			}
			foreach (Survivor survivor in node.GetComponentsInChildren<Survivor>())
			{
				Singleton.Visual.ShowOutline(survivor.gameObject, new Color(0f, 1f, 0f), 1);
			}
		}
		if (IsTask(node))
		{
			node.SendMessage("OnStarted");
		}
	}

	private void DeactivateNode(GameObject node)
	{
		if (IsTask(node))
		{
			node.SendMessage("OnFinished");
		}
		// PC port: move out what keeps running under 3.5 before the node goes inactive (see NewUnityLegacyActive)
		NewUnityLegacyActive.RescueBeforeDeactivate(node);
		if (IsTask(node) || IsAction(node))
		{
			node.active = false;
		}
		if (!(!IsTask(node)))
		{
			Util.DoForHierarchy(node, true, (GameObject subNode) =>
			{
				if ((bool)subNode.GetComponent<Character>())
				{
					return false;
				}
				if ((bool)subNode.GetComponent<ActivateItem>())
				{
					subNode.GetComponent<ActivateItem>().Disappear();
					return false;
				}
				subNode.active = false;
				return true;
			});
		}
		NewUnityLegacyActive.RescueLegacyActiveChildren(node);
	}

	private string TaskIdFromNode(GameObject task)
	{
		string text = string.Empty;
		GameObject gameObject = task;
		while (gameObject != null && !gameObject.GetComponent<Mission>())
		{
			text = gameObject.name.Split(")")[0] + (gameObject == task ? string.Empty : "-") + text;
			gameObject = gameObject.GetParent();
		}
		return text;
	}

	private void OnTaskCompleted()
	{
		log.Info(currentTask, "task '{0}' completed", currentTask.name);
		ProceedToNextTask();
		if (currentTask == null)
		{
			MissionCompleted();
		}
	}

	private void OnCharacterDead(GameObject character)
	{
		if (!character.GetComponent<Human>())
		{
			Singleton.Visual.HideOutline(character);
		}
		if ((bool)character.GetComponent<Player>())
		{
			MissionFailed(GameEvents.MissionFailure.PlayerKilled);
		}
		else if ((bool)character.GetComponent<Human>() && ShouldResurrect(character))
		{
			MissionFailed(GameEvents.MissionFailure.SurvivorKilled);
		}
	}

	public void ReportAction(WaitForAction.Type type)
	{
		ReportAction(type, string.Empty);
	}

	public void ReportAction(WaitForAction.Type type, string argument)
	{
		if (currentTask != null)
		{
			currentTask.ReportAction(type, argument);
		}
	}

	public int ExperienceBase()
	{
		bool isSpecialHunt = !Singleton.PlayerData.IsSpecialHuntReceived && PlayerData.Instance.CurrentMissionIndex == Singleton.ShopData.SpecialHunt.missionIndex;
		return ExperienceBase(missionType, Scene.Mission.difficulty, isSpecialHunt);
	}

	public static int ExperienceBase(Type missionType, Difficulty difficulty, bool isSpecialHunt)
	{
		return Singleton.GameData.GetRewardXP(Singleton.PlayerData.Level, missionType, difficulty, isSpecialHunt);
	}

	public int SoftCurrencyBase()
	{
		bool isSpecialHunt = !Singleton.PlayerData.IsSpecialHuntReceived && PlayerData.Instance.CurrentMissionIndex == Singleton.ShopData.SpecialHunt.missionIndex;
		return SoftCurrencyBase(Scene.Mission.missionType, Scene.Mission.difficulty, isSpecialHunt);
	}

	public static int SoftCurrencyBase(Type missionType, Difficulty difficulty, bool isSpecialHunt)
	{
		return Singleton.GameData.GetRewardSC(Singleton.PlayerData.Level, missionType, difficulty, isSpecialHunt);
	}

	public bool ShouldResurrect(GameObject character)
	{
		return !((character.GetComponent<Human>() && character.GetComponent<Human>().isEvil) || (character.GetComponent<Bot>() && IsSurvivalArena()));
	}

	public static bool IsGroup(GameObject node)
	{
		return node.name.Contains("[Group]");
	}

	public static bool IsSelector(GameObject node)
	{
		return node.name.Contains("[Selector]");
	}

	public static bool IsTask(GameObject node)
	{
		return node.name.Contains("[Task]");
	}

	public static bool IsAction(GameObject node)
	{
		return node.name.Contains("[Action]");
	}

	public static bool IsTrigger(GameObject node)
	{
		return node.name.Contains("[Trigger]");
	}

	public static bool IsInit(GameObject node)
	{
		return node.name.Contains("[Init]");
	}

	private void MissionCompleted()
	{
		if (finished)
		{
			return;
		}
		finished = true;
		if (missionType == Type.KillAll)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.CompleteKillAllMissions);
		}
		else if (missionType == Type.Escort)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.EscortSurvivors);
		}
		SoundManager.Play("Mission/mission_completed", false);
		if (IsSurvival())
		{
			Singleton.PlayerData.ProgressGoal(Goal.Type.PlayEndlessMode);
		}
		if (missionType == Type.KillAll)
		{
			float num = (int)(Singleton.PlayerData.PlayingTime - Singleton.PlayerData.PlayerStatistics.MissionStartTime);
			if (num <= 180f)
			{
				Singleton.PlayerData.ProgressGoal(Goal.Type.CompleteKillAllIn3Min);
			}
			if (num <= 240f)
			{
				Singleton.PlayerData.ProgressGoal(Goal.Type.CompleteKillAllIn4Min);
			}
		}
		Singleton.GameEvents.OnMissionCompleted(PlayerData.Instance.CurrentLocation);
		if (bonusesTotal == bonusesCollected)
		{
			GameAnalytics.LogAllBonusCratesFound(missionType, Singleton.PlayerData.CurrentMissionIndex, Singleton.PlayerData.Level);
		}
		if (IsSurvivalArena())
		{
			Singleton.PlayerData.PlayerStatistics.MissionStatistics.SurvivalArenaBestScore = GetComponentInChildren<ArenaWaves>().scoreTotal;
			ArenaMissionReward arenaMissionReward = new ArenaMissionReward(GetComponentInChildren<ArenaWaves>());
			arenaMissionReward.GiveRewards();
			Singleton.UI.PopupManager.ShowArenaRevivePopup(arenaMissionReward);
		}
		else
		{
			BasicMissionReward basicMissionReward = new BasicMissionReward();
			basicMissionReward.GiveRewards();
			Singleton.UI.PopupManager.ShowMissionCompletePopup(basicMissionReward);
		}
		OnCompleted(this);
	}

	private void MissionFailed(GameEvents.MissionFailure failureType)
	{
		if (finished)
		{
			return;
		}
		finished = true;
		SoundManager.Play("Mission/mission_failed", false);
		if (IsSurvival())
		{
			Singleton.PlayerData.ProgressGoal(Goal.Type.PlayEndlessMode);
		}
		if (IsSurvivalArena())
		{
			Singleton.PlayerData.PlayerStatistics.MissionStatistics.SurvivalArenaBestScore = GetComponentInChildren<ArenaWaves>().scoreTotal;
		}
		Singleton.GameEvents.OnMissionFailed(PlayerData.Instance.CurrentLocation, failureType, missionType);
		if (IsSurvivalGates())
		{
			Singleton.UI.PopupManager.ShowSurvivalModeFailPopup(new SurvivalGatesMissionReward());
		}
		else if (IsSurvivalArena())
		{
			Singleton.UI.PopupManager.ShowArenaRevivePopup(new ArenaMissionReward(GetComponentInChildren<ArenaWaves>()));
		}
		else
		{
			Singleton.UI.PopupManager.ShowMissionFailPopup(failureType, new BasicMissionReward());
		}
	}

	public void ExitMission()
	{
		Singleton.Game.GoToCityMap();
	}

	public void OnRevived(bool revived, bool consumablePurchased)
	{
		if (revived)
		{
			finished = false;
			if (Singleton.PlayerData.HasMission(Singleton.PlayerData.CurrentLocation))
			{
				int mission = Singleton.PlayerData.GetMission(Singleton.PlayerData.CurrentLocation);
				Type type = Singleton.GameData.Missions[mission].Type;
				GameAnalytics.LogReviveUsed(type, Singleton.PlayerData.Level, consumablePurchased);
			}
			if (IsSurvivalArena())
			{
				GetComponentInChildren<ArenaWaves>().OnRevived();
			}
			{
				foreach (Human human in Scene.Humans)
				{
					human.Health = human.healthTotal;
				}
			}
		}
		else
		{
			OnFailed(this);
			if (Scene.Mission.IsSurvivalGates())
			{
				new SurvivalGatesMissionReward().GiveRewards();
			}
			if (Scene.Mission.IsSurvivalArena())
			{
				new ArenaMissionReward(GetComponentInChildren<ArenaWaves>()).GiveRewards();
			}
			Singleton.Game.GoToCityMap();
		}
	}

	private static Dictionary<string, string> CustomTags()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("AmmoBox", "Ammo");
		dictionary.Add("BonusBox", "Bonus");
		return dictionary;
	}

	public static int AmountForTag(string tag, Type type, Difficulty difficulty)
	{
		if (tag == "Special")
		{
			return Singleton.GameData.GetCurrentPlayerLevelWaveData().Specials;
		}
		if (tag == "Ammo")
		{
			return IsStorylineMission(type) ? 0 : IsHard(difficulty) ? 0 : 2;
		}
		if (tag == "Bonus")
		{
			return IsStorylineMission(type) ? (type == Type.Story2 ? 1 : 2) : (IsHard(difficulty) ? 2 : 1);
		}
		return 1;
	}
}
