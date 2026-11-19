using UnityEngine;

public class Thrower : Special
{
	private const float throwingFromHeight = 1.3f;

	private const float throwingToHeight = 0.2f;

	public float desiredRange = 12f;

	public float desiredRangeVariety = 2f;

	public float fastThrowingRangeMod = 0.5f;

	public float reloadingDamageMod = 1.3f;

	public float throwingDuration = 3f;

	public float fastThrowingDuration = 3f;

	public GameObject projectilePrefab;

	public float projectileVelocityX = 5f;

	public float projectileGravity = -5f;

	public float projectileDamage = 100f;

	public float usageOfAmmo = 50f;

	public float usageOfAmmoFast = 33f;

	public float reloadDuration = 4f;

	public float interdictionLimit = 1f;

	public float inaccuracy = 0.5f;

	public float reloadMovementChance = 80f;

	public float reloadMovementRange = 5f;

	private GameObject targetToChase;

	private bool isThrowingFast;

	private float[] throwingNormal = new float[1] { 0.58f };

	private float[] throwingFast = new float[2] { 0.6f, 0.76f };

	private bool[] thrown;

	private float[] preThrowingNormal = new float[1] { 0.2f };

	private float[] preThrowingFast = new float[2] { 0.16f, 0.18f };

	private bool[] preThrown;

	private GameObject[] preThrownProjectiles;

	private GameObject leftHand;

	private GameObject rightHand;

	private GameObject ammoSource;

	private float ammo = 100f;

