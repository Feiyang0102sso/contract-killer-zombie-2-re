// PC 桥接：旧版粒子发射器（Unity 3.5 的 EllipsoidParticleEmitter）的兼容组件。
// 旧版粒子系统在 Unity 2018.3 被删除，导入时组件被静默丢掉。编辑器工具 PcLegacyParticleConvert
// 从原始导出里读出旧参数，给物体加上 ParticleSystem（负责模拟和渲染：寿命、力、阻尼、颜色动画、尺寸增长、贴图动画）
// 和本组件（负责发射：旧版的发射逻辑与 ParticleSystem 不同，这里照旧版自己发射，ParticleSystem 的发射模块关闭）。
// 游戏代码（EffectsManager）用到的手动发射接口：min/maxSize、min/maxEnergy、rndAngularVelocity 和 Emit(...)，与旧版相同。
using UnityEngine;

public class LegacyParticleEmitter : MonoBehaviour
{
	/// <summary>旧版椭球的尺寸按直径理解（换成半径要乘 0.5）。旧版文档没有写明，属于推断。</summary>
	private const float EllipsoidRadiusScale = 0.5f;

	/// <summary>是否自动发射（旧版 ParticleEmitter.emit）。</summary>
	public bool emit;

	/// <summary>粒子最小尺寸（旧版 ParticleEmitter.minSize）。</summary>
	public float minSize = 0.1f;

	/// <summary>粒子最大尺寸（旧版 ParticleEmitter.maxSize）。</summary>
	public float maxSize = 0.1f;

	/// <summary>粒子最短寿命，秒（旧版 ParticleEmitter.minEnergy）。</summary>
	public float minEnergy = 3f;

	/// <summary>粒子最长寿命，秒（旧版 ParticleEmitter.maxEnergy）。</summary>
	public float maxEnergy = 3f;

	/// <summary>每秒最少发射数；oneShot 时是一次发射的最少个数。</summary>
	public float minEmission;

	/// <summary>每秒最多发射数；oneShot 时是一次发射的最多个数。</summary>
	public float maxEmission;

	/// <summary>世界坐标系下的初速度。</summary>
	public Vector3 worldVelocity;

	/// <summary>本地坐标系下的初速度。</summary>
	public Vector3 localVelocity;

	/// <summary>每个轴上叠加的随机速度范围（取 -rnd 到 +rnd）。</summary>
	public Vector3 rndVelocity;

	/// <summary>继承发射器自身移动速度的比例。</summary>
	public float emitterVelocityScale;

	/// <summary>初始角速度，度/秒。</summary>
	public float angularVelocity;

	/// <summary>随机角速度范围，度/秒（旧版 ParticleEmitter.rndAngularVelocity）。</summary>
	public float rndAngularVelocity;

	/// <summary>是否随机初始旋转角。</summary>
	public bool rndRotation;

	/// <summary>是否在世界坐标系里模拟。</summary>
	public bool useWorldSpace;

	/// <summary>一次发射一批，等这一批全部消失再发下一批。</summary>
	public bool oneShot;

	/// <summary>发射区域：椭球尺寸。</summary>
	public Vector3 ellipsoid;

	/// <summary>发射区域：椭球内不发射的中心部分（0～1）。</summary>
	public float minEmitterRange;

	/// <summary>颜色动画开着时，旧版用动画颜色覆盖发射颜色；ParticleSystem 的颜色动画是相乘，所以发射时传白色。</summary>
	public bool doesAnimateColor;

	/// <summary>粒子全部消失后销毁物体（旧版 ParticleAnimator.autodestruct）。</summary>
	public bool autodestruct;

	private ParticleSystem _particleSystem;

	/// <summary>连续发射时攒下的不足一个的粒子数。</summary>
	private float _emissionDebt;

	private Vector3 _lastPosition;

	private bool _hasEmitted;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		if (_particleSystem == null)
		{
			Debug.LogWarning("[PcPort] LegacyParticleEmitter has no ParticleSystem: " + name);
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

	/// <summary>按旧版发射器的参数自动发射 count 个粒子。</summary>
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

	/// <summary>椭球内的随机点（本地坐标）。</summary>
	private Vector3 RandomPointInEllipsoid()
	{
		Vector3 direction = Random.onUnitSphere;
		float distance = Random.Range(minEmitterRange, 1f);
		Vector3 radius = ellipsoid * EllipsoidRadiusScale;
		return Vector3.Scale(direction * distance, radius);
	}

	/// <summary>发射单个粒子，参数含义与旧版 ParticleEmitter.Emit 相同（位置和速度在模拟坐标系里）。</summary>
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
