using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

public class Piper : Special
{
	public float desiredRange = 15f;

	public float desiredRangeVariety = 3f;

	public float urgentMoveThresholdRange = 3f;

	public float standingDuration = 3f;

	public float standingDurationRandomness = 0.9f;

	public float releaseWaveEvery = 20f;

	public float firstReleaseDelay = 5f;

	public float runningSpeedMod = 3f;

	public float summoningDuration = 3f;

	public int summoningRepeat = 2;

	public float meleeDistance = 0.5f;

	public float meleeDamage = 200f;

	public float summoningDamageMod = 1.5f;

	protected bool smart = true;

	private GameObject targetToChase;

	private Vector3 goingTo;

	private string movementSubstate = string.Empty;

	private GameObject targetToMelee;

	private float lastMeleeTime;

	private CountdownTimer firstReleaseTimer;

	private ScenarioWave wave;

	public override float Height()
	{
		return (!(state != State.Summoning) ? 2f : 1f) * base.Height();
	}

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Piper");
		wave = GetComponent<ScenarioWave>();
		if (wave != null)
		{
			wave.manualActivate = true;
		}
		Wander();
	}

	protected override void Start()
	{
		base.Start();
		InitAnimations();
		firstReleaseTimer = new CountdownTimer(firstReleaseDelay);
	}

	private void DecideWhereToGo()
	{
		DecideWhereToGo(false);
	}

	private void DecideWhereToGo(bool changeDistance)
	{
		if (!targetToChase)
		{
			goingTo = transform.position + Util.Range(1f, 3f) * Util.RandomInCircle();
			return;
		}
		Vector3 vector = targetToChase.transform.position - transform.position;
		bool approaching = vector.magnitude > desiredRange;
		float num = vector.magnitude > desiredRange ? vector.magnitude - (desiredRange - desiredRangeVariety) : desiredRange + desiredRangeVariety - vector.magnitude;
		if (!smart || (approaching && num > (desiredRange + desiredRangeVariety) * 1.5f))
		{
			goingTo = targetToChase.transform.position;
		}
		else
		{
			Vector3 a;
			float unusedLocal1;
			while (true)
			{
				a = FindMovement((approaching ? 1 : (-1)) * vector.normalized, changeDistance ? 90 : 180, num, (Vector3 point) => changeDistance ? (float)(approaching ? 1 : -1) * Vector3.Distance(point, targetToChase.transform.position) : -Vector3.Distance(point, transform.position));
				if (changeDistance && Vector3.Distance(a, transform.position) < 1f)
				{
					changeDistance = false;
					continue;
				}
				goingTo = a;
				UnityEngine.Debug.DrawLine(transform.position, goingTo, new Color(1f, changeDistance ? 0 : 1, 0f));
				break;
			}
		}
	}

	private void Stay()
	{
		StopMovement();
		ChangeState(State.Staying, State.Wandering, Util.ApplyRandomness(standingDuration, standingDurationRandomness));
	}

	private void Wander()
	{
		Wander(false);
	}

	private void Wander(bool changeDistance)
	{
		DecideWhereToGo(changeDistance);
		ChangeState(State.Wandering, State.Staying, Util.Range(10f, 30f));
	}

	private void Run()
	{
		Run(true);
	}

	private void Run(bool changeDistance)
	{
		DecideWhereToGo(changeDistance);
		ChangeState(State.Running);
	}

	private void Summon()
	{
		StopMovement();
		Singleton.UI.Hud.ShowInfo(Strings.GetString("Piper is summoning zombies..."));
		SoundManager.Play("Piper/piper_summon", false);
		ChangeState(State.Summoning, State.Running, 0f);
		SetHealthBarColor(true);
		SetCharStatus(CharStatus.Summoning, nextStateDelay);
	}

	private void Melee()
	{
		Melee(targetToChase);
	}

	private void Melee(GameObject target)
	{
		targetToMelee = target;
		Character component = targetToMelee.GetComponent<Character>();
		if (component != null)
		{
			SoundManager.Play("Piper/piper_attack", transform.position, 5f, 25f, false);
			component.OnDamage(new HitInfo(meleeDamage, DamageInfo.Type.Melee, this, mass * transform.forward.normalized, 60f, 30f, 100f, 1f, component.Center(), null, 0f));
		}
		StopMovement();
		ChangeState(State.Melee, State.Running, 0f);
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		movementSubstate = string.Empty;
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void proceedToNextState()
	{
		if (state == State.Summoning)
		{
			wave.OnActivateManual();
			SetHealthBarColor(false);
		}
		if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Wandering)
		{
			Wander();
		}
		else if (nextState == State.Running)
		{
			Run();
		}
		else if (nextState == State.Summoning)
		{
			Summon();
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

	protected override void Update()
	{
		float distance;
		if (state == State.Staying || state == State.Wandering || state == State.Running || state == State.Summoning)
		{
			if (!targetToChase)
			{
				targetToChase = Mission.GetSpecialTarget();
			}
			if (!targetToChase && state != State.Staying)
			{
				Stay();
				return;
			}
		}
		if (state == State.Staying || state == State.Wandering || state == State.Running)
		{
			if (Util.DoRarely(this, "release_check", 1f) && smart && !firstReleaseTimer.IsRunning() && (!(wave != null) || wave.specialType <= Spawner.SpecialType.Any || (float)Scene.Specials.Count((Special s) => s.GetType().Name == wave.specialType.ToString()) < wave.specialQuantity) && Util.DoRarely(this, "release_wave", releaseWaveEvery))
			{
				Summon();
				return;
			}
			if (!smart || Time.time - lastMeleeTime > 1f)
			{
				List<Human> list = Scene.Nearby<Human>(Center(), Radius() + meleeDistance);
				if (list.Count > 0)
				{
					Melee(list[0].gameObject);
					return;
				}
				if (GetIntermediateRange(targetToChase) <= meleeDistance)
				{
					Melee(targetToChase);
					return;
				}
			}
		}
		if ((state == State.Staying || state == State.Wandering) && (bool)targetToChase)
		{
			float num = Vector3.Distance(Center(), targetToChase.GetComponent<Character>() ? targetToChase.GetComponent<Character>().Center() : targetToChase.transform.position);
			if (!smart || num < desiredRange - desiredRangeVariety - urgentMoveThresholdRange || num > desiredRange + desiredRangeVariety + urgentMoveThresholdRange)
			{
				Run(true);
				return;
			}
		}
		if (state == State.Wandering || state == State.Running)
		{
			if (!smart)
			{
				goingTo = targetToChase.transform.position;
			}
			if (Vector3.Distance(goingTo, transform.position) <= 0.1f)
			{
				Stay();
				return;
			}
			UnityEngine.Debug.DrawLine(transform.position, goingTo, new Color(1f, 0f, 1f));
			if (Util.DoRarely(this, "repath", 0.2f))
			{
				bool planned = PlanMovementTo(goingTo, speedBase * (state == State.Running ? runningSpeedMod : 1f));
				if (!planned)
				{
					Wander(false);
					return;
				}
			}
		}
		if (state == State.Melee)
		{
			lastMeleeTime = Time.time;
			if (!targetToMelee)
			{
				Stay();
				return;
			}
			agent.updateRotation = false;
			RotateTo(targetToMelee.transform.position, agent.angularSpeed);
			if (GetIntermediateRange(targetToMelee) > meleeDistance * 2f)
			{
				Run(true);
				return;
			}
		}
		if (state == State.Summoning)
		{
			RotateTo(targetToChase.transform.position, agent.angularSpeed / 3f);
			agent.updateRotation = false;
		}
		base.Update();
	}

	public override void UpdateTarget(GameObject newTarget)
	{
		if (targetToChase == newTarget)
		{
			return;
		}
		if (state == State.Staying || state == State.Wandering || state == State.Running || state == State.Melee)
		{
			targetToChase = newTarget;
			Run();
			return;
		}
	}

	protected override float ModifyDamage(float damage, DamageInfo info)
	{
		return base.ModifyDamage(damage * (!(state != State.Summoning) ? summoningDamageMod : 1f), info);
	}

	private void InitAnimations()
	{
		foreach (string text in new string[4] { "idle", "walk", "run", "summon" })
		{
			animation[text].wrapMode = WrapMode.Loop;
		}
		foreach (string text2 in new string[5] { "rotate", "attack", "attack_feared", "attack_standing", "death" })
		{
			animation[text2].wrapMode = WrapMode.ClampForever;
		}
		stunable = false;
		animation["pain"].layer = 1;
		animation["pain"].blendMode = AnimationBlendMode.Additive;
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
			bool flag = speed < agent.speed * 0.3f && Mathf.Abs(Vector3.Angle(transform.forward, agent.steeringTarget)) > 30f;
			string text = flag ? "rotate" : state == State.Wandering ? "walk" : "run";
			float num = flag ? 1f : state == State.Wandering ? 3.5f : 5f;
			if (firstRun || movementSubstate != text)
			{
				PlayAnimation(text);
			}
			movementSubstate = text;
			animation[text].speed = speed / num / transform.lossyScale.z;
		}
		else if (state == State.Summoning)
		{
			if (firstRun)
			{
				string text2 = "summon";
				float num2 = animation[text2].length / summoningDuration * (float)summoningRepeat;
				PlayAnimationAndAddTime(num2, text2);
				nextStateDelay *= summoningRepeat;
			}
		}
		else if (state == State.Melee)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime((!smart && Util.Chance(60f)) ? "attack_standing" : (Util.Chance(80f) ? "attack" : "attack_feared"));
			}
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				float num3 = Util.ApplyRandomness(1f, 0.5f);
				PlayAnimationAndAddTime(num3, "death");
				nextStateDelay += 2f / num3;
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
		AnimatePainWhileMoving(!(state != State.Summoning) ? 0.4f : 1f, "pain");
	}
}