	private Vector3 movingTo;

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Thrower");
		thrown = new bool[Mathf.Max(throwingNormal.Length, throwingFast.Length)];
		preThrown = new bool[Mathf.Max(preThrowingNormal.Length, preThrowingFast.Length)];
		preThrownProjectiles = new GameObject[preThrown.Length];
		InitAnimations();
		leftHand = this.FindChildInHierarchy("LeftHand");
		rightHand = this.FindChildInHierarchy("RightHand");
		ammoSource = this.FindChildInHierarchy("stomach_Bone");
	}

	protected override void Start()
	{
		base.Start();
		Stay();
	}

	private void Stay()
	{
		StopMovement();
		ChangeState(State.Staying);
	}

	private void Chase()
	{
		ChangeState(State.Chasing);
	}

	private bool InRange(float rangeMod)
	{
		return Vector3.Distance(transform.position, targetToChase.transform.position) < desiredRange * rangeMod;
	}

	private void Throw()
	{
		if (ammo - (!isThrowingFast ? usageOfAmmo : usageOfAmmoFast) < 0f)
		{
			Reload();
			return;
		}
		StopMovement();
		for (int i = 0; i < thrown.Length; i++)
		{
			thrown[i] = false;
		}
		for (int j = 0; j < preThrown.Length; j++)
		{
			preThrown[j] = false;
		}
		isThrowingFast = InRange(fastThrowingRangeMod);
		SoundManager.PlayExclusive(isThrowingFast ? "Zombie/Thrower/thrower_attack" : "Zombie/Thrower/thrower_attack_2", transform.position, 5f, 25f, false);
		ChangeState(State.Throwing, State.Throwing, 0f);
	}

	private void Reload()
	{
		float unusedMovementValue;
		movingTo = Util.Chance(reloadMovementChance)
			? FindMovement((transform.position - targetToChase.transform.position).normalized, InRange(0.5f) ? 80 : 135,
				reloadMovementRange, (Vector3 point) => -Vector3.Distance(point, transform.position) + Util.Range(0f, 1f))
			: transform.position;
		ammo = 100f;
		StopMovement();
		SetHealthBarColor(true);
		ChangeState(State.Reloading, State.Throwing, 0f);
	}

	protected override void proceedToNextState()
	{
		if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Chasing)
		{
			Chase();
		}
		else if (nextState == State.Throwing)
		{
			Throw();
		}
		else if (nextState == State.Reloading)
		{
			Reload();
		}
		else
		{
			base.proceedToNextState();
		}
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		if (state == State.Reloading)
		{
			SetHealthBarColor(false);
			GetComponent<Animation>().Stop("reload");
		}
		if (state == State.Throwing)
		{
			for (int i = 0; i < preThrownProjectiles.Length; i++)
			{
				GameObject gameObject = preThrownProjectiles[i];
				if (!((bool)gameObject))
				{
					continue;
				}
				gameObject.transform.parent = null;
				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				gameObject.GetComponent<Rigidbody>().position = gameObject.transform.position;
				gameObject.GetComponent<Rigidbody>().rotation = gameObject.transform.rotation;
				gameObject.GetComponent<Rigidbody>().linearVelocity = gameObject.GetComponent<MotionCapture>().Velocity();
				gameObject.GetComponent<Rigidbody>().angularVelocity = gameObject.GetComponent<MotionCapture>().AngularVelocity();
				Singleton.Visual.HideOutline(gameObject);
				Object.Destroy(gameObject, 3f);
				preThrownProjectiles[i] = null;
			}
		}
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	private bool CanThrow(bool canOrNot)
	{
		Vector3 vector = targetToChase.GetComponent<Character>()
			? targetToChase.GetComponent<Character>().PartialHeight(0.2f)
			: targetToChase.transform.position;
		Vector3 vector2 = PartialHeight(1.3f);
		Vector3 vector3 = vector - vector2;
		float num = vector3.magnitude / 2f / projectileVelocityX;
		Vector3 vector4 = vector2 + Vector3.up * Mathf.Pow(num, 2f) * -projectileGravity / 2f + vector3.normalized * num * projectileVelocityX;
		bool flag = Vector3.Distance(transform.position, targetToChase.transform.position) < desiredRange + (float)(canOrNot ? -1 : 1) * desiredRangeVariety && !Util.AnyObstacles(Center(), vector) && !Util.AnyObstacles(vector2, vector4) && !Util.AnyObstacles(vector4, vector);
		UnityEngine.Debug.DrawLine(vector2, vector4, Color.blue, 1f);
		UnityEngine.Debug.DrawLine(vector4, vector, Color.blue, 1f);
		return canOrNot ? flag : !flag;
	}

	private GameObject IndexToHand(int projectileIndex)
	{
		return !(projectileIndex != 0) ? rightHand : leftHand;
	}

	protected override void Update()
	{
		if (!targetToChase)
		{
			targetToChase = Mission.GetSpecialTarget();
		}
		if ((state == State.Chasing || state == State.Throwing) && !targetToChase)
		{
			Stay();
			return;
		}
		if (state == State.Staying)
		{
			if ((bool)targetToChase)
			{
				Chase();
				return;
			}
		}
		else if (state == State.Chasing)
		{
			Character character;
			if (Util.DoRarely(this, "find_target", 1.5f) && (bool)(character = Scene.NearbyAny<Human>(transform.position, 2f + Radius())) && character != targetToChase && Vector3.Distance(transform.position, character.transform.position) < Vector3.Distance(transform.position, targetToChase.transform.position))
			{
				targetToChase = character.gameObject;
			}
			if (Util.DoRarely(this, "throw_check", 0.9f) && CanThrow(true))
			{
				Throw();
				return;
			}
			if (Util.DoRarely(this, "repath", 1f))
			{
				PlanMovementTo(targetToChase.transform.position, speedBase);
			}
		}
		else if (state == State.Throwing)
		{
			if (Util.DoRarely(this, "stop_throwing_check", 0.5f) && CanThrow(false))
			{
				Chase();
				return;
			}
			agent.updateRotation = false;
			RotateTo(targetToChase.transform.position, agent.angularSpeed);
			float[] array = !isThrowingFast ? preThrowingNormal : preThrowingFast;
			for (int i = 0; i < array.Length; i++)
			{
				if (!preThrown[i] && stateProgress >= array[i])
				{
					preThrown[i] = true;
					ammo -= (!isThrowingFast ? usageOfAmmo : usageOfAmmoFast);
					GameObject gameObject = Object.Instantiate(projectilePrefab, IndexToHand(i).transform.position, Quaternion.identity) as GameObject;
					gameObject.transform.parent = IndexToHand(i).transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.GetComponent<AbstractProjectile>().enabled = false;
					gameObject.GetComponent<Rigidbody>().isKinematic = true;
					gameObject.AddComponent<MotionCapture>();
					preThrownProjectiles[i] = gameObject;
				}
			}
			array = !isThrowingFast ? throwingNormal : throwingFast;
			Character component = targetToChase.GetComponent<Character>();
			Vector3 vector = component ? component.PartialHeight(0.2f) + Mathf.Min(interdictionLimit, Util.ZeroY(Center() - component.Center()).magnitude / projectileVelocityX) * component.Speed * component.Direction : targetToChase.transform.position;
			for (int j = 0; j < array.Length; j++)
			{
				if (!thrown[j] && stateProgress >= array[j])
				{
					thrown[j] = true;
					Vector3 position = preThrownProjectiles[j].transform.position;
					Vector3 vector2 = vector - position + inaccuracy * Util.RandomInCircle();
					float num = vector2.magnitude / 2f / projectileVelocityX * -projectileGravity;
					Vector3 velocity = projectileVelocityX * vector2.normalized + num * Vector3.up;
					GameObject gameObject2 = AbstractProjectile.Create(projectilePrefab, position, velocity, this, null);
					ThrowerProjectile component2 = gameObject2.GetComponent<ThrowerProjectile>();
					if ((bool)component2)
					{
						component2.CustomGravity = projectileGravity;
						component2.MaxDamage = projectileDamage;
					}
					gameObject2.GetComponent<Rigidbody>().position = preThrownProjectiles[j].transform.position;
					gameObject2.GetComponent<Rigidbody>().rotation = preThrownProjectiles[j].transform.rotation;
					gameObject2.GetComponent<Rigidbody>().angularVelocity = new Vector3(Util.Range(0f, 360f), Util.Range(0f, 360f), Util.Range(0f, 360f));
					Object.Destroy(preThrownProjectiles[j]);
				}
			}
		}
		else if (state == State.Reloading && Util.DoRarely(this, "move", 0.5f))
		{
			PlanMovementTo(movingTo, speedBase);
		}
		base.Update();
	}

	protected override void LateUpdate()
	{
		float t = (!(state != State.Reloading) ? stateProgress : (ammo / 100f));
		float num = Mathf.Lerp(0.5f, 1.5f, t);
		ammoSource.transform.localScale = new Vector3(num, num, num);
		base.LateUpdate();
	}

	public override void UpdateTarget(GameObject newTarget)
	{
		if (targetToChase == newTarget)
		{
			return;
		}
		if (state == State.Staying || state == State.Chasing || state == State.Throwing)
		{
			targetToChase = newTarget;
			Chase();
			return;
		}
	}

	protected override float ModifyDamage(float damage, DamageInfo info)
	{
		return base.ModifyDamage(damage * (!(state != State.Reloading) ? reloadingDamageMod : 1f), info);
	}

	private void InitAnimations()
	{
		GetComponent<Animation>()["idle"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["walk"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["attack"].wrapMode = WrapMode.ClampForever;
		GetComponent<Animation>()["attack_fast"].wrapMode = WrapMode.ClampForever;
		stunable = false;
		GetComponent<Animation>()["pain"].layer = 1;
		GetComponent<Animation>()["pain"].blendMode = AnimationBlendMode.Additive;
		GetComponent<Animation>()["reload"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["reload"].layer = 1;
		GetComponent<Animation>()["reload"].weight = 1f;
		GetComponent<Animation>()["reload"].AddMixingTransform(this.QuickFindChild("HIP_Bone/Spine_Bone").transform, true);
		GetComponent<Animation>()["reload"].AddMixingTransform(this.QuickFindChild("HIP_Bone/stomach_Bone").transform, true);
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Staying)
		{
			if (firstRun)
			{
				PlayAnimation("idle");
			}
		}
		else if (state == State.Chasing || state == State.Reloading)
		{
			string text = "walk";
			if (firstRun)
			{
				PlayAnimation(text);
			}
			GetComponent<Animation>()[text].speed = speed / 1.5f / transform.lossyScale.z;
			if (state == State.Reloading)
			{
				if (firstRun)
				{
					PlayAnimation("reload");
					nextStateDelay += reloadDuration;
				}
				if (Util.ZeroY(movingTo - transform.position).magnitude < 0.01f && !GetComponent<Animation>().IsPlaying("idle"))
				{
					PlayAnimation("idle");
				}
			}
		}
		else if (state == State.Throwing)
		{
			if (firstRun)
			{
				string text2 = !isThrowingFast ? "attack" : "attack_fast";
				float num = !isThrowingFast ? fastThrowingDuration : throwingDuration;
				float num2 = GetComponent<Animation>()[text2].length / num;
				PlayAnimationAndAddTime(num2, text2);
			}
		}
		else if (state == State.Pain)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("pain");
			}
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("death");
				nextStateDelay += 2f;
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
		AnimatePainWhileMoving(1f, "pain");
	}
}
