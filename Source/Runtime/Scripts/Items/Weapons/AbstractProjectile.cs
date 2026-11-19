using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbstractProjectile : MonoBehaviourAndLog
{
	protected enum CollisionMode
	{
		Primitive,
		Trajectory,
		Velocity
	}

	public float CustomGravity;

	public float MaxLifeTime = 60f;

	private float _lifeTime;

	private int _numUpdateFrames;

	private int _numCollisions;

	private int _numHitableCollisions;

	private Vector3 _prevVel = Vector3.zero;

	private Vector3 _prevPos = Vector3.zero;

	private Rigidbody _rigidbody;

	private Collider _collider;

	private HashSet<Collider> _ignoreCollisions = new HashSet<Collider>();

	private GameObject _hitObject;

	private Vector3 _hitPoint = Vector3.zero;

	private Vector3 _hitNormal = Vector3.up;

	public float LifeTime
	{
		get
		{
			return _lifeTime;
		}
	}

	public int NumUpdateFrames
	{
		get
		{
			return _numUpdateFrames;
		}
	}

	public int NumCollisions
	{
		get
		{
			return _numCollisions;
		}
	}

	public int NumHitableCollisions
	{
		get
		{
			return _numHitableCollisions;
		}
	}

	public HashSet<Collider> IgnoreCollisions
	{
		get
		{
			return _ignoreCollisions;
		}
	}

	public Vector3 PrevPos
	{
		get
		{
			return _prevPos;
		}
	}

	public Vector3 PrevVel
	{
		get
		{
			return _prevVel;
		}
	}

	public Character Source { get; set; }

	public Consumable Consumable { get; set; }

	public bool Active { get; set; }

	protected virtual void OnCollision(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, CollisionMode collisionMode)
	{
	}

	protected virtual void Awake()
	{
		LogTo("Other.Projectile");
		_rigidbody = GetComponent<Rigidbody>();
		_collider = GetComponentInChildren<Collider>();
	}

	protected virtual void Start()
	{
		if (CustomGravity != 0f)
		{
			_rigidbody.useGravity = false;
		}
		_prevPos = transform.position;
		Active = true;
	}

	protected virtual void Update()
	{
	}

	protected virtual void FixedUpdate()
	{
		if (!Active)
		{
			return;
		}
		_lifeTime += Time.fixedDeltaTime;
		if (_lifeTime >= MaxLifeTime)
		{
			Active = false;
			Object.Destroy(gameObject);
			return;
		}
		if (CustomGravity != 0f && (bool)_rigidbody)
		{
			_rigidbody.AddForce(new Vector3(0f, CustomGravity, 0f) * _rigidbody.mass);
		}
		FixedUpdateCollision();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!Active || !enabled)
		{
			return;
		}
		if (collision.contacts.Length > 0)
		{
			_hitObject = collision.collider.gameObject;
			_hitPoint = collision.contacts[0].point;
			_hitNormal = collision.contacts[0].normal;
			_numCollisions++;
			ImpactDamage(collision.collider.gameObject, collision.contacts[0].point);
			OnCollision(_hitObject, _hitPoint, _hitNormal, CollisionMode.Primitive);
			if (((1 << collision.collider.gameObject.layer) & Const.LayerMaskAllHitable) != 0)
			{
				if (_numHitableCollisions == 0)
				{
					GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
				}
				_numHitableCollisions++;
			}
		}
	}

	private void FixedUpdateCollision()
	{
		int numCollisions = _numCollisions;
		if (_numUpdateFrames > 0 && (transform.position - _prevPos).magnitude > 1.1754944E-38f)
		{
			Ray ray = new Ray(_prevPos, transform.position - _prevPos);
			float magnitude = (transform.position - _prevPos).magnitude;
			ray.direction.Normalize();
			RaycastHit[] array = Physics.RaycastAll(ray, magnitude, Const.LayerMaskHitByProjectile);
			bool flag = false;
			RaycastHit raycastHit = new RaycastHit();
			raycastHit.distance = float.PositiveInfinity;
			foreach (RaycastHit raycastHit2 in array)
			{
				if (raycastHit2.collider.transform.root.gameObject != gameObject && !_ignoreCollisions.Contains(raycastHit2.collider))
				{
					flag = true;
					if (raycastHit.distance > raycastHit2.distance)
					{
						raycastHit = raycastHit2;
					}
				}
			}
			if (flag)
			{
				_hitObject = raycastHit.collider.gameObject;
				_hitPoint = raycastHit.point;
				_hitNormal = raycastHit.normal;
				transform.position = raycastHit.point + raycastHit.normal * _collider.bounds.extents.magnitude;
				ImpactDamage(raycastHit.collider.gameObject, raycastHit.point);
				OnCollision(_hitObject, _hitPoint, _hitNormal, CollisionMode.Trajectory);
				if (Active)
				{
					float bounciness = _collider.material.bounciness;
					float bounciness2 = raycastHit.collider.material.bounciness;
					float num = (bounciness + bounciness2) / 2f;
					switch (_collider.material.bounceCombine)
					{
					case PhysicsMaterialCombine.Maximum:
						num = Mathf.Max(bounciness, bounciness2);
						break;
					case PhysicsMaterialCombine.Minimum:
						num = Mathf.Min(bounciness, bounciness2);
						break;
					case PhysicsMaterialCombine.Multiply:
						num = bounciness * bounciness2;
						break;
					}
					float dynamicFriction = _collider.material.dynamicFriction;
					float dynamicFriction2 = raycastHit.collider.material.dynamicFriction;
					float value = (dynamicFriction + dynamicFriction2) / 2f;
					switch (_collider.material.frictionCombine)
					{
					case PhysicsMaterialCombine.Maximum:
						value = Mathf.Max(dynamicFriction, dynamicFriction2);
						break;
					case PhysicsMaterialCombine.Minimum:
						value = Mathf.Min(dynamicFriction, dynamicFriction2);
						break;
					case PhysicsMaterialCombine.Multiply:
						value = dynamicFriction * dynamicFriction2;
						break;
					}
					Vector3 vector = raycastHit.normal * Vector3.Dot(raycastHit.normal, _rigidbody.linearVelocity);
					Vector3 vector2 = _rigidbody.linearVelocity - vector;
					GetComponent<Rigidbody>().linearVelocity = -vector * num + vector2 * (1f - Mathf.Clamp01(value));
					_numCollisions++;
					if (((1 << raycastHit.collider.gameObject.layer) & Const.LayerMaskAllHitable) != 0)
					{
						if (_numHitableCollisions == 0)
						{
							GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
						}
						_numHitableCollisions++;
					}
				}
			}
		}
		if (Active && numCollisions > _numCollisions && Vector3.Dot(_rigidbody.linearVelocity, _prevVel) < 0f)
		{
			Ray ray2 = new Ray(transform.position, -_rigidbody.linearVelocity.normalized);
			float distance = _rigidbody.linearVelocity.magnitude * Time.fixedDeltaTime * 2f;
			RaycastHit[] array3 = Physics.RaycastAll(ray2, distance, Const.LayerMaskHitByProjectile);
			bool flag2 = false;
			RaycastHit raycastHit3 = new RaycastHit();
			raycastHit3.distance = float.PositiveInfinity;
			foreach (RaycastHit raycastHit4 in array3)
			{
				if (raycastHit4.collider.transform.root.gameObject != gameObject && !_ignoreCollisions.Contains(raycastHit4.collider))
				{
					flag2 = true;
					if (raycastHit3.distance > raycastHit4.distance)
					{
						raycastHit3 = raycastHit4;
					}
				}
			}
			if (flag2)
			{
				_numCollisions++;
				_hitObject = raycastHit3.collider.gameObject;
				_hitPoint = raycastHit3.point;
				_hitNormal = raycastHit3.normal;
				transform.position = _hitPoint;
				ImpactDamage(_hitObject, _hitPoint);
				OnCollision(_hitObject, _hitPoint, _hitNormal, CollisionMode.Velocity);
			}
		}
		_prevVel = _rigidbody.linearVelocity;
		float unusedLocal1;
		Vector3 vector3 = transform.position - _prevPos;
		if (vector3.magnitude > 1f)
		{
			_prevPos = _prevPos + vector3.normalized * (vector3.magnitude - 1f);
		}
		_numUpdateFrames++;
	}

	private void ImpactDamage(GameObject obj, Vector3 hitPoint)
	{
		Character character = obj.FindComponentUpwards<Character>();
		if ((bool)character)
		{
			character.OnDamage(new HitInfo(GetComponent<Rigidbody>().linearVelocity.magnitude, DamageInfo.Type.Bullet, Source, GetComponent<Rigidbody>().linearVelocity, 0f, 0f, 100f, 1f, hitPoint, obj, 50f));
		}
	}

	public static GameObject Create(GameObject prefab, Vector3 position, Vector3 velocity, Character owner, Consumable consumable)
	{
		GameObject gameObject = Object.Instantiate(prefab, position, Quaternion.identity) as GameObject;
		gameObject.transform.parent = Scene.Groups.Projectiles;
		gameObject.transform.position = position;
		gameObject.GetComponent<Rigidbody>().linearVelocity = velocity;
		AbstractProjectile component = gameObject.GetComponent<AbstractProjectile>();
		if ((bool)component)
		{
			foreach (Collider item in from c in owner.gameObject.GetComponentsInChildren<Collider>(false)
				where c.enabled
				select c)
			{
				component.IgnoreCollisions.Add(item);
				Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), item);
			}
			component.Source = owner;
			component.Consumable = consumable;
		}
		return gameObject;
	}
}
