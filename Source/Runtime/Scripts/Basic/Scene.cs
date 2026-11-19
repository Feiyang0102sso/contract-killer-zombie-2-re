using System;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviourAndLog
{
	public class SceneGroups
	{
		public Transform Ragdolls;

		public Transform Spawned;

		public Transform Statics;

		public Transform Projectiles;

		public SceneGroups()
		{
			Ragdolls = new GameObject("Ragdolls").transform;
			Spawned = new GameObject("Spawned").transform;
			Statics = new GameObject("Statics").transform;
			Projectiles = new GameObject("Projectiles").transform;
		}
	}

	private static Scene instance = null;

	private Spawner _spawner;

	private PlayerProgress _playerProgress;

	private EffectsManager _effectsManager;

	private RagdollManager _ragdollManager;

	private MusicManager _musicManager;

	private SoundManager _soundManager;

	private ProjectileManager _projectileManager;

	private Mission _mission;

	private MiniZombieCamera _camera;

	private Camera _mainCamera;

	private Player _player;

	private SceneGroups _groups;

	public static HashSet<Character> characters = new HashSet<Character>();

	public static HashSet<Zombie> zombies = new HashSet<Zombie>();

	public static HashSet<Unit> enemies = new HashSet<Unit>();

	public static HashSet<Special> specials = new HashSet<Special>();

	public static HashSet<Human> humans = new HashSet<Human>();

	public static HashSet<Survivor> survivors = new HashSet<Survivor>();

	public static bool Exists()
	{
		return instance != null;
	}

	private void Awake()
	{
		LogTo("Game.Scene");
		instance = this;
		_spawner = gameObject.AddComponent<Spawner>();
		_playerProgress = gameObject.AddComponent<PlayerProgress>();
		_soundManager = Util.CreateGameObject("SoundManager").GetComponent<SoundManager>();
		_soundManager.transform.parent = transform;
		_effectsManager = Util.CreateGameObject(!(!Singleton.Quality.HitEffectsOnScene) ? "EffectsManager" : "EffectsManagerLowQuality").GetComponent<EffectsManager>();
		_effectsManager.transform.parent = transform;
		_ragdollManager = Util.CreateGameObject("RagdollManager").GetComponent<RagdollManager>();
		_ragdollManager.transform.parent = transform;
		_musicManager = Util.CreateGameObject("MusicManagerLowQuality").GetComponent<MusicManager>();
		_musicManager.transform.parent = transform;
		_projectileManager = Util.CreateGameObject("ProjectileManager").GetComponent<ProjectileManager>();
		_projectileManager.transform.parent = transform;
		_groups = new SceneGroups();
	}

	public void Init()
	{
		_camera = UnityEngine.Camera.main.GetComponent<MiniZombieCamera>();
		_mainCamera = _camera.GetComponent<Camera>();
		if (Util.BuildTag() != "NOT_TAGGED")
		{
			Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("build: {0}", Util.BuildTag()));
		}
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("level: {0} ({1}), time: {2}", Singleton.PlayerData.Level, Mission.GetEffectivePlayersLevel(), Util.IsoTime()));
		Singleton.FpsCounter.debugInfo += (FpsCounter.DebugInfoProc)(() => string.Format("map: {0}, pos: {1}", Application.loadedLevelName, Player ? Player.transform.position.ToString("0") : "-"));
		_mission = Util.FindObjectOfType<Mission>();
		if ((bool)_mission)
		{
			_mission.Init();
			_playerProgress.Init();
		}
		_spawner.Init();
		_projectileManager.Init();
		_player = Util.FindObjectOfType<Player>();
		OptimizeSceneForQuality();
		Preload();
	}

	private void OptimizeSceneForQuality()
	{
		foreach (UnityEngine.Object obj in UnityEngine.Object.FindSceneObjectsOfType(typeof(Light)))
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	private void Preload()
	{
		Character.PreLoad();
		Singleton.Cache.PerPrefabCache.PreLoad(Singleton.Cache.PerResourceCache.Load("CharInfo"), 16, 10);
		Spawner.PreLoadCharacters();
	}

	public static Scene inst
	{
		get
		{
			return instance;
		}
	}

	public static Spawner Spawner
	{
		get
		{
			return instance._spawner;
		}
	}

	public static PlayerProgress PlayerProgress
	{
		get
		{
			return instance._playerProgress;
		}
	}

	public static EffectsManager EffectsManager
	{
		get
		{
			return instance._effectsManager;
		}
	}

	public static RagdollManager RagdollManager
	{
		get
		{
			return instance._ragdollManager;
		}
	}

	public static ProjectileManager ProjectileManager
	{
		get
		{
			return instance._projectileManager;
		}
	}

	public static Mission Mission
	{
		get
		{
			return instance._mission;
		}
	}

	public static Player Player
	{
		get
		{
			return instance._player;
		}
	}

	public static MiniZombieCamera Camera
	{
		get
		{
			return instance._camera;
		}
	}

	public static Camera MainCamera
	{
		get
		{
			return instance._mainCamera;
		}
	}

	public static SceneGroups Groups
	{
		get
		{
			return instance._groups;
		}
	}

	public static void AddChar(Character character)
	{
		AddRemoveChar(character, true);
	}

	public static void RemoveChar(Character character)
	{
		AddRemoveChar(character, false);
	}

	private static void AddRemoveChar(Character character, bool add)
	{
		if (add)
		{
			characters.Add(character);
		}
		else
		{
			characters.Remove(character);
		}
		Zombie zombie = character as Zombie;
		if ((bool)zombie)
		{
			if (add)
			{
				zombies.Add(zombie);
			}
			else
			{
				zombies.Remove(zombie);
			}
		}
		Unit unit = character as Unit;
		if ((bool)unit)
		{
			if (add)
			{
				enemies.Add(unit);
			}
			else
			{
				enemies.Remove(unit);
			}
		}
		Special special = character as Special;
		if ((bool)special)
		{
			if (add)
			{
				specials.Add(special);
			}
			else
			{
				specials.Remove(special);
			}
		}
		Human human = character as Human;
		if ((bool)human)
		{
			if (add)
			{
				humans.Add(human);
			}
			else
			{
				humans.Remove(human);
			}
		}
		Survivor survivor = character as Survivor;
		if ((bool)survivor)
		{
			if (add)
			{
				survivors.Add(survivor);
			}
			else
			{
				survivors.Remove(survivor);
			}
		}
	}

	public static bool FilterChar(Character c, bool excludeDead)
	{
		return c.enabled && c.gameObject.active && (excludeDead ? !c.IsDying() : true);
	}

	private static HashSet<T> FilterChars<T>(HashSet<T> from) where T : Character
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T item in from)
		{
			if (FilterChar(item, true))
			{
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	private static HashSet<T> FilterChars<T>(HashSet<T> from, Func<T, bool> filter) where T : Character
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T item in from)
		{
			if (FilterChar(item, true) && filter(item))
			{
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	public static HashSet<Character> Chars
	{
		get
		{
			return FilterChars(characters);
		}
	}

	public static HashSet<Character> CharsAll
	{
		get
		{
			return characters;
		}
	}

	public static HashSet<Zombie> Zombies
	{
		get
		{
			return FilterChars(zombies);
		}
	}

	public static HashSet<Zombie> AttackingZombies
	{
		get
		{
			return FilterChars(zombies, (Zombie c) => c.IsAttacking(null));
		}
	}

	public static HashSet<Unit> Enemies
	{
		get
		{
			return FilterChars(enemies);
		}
	}

	public static HashSet<Unit> AttackingEnemies
	{
		get
		{
			return FilterChars(enemies, (Unit c) => (c is Zombie && (c as Zombie).IsAttacking(null)) || c is Special);
		}
	}

	public static HashSet<Special> Specials
	{
		get
		{
			return FilterChars(specials);
		}
	}

	public static HashSet<Human> Humans
	{
		get
		{
			return FilterChars(humans);
		}
	}

	public static HashSet<Survivor> Survivors
	{
		get
		{
			return FilterChars(survivors);
		}
	}

	public static void FindNearby(Vector3 center, float distance, int layerMask, Func<GameObject, bool> action)
	{
		foreach (Collider collider in Physics.OverlapSphere(center, distance, layerMask))
		{
			if (!action(collider.gameObject))
			{
				return;
			}
		}
	}

	public static List<T> Nearby<T>(Vector3 center, float distance, int layerMask) where T : Component
	{
		List<T> nearby = new List<T>();
		FindNearby(center, distance, layerMask, (GameObject g) =>
		{
			T component = g.GetComponent<T>();
			if ((bool)component)
			{
				nearby.Add(component);
			}
			return true;
		});
		return nearby;
	}

	public static T NearbyAny<T>(Vector3 center, float distance, int layerMask) where T : Component
	{
		T result = (T)null;
		FindNearby(center, distance, layerMask, (GameObject g) =>
		{
			T component = g.GetComponent<T>();
			if ((bool)component)
			{
				result = component;
				return false;
			}
			return true;
		});
		return result;
	}

	public static List<C> Nearby<C>(Vector3 center, float distance) where C : Character
	{
		List<C> nearby = new List<C>();
		FindNearby(center, distance, 1 << Const.LayerHitable, (GameObject g) =>
		{
			C component = g.GetComponent<C>();
			if ((bool)component && FilterChar(component, true))
			{
				nearby.Add(component);
			}
			return true;
		});
		return nearby;
	}

	public static C NearbyAny<C>(Vector3 center, float distance) where C : Character
	{
		C result = (C)null;
		FindNearby(center, distance, 1 << Const.LayerHitable, (GameObject g) =>
		{
			C component = g.GetComponent<C>();
			if ((bool)component && FilterChar(component, true))
			{
				result = component;
				return false;
			}
			return true;
		});
		return result;
	}

	private void OnDestroy()
	{
		instance = null;
	}
}

public class Formatter : Logging.IFormatter
{
	public string Format(ref Logging.LogRecord record)
	{
		string arg = Logging.SimpleFormatter.FormatMessage(record.Format, record.Args);
		string text = Logging.GetLevelName(record.Level);
		if (record.Level <= 30)
		{
			text = text.ToLower();
		}
		string arg2 = (!(!string.IsNullOrEmpty(record.Name)) ? "Root" : record.Name);
		return string.Format("{0}: ", text) + string.Format("{0} ({1})", arg, arg2);
	}
}
