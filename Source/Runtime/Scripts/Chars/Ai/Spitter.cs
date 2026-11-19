using UnityEngine;

public class Spitter : Special
{
	public float stunChanceMod = 30f;

	public float stayingDuration = 4f;

	public float stayingDurationRandomness = 0.5f;

	public float wanderingRange = 5f;

	public float wanderingRangeRandomness = 0.5f;

	public float runningSpeedMod = 4f;

	public float hearShotRange = 25f;

	public float hearMovementRange = 20f;

	public float closeSpitRange = 2f;

	public float spitDuration = 5f;

	public float spitDamage = 100f;

	public float spitDamageAngle = 45f;

	public float spitDamageRange = 1.5f;

	public float rotationSpeed = 90f;

	public float gooDamage = 50f;

	public float postSpitDelay = 1f;

	public float idleTimeout = 20f;

	private Vector3 goingTo;

	private ParticleSystem spitEffect;

	private float rotationDir;

	private CountdownTimer noSpitTimer = new CountdownTimer();

	private float idleTime;

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Spitter");
		GameObject newParent = this.FindChildInHierarchy("SpitMount");
		Object original = Resources.Load("FxSpit");
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		gameObject.SetParent(newParent);
		gameObject.transform.localPosition = Vector3.zero;
		spitEffect = gameObject.GetComponent<ParticleSystem>();
		spitEffect.enableEmission = false;
		spitEffect.Play();
		Stay();
	}

	protected override void Start()
	{
		base.Start();
		InitAnimations();
	}

	private void Stay()
	{
		StopMovement();
		ChangeState(State.Staying, State.Wandering, Random.Range(stayingDuration, stayingDurationRandomness));
	}

	private void Wander()
	{
		goingTo = FindMovement(transform.forward, 180f, Util.ApplyRandomness(wanderingRange, wanderingRangeRandomness), (Vector3 point) => -Vector3.Distance(point, transform.position) + Util.Range(0f, 1f));
		ChangeState(State.Wandering);
	}

	private void Run()
	{
		Run(Scene.Player.transform.position);
	}

	private void Run(Vector3 to)
	{
		goingTo = to;
		SoundManager.PlayExclusive("Zombie/Spitter/spitter_aggro_1", transform.position, 5f, 25f, false);
		ChangeState(State.Running);
	}

	private void Spit(Vector3 point)
	{
		goingTo = point;
		Spit();
	}

	private void Spit()
	{
		rotationDir = (Util.Chance(50f) ? 1 : (-1));
		spitEffect.enableEmission = true;
		StopMovement();
		SoundManager.PlayExclusive("Zombie/Spitter/spitter_spit", transform.position, 5f, 25f, false);
		ChangeState(State.Throwing);
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		if (state == State.Throwing)
		{
			spitEffect.enableEmission = false;
		}
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void proceedToNextState()
	{
		if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Wandering)
		{
			Wander();
		}
		else if (nextState == State.Throwing)
		{
			Spit();
		}
		else if (nextState == State.Running)
		{
			Run();
		}
		else
		{
			base.proceedToNextState();
		}
	}

	protected override void Update()
	{
		if ((state == State.Staying || state == State.Wandering || state == State.Running) && !noSpitTimer.IsRunning())
		{
			if (Util.DoRarely(this, "close_spit_check", 0.3f))
			{
				Human human = Scene.NearbyAny<Human>(transform.position, closeSpitRange);
				if ((bool)human)
				{
					Spit(human.transform.position);
					return;
				}
			}
			if (Util.DoRarely(this, "hear_movement", 0.5f))
			{
				Human human2 = Scene.NearbyAny<Human>(transform.position, hearMovementRange);
				if (human2 != null && human2.Speed > human2.baseSpeed * 0.5f)
				{
					Run(human2.transform.position);
					return;
				}
			}
		}
		if ((state == State.Staying || state == State.Wandering) && Util.DoRarely(this, "absorb_aggresion", 0.8f))
		{
			float unusedAggroValue1;
			float unusedAggroValue2;
			foreach (Zombie item in Scene.Nearby<Zombie>(Center(), 3f))
			{
				if (item.IsAttacking(null))
				{
					Run(item.AttackingWhom().transform.position);
					return;
				}
			}
		}
		if (state == State.Staying || state == State.Wandering)
		{
			idleTime += Time.deltaTime;
		}
		else
		{
			idleTime = 0f;
		}
		if (idleTime >= idleTimeout)
		{
			Run(Scene.Player.transform.position);
			return;
		}
		if (state == State.Staying)
		{
			if (Vector3.Distance(transform.position, Scene.Player.transform.position) >= 20f)
			{
				Run();
				return;
			}
		}
		else if (state == State.Wandering)
		{
			if (Vector3.Distance(transform.position, goingTo) < 0.1f)
			{
				Stay();
				return;
			}
			if (Util.DoRarely(this, "wandering_repath", 0.5f))
			{
				PlanMovementTo(goingTo, speedBase);
			}
		}
		else if (state == State.Running)
		{
			if (Vector3.Distance(transform.position, goingTo) < 0.1f)
			{
				Spit();
				return;
			}
			if (Util.DoRarely(this, "moving_repath", 0.5f))
			{
				PlanMovementTo(goingTo, speedBase * runningSpeedMod);
			}
		}
		else if (state == State.Throwing)
		{
			if (Util.DoRarely(this, "spit_damage", 0.25f))
			{
				float unusedSpitValue1;
				float unusedSpitValue2;
				foreach (Character @char in Scene.Chars)
				{
					Vector3 to = @char.transform.position - transform.position;
					float unusedDamageValue;
					if (to.magnitude - Radius() > spitDamageRange || Mathf.Abs(Vector3.Angle(transform.forward, to)) > spitDamageAngle)
					{
						continue;
					}
					@char.OnDamage(new ExplosionInfo(spitDamage / 4f, DamageInfo.Type.Explosion, this, to.normalized * 50f, 0f, 0f, 20f, spitEffect.transform.position, spitDamageRange, null));
				}
			}
			if (Util.DoRarely(this, "launch_goo_ball", 0.1f))
			{
				GameObject gameObject = Object.Instantiate(Resources.Load("GooBall")) as GameObject;
				gameObject.SetParent(Util.FindOrCreate("/GooBalls"));
				gameObject.transform.position = spitEffect.transform.position;
				gameObject.transform.rotation = spitEffect.transform.rotation;
				gameObject.GetComponent<Rigidbody>().linearVelocity = gameObject.transform.forward * spitEffect.startSpeed * 0.7f;
				gameObject.GetComponent<Goo>().source = this;
				Object.Destroy(gameObject, spitEffect.startLifetime);
			}
			if (timeSpentInState >= spitDuration)
			{
				noSpitTimer.Run(postSpitDelay);
				Stay();
				return;
			}
			float num = rotationDir;
			transform.Rotate(0f, num * rotationSpeed * Time.deltaTime, 0f);
		}
		base.Update();
	}

	public override void UpdateTarget(GameObject newTarget)
	{
		if (state == State.Staying || state == State.Wandering || (state == State.Running && Vector3.Distance(newTarget.transform.position, goingTo) > 0.5f))
		{
			Run(newTarget.transform.position);
			return;
		}
	}

	public override void OnHearShotSound(Vector3 point)
	{
		if ((state == State.Staying || state == State.Wandering || state == State.Running) && (Eyes() - point).magnitude <= hearShotRange)
		{
			Run(point);
		}
	}

	protected override void OnHurt(DamageInfo info)
	{
		base.OnHurt(info);
		if (state != State.Throwing && info.source is Human && (info.source as Human).attractiveness > 0f)
		{
			Vector3 position = info.source.transform.position;
			if (state != State.Pain)
			{
				Run(position);
			}
			else
			{
				goingTo = position;
			}
		}
	}

	public override void OnDamage(DamageInfo info)
	{
		info.stunChance *= stunChanceMod / 100f;
		base.OnDamage(info);
	}

	protected override bool CanBeStunned(DamageInfo info)
	{
		return state != State.Throwing && base.CanBeStunned(info);
	}

	private void InitAnimations()
	{
		foreach (string text in new string[4] { "idle", "walk", "run", "attack" })
		{
			GetComponent<Animation>()[text].wrapMode = WrapMode.Loop;
		}
		foreach (string text2 in new string[3] { "pain1", "pain2", "death" })
		{
			GetComponent<Animation>()[text2].wrapMode = WrapMode.ClampForever;
		}
		stunable = true;
		GetComponent<Animation>()["pain_on_move"].layer = 1;
		GetComponent<Animation>()["pain_on_move"].blendMode = AnimationBlendMode.Additive;
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
		else if (state == State.Wandering || state == State.Running)
		{
			bool flag = state == State.Running;
			string text = flag ? "run" : "walk";
			float num = flag ? 4.4f : 0.8f;
			if (firstRun && !GetComponent<Animation>().IsPlaying(text))
			{
				PlayAnimation(text);
			}
			GetComponent<Animation>()[text].speed = speed / num / transform.lossyScale.z;
		}
		else if (state == State.Throwing)
		{
			if (firstRun)
			{
				PlayAnimation("attack");
			}
		}
		else if (state == State.Pain)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("pain1", "pain2");
			}
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("death");
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
		AnimatePainWhileMoving(0.5f, "pain_on_move");
	}
}
