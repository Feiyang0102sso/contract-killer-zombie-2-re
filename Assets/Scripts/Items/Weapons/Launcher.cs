using UnityEngine;

public class Launcher : Weapon
{
	private GameObject _projectilePrefab;

	public string ProjectilePrefabPath;

	private GameObject _dummyPrefab;

	public string DummyPrefabPath;

	public GameObject MuzzleFirePrefab;

	public float MuzzleVelocity = 30f;

	public float RateOfFire = 0.5f;

	public float SpreadAngle = 5f;

	public int MaxAmmo = -1;

	public int AmmoClip = 1;

	private GameObject _dummyAmmo;

	private int _totalAmmo = -1;

	private int _ammo;

	private float _explosionRadius;

	private float _cooldownTime;

	public GameObject ProjectilePrefab
	{
		get
		{
			if (_projectilePrefab == null)
			{
				_projectilePrefab = Resources.Load(ProjectilePrefabPath) as GameObject;
			}
			return _projectilePrefab;
		}
	}

	public GameObject DummyPrefab
	{
		get
		{
			if (DummyPrefabPath.Length > 0 && _dummyPrefab == null)
			{
				_dummyPrefab = Resources.Load(DummyPrefabPath) as GameObject;
			}
			return _dummyPrefab;
		}
	}

	public override bool IsExplosive()
	{
		return true;
	}

	public override int GetMaxAmmo()
	{
		return MaxAmmo;
	}

	public override int GetAmmoInBackpack()
	{
		return _totalAmmo;
	}

	public override void AddAmmo(int ammo)
	{
		if (!(AmmoClip >= 0))
		{
			return;
		}
		_ammo = Mathf.Min(_ammo + ammo, AmmoClip);
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

	public override float GetRateOfFire()
	{
		return RateOfFire;
	}

	public override float GetTimeToNextShot()
	{
		return _cooldownTime;
	}

	public override float GetExplosionRadius()
	{
		return _explosionRadius;
	}

	public override float GetSpreadAngle()
	{
		return SpreadAngle;
	}

	public override void OnReloadBegins()
	{
		Transform barrel = _owner.GetComponent<HumanWeapon>().GetBarrel(0);
		SoundManager.Play(ReloadingSound, barrel, 5f, 25f, false);
	}

	public override void OnReloadCycle()
	{
	}

	public override void OnReloadCompleted()
	{
		int num = AmmoClip - _ammo;
		num = (!(num <= _totalAmmo) ? _totalAmmo : num);
		_totalAmmo -= num;
		_ammo += num;
		ShowDummyAmmo(true);
	}

	private void ShowDummyAmmo(bool flag)
	{
		if (!((bool)DummyPrefab))
		{
			return;
		}
		if (!_dummyAmmo)
		{
			Transform barrel = _owner.GetComponent<HumanWeapon>().GetBarrel(0);
			_dummyAmmo = Object.Instantiate(DummyPrefab) as GameObject;
			_dummyAmmo.transform.position = barrel.position;
			_dummyAmmo.transform.forward = barrel.right;
			_dummyAmmo.transform.parent = barrel;
		}
		MeshRenderer[] componentsInChildren = _dummyAmmo.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = flag;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_totalAmmo = MaxAmmo;
		_ammo = AmmoClip;
		_explosionRadius = 0f;
		ExplosiveProjectile component = ProjectilePrefab.GetComponent<ExplosiveProjectile>();
		if ((bool)component)
		{
			_explosionRadius = component.ActionRadius;
		}
	}

	protected override void Start()
	{
		ShowDummyAmmo(true);
	}

	private void Update()
	{
		bool flag = _owner.IsShooting && !_owner.IsDyingOrAppearing();
		flag = flag && _owner.GetComponent<HumanAnimation>().IsReadyToShoot();
		float rateOfFire = RateOfFire;
		_cooldownTime -= Time.deltaTime;
		_cooldownTime = _cooldownTime < 0f ? 0f : _cooldownTime;
		if (_cooldownTime == 0f && flag && (bool)ProjectilePrefab && (MaxAmmo < 0 || (MaxAmmo > 0 && _ammo > 0)))
		{
			Singleton.TimeControl.ThisIsSparta(0f);
			ShowDummyAmmo(false);
			if (Weapon._damageBooster == null || (Weapon._damageBooster != null && !Weapon._damageBooster.IsRunning()))
			{
				_ammo--;
			}
			_cooldownTime = 1f / rateOfFire;
			Transform barrel = _owner.GetComponent<HumanWeapon>().GetBarrel(0);
			Vector3 normalized = (_owner.ShootingTo - barrel.position).normalized;
			if ((bool)MuzzleFirePrefab)
			{
				Vector3 right = barrel.right;
				Vector3 lhs = new Vector3(right.y, right.z, right.x);
				Vector3 vector = Vector3.Cross(lhs, right);
				lhs = Vector3.Cross(right, vector);
				Matrix4x4 identity = Matrix4x4.identity;
				identity.SetColumn(0, vector);
				identity.SetColumn(1, lhs);
				identity.SetColumn(2, right);
				Quaternion rotation = Dismemberment.QuaternionFromMatrix(identity);
				GameObject gameObject = Object.Instantiate(MuzzleFirePrefab, barrel.position, rotation) as GameObject;
				gameObject.transform.parent = barrel;
				Object.Destroy(gameObject, 10f);
			}
			GameObject gameObject2 = AbstractProjectile.Create(ProjectilePrefab, barrel.position, normalized * MuzzleVelocity, _owner, null);
			gameObject2.transform.forward = normalized;
			GuidedProjectile component = gameObject2.GetComponent<GuidedProjectile>();
			if ((bool)component)
			{
				component.InitiateGuidance(_owner.ShootingToObject, _owner.ShootingTo);
			}
			gameObject2.rigidbody.SetMaxAngularVelocity(100f);
			SoundManager.Play(ShotSound, barrel.position, 5f, 25f, false);
			_ownerAnimation.OnWeaponShot();
			Singleton.GameEvents.OnWeaponShot(_owner, this, 1);
		}
	}

	public override Launcher IsLauncher()
	{
		return this;
	}
}
