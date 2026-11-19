using Glu.AssetBundles;
using UnityEngine;

public abstract class Weapon : Equippable
{
	public enum AmmoType
	{
		PistolCartridge,
		SMGCartridge,
		AssaultRifleCartridge,
		SniperRifleCartridge,
		MGCartridge,
		ShotgunShell,
		Grenade,
		Rocket,
		Bolt
	}

	public enum OperationType
	{
		OneHanded,
		TwoHanded,
		DualWield
	}

	private Texture2D _hudIcon;

	public string HudIconTexturePath;

	private Texture2D _shopIcon;

	private GameObject _scopeModel;

	public string ScopeModelPath;

	public float ZoomModeFov = 10f;

	public OperationType Operation;

	public AmmoType Ammunition;

	private GameObject _model;

	public string ModelPath;

	public string ModelTexturePath;

	public string ModelTextureNormalMapPath;

	protected Human _owner;

	protected Player _ownerAsPlayer;

	protected HumanAnimation _ownerAnimation;

	protected static CountdownTimer _damageBooster = null;

	public bool IsAutomatic;

	public float MaxAutofireDistance = float.PositiveInfinity;

	public float ReloadingTime = 1f;

	public string ShotSound = string.Empty;

	public string ReloadingSound = string.Empty;

	public string AnimationPrefix = string.Empty;

	public float CriticalMultiplier = 1f;

	public float DefragProbability;

	public float MutilationProbability;

	public Texture2D HudIconTexture
	{
		get
		{
			if (_hudIcon == null && HudIconTexturePath != null && HudIconTexturePath.Length > 0)
			{
				_hudIcon = Resources.Load(HudIconTexturePath, typeof(Texture2D)) as Texture2D;
			}
			return _hudIcon;
		}
	}

	public Texture2D ShopIconTexture
	{
		get
		{
			if (_shopIcon == null)
			{
				_shopIcon = AssetBundles.Load(ShopIconTexturePath) as Texture2D;
			}
			return _shopIcon;
		}
	}

	public string ShopIconTexturePath
	{
		get
		{
			return Singleton.ShopData.GetWeaponShopItemByItem(name).image;
		}
	}

	public GameObject ScopeModel
	{
		get
		{
			if (ScopeModelPath.Length > 0 && _scopeModel == null)
			{
				_scopeModel = Resources.Load(ScopeModelPath, typeof(GameObject)) as GameObject;
			}
			return _scopeModel;
		}
	}

	public GameObject Model
	{
		get
		{
			if (_model == null && ModelPath != null && ModelPath.Length > 0)
			{
				_model = Resources.Load(ModelPath, typeof(GameObject)) as GameObject;
			}
			return _model;
		}
	}

	public Human Owner
	{
		get
		{
			return _owner;
		}
	}

	public Player OwnerAsPlayer
	{
		get
		{
			return _ownerAsPlayer;
		}
	}

	public void SetOwner(GameObject root)
	{
		_owner = root.GetComponent<Human>();
		_ownerAsPlayer = _owner as Player;
		_ownerAnimation = root.GetComponent<HumanAnimation>();
	}

	public virtual GameObject InstantiateModel()
	{
		GameObject gameObject = Object.Instantiate(Model) as GameObject;
		foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
		{
			if (ModelTexturePath != null && ModelTexturePath.Length > 0)
			{
				renderer.material.SetTexture("_MainTex", Resources.Load(ModelTexturePath) as Texture2D);
			}
			if (ModelTextureNormalMapPath != null && ModelTextureNormalMapPath.Length > 0)
			{
				renderer.material.SetTexture("_BumpMap", Resources.Load(ModelTextureNormalMapPath) as Texture2D);
			}
		}
		return gameObject;
	}

	public virtual bool IsMelee()
	{
		return false;
	}

	public virtual bool IsExplosive()
	{
		return false;
	}

	protected override void Start()
	{
		base.Start();
	}

	public virtual void OnDraw()
	{
	}

	public virtual void OnHide()
	{
	}

	public virtual void SetupModel(GameObject model1, GameObject model2)
	{
	}

	public virtual void OnReloadBegins()
	{
	}

	public virtual void OnReloadCycle()
	{
	}

	public virtual void OnReloadCompleted()
	{
	}

	public virtual void ResetAnimations()
	{
	}

	public virtual int GetMaxAmmo()
	{
		return -1;
	}

	public virtual int GetAmmoInBackpack()
	{
		return -1;
	}

	public virtual void AddTotalAmmo(int ammo)
	{
	}

	public virtual int GetAmmoInClip()
	{
		return 1;
	}

	public virtual int GetAmmoClipSize()
	{
		return 1;
	}

	public virtual void AddAmmo(int ammo)
	{
	}

	public virtual float GetSpreadAngle()
	{
		return 1f;
	}

	public virtual float GetRateOfFire()
	{
		return 1f;
	}

	public virtual float GetTimeToNextShot()
	{
		return 0f;
	}

	public virtual float GetExplosionRadius()
	{
		return 0f;
	}

	public virtual bool IsShooting()
	{
		return false;
	}

	public override GameObject CreateHudIcon()
	{
		GameObject gameObject = Object.Instantiate(AssetBundles.Load("Assets/Prefabs/Hud/WeaponScrollItem.prefab")) as GameObject;
		gameObject.GetComponentInChildren<UISingleSprite>().SetTexture(HudIconTexture);
		gameObject.GetComponentInChildren<UISingleSprite>().SetSize(GUIUtils.Pixels2GUIUnits(HudIconTexture.width), GUIUtils.Pixels2GUIUnits(HudIconTexture.height));
		return gameObject;
	}

	public virtual MeleeWeapon IsMeleeWeapon()
	{
		return null;
	}

	public virtual Firearm IsFirearm()
	{
		return null;
	}

	public virtual Launcher IsLauncher()
	{
		return null;
	}

	public bool IsAmmoEmpty()
	{
		return GetAmmoInBackpack() == 0 && GetTotalAmmo() == 0;
	}

	public int GetTotalAmmo()
	{
		return GetAmmoInBackpack() + GetAmmoInClip();
	}

	public int GetTotalAmmoMax()
	{
		return GetMaxAmmo() + GetAmmoClipSize();
	}

	public float GetAmmoLeftPercent()
	{
		return (float)GetTotalAmmo() / (float)GetTotalAmmoMax() * 100f;
	}

	public static void OnDamageBoost(float time)
	{
		_damageBooster = new CountdownTimer(time);
	}

	public static bool IsDamageBoosted
	{
		get
		{
			return _damageBooster != null ? _damageBooster.IsRunning() : false;
		}
	}
}
