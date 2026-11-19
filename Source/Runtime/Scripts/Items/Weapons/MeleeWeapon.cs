using System;
using UnityEngine;

public class MeleeWeapon : Weapon
{
	public const float ColliderLength = 1.5f;

	public const float ColliderWidth = 0.3f;

	public float Damage = 1000f;

	public float Mass = 10f;

	public float PushbackChance = 100f;

	public float AttackSpeed = 1f;

	public string[] MeleeAnimations;

	public string[] BroadMeleeAnimations;

	public float SpreadAngle = 1f;

	public string HitSound;

	public string DrawSound;

	public string HideSound;

	public string IdleLoopSound;

	public float IdleLoopSoundFadeInTime;

	private CountdownTimer _timer = new CountdownTimer();

	private AnimationState _meleeAnimation;

	private AnimationState _meleeAnimationLeg;

	private bool _hasLoopMeleeAnimation;

	private WrapMode _meleeAnimationWrapMode = WrapMode.ClampForever;

	private int _loopIndex;

	private MeleeWeaponDamager _meleeWeapondamager;

	public override float GetSpreadAngle()
	{
		return SpreadAngle;
	}

	public bool IsPerformingAttack { get; set; }

	protected override void Start()
	{
		base.Start();
		foreach (string text in MeleeAnimations)
		{
			if (_owner.GetComponent<Animation>()[text].wrapMode == WrapMode.Loop)
			{
				_hasLoopMeleeAnimation = true;
				break;
			}
		}
		ChooseMeleeAnimation();
		IsPerformingAttack = false;
	}

	public DamageInfo.Type DamageType
	{
		get
		{
			return _hasLoopMeleeAnimation ? DamageInfo.Type.Laceration : DamageInfo.Type.Melee;
		}
	}

	public override bool IsMelee()
	{
		return true;
	}

	public override void OnDraw()
	{
		if (DrawSound != string.Empty)
		{
			SoundManager.PlayExclusive(DrawSound, 2000, 1f, 0f, false);
		}
		if (IdleLoopSound != string.Empty)
		{
			SoundManager.PlayExclusive(IdleLoopSound, 2000, 1f, 0f, true, IdleLoopSoundFadeInTime, 0f);
		}
	}

	public override void OnHide()
	{
		if (HideSound != string.Empty)
		{
			SoundManager.PlayExclusive(HideSound, false);
		}
		if (IdleLoopSound != string.Empty)
		{
			SoundManager.Stop(IdleLoopSound);
		}
	}

	public override void SetupModel(GameObject model1, GameObject model2)
	{
		GameObject gameObject = model1.GetComponentInChildren<Renderer>().gameObject;
		BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		Vector3 center = boxCollider.center;
		Vector3 size = boxCollider.size;
		size -= Vector3.one * Mathf.Min(size.x, size.y, size.z);
		size -= Vector3.one * Mathf.Min(size.x, size.y, size.z);
		size = size.normalized * Mathf.Max(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z);
		center += size.normalized * (1.5f - Vector3.Dot(size, boxCollider.size)) * 0.5f;
		boxCollider.center = center;
		gameObject.layer = Const.LayerTransparentFX;
		Vector3 rhs = Vector3.Cross(size, new Vector3(size.y, size.z, size.x));
		Vector3 vector = Vector3.Cross(size, rhs);
		boxCollider.size = size.normalized * 1.5f + rhs.normalized * 0.3f + vector.normalized * 0.3f;
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
		_meleeWeapondamager = gameObject.AddComponent<MeleeWeaponDamager>();
		_meleeWeapondamager.Weapon = this;
	}

	private void ChooseMeleeAnimation()
	{
		if (_hasLoopMeleeAnimation)
		{
			if (_meleeAnimationWrapMode == WrapMode.Loop)
			{
				_meleeAnimationWrapMode = WrapMode.Loop;
			}
			string text = string.Empty;
			foreach (string text2 in MeleeAnimations)
			{
				if (_owner.GetComponent<Animation>()[text2].wrapMode == _meleeAnimationWrapMode)
				{
					text = text2;
					break;
				}
			}
			if (text.Length == 0)
			{
				text = Util.RandomFrom(MeleeAnimations);
			}
			ResetAnimations();
			_meleeAnimation = _owner.GetComponent<Animation>()[text];
			_meleeAnimationLeg = _owner.GetComponent<Animation>()[text + "_leg"];
			_meleeAnimationWrapMode = WrapMode.Loop;
		}
		else
		{
			ResetAnimations();
			string text3 = Util.RandomFrom(MeleeAnimations);
			if (BroadMeleeAnimations != null && BroadMeleeAnimations.Length > 0 && GetNumEnemiesInFrontOfPlayer(60f) > 1)
			{
				text3 = Util.RandomFrom(BroadMeleeAnimations);
			}
			_meleeAnimation = _owner.GetComponent<Animation>()[text3];
			_meleeAnimationLeg = _owner.GetComponent<Animation>()[text3 + "_leg"];
		}
	}

