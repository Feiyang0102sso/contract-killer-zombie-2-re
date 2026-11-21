using System;
using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

public class Character : MonoBehaviourAndLog
{
	public enum State
	{
		None,
		Appearing,
		Moving,
		Reloading,
		Pain,
		Melee,
		Throwing,
		Dying,
		Dead,
		Staying,
		Sitting,
		StandingUp,
		Leaning,
		Wandering,
		Chasing,
		Running,
		Summoning,
		Preparing,
		Exploding,
		Using,
		Feared
	}

	protected enum ShowHealth
	{
		Usual,
		Never,
		Always
	}

	public enum InputFrom
	{
		None,
		User,
		Net,
		Bot
	}

	protected enum CharStatus
	{
		None,
		Summoning,
		WasSummoned
	}

	public delegate void OnStateChanged(State fromState, State toState);

	private const float pushObjectForce = 800f;

	private const float defaultFade = 0.3f;

	public GameObject RagdollPrefab;

	public float mass = 70f;

	public float navigationRadiusIncrement;

	protected GameObject targetToAttack;

	public string[] PainSound;

	public string[] DeathSound;

	protected State state;

	protected State nextState;

	protected State previousState;

	protected float nextStateDelay;

	protected float timeSpentInState;

	protected int updatesDoneInState;

	protected float stateProgress;

	protected float previousStateProgress;

	public float healthTotal = 500f;

	protected float health;

	protected float showHealthTime;

	protected ShowHealth showHealth;

	protected bool updateCharInfoPosition = true;

	protected bool affectedByGravity = true;

	protected float speed;

	protected Vector3 direction;

	protected InputFrom sourceOfInput = InputFrom.User;

	private Action<Character> notifyOnDestroy = delegate
	{
	};

	protected CharInfo _charInfo;

	private CharStatus charStatus;

	protected CountdownTimer statusTimer;

	private bool _hiddenForOneFrame;

	private bool _hidden;

	private GameObject _shadow;

	protected CharacterController characterController;

	private CountdownTimer outlineTimer;

	protected bool needRagdoll;

	private bool radgollDelayed;

	private DamageInfo lastDamageInfo;

	private Dictionary<string, Texture2D> textureMapping = new Dictionary<string, Texture2D>();

	private bool _hitSoundsOnCharacter;

	private List<GameObject> shadowOccluders = new List<GameObject>();

	[NonSerialized]
	public int tier;

	private GameObject charInfo;

	private SkinnedMeshRenderer skinRenderer;

	private List<GameObject> piercedProjectiles = new List<GameObject>();

	private List<Transform> piercingControlles = new List<Transform>();

	protected bool _localHitEffects = true;

	private static Dictionary<string, Dictionary<string, float>> variousTextures = new Dictionary<string, Dictionary<string, float>>
	{
		{
			"zombie_1",
			new Dictionary<string, float>
			{
				{ "1", 2f },
				{ "2", 1f },
				{ "3", 1f }
			}
		},
		{
			"zombie_2",
			new Dictionary<string, float>
			{
				{ "1", 2f },
				{ "2", 1f },
				{ "3", 1f }
			}
		},
		{
			"zombie_3",
			new Dictionary<string, float>
			{
				{ "1", 2f },
				{ "2", 1f },
				{ "3", 0.5f }
			}
		},
		{
			"zombie_4",
			new Dictionary<string, float>
			{
				{ "1", 1f },
				{ "2", 1f }
			}
		},
		{
			"zombie_5",
			new Dictionary<string, float>
			{
				{ "1", 1f },
				{ "2", 1f }
			}
		},
		{
			"uncommon_1",
			new Dictionary<string, float> { { "1", 1f } }
		},
		{
			"uncommon_2",
			new Dictionary<string, float> { { "1", 1f } }
		},
		{
			"survivor_1",
			new Dictionary<string, float>
			{
				{ "1", 1f },
				{ "2", 1f }
			}
		},
		{
			"survivor_2a",
			new Dictionary<string, float> { { "1", 1f } }
		},
		{
			"survivor_2b",
			new Dictionary<string, float> { { "1", 1f } }
		}
	};

	private static string[] variousTexturesPostfixes = new string[3]
	{
		string.Empty,
		"_body",
		"_head"
	};

	private static HashSet<string> variousTexturesSync = new HashSet<string> { "zombie_1", "zombie_2" };

