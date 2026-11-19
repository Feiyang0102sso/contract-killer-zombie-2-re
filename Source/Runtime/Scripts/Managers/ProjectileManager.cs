using System;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
	public struct Projectile
	{
		public GameObject gameObject;

		public MeshFilter meshFilter;

		public MeshRenderer meshRenderer;

		public bool enabled;

		public float lifeTimeLeft;

		public Vector3 startPos;

		public Vector3 prevPos;

		public Vector3 pos;

		public Vector3 vel;

		public int collisions;

		public float ricochetTime;

		public Firearm source;
	}

	private const float BouncingFriction = 0.55f;

	private const float BouncingRestitution = 0.45f;

	private const float RicochetAngle0 = 0f;

	private const float RicochetProbability0 = 0.1f;

	private const float RicochetAngle1 = (float)Math.PI / 4f;

	private const float RicochetProbability1 = 0f;

	private const float MaxRicochetTime = 0.15f;

	public Mesh DefaultMesh;

	public Material DefaultMaterial;

	public Material DefaultDamageBoostMaterial;

	public int MaxProjectiles = 32;

	public float MaxLifeTime = 2f;

	public Vector3 Gravity = Vector3.down * 9.8f;

	private Firearm[] _generators;

	private Projectile[] _projectiles;

	private int _numActiveProjectiles;

	private int _nextProjectileID;

	private CountdownTimer _damageBooster;

	private bool _isInitialized;

	public Projectile GetProjectileById(int id)
	{
		return _projectiles[id];
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

	public void Init()
	{
		_projectiles = new Projectile[MaxProjectiles];
		for (int i = 0; i < MaxProjectiles; i++)
		{
			_projectiles[i].gameObject = new GameObject(string.Format("Projectile_{0}", i));
			_projectiles[i].gameObject.transform.parent = null;
			_projectiles[i].meshFilter = _projectiles[i].gameObject.AddComponent<MeshFilter>();
			_projectiles[i].meshFilter.mesh = DefaultMesh;
			_projectiles[i].meshRenderer = _projectiles[i].gameObject.AddComponent<MeshRenderer>();
			_projectiles[i].meshRenderer.material = DefaultMaterial;
			_projectiles[i].meshRenderer.transform.localScale = Vector3.one;
			_projectiles[i].meshRenderer.enabled = false;
			_projectiles[i].pos = Vector3.zero;
			_projectiles[i].startPos = Vector3.zero;
			_projectiles[i].prevPos = Vector3.zero;
			_projectiles[i].vel = Vector3.zero;
			_projectiles[i].enabled = false;
			_projectiles[i].lifeTimeLeft = 0f;
			_projectiles[i].collisions = 0;
			_projectiles[i].ricochetTime = 0f;
			_projectiles[i].source = null;
		}
		_isInitialized = true;
	}

	private void Generate(Firearm firearm, bool muzzleFlash)
	{
		Transform barrel;
		Vector3 vector2;
		Vector3 normalized;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Quaternion q;
		Matrix4x4 matrix4x;
		Vector3 axis;
		float angle;
		bool flag = true;
		firearm.OnCreateProjectile(_nextProjectileID);
		bool flag2 = _damageBooster != null && _damageBooster.IsRunning();
		bool flag3 = Scene.Camera.HUD.IsZoomMode() && (bool)firearm.OwnerAsPlayer;
		Vector3 vector = Scene.Camera.transform.position + Scene.Camera.transform.forward * 0.25f - Scene.Camera.transform.right * 0.1f - Scene.Camera.transform.up * 0.1f;
		int numBarrels = firearm.NumBarrels;
		int numPellets = firearm.NumPellets;
		bool flag4 = numPellets > 1;
		Material material = firearm.TracerMaterial ? firearm.TracerMaterial : DefaultMaterial;
		float muzzleVelocity = firearm.MuzzleVelocity;
		float spreadAngle = firearm.GetSpreadAngle();
		int num = 0;
		int num2 = 0;
		int num3 = numBarrels * numPellets;
		for (int i = 0; i < num3; i++)
		{
			barrel = firearm.GetBarrel(num);
			vector2 = flag3 ? vector : barrel.position;
			normalized = (firearm.ShootingTo - vector2).normalized;
			flag = flag3 ? true : Vector3.Dot(normalized, barrel.right) >= (float)Math.PI / 4f;
			zero = Vector3.up;
			zero2 = Vector3.Cross(zero, normalized);
			zero = Vector3.Cross(normalized, zero2);
			_projectiles[_nextProjectileID].enabled = true;
			_projectiles[_nextProjectileID].meshRenderer.enabled = flag;
			_projectiles[_nextProjectileID].meshRenderer.material = material;
			_projectiles[_nextProjectileID].pos = vector2;
			_projectiles[_nextProjectileID].prevPos = vector2;
			_projectiles[_nextProjectileID].startPos = vector2;
			_projectiles[_nextProjectileID].vel = normalized * muzzleVelocity;
			_projectiles[_nextProjectileID].lifeTimeLeft = MaxLifeTime;
			_projectiles[_nextProjectileID].collisions = 0;
			_projectiles[_nextProjectileID].ricochetTime = 0f;
			_projectiles[_nextProjectileID].source = firearm;
			if (muzzleFlash && num2 == 0)
			{
				EffectsManager.instance.OnBulletShot(vector2, normalized, firearm.Owner.gameObject, firearm.Ammunition);
			}
			if (flag4)
			{
				if (num2 > 0)
				{
					q = Quaternion.AngleAxis(-180 + (num2 - 1) * 360 / (numPellets - 1), normalized);
					matrix4x = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
					axis = matrix4x.MultiplyVector(zero2);
					q = Quaternion.AngleAxis(spreadAngle * 2f / 3f + UnityEngine.Random.Range(-spreadAngle / 3f, spreadAngle / 3f), axis);
					matrix4x = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
					_projectiles[_nextProjectileID].vel = matrix4x.MultiplyVector(_projectiles[_nextProjectileID].vel);
				}
			}
			else
			{
				q = Quaternion.AngleAxis(UnityEngine.Random.Range(-180, 180), normalized);
				matrix4x = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
				axis = matrix4x.MultiplyVector(zero2);
				q = Quaternion.AngleAxis(UnityEngine.Random.Range(-spreadAngle, spreadAngle), axis);
				matrix4x = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
				_projectiles[_nextProjectileID].vel = matrix4x.MultiplyVector(_projectiles[_nextProjectileID].vel);
			}
			if (Util.DoRarely(this, "emit_sound", 1f))
			{
				Vector3 vector3 = vector2;
				float soundDistance;
				foreach (Unit item in Scene.Nearby<Unit>(vector3, 30f))
				{
					item.OnHearShotSound(vector3);
				}
			}
			_numActiveProjectiles++;
			if (_numActiveProjectiles > MaxProjectiles)
			{
				_numActiveProjectiles = MaxProjectiles;
			}
			_nextProjectileID++;
			if (_nextProjectileID > MaxProjectiles - 1)
			{
				_nextProjectileID = 0;
			}
			num2++;
			if (num2 >= numPellets)
			{
				num2 = 0;
				num++;
			}
		}
	}

	private void Update()
	{
		if (!_isInitialized)
		{
			return;
		}
		float num3;
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		Vector3 vector4;
		float num5;
		float num6;
		float value;
		Vector3 prevPos;
		bool flag = _damageBooster != null && _damageBooster.IsRunning();
		for (int i = 0; i < MaxProjectiles; i++)
		{
			if (_projectiles[i].enabled)
			{
				_projectiles[i].prevPos = _projectiles[i].pos;
			}
		}
		float num = Time.deltaTime;
		while (num > 0f)
		{
			Firearm firearm = null;
			int num2 = -1;
			if (_generators != null)
			{
				for (int j = 0; j < _generators.Length; j++)
				{
					if ((_generators[j].FireTrigger || _generators[j].BurstTrigger) && _generators[j].CooldownTime == 0f && _generators[j].GetAmmoInClip() > 0 && (!firearm || ((bool)firearm && _generators[j].CooldownTime < firearm.CooldownTime)))
					{
						firearm = _generators[j];
						num2 = j;
					}
				}
			}
			if ((bool)firearm)
			{
				if (firearm.CooldownTime < 0f)
				{
					firearm.CooldownTime = 0f;
				}
				num3 = num > firearm.CooldownTime ? firearm.CooldownTime : num;
			}
			else
			{
				num3 = num;
			}
			for (int k = 0; k < MaxProjectiles; k++)
			{
				if (_projectiles[k].enabled)
				{
					vector = _projectiles[k].vel * num3;
					_projectiles[k].pos += vector;
					if (_projectiles[k].collisions > 0)
					{
						_projectiles[k].ricochetTime += num3;
						if (_projectiles[k].ricochetTime > 0.15f)
						{
							float num4 = _projectiles[k].vel.magnitude * (_projectiles[k].ricochetTime - 0.15f);
							_projectiles[k].startPos = _projectiles[k].startPos + (_projectiles[k].prevPos - _projectiles[k].startPos).normalized * num4;
							_projectiles[k].ricochetTime = 0.15f;
						}
					}
					_projectiles[k].lifeTimeLeft -= num3;
					if (_projectiles[k].lifeTimeLeft <= 0f)
					{
						_projectiles[k].enabled = false;
						_projectiles[k].meshRenderer.enabled = false;
						_numActiveProjectiles--;
					}
				}
			}
			for (int l = 0; l < MaxProjectiles; l++)
			{
				if (_projectiles[l].enabled)
				{
					vector2 = Physics.gravity * num3;
					_projectiles[l].vel += vector2;
				}
			}
			if ((bool)firearm)
			{
				Generate(firearm, true);
			}
			num -= num3;
			if ((bool)firearm)
			{
				if (firearm.CooldownTime == 0f)
				{
					if (firearm.BurstModeEnabled && firearm.BurstShotsLeft > 0)
					{
						firearm.CooldownTime = 1f / firearm.BurstRateOfFire;
					}
					else
					{
						if (firearm.BurstModeEnabled)
						{
							firearm.BurstShotsLeft = firearm.BurstNumShots;
						}
						firearm.CooldownTime = 1f / firearm.RateOfFire;
					}
				}
				else
				{
					firearm.CooldownTime -= num3;
				}
			}
			if (_generators != null)
			{
				for (int m = 0; m < _generators.Length; m++)
				{
					if (m != num2)
					{
						_generators[m].CooldownTime = Mathf.Clamp(_generators[m].CooldownTime - num3, 0f, float.MaxValue);
					}
				}
			}
		}
		for (int n = 0; n < MaxProjectiles; n++)
		{
			RaycastHit hit;
			if (_projectiles[n].enabled && Physics.Linecast(_projectiles[n].prevPos, _projectiles[n].pos, out hit, Const.LayerMaskHitByProjectile))
			{
				GameObject gameObject = hit.transform.gameObject;
				if (hit.transform.gameObject.GetComponent<Human>() != _projectiles[n].source.Owner)
				{
					_projectiles[n].pos = hit.point + hit.normal * 0.01f;
					num5 = Vector3.Dot(hit.normal, _projectiles[n].vel.normalized);
					value = num5 / ((float)Math.PI / 4f);
					value = Mathf.Clamp01(value);
					num6 = 0.1f * (1f - value) + 0f * value;
					if (((1 << gameObject.layer) & Const.LayerMaskObstacles) != 0 && (UnityEngine.Random.Range(0f, 1f) <= num6 || _projectiles[n].collisions > 0))
					{
						vector3 = hit.normal * Vector3.Dot(hit.normal, _projectiles[n].vel);
						vector4 = _projectiles[n].vel - vector3;
						_projectiles[n].vel = vector3 * -0.45f + vector4 * 0.55f;
						_projectiles[n].startPos = _projectiles[n].pos;
						_projectiles[n].collisions++;
						_projectiles[n].ricochetTime = 0f;
						if (_projectiles[n].collisions < 2)
						{
							Scene.EffectsManager.OnHit(hit.collider, hit.point, hit.normal, _projectiles[n].vel, true, false, null);
						}
					}
					else
					{
						_projectiles[n].enabled = false;
						_projectiles[n].meshRenderer.enabled = false;
						float travelledDistance = (_projectiles[n].startPos - _projectiles[n].pos).magnitude;
						if (_projectiles[n].collisions > 0)
						{
							travelledDistance = (MaxLifeTime - _projectiles[n].lifeTimeLeft) * _projectiles[n].source.MuzzleVelocity;
						}
						_projectiles[n].source.OnCollideProjectile(gameObject, ref hit, travelledDistance, ref _projectiles[n].pos, ref _projectiles[n].vel);
						_projectiles[n].source = null;
					}
				}
			}
		}
		Vector3 vector5;
		Vector3 lhs;
		Vector3 normalized;
		float num8;
		Vector3 localScale = new Vector3(0f, 0f, 0f);
		Vector3 eulerAngles = new Vector3(0f, 0f, 0f);
		Vector3 position = Scene.MainCamera.transform.position;
		GameObject gameObject2;
		Transform transform;
		for (int num7 = 0; num7 < MaxProjectiles; num7++)
		{
			if (_projectiles[num7].enabled && _projectiles[num7].meshRenderer.enabled)
			{
				gameObject2 = _projectiles[num7].gameObject;
				transform = gameObject2.transform;
				prevPos = _projectiles[num7].prevPos;
				if (flag)
				{
					prevPos = _projectiles[num7].startPos + (prevPos - _projectiles[num7].startPos) / 4f;
				}
				else
				{
					prevPos = (prevPos + _projectiles[num7].startPos) / 2f;
				}
				vector5 = _projectiles[num7].pos - prevPos;
				transform.position = prevPos + vector5 / 2f;
				transform.right = _projectiles[num7].vel.normalized;
				lhs = position - _projectiles[num7].pos;
				normalized = transform.up * Vector3.Dot(lhs, transform.up) + transform.forward * Vector3.Dot(lhs, transform.forward);
				normalized = normalized.normalized;
				num5 = GetAngle(normalized, transform.forward, transform.right);
				eulerAngles.x = -num5;
				transform.Rotate(eulerAngles);
				value = lhs.magnitude / _projectiles[num7].source.TracerProgressiveDistance;
				value = value > 1f ? 1f : value;
				num8 = _projectiles[num7].source.TracerWidth * (1f - value) + _projectiles[num7].source.TracerProgressiveWidth * value;
				localScale.x = _projectiles[num7].source.TracerLengthScale * vector5.magnitude;
				localScale.y = num8;
				localScale.z = num8;
				transform.localScale = localScale;
			}
		}
		if (_damageBooster != null && !_damageBooster.IsRunning())
		{
			OnDamageBoostExpire();
			_damageBooster = null;
		}
	}

	public void AddFirearm(Firearm source)
	{
		bool flag = false;
		if (_generators != null)
		{
			for (int i = 0; i < _generators.Length; i++)
			{
				if (_generators[i] == source)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			if (_generators != null)
			{
				Array.Resize(ref _generators, _generators.Length + 1);
			}
			else
			{
				_generators = new Firearm[1];
			}
			_generators[_generators.Length - 1] = source;
		}
	}

	public void RemoveFirearm(Firearm source)
	{
		bool flag = false;
		if (_generators != null)
		{
			for (int i = 0; i < _generators.Length; i++)
			{
				if (_generators[i] == source)
				{
					flag = true;
					if (_generators.Length > 1)
					{
						for (int j = i + 1; j < _generators.Length; j++)
						{
							_generators[j - 1] = _generators[j];
						}
						Array.Resize(ref _generators, _generators.Length - 1);
					}
					else
					{
						_generators = null;
					}
					break;
				}
			}
		}
	}

	public void OnDamageBoost(float duration)
	{
		_damageBooster = new CountdownTimer(duration);
		if ((bool)DefaultDamageBoostMaterial)
		{
			for (int i = 0; i < MaxProjectiles; i++)
			{
				_projectiles[i].gameObject.GetComponent<Renderer>().material = DefaultDamageBoostMaterial;
			}
		}
	}

	private void OnDamageBoostExpire()
	{
		if (DefaultDamageBoostMaterial)
		{
			for (int i = 0; i < MaxProjectiles; i++)
			{
				if ((bool)_projectiles[i].source)
				{
					_projectiles[i].gameObject.GetComponent<Renderer>().material = _projectiles[i].source.TracerMaterial;
				}
				else
				{
					_projectiles[i].gameObject.GetComponent<Renderer>().material = DefaultMaterial;
				}
			}
		}
	}
}