	private int GetNumEnemiesInFrontOfPlayer(float coneAngle)
	{
		float unusedLocal1;
		int num = 0;
		Vector3 position = Scene.Player.transform.position;
		Vector3 forward = Scene.Player.transform.forward;
		foreach (Unit enemy in Scene.Enemies)
		{
			Vector3 position2 = enemy.transform.position;
			Vector3 vector = position2 - position;
			if (vector.sqrMagnitude <= 4f && Mathf.Abs(Vector3.Dot(forward, vector.normalized)) <= (float)Math.PI / 180f * coneAngle)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasLoopMeleeAnimation
	{
		get
		{
			return _hasLoopMeleeAnimation;
		}
	}

	private void Update()
	{
		if (IdleLoopSound != string.Empty && !SoundManager.IsPlaying(IdleLoopSound))
		{
			SoundManager.PlayExclusive(IdleLoopSound, 2000, 1f, 0f, true, IdleLoopSoundFadeInTime, 0f);
		}
		bool flag = _owner.IsShooting && !_owner.IsDyingOrAppearing() && !_owner.IsReloading;
		if (_hasLoopMeleeAnimation && !_owner.WantToShoot)
		{
			_meleeAnimationWrapMode = WrapMode.ClampForever;
			if ((bool)_meleeAnimation && _meleeAnimation.wrapMode != _meleeAnimationWrapMode)
			{
				ChooseMeleeAnimation();
			}
			_loopIndex = 0;
		}
		if (!_timer.IsRunning())
		{
			if (IsPerformingAttack)
			{
				ChooseMeleeAnimation();
				IsPerformingAttack = false;
			}
			if (flag)
			{
				if (_hasLoopMeleeAnimation && _loopIndex > 0 && _loopIndex < 2)
				{
					ChooseMeleeAnimation();
				}
				_meleeAnimation.time = 0f;
				_meleeAnimation.weight = 1f;
				_meleeAnimation.speed = AttackSpeed;
				_owner.GetComponent<Animation>().Play(_meleeAnimation.name);
				_meleeAnimationLeg.time = 0f;
				_meleeAnimationLeg.speed = AttackSpeed;
				_meleeAnimationLeg.enabled = true;
				_owner.GetComponent<Animation>().Blend(_meleeAnimationLeg.name, 0.33f, 0.1f);
				_timer.Run(_meleeAnimation.length / _meleeAnimation.speed);
				IsPerformingAttack = true;
				Singleton.GameEvents.OnWeaponShot(_owner, this, 1);
				if (ShotSound.Length > 0)
				{
					SoundManager.PlayExclusive(ShotSound, _meleeWeapondamager.transform.position, 5f, 25f, false);
				}
				_meleeWeapondamager.OnAttack();
				_loopIndex++;
			}
			else if (_owner.WantToShoot)
			{
				_meleeAnimation.time = 0f;
				_meleeAnimation.speed = 0f;
				_owner.GetComponent<Animation>().Blend(_meleeAnimation.name, 1f);
			}
			else
			{
				_meleeAnimation.time = 0f;
				_meleeAnimation.speed = 0f;
				_owner.GetComponent<Animation>().Blend(_meleeAnimation.name, 0f);
			}
		}
	}

	public override void ResetAnimations()
	{
		if (_meleeAnimation != null)
		{
			_owner.GetComponent<Animation>().Blend(_meleeAnimation.name, 0f, 0f);
			_owner.GetComponent<Animation>().Blend(_meleeAnimationLeg.name, 0f, 0f);
		}
	}

	public override MeleeWeapon IsMeleeWeapon()
	{
		return this;
	}
}
