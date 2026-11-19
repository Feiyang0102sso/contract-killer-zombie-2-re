using System;
using UnityEngine;

public class Firearm : Weapon
{
	private struct DamageAtDistance
	{
		public float distance;

		public float percents;
	}

	private const float BouncingFriction = 0.33f;

	private const float BouncingRestitution = 0.66f;

	private const float RicochetAngle0 = 0f;

	private const float RicochetProbability0 = 0.05f;

	private const float RicochetAngle1 = (float)Math.PI / 4f;

	private const float RicochetProbability1 = 0f;

	public Material TracerMaterial;

	public Mesh TracerMesh;

	public float TracerWidth = 1f;

	public float SmokeWidth = 0.1f;

	public float SmokeTime = 0.5f;

	public float TracerLengthScale = 1f;

	public float TracerProgressiveWidth = 2f;

	public float TracerProgressiveDistance = 100f;

	public int NumPellets = 1;

	public float MuzzleVelocity = 150f;

	public float BulletMass = 0.02f;

	public float RateOfFire = 2f;

	public int MaxAmmo = -1;

	public int AmmoClip = 10;

	public float SpreadAngleRunning = 6f;

	public float SpreadAngleStanding = 3f;

	public float SpreadAngleAiming = 1.5f;

	public float Damage = 100f;

	public float StunProbability;

	public float DistanceWhenDamagePercent100 = 100f;

	public float DistanceWhenDamagePercent75 = 101f;

	public float DistanceWhenDamagePercent50 = 102f;

	public float DistanceWhenDamagePercent25 = 103f;

	public float DistanceWhenDamagePercent0 = 104f;

	public int BurstNumShots = 1;

	public float BurstRateOfFire = 1f;

	private DamageAtDistance[] DamageByDistance;

	private int _ammo;

	private int _totalAmmo = -1;

	private float _cooldownTime;

	private int _burstShotsLeft;

	private Vector3 _currPos = Vector3.zero;

	private Vector3 _prevPos = Vector3.zero;

	private Vector3 _currVel = Vector3.zero;

	private float _currSpreadAngle;

	private Vector3 VectorZero = new Vector3(0f, 0f, 0f);

	private Vector3 VectorUniform = new Vector3(1f, 1f, 1f);

	public bool BurstModeEnabled
	{
		get
		{
			return BurstNumShots > 0;
		}
	}

	public override void OnReloadBegins()
	{
		bool isManualShotgun = NumPellets > 1 && !IsAutomatic;
		if (isManualShotgun)
		{
			SoundManager.Play("shotgun_reload_start", GetBarrel(0).position, 5f, 25f, false);
		}
		else
		{
			SoundManager.Play(ReloadingSound, GetBarrel(0), 5f, 25f, false);
		}
	}

	public override void OnReloadCycle()
	{
		bool reloadOneRound = NumPellets > 1;
		if (reloadOneRound)
		{
			if (MaxAmmo > 0)
			{
				if (_totalAmmo > 0)
				{
					_ammo++;
					_totalAmmo--;
				}
			}
			else
			{
				_ammo++;
			}
			_ammo = _ammo > AmmoClip ? AmmoClip : _ammo;
			SoundManager.Play(Util.Chance(50f) ? "shotgun_reload_bullet1" : "shotgun_reload_bullet2", GetBarrel(0).position, 5f, 25f, false);
		}
	}

	public override void OnReloadCompleted()
	{
		bool reloadOneRound = NumPellets > 1 && AnimationPrefix == "shotgun";
		if (!reloadOneRound)
		{
			if (MaxAmmo > 0)
			{
				_totalAmmo += _ammo;
				_ammo = AmmoClip <= _totalAmmo ? AmmoClip : _totalAmmo;
				_totalAmmo -= _ammo;
			}
			else
			{
				_ammo = AmmoClip;
			}
		}
	}

	public override int GetMaxAmmo()
	{
		return MaxAmmo;
	}

	public override int GetAmmoInBackpack()
	{
		return _totalAmmo;
	}

	public override void AddTotalAmmo(int ammo)
	{
		if (!(MaxAmmo >= 0))
		{
			return;
		}
		_totalAmmo = Mathf.Min(_totalAmmo + ammo, MaxAmmo);
	}

	public override int GetAmmoInClip()
	{
		return _ammo;
	}

	public override int GetAmmoClipSize()
	{
		return AmmoClip;
	}

