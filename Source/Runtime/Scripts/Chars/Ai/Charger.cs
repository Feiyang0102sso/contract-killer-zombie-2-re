using System.Collections.Generic;
using UnityEngine;

public class Charger : Special
{
	private const float chargeMaxRangeThreshold = 1.3f;

	public float chargeDamage = 500f;

	public float chargeMinRange = 0.5f;

	public float chargeMaxRange = 15f;

	public float chargeSpeedMod = 2.5f;

	public float chargeDelay = 10f;

	public float stopDuration = 4f;

	public float stopDurationRandomness = 0.3f;

	public float stopDamageMod = 1.5f;

	public float meleeDistance = 0.5f;

	public float meleeDamage = 500f;

	private GameObject target;

	private Vector3 chargingTo;

	private CountdownTimer chargeTimer = new CountdownTimer();

	private GameObject targetToMelee;

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Charger");
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
		ChangeState(State.Staying);
	}

	private void Move()
	{
		ChangeState(State.Moving);
	}

	private void PreCharge()
	{
		StopMovement();
		SoundManager.Play("Charger/pre_charge", transform.position, 5f, 25f, false);
		ChangeState(State.Preparing, State.Running, 0f);
	}

	private void Charge()
	{
		if (!CanCharge())
		{
			Move();
			return;
		}
		chargingTo = transform.position + (target.transform.position - transform.position).normalized * 100f;
		UnityEngine.AI.NavMeshHit hit;
		float unusedChargeValue;
		UnityEngine.AI.NavMesh.Raycast(transform.position, chargingTo, out hit, agent.walkableMask);
		if (hit.hit)
		{
			chargingTo = hit.position;
		}
		chargeTimer.Run(chargeDelay);
		StopMovement();
		ChangeState(State.Running);
	}

	private void Reload()
	{
		SoundManager.Play("Charger/stop", transform.position, 5f, 25f, false);
		SetHealthBarColor(true);
		StopMovement();
		ChangeState(State.Reloading, State.Moving, 0f);
	}

	private void Melee(GameObject target_)
	{
		float unusedLocal1;
		targetToMelee = target_ ? target_ : target;
		Invoke("MeleeHit", 0.8f);
		StopMovement();
		ChangeState(State.Melee, State.Melee, 0f);
	}

	private void MeleeHit()
	{
		SoundManager.Play("Charger/melee", transform.position, 5f, 25f, false);
		float unusedHitRange;
		List<Character> list = Scene.Nearby<Character>(Center(), Radius() + meleeDistance * 2f);
		Character character = targetToMelee ? targetToMelee.GetComponent<Character>() : null;
		if (character != null && !list.Contains(character) && GetIntermediateRange(character.gameObject) <= meleeDistance * 2f)
		{
			list.Add(character);
		}
		bool flag = false;
		foreach (Character item in list)
		{
			float unusedHitAngle;
			if (item == this)
			{
				continue;
			}
			Vector3 to = item.transform.position - transform.position;
			if (Mathf.Abs(Vector3.Angle(transform.forward, to)) > 90f)
			{
				continue;
			}
			item.OnDamage(new HitInfo(meleeDamage, DamageInfo.Type.Melee, this, mass * to.normalized, 50f, 15f, 70f, 1f, item.Center(), null, 100f));
			flag |= item is Player;
		}
		Scene.Camera.OnCharger(flag);
	}

	protected override void proceedToNextState()
	{
		if (state == State.Reloading)
		{
			SetHealthBarColor(false);
		}
		if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Moving)
		{
			Move();
		}
		else if (nextState == State.Running)
		{
			Charge();
		}
		else if (nextState == State.Reloading)
		{
			Reload();
		}
		else if (nextState == State.Melee)
		{
			Melee(null);
		}
		else
		{
			base.proceedToNextState();
		}
	}

	private bool CanCharge()
	{
		return !chargeTimer.IsRunning() && Vector3.Distance(transform.position, target.transform.position).InSegment(chargeMinRange, chargeMaxRange * (!(state != State.Preparing) ? 1.3f : 1f)) && !Util.AnyObstacles(Center(), target.transform.position + Vector3.up * Height() / 2f) && !Util.AnyNavObstacles(transform.position, target.transform.position);
	}

	protected override void Update()
	{
		if (state == State.Staying || state == State.Moving || state == State.Preparing || state == State.Running)
		{
			if (!target)
			{
				target = Mission.GetSpecialTarget();
			}
			if (!target && state != State.Staying)
			{
				Stay();
				return;
			}
		}
		if (state == State.Staying)
		{
			if ((bool)target)
			{
				Move();
				return;
			}
		}
		else if (state == State.Moving)
		{
			List<Human> list = Scene.Nearby<Human>(Center(), Radius() + meleeDistance);
			if (list.Count > 0)
			{
				Melee(list[0].gameObject);
				return;
			}
			if (GetIntermediateRange(target) <= meleeDistance)
			{
				Melee(target);
				return;
			}
			if (Util.DoRarely(this, "charge_check", 0.3f) && CanCharge())
			{
				PreCharge();
				return;
			}
			if (Util.DoRarely(this, "moving_repath", 0.5f))
			{
				PlanMovementTo(target.transform.position, speedBase);
			}
		}
		else if (state == State.Preparing)
		{
			agent.updateRotation = false;
			RotateTo(target.transform.position, agent.angularSpeed);
		}
		else if (state == State.Running)
		{
			foreach (Character item in Scene.Nearby<Character>(Center(), Radius() + 0.3f))
			{
				if (item == this)
				{
					continue;
				}
				if (Util.DoRarely(item, "charge_damage", 1f))
				{
					item.OnDamage(new HitInfo(chargeDamage, DamageInfo.Type.Melee, this, mass * 2f / 3f * (item.Center() - Center()).normalized, 50f, 10f, 80f, 1f, item.Center(), null, 100f));
				}
				if (item is Human)
				{
					Scene.Camera.OnCharger(item is Player);
					Reload();
					return;
				}
			}
			if (Util.ZeroY(transform.position - chargingTo).magnitude < Radius() + 0.1f)
			{
				Scene.Camera.OnCharger(false);
				Reload();
				return;
			}
			agent.updateRotation = false;
			Vector3 normalized = (chargingTo - transform.position).normalized;
			RotateTo(chargingTo, agent.angularSpeed);
			if (Util.DoRarely(this, "charging_repath", 0.5f))
			{
				PlanMovementTo(chargingTo, speedBase * chargeSpeedMod);
			}
			UnityEngine.Debug.DrawLine(transform.position, chargingTo, Color.red);
		}
		else if (state == State.Melee)
		{
			if (!targetToMelee)
			{
				Stay();
				return;
			}
			agent.updateRotation = false;
			RotateTo(targetToMelee.transform.position, agent.angularSpeed);
			if (GetIntermediateRange(targetToMelee) > meleeDistance * 2f)
			{
				Move();
				return;
			}
		}
		base.Update();
	}

	public override void UpdateTarget(GameObject newTarget)
	{
		if (target == newTarget)
		{
			return;
		}
		if (state == State.Staying || state == State.Moving)
		{
			target = newTarget;
			Move();
			return;
		}
	}

	protected override float ModifyDamage(float damage, DamageInfo info)
	{
		return base.ModifyDamage(damage * (!(state != State.Reloading) ? stopDamageMod : 1f), info);
	}

	private void InitAnimations()
	{
		stunable = false;
		foreach (string text in new string[4] { "stop", "melee1", "melee2", "stop2_end" })
		{
			GetComponent<Animation>()[text].wrapMode = WrapMode.ClampForever;
		}
		foreach (string text2 in new string[3] { "idle", "walk", "charge" })
		{
			GetComponent<Animation>()[text2].wrapMode = WrapMode.Loop;
		}
		GetComponent<Animation>()["melee2"].speed = 0.6f;
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
		else if (state == State.Moving || state == State.Running)
		{
			bool flag = state == State.Running;
			string text = flag ? "charge" : "walk";
			float num = flag ? 5.2f : 4.2f;
			if (firstRun)
			{
				PlayAnimation(text);
			}
			GetComponent<Animation>()[text].speed = speed / num / transform.lossyScale.z;
		}
		else if (state == State.Preparing)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("pre_charge");
			}
		}
		else if (state == State.Reloading)
		{
			if (firstRun)
			{
				float num2 = Util.ApplyRandomness(stopDuration, stopDurationRandomness);
				PlayAnimationAndAddTime(false, -1f, 1f, "stop2_start");
				string text2 = "stop2_middle";
				int num3 = Mathf.CeilToInt(num2 / GetComponent<Animation>()[text2].length);
				for (int i = 0; i < num3; i++)
				{
					PlayAnimationAndAddTime(true, -1f, 1f, text2);
				}
				PlayAnimationAndAddTime(true, -1f, 1f, "stop2_end");
			}
		}
		else if (state == State.Melee)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("melee1", "melee2");
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
	}
}
