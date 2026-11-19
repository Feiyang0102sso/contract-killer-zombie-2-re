using System;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Unit
{
	private const float maxAngleToMelee = 30f;

	private const float minAngleToMelee = 5f;

	private const float interdictionOffsetMax = 3f;

	private const float flankingIntermediateCircle = 0.5f;

	private const float summonStatusDuration = 15f;

	private const float pushBackSpeed = 0.7692308f;

	public bool demoMode;

	public float meleeDistance = 0.5f;

	public float meleeDamage = 200f;

	public float meleeDuration = 1.2f;

	public float meleeDurationRandomness = 0.3f;

	public float meleeDamageIndent = 0.4f;

	public GameObject attackSpecificTarget;

	public float wanderingSpeedMod = 0.4f;

	public float runningSpeedMod = 3f;

	public float senseAroundRadius = 1.5f;

	public float sightFrontalMax = 20f;

	public float stopChasingRange = 20f;

	public float hearingRange = 10f;

	public float wanderRangeMin = 1f;

	public float wanderRangeMax = 5f;

	public float wanderAngleAlteration = 180f;

	public float spreadAgressionRange = 2f;

	public float painStunTime = 0.5f;

	public float interdictionAngleLimit = 80f;

	public float flanking = 50f;

	public float flankingShift = 10f;

	public float changeFlankingChancePerSec = 30f;

	public float crippledChance = 20f;

	public float crippledSpeedMod = 0.7f;

	public string[] ChaseSound;

	public bool reactToDamage = true;

	public bool canBeStunned = true;

	private Vector3 wanderingTo;

	private bool meleeDamageWasDealt;

	private float headPitch;

	private float headYaw;

	private int seedFlanking;

	private int seedMovement;

	private bool isCrippled;

	private float meleeDurationActual;

	private GameObject head;

	private Quaternion headRestoration;

	private Quaternion headRestorationInversed;

	private SmartSpawnPoint.Appearing appearingType;

	private GameObject applyPainTo;

	private bool isPushedBack;

	private Vector3 pushedBackEndPoint;

	private string animWalk;

	private string animRun;

	protected override void Awake()
	{
		base.Awake();
		seedFlanking = UnityEngine.Random.Range(0, int.MaxValue);
		seedMovement = UnityEngine.Random.Range(0, int.MaxValue);
		isCrippled = Util.Chance(crippledChance, seedMovement);
		speedBase = (isCrippled ? (speedBase * crippledSpeedMod) : speedBase);
		meleeDurationActual = Util.ApplyRandomness(meleeDuration, meleeDurationRandomness);
		head = gameObject.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01spine_Bone/Bip01chest_Bone/Bip01head_Bone");
		headRestoration = Quaternion.LookRotation(new Vector3(0f, -1f, 0f), new Vector3(-1f, 0f, 0f));
		headRestorationInversed = Quaternion.Inverse(headRestoration);
		InitAnimations();
		if (Singleton.Quality.HitSoundsOnScene && Singleton.Quality.PrecacheSoundEffects)
		{
			foreach (string soundName in ChaseSound)
			{
				SoundManager.PreloadInBackground(soundName);
			}
		}
		if (sourceOfInput == InputFrom.User)
		{
			if (attackSpecificTarget != null)
			{
				Chase(attackSpecificTarget);
			}
			else
			{
				Stay();
			}
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private Vector3 senseCenter()
	{
		return Center();
	}

	private float senseRadius()
	{
		return senseAroundRadius;
	}

	private Vector3 sightCenter()
	{
		return Center() + transform.forward * sightRadius();
	}

	private float sightRadius()
	{
		return sightFrontalMax / 2f;
	}

	private void Appear()
	{
		Appear(SmartSpawnPoint.Appearing.None);
	}

	public void Appear(SmartSpawnPoint.Appearing appearingType_)
	{
		appearingType = appearingType_;
		ChangeState(State.Appearing, State.Chasing, 0f);
	}

	public void Sit()
	{
		ChangeState(State.Sitting);
	}

	private void StandUp()
	{
		ChangeState(State.StandingUp, State.Chasing, 0f);
	}

	public void Lean()
	{
		ChangeState(State.Leaning);
	}

	private void Stay()
	{
		StopMovement();
		ChangeState(State.Staying, State.Wandering, Util.Range(1f, 5f));
	}

	private void Wander()
	{
		Wander(transform.position + UnityEngine.Random.Range(wanderRangeMin, wanderRangeMax) * Util.InCircle(wanderAngleAlteration * UnityEngine.Random.Range(-1f, 1f)));
	}

	private void Wander(Vector3 point)
	{
		UnityEngine.Debug.DrawLine(transform.position, point, new Color(0f, 1f, 0f), 1f);
		wanderingTo = point;
		ChangeState(State.Wandering, State.Staying, Util.Range(10f, 30f));
	}

	public void Chase()
	{
		Chase(targetToAttack != null ? targetToAttack : SelectTarget());
	}

	public void Chase(GameObject target)
	{
		targetToAttack = target;
		if (state == State.Sitting)
		{
			StandUp();
			return;
		}
		if (state == State.StandingUp && stateProgress < 0.9f)
		{
			return;
		}
		if (!targetToAttack)
		{
			Stay();
		}
		else
		{
			ChangeState(State.Chasing, State.Running, Util.Range(0.5f, 4f));
			if (Singleton.Quality.HitSoundsOnScene && ChaseSound != null && ChaseSound.Length > 0)
			{
				SoundManager.PlayExclusive(ChaseSound[UnityEngine.Random.Range(0, ChaseSound.Length)], transform.position, 5f, 25f, false);
			}
		}
	}

	public void Run()
	{
		Run(targetToAttack != null ? targetToAttack : SelectTarget());
	}

	public void Run(GameObject target)
	{
		targetToAttack = target;
		if (state == State.Sitting)
		{
			StandUp();
			return;
		}
		if (state == State.StandingUp && stateProgress < 0.9f)
		{
			return;
		}
		if (!targetToAttack)
		{
			Stay();
		}
		else
		{
			ChangeState(State.Running);
			if (Singleton.Quality.HitSoundsOnScene && ChaseSound != null && ChaseSound.Length > 0)
			{
				SoundManager.PlayExclusive(ChaseSound[UnityEngine.Random.Range(0, ChaseSound.Length)], transform.position, 5f, 25f, false);
			}
		}
	}

	public void Melee(GameObject target)
	{
		targetToAttack = target;
		Melee();
	}

	public void Melee()
	{
		StopMovement();
		meleeDamageWasDealt = false;
		ChangeState(State.Melee);
	}

	protected override void proceedToNextState()
	{
		if (nextState == State.Appearing)
		{
			Appear();
		}
		else if (nextState == State.Sitting)
		{
			Sit();
		}
		else if (nextState == State.StandingUp)
		{
			StandUp();
		}
		else if (nextState == State.Leaning)
		{
			Lean();
		}
		else if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Wandering)
		{
			Wander();
		}
		else if (nextState == State.Chasing)
		{
			Chase();
		}
		else if (nextState == State.Running)
		{
			Run();
		}
		else if (nextState == State.Melee)
		{
			Melee();
		}
		else
		{
			base.proceedToNextState();
		}
	}

	public bool IsAttacking(GameObject obj)
	{
		return (state == State.Chasing || state == State.Running || state == State.Melee) && (obj == null || targetToAttack == obj);
	}

	public GameObject AttackingWhom()
	{
		return !(!IsAttacking(null)) ? targetToAttack : null;
	}

	private bool IsIdle()
	{
		return state == State.Sitting || state == State.Leaning || state == State.Staying || state == State.Wandering;
	}

	protected override void OnCreateRagdoll(GameObject ragdoll)
	{
		base.OnCreateRagdoll(ragdoll);
		ReplaceTextures(ragdoll);
		if (previousState == State.Appearing && (appearingType == SmartSpawnPoint.Appearing.Floor || appearingType == SmartSpawnPoint.Appearing.Wall || appearingType == SmartSpawnPoint.Appearing.UnderWall) && previousStateProgress < 0.75f)
		{
			Util.DoForHierarchy(ragdoll, (GameObject node) =>
			{
				node.layer = LayerMask.NameToLayer("AppearingRagdoll");
				return true;
			});
		}
	}

	private GameObject SelectTarget()
	{
		Dictionary<GameObject, float> dictionary = new Dictionary<GameObject, float>();
		foreach (Character item in Scene.CharsAll)
		{
			Human human = item as Human;
			if (!(human != null) || !Scene.FilterChar(item, false))
			{
				continue;
			}
			float a = Vector3.Distance(transform.position, human.transform.position);
			dictionary[human.gameObject] = human.attractiveness * Mathf.Pow(3f, -Mathf.Min(a, 25f) / 10f);
		}
		if (dictionary.Count > 0)
		{
			return Util.RandomByWeights(dictionary);
		}
		log.Error(gameObject, "no targets found for spawned zombie");
		return null;
	}

	private void ChaseHumanCheck()
	{
		if (Util.DoRarely(this, "chase_check", 0.8f))
		{
			foreach (Human human in Scene.Humans)
			{
				bool flag = (human.Center() - senseCenter()).magnitude <= senseRadius();
				bool flag2 = (human.Center() - sightCenter()).magnitude <= sightRadius();
				bool flag3 = Util.AnyObstacles(Eyes(), human.Eyes());
				if ((flag || flag2) && !flag3 && human.attractiveness > 0f)
				{
					Chase(human.gameObject);
					return;
				}
			}
		}
	}

	private bool CheckMelee()
	{
		return GetIntermediateRange(targetToAttack) <= meleeDistance && Vector3.Angle(transform.forward, targetToAttack.transform.position - transform.position) <= 30f && !Util.AnyObstacles(Center(), !(!targetToAttack.GetComponent<Character>()) ? targetToAttack.GetComponent<Character>().Center() : targetToAttack.transform.position);
	}

	private void RotateToTarget(Vector3 toTarget)
	{
		if (Vector3.Angle(transform.forward, toTarget) > 5f)
		{
			agent.updateRotation = false;
			RotateTo(targetToAttack.transform.position, agent.angularSpeed);
		}
	}

	protected override void Update()
	{
		if (demoMode)
		{
			health = 1000000f;
			animation.Play("run");
			return;
		}
		else
		{
			if (sourceOfInput != InputFrom.User)
			{
				return;
			}
			if ((state == State.Chasing || state == State.Running || state == State.Melee) && (!targetToAttack || ((bool)targetToAttack.GetComponent<Character>() && targetToAttack.GetComponent<Character>().IsDead())))
			{
				Stay();
				return;
			}
			if (state == State.Sitting || state == State.Leaning || state == State.Staying)
			{
				ChaseHumanCheck();
			}
			else if (state == State.Wandering)
			{
				ChaseHumanCheck();
				if ((wanderingTo - transform.position).magnitude <= agent.radius)
				{
					Stay();
					return;
				}
				bool movementPlanned = PlanMovementTo(wanderingTo, speedBase * wanderingSpeedMod);
				if (!movementPlanned)
				{
					Stay();
					return;
				}
			}
			else if (state == State.Chasing || state == State.Running)
			{
				if (Util.DoRarely(this, "spread_aggresion", 1.2f))
				{
					foreach (Zombie item in Scene.Nearby<Zombie>(Center(), spreadAgressionRange))
					{
						if (item != this && IsIdle())
						{
							item.Chase(targetToAttack);
						}
					}
				}
				Vector3 vector = targetToAttack.transform.position - transform.position;
				if (Util.DoRarely(this, "stop_chase", 1f) && vector.magnitude >= stopChasingRange)
				{
					Stay();
					return;
				}
				if (CheckMelee())
				{
					Melee();
					return;
				}
				if (speed < speedBase * 0.1f && Util.DoRarely(this, "meleeable_obstruction_check", 0.7f))
				{
					foreach (Human item2 in Scene.Nearby<Human>(Center(), Radius() + meleeDistance * 1.2f))
					{
						if (item2.gameObject != targetToAttack)
						{
							Melee(item2.gameObject);
							return;
						}
					}
				}
				if (GetIntermediateRange(targetToAttack) <= meleeDistance * 1.2f)
				{
					RotateToTarget(vector);
				}
				if (updatesDoneInState == 0 || Util.DoRarely(this, "plan_chasing", 0.3f + 3f * Mathf.Clamp01(0.02f * (targetToAttack.transform.position - transform.position).magnitude)))
				{
					float num = state == State.Chasing ? speedBase : speedBase * runningSpeedMod;
					Character component = targetToAttack.GetComponent<Character>();
					float unusedChaseValue;
					if (!component)
					{
						PlanMovementTo(targetToAttack.transform.position, num);
					}
					else
					{
						int num2 = 0;
						do
						{
							float num3 = 1f / Mathf.Pow(2f, num2);
							float num4 = Vector3.Angle(-vector, component.Direction);
							float num5 = Mathf.Sin((float)Math.PI / 180f * num4) * 57.29578f * Mathf.Asin(Mathf.Clamp(component.Speed / num, 0f, Mathf.Sin((float)Math.PI / 180f * interdictionAngleLimit)));
							float num6 = Mathf.Abs(180f - num4 - num5);
							Vector3 vector2 = component.Direction.normalized * Mathf.Min(3f * num3, (targetToAttack.transform.position - transform.position).magnitude / Mathf.Sin((float)Math.PI / 180f * num6) * Mathf.Sin((float)Math.PI / 180f * num5));
							Vector3 vector3 = component.transform.position + vector2;
							UnityEngine.Debug.DrawLine(transform.position, vector3, new Color(0f, 0f, 1f));
							Vector3 vector4 = Quaternion.Euler(0f, flankingShift + (float)((seedFlanking % 2 == 0) ? 1 : (-1)) * ((float)seedFlanking % flanking), 0f) * ((transform.position - vector3) * 0.5f * num3);
							if (Util.ChancePerTime(changeFlankingChancePerSec, this, "reseed_flanking", 1f))
							{
								seedFlanking = UnityEngine.Random.Range(0, int.MaxValue);
							}
							vector3 += vector4;
							NavMeshPath path = new NavMeshPath();
							bool pathFound = NavMesh.CalculatePath(component.transform.position, vector3, -1, path);
							float unusedPathValue;
							bool pathShortEnough = pathFound && PathHelper.GetPathLength(path) < (component.transform.position - vector3).magnitude * 2f;
							if (pathShortEnough)
							{
								if (!PlanMovementTo(vector3, num))
								{
									Stay();
									return;
								}
								break;
							}
							if ((float)num2 < 1f)
							{
								num2++;
								continue;
							}
							if (!PlanMovementTo(component.transform.position, num))
							{
								Stay();
								return;
							}
							break;
						} while (true);
					}
				}
			}
			else if (state == State.Melee)
			{
				if (timeSpentInState / meleeDurationActual >= meleeDamageIndent && !meleeDamageWasDealt)
				{
					meleeDamageWasDealt = true;
					Character component2 = targetToAttack.GetComponent<Character>();
					if (component2 != null)
					{
						component2.OnDamage(new HitInfo(meleeDamage, DamageInfo.Type.Melee, this, mass * transform.forward.normalized, 20f, 5f, 20f, 1f, component2.Center(), null, 0f));
					}
					if (Tutorial.IsRunning() && ChaseSound != null && ChaseSound.Length > 0)
					{
						SoundManager.PlayExclusive(ChaseSound[UnityEngine.Random.Range(0, ChaseSound.Length)], transform.position + Vector3.up * 1.5f, 5f, 25f, false);
					}
				}
				if (timeSpentInState >= meleeDurationActual)
				{
					if ((bool)targetToAttack.GetComponent<Character>())
					{
						Melee();
						return;
					}
					Stay();
					return;
				}
				Vector3 vector5 = targetToAttack.transform.position - transform.position;
				float unusedMeleeValue;
				RotateToTarget(vector5);
				if (GetIntermediateRange(targetToAttack) > meleeDistance * 2f || Vector3.Angle(transform.forward, vector5) > 60f)
				{
					agent.speed = speedBase * 0.1f;
					Run(targetToAttack);
					return;
				}
			}
			else if (state == State.Pain && isPushedBack)
			{
				PlanMovementTo(pushedBackEndPoint, 0.7692308f);
				agent.updateRotation = false;
				if (Vector3.Distance(transform.position, pushedBackEndPoint) > 0.1f)
				{
					RotateTo(transform.position + (transform.position - pushedBackEndPoint), agent.angularSpeed);
				}
			}
			base.Update();
		}
	}

	protected override void LateUpdate()
	{
		if (animation.isPlaying)
		{
			float unusedLocal1;
			float unusedLocal2;
			float unusedLocal3;
			float unusedLocal4;
			Quaternion quaternion = head.transform.rotation * headRestoration;
			if ((state == State.Chasing || state == State.Running) && targetToAttack != null && (targetToAttack.transform.position - transform.position).magnitude < 4f)
			{
				if (Util.DoRarelyReal(this, "rotate_head", 0.1f))
				{
					Quaternion quaternion2 = Quaternion.LookRotation((targetToAttack.GetComponent<Character>() ? targetToAttack.GetComponent<Character>().PartialHeight(0.5f) : targetToAttack.transform.position) - head.transform.position);
					Vector3 fromDirection = quaternion * new Vector3(0f, 0f, 1f);
					Vector3 toDirection = quaternion2 * new Vector3(0f, 0f, 1f);
					Quaternion quaternion3 = Quaternion.FromToRotation(fromDirection, toDirection);
					float num = quaternion3.eulerAngles.x;
					if (num > 180f)
					{
						num -= 360f;
					}
					float num2 = quaternion3.eulerAngles.y;
					if (num2 > 180f)
					{
						num2 -= 360f;
					}
					num = Mathf.Clamp(num, -20f, 20f);
					num2 = Mathf.Clamp(num2, -70f, 70f);
					headPitch = Mathf.MoveTowards(headPitch, num, Time.deltaTime * 200f);
					headYaw = Mathf.MoveTowards(headYaw, num2, Time.deltaTime * 200f);
				}
			}
			else
			{
				headPitch = Mathf.MoveTowards(headPitch, 0f, Time.deltaTime * 200f);
				headYaw = Mathf.MoveTowards(headYaw, 0f, Time.deltaTime * 200f);
			}
			Quaternion quaternion4 = Quaternion.Euler(headPitch, headYaw, 0f);
			head.transform.rotation = quaternion4 * quaternion * headRestorationInversed;
		}
		base.LateUpdate();
	}

	protected override bool CanBeStunned(DamageInfo info)
	{
		return canBeStunned && state != State.Sitting && state != State.StandingUp && base.CanBeStunned(info);
	}

	public override void OnHearShotSound(Vector3 point)
	{
		if (IsIdle() && (Eyes() - point).magnitude <= hearingRange && !Util.AnyObstacles(Center(), point))
		{
			Wander(point);
		}
	}

	protected override void OnHurt(DamageInfo info)
	{
		isPushedBack = info.type == DamageInfo.Type.Melee && CanBeStunned(info);
		base.OnHurt(info);
		if (isPushedBack)
		{
			NavMeshHit hit;
			Vector3 vector = (info.source ? (transform.position - info.source.transform.position).normalized : -transform.forward) * nextStateDelay * 0.7692308f;
			UnityEngine.Debug.DrawLine(transform.position, transform.position + vector, new Color(1f, 0f, 1f), 1f);
			NavMesh.Raycast(transform.position, transform.position + vector, out hit, agent.walkableMask);
			pushedBackEndPoint = hit.position;
		}
		if (reactToDamage && state != State.Appearing && (bool)info.source && !IsAttacking(info.source.gameObject) && info.source is Human && (info.source as Human).attractiveness > 0f)
		{
			GameObject target = info.source.gameObject;
			if (state != State.Pain)
			{
				Run(target);
			}
			else
			{
				targetToAttack = target;
			}
		}
	}

	protected override void OnDying(DamageInfo info)
	{
		if (info != null && info.source is Human)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.KillCommonInfected);
		}
		if (state == State.Appearing)
		{
			needRagdoll = true;
		}
		base.OnDying(info);
	}

	private void InitAnimations()
	{
		if (demoMode)
		{
			animation["run"].speed = 1f + UnityEngine.Random.value * 0.5f;
		}
		foreach (string text in new string[12]
		{
			"idle", "idle2", "run", "run2", "run3", "run4", "walk", "slow_walk1", "slow_walk2", "slow_walk3",
			"sit", "stand_near_wall"
		})
		{
			animation[text].wrapMode = WrapMode.Loop;
		}
		foreach (SmartSpawnPoint.Appearing value in Enum.GetValues(typeof(SmartSpawnPoint.Appearing)))
		{
			string text2 = "spawn_" + value.ToString().ToLower();
			if ((bool)animation[text2])
			{
				animation[text2].wrapMode = WrapMode.ClampForever;
			}
		}
		foreach (string text3 in new string[4] { "pain1", "pain2", "pain3", "stand_up" })
		{
			animation[text3].wrapMode = WrapMode.ClampForever;
		}
		animation["pushback1"].wrapMode = WrapMode.Once;
		animation["pushback1"].layer = 1;
		animation["pushback_down1"].wrapMode = WrapMode.ClampForever;
		animation["pushback_down1"].AddMixingTransform(this.QuickFindChild("Point01").transform, false);
		animation["pushback_down1"].AddMixingTransform(this.QuickFindChild("Point01/HIP_locator").transform, false);
		animation["pushback_down1"].AddMixingTransform(this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone").transform, false);
		animation["pushback_down1"].AddMixingTransform(this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01spine_Bone").transform, false);
		animation["pushback1"].AddMixingTransform(this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01spine_Bone/Bip01chest_Bone").transform);
		animation["pushback_down1"].AddMixingTransform(this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01L_thigh_Bone").transform);
		animation["pushback_down1"].AddMixingTransform(this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01R_thigh_Bone").transform);
		applyPainTo = this.QuickFindChild("Point01/HIP_locator/Bip01hip_Bone/Bip01spine_Bone");
		foreach (string text4 in new string[4] { "pain_body1", "pain_body2", "pain_body3", "pain_body4" })
		{
			animation[text4].layer = 1;
			animation[text4].AddMixingTransform(applyPainTo.transform, true);
		}
		animWalk = Util.RandomFrom<string>("slow_walk1", "slow_walk2", "slow_walk3");
		animRun = Util.RandomByWeights(new Dictionary<string, float>
		{
			{ "run", 50f },
			{ "run3", 30f },
			{ "run4", 20f }
		});
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Appearing)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime(0f, 1f, "spawn_" + appearingType.ToString().ToLower());
			}
		}
		else if (state == State.Sitting)
		{
			if (firstRun)
			{
				PlayAnimation("sit");
			}
		}
		else if (state == State.StandingUp)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("stand_up");
			}
		}
		else if (state == State.Leaning)
		{
			if (firstRun)
			{
				PlayAnimation("stand_near_wall");
			}
		}
		else if (state == State.Staying)
		{
			if (firstRun)
			{
				PlayAnimation("idle", "idle2");
			}
		}
		else if (state == State.Wandering || state == State.Chasing || state == State.Running)
		{
			string text = state == State.Running
				? (isCrippled ? "run2" : animRun)
				: (state == State.Chasing ? "walk" : animWalk);
			if (firstRun)
			{
				PlayAnimation(text);
			}
			float num = state == State.Running ? 3.3f : (state == State.Chasing ? 1.67f : 0.5f);
			animation[text].speed = Mathf.Max(speed, 0.001f) / num / transform.lossyScale.z;
		}
		else if (state == State.Melee)
		{
			if (firstRun)
			{
				List<string> attacks = new List<string> { "attack1", "attack2", "attack3", "attack4" };
				List<string> availableAttacks = new List<string>(attacks);
				foreach (string item in attacks)
				{
					if (animation.IsPlaying(item))
					{
						availableAttacks.Remove(item);
					}
				}
				string text2 = Util.RandomFrom((availableAttacks.Count > 0 ? availableAttacks : attacks).ToArray());
				float num2 = animation[text2].length > meleeDurationActual ? animation[text2].length / meleeDurationActual : 1f;
				PlayAnimation(num2, text2);
				PlayAnimationQueued("idle");
			}
		}
		else if (state == State.Pain)
		{
			if (firstRun)
			{
				if (!isPushedBack)
				{
					PlayAnimation("pain1", "pain2", "pain3");
					nextStateDelay += Util.ApplyRandomness(painStunTime, 0.2f);
				}
				else
				{
					PlayAnimationAndAddTime("pushback1");
					PlayAnimation("pushback_down1");
				}
			}
			animation["pushback_down1"].speed = Mathf.Max(speed, 0.001f) / 0.7692308f / transform.lossyScale.z;
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("death1", "death2", "death3");
			}
		}
		else if (state == State.Dead)
		{
		}
		else
		{
			base.Animate(firstRun);
		}
	}

	protected override void PainWhileMoving()
	{
		AnimatePainWhileMoving(0.2f, "pain_body1", "pain_body3", "pain_body4");
	}

	public void WasSummoned()
	{
		SetCharStatus(CharStatus.WasSummoned, 15f);
	}

	protected override void OnDrawGizmos()
	{
		if (!(Application.isPlaying))
		{
			return;
		}
		base.OnDrawGizmos();
	}
}
