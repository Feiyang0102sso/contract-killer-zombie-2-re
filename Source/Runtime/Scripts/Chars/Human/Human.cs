using System;
using UnityEngine;
using UnityEngine.AI;

public class Human : Character
{
	private const float MinSqrDistanceToBeInDanger = 900f;

	private const float RotationAngularSpeed = 360f;

	public float baseSpeed = 4f;

	public float meleeRecoveryDuration = 0.6f;

	public float meleeRecoveryMinSpeedMod = 0.4f;

	public float regenerationPerSecond = 0.01f;

	public float minHealthPercent;

	public float meleeAttackDistance = 1.5f;

	public float attractiveness = 1f;

	public bool isEvil;

	[NonSerialized]
	public float lastDamageTime;

	public Action<DamageInfo> OnHurtExternal = delegate
	{
	};

	protected HumanAnimation _humanAnimation;

	protected HumanInventory inventory;

	protected bool wantToShoot;

	protected bool isInDanger;

	protected bool isShooting;

	protected Vector3 shootingTo;

	protected GameObject shootingToObject;

	private UnityEngine.AI.NavMeshAgent agent;

	protected float lastMeleeDamageTime;

	public GameObject ObjectToThrow;

	public Consumable ConsumableToThrow;

	private string[] _reloadingSequence;

	private float _reloadingSpeed = 1f;

	private int _reloadingPhase;

	private bool _delayedReload;

	public HumanInventory Inventory
	{
		get
		{
			if (!inventory)
			{
				inventory = GetComponent<HumanInventory>();
			}
			return inventory;
		}
	}

	public bool WantToShoot
	{
		get
		{
			return wantToShoot;
		}
	}

	public bool IsInDanger
	{
		get
		{
			return isInDanger;
		}
	}

	public bool IsShooting
	{
		get
		{
			return isShooting;
		}
	}

	public bool IsReloading
	{
		get
		{
			return state == State.Reloading;
		}
	}

	public Vector3 ShootingTo
	{
		get
		{
			return shootingTo;
		}
	}

	public GameObject ShootingToObject
	{
		get
		{
			return shootingToObject;
		}
	}

