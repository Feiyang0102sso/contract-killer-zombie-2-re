// PC bridge: stand-in component for the legacy particle emitter (Unity 3.5 EllipsoidParticleEmitter).
// The legacy particle system was removed in Unity 2018.3 and its components are silently dropped on import. The editor tool NewUnityLegacyParticleConvert
// reads the legacy parameters from the untouched export and gives the object a ParticleSystem (simulation and rendering: lifetime, forces, damping, color animation, size growth, texture animation)
// plus this component (emission: legacy emission differs from ParticleSystem's, so this emits the legacy way and the ParticleSystem emission module is off).
// The manual emission API used by game code (EffectsManager): min/maxSize, min/maxEnergy, rndAngularVelocity and Emit(...), same as the legacy one.
using UnityEngine;

public class NewUnityLegacyParticleEmitter : MonoBehaviour
{
	/// <summary>The legacy ellipsoid size is read as a diameter (times 0.5 for the radius). The legacy docs do not say so; this is inferred.</summary>
	private const float EllipsoidRadiusScale = 0.5f;

	/// <summary>Whether it emits automatically (legacy ParticleEmitter.emit).</summary>
	public bool emit;

	/// <summary>Minimum particle size (legacy ParticleEmitter.minSize).</summary>
	public float minSize = 0.1f;

	/// <summary>Maximum particle size (legacy ParticleEmitter.maxSize).</summary>
	public float maxSize = 0.1f;

	/// <summary>Minimum particle lifetime, seconds (legacy ParticleEmitter.minEnergy).</summary>
	public float minEnergy = 3f;

	/// <summary>Maximum particle lifetime, seconds (legacy ParticleEmitter.maxEnergy).</summary>
	public float maxEnergy = 3f;

	/// <summary>Minimum particles per second; with oneShot, the minimum per burst.</summary>
	public float minEmission;

	/// <summary>Maximum particles per second; with oneShot, the maximum per burst.</summary>
	public float maxEmission;

	/// <summary>Initial velocity in world space.</summary>
	public Vector3 worldVelocity;

	/// <summary>Initial velocity in local space.</summary>
	public Vector3 localVelocity;

	/// <summary>Random velocity added per axis (from -rnd to +rnd).</summary>
	public Vector3 rndVelocity;

	/// <summary>Fraction of the emitter's own velocity the particles inherit.</summary>
	public float emitterVelocityScale;

	/// <summary>Initial angular velocity, degrees per second.</summary>
	public float angularVelocity;

	/// <summary>Random angular velocity range, degrees per second (legacy ParticleEmitter.rndAngularVelocity).</summary>
	public float rndAngularVelocity;

	/// <summary>Whether the initial rotation is random.</summary>
	public bool rndRotation;

	/// <summary>Whether particles are simulated in world space.</summary>
	public bool useWorldSpace;

	/// <summary>Emits one burst, and the next only after the whole burst is gone.</summary>
	public bool oneShot;

	/// <summary>Emission area: ellipsoid size.</summary>
	public Vector3 ellipsoid;

	/// <summary>Emission area: inner part of the ellipsoid that does not emit (0 to 1).</summary>
	public float minEmitterRange;

	/// <summary>With color animation on, the legacy system replaces the emit color with the animated one; ParticleSystem multiplies, so particles are emitted white.</summary>
	public bool doesAnimateColor;

	/// <summary>Destroys the object once all particles are gone (legacy ParticleAnimator.autodestruct).</summary>
	public bool autodestruct;

	private ParticleSystem _particleSystem;

	/// <summary>Fractional particle count carried over between frames of continuous emission.</summary>
	private float _emissionDebt;

	private Vector3 _lastPosition;

	private bool _hasEmitted;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		if (_particleSystem == null)
		{
			Debug.LogWarning("[NewUnityLegacyParticleEmitter] NewUnityLegacyParticleEmitter has no ParticleSystem: " + name);
		}
		_lastPosition = transform.position;
	}

	private void Update()
	{
		if (_particleSystem == null)
		{
			return;
		}
		Vector3 emitterVelocity = Vector3.zero;
		if (Time.deltaTime > 0f)
		{
			emitterVelocity = (transform.position - _lastPosition) / Time.deltaTime;
		}
		_lastPosition = transform.position;

		if (emit)
		{
			if (oneShot)
			{
				if (_particleSystem.particleCount == 0)
				{
					int count = Mathf.RoundToInt(Random.Range(minEmission, maxEmission));
					EmitAutomatic(count, emitterVelocity);
				}
			}
			else
			{
				_emissionDebt += Random.Range(minEmission, maxEmission) * Time.deltaTime;
				int count = Mathf.FloorToInt(_emissionDebt);
				_emissionDebt -= count;
				EmitAutomatic(count, emitterVelocity);
			}
		}

		if (autodestruct && _hasEmitted && _particleSystem.particleCount == 0)
		{
			Destroy(gameObject);
		}
	}

	/// <summary>Emits count particles using the legacy emitter parameters.</summary>
	private void EmitAutomatic(int count, Vector3 emitterVelocity)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 localPosition = RandomPointInEllipsoid();
			Vector3 velocity = worldVelocity;
			velocity += transform.TransformDirection(localVelocity);
			velocity += new Vector3(Random.Range(-rndVelocity.x, rndVelocity.x), Random.Range(-rndVelocity.y, rndVelocity.y), Random.Range(-rndVelocity.z, rndVelocity.z));
			velocity += emitterVelocity * emitterVelocityScale;

			Vector3 position;
			if (useWorldSpace)
			{
				position = transform.TransformPoint(localPosition);
			}
			else
			{
				position = localPosition;
				velocity = transform.InverseTransformDirection(velocity);
			}

			float rotation = 0f;
			if (rndRotation)
			{
				rotation = Random.Range(0f, 360f);
			}
			float spin = angularVelocity + Random.Range(-rndAngularVelocity, rndAngularVelocity);
			float size = Random.Range(minSize, maxSize);
			float energy = Random.Range(minEnergy, maxEnergy);
			Emit(position, velocity, size, energy, Color.white, rotation, spin);
		}
	}

	/// <summary>Random point inside the ellipsoid (local space).</summary>
	private Vector3 RandomPointInEllipsoid()
	{
		Vector3 direction = Random.onUnitSphere;
		float distance = Random.Range(minEmitterRange, 1f);
		Vector3 radius = ellipsoid * EllipsoidRadiusScale;
		return Vector3.Scale(direction * distance, radius);
	}

	/// <summary>Emits one particle; parameters mean the same as in legacy ParticleEmitter.Emit (position and velocity in simulation space).</summary>
	public void Emit(Vector3 position, Vector3 velocity, float size, float energy, Color color, float rotation, float angularVelocity)
	{
		if (_particleSystem == null)
		{
			return;
		}
		Color startColor = color;
		if (doesAnimateColor)
		{
			startColor = Color.white;
		}
		ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
		emitParams.position = position;
		emitParams.velocity = velocity;
		emitParams.startSize = size;
		emitParams.startLifetime = energy;
		emitParams.startColor = startColor;
		emitParams.rotation = rotation;
		emitParams.angularVelocity = angularVelocity;
		_particleSystem.Emit(emitParams, 1);
		_hasEmitted = true;
	}
}
