using UnityEngine;

public class ExplosiveProjectile : GuidedProjectile
{
	public GameObject EffectPrefab;

	public GameObject Smoke;

	public float SmokeLifetime;

	public float ActionDelay = 1f;

	public float ActionRadius = 7.5f;

	public string ActionSound = string.Empty;

	public bool AutoAiming;

	public float MinDamage = 50f;

	public float MaxDamage = 750f;

	public float DamageRandomness = 0.2f;

	public float MaxImpulse = 20f;

	private float _timeout;

	private GameObject _lastHitObject;

	private Vector3 _lastHitPoint = Vector3.zero;

	private Vector3 _lastHitNormal = Vector3.up;

	protected override void OnCollision(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, CollisionMode collisionMode)
	{
		_lastHitObject = hitObject;
		_lastHitPoint = hitPoint;
		_lastHitNormal = hitNormal;
	}

	protected override void Awake()
	{
		base.Awake();
		_guidanceToHead = false;
		_guidanceToPartialHeight = Util.Range(0.25f, 0.5f);
	}

	protected override void Start()
	{
		if (ActionDelay > 0f)
		{
			_timeout = ActionDelay;
		}
		base.Start();
	}

	protected override void FixedUpdate()
	{
		if (Active && NumCollisions > 0)
		{
			_timeout -= Time.fixedDeltaTime;
			if (_timeout <= 0f)
			{
				Action();
			}
		}
		base.FixedUpdate();
	}

	protected virtual void Action()
	{
		if ((bool)Smoke)
		{
			Smoke.transform.parent = transform.parent;
			Object.Destroy(Smoke, SmokeLifetime);
		}
		if ((bool)EffectPrefab)
		{
			if (rigidbody.velocity.magnitude < 1f)
			{
				Scene.EffectsManager.DrawExplosionSpot(transform.position + Vector3.up * 0.5f, Vector3.down);
			}
			else
			{
				float num = (transform.position - PrevPos).magnitude + 1f;
				Scene.EffectsManager.DrawExplosionSpot(PrevPos, (transform.position - PrevPos).normalized * num);
			}
			Vector3 lastHitNormal = _lastHitNormal;
			lastHitNormal.y = (lastHitNormal.y < 0f ? 0f : lastHitNormal.y);
			lastHitNormal = (lastHitNormal + Vector3.up).normalized;
			Quaternion rotation = Quaternion.LookRotation(lastHitNormal);
			GameObject gameObject = Object.Instantiate(EffectPrefab, transform.position, rotation) as GameObject;
			TrailRenderer component = GetComponent<TrailRenderer>();
			if ((bool)component)
			{
				Object.Destroy(base.gameObject, component.time);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
			Object.Destroy(gameObject.gameObject, 5f);
		}
		Util.ExplosionDealsDamage(MaxDamage, MinDamage, DamageRandomness, transform.position, ActionRadius, Source, Consumable, MaxImpulse, (Character c) => true);
		if (ActionSound != string.Empty)
		{
			SoundManager.Play(ActionSound, transform.position, 25f, 150f, false);
		}
		if ((bool)rigidbody)
		{
			rigidbody.detectCollisions = false;
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.Sleep();
		}
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = false;
		}
		Scene.Camera.OnExplosion(transform.position);
		Active = false;
	}
}