	public float LastMeleeDamageTime
	{
		get
		{
			return lastMeleeDamageTime;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Human");
		_humanAnimation = GetComponent<HumanAnimation>();
		GameObject gameObject = new GameObject("Navigation");
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = Vector3.zero;
		agent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
		agent.radius = characterController.radius + navigationRadiusIncrement;
		agent.height = characterController.height;
		agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance;
	}

	protected override void Start()
	{
		ChangeState(State.Moving);
		base.Start();
	}

	protected override void Update()
	{
		if (state == State.Dying || state == State.Dead)
		{
			speed = 0f;
		}
		if ((state == State.Moving && (bool)Inventory.Weapon && Inventory.Weapon.GetAmmoInClip() == 0) || _delayedReload)
		{
			Reload();
		}
		if (state == State.Moving && health < healthTotal)
		{
			health = Mathf.Min(healthTotal, health + healthTotal * Time.deltaTime * regenerationPerSecond);
		}
		if (state == State.Throwing)
		{
			RotateTo(Scene.MainCamera.transform.position + Scene.MainCamera.transform.forward * 100f, 360f);
		}
		agent.transform.localPosition = Vector3.zero;
		if (Util.DoRarely(this, "human_dangers", 0.5f))
		{
			UpdateDangers();
		}
		base.Update();
	}

	private void UpdateDangers()
	{
		isInDanger = false;
		if (!(this is Player))
		{
			return;
		}
		foreach (Unit attackingEnemy in Scene.AttackingEnemies)
		{
			float sqrMagnitude = (attackingEnemy.transform.position - transform.position).sqrMagnitude;
			if (sqrMagnitude < 900f)
			{
				isInDanger = true;
				break;
			}
		}
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		if (state == State.Reloading && stateProgress > 0.9f)
		{
			Inventory.Weapon.OnReloadCompleted();
		}
		if (state == State.Appearing && this is Player)
		{
			Scene.Camera.SetEffect(PostEffectManager.PostEffect.Normal);
		}
		if (newState == State.Dead)
		{
			base.ChangeState(newState, nextState_, nextStateDelay_);
			if ((bool)Scene.Mission && !Scene.Mission.ShouldResurrect(gameObject))
			{
				UnityEngine.Object.Destroy(gameObject);
				return;
			}
			speed = 0f;
			health = healthTotal;
			ChangeState(State.Appearing, State.Moving, 0f);
			Util.ExplosionDealsDamage(3000f, 0f, 0.1f, Center(), 10f, this, null, 150f, (Character c) => !(c is Human));
			return;
		}
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void proceedToNextState()
	{
		base.proceedToNextState();
	}

	public bool IsReadyToThrow()
	{
		return state == State.Moving || state == State.Pain;
	}

	public void Throw(GameObject objectToThrow, Consumable consumableToThrow)
	{
		ObjectToThrow = objectToThrow;
		ConsumableToThrow = consumableToThrow;
		if (state == State.Moving || state == State.Pain)
		{
			ChangeState(State.Throwing, State.Moving, 0f);
		}
	}

	public void Reload()
	{
		if (state != State.Moving && state != State.Pain)
		{
			return;
		}
		if (Inventory.Weapon.GetAmmoInClip() == Inventory.Weapon.GetAmmoClipSize())
		{
			return;
		}
		if (Inventory.Weapon.GetMaxAmmo() == -1 || Inventory.Weapon.GetAmmoInBackpack() > 0)
		{
			if (_humanAnimation.IsShootingAnimationCompleted)
			{
				Inventory.Weapon.OnReloadBegins();
				ChangeState(State.Reloading, state, 0f);
				_delayedReload = false;
			}
			else
			{
				_delayedReload = true;
			}
		}
	}

	public override bool CanBeHurt()
	{
		return state != State.Appearing && base.CanBeHurt();
	}

	protected override void OnHurt(DamageInfo info)
	{
		lastDamageTime = Time.time;
		OnHurtExternal(info);
		if (info.type == DamageInfo.Type.Melee)
		{
			lastMeleeDamageTime = Time.time;
		}
		if ((bool)_humanAnimation && this is Player)
		{
			_humanAnimation.AnimateDamage(transform.worldToLocalMatrix.MultiplyVector(info.momentum.normalized * -1f));
		}
		base.OnHurt(info);
	}

	protected override float ModifyDamage(float damage, DamageInfo info)
	{
		DamageBooster component = GetComponent<DamageBooster>();
		if (component != null)
		{
			damage *= component.conf.defenseBoost;
		}
		if (minHealthPercent != 0f && health - damage < minHealthPercent * healthTotal)
		{
			damage = health - minHealthPercent * healthTotal;
		}
		return base.ModifyDamage(damage, info);
	}

	public bool AllowExternalAnimation()
	{
		return state == State.Moving || state == State.Pain || state == State.Reloading || state == State.Melee;
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Appearing)
		{
			if (firstRun)
			{
				PlayAnimationAndAddTime("stand_up");
				if (this is Player)
				{
					Scene.Camera.SetEffect(PostEffectManager.PostEffect.Resurrect);
				}
			}
		}
		else if (state == State.Moving)
		{
			if (firstRun && (bool)Inventory.Weapon)
			{
				GetComponent<Animation>().Stop(Inventory.Weapon.AnimationPrefix + "_reload");
			}
		}
		else if (state == State.Reloading)
		{
			if (firstRun)
			{
				_reloadingPhase = 0;
				_reloadingSequence = _humanAnimation.ReloadingSequence;
				if (_reloadingSequence.Length == 1)
				{
					string text = _reloadingSequence[0];
					float num = GetComponent<Animation>()[text].length / Inventory.Weapon.ReloadingTime;
					PlayAnimationAndAddTime(0.01f, num, text);
				}
				else if (_reloadingSequence.Length > 2)
				{
					float num2 = 0f;
					foreach (string text2 in _reloadingSequence)
					{
						num2 += GetComponent<Animation>()[text2].length;
					}
					float reloadingTime = Inventory.Weapon.ReloadingTime;
					reloadingTime = (float)(2 + Inventory.Weapon.GetAmmoClipSize() - Inventory.Weapon.GetAmmoInClip()) * reloadingTime / (float)(2 + Inventory.Weapon.GetAmmoClipSize());
					_reloadingSpeed = num2 / reloadingTime;
					PlayAnimation(0.1f, _reloadingSpeed, _reloadingSequence[0]);
					nextStateDelay = reloadingTime;
					timeSpentInState = 0f;
				}
			}
			else if (_reloadingSequence.Length > 0 && _reloadingPhase < _reloadingSequence.Length)
			{
				if (GetComponent<Animation>()[_reloadingSequence[_reloadingPhase]].time >= GetComponent<Animation>()[_reloadingSequence[_reloadingPhase]].length)
				{
					if (_reloadingPhase > 0)
					{
						Inventory.Weapon.OnReloadCycle();
					}
					if ((WantToShoot || Inventory.Weapon.GetAmmoInClip() == Inventory.Weapon.GetAmmoClipSize()) && _reloadingPhase > 0 && _reloadingPhase < _reloadingSequence.Length - 1)
					{
						_reloadingPhase = _reloadingSequence.Length - 1;
						nextStateDelay = GetComponent<Animation>()[_reloadingSequence[_reloadingPhase]].length / _reloadingSpeed;
						timeSpentInState = 0f;
						PlayAnimation(0f, _reloadingSpeed, _reloadingSequence[_reloadingPhase]);
						if (_reloadingSequence.Length > 2)
						{
							GetComponent<Animation>().Blend(_reloadingSequence[_reloadingSequence.Length - 2], 0f, 0.1f);
						}
					}
					else
					{
						_reloadingPhase++;
						PlayAnimation(0f, _reloadingSpeed, _reloadingSequence[_reloadingPhase]);
						if (_reloadingPhase == _reloadingSequence.Length - 1)
						{
							GetComponent<Animation>().Blend(_reloadingSequence[_reloadingPhase], 0f, Mathf.Clamp(nextStateDelay - timeSpentInState, 0f, 10f));
						}
					}
				}
			}
			else if (_reloadingPhase == _reloadingSequence.Length)
			{
				timeSpentInState = nextStateDelay;
				if (_reloadingPhase == _reloadingSequence.Length - 1)
				{
					GetComponent<Animation>().Blend(_reloadingSequence[_reloadingPhase], 0f, 0.1f);
				}
			}
		}
		else if (state == State.Pain)
		{
			if (firstRun)
			{
				PlayAnimation(3f, "carbine_pain");
			}
		}
		else if (state == State.Dying)
		{
			if (firstRun)
			{
				GetComponent<Animation>().Stop();
				PlayAnimationAndAddTime("death");
				if (this is Player)
				{
					Scene.Camera.SetEffect(PostEffectManager.PostEffect.Dying);
				}
			}
		}
		else if (state == State.Dead)
		{
		}
		else if (state == State.Throwing)
		{
			if (firstRun)
			{
				GetComponent<Animation>().Stop();
				PlayAnimation(0f, 1f, "throw");
				nextStateDelay += GetComponent<Animation>()["throw"].length;
				speed = 0f;
			}
		}
		else
		{
			base.Animate(firstRun);
		}
	}

	public virtual void Aim()
	{
	}

	public void OnSwitchWeapon(Weapon weapon)
	{
		_delayedReload = false;
	}
}