	public override void AddAmmo(int ammo)
	{
		if (!(AmmoClip >= 0))
		{
			return;
		}
		_ammo = Mathf.Min(_ammo + ammo, AmmoClip);
	}

	public override float GetSpreadAngle()
	{
		return _currSpreadAngle;
	}

	public override float GetRateOfFire()
	{
		return RateOfFire;
	}

	public override float GetTimeToNextShot()
	{
		return _cooldownTime;
	}

	public override bool IsShooting()
	{
		return BurstModeEnabled ? BurstShotsLeft < BurstNumShots && BurstShotsLeft > 0 : false;
	}

	protected override void Awake()
	{
		base.Awake();
		DamageByDistance = new DamageAtDistance[5];
		DamageByDistance[0].percents = 100f;
		DamageByDistance[0].distance = DistanceWhenDamagePercent100;
		DamageByDistance[1].percents = 75f;
		DamageByDistance[1].distance = DistanceWhenDamagePercent75;
		DamageByDistance[2].percents = 50f;
		DamageByDistance[2].distance = DistanceWhenDamagePercent50;
		DamageByDistance[3].percents = 25f;
		DamageByDistance[3].distance = DistanceWhenDamagePercent25;
		DamageByDistance[4].percents = 0f;
		DamageByDistance[4].distance = DistanceWhenDamagePercent0;
		if (DamageByDistance != null && DamageByDistance.Length > 0)
		{
			for (int i = 0; i < DamageByDistance.Length; i++)
			{
				if (i > 0)
				{
				}
			}
		}
		if (SpreadAngleStanding >= SpreadAngleRunning)
		{
			SpreadAngleStanding = SpreadAngleRunning - 0.001f;
		}
		_ammo = AmmoClip;
		_totalAmmo = MaxAmmo;
		_currPos = (_prevPos = transform.position);
		_currSpreadAngle = SpreadAngleStanding;
		_burstShotsLeft = ((BurstNumShots > 1) ? BurstNumShots : 0);
		SoundManager.Preload(ShotSound);
		SoundManager.Preload(ReloadingSound);
		Scene.ProjectileManager.AddFirearm(this);
	}

	private void OnDestroy()
	{
		if ((bool)Scene.inst && (bool)Scene.ProjectileManager)
		{
			Scene.ProjectileManager.RemoveFirearm(this);
		}
	}

	public static float GetAngle(Vector3 v1, Vector3 v2, Vector3 axis)
	{
		float num = Vector3.Dot(v1, v2);
		if (num > 1f)
		{
			num = 0f;
		}
		else if (num < -1f)
		{
			num = (float)Math.PI;
		}
		else
		{
			num = 57.29578f * Mathf.Acos(num);
		}
		Vector3 lhs = Vector3.Cross(v1, v2);
		if (!Mathf.Approximately(lhs.sqrMagnitude, 0f))
		{
			num *= Mathf.Sign(Vector3.Dot(lhs, axis));
		}
		return num;
	}

	protected void Update()
	{
		_prevPos = _currPos;
		_currPos = _owner ? _owner.transform.position : transform.position;
		_currVel = (_currPos - _prevPos) / Time.deltaTime;
		float unusedAngle1;
		float unusedAngle2;
		float num = (_owner is Player ? (_owner as Player).IsZoomMode() : false) ? SpreadAngleAiming : SpreadAngleStanding;
		if (_currVel.magnitude <= 1f)
		{
			_currSpreadAngle -= 5f * (SpreadAngleRunning - num) * Time.deltaTime;
		}
		else
		{
			_currSpreadAngle += 7.5f * (SpreadAngleRunning - num) * Time.deltaTime;
		}
		_currSpreadAngle = Mathf.Clamp(_currSpreadAngle, num, SpreadAngleRunning);
	}

	public bool FireTrigger
	{
		get
		{
			bool ready = _owner.Inventory.Weapon == this;
			ready = ready && _owner.IsShooting && !_owner.IsDyingOrAppearing() && !_owner.IsReloading;
			ready = ready && _owner.GetComponent<HumanAnimation>().IsReadyToShoot();
			return ready;
		}
	}

	public bool BurstTrigger
	{
		get
		{
			return BurstModeEnabled && BurstShotsLeft < BurstNumShots && BurstShotsLeft > 0;
		}
	}

	public int BurstShotsLeft
	{
		get
		{
			return _burstShotsLeft;
		}
		set
		{
			_burstShotsLeft = value;
		}
	}

