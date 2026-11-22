using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class Bot : Human
{
	public enum Mode
	{
		Idle,
		MakeMission,
		GotoGate,
		FollowCharacter
	}

	public enum Status
	{
		Staying,
		Moving,
		Engaging,
		EngagingAndMoving
	}

	private const float inIdleRotateEvery = 8f;

	public float keepRange = 3f;

	public float keepRangeWhileShooting = 6f;

	public float moveCloserThreshold = 1.3f;

	public float moveFartherThreshold = 1.1f;

	public float shootingRange = 15f;

	public float rotationSpeed = 270f;

	public float aimingRotationSpeed = 120f;

	public float timeBeforeRotation = 3f;

	public float ChanceToMelee = 50f;

	private bool AiEnabled = true;

	protected Mode mode;

	private Status status;

	private float lastMovementTime;

	private float lastEngagementTime;

	private Character target;

	private Vector3 point;

	private Unit enemy;

	private ActivateItem activateItem;

	private float lastRangeToActivateItem;

	private PlayerData.Slots shootingWeapon = (PlayerData.Slots)(-1);

	public Mode Mode_
	{
		get
		{
			return mode;
		}
	}

	public bool IsFollowing()
	{
		return mode == Mode.FollowCharacter;
	}

	public Status Status_
	{
		get
		{
			return status;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Inventory.AddConsumable("GrenadeBeacon", 40, false);
		Inventory.AddConsumable("MedkitLarge", 100, false);
		LogTo("Char.Bot");
	}

	protected override void Start()
	{
		base.Start();
		target = Scene.Player;
	}

	private bool TargetSuitable(Unit unit)
	{
		float a = shootingRange;
		if (Inventory.Weapon != null)
		{
			if (!(Inventory.Weapon is MeleeWeapon))
			{
				float maxAutofireDistance = Inventory.Weapon.MaxAutofireDistance;
				if (!float.IsInfinity(maxAutofireDistance))
				{
					a = maxAutofireDistance;
				}
			}
			else
			{
				a = Radius() + unit.Radius() + meleeAttackDistance;
			}
		}
		a = Mathf.Min(a, shootingRange);
		if (Vector3.Distance(unit.Center(), Center()) > a)
		{
			return false;
		}
		Vector3 vector = unit.Eyes() - Eyes();
		RaycastHit[] array = Physics.SphereCastAll(Eyes(), 0.2f, vector, vector.magnitude, Const.LayerMaskObstacles | Const.LayerMaskAllHitable);
		foreach (RaycastHit raycastHit in array)
		{
			GameObject gameObject = raycastHit.collider.gameObject;
			if (!(!gameObject.GetComponent<Gate>()))
			{
				continue;
			}
			if (((1 << gameObject.layer) & Const.LayerMaskObstacles) != 0)
			{
				return false;
			}
			Character character = gameObject.FindComponentUpwards<Character>();
			if (character.gameObject != base.gameObject && character.gameObject != unit.gameObject)
			{
				return false;
			}
		}
		return true;
	}

	private void Reset()
	{
		speed = 0f;
		wantToShoot = false;
		isShooting = false;
	}

	protected override void Update()
	{
		if (state == State.Moving || state == State.Reloading || state == State.Melee)
		{
			UpdateAi();
		}
		else if (state == State.Using)
		{
			transform.rotation = Util.OnlyY(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(activateItem.transform.position - transform.position), Time.deltaTime * rotationSpeed));
		}
		base.Update();
	}

	private void UpdateAi()
	{
		if (!AiEnabled)
		{
			return;
		}
		bool flag = false;
		Vector3 position = transform.position;
		if (mode == Mode.Idle)
		{
			status = Status.Staying;
		}
		else if (mode == Mode.MakeMission)
		{
			status = Status.Moving;
			Task currentTask;
			GameObject gameObject;
			if (!Scene.Mission || !(currentTask = Scene.Mission.GetCurrentTask()) || !(gameObject = currentTask.GetTarget()))
			{
			}
			else
			{
				position = gameObject.transform.position;
			}
		}
		else if (mode == Mode.GotoGate)
		{
			GameObject gameObject2 = Util.FindObjectOfType<Gate>().gameObject;
			if (!gameObject2)
			{
			}
			else
			{
				position = gameObject2.transform.position;
				status = Status.Moving;
				if (GetIntermediateRange(gameObject2) < 1f)
				{
					Object.Destroy(base.gameObject);
					return;
				}
			}
		}
		else if (mode == Mode.FollowCharacter)
		{
			if ((bool)activateItem)
			{
				position = activateItem.transform.position;
				status = Status.Moving;
				if (!Scene.NearbyAny<Unit>(Center(), Radius() + meleeAttackDistance * 1.5f))
				{
					float unusedLocal1;
					float num = Radius() * 1.1f + 0.3f;
					float magnitude = Util.ZeroY(transform.position - activateItem.transform.position).magnitude;
					if (magnitude < num || (magnitude < num + 2f &&
						((lastRangeToActivateItem >= 0f ? magnitude >= lastRangeToActivateItem : false) || speed < baseSpeed * 0.1f)))
					{
						speed = 0f;
						ChangeState(State.Using, State.Moving, 0f);
						return;
					}
					lastRangeToActivateItem = magnitude;
				}
				else
				{
					lastRangeToActivateItem = -1f;
				}
			}
			else
			{
				UnityEngine.Debug.DrawLine(target.transform.position, target.transform.position + Vector3.up, new Color(1f, 0f, 0f));
				float unusedLocal2;
				Vector3 vector = target.transform.position - transform.position;
				float num2 = Radius() + target.Radius() + 0.1f;
				bool flag2 = status == Status.Staying || status == Status.Engaging;
				float num3 = num2 * (flag2 ? 1f : moveFartherThreshold);
				if (vector.magnitude >= keepRange * ((status == Status.Staying || status == Status.Engaging) ? moveCloserThreshold : 1f))
				{
					status = Status.Moving;
					Vector3 vector2 = -vector.normalized * keepRange;
					position = target.transform.position + vector2;
				}
				else if (vector.magnitude < num3)
				{
					flag = true;
					status = Status.Moving;
					position = target.transform.position - vector.normalized * num3;
				}
				else
				{
					status = Status.Staying;
				}
			}
		}
		else
		{
			return;
		}
		if (mode != Mode.Idle && (Util.DoRarely(this, "repath", 0.5f) || flag || Util.ZeroY(point - transform.position).magnitude < 0.1f))
		{
			UnityEngine.AI.NavMeshPath path;
			if (!Util.SamplePosition(ref position) || !Util.FindPath(transform.position, position, out path))
			{
				// PC port: start off the rebuilt NavMesh -> path from the nearest NavMesh point instead (see NewUnityNavCompat)
				if (!NewUnityNavCompat.FindPathFromNearestNavMesh(this, position, out path))
				{
					Reset();
					UnityEngine.Debug.DrawLine(transform.position, position, new Color(1f, 0.5f, 0f), 1f);
					return;
				}
			}
			for (int i = 0; i < path.corners.Length; i++)
			{
				point = path.corners[i];
				if (Util.ZeroY(point - transform.position).magnitude < 0.1f)
				{
					continue;
				}
				break;
			}
		}
		float unusedLocal3;
		float unusedLocal4;
		if (state != State.Reloading && Inventory.Weapon != null && Util.DoRarely(this, "change_weapon_check", 0.3f))
		{
			if (Inventory.MeleeWeapon != null && Inventory.Weapon != Inventory.MeleeWeapon && (bool)Scene.NearbyAny<Unit>(Center(), Radius() + meleeAttackDistance * 0.8f) && (Inventory.Weapon.IsAmmoEmpty() || Util.ChancePerTime(ChanceToMelee, this, "melee", 1f)))
			{
				shootingWeapon = Inventory.GetCurrentSlot();
				Inventory.SetCurrentSlot(PlayerData.Slots.melee);
				Util.DoRarely(this, "perform_melee", 0f);
			}
			else if (Inventory.Weapon == Inventory.MeleeWeapon && shootingWeapon != (PlayerData.Slots)(-1) && !Inventory.WeaponsBySlot[(int)shootingWeapon].IsAmmoEmpty() && !Inventory.MeleeWeapon.IsPerformingAttack && Util.DoRarely(this, "perform_melee", 1.5f))
			{
				Inventory.SetCurrentSlot(shootingWeapon);
			}
		}
		if (enemy != null && enemy.IsDying())
		{
			enemy = null;
		}
		if (Util.DoRarely(this, "find_new_enemy", 0.9f) || (Util.DoRarely(this, "check_for_obstacles", 0.3f) && (bool)enemy && !TargetSuitable(enemy)))
		{
			List<KeyValuePair<Unit, float>> list = new List<KeyValuePair<Unit, float>>();
			float distance = shootingRange * 1.2f;
			foreach (Unit item in Scene.Nearby<Unit>(Center(), distance))
			{
				if (item.CanBeHurt())
				{
					float value = Vector3.Distance(item.Center(), Center()) + 1f / 90f * Mathf.Abs(Vector3.Angle(transform.forward, item.Center() - Center()));
					list.Add(new KeyValuePair<Unit, float>(item, value));
				}
			}
			list.Sort((KeyValuePair<Unit, float> a, KeyValuePair<Unit, float> b) => (a.Value < b.Value) ? (-1) : ((a.Value > b.Value) ? 1 : 0));
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				Unit key = list[num4].Key;
				if (TargetSuitable(key))
				{
					enemy = key;
					break;
				}
			}
		}
		Reset();
		if (enemy != null && Inventory.Weapon != null)
		{
			Quaternion quaternion = Util.OnlyY(Quaternion.LookRotation(enemy.Center() - Eyes()));
			transform.rotation = Quaternion.RotateTowards(transform.rotation, quaternion, Time.deltaTime * aimingRotationSpeed);
			if (mode == Mode.Idle || (!flag && target != null && !(enemy is Special) && (target.transform.position - transform.position).magnitude <= keepRangeWhileShooting))
			{
				status = Status.Engaging;
			}
			else
			{
				status = Status.EngagingAndMoving;
			}
			if (state != State.Reloading)
			{
				wantToShoot = true;
				lastEngagementTime = Time.time;
				if (GetComponent<HumanAnimation>().IsReadyToShoot() &&
					((Inventory.Weapon != null && Inventory.Weapon == Inventory.MeleeWeapon) ? Vector3.Distance(Center(), enemy.Center()) < meleeAttackDistance : true) &&
					Quaternion.FromToRotation(transform.rotation * Vector3.forward, quaternion * Vector3.forward).eulerAngles.magnitude < new Vector3(10f, 10f, 10f).magnitude)
				{
					isShooting = true;
				}
				shootingTo = enemy.Eyes();
				UnityEngine.Debug.DrawLine(Center(), shootingTo, new Color(1f, 0f, 1f));
			}
		}
		if ((bool)enemy && Inventory.Weapon == null && Vector3.Distance(enemy.transform.position, transform.position) < 4f && Util.DoRarely(this, "report_fear", 20f))
		{
			Report(Util.RandomFrom<string>(Strings.GetString("Enemy attacking us!"), Strings.GetString("Please defend me!"), Strings.GetString("I need your help!"), Strings.GetString("Evil beasts incoming!"), Strings.GetString("Help, they are gonna eat me!")));
			speed = 0f;
			ChangeState(State.Feared, State.Moving, 0f);
			return;
		}
		float unusedLocal5;
		float unusedLocal6;
		if (status == Status.Moving || status == Status.EngagingAndMoving)
		{
			lastMovementTime = Time.time;
			speed = Mathf.Min(baseSpeed, (point - transform.position).magnitude / Time.deltaTime);
			UnityEngine.Debug.DrawLine(transform.position, point, new Color(0f, 0f, 0.5f));
			Vector3 vector3 = Util.ZeroY(point - transform.position);
			direction = vector3.normalized;
			if (status != Status.EngagingAndMoving && vector3 != Vector3.zero)
			{
				transform.rotation = Util.OnlyY(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(vector3), Time.deltaTime * rotationSpeed));
			}
			UnityEngine.Debug.DrawLine(transform.position, position, new Color(0f, 1f, 0f));
		}
		else if (status == Status.Staying)
		{
			float num5 = Time.time - Mathf.Max(lastMovementTime, lastEngagementTime);
			if (timeBeforeRotation >= 0f && num5 >= timeBeforeRotation)
			{
				float num6 = 1000f * Mathf.Abs(transform.position.x + transform.position.z);
				float num7 = 8f * (num6 % 1f + 0.5f);
				transform.rotation = Util.OnlyY(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, num6 * (float)Mathf.CeilToInt(Time.time / num7) * num7, 0f), Time.deltaTime * rotationSpeed));
			}
			if (mode == Mode.FollowCharacter && num5 >= 45f && Util.DoRarely(this, "talk", 45f))
			{
				Report(Util.RandomFrom<string>(Strings.GetString("Ok, lets move out"), Strings.GetString("Time to move"), Strings.GetString("We must go..."), Strings.GetString("Are we waiting for something specific?"), Strings.GetString("It is dangerous here..."), Strings.GetString("We have a little time, lets go!"), Strings.GetString("Creepy..."), Strings.GetString("I am scared"), Strings.GetString("All these zombies really piss me off")));
			}
		}
		float unusedLocal7;
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		if (newState == State.Pain)
		{
			speed = 0f;
		}
		if (newState == State.Using)
		{
			activateItem.BeginUse();
		}
		if (state == State.Using)
		{
			activateItem.Use();
			activateItem = null;
			GetComponent<Animation>().Stop();
			GetComponent<HumanWeapon>().ShowWeapon(true);
		}
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void OnHurt(DamageInfo info)
	{
		float unusedLocal1;
		if (Util.DoRarely(this, "on_hurt_text", 15f))
		{
			Report(Util.RandomFrom<string>(Strings.GetString("Uf!"), Strings.GetString("Aw!"), Strings.GetString("Aww!"), Strings.GetString("Ow!"), Strings.GetString("I am hit!"), Strings.GetString("It hurts!"), Strings.GetString("Help me!"), Strings.GetString("Help!")));
		}
		base.OnHurt(info);
	}

	protected override bool CanBeStunned(DamageInfo info)
	{
		return Inventory.Weapons.Count == 0 && state != State.Feared && base.CanBeStunned(info);
	}

	public void OnEnteredActivateItem(ActivateItem item)
	{
		if (activateItem != null)
		{
			return;
		}
		if (item.ActivateByNpc(this))
		{
			activateItem = item;
			lastRangeToActivateItem = -1f;
			Report(Util.RandomFrom<string>(Strings.GetString("Hold on for a second, I must activate this"), Strings.GetString("Wait a second..."), Strings.GetString("Protect me while I use this thing"), Strings.GetString("This will not take long...")));
		}
	}

	protected override void Report(string text)
	{
		if (!Singleton.Tutorial || Singleton.PlayerData.TutorialState >= 1)
		{
			base.Report(text);
		}
	}

	protected void ReportHud(string text)
	{
		Singleton.UI.Hud.ShowInfo(string.Format("{0}: {1}", Strings.GetString("Survivor"), text));
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Pain)
		{
			if (firstRun)
			{
				GetComponent<Animation>()["pain"].layer = 1;
				GetComponent<Animation>()["pain"].wrapMode = WrapMode.Once;
				PlayAnimationAndAddTime("pain");
			}
		}
		else if (state == State.Feared)
		{
			if (firstRun)
			{
				GetComponent<Animation>()["fear"].wrapMode = WrapMode.Once;
				PlayAnimationAndAddTime("fear");
			}
		}
		else if (state == State.Using)
		{
			if (firstRun)
			{
				GetComponent<HumanWeapon>().ShowWeapon(false);
				if (activateItem.usageDuration == 0f)
				{
					string text = "use_button";
					GetComponent<Animation>()[text].wrapMode = WrapMode.Once;
					PlayAnimationAndAddTime(text);
				}
				else
				{
					if (!activateItem.FindCustomChild((GameObject n) => n.name.ToLower().Contains("antenna")))
					{
						GetComponent<Animation>()["use_computer_start"].layer = 100;
						GetComponent<Animation>()["use_computer_start"].wrapMode = WrapMode.Once;
						GetComponent<Animation>()["use_computer"].layer = 100;
						GetComponent<Animation>()["use_computer"].wrapMode = WrapMode.Loop;
						PlayAnimationAndAddTime("use_computer_start");
						PlayAnimationQueued("use_computer");
					}
					else
					{
						GetComponent<Animation>()["use_radio"].layer = 100;
						GetComponent<Animation>()["use_radio"].wrapMode = WrapMode.Loop;
						PlayAnimationAndAddTime("use_radio");
					}
					nextStateDelay += activateItem.usageDuration;
				}
			}
		}
		else
		{
			base.Animate(firstRun);
		}
	}

	public float GetActivationProgress()
	{
		return activateItem ? stateProgress : (-1f);
	}

	public void EnableAi(bool enable)
	{
		Reset();
		AiEnabled = enable;
	}
}
