using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviourAndLog
{
	private class StaticSpawn
	{
		public enum NearWall
		{
			No,
			Sit,
			Lean
		}

		public Vector3 position;

		public int nearestPathIndex;

		public NearWall nearWall;

		public Vector3 wallNormal;

		public Zombie zombie;

		public bool used;
	}

	public class SpawnInfo
	{
		public GameObject prefab;

		public int tier;

		public SpawnInfo(GameObject prefab_, int tier_)
		{
			prefab = prefab_;
			tier = tier_;
		}
	}

	public enum SpecialType
	{
		Any,
		Piper,
		Charger,
		PiperMinion,
		Thrower,
		PiperBoss,
		Bomber,
		Spitter,
		Total
	}

	public enum UncommonType
	{
		Any,
		Uncommon1,
		Uncommon2,
		Uncommon3,
		Total
	}

	public enum SurvivorType
	{
		SurvivorUnarmed,
		SurvivorArmed
	}

	public class PointDesc
	{
		public enum PointRelativeToRoute
		{
			AHEAD,
			BEHIND,
			ANY
		}

		private float _minRadius;

		private float _maxRadius;

		private bool _visible;

		private PointRelativeToRoute _relativeToRoute;

		private SpawnPoint[] _specificPoints = new SpawnPoint[0];

		public static PointDesc AnyVisible()
		{
			return new PointDesc(true);
		}

		public static PointDesc AnyInvisible()
		{
			return new PointDesc(false);
		}

		private PointDesc(bool visible)
			: this(visible, PointRelativeToRoute.ANY)
		{
		}

		private PointDesc(bool visible, PointRelativeToRoute relativeToRoute)
			: this(visible, relativeToRoute, -1, -1)
		{
		}

		private PointDesc(bool visible, PointRelativeToRoute relativeToRoute, int nearestRank)
			: this(visible, relativeToRoute, nearestRank, -1)
		{
		}

		private PointDesc(bool visible, PointRelativeToRoute relativeToRoute, int nearestRank, int farthestRank)
			: this(visible, relativeToRoute, nearestRank, farthestRank, 0f, float.MaxValue)
		{
		}

		private PointDesc(bool visible, PointRelativeToRoute relativeToRoute, float minRadius, float maxRadius)
			: this(visible, relativeToRoute, -1, -1, minRadius, maxRadius)
		{
		}

		public PointDesc(bool visible, PointRelativeToRoute relativeToRoute, int nearestRank, int farthestRank, float minRadius, float maxRadius)
		{
			_minRadius = minRadius;
			_maxRadius = maxRadius;
			_visible = visible;
			_relativeToRoute = relativeToRoute;
			NearestRank = nearestRank;
			FarthestRank = farthestRank;
		}

		private PointDesc(SpawnPoint[] points)
		{
			_specificPoints = points;
		}

		public static PointDesc NearestVisible()
		{
			return new PointDesc(true, PointRelativeToRoute.ANY, 0);
		}

		public static PointDesc NearestInvisible()
		{
			return new PointDesc(false, PointRelativeToRoute.ANY, 0);
		}

		public static PointDesc AnyVisibleAhead()
		{
			return new PointDesc(true, PointRelativeToRoute.AHEAD);
		}

		public static PointDesc AnyInvisibleAhead()
		{
			return new PointDesc(false, PointRelativeToRoute.AHEAD);
		}

		public static PointDesc NearestVisibleAhead()
		{
			return new PointDesc(true, PointRelativeToRoute.AHEAD, 0);
		}

		public static PointDesc NearestInvisibleAhead()
		{
			return new PointDesc(false, PointRelativeToRoute.AHEAD, 0);
		}

		public static PointDesc AnyVisibleBehind()
		{
			return new PointDesc(true, PointRelativeToRoute.BEHIND);
		}

		public static PointDesc AnyInvisibleBehind()
		{
			return new PointDesc(false, PointRelativeToRoute.BEHIND);
		}

		public static PointDesc NearestVisibleBehind()
		{
			return new PointDesc(true, PointRelativeToRoute.BEHIND, 0);
		}

		public static PointDesc NearestInvisibleBehind()
		{
			return new PointDesc(false, PointRelativeToRoute.BEHIND, 0);
		}

		public static PointDesc Visible(float minRadius, float maxRadius)
		{
			return new PointDesc(true, PointRelativeToRoute.ANY, -1, -1, minRadius, maxRadius);
		}

		public static PointDesc Visible(int nearestRank)
		{
			return new PointDesc(true, PointRelativeToRoute.ANY, nearestRank, -1, 0f, float.MaxValue);
		}

		public static PointDesc VisibleAhead(int nearestRank)
		{
			return new PointDesc(true, PointRelativeToRoute.AHEAD, nearestRank, -1, 0f, float.MaxValue);
		}

		public static PointDesc VisibleBehind(int nearestRank)
		{
			return new PointDesc(true, PointRelativeToRoute.BEHIND, nearestRank, -1, 0f, float.MaxValue);
		}

		public static PointDesc Invisible(float minRadius, float maxRadius)
		{
			return new PointDesc(false, PointRelativeToRoute.ANY, -1, -1, minRadius, maxRadius);
		}

		public static PointDesc Invisible(int nearestRank, float minRadius)
		{
			return new PointDesc(false, PointRelativeToRoute.ANY, nearestRank, -1, minRadius, float.MaxValue);
		}

		public static PointDesc InvisibleAhead(int nearestRank, float minRadius)
		{
			return new PointDesc(false, PointRelativeToRoute.AHEAD, nearestRank, -1, minRadius, float.MaxValue);
		}

		public static PointDesc InvisibleBehind(int nearestRank, float minRadius)
		{
			return new PointDesc(false, PointRelativeToRoute.BEHIND, nearestRank, -1, minRadius, float.MaxValue);
		}

		public static PointDesc SpecificPoints(SpawnPoint[] points)
		{
			return new PointDesc(points);
		}

		public int NearestRank { get; private set; }

		public int FarthestRank { get; private set; }

		public bool MaxRadiusIsValid()
		{
			return _maxRadius != float.MaxValue;
		}

		public SpawnPoint[] GetSpecificPoints()
		{
			return _specificPoints;
		}

		public bool Check(SmartSpawnPoint point, float maxRadiusAdd, out float pathDistance)
		{
			pathDistance = point.GetApproximateDistanceFromPlayer();
			if (!point.Enabled)
			{
				return false;
			}
			if (!_visible && (point.PlayerCanSee || point.IsVisibleSpawn()))
			{
				return false;
			}
			if (_visible && (!point.IsVisibleSpawn() || !point.IsVisibleSpawnWorking()))
			{
				return false;
			}
			if (pathDistance < _minRadius)
			{
				return false;
			}
			if (pathDistance > _maxRadius + maxRadiusAdd)
			{
				return false;
			}
			if (_relativeToRoute == PointRelativeToRoute.BEHIND && point.IsAheadPlayerPath && !point.IsUndeterminedRelativeState)
			{
				return false;
			}
			if (_relativeToRoute == PointRelativeToRoute.AHEAD && !point.IsAheadPlayerPath && !point.IsUndeterminedRelativeState)
			{
				return false;
			}
			return true;
		}

		public void ExpandConditions()
		{
			if (_relativeToRoute != PointRelativeToRoute.ANY)
			{
				_relativeToRoute = PointRelativeToRoute.ANY;
				return;
			}
			if (_visible)
			{
				_visible = false;
				return;
			}
			if (NearestRank >= 0)
			{
				NearestRank = -1;
				return;
			}
			if (FarthestRank >= 0)
			{
				FarthestRank = -1;
				return;
			}
			if (_minRadius >= 0f)
			{
				_minRadius = 0f;
				return;
			}
			if (_maxRadius < float.MaxValue)
			{
				_maxRadius = float.MaxValue;
				return;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}visible, relative:{1}", _visible ? string.Empty : "in", _relativeToRoute.ToString().ToLower());
		}
	}

	private HashSet<Character> _spawnedUnits = new HashSet<Character>();

	private SmartSpawnPoint[] _spawnPoints;

	private bool _spawnPointsDataUpdated;

	private List<StaticSpawn> staticSpawns = new List<StaticSpawn>();

	private int staticsTotal = -1;

	private int staticsLeft = -1;

	private Dictionary<KeyValuePair<string, object>, Queue<GameObject>> hellPool = new Dictionary<KeyValuePair<string, object>, Queue<GameObject>>();

	private Dictionary<string, float> _commonZombies;

	private Dictionary<string, float> _uncommonZombies;

	private GameObject anyNavPoint;

	private static int NextCharacterTier = -1;

	private int maxUnits
	{
		get
		{
			return !(!Scene.Mission || !Scene.Mission.Config || Scene.Mission.Config.maxUnits < 0) ? Scene.Mission.Config.maxUnits : Singleton.Quality.ZombiesLimit;
		}
	}

	public GameObject GetCharacter(string name)
	{
		return Singleton.Cache.PerResourceCache.Load(name);
	}

	private Dictionary<string, float> commonZombies
	{
		get
		{
			if (_commonZombies == null)
			{
				_commonZombies = Singleton.Quality.NewZombies ? new Dictionary<string, float>
				{
					{ "Zombie3", 1f },
					{ "Zombie4", 1f },
					{ "Zombie5", 1f }
				} : new Dictionary<string, float>
				{
					{ "Zombie1", 1f },
					{ "Zombie2", 1f }
				};
			}
			return _commonZombies;
		}
	}

	private Dictionary<string, float> uncommonZombies
	{
		get
		{
			if (_uncommonZombies == null)
			{
				_uncommonZombies = new Dictionary<string, float>
				{
					{ "Uncommon1", 1f },
					{ "Uncommon2", 1f },
					{ "Uncommon3", 1f }
				};
			}
			return _uncommonZombies;
		}
	}

	public void PreLoadCharacters()
	{
		foreach (KeyValuePair<string, float> commonZombie in commonZombies)
		{
			GetCharacter(commonZombie.Key);
		}
		foreach (KeyValuePair<string, float> uncommonZombie in uncommonZombies)
		{
			GetCharacter(uncommonZombie.Key);
		}
	}

	public Func<SpawnInfo> CommonOrUncommonZombie()
	{
		GameData.WaveData currentPlayerLevelWaveData = Singleton.GameData.GetCurrentPlayerLevelWaveData();
		return CommonOrUncommonZombie(currentPlayerLevelWaveData.UncommonChance, currentPlayerLevelWaveData.Tier2Common, currentPlayerLevelWaveData.Tier2Uncommon);
	}

	public Func<SpawnInfo> CommonOrUncommonZombie(float uncommonChance, float commonTier2Chance, float uncommonTier2Chance)
	{
		return () =>
		{
			bool flag = Util.Chance(uncommonChance);
			return new SpawnInfo(GetCharacter(Util.RandomByWeights(flag ? uncommonZombies : commonZombies)), Util.Chance(flag ? uncommonTier2Chance : commonTier2Chance) ? 2 : 1);
		};
	}

	public Func<SpawnInfo> SpecialZombie(SpecialType specialType)
	{
		GameData.WaveData currentPlayerLevelWaveData = Singleton.GameData.GetCurrentPlayerLevelWaveData();
		return SpecialZombie(specialType, currentPlayerLevelWaveData.Tier2Special);
	}

	public Func<SpawnInfo> SpecialZombie(SpecialType specialType, float tier2Chance)
	{
		return () =>
		{
			string text = specialType == SpecialType.Any ? ((SpecialType)UnityEngine.Random.Range(1, 7)).ToString() : specialType.ToString();
			return new SpawnInfo(GetCharacter(text), Util.Chance(tier2Chance) ? 2 : 1);
		};
	}

	public Func<SpawnInfo> UncommonZombie(UncommonType uncommonType, float tier2Chance)
	{
		return () =>
		{
			string text = uncommonType == UncommonType.Any ? ((UncommonType)UnityEngine.Random.Range(1, 3)).ToString() : uncommonType.ToString();
			return new SpawnInfo(GetCharacter(text), Util.Chance(tier2Chance) ? 2 : 1);
		};
	}

	public Func<SpawnInfo> Survivor()
	{
		return () =>
		{
			GameObject character = GetCharacter(Util.RandomFrom<string>("Survivor1", "Survivor2a", "Survivor2b", "Survivor3"));
			return new SpawnInfo(character, 1);
		};
	}

	private void Awake()
	{
		LogTo("Spawn.Spawner");
	}

	public void Init()
	{
		if (!(Loader.IsGameScene()))
		{
			return;
		}
		_spawnedUnits.Clear();
		foreach (Zombie item in UnityEngine.Object.FindSceneObjectsOfType(typeof(Zombie)) as Zombie[])
		{
			_spawnedUnits.Add(item);
		}
		_spawnPoints = UnityEngine.Object.FindSceneObjectsOfType(typeof(SmartSpawnPoint)) as SmartSpawnPoint[];
		foreach (SmartSpawnPoint smartSpawnPoint in _spawnPoints)
		{
			smartSpawnPoint.Init();
		}
		if ((bool)Scene.Mission)
		{
			Scene.Mission.PreWarmSpawn();
		}
		InitStatics();
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("attacking/all zombies: {0}/{1}, specials: {2}, humans: {3}\n", Scene.AttackingZombies.Count, Scene.Zombies.Count, Scene.Specials.Count, Scene.Humans.Count) + string.Format("progress: {0}/{1}\n", Scene.PlayerProgress.CurrentPathIndex, Scene.PlayerProgress.GetPath().Count - 1) + string.Format("enemies killed: {0}/{1}", Singleton.PlayerData.LastMissionStatistics.TotalNumKills, Singleton.PlayerData.LastMissionStatistics.TotalNumSpawns()));
	}

	public void AddToHell(Func<SpawnInfo> what, int quantity)
	{
		for (int i = 0; i < quantity; i++)
		{
			AddToHell(what());
		}
	}

	private void AddToHell(SpawnInfo what)
	{
		if (!anyNavPoint)
		{
			anyNavPoint = Util.FindObjectOfType<SmartSpawnPoint>().gameObject;
		}
		if (!((bool)anyNavPoint))
		{
			return;
		}
		NextCharacterTier = what.tier;
		GameObject gameObject = UnityEngine.Object.Instantiate(what.prefab, anyNavPoint.transform.position, Quaternion.identity) as GameObject;
		gameObject.SetActiveRecursively(false);
		gameObject.SetParent(Util.FindOrCreate("/Hell"));
		object value = null;
		KeyValuePair<string, object> key = new KeyValuePair<string, object>(what.prefab.name, value);
		if (!hellPool.ContainsKey(key))
		{
			hellPool[key] = new Queue<GameObject>();
		}
		hellPool[key].Enqueue(gameObject);
	}

	public GameObject GetFromHell(string name, object customData, Vector3? position)
	{
		KeyValuePair<string, object> key = new KeyValuePair<string, object>(name, customData);
		if (hellPool.ContainsKey(key) && hellPool[key].Count > 0)
		{
			GameObject gameObject = hellPool[key].Dequeue();
			if (position.HasValue)
			{
				gameObject.transform.position = position.Value;
			}
			gameObject.SetActiveRecursively(true);
			return gameObject;
		}
		return null;
	}

	public bool IsOverpopulated()
	{
		return _spawnedUnits.Count >= maxUnits;
	}

	public static void PreSetupCharacter(Character character)
	{
		int tier = 1;
		if (NextCharacterTier > 0)
		{
			tier = NextCharacterTier;
			NextCharacterTier = -1;
		}
		character.tier = tier;
	}

	public void PostSpawn(Character character)
	{
		_spawnedUnits.Add(character);
		character.RegisterOnDestroy(OnSpawnedUnitDestroyed);
		if (character is Unit)
		{
			Singleton.GameEvents.OnEnemySpawned(character as Unit);
		}
	}

	public GameObject Spawn(SpawnScenario source, SpawnPoint spawnPoint, SpawnInfo spawnInfo, Action preSpawn, Action<GameObject> postSpawn, bool chasing, bool summoned)
	{
		GameObject prefab = spawnInfo.prefab;
		log.Info(spawnPoint.gameObject, "trying to spawn '{0}'", prefab.name);
		if (spawnPoint == null)
		{
			return null;
		}
		if (IsOverpopulated())
		{
			return null;
		}
		SmartSpawnPoint smartSpawnPoint = spawnPoint as SmartSpawnPoint;
		if (smartSpawnPoint != null && smartSpawnPoint.IsVisibleSpawn() && !smartSpawnPoint.IsVisibleSpawnWorking())
		{
			return null;
		}
		if (preSpawn != null)
		{
			preSpawn();
		}
		NextCharacterTier = spawnInfo.tier;
		GameObject gameObject = spawnPoint.Spawn(prefab);
		if (!gameObject)
		{
			return null;
		}
		log.Info(spawnPoint.gameObject, "spawned '{0}'", prefab.name);
		PostSpawn(gameObject.GetComponent<Character>());
		if (postSpawn != null)
		{
			postSpawn(gameObject);
		}
		Unit component = gameObject.GetComponent<Unit>();
		if ((bool)component)
		{
			if (chasing && component is Zombie && !component.IsAppearing())
			{
				(component as Zombie).Chase();
			}
			if (summoned && component is Zombie)
			{
				(component as Zombie).WasSummoned();
			}
			if (source != null && (bool)component.GetComponent<KillBoss>())
			{
				component.gameObject.SetParent(source.gameObject.GetParent());
				component.SendMessageUpwards("OnSpawnedKillBoss", component.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		return gameObject;
	}

	public void OnSpawnedUnitDestroyed(Character character)
	{
		_spawnedUnits.Remove(character);
	}

	private void InitStatics()
	{
		if ((bool)Scene.Mission && !Scene.Mission.Config.spawnStatics)
		{
			return;
		}
		staticsTotal = Singleton.GameData.GetCurrentPlayerLevelWaveData().Statics;
		staticsLeft = staticsTotal;
		GameData.SceneData currentSceneData = Singleton.PlayerData.GetCurrentSceneData();
		bool isOutdoor = currentSceneData != null ? currentSceneData.IsOutdoor != 0 : false;
		float num = isOutdoor ? 3 : 1;
		float b = 8f * num;
		int num2 = 0;
		for (int i = 0; i < Scene.PlayerProgress.GetPath().Count; i++)
		{
			int unusedPathAttempt;
			for (int j = 0; j < 3; j++)
			{
				Vector3 vector = Scene.PlayerProgress.GetPath()[i];
				Vector3 vector2 = vector + Util.RandomInCircle() * Util.Range(0f, b);
				NavMeshHit hit;
				NavMesh.SamplePosition(vector2, out hit, 3f, -1);
				if (!hit.hit)
				{
					continue;
				}
				Vector3 vector3 = Util.ZeroY(hit.position - vector2);
				vector3 = vector3.magnitude > 0.001f ? vector3.normalized : Vector3.zero;
				vector2 = hit.position;
				bool flag = vector3 != Vector3.zero;
				if (Physics.OverlapSphere(vector2 + Vector3.up * 1f, 0.5f, 1 << Const.LayerDynamicObstacle).Length > 0)
				{
					continue;
				}
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				float unusedRayHeight;
				float unusedRayDistance;
				float unusedRayOffset;
				if (flag)
				{
					flag3 = Physics.Raycast(vector2 + Vector3.up * 0.5f, -vector3, 1f, 1 << Const.LayerDefault);
					flag4 = Physics.Raycast(vector2 + Vector3.up * 1.5f, -vector3, 1f, 1 << Const.LayerDefault);
					flag2 = flag3 || flag4;
				}
				bool flag5 = staticSpawns.Count != 0 ? (float)num2 / (float)staticSpawns.Count >= 0.2f : true;
				if ((flag && !flag2) || (flag2 && flag5))
				{
					for (int k = 0; k < 3; k++)
					{
						Vector3 vector4 = vector2 + vector3 * 0.1f + Quaternion.Euler(0f, Util.Range(-80f, 80f), 0f) * vector3 * Util.Range(0f, 3f);
						NavMesh.Raycast(vector2, vector4, out hit, -1);
						bool noWall = !hit.hit;
						if (noWall)
						{
							flag2 = false;
							vector2 = vector4;
							break;
						}
					}
				}
				bool flag6 = true;
				for (int l = 0; l < Mathf.Min(6, staticSpawns.Count); l++)
				{
					if (Vector3.Distance(vector2, staticSpawns[staticSpawns.Count - 1 - l].position) < 3f)
					{
						flag6 = false;
						break;
					}
				}
				if (!flag6)
				{
					continue;
				}
				float unusedSpawnSpacing;
				staticSpawns.Add(new StaticSpawn
				{
					position = vector2,
					nearestPathIndex = i,
					nearWall = !flag2 ? StaticSpawn.NearWall.No : (flag4 ? StaticSpawn.NearWall.Lean : StaticSpawn.NearWall.Sit),
					wallNormal = flag2 ? vector3 : Vector3.zero
				});
				if (flag2)
				{
					num2++;
				}
			}
		}
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("statics pool: {0} ({1})", staticsLeft, staticsTotal));
	}

	private void UpdateStatics()
	{
		if (staticSpawns.Count == 0 || !Util.DoRarelyReal(this, "update_statics", 2f))
		{
			return;
		}
		foreach (StaticSpawn staticSpawn in staticSpawns)
		{
			if (staticSpawn.zombie != null && (_spawnedUnits.Count >= maxUnits || (Vector3.Distance(staticSpawn.position, Scene.Player.transform.position) > 10f && staticSpawn.nearestPathIndex < Scene.PlayerProgress.CurrentPathIndex)) && !staticSpawn.zombie.IsAttacking(null) && Util.AnyObstacles(Scene.MainCamera.transform.position, staticSpawn.zombie.transform.position + Vector3.up * Scene.Player.Height() / 2f))
			{
				UnityEngine.Object.Destroy(staticSpawn.zombie.gameObject);
				staticSpawn.zombie = null;
				staticSpawn.used = false;
			}
		}
		List<StaticSpawn> list = new List<StaticSpawn>(staticSpawns);
		list.Shuffle();
		List<StaticSpawn> list2 = new List<StaticSpawn>();
		foreach (StaticSpawn item in list)
		{
			if (!((float)_spawnedUnits.Count < (float)maxUnits * 0.5f) || !((float)staticsLeft / (float)staticsTotal > Mathf.Clamp01(1f - ((float)Scene.PlayerProgress.CurrentPathIndex / (float)(Scene.PlayerProgress.GetPath().Count - 1) / 0.8f + 0.1f))))
			{
				break;
			}
			if (item.zombie == null && !item.used && Vector3.Distance(item.position, Scene.Player.transform.position).InSegment(10f, 20f) && item.nearestPathIndex > Scene.PlayerProgress.CurrentPathIndex && item.nearestPathIndex <= Scene.PlayerProgress.CurrentPathIndex + 10 && Util.AnyObstacles(Scene.MainCamera.transform.position, item.position + Vector3.up * Scene.Player.Height() / 2f))
			{
				staticsLeft--;
				list2.Add(item);
			}
		}
		foreach (StaticSpawn item2 in list2)
		{
			SpawnInfo spawnInfo = CommonOrUncommonZombie()();
			NextCharacterTier = spawnInfo.tier;
			GameObject gameObject = UnityEngine.Object.Instantiate(spawnInfo.prefab, item2.position, Quaternion.Euler(0f, Util.Range(0f, 360f), 0f)) as GameObject;
			gameObject.transform.parent = Scene.Groups.Statics;
			Zombie component = gameObject.GetComponent<Zombie>();
			float unusedSpawnValue;
			if ((bool)gameObject.GetComponent<Zombie>() && item2.nearWall != StaticSpawn.NearWall.No)
			{
				if (Util.Chance(50f) && item2.nearWall == StaticSpawn.NearWall.Lean)
				{
					component.Lean();
					component.transform.forward = -item2.wallNormal;
				}
				else
				{
					component.Sit();
					component.transform.forward = item2.wallNormal;
				}
			}
			item2.zombie = gameObject.GetComponent<Zombie>();
			item2.used = true;
			PostSpawn(item2.zombie);
		}
	}

	private void Update()
	{
		UpdateStatics();
		_spawnPointsDataUpdated = false;
	}

	private void UpdateSpawnPointsData()
	{
		if (_spawnPointsDataUpdated)
		{
			return;
		}
		if (_spawnPoints == null)
		{
			return;
		}
		foreach (SmartSpawnPoint smartSpawnPoint in _spawnPoints)
		{
			smartSpawnPoint.UpdateData(Scene.Player);
		}
		_spawnPointsDataUpdated = true;
	}

	public SmartSpawnPoint FindSpawnPoint(PointDesc description)
	{
		float unusedSpawnDistance;
		UpdateSpawnPointsData();
		List<KeyValuePair<float, SmartSpawnPoint>> list = new List<KeyValuePair<float, SmartSpawnPoint>>();
		for (int i = 0; i < 5; i++)
		{
			list.Clear();
			foreach (SmartSpawnPoint smartSpawnPoint in _spawnPoints)
			{
				float pathDistance;
				if (!description.Check(smartSpawnPoint, (float)i * 10f, out pathDistance))
				{
					continue;
				}
				list.Add(new KeyValuePair<float, SmartSpawnPoint>(pathDistance, smartSpawnPoint));
			}
			if (list.Count > 0)
			{
				break;
			}
			if (!description.MaxRadiusIsValid())
			{
				break;
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort((KeyValuePair<float, SmartSpawnPoint> a, KeyValuePair<float, SmartSpawnPoint> b) => (int)Mathf.Sign(a.Key - b.Key));
		int value = ((description.FarthestRank >= 0) ? (list.Count - description.FarthestRank) : 0);
		int num = description.NearestRank >= 0 ? description.NearestRank : list.Count;
		value = Mathf.Clamp(value, 0, num);
		num = Mathf.Clamp(num, value, list.Count);
		return list[UnityEngine.Random.Range(value, num)].Value;
	}

	public void UpdateZombieSettings()
	{
		foreach (Zombie zombie in UnityEngine.Object.FindSceneObjectsOfType(typeof(Zombie)) as Zombie[])
		{
			zombie.gameObject.SetActiveRecursively(Singleton.Settings.ZombieUpdate);
			foreach (Animation animation in zombie.gameObject.GetComponentsInChildren<Animation>())
			{
				animation.enabled = Singleton.Settings.ZombieAnimations;
			}
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in zombie.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (Singleton.Settings.ZombieSkinQuality >= 0)
				{
					skinnedMeshRenderer.quality = (SkinQuality)Singleton.Settings.ZombieSkinQuality;
				}
				skinnedMeshRenderer.enabled = Singleton.Settings.ZombieSkinQuality >= 0;
			}
			foreach (Renderer renderer in zombie.gameObject.GetComponentsInChildren<Renderer>())
			{
				renderer.enabled = Singleton.Settings.ZombieRender;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		foreach (StaticSpawn staticSpawn in staticSpawns)
		{
			bool flag = staticSpawn.nearWall == StaticSpawn.NearWall.Sit;
			bool flag2 = staticSpawn.nearWall == StaticSpawn.NearWall.Lean;
			float unusedGizmoSize;
			Gizmos.color = flag2 ? Color.yellow : (flag ? new Color(1f, 0.5f, 0f) : Color.white);
			Gizmos.DrawSphere(staticSpawn.position, 0.2f);
			Util.LabelInEditor(
				staticSpawn.position + Vector3.up * 0.2f * 2f,
				(flag2 ? "*" : (flag ? "**" : string.Empty)) +
				(staticSpawn.zombie ? "!" : (staticSpawn.used ? "+" : ".")));
			UnityEngine.Debug.DrawLine(staticSpawn.position, staticSpawn.position + staticSpawn.wallNormal);
		}
	}
}
