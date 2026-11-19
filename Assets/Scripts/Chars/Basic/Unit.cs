using System.Collections.Generic;
using UnityEngine;

public class Unit : Character
{
	protected delegate float PointMetric(Vector3 point);

	private const int MaxHiddenBones = 48;

	public float speedBase = 1f;

	public float speedMaxRandomness = 0.5f;

	public float acceleration = 10f;

	public GameObject HeadHitzone;

	public GameObject HeadBone;

	public GameObject LeftHandHitzone;

	public GameObject LeftHandBone;

	public GameObject LeftHandPrefab;

	public GameObject RightHandHitzone;

	public GameObject RightHandBone;

	public GameObject RightHandPrefab;

	public GameObject ArmorHitzone1;

	public GameObject ArmorHitzone2;

	public float ArmorHitzoneDamageFactor = 0.5f;

	protected bool stunable = true;

	protected NavMeshAgent agent;

	private Vector3? movingTo;

	private bool obstacled;

	private GameObject obstacle;

	private Vector3 lastSteeringTarget;

	private int _numDismemberedParts;

	private int _numHiddenBones;

	private Transform[] _hiddenBones = new Transform[48];

	public bool Obstacled
	{
		get
		{
			return obstacled;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		agent = GetComponent<NavMeshAgent>();
		agent.radius = characterController.radius + navigationRadiusIncrement;
		speedBase = Util.ApplyRandomness(speedBase, speedMaxRandomness);
		agent.speed = speedBase;
		agent.acceleration = acceleration;
		obstacle = new GameObject("Obstacle", typeof(Rigidbody), typeof(CapsuleCollider));
		obstacle.transform.parent = transform;
		obstacle.transform.localPosition = Vector3.zero;
		obstacle.rigidbody.isKinematic = true;
		obstacle.layer = Const.LayerDynamicObstacle;
		obstacle.GetComponent<CapsuleCollider>().center = characterController.center;
		obstacle.GetComponent<CapsuleCollider>().height = characterController.height * 0.1f;
		obstacle.GetComponent<CapsuleCollider>().radius = characterController.radius;
		obstacle.active = false;
		if ((bool)LeftHandPrefab)
		{
			Scene.RagdollManager.Preinstance(LeftHandPrefab);
		}
		if ((bool)RightHandPrefab)
		{
			Scene.RagdollManager.Preinstance(RightHandPrefab);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnActivate()
	{
		Singleton.GameEvents.OnEnemySpawned(this);
		base.OnActivate();
	}

	private void CheckDynamicObstacles(bool always)
	{
		if (always || (movingTo != null &&
			((agent.steeringTarget - lastSteeringTarget).magnitude > 0.01f ||
			 (agent.steeringTarget - transform.position).magnitude < 0.01f ||
			 Util.DoRarely(this, "reobstacle", 0.5f))))
		{
			lastSteeringTarget = agent.steeringTarget;
			CharacterController characterController = base.characterController;
			Vector3 normalized = (agent.steeringTarget - transform.position).normalized;
			float num = Mathf.Min(2f, (agent.steeringTarget - transform.position).magnitude);
			RaycastHit hitInfo;
			obstacled = Physics.CapsuleCast(transform.position + Vector3.up * (characterController.radius + 0.1f * (characterController.height - 2f * characterController.radius)), transform.position + Vector3.up * (characterController.radius + 0.9f * (characterController.height - 2f * characterController.radius)), Radius() * 0.1f, normalized, out hitInfo, num, 1 << Const.LayerDynamicObstacle);
			UnityEngine.Debug.DrawLine(transform.position, transform.position + normalized * num, new Color(1f, 1f, 0f));
		}
		if (obstacled)
		{
			agent.speed = 0f;
			agent.velocity = Vector3.zero;
			obstacle.active = true;
			agent.radius = 0f;
		}
		else
		{
			obstacle.active = false;
			agent.radius = base.characterController.radius + navigationRadiusIncrement;
		}
	}

	protected override void Update()
	{
		CheckDynamicObstacles(false);
		base.Update();
	}

	protected override void LateUpdate()
	{
		for (int i = 0; i < _numHiddenBones; i++)
		{
			_hiddenBones[i].localScale = Dismemberment.InvisibleBoneScale;
		}
		base.LateUpdate();
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		CheckDynamicObstacles(true);
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void UpdateMovement()
	{
		NavMeshAgent component = GetComponent<NavMeshAgent>();
		speed = component.velocity.magnitude;
		direction = transform.forward;
	}

	protected void StopMovement()
	{
		agent.Stop();
		agent.ResetPath();
		agent.velocity = Vector3.zero;
		movingTo = null;
	}

	protected bool PlanMovementTo(Vector3 point, float desiredSpeed)
	{
		agent.speed = desiredSpeed;
		agent.autoRepath = false;
		bool flag = movingTo == null || (point - (Vector3)movingTo).magnitude >= 0.1f;
		if (flag)
		{
			movingTo = point;
			agent.Resume();
		}
		agent.SetDestination((Vector3)movingTo);
		CheckDynamicObstacles(flag);
		if (!flag && !agent.hasPath)
		{
			return false;
		}
		if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
		{
			return false;
		}
		return true;
	}

	protected void MoveStraight(Vector3 point)
	{
		bool useAcceleration;
		direction = (point - transform.position).normalized;
		useAcceleration = true;
		if (!useAcceleration)
		{
			speed = speedBase;
		}
		else
		{
			speed += Mathf.Sign(speedBase - speed) * agent.acceleration * Time.deltaTime;
			speed = Mathf.Clamp(speed, -speedBase, speedBase);
		}
	}

	protected Vector3 FindMovement(Vector3 spreadDirection, float spreadAngle, float rangeMax, PointMetric metric)
	{
		List<KeyValuePair<Vector3, float>> list = new List<KeyValuePair<Vector3, float>>();
		for (float num = -spreadAngle; num < spreadAngle; num += Util.Range(30f, 45f))
		{
			NavMeshHit hit;
			Vector3 vector = transform.position + Quaternion.Euler(0f, num, 0f) * (spreadDirection * rangeMax);
			NavMesh.Raycast(transform.position, vector, out hit, -1);
			Vector3 vector2 = hit.hit ? hit.position : vector;
			vector2 = transform.position + (vector2 - transform.position) * 0.9f;
			list.Add(new KeyValuePair<Vector3, float>(vector2, metric(vector2)));
			UnityEngine.Debug.DrawLine(transform.position, vector2, new Color(1f, 0.5f, 0f));
			UnityEngine.Debug.DrawLine(vector, vector2, new Color(0.5f, 0.5f, 0.5f));
		}
		list.Sort((KeyValuePair<Vector3, float> a, KeyValuePair<Vector3, float> b) => a.Value.CompareTo(b.Value));
		return list[0].Key;
	}

	protected override bool CanBeStunned(DamageInfo info)
	{
		return stunable && base.CanBeStunned(info);
	}

	protected override void OnHurt(DamageInfo info)
	{
		if (CanBeStunned(info))
		{
			StopMovement();
		}
		else if (CanBePained(info))
		{
			PainWhileMoving();
		}
		base.OnHurt(info);
	}

	protected override void OnDying(DamageInfo info)
	{
		StopMovement();
		characterController.enabled = false;
		agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		base.OnDying(info);
	}

	public override void OnHeadshot(DamageInfo info_)
	{
		if ((bool)HeadBone)
		{
			Scene.EffectsManager.DrawHeadshot(HeadBone.transform.position);
			HeadHitzone = null;
			HideBone(HeadBone.transform, Vector3.up, RagdollManager.LimbType.Hand);
			SoundManager.Play("skill_shot", 2000, false);
		}
		base.OnHeadshot(info_);
	}

	public override void OnDamage(DamageInfo info_)
	{
		if (info_ is HitInfo)
		{
			HitInfo hitInfo = info_ as HitInfo;
			if ((bool)hitInfo.hitObject)
			{
				if (hitInfo.hitObject == HeadHitzone)
				{
					if (hitInfo.type == DamageInfo.Type.Bullet)
					{
						hitInfo.hitZone = DamageInfo.HitZone.Head;
					}
				}
				else if (hitInfo.hitObject == LeftHandHitzone)
				{
					if (Util.Chance(hitInfo.defragChance))
					{
						GameObject gameObject = Scene.RagdollManager.Instantiate(LeftHandPrefab);
						gameObject.transform.parent = Scene.Groups.Ragdolls;
						gameObject.transform.position = LeftHandBone.transform.position;
						gameObject.transform.eulerAngles = LeftHandBone.transform.eulerAngles;
						gameObject.rigidbody.velocity = hitInfo.momentum / gameObject.rigidbody.mass;
						OnDismember(gameObject);
						if ((bool)RagdollManager.instance)
						{
							RagdollManager.instance.Manage(gameObject);
						}
						LeftHandHitzone = null;
						HideBone(LeftHandBone.transform, Vector3.forward, RagdollManager.LimbType.Hand);
						Singleton.GameEvents.OnEnemyDismemberment(hitInfo.source, this, hitInfo.source is Human ? (hitInfo.source as Human).Inventory.Weapon : null, null, false);
						_numDismemberedParts++;
					}
				}
				else if (hitInfo.hitObject == RightHandHitzone)
				{
					if (Util.Chance(hitInfo.defragChance))
					{
						GameObject gameObject2 = Scene.RagdollManager.Instantiate(RightHandPrefab);
						gameObject2.transform.parent = Scene.Groups.Ragdolls;
						gameObject2.transform.position = RightHandBone.transform.position;
						gameObject2.transform.eulerAngles = RightHandBone.transform.eulerAngles;
						gameObject2.rigidbody.velocity = hitInfo.momentum / gameObject2.rigidbody.mass;
						OnDismember(gameObject2);
						if ((bool)RagdollManager.instance)
						{
							RagdollManager.instance.Manage(gameObject2);
						}
						RightHandHitzone = null;
						HideBone(RightHandBone.transform, -Vector3.forward, RagdollManager.LimbType.Hand);
						Singleton.GameEvents.OnEnemyDismemberment(hitInfo.source, this, hitInfo.source is Human ? (hitInfo.source as Human).Inventory.Weapon : null, null, false);
						_numDismemberedParts++;
					}
				}
				else if (((bool)ArmorHitzone1 && hitInfo.hitObject == ArmorHitzone1) || ((bool)ArmorHitzone2 && hitInfo.hitObject == ArmorHitzone2))
				{
					hitInfo.damage *= ArmorHitzoneDamageFactor;
				}
			}
		}
		base.OnDamage(info_);
	}

	protected override void OnCreateRagdoll(GameObject ragdoll)
	{
		base.OnCreateRagdoll(ragdoll);
		Dismemberment component = ragdoll.GetComponent<Dismemberment>();
		if ((bool)component)
		{
			if ((bool)HeadBone && IsHidden(HeadBone.transform))
			{
				component.Dismember(component.HeadBone, Dismemberment.Mode.HideBone);
			}
			if ((bool)LeftHandBone && IsHidden(LeftHandBone.transform))
			{
				component.Dismember(component.LeftHandBone, Dismemberment.Mode.HideBone);
			}
			if ((bool)RightHandBone && IsHidden(RightHandBone.transform))
			{
				component.Dismember(component.RightHandBone, Dismemberment.Mode.HideBone);
			}
		}
		else if ((bool)RagdollManager.instance)
		{
			RagdollManager.instance.Manage(ragdoll);
		}
	}

	public virtual void OnHearShotSound(Vector3 point)
	{
	}

	protected override float ModifyDamage(float damage, DamageInfo info)
	{
		return base.ModifyDamage(damage * (info.hitZone == DamageInfo.HitZone.Head
			? Mathf.Min((info as HitInfo).criticalMod, this is Special ? 1.25f : float.MaxValue)
			: 1f), info);
	}

	public int NumDismemberedParts
	{
		get
		{
			return _numDismemberedParts;
		}
	}

	protected bool IsHidden(Transform bone)
	{
		for (int i = 0; i < _numHiddenBones; i++)
		{
			if (_hiddenBones[i] == bone)
			{
				return true;
			}
		}
		return false;
	}

	protected void HideBone(Transform bone, Vector3 axis, RagdollManager.LimbType limbType)
	{
		RagdollManager.instance.ShowDismemberedJoint(bone, axis, limbType);
		if (!IsHidden(bone) && _numHiddenBones < 48)
		{
			_hiddenBones[_numHiddenBones] = bone;
			_numHiddenBones++;
		}
	}

	protected virtual void PainWhileMoving()
	{
	}

	protected void AnimatePainWhileMoving(float weight, params string[] painAnimations)
	{
		if (!animation.isPlaying)
		{
			return;
		}
		foreach (string text in painAnimations)
		{
			if (animation.IsPlaying(text) && animation[text].time / animation[text].length < 0.6f)
			{
				return;
			}
		}
		string text2 = Util.RandomFrom(painAnimations);
		animation[text2].weight = weight;
		if (animation[text2].blendMode == AnimationBlendMode.Additive)
		{
			animation.Play(text2);
		}
		else
		{
			PlayAnimation(text2);
		}
	}
}