	private Queue<DamageInfo> delayedDamages = new Queue<DamageInfo>();

	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = Mathf.Min(value, healthTotal);
		}
	}

	public float Speed
	{
		get
		{
			return speed;
		}
		set
		{
			speed = value;
		}
	}

	public Vector3 Direction
	{
		get
		{
			return direction;
		}
		set
		{
			direction = value;
		}
	}

	public void SetSourceOfInput(InputFrom source)
	{
		sourceOfInput = source;
	}

	private event OnStateChanged _onStateChanged = delegate
	{
	};

	public void RegisterOnDestroy(Action<Character> func)
	{
		notifyOnDestroy += func;
	}

	public void RegisterOnStateChanged(OnStateChanged onStateChanged)
	{
		_onStateChanged += onStateChanged;
	}

	public Vector3 Center()
	{
		return transform.position + characterController.center;
	}

	public Vector3 PartialHeight(float progressToTop)
	{
		return transform.position + Vector3.up * characterController.height * progressToTop;
	}

	public Vector3 Eyes()
	{
		return PartialHeight(0.85f);
	}

	public virtual float Height()
	{
		return characterController.height;
	}

	public float Radius()
	{
		return characterController.radius + navigationRadiusIncrement;
	}

	public bool LocalHitEffects()
	{
		return _localHitEffects;
	}

	public bool Hidden
	{
		get
		{
			return _hidden;
		}
		set
		{
			if (_hidden != value)
			{
				_hidden = value;
				Util.SetGameObjectVisible(gameObject, !_hidden, new HashSet<string>(new string[1] { "CharInfo" }));
			}
		}
	}

	protected void ShowBurningEyes(bool flag)
	{
		GameObject gameObject = base.gameObject.FindChildInHierarchy("RightEye");
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Renderer>().enabled = flag;
		}
		gameObject = base.gameObject.FindChildInHierarchy("LeftEye");
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Renderer>().enabled = flag;
		}
	}

	protected virtual void Awake()
	{
		LogTo("Char.Base");
		InitPiercingControllers();
		transform.position = transform.position + Vector3.up * 0.001f;
		Spawner.PreSetupCharacter(this);
		if (gameObject.name.EndsWith("(Clone)"))
		{
			gameObject.RemoveClonePostfix();
			KeyValuePair<string, int> key = new KeyValuePair<string, int>(gameObject.name, tier);
			if (Singleton.GameData.charConfMap.ContainsKey(key))
			{
				Util.ModifyValues(gameObject, Singleton.GameData.charConfMap[key]);
			}
		}
		else
		{
			tier = 0;
		}
		ShowBurningEyes(tier > 1);
		characterController = GetComponent<CharacterController>();
		if (!characterController)
		{
			characterController = GetComponentInChildren<CharacterController>();
		}
		PcPhysics.MatchLegacyController(characterController);
		health = healthTotal;
		Scene.AddChar(this);
		_charInfo = Singleton.Cache.PerPrefabCache.Instantiate(Singleton.Cache.PerResourceCache.Load("CharInfo"), 16).GetComponent<CharInfo>();
		_charInfo.gameObject.RemoveClonePostfix();
		_charInfo.transform.parent = gameObject.transform;
		SetHealthBarColor();
		_charInfo.HealthBar.Hide(true);
		_charInfo.StatusIcon.Hide(true);
		_charInfo.BasicText.Hide(true);
		_charInfo.QuickText.Hide(true);
		SortedList<int, SkinnedMeshRenderer> sortedList = new SortedList<int, SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			int num = 0;
			GameObject parent = skinnedMeshRenderer.gameObject;
			while (parent != gameObject && parent != null)
			{
				num++;
				parent = parent.GetParent();
			}
			sortedList.Add(num, skinnedMeshRenderer);
		}
		skinRenderer = sortedList.Count >= 1 ? Util.GetFirst(sortedList).Value : null;
		gameObject.RemoveClonePostfix();
		_hitSoundsOnCharacter = Singleton.Quality.HitSoundsOnScene || this is Human;
		if (_hitSoundsOnCharacter && Singleton.Quality.PrecacheSoundEffects)
		{
			foreach (string soundName in PainSound)
			{
				SoundManager.PreloadInBackground(soundName);
			}
			foreach (string soundName2 in DeathSound)
			{
				SoundManager.PreloadInBackground(soundName2);
			}
		}
		if (Singleton.Quality.RagdollsOnScene && (bool)RagdollPrefab)
		{
			Scene.RagdollManager.Preinstance(RagdollPrefab);
		}
	}

	protected virtual void Start()
	{
		if (state != State.Appearing)
		{
			_shadow = CreateShadow(gameObject);
		}
		VariousTextures();
	}

	protected virtual void OnDestroy()
	{
		if (Singleton.Exists() && (bool)_charInfo)
		{
			Singleton.Cache.PerPrefabCache.Destroy(_charInfo.gameObject);
		}
		Scene.RemoveChar(this);
		notifyOnDestroy(this);
	}

	private void ReportUpwards(string method, object arg)
	{
		if (transform.parent != null)
		{
			SendMessageUpwards(method, arg, SendMessageOptions.DontRequireReceiver);
		}
		else if (Scene.Mission != null)
		{
			Scene.Mission.SendMessage(method, arg, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected void ChangeState(State newState)
	{
		ChangeState(newState, State.None, 0f);
	}

	protected virtual void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		if (state == State.Appearing && !_shadow)
		{
			_shadow = CreateShadow(gameObject);
		}
		previousState = state;
		previousStateProgress = stateProgress;
		state = newState;
		nextState = nextState_;
		nextStateDelay = nextStateDelay_;
		timeSpentInState = 0f;
		updatesDoneInState = 0;
		stateProgress = 0f;
		_onStateChanged(previousState, newState);
		Animate(true);
		if (nextState == State.None || nextStateDelay == 0f)
		{
		}
		if (newState == State.Dying)
		{
			ReportUpwards("OnCharacterDying", gameObject);
			if (!(this is Human))
			{
				bool flag = RagdollPrefab != null && Singleton.Quality.RagdollsOnScene && Scene.RagdollManager.CanSpawnRagdoll;
				bool flag2 = lastDamageInfo != null && lastDamageInfo.mutilated;
				if (flag)
				{
					needRagdoll = true;
					if (needRagdoll)
					{
						CreateRagdoll(lastDamageInfo);
					}
					else
					{
						radgollDelayed = true;
					}
				}
				else if (flag2)
				{
					Mutilate(lastDamageInfo);
				}
			}
		}
		else if (newState == State.Dead)
		{
			ReportUpwards("OnCharacterDead", gameObject);
			if (!(this is Human))
			{
				RagdollManager.instance.ManageCharacter(this);
			}
		}
	}

	protected virtual void proceedToNextState()
	{
		ChangeState(nextState);
	}

	protected virtual void OnActivate()
	{
		if (state != State.None)
		{
			Animate(true);
		}
	}

	protected virtual void Update()
	{
		float unusedLocal1;
		RaycastHit hitInfo;
		if (radgollDelayed && stateProgress >= 0.33f)
		{
			CreateRagdoll(null);
		}
		if (state == State.Dying && !radgollDelayed && (bool)skinRenderer && !skinRenderer.isVisible && !(this is Human))
		{
			GameObject gameObject = Util.FindObjectWithTagInHierarchy(base.gameObject, "HipBone");
			if ((bool)gameObject && Physics.Raycast(gameObject.transform.position + Vector3.up * 0.1f, Vector3.down, out hitInfo, 2f, Const.LayerMaskObstacles))
			{
				EffectsManager.instance.OnRagdollSleep(hitInfo.point + hitInfo.normal * 0.01f);
			}
			ChangeState(State.Dead);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		timeSpentInState += Time.deltaTime;
		updatesDoneInState++;
		if (nextState != State.None)
		{
			stateProgress = nextStateDelay == 0f ? 1f : Mathf.Clamp01(timeSpentInState / nextStateDelay);
			if (timeSpentInState >= nextStateDelay)
			{
				proceedToNextState();
			}
		}
		Animate(false);
		UpdateMovement();
		if (updateCharInfoPosition)
		{
			_charInfo.transform.localPosition = new Vector3(0f, Height() * 1.15f, 0f);
			Util.RotateToCamera(_charInfo.transform);
		}
		showHealthTime -= Time.deltaTime;
		float unusedLocal2;
		bool showHealthBar = showHealth == ShowHealth.Always ? true :
			showHealth == ShowHealth.Never ? false : showHealthTime > 0f && !(this is Player);
		if (showHealthBar)
		{
			if (!_charInfo.HealthBar.GetComponent<Renderer>().enabled)
			{
				_charInfo.HealthBar.Hide(false);
			}
			_charInfo.HealthBar.Value = health / healthTotal;
			float y = 200f / (Scene.MainCamera.WorldToScreenPoint(_charInfo.transform.position) - Scene.MainCamera.WorldToScreenPoint(_charInfo.transform.position + Scene.MainCamera.transform.right)).magnitude;
			_charInfo.HealthBarParent.transform.localScale = new Vector3(Radius() * 2f * 1f / _charInfo.HealthBar.width, y, 1f);
		}
		else
		{
			if (_charInfo.HealthBar.GetComponent<Renderer>().enabled)
			{
				_charInfo.HealthBar.Hide(true);
			}
			showHealthTime = 0f;
		}
		bool showStatusIcon = statusTimer != null && statusTimer.IsRunning() && !IsDying();
		if (showStatusIcon)
		{
			if (!_charInfo.StatusIcon.GetComponent<Renderer>().enabled)
			{
				_charInfo.StatusIcon.Hide(false);
			}
			float progress = statusTimer.Progress();
			_charInfo.StatusIcon.SetColor(new Color(1f, 1f, 1f,
				progress < 0.1f ? progress / 0.1f :
				progress > 0.9f ? (1f - progress) / 0.1f :
				Mathf.Clamp01(1.2f + Mathf.Cos(Time.time % 0.5f / 0.5f * 2f * (float)Math.PI))));
		}
		else if (_charInfo.StatusIcon.GetComponent<Renderer>().enabled)
		{
			_charInfo.StatusIcon.Hide(true);
		}
		if (outlineTimer != null && (!outlineTimer.IsRunning() || IsDying()))
		{
			Singleton.Visual.HideOutline(base.gameObject);
			outlineTimer = null;
		}
		if (_hiddenForOneFrame)
		{
			_hiddenForOneFrame = false;
			Hidden = false;
		}
	}

	protected virtual void LateUpdate()
	{
		MaintainPiercedProjectiles();
	}

	protected virtual void UpdateMovement()
	{
		UnityEngine.Debug.DrawLine(Center(), Center() + speed * direction);
		Vector3 vector = speed * direction * Time.deltaTime;
		if (affectedByGravity)
		{
			vector += Physics.gravity * Time.deltaTime;
		}
		Vector3 position = transform.position;
		bool useCharacterController = true;
		if (useCharacterController)
		{
			PcPhysics.LegacyMove(characterController, vector, transform);
		}
		else
		{
			vector = Util.ZeroY(vector);
			transform.position += Util.ZeroY(vector);
			UnityEngine.AI.NavMeshAgent component = GetComponent<UnityEngine.AI.NavMeshAgent>();
			component.Move(Util.ZeroY(vector));
			component.speed = 0f;
			component.updatePosition = true;
		}
		speed = (transform.position - position).magnitude / Time.deltaTime;
		direction = (transform.position - position).normalized;
	}

	private GameObject CreateShadow(GameObject biped)
	{
		GameObject gameObject = null;
		if (shadowOccluders.Count == 0)
		{
			GameObject item = this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone") ?? this.FindCustomChild((GameObject n) => n.name.ToLower().EndsWith("hip_bone"));
			GameObject item2 = this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01R_thigh_Bone/Bip01R_leg_Bone/Bip01R_foot_Bone") ?? this.FindCustomChild((GameObject n) => n.name.ToLower().EndsWith("l_leg_bone"));
			GameObject item3 = this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01L_thigh_Bone/Bip01L_leg_Bone/Bip01L_foot_Bone") ?? this.FindCustomChild((GameObject n) => n.name.ToLower().EndsWith("r_leg_bone"));
			shadowOccluders.Add(item);
			shadowOccluders.Add(item2);
			shadowOccluders.Add(item3);
		}
		foreach (GameObject shadowOccluder in shadowOccluders)
		{
			if (!shadowOccluder)
			{
				return null;
			}
		}
		GameObject gameObject2 = (Singleton.Quality.ComplexShadows ? Resources.Load("Shadow") : Resources.Load("SimpleShadow")) as GameObject;
		if ((bool)gameObject2)
		{
			gameObject = UnityEngine.Object.Instantiate(gameObject2) as GameObject;
			gameObject.transform.position = biped.transform.position;
			gameObject.transform.position += Vector3.up * 0.05f;
			gameObject.transform.eulerAngles = biped.transform.eulerAngles;
			gameObject.transform.Rotate(gameObject.transform.right, -90f, Space.World);
			gameObject.transform.parent = biped.transform;
			if (Singleton.Quality.ComplexShadows)
			{
				OcclusionController component = gameObject.GetComponent<OcclusionController>();
				float unusedLocal1;
				float unusedLocal2;
				for (int num = 0; num < 3; num++)
				{
					component.AddOccluder(shadowOccluders[num].transform, num == 0 ? 0.45f : 0.2f);
				}
			}
		}
		return gameObject;
	}

	private void ReuseShadow(GameObject biped, GameObject shadow)
	{
		shadow.transform.parent = null;
		shadow.transform.position = biped.transform.position;
		shadow.transform.eulerAngles = biped.transform.eulerAngles;
		shadow.transform.Rotate(shadow.transform.right, -90f, Space.World);
		shadow.transform.parent = biped.transform;
		OcclusionController component = shadow.GetComponent<OcclusionController>();
		if ((bool)component)
		{
			component.Cleanup();
			component.SetAdaptiveMode(true);
			float unusedShadowValue1;
			float unusedShadowValue2;
			GameObject gameObject = biped.FindCustomChild((GameObject n) => n.name.EndsWith("hip_Bone"));
			component.AddOccluder(gameObject.transform, 0.5f);
			gameObject = biped.FindCustomChild((GameObject n) => n.name.EndsWith("L_leg_Bone"));
			component.AddOccluder(gameObject.transform, 0.25f);
			gameObject = biped.FindCustomChild((GameObject n) => n.name.EndsWith("R_leg_Bone"));
			component.AddOccluder(gameObject.transform, 0.25f);
		}
	}

	protected string PlayAnimationAndAddTime(params string[] animations)
	{
		return PlayAnimationAndAddTime(1f, animations);
	}

	protected string PlayAnimationAndAddTime(float speed, params string[] animations)
	{
		return PlayAnimationAndAddTime(-1f, speed, animations);
	}

	protected string PlayAnimationAndAddTime(float fadeDuration, float speed, params string[] animations)
	{
		return PlayAnimationAndAddTime(false, fadeDuration, speed, animations);
	}

	protected string PlayAnimationAndAddTime(bool queued, float fadeDuration, float speed, params string[] animations)
	{
		if (fadeDuration < 0f)
		{
			fadeDuration = 0.3f;
		}
		string text = PlayAnimation(queued, fadeDuration, speed, animations);
		if (text == string.Empty)
		{
			return string.Empty;
		}
		if (sourceOfInput != InputFrom.Net)
		{
			float num = Mathf.Abs(GetComponent<Animation>()[text].length / GetComponent<Animation>()[text].speed);
			nextStateDelay += num - (queued ? Mathf.Min(fadeDuration, num) : 0f);
		}
		return text;
	}

	protected string PlayAnimation(params string[] animations)
	{
		return PlayAnimation(1f, animations);
	}

	protected string PlayAnimation(float speed, params string[] animations)
	{
		return PlayAnimation(-1f, speed, animations);
	}

	protected string PlayAnimation(float fadeDuration, float speed, params string[] animations)
	{
		return PlayAnimation(false, fadeDuration, speed, animations);
	}

	protected string PlayAnimation(bool queued, float fadeDuration, float speed, params string[] animations)
	{
		if (fadeDuration < 0f)
		{
			fadeDuration = 0.3f;
		}
		string result = Util.RandomFrom(animations);
		if (!GetComponent<Animation>()[result])
		{
			return string.Empty;
		}
		GetComponent<Animation>()[result].speed = speed;
		if (!queued && GetComponent<Animation>()[result].weight > 0f)
		{
			GetComponent<Animation>().Rewind(result);
		}
		if (fadeDuration != 0f)
		{
			if (!queued)
			{
				GetComponent<Animation>().CrossFade(result, fadeDuration);
			}
			else
			{
				GetComponent<Animation>().CrossFadeQueued(result, fadeDuration);
			}
		}
		else if (!queued)
		{
			GetComponent<Animation>().Play(result);
		}
		else
		{
			GetComponent<Animation>().PlayQueued(result);
		}
		return result;
	}

	protected string PlayAnimationQueued(params string[] animations)
	{
		return PlayAnimation(true, -1f, 1f, animations);
	}

	protected string PlayAnimationQueuedAndAddTime(params string[] animations)
	{
		return PlayAnimationAndAddTime(true, -1f, 1f, animations);
	}

	protected virtual void Animate(bool firstRun)
	{
		if (firstRun)
		{
		}
	}

	public void Outline(float duration, Color color)
	{
		if (outlineTimer != null)
		{
			Singleton.Visual.HideOutline(gameObject);
		}
		Singleton.Visual.ShowOutline(gameObject, color, 0);
		outlineTimer = new CountdownTimer(duration);
	}

	protected void SetHealthBarColor()
	{
		SetHealthBarColor(false);
	}

	protected void SetHealthBarColor(bool weak)
	{
		Color color = weak ? new Color(0.82f, 0.1f, 0.7f) :
			GetComponent<Special>() ? new Color(0.95f, 0.25f, 0.1f) :
			GetComponent<Unit>() ? new Color(0.92f, 0.1f, 0.1f) :
			GetComponent<Human>() ? new Color(0.2f, 0.75f, 1f) :
			new Color(1f, 0.79f, 0.01f);
		_charInfo.HealthBar.SetColor(color);
	}

	protected void SetCharStatus(CharStatus status, float duration)
	{
		charStatus = status;
		_charInfo.StatusIcon.SetTexture(Resources.Load(charStatus.ToString()) as Texture2D);
		statusTimer = new CountdownTimer(duration);
	}

	public static void PreLoad()
	{
		if (!Singleton.Quality.NewZombies)
		{
			return;
		}
		foreach (KeyValuePair<string, Dictionary<string, float>> variousTexture in variousTextures)
		{
			if (!variousTexture.Key.ToLower().StartsWith("zombie") && !variousTexture.Key.ToLower().StartsWith("uncommon"))
			{
				continue;
			}
			foreach (KeyValuePair<string, float> item in variousTexture.Value)
			{
				string text = variousTexture.Key + "_" + item.Key;
				bool flag = false;
				foreach (string text2 in variousTexturesPostfixes)
				{
					string text3 = text + text2;
					Texture2D texture2D = Resources.Load(text3) as Texture2D;
					if (texture2D != null)
					{
						TexturesReplacer.GetTexture(text3);
						string path = text3 + "_nm";
						Resources.Load(path);
						TexturesReplacer.GetTexture(path);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
				}
			}
		}
	}

	private void VariousTextures()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
		{
			Material material = renderer.material;
			if (!material || !material.mainTexture)
			{
				continue;
			}
			Texture mainTexture = material.mainTexture;
			string str = mainTexture.name;
			string text = null;
			foreach (string text2 in variousTexturesPostfixes)
			{
				if (str.EndsWith(text2))
				{
					Util.RemovePostfix(ref str, text2);
					text = text2;
					break;
				}
			}
			if (text == null)
			{
				continue;
			}
			Util.TryRemovePostfix(ref str, "_1");
			if (!variousTextures.ContainsKey(str))
			{
				continue;
			}
			string text3;
			if (dictionary.ContainsKey(str))
			{
				text3 = dictionary[str] + text;
			}
			else
			{
				string newStart = str + "_" + Util.RandomByWeights(variousTextures[str]);
				text3 = newStart + text;
				if (variousTexturesSync.Where((string s) => newStart.StartsWith(s)).ToList().Count > 0)
				{
					dictionary[str] = newStart;
				}
			}
			Texture2D texture = TexturesReplacer.GetTexture(text3);
			if (!texture)
			{
				continue;
			}
			textureMapping[material.name + ":" + mainTexture.name] = texture;
			material.SetTexture("_MainTex", texture);
			if (material.shader.name.ToLower().Contains("bump"))
			{
				Texture2D texture2 = TexturesReplacer.GetTexture(text3 + "_nm");
				if (!texture2)
				{
				}
				material.SetTexture("_BumpMap", texture2);
			}
		}
	}

	protected void ReplaceTextures(GameObject obj)
	{
		foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
		{
			Material material = renderer.material;
			if (!((bool)material && (bool)material.mainTexture))
			{
				continue;
			}
			string key = material.name + ":" + material.mainTexture.name;
			if (textureMapping.ContainsKey(key))
			{
				material.mainTexture = textureMapping[key];
			}
		}
	}

	protected void SynchronizeHierarchy(Transform src, Transform dst)
	{
		dst.position = src.position;
		dst.eulerAngles = src.eulerAngles;
		int childCount = src.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = src.GetChild(i);
			if ((bool)child)
			{
				Transform transform = dst.Find(child.name);
				if ((bool)transform && transform.name == child.name)
				{
					SynchronizeHierarchy(child, transform);
				}
			}
		}
	}

	protected virtual void OnCreateRagdoll(GameObject ragdoll)
	{
	}

	public virtual bool CanBeHurt()
	{
		return state != State.Dying && state != State.Dead;
	}

	protected virtual float ModifyDamage(float damage, DamageInfo info)
	{
		DamageBooster component;
		if (info.source is Human && (bool)(component = info.source.GetComponent<DamageBooster>()))
		{
			damage *= component.conf.damageBoost;
		}
		if (this is Survivor && info.source is Special)
		{
			damage *= Singleton.GameData.DamageModifierFromSpecialToSurvivor;
		}
		if (this is Human && info.source is Human)
		{
			damage = Mathf.Min(1f, damage);
		}
		if (this is Gate && info.source is Special)
		{
			damage *= Singleton.GameData.DamageModifierFromSpecialToGates;
		}
		return damage;
	}

	public bool IsDying()
	{
		return state == State.Dying || state == State.Dead;
	}

	public bool IsDyingOrAppearing()
	{
		return IsDying() || state == State.Appearing;
	}

	public bool IsDead()
	{
		return state == State.Dead;
	}

	public bool IsAppearing()
	{
		return state == State.Appearing;
	}

	public bool IsMeleeing()
	{
		return state == State.Melee;
	}

	public virtual void OnHeadshot(DamageInfo info)
	{
	}

	public virtual void OnDamage(DamageInfo info)
	{
		OnDamage(info, 0f);
	}

	public virtual void OnDamage(DamageInfo info, float delay)
	{
		if (delay != 0f && !(this is Human))
		{
			delayedDamages.Enqueue(info);
			Invoke("DelayedDamage", delay);
			return;
		}
		if (!CanBeHurt())
		{
			return;
		}
		if (info.damage != float.PositiveInfinity)
		{
			info.actualDamage = ModifyDamage(info.damage, info);
			health -= info.actualDamage;
		}
		else
		{
			health = 0f;
		}
		if (health > 0f)
		{
			OnHurt(info);
		}
		else
		{
			OnDying(info);
		}
	}

	private void DelayedDamage()
	{
		OnDamage(delayedDamages.Dequeue());
	}

	protected virtual void Report(string text)
	{
		if ((bool)_charInfo.BasicText)
		{
			Report(_charInfo.BasicText.gameObject, text);
		}
	}

	protected void ReportDamage(string text)
	{
		if ((bool)_charInfo.QuickText)
		{
			Report(_charInfo.QuickText.gameObject, text);
		}
	}

	private void Report(GameObject obj, string text)
	{
		float unusedLocal1;
		// PC port: the original compared the GameObject with the SpriteText component (never equal), so the
		// 2 s limit never applied and two quick reports drew overlapping speech bubbles
		if (obj == _charInfo.BasicText.gameObject && !Util.DoRarely(this, "report_delay", 2f))
		{
			return;
		}
		SpriteText component = (UnityEngine.Object.Instantiate(obj) as GameObject).GetComponent<SpriteText>();
		component.transform.parent = obj.transform.parent;
		component.transform.localPosition = obj.transform.localPosition + (this is Player ? new Vector3(0f, -0.3f, 0f) : Vector3.zero);
		component.Text = text;
		component.gameObject.AddComponent<MiniScript>().Charge(2f, null, null, (MiniScript script) =>
		{
			Util.RotateToCamera(script.transform);
		}, (MiniScript script) =>
		{
			UnityEngine.Object.Destroy(script.gameObject);
		});
		Util.RotateToCamera(component.transform);
		component.Hide(false);
		component.GetComponent<Renderer>().enabled = true;
		GUIUtils.RunAllAnimations(component.gameObject);
	}

	protected void Mutilate(DamageInfo info)
	{
		if ((bool)Scene.EffectsManager)
		{
			Scene.EffectsManager.DrawBodyblow(transform.position);
		}
		SoundManager.Play("skill_shot_2", 2000, false);
		if (this is Unit)
		{
			if (info is ExplosionInfo)
			{
				Consumable consumable = (info as ExplosionInfo).consumable;
				Weapon arg = consumable ? null :
					info.source is Human ? (info.source as Human).Inventory.Weapon : null;
				Singleton.GameEvents.OnEnemyMutilated(info.source, this as Unit, arg, consumable);
			}
			else
			{
				Weapon arg2 = info.source is Human ? (info.source as Human).Inventory.Weapon : null;
				Singleton.GameEvents.OnEnemyMutilated(info.source, this as Unit, arg2, null);
			}
		}
		ChangeState(State.Dead);
		UnityEngine.Object.Destroy(gameObject);
	}

	protected void CreateRagdoll(DamageInfo info)
	{
		GameObject gameObject = Scene.RagdollManager.Instantiate(RagdollPrefab);
		gameObject.transform.position = base.gameObject.transform.position;
		gameObject.transform.rotation = base.gameObject.transform.rotation;
		gameObject.transform.parent = Scene.Groups.Ragdolls;
		Vector3 vector = Vector3.zero;
		UnityEngine.AI.NavMeshAgent component = GetComponent<UnityEngine.AI.NavMeshAgent>();
		if ((bool)component)
		{
			vector = component.velocity;
		}
		if (vector.magnitude > 2f)
		{
			float unusedLocal1;
			float unusedLocal2;
			Rigidbody[] componentsInChildren = gameObject.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody in componentsInChildren)
			{
				rigidbody.linearVelocity = vector * 0.75f;
			}
		}
		if (!(this is Bomber))
		{
			SynchronizeHierarchy(transform, gameObject.transform);
		}
		SynchronizePiercedProjectiles(gameObject);
		OnCreateRagdoll(gameObject);
		if ((bool)_shadow)
		{
			ReuseShadow(gameObject, _shadow);
		}
		if (info != null)
		{
			int num = 0;
			if (info is HitInfo)
			{
				num = DamageAnimator(this, gameObject, info as HitInfo, vector.magnitude > 2f);
			}
			else if (info is ExplosionInfo)
			{
				num = ExplosionAnimator(this, gameObject, info as ExplosionInfo);
			}
			if (num > 0 && this is Unit)
			{
				Singleton.GameEvents.OnEnemyDismemberment(info.source, this as Unit, info.source is Human ? (info.source as Human).Inventory.Weapon : null, null, true);
			}
		}
		else
		{
			SuicideAnimator(gameObject);
		}
		ChangeState(State.Dead);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private static int DamageAnimator(Character character, GameObject ragdoll, HitInfo info, bool deriveVelocity)
	{
		int num = 0;
		Weapon weapon = null;
		if (info.source is Human)
		{
			weapon = (info.source as Human).Inventory.Weapon;
		}
		float percent = 0f;
		if ((bool)weapon)
		{
			if ((bool)weapon.IsMeleeWeapon() || (bool)weapon.IsLauncher())
			{
				percent = 100f;
			}
			else if ((bool)weapon.IsFirearm())
			{
				switch (weapon.IsFirearm().Ammunition)
				{
				case Weapon.AmmoType.SniperRifleCartridge:
				case Weapon.AmmoType.ShotgunShell:
				case Weapon.AmmoType.Bolt:
					percent = 100f;
					break;
				case Weapon.AmmoType.AssaultRifleCartridge:
				case Weapon.AmmoType.MGCartridge:
					percent = 50f;
					break;
				default:
					percent = 25f;
					break;
				}
			}
		}
		if (deriveVelocity)
		{
			percent = 0f;
		}
		if (Util.Chance(percent))
		{
			Vector3 vector = -info.momentum;
			vector.y = 0f;
			vector.Normalize();
			vector.y = 1f;
			vector.Normalize();
			float unusedLocal1;
			float unusedLocal2;
			GameObject gameObject = ragdoll.FindChildInHierarchy("Bip01L_thigh_Bone");
			GameObject gameObject2 = ragdoll.FindChildInHierarchy("Bip01R_thigh_Bone");
			GameObject gameObject3 = ragdoll.FindChildInHierarchy("Bip01L_leg_Bone");
			GameObject gameObject4 = ragdoll.FindChildInHierarchy("Bip01R_leg_Bone");
			if ((bool)gameObject)
			{
				gameObject.GetComponent<Rigidbody>().linearVelocity = 2f * vector;
			}
			if ((bool)gameObject2)
			{
				gameObject2.GetComponent<Rigidbody>().linearVelocity = 2f * vector;
			}
			if ((bool)gameObject3)
			{
				gameObject3.GetComponent<Rigidbody>().linearVelocity = 4f * vector;
			}
			if ((bool)gameObject4)
			{
				gameObject4.GetComponent<Rigidbody>().linearVelocity = 4f * vector;
			}
		}
		else if (deriveVelocity)
		{
			GameObject gameObject5 = ragdoll.FindChildInHierarchy("Bip01L_leg_Bone");
			GameObject gameObject6 = ragdoll.FindChildInHierarchy("Bip01R_leg_Bone");
			if ((bool)gameObject5)
			{
				gameObject5.GetComponent<Rigidbody>().linearDamping = UnityEngine.Random.Range(2f, 3f);
			}
			if ((bool)gameObject6)
			{
				gameObject6.GetComponent<Rigidbody>().linearDamping = UnityEngine.Random.Range(2f, 3f);
			}
		}
		else
		{
			GameObject gameObject7 = ragdoll.FindChildInHierarchy("Bip01L_leg_Bone");
			GameObject gameObject8 = ragdoll.FindChildInHierarchy("Bip01R_leg_Bone");
			if ((bool)gameObject7)
			{
				gameObject7.GetComponent<Rigidbody>().linearDamping = UnityEngine.Random.Range(2f, 3f);
			}
			if ((bool)gameObject8)
			{
				gameObject8.GetComponent<Rigidbody>().linearDamping = UnityEngine.Random.Range(2f, 3f);
			}
			CharacterJoint characterJoint = gameObject7 ? gameObject7.GetComponent<CharacterJoint>() : null;
			CharacterJoint characterJoint2 = gameObject8 ? gameObject8.GetComponent<CharacterJoint>() : null;
			CharacterJoint characterJoint3 = Util.Chance(33f) ? characterJoint : Util.Chance(50f) ? characterJoint2 : null;
			if ((bool)characterJoint3)
			{
				int num2 = Util.Chance(50f) ? -90 : -45;
				SoftJointLimit lowTwistLimit = characterJoint3.lowTwistLimit;
				lowTwistLimit.limit = num2;
				characterJoint3.lowTwistLimit = lowTwistLimit;
			}
		}
		Collider[] array = Physics.OverlapSphere(info.hitPoint, 0.5f, 1 << Const.LayerRagdoll);
		float num3 = float.MaxValue;
		Collider collider = null;
		foreach (Collider collider2 in array)
		{
			if (!collider2.GetComponent<Rigidbody>())
			{
				continue;
			}
			Vector3 position = collider2.gameObject.transform.position;
			float sqrMagnitude = (info.hitPoint - position).sqrMagnitude;
			if (sqrMagnitude < num3)
			{
				num3 = sqrMagnitude;
				collider = collider2;
			}
		}
		if ((bool)collider)
		{
			if (!collider.GetComponent<Rigidbody>().isKinematic)
			{
				collider.GetComponent<Rigidbody>().linearVelocity += info.momentum / collider.GetComponent<Rigidbody>().mass;
			}
			CharacterJoint component = collider.gameObject.GetComponent<CharacterJoint>();
			if ((bool)component)
			{
				component.connectedBody.linearVelocity += info.momentum / component.connectedBody.mass;
			}
			if (info.mutilated)
			{
				Dismemberment component2 = ragdoll.GetComponent<Dismemberment>();
				if ((bool)component2)
				{
					component2.Dismember(component2.SpineBone);
					if (character is Unit)
					{
						Weapon arg = info.source is Human ? (info.source as Human).Inventory.Weapon : null;
						Singleton.GameEvents.OnEnemyMutilated(info.source, character as Unit, arg, null);
					}
				}
			}
			else if (Util.Chance(info.defragChance))
			{
				Dismemberment component3 = ragdoll.GetComponent<Dismemberment>();
				if ((bool)component3)
				{
					component3.Dismember(collider.GetComponent<Rigidbody>().gameObject);
					num++;
				}
			}
		}
		float unusedLocal3;
		float distanceToEvent = Scene.Exists() && Scene.Camera ? (character.transform.position - Scene.Camera.transform.position).magnitude : 9999.9f;
		Singleton.TimeControl.ThisIsSparta(distanceToEvent);
		return num;
	}

	private static int ExplosionAnimator(Character character, GameObject ragdoll, ExplosionInfo info)
	{
		int num = 0;
		Rigidbody[] componentsInChildren = ragdoll.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			Vector3 vector = rigidbody.GetComponent<Collider>().bounds.center - info.epicenter;
			Vector3 normalized = vector.normalized;
			float num2 = Mathf.Clamp(vector.magnitude / info.radius, 0f, 1f);
			num2 = 1f - num2;
			num2 = Mathf.Pow(num2, 0.25f);
			num2 = num2 * 0.25f + num2 * Vector3.Dot(normalized, Vector3.up) * 0.75f;
			if (!rigidbody.isKinematic)
			{
				rigidbody.linearVelocity = normalized * (info.momentum.magnitude * num2 / rigidbody.mass);
			}
			num2 = Mathf.Clamp(vector.magnitude / info.radius, 0f, 1f);
			num2 = Mathf.Pow(num2, 0.33f);
			float unusedLocal1;
			float unusedLocal2;
			float unusedLocal3;
			float unusedLocal4;
			float num3 = 1f * (1f - num2) + 0.01f * num2;
			if (UnityEngine.Random.Range(0f, 1f) <= num3 && (bool)ragdoll.GetComponent<Dismemberment>())
			{
				ragdoll.GetComponent<Dismemberment>().DismemberInstantly(rigidbody.gameObject);
				num++;
			}
		}
		float unusedLocal5;
		float distanceToEvent = Scene.Exists() && Scene.Camera ? (character.transform.position - Scene.Camera.transform.position).magnitude : 9999.9f;
		Singleton.TimeControl.ThisIsSparta(distanceToEvent);
		return num;
	}

	private static void SuicideAnimator(GameObject ragdoll)
	{
		foreach (Rigidbody rigidbody in ragdoll.GetComponentsInChildren<Rigidbody>())
		{
			if (!rigidbody.isKinematic)
			{
				rigidbody.linearVelocity = Vector3.up * Util.Range(0.5f, 0.7f) + Util.RandomInCircle() * Util.Range(0.1f, 0.2f);
			}
		}
	}

	protected virtual bool CanBePained(DamageInfo info)
	{
		return state != State.Appearing && state != State.Pain;
	}

	protected virtual bool CanBeStunned(DamageInfo info)
	{
		return info.stunned && CanBePained(info);
	}

	protected virtual void OnHurt(DamageInfo info)
	{
		float unusedLocal1;
		if (info.actualDamage > 0f)
		{
			showHealthTime = 3f;
		}
		if (_hitSoundsOnCharacter && PainSound != null && PainSound.Length > 0 && Util.DoRarely(this, "CHAR_PAIN", 1f))
		{
			SoundManager.PlayExclusive(PainSound[UnityEngine.Random.Range(0, PainSound.Length)], transform.position, 5f, 25f, false);
		}
		if (CanBeStunned(info))
		{
			ChangeState(State.Pain, state, 0f);
		}
		if (this is Unit)
		{
			Singleton.GameEvents.OnEnemyHit(info.source, this as Unit, info.source is Human ? (info.source as Human).Inventory.Weapon : null, null, info.damage);
		}
	}

	protected virtual void OnDying(DamageInfo info)
	{
		showHealthTime = 0f;
		if (_hitSoundsOnCharacter && DeathSound != null && DeathSound.Length > 0)
		{
			SoundManager.Play(DeathSound[UnityEngine.Random.Range(0, DeathSound.Length)], transform.position, 5f, 25f, false);
		}
		if (info != null)
		{
			if (this is Unit)
			{
				if (info is ExplosionInfo)
				{
					Consumable consumable = (info as ExplosionInfo).consumable;
					Weapon arg = consumable ? null :
						info.source is Human ? (info.source as Human).Inventory.Weapon : null;
					Singleton.GameEvents.OnEnemyHit(info.source, this as Unit, arg, consumable, info.damage);
					Singleton.GameEvents.OnEnemyKill(info.source, this as Unit, arg, consumable);
				}
				else
				{
					Weapon arg2 = info.source is Human ? (info.source as Human).Inventory.Weapon : null;
					Singleton.GameEvents.OnEnemyHit(info.source, this as Unit, arg2, null, info.damage);
					Singleton.GameEvents.OnEnemyKill(info.source, this as Unit, arg2, null);
				}
				if (info.hitZone == DamageInfo.HitZone.Head)
				{
					OnHeadshot(info);
					if (this is Unit)
					{
						Singleton.GameEvents.OnEnemyHeadshot(info.source, this as Unit, info.source is Human ? (info.source as Human).Inventory.Weapon : null, null);
					}
					if ((bool)Singleton.UI.Hud)
					{
						Singleton.UI.Hud.ShowNotifier(Strings.GetString("Headshot!"));
					}
				}
			}
			if (Util.Chance(info.defragChance) || Util.Chance(info.ragdollChance) || info.mutilated)
			{
				needRagdoll = true;
			}
		}
		lastDamageInfo = info;
		ChangeState(State.Dying, State.Dead, 0f);
	}

	protected virtual void OnDismember(GameObject limb)
	{
		ReplaceTextures(limb);
	}

	protected void RotateTo(Vector3 point, float angularSpeed)
	{
		Vector3 forward = Util.ZeroY(point - transform.position);
		bool useAngularSpeed = true;
		if (!useAngularSpeed)
		{
			transform.rotation = Quaternion.LookRotation(forward);
		}
		else
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(forward), Time.deltaTime * angularSpeed);
		}
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (!(bool)attachedRigidbody || attachedRigidbody.isKinematic)
		{
			return;
		}
		if (hit.moveDirection.y < -0.3f)
		{
			return;
		}
		Vector3 force = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
		force *= 800f;
		// PC port: called every frame while touching, and the forces add up until the next physics step, so the push grew
		// with the frame rate. Scale to the force one original 60 fps frame gave; unscaled time keeps bullet time as
		// strong as on iOS too (there the physics step also collected more frames when slowed down).
		force *= PcFrameRate.OriginalFrames(Time.unscaledDeltaTime);
		attachedRigidbody.AddForce(force);
	}

	public void HideForOneFrame()
	{
		_hiddenForOneFrame = true;
		Hidden = true;
	}

	protected virtual void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		string text = string.Format("{0}{1}: {2}", GetType().Name, tier != 1 ? "_t" + tier : string.Empty, state.ToString());
		if (nextStateDelay > 0f)
		{
			text += string.Format(" ({0:00}%)", 100f * stateProgress);
		}
		if (this is Bot)
		{
			text += string.Format(", {0}, {1}", (this as Bot).Mode_, (this as Bot).Status_);
		}
		if (this is Unit && (this as Unit).Obstacled)
		{
			text += string.Format(" *");
		}
		Util.LabelInEditor(transform.position + transform.up * Height(), text);
	}

	protected float GetIntermediateRange(GameObject target)
	{
		return (target.transform.position - transform.position).magnitude - Radius() - (target.GetComponent<Character>() ? target.GetComponent<Character>().Radius() : (!(!target.GetComponent<Renderer>()) ? target.GetComponent<Renderer>().bounds.extents.magnitude : 0.1f));
	}

	private void InitPiercingControllers()
	{
		string unusedLocal1;
		if (this is Survivor)
		{
			return;
		}
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if ((bool)transform.parent && transform.name != "Point01")
			{
				piercingControlles.Add(transform);
			}
		}
	}

	private void MaintainPiercedProjectiles()
	{
		foreach (GameObject piercedProjectile in piercedProjectiles)
		{
			Vector3 lossyScale = piercedProjectile.transform.parent.lossyScale;
			Vector3 localScale = lossyScale;
			localScale.x = 1f / localScale.x;
			localScale.y = 1f / localScale.y;
			localScale.z = 1f / localScale.z;
			piercedProjectile.transform.localScale = localScale;
		}
	}

	private void SynchronizePiercedProjectiles(GameObject ragdoll)
	{
		MaintainPiercedProjectiles();
		foreach (GameObject piercedProjectile in piercedProjectiles)
		{
			GameObject gameObject = ragdoll.FindChildInHierarchy(piercedProjectile.transform.parent.name);
			if ((bool)gameObject)
			{
				piercedProjectile.transform.parent = gameObject.transform;
			}
		}
		piercedProjectiles.Clear();
	}

	public bool Pierce(GameObject projectile, Vector3 hitPoint)
	{
		Transform transform = null;
		foreach (Transform piercingControlle in piercingControlles)
		{
			if (!transform || (transform.position - hitPoint).sqrMagnitude > (piercingControlle.position - hitPoint).sqrMagnitude)
			{
				transform = piercingControlle;
			}
		}
		if ((bool)transform)
		{
			projectile.transform.position = transform.position;
			projectile.transform.parent = transform;
			piercedProjectiles.Add(projectile);
			return true;
		}
		return false;
	}
}
