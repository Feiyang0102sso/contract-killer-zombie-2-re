using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
	[Serializable]
	public class ScaleByDistance
	{
		public bool Enabled;

		public float MinDistance;

		public float MinScale = 1f;

		public float MaxDistance = 1f;

		public float MaxScale = 2f;
	}

	[Serializable]
	public class EffectByMaterial
	{
		public PhysicsMaterial PhysicsMaterial;

		public string[] HitSound;

		public string[] ImpactSound;

		public int NumFlashParticles;

		public int NumSparkParticles;

		public int NumDustParticles;

		public int NumFragmentParticles;

		public int NumBloodSplatterParticles;

		public GameObject[] DecalPrefab;

		public bool DecalRandomOrientation;

		public bool DecalUseLightProbes;

		public float DecalSize = 0.1f;
	}

	[Serializable]
	public class Splatter
	{
		public GameObject Emitter;

		public float TextureRotation;

		public float NormalVelocity = 1f;

		public float TangentialVelocity = 1f;
	}

	private struct Decal
	{
		public GameObject prefab;

		public GameObject gameObject;

		public bool exclusive;

		public float scale;
	}

	private struct Emission
	{
		public enum Type
		{
			Null,
			Spark,
			Fragment,
			Dust,
			Blood,
			SizeOf
		}

		public Type type;

		public Vector3 position;

		public int quantity;

		public float timestamp;
	}

	private enum EffectsLevel
	{
		Min,
		Lite,
		Full
	}

	private delegate void AddDecalCallback(GameObject decal);

	private const int NumDecalsPrecached = 4;

	private const int EmissionHistorySize = 16;

	public GameObject PistolShells;

	public GameObject PistolShellsLite;

	public GameObject ShotgunShells;

	public GameObject ShotgunShellsLite;

	public GameObject RifleShells;

	public GameObject RifleShellsLite;

	public GameObject[] MuzzleFlash;

	public GameObject[] Flash;

	public GameObject Sparks;

	public GameObject SparksLite;

	public float SparksSpread = 45f;

	public float SparksVelocity = 5f;

	public float SparksVelocityBias = 1f;

	public GameObject Dust;

	public float DustSpread = 45f;

	public float DustVelocity = 0.25f;

	public float DustVelocityBias = 0.05f;

	public GameObject[] Fragment;

	public GameObject[] FragmentLite;

	public float FragmentSpread = 45f;

	public float FragmentVelocity = 2.5f;

	public float FragmentVelocityBias = 0.5f;

	public Splatter[] BloodSplatter;

	public Splatter[] GreenGooSplatter;

	public GameObject Bleedout;

	public GameObject SurfaceGoo;

	public ScaleByDistance scaleByDistance;

	public EffectByMaterial[] effectByMaterial;

	public float BloodSplashDistance = 10f;

	public GameObject BloodSplashPrefab;

	public int DecalLimit = 256;

	public float DecalNormalOffset = 0.01f;

	public GameObject DecalExplosion;

	public float DecalExplosionSize = 1.5f;

	public float DecalExplosionFadeTime = 10f;

	public GameObject HeadshotPrefab;

	public float HeadshotLifetime = 5f;

	public GameObject BodyblowPrefab;

	public float BodyblowLifetime = 5f;

	public GameObject BomberBlowPrefab;

	public float BomberblowLifetime = 5f;

	public GameObject ThrowerExplosionPrefab;

	public float ThrowerExplosionLifeTime = 5f;

	public static EffectsManager instance = null;

	private EffectByMaterial FleshEffect;

	private Transform bloodSplashGroup;

	private Transform decalsCasheGroup;

	private float defaultBloodSplashRadius = 0.5f;

	private Dictionary<GameObject, List<GameObject>> decalsCache = new Dictionary<GameObject, List<GameObject>>();

	private Decal[] decals;

	private int nextDecalIndex;

	private bool addDecalExLock;

	private Emission[] _emissionHistory = new Emission[16];

	private float[] _durationByType = new float[5];

	private int _checkedEmissions;

	private int _approvedEmissions;

	private int _rejectedEmissions;

	private EffectsLevel _effectsLevel = EffectsLevel.Full;

	private Vector3 _cameraPos = Vector3.zero;

	private Vector3 _cameraDir = Vector3.zero;

	private void Awake()
	{
		if (Singleton.Quality.PrecacheSoundEffects)
		{
			SoundManager.PreloadInBackground("skill_shot");
			SoundManager.PreloadInBackground("skill_shot_2");
		}
		foreach (EffectByMaterial effectByMaterial in this.effectByMaterial)
		{
			if (Singleton.Quality.HitSoundsOnScene && Singleton.Quality.PrecacheSoundEffects)
			{
				foreach (string soundName in effectByMaterial.HitSound)
				{
					SoundManager.Preload(soundName);
				}
				foreach (string soundName2 in effectByMaterial.ImpactSound)
				{
					SoundManager.Preload(soundName2);
				}
			}
			if (effectByMaterial.NumBloodSplatterParticles > 0)
			{
				FleshEffect = effectByMaterial;
			}
		}
		bloodSplashGroup = new GameObject("_BloodSplash").transform;
		bloodSplashGroup.transform.parent = transform;
		decalsCasheGroup = new GameObject("_DecalsCache").transform;
		decalsCasheGroup.parent = transform;
		decals = new Decal[DecalLimit];
		for (int l = 0; l < DecalLimit; l++)
		{
			decals[l].prefab = null;
			decals[l].gameObject = null;
			decals[l].exclusive = false;
			decals[l].scale = 0f;
		}
		foreach (EffectByMaterial effectByMaterial2 in this.effectByMaterial)
		{
			foreach (GameObject prefab in effectByMaterial2.DecalPrefab)
			{
				PrecacheDecals(prefab);
			}
		}
		PrecacheDecals(DecalExplosion);
		for (int num = 0; num < 16; num++)
		{
			_emissionHistory[num].type = Emission.Type.Null;
			_emissionHistory[num].position = Vector3.zero;
			_emissionHistory[num].quantity = 0;
			_emissionHistory[num].timestamp = Time.time;
		}
		_durationByType[0] = 0f;
		_durationByType[4] = GetDurationByType(Emission.Type.Blood);
		_durationByType[3] = GetDurationByType(Emission.Type.Dust);
		_durationByType[2] = GetDurationByType(Emission.Type.Fragment);
		_durationByType[1] = GetDurationByType(Emission.Type.Spark);
		Singleton.GameEvents.OnFPSDecreased += new Action<int>(OnFPSDecreased);
		Singleton.GameEvents.OnFPSIncreased += new Action<int>(OnFPSIncreased);
		instance = this;
	}

	private void OnDestroy()
	{
		Debug.Log("EffectsManager checked emissions:" + _checkedEmissions + " approved: " + _approvedEmissions + " rejected: " + _rejectedEmissions);
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnFPSDecreased -= new Action<int>(OnFPSDecreased);
		}
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnFPSIncreased -= new Action<int>(OnFPSIncreased);
		}
		StopCoroutine("AddDecalEx");
		instance = null;
	}

	private void Update()
	{
		if ((bool)Scene.MainCamera)
		{
			_cameraPos = Scene.MainCamera.transform.position;
			_cameraDir = Scene.MainCamera.transform.forward;
		}
	}

	private float GetDurationByType(Emission.Type type)
	{
		float num = 0f;
		switch (type)
		{
		case Emission.Type.Blood:
		{
			foreach (Splatter splatter in BloodSplatter)
			{
				num = Mathf.Max(num, splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy);
			}
			break;
		}
		case Emission.Type.Dust:
			num = (Dust ? Dust.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy : 0f);
			break;
		case Emission.Type.Fragment:
		{
			foreach (GameObject gameObject in Fragment)
			{
				num = Mathf.Max(num, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy);
			}
			break;
		}
		case Emission.Type.Spark:
			num = (Sparks ? Sparks.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy : 0f);
			break;
		}
		return num;
	}

	private void AddToHistory(Emission.Type type, Vector3 position, int quantity)
	{
		if (quantity <= 0)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < 16; i++)
		{
			if (_emissionHistory[i].type == Emission.Type.Null || Time.time - _emissionHistory[i].timestamp > _durationByType[(int)_emissionHistory[i].type])
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			_emissionHistory[num].type = type;
			_emissionHistory[num].position = position;
			_emissionHistory[num].quantity = quantity;
			_emissionHistory[num].timestamp = Time.time;
		}
	}

	private int CheckEmissionQuantity(Emission.Type type, Vector3 position, int quantity, float sphereRadius, int maxQuantity)
	{
		_checkedEmissions++;
		float num = _durationByType[(int)type];
		float num2 = sphereRadius * sphereRadius;
		int num3 = 0;
		for (int i = 0; i < 16; i++)
		{
			if (_emissionHistory[i].type == type && Time.time - _emissionHistory[i].timestamp < num && (position - _emissionHistory[i].position).sqrMagnitude < num2)
			{
				num3 += _emissionHistory[i].quantity;
			}
		}
		int num4 = maxQuantity - num3;
		num4 = (!(num4 >= 0) ? 0 : num4);
		if (num4 < quantity)
		{
			_approvedEmissions += num4;
			_rejectedEmissions += quantity - num4;
			return num4;
		}
		_approvedEmissions += quantity;
		return quantity;
	}

	private void OnFPSDecreased(int fps)
	{
		if (fps < Const.MinEffectsFPS)
		{
			_effectsLevel = EffectsLevel.Min;
		}
		else if (fps < Const.LiteEffectsFPS)
		{
			_effectsLevel = EffectsLevel.Lite;
		}
	}

	private void OnFPSIncreased(int fps)
	{
		if (fps > Const.FullEffectsFPS)
		{
			_effectsLevel = EffectsLevel.Full;
		}
		else if (fps > Const.LiteEffectsFPS)
		{
			_effectsLevel = EffectsLevel.Lite;
		}
	}

	private Vector3 Spread(Vector3 normal, float angle)
	{
		Vector3 v = new Vector3(normal.y, normal.z, normal.x);
		Quaternion q = Quaternion.AngleAxis(UnityEngine.Random.Range(-180, 180), normal);
		Matrix4x4 rotation = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
		Vector3 axis = rotation.MultiplyVector(v);
		q = Quaternion.AngleAxis(UnityEngine.Random.Range(-angle, angle), axis);
		rotation = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
		return rotation.MultiplyVector(normal);
	}

	private float GetParticleScale(ref Vector3 position)
	{
		float result = 1f;
		if (scaleByDistance.Enabled)
		{
			float magnitude = (Scene.MainCamera.transform.position - position).magnitude;
			float value = (magnitude - scaleByDistance.MinDistance) / (scaleByDistance.MaxDistance - scaleByDistance.MinDistance);
			value = Mathf.Clamp(value, 0f, 1f);
			result = scaleByDistance.MinScale * (1f - value) + scaleByDistance.MaxScale * value;
		}
		return result;
	}

	private void EmitShell(Vector3 position, Vector3 normal, Weapon.AmmoType type)
	{
		GameObject gameObject = null;
		switch (type)
		{
		case Weapon.AmmoType.PistolCartridge:
		case Weapon.AmmoType.SMGCartridge:
			gameObject = _effectsLevel == EffectsLevel.Full
				? PistolShells
				: (_effectsLevel == EffectsLevel.Lite ? PistolShellsLite : null);
			break;
		case Weapon.AmmoType.ShotgunShell:
			gameObject = _effectsLevel == EffectsLevel.Full
				? ShotgunShells
				: (_effectsLevel == EffectsLevel.Lite ? ShotgunShellsLite : null);
			break;
		case Weapon.AmmoType.AssaultRifleCartridge:
		case Weapon.AmmoType.SniperRifleCartridge:
		case Weapon.AmmoType.MGCartridge:
			gameObject = _effectsLevel == EffectsLevel.Full
				? RifleShells
				: (_effectsLevel == EffectsLevel.Lite ? RifleShellsLite : null);
			break;
		}
		if ((bool)gameObject)
		{
			float particleScale = GetParticleScale(ref position);
			gameObject.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, normal, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minSize, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, 0f, 0f);
		}
	}

	private void EmitMuzzleFlash(Vector3 position, Vector3 normal)
	{
		GameObject gameObject = null;
		if (MuzzleFlash.Length > 0)
		{
			gameObject = MuzzleFlash[UnityEngine.Random.Range(0, MuzzleFlash.Length)];
		}
		if ((bool)gameObject)
		{
			float particleScale = GetParticleScale(ref position);
			gameObject.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, normal * 0.1f, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minSize, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, UnityEngine.Random.Range(-180f, 180f), 0f);
		}
	}

	private void EmitFlash(Vector3 position, Vector3 normal)
	{
		GameObject gameObject = null;
		if (Flash.Length > 0)
		{
			gameObject = Flash[UnityEngine.Random.Range(0, Flash.Length)];
		}
		if ((bool)gameObject)
		{
			float particleScale = GetParticleScale(ref position);
			gameObject.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, normal, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minSize, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, UnityEngine.Random.Range(-180f, 180f), 0f);
		}
	}

	private void EmitSparks(Vector3 position, Vector3 normal, int numSparks)
	{
		GameObject gameObject = _effectsLevel == EffectsLevel.Full
			? Sparks
			: (_effectsLevel == EffectsLevel.Lite ? SparksLite : null);
		if ((bool)gameObject)
		{
			numSparks = CheckEmissionQuantity(Emission.Type.Spark, position, numSparks, 0.25f, 16);
			float unusedFloat;
			int unusedCount;
			float particleScale = GetParticleScale(ref position);
			for (int i = 0; i < numSparks; i++)
			{
				Vector3 velocity = Spread(normal, SparksSpread) * (SparksVelocity + UnityEngine.Random.Range(-SparksVelocityBias, SparksVelocityBias));
				gameObject.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, velocity, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minSize, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, 0f, 0f);
			}
			AddToHistory(Emission.Type.Spark, position, numSparks);
		}
	}

	private void EmitDust(Vector3 position, Vector3 normal, int numParticles)
	{
		if ((bool)Dust)
		{
			numParticles = CheckEmissionQuantity(Emission.Type.Dust, position, numParticles, 0.5f, 7);
			float unusedFloat;
			int unusedCount;
			float particleScale = GetParticleScale(ref position);
			for (int i = 0; i < numParticles; i++)
			{
				Vector3 velocity = Spread(normal, DustSpread) * (DustVelocity + UnityEngine.Random.Range(-DustVelocityBias, DustVelocityBias));
				Dust.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, velocity, UnityEngine.Random.Range(Dust.GetComponent<NewUnityLegacyParticleEmitter>().minSize, Dust.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(Dust.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, Dust.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, UnityEngine.Random.Range(-180f, 180f), UnityEngine.Random.Range(-Dust.GetComponent<NewUnityLegacyParticleEmitter>().rndAngularVelocity, Dust.GetComponent<NewUnityLegacyParticleEmitter>().rndAngularVelocity));
			}
			AddToHistory(Emission.Type.Dust, position, numParticles);
		}
	}

	private void EmitFragments(Vector3 position, Vector3 normal, int numParticles)
	{
		numParticles = CheckEmissionQuantity(Emission.Type.Fragment, position, numParticles, 0.25f, 10);
		float unusedLocal1;
		int unusedLocal2;
		float particleScale = GetParticleScale(ref position);
		for (int i = 0; i < numParticles; i++)
		{
			GameObject gameObject = null;
			if (_effectsLevel == EffectsLevel.Lite)
			{
				if (FragmentLite.Length > 0)
				{
					gameObject = FragmentLite[UnityEngine.Random.Range(0, FragmentLite.Length)];
				}
			}
			else if (_effectsLevel == EffectsLevel.Full && Fragment.Length > 0)
			{
				gameObject = Fragment[UnityEngine.Random.Range(0, Fragment.Length)];
			}
			if ((bool)gameObject)
			{
				gameObject.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, Spread(normal, FragmentSpread) * (FragmentVelocity + UnityEngine.Random.Range(-FragmentVelocityBias, FragmentVelocityBias)), UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minSize, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(gameObject.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, gameObject.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, UnityEngine.Random.Range(-180f, 180f), 0f);
			}
		}
		AddToHistory(Emission.Type.Fragment, position, numParticles);
	}

	private void EmitBulletSplatter(ref Splatter[] preset, Vector3 position, int numParticles, float minRotationAngle, float maxRotationAngle)
	{
		float particleScale = GetParticleScale(ref position);
		Vector3 forward = Scene.MainCamera.transform.forward;
		Vector3 right = Scene.MainCamera.transform.right;
		float max = (maxRotationAngle - minRotationAngle) / (float)(numParticles + 1);
		float num = minRotationAngle;
		for (int i = 0; i < numParticles; i++)
		{
			Splatter splatter = preset[UnityEngine.Random.Range(0, preset.Length)];
			Quaternion q = Quaternion.AngleAxis(num, forward);
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
			Vector3 velocity = -forward * splatter.NormalVelocity + matrix4x.MultiplyVector(right) * splatter.TangentialVelocity;
			splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, velocity, UnityEngine.Random.Range(splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().minSize, splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, -num + splatter.TextureRotation, 0f);
			num += UnityEngine.Random.Range(0f, max);
		}
	}

	private void EmitMeleeSplatter(ref Splatter[] preset, Vector3 position, Vector3 velocity, int numParticles, float minRotationAngle, float maxRotationAngle)
	{
		float particleScale = GetParticleScale(ref position);
		Vector3 forward = Scene.MainCamera.transform.forward;
		Vector3 right = Scene.MainCamera.transform.right;
		Vector3 up = Scene.MainCamera.transform.up;
		Vector3 vector = right * Vector3.Dot(right, velocity) + up * Vector3.Dot(up, velocity);
		float angle = Firearm.GetAngle(vector.normalized, up, forward);
		for (int i = 0; i < numParticles; i++)
		{
			Splatter splatter = preset[UnityEngine.Random.Range(0, preset.Length)];
			float num = angle + UnityEngine.Random.Range(minRotationAngle, maxRotationAngle);
			Vector3 velocity2 = -forward * splatter.NormalVelocity + vector.normalized * splatter.TangentialVelocity;
			splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().Emit(position, velocity2, UnityEngine.Random.Range(splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().minSize, splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().maxSize) * particleScale, UnityEngine.Random.Range(splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().minEnergy, splatter.Emitter.GetComponent<NewUnityLegacyParticleEmitter>().maxEnergy), Color.white, num + splatter.TextureRotation, 0f);
		}
	}

	private void EmitBulletBloodSplatter(Vector3 position, int numParticles)
	{
		float unusedLocal1;
		int unusedLocal2;
		float unusedLocal3;
		float unusedLocal4;
		if (BloodSplatter != null && BloodSplatter.Length > 0)
		{
			numParticles = CheckEmissionQuantity(Emission.Type.Blood, position, numParticles, 0.175f, 8);
			if (numParticles > 0)
			{
				EmitBulletSplatter(ref BloodSplatter, position, numParticles, 75f, 285f);
				AddToHistory(Emission.Type.Blood, position, numParticles);
			}
		}
	}

	private void EmitMeleeBloodSplatter(Vector3 position, Vector3 velocity, int numParticles)
	{
		if (BloodSplatter != null && BloodSplatter.Length > 0)
		{
			numParticles = CheckEmissionQuantity(Emission.Type.Blood, position, numParticles, 0.175f, 8);
			float unusedFloat1;
			int unusedCount;
			float unusedFloat2;
			float unusedFloat3;
			float unusedFloat4;
			if (numParticles > 0)
			{
				for (int i = 0; i < numParticles; i++)
				{
					EmitMeleeSplatter(ref BloodSplatter, position, velocity, numParticles, -105f, -75f);
					AddToHistory(Emission.Type.Blood, position, numParticles);
					position += velocity.normalized * 0.15f;
				}
			}
		}
	}

	private void EmitGreenGoo(Vector3 position, int numParticles)
	{
		float unusedLocal1;
		float unusedLocal2;
		if (GreenGooSplatter != null && GreenGooSplatter.Length > 0)
		{
			EmitBulletSplatter(ref GreenGooSplatter, position, numParticles, 30f, 330f);
		}
	}

	private bool IsBloodSplashAllowed(Vector3 center, float radius)
	{
		BloodSplashAttenuation[] componentsInChildren = bloodSplashGroup.GetComponentsInChildren<BloodSplashAttenuation>();
		foreach (BloodSplashAttenuation bloodSplashAttenuation in componentsInChildren)
		{
			if ((bloodSplashAttenuation.GetSplashSphereCenter() - center).magnitude < radius + bloodSplashAttenuation.GetSplashSphereRadius())
			{
				return false;
			}
		}
		return true;
	}

	private void DrawBloodSplash(Vector3 position, Vector3 normal)
	{
		int unusedLocal1;
		if (BloodSplashAttenuation.GetNumActiveSplashes() >= 4)
		{
			return;
		}
		if (BloodSplashPrefab)
		{
			float num = 0f;
			RaycastHit hitInfo;
			if (Physics.Linecast(position, position - Vector3.up * BloodSplashDistance, out hitInfo, Const.LayerMaskObstacles))
			{
				num = hitInfo.distance;
			}
			if (num <= 0.125f)
			{
				return;
			}
			float unusedLocal2;
			Vector3 right = Scene.MainCamera.transform.right;
			Quaternion q;
			Matrix4x4 rotation;
			Vector3 vector;
			for (float num2 = 0f; num2 < 90f; num2 += 5f)
			{
				q = Quaternion.AngleAxis(num2, right);
				rotation = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
				vector = rotation.MultiplyVector(normal);
				if (Physics.Linecast(position, position + vector * BloodSplashDistance, out hitInfo, Const.LayerMaskObstacles))
				{
					float num3 = Mathf.Min(defaultBloodSplashRadius, Mathf.Min(num, hitInfo.distance));
					if (IsBloodSplashAllowed(hitInfo.point, num3))
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(BloodSplashPrefab) as GameObject;
						gameObject.transform.position = position;
						gameObject.transform.forward = vector;
						gameObject.transform.parent = bloodSplashGroup.transform;
						gameObject.transform.RotateAround(gameObject.transform.forward, UnityEngine.Random.Range(-180, 180));
						Projector component = gameObject.GetComponent<Projector>();
						defaultBloodSplashRadius = component.orthographicSize;
						component.orthographicSize = num3;
						gameObject.GetComponent<BloodSplashAttenuation>().SetSplashSphere(hitInfo.point + hitInfo.normal * 0.01f, component.orthographicSize);
						break;
					}
				}
			}
			float unusedLocal3;
		}
	}

	private void DrawBleedOut(Vector3 position)
	{
		if ((bool)Bleedout)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Bleedout, position, new Quaternion(0f, 1f, 0f, 0f)) as GameObject;
			gameObject.transform.parent = bloodSplashGroup;
			UnityEngine.Object.Destroy(gameObject, gameObject.GetComponent<ParticleSystem>().startLifetime);
		}
	}

	public GameObject DrawSurfaceGoo(Vector3 position)
	{
		if ((bool)SurfaceGoo)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(SurfaceGoo, position, new Quaternion(0f, 1f, 0f, 0f)) as GameObject;
			gameObject.SetParent(Util.FindOrCreate("/Goo"));
			UnityEngine.Object.Destroy(gameObject, gameObject.GetComponent<ParticleSystem>().startLifetime);
			return gameObject;
		}
		return null;
	}

	private void PrecacheDecals(GameObject prefab)
	{
		if (decalsCache.ContainsKey(prefab))
		{
			for (int i = 0; i < 4; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
				gameObject.GetComponent<Renderer>().enabled = false;
				gameObject.transform.parent = decalsCasheGroup;
				decalsCache[prefab].Add(gameObject);
			}
		}
		else
		{
			List<GameObject> list = new List<GameObject>();
			for (int j = 0; j < 4; j++)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(prefab) as GameObject;
				gameObject2.GetComponent<Renderer>().enabled = false;
				gameObject2.transform.parent = decalsCasheGroup;
				list.Add(gameObject2);
			}
			decalsCache.Add(prefab, list);
		}
	}

	private GameObject GetRandomCachedPrefab(ref GameObject[] prefabs)
	{
		int num = UnityEngine.Random.Range(0, prefabs.Length);
		int num2 = prefabs.Length;
		while (decalsCache[prefabs[num]].Count == 0 && num2 > 0)
		{
			num++;
			num = ((num < prefabs.Length) ? num : 0);
			num2--;
		}
		return prefabs[num];
	}

	private GameObject GetDecal(GameObject prefab)
	{
		List<GameObject> list = decalsCache[prefab];
		int count = list.Count;
		if (count > 0)
		{
			GameObject gameObject = list[count - 1];
			list.RemoveAt(count - 1);
			gameObject.GetComponent<Renderer>().enabled = true;
			gameObject.transform.parent = null;
			return gameObject;
		}
		return UnityEngine.Object.Instantiate(prefab) as GameObject;
	}

	private void ReuseDecal(GameObject prefab, GameObject gameObject)
	{
		gameObject.transform.parent = decalsCasheGroup;
		gameObject.GetComponent<Renderer>().enabled = false;
		decalsCache[prefab].Add(gameObject);
	}

	private GameObject AddDecal(GameObject parent, Vector3 position, Vector3 normal, float scale, bool randomOrientation, bool useLightProbes, GameObject prefab, bool exclusive)
	{
		GameObject gameObject = null;
		position += normal * DecalNormalOffset;
		if ((bool)decals[nextDecalIndex].gameObject)
		{
			ReuseDecal(decals[nextDecalIndex].prefab, decals[nextDecalIndex].gameObject);
		}
		decals[nextDecalIndex].prefab = prefab;
		decals[nextDecalIndex].gameObject = GetDecal(prefab);
		decals[nextDecalIndex].gameObject.layer = Const.LayerDecal;
		decals[nextDecalIndex].gameObject.GetComponent<Renderer>().useLightProbes = useLightProbes;
		if (useLightProbes)
		{
			decals[nextDecalIndex].gameObject.GetComponent<Renderer>().probeAnchor = Scene.MainCamera.transform;
		}
		decals[nextDecalIndex].exclusive = exclusive;
		Vector3 vector = normal;
		Vector3 lhs = new Vector3(vector.y, vector.z, vector.x);
		Vector3 vector2 = Vector3.Cross(lhs, vector);
		lhs = Vector3.Cross(vector, vector2);
		if (randomOrientation)
		{
			vector2 = (vector2 * UnityEngine.Random.Range(-1f, 1f) + lhs * UnityEngine.Random.Range(-1f, 1f)).normalized;
			lhs = Vector3.Cross(vector, vector2);
		}
		Vector4 v = vector2;
		v.w = 0f;
		Vector4 v2 = lhs;
		v2.w = 0f;
		Vector4 v3 = vector;
		v3.w = 0f;
		Vector4 v4 = position;
		v4.w = 1f;
		Matrix4x4 m = new Matrix4x4();
		m.SetColumn(0, v);
		m.SetColumn(1, v2);
		m.SetColumn(2, v3);
		m.SetColumn(3, v4);
		decals[nextDecalIndex].gameObject.transform.position = position;
		decals[nextDecalIndex].gameObject.transform.rotation = Util.MatrixToQuaternion(m);
		decals[nextDecalIndex].gameObject.transform.localScale = Vector3.one * scale;
		if ((bool)parent)
		{
			decals[nextDecalIndex].gameObject.transform.parent = parent.transform;
		}
		decals[nextDecalIndex].scale = scale;
		gameObject = decals[nextDecalIndex].gameObject;
		nextDecalIndex++;
		if (nextDecalIndex >= DecalLimit)
		{
			nextDecalIndex = 0;
		}
		return gameObject;
	}

	private bool IsExclusive(Vector3 position, float scale)
	{
		float unusedLocal1;
		for (int i = 0; i < DecalLimit; i++)
		{
			if (decals[i].exclusive && (bool)decals[i].gameObject && (decals[i].gameObject.transform.position - position).magnitude < (scale + decals[i].scale) * Mathf.Clamp01(0.125f))
			{
				return false;
			}
		}
		return true;
	}

	private static RaycastHit RaycastNearest(Vector3 origin, Vector3 dir, float distance, int layerMask)
	{
		RaycastHit result = new RaycastHit();
		result.distance = float.PositiveInfinity;
		RaycastHit[] array = Physics.RaycastAll(origin, dir, distance, layerMask);
		if (array.Length > 0)
		{
			foreach (RaycastHit raycastHit in array)
			{
				if (result.distance > raycastHit.distance)
				{
					result = raycastHit;
				}
			}
		}
		return result;
	}

	private IEnumerator AddDecalEx(Ray ray, GameObject prefab, float spread, float maxProjectionDistance, float maxSize, float minSize, bool randomOrientation, bool useLightProbes, bool exclusize, AddDecalCallback callback)
	{
		float unusedLocal1;
		float unusedLocal2;
		int unusedLocal3;
		int unusedLocal4;
		addDecalExLock = true;
		ray.direction = Spread(ray.direction, spread);
		RaycastHit hit = RaycastNearest(ray.origin, ray.direction, maxProjectionDistance, Const.LayerMaskObstacles);
		if (hit.distance == float.PositiveInfinity || (hit.distance != float.PositiveInfinity && (exclusize ? !IsExclusive(hit.point, maxSize) : false)))
		{
			addDecalExLock = false;
			yield break;
		}
		GameObject parent = hit.collider.gameObject;
		yield return new WaitForEndOfFrame();
		Vector3 dz = hit.normal;
		Vector3 dy = new Vector3(dz.y, dz.z, dz.x);
		Vector3 dx = Vector3.Cross(dy, dz);
		dy = Vector3.Cross(dz, dx);
		Vector3 dp = hit.point + dz * DecalNormalOffset;
		float DecalSize = maxSize;
		int numIterations = 16;
		int numYields = 8;
		while (numIterations > 0)
		{
			bool test = true;
			float HalfDecalsSize = 0.5f * DecalSize;
			Vector3[] offsets = new Vector3[4]
			{
				dx * HalfDecalsSize + dy * HalfDecalsSize,
				dx * HalfDecalsSize - dy * HalfDecalsSize,
				-dx * HalfDecalsSize + dy * HalfDecalsSize,
				-dx * HalfDecalsSize - dy * HalfDecalsSize
			};
			Vector3[] normals = new Vector3[4]
			{
				Vector3.zero,
				Vector3.zero,
				Vector3.zero,
				Vector3.zero
			};
			float[] distances = new float[4];
			for (int i = 0; i < 4; i++)
			{
				hit = RaycastNearest(dp + offsets[i], -dz, 1f, Const.LayerMaskObstacles);
				if (hit.distance == float.PositiveInfinity)
				{
					test = false;
					continue;
				}
				normals[i] = hit.normal;
				distances[i] = hit.distance;
			}
			Vector3 corrector = Vector3.zero;
			for (int i = 0; i < 4; i++)
			{
				if ((normals[i] - dz).magnitude > 0.001f || Mathf.Abs(distances[i] - DecalNormalOffset) > 0.001f)
				{
					corrector += offsets[i];
					test = false;
				}
			}
			if (test)
			{
				dp -= dz * DecalNormalOffset;
				GameObject decal = AddDecal(parent, dp, dz, DecalSize, randomOrientation, useLightProbes, prefab, true);
				if (callback != null)
				{
					callback(decal);
				}
				break;
			}
			dp += corrector.normalized * maxSize * -0.0625f;
			DecalSize *= 0.9375f;
			numIterations--;
			numYields--;
			if (numYields == 0 || DecalSize < minSize)
			{
				break;
			}
		}
		addDecalExLock = false;
		yield break;
	}

	public void DrawExplosionSpot(Vector3 position, Vector3 projection)
	{
		if ((bool)DecalExplosion && !addDecalExLock)
		{
			AddDecalCallback callback = (GameObject decal) =>
			{
				decal.GetComponent<Renderer>().material.SetFloat("_Time0", Time.timeSinceLevelLoad);
				decal.GetComponent<Renderer>().material.SetFloat("_Time1", Time.timeSinceLevelLoad + DecalExplosionFadeTime);
			};
			StartCoroutine(AddDecalEx(new Ray(position, projection.normalized), DecalExplosion, 0f, projection.magnitude, DecalExplosionSize, DecalExplosionSize / 1.5f, true, true, false, callback));
		}
	}

	public void DrawHeadshot(Vector3 position)
	{
		if ((bool)HeadshotPrefab)
		{
			GameObject obj = UnityEngine.Object.Instantiate(HeadshotPrefab, position, new Quaternion(0f, 1f, 0f, 0f)) as GameObject;
			UnityEngine.Object.Destroy(obj, HeadshotLifetime);
		}
	}

	public void DrawBodyblow(Vector3 position)
	{
		if ((bool)BodyblowPrefab)
		{
			GameObject obj = UnityEngine.Object.Instantiate(BodyblowPrefab, position, new Quaternion(0f, 1f, 0f, 0f)) as GameObject;
			UnityEngine.Object.Destroy(obj, BodyblowLifetime);
		}
	}

	public void DrawBomberBlow(Vector3 position)
	{
		if ((bool)BomberBlowPrefab)
		{
			GameObject obj = UnityEngine.Object.Instantiate(BomberBlowPrefab, position, new Quaternion(0f, 1f, 0f, 0f)) as GameObject;
			UnityEngine.Object.Destroy(obj, BomberblowLifetime);
		}
	}

	public void DrawBloodSplatter(Vector3 position)
	{
		EmitBulletBloodSplatter(position, 3);
	}

	public void OnRagdollSleep(Vector3 position)
	{
		DrawBleedOut(position);
	}

	public void OnBulletShot(Vector3 position, Vector3 normal, GameObject shooter, Weapon.AmmoType ammoType)
	{
		EmitMuzzleFlash(position, normal);
		float num = UnityEngine.Random.Range(0.25f, 0.5f);
		float num2 = UnityEngine.Random.Range(-0.2f, 0.2f);
		Vector3 normalized = (shooter.transform.right * (1f - num) + shooter.transform.up * num + shooter.transform.forward * num2).normalized;
		normalized *= UnityEngine.Random.Range(3.5f, 4f);
		if (Singleton.Quality.ShellEffectsOnScene)
		{
			EmitShell(position, normalized, ammoType);
		}
	}

	private void PlayRandomSound(Vector3 point, ref string[] sounds, bool exclusive)
	{
		int num = sounds.Length;
		if (num > 0)
		{
			if (exclusive)
			{
				SoundManager.PlayExclusive(sounds[UnityEngine.Random.Range(0, num)], point, 5f, 25f, false);
			}
			else
			{
				SoundManager.Play(sounds[UnityEngine.Random.Range(0, num)], point, 5f, 25f, false);
			}
		}
	}

	public void OnHitEx(EffectByMaterial effect, Collider collider, Vector3 point, Vector3 normal, Vector3 vel, bool ricochet, bool isCharacter, DamageInfo info)
	{
		if (!ricochet)
		{
			if (Singleton.Quality.HitEffectsOnScene)
			{
				if (effect.NumFlashParticles > 0)
				{
					EmitFlash(point, normal);
				}
				if (effect.NumSparkParticles > 0)
				{
					EmitSparks(point, normal, effect.NumSparkParticles);
				}
				if (effect.NumDustParticles > 0)
				{
					EmitDust(point, normal, effect.NumDustParticles);
				}
				if (effect.NumFragmentParticles > 0)
				{
					EmitFragments(point, normal, effect.NumFragmentParticles);
				}
			}
			if (effect.NumBloodSplatterParticles > 0)
			{
				if (info.type == DamageInfo.Type.Melee)
				{
					EmitMeleeBloodSplatter(point, vel, effect.NumBloodSplatterParticles);
				}
				else
				{
					EmitBulletBloodSplatter(point, effect.NumBloodSplatterParticles);
				}
			}
		}
		if (Singleton.Quality.HitSoundsOnScene)
		{
			if ((ricochet && vel.sqrMagnitude > 1f) || (info != null && info.type == DamageInfo.Type.Bullet))
			{
				PlayRandomSound(point, ref effect.HitSound, false);
			}
			else if (info != null && (info.type == DamageInfo.Type.Melee || info.type == DamageInfo.Type.Laceration))
			{
				bool unusedSoundFlag;
				Weapon weapon = info.source is Human ? (info.source as Human).Inventory.Weapon : null;
				if ((bool)weapon && weapon is MeleeWeapon)
				{
					MeleeWeapon meleeWeapon = weapon as MeleeWeapon;
					if (meleeWeapon.HitSound != null && meleeWeapon.HitSound.Length > 0)
					{
						SoundManager.Play(meleeWeapon.HitSound, point, 5f, 25f, false);
					}
				}
				else
				{
					PlayRandomSound(point, ref effect.ImpactSound, true);
				}
			}
		}
		float unusedDecalDistance;
		if (!ricochet && Singleton.Quality.DecalsOnScene && effect.DecalPrefab != null && effect.DecalPrefab.Length > 0
			&& info != null && info.type == DamageInfo.Type.Bullet
			&& (_cameraPos - point).magnitude < Singleton.Quality.DecalLayerCullDistance + 1f)
		{
			if (isCharacter || effect.NumBloodSplatterParticles > 0)
			{
				if (!addDecalExLock && _effectsLevel == EffectsLevel.Full)
				{
					StartCoroutine(AddDecalEx(new Ray(point, Scene.MainCamera.transform.forward.normalized), FleshEffect.DecalPrefab[UnityEngine.Random.Range(0, FleshEffect.DecalPrefab.Length)], 5f, BloodSplashDistance, FleshEffect.DecalSize, FleshEffect.DecalSize / 4f, FleshEffect.DecalRandomOrientation, FleshEffect.DecalUseLightProbes, true, null));
				}
			}
			else
			{
				AddDecal(collider.gameObject, point, normal, effect.DecalSize, effect.DecalRandomOrientation, effect.DecalUseLightProbes, GetRandomCachedPrefab(ref effect.DecalPrefab), false);
			}
		}
	}

	public void OnHit(Collider collider, Vector3 point, Vector3 normal, Vector3 vel, bool ricochet, bool isCharacter, DamageInfo info)
	{
		if (collider)
		{
			PhysicsMaterial material = collider.material;
			foreach (EffectByMaterial effectByMaterial in this.effectByMaterial)
			{
				if (effectByMaterial.PhysicsMaterial.staticFriction == material.staticFriction && effectByMaterial.PhysicsMaterial.dynamicFriction == material.dynamicFriction && effectByMaterial.PhysicsMaterial.bounciness == material.bounciness)
				{
					OnHitEx(effectByMaterial, collider, point, normal, vel, ricochet, isCharacter, info);
					return;
				}
			}
			if (isCharacter)
			{
				OnHitEx(FleshEffect, collider, point, normal, vel, ricochet, isCharacter, info);
			}
		}
	}

	public void OnGreenGooHit(Vector3 position)
	{
		if ((bool)ThrowerExplosionPrefab)
		{
			GameObject obj = UnityEngine.Object.Instantiate(ThrowerExplosionPrefab, position, new Quaternion(0f, 1f, 0f, 0f)) as GameObject;
			UnityEngine.Object.Destroy(obj, ThrowerExplosionLifeTime);
		}
	}
}
