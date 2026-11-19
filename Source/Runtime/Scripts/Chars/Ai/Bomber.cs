using System.Collections.Generic;
using UnityEngine;

public class Bomber : Special
{
	public float stunChanceMod = 30f;

	public float explosionDamage = 800f;

	public float explosionDamageRandomness = 0.1f;

	public float explosionRadius = 5f;

	public float preparationDuration = 1f;

	public float explosionDistance = 0.8f;

	private GameObject target;

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Bomber");
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

	private void PreExplode()
	{
		StopMovement();
		ChangeState(State.Preparing, State.Exploding, 0f);
	}

	private void Explode(bool die)
	{
		Util.ExplosionDealsDamage(explosionDamage, 0f, explosionDamageRandomness, Center(), explosionRadius, this, null, 300f, (Character c) =>
		{
			if (c is Player)
			{
				Scene.Camera.OnBomber();
			}
			return c != this;
		});
		Scene.EffectsManager.DrawBomberBlow(transform.position);
		needRagdoll = true;
		if (die)
		{
			OnDying(null);
		}
	}

	protected override void proceedToNextState()
	{
		if (nextState == State.Staying)
		{
			Stay();
		}
		else if (nextState == State.Moving)
		{
			Move();
		}
		else if (nextState == State.Preparing)
		{
			PreExplode();
		}
		else if (nextState == State.Exploding)
		{
			Explode(true);
		}
		else
		{
			base.proceedToNextState();
		}
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void Update()
	{
		if (state == State.Staying || state == State.Moving)
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
			if (Util.DoRarely(this, "explosion_check", 0.5f))
			{
				List<Human> list = Scene.Nearby<Human>(Center(), Radius() + explosionDistance);
				if (list.Count > 0)
				{
					PreExplode();
					return;
				}
				if (GetIntermediateRange(target) <= explosionDistance)
				{
					PreExplode();
					return;
				}
				if (updatesDoneInState > 0 && speed < speedBase * 0.1f)
				{
					float distance = explosionRadius / 2f;
					List<Human> list2 = Scene.Nearby<Human>(Center(), distance);
					if (list2.Count > 0)
					{
						PreExplode();
						return;
					}
				}
			}
			if (Util.DoRarely(this, "moving_repath", 0.5f))
			{
				PlanMovementTo(target.transform.position, speedBase);
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

	protected override void OnDying(DamageInfo info)
	{
		if (info != null)
		{
			Explode(false);
		}
		base.OnDying(info);
	}

	public override void OnDamage(DamageInfo info)
	{
		info.stunChance *= stunChanceMod / 100f;
		base.OnDamage(info);
	}

	private void InitAnimations()
	{
		foreach (string text in new string[2] { "idle", "walk" })
		{
			GetComponent<Animation>()[text].wrapMode = WrapMode.Loop;
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
		else if (state == State.Moving)
		{
			string text = "walk";
			if (firstRun)
			{
				PlayAnimation(text);
			}
			GetComponent<Animation>()[text].speed = speed / 2.8f / transform.lossyScale.z;
		}
		else if (state == State.Preparing)
		{
			if (firstRun)
			{
				string text2 = "explode";
				float num = GetComponent<Animation>()[text2].length / preparationDuration;
				PlayAnimationAndAddTime(text2);
			}
		}
		else if (state == State.Pain)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime(1.5f, "pain");
			}
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				nextStateDelay += 0.1f;
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

	protected override void OnCreateRagdoll(GameObject ragdoll)
	{
		if ((bool)RagdollManager.instance)
		{
			RagdollManager.instance.Manage(ragdoll);
		}
	}
}