	public float CooldownTime
	{
		get
		{
			return _cooldownTime;
		}
		set
		{
			_cooldownTime = value;
		}
	}

	public Vector3 ShootingTo
	{
		get
		{
			return _owner.ShootingTo;
		}
	}

	public int NumBarrels
	{
		get
		{
			return !(Operation != OperationType.DualWield) ? 2 : 1;
		}
	}

	public Transform GetBarrel(int barrelID)
	{
		return _owner.GetComponent<HumanWeapon>().GetBarrel(barrelID);
	}

	public void OnCreateProjectile(int projectileId)
	{
		if (_owner is Player)
		{
			Scene.Camera.OnWeaponShot();
			switch (Ammunition)
			{
			case AmmoType.SniperRifleCartridge:
				Scene.Camera.Shake(0.3f, 0.055f, 50f, 2.5f);
				Scene.Camera.Flinch(0.4f, new Vector3(UnityEngine.Random.Range(-1f, -2f), UnityEngine.Random.Range(-1f, 1f), 0f));
				break;
			case AmmoType.ShotgunShell:
				Scene.Camera.Shake(0.25f, 0.0375f, 50f, 2.5f);
				break;
			}
		}
		_ownerAnimation.OnWeaponShot();
		Singleton.GameEvents.OnWeaponShot(_owner, this, NumPellets);
		if (Weapon._damageBooster == null || (Weapon._damageBooster != null && !Weapon._damageBooster.IsRunning()))
		{
			_ammo--;
			_ammo = (!(_ammo >= 0) ? 0 : _ammo);
		}
		_burstShotsLeft--;
		_burstShotsLeft = (!(_burstShotsLeft >= 0) ? 0 : _burstShotsLeft);
		SoundManager.Play(ShotSound, GetBarrel(0).position, 5f, 25f, false);
	}

	public void OnCollideProjectile(GameObject hitObject, ref RaycastHit hit, float travelledDistance, ref Vector3 projectilePos, ref Vector3 projectileVel)
	{
		float num = Damage;
		float stunChance_ = StunProbability;
		if (DamageByDistance != null && DamageByDistance.Length > 0)
		{
			int num2 = DamageByDistance.Length;
			if (travelledDistance < DamageByDistance[0].distance)
			{
				float num3 = travelledDistance / DamageByDistance[0].distance;
				num = Damage * (1f - num3) + Damage * DamageByDistance[0].percents * num3 * 0.01f;
			}
			else if (travelledDistance > DamageByDistance[num2 - 1].distance)
			{
				num = Damage * DamageByDistance[num2 - 1].percents * 0.01f;
			}
			else
			{
				for (int i = 0; i < num2 - 1; i++)
				{
					if (travelledDistance >= DamageByDistance[i].distance && travelledDistance < DamageByDistance[i + 1].distance)
					{
						float num4 = (travelledDistance - DamageByDistance[i].distance) / (DamageByDistance[i + 1].distance - DamageByDistance[i].distance);
						num = Damage * (1f - num4) * DamageByDistance[i].percents * 0.01f + Damage * num4 * DamageByDistance[i + 1].percents * 0.01f;
						break;
					}
				}
			}
			if (travelledDistance > DistanceWhenDamagePercent75)
			{
				stunChance_ = 0f;
			}
		}
		if ((bool)hitObject.rigidbody && !hitObject.rigidbody.isKinematic)
		{
			hitObject.rigidbody.velocity += projectileVel * BulletMass / hitObject.rigidbody.mass;
			if (UnityEngine.Random.Range(0f, 100f) <= DefragProbability)
			{
				Dismemberment dismemberment = hitObject.FindComponentUpwards<Dismemberment>();
				if ((bool)dismemberment)
				{
					dismemberment.Dismember(hitObject);
				}
			}
		}
		HitInfo info = new HitInfo(num, DamageInfo.Type.Bullet, Owner, Scene.MainCamera.transform.forward * projectileVel.magnitude * BulletMass, DefragProbability, MutilationProbability, stunChance_, CriticalMultiplier, projectilePos, hitObject, 0f);
		Character character = hitObject.FindComponentUpwards<Character>();
		if (num > 0f && character != null)
		{
			character.OnDamage(info);
		}
		bool isCharacter = character != null ? character.LocalHitEffects() : false;
		Scene.EffectsManager.OnHit(hit.collider, hit.point, hit.normal, projectileVel, false, isCharacter, info);
	}

	public override Firearm IsFirearm()
	{
		return this;
	}
}
