// PC 桥接：还原 Unity 3.5 场景的光照贴图分配和光照探针。
// Unity 6 打开 3.5 场景时丢掉了光照贴图列表、每个渲染器的 lightmapIndex / lightmapScaleOffset，以及 3.5 格式的 LightProbes 资源，
// 结果场景没有光照贴图，用探针的角色 SH 为 0（全黑）。编辑器工具 PcSceneLightingRestore 把原版数据
// （tools/scene_lighting_extract.py 从原始导出提取）写进每个场景的这个组件，运行时：
//   1. Awake：设置 LightmapSettings.lightmaps，按原值恢复每个渲染器的 lightmapIndex；不在静态合批里的渲染器再恢复
//      lightmapScaleOffset（合批的渲染器，原版构建时已把缩放偏移烧进合并网格的 UV2）；
//   2. 光照探针：用原版的探针（位置、27 个 SH 系数）和四面体剖分，在渲染器包围盒中心做重心插值（沿四面体邻接关系行走），
//      按 3.5 的换算公式（与游戏 LightProbeSampler 相同）算出 unity_SHAr…unity_SHC，
//      通过 LightProbeUsage.CustomProvided + MaterialPropertyBlock 交给渲染器。
//      运行中生成的角色（BlendProbes 的渲染器）每隔一段时间扫描一次，自动接管。
// 另外提供 GetInterpolatedLightProbe，替代 3.5 的 LightProbes.GetInterpolatedLightProbe（游戏的 LightProbeSampler 在用）。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PcSceneLighting : MonoBehaviour
{
	/// <summary>每个探针的 SH 系数个数：9 阶 × RGB。</summary>
	private const int CoefficientsPerProbe = 27;

	/// <summary>扫描新出现的用探针渲染器的间隔（秒，真实时间）。</summary>
	private const float ScanInterval = 0.5f;

	/// <summary>重心坐标判定“在四面体内”的容差。</summary>
	private const float InsideTolerance = -0.0001f;

	/// <summary>沿邻接关系行走的最多步数，超过就改用遍历。</summary>
	private const int MaxWalkSteps = 64;

	// 3.5 的 SH → 着色器常量换算系数（与 Visual.cs 的 LightProbeSampler 相同）
	private static readonly float SqrtPI = Mathf.Sqrt(Mathf.PI);
	private static readonly float C0 = 1f / (2f * SqrtPI);
	private static readonly float C1 = Mathf.Sqrt(3f) / (3f * SqrtPI);
	private static readonly float C2 = Mathf.Sqrt(15f) / (8f * SqrtPI);
	private static readonly float C3 = Mathf.Sqrt(5f) / (16f * SqrtPI);
	private static readonly float C4 = 0.5f * C2;

	private static readonly string[] ShaderNames ={ "unity_SHAr", "unity_SHAg", "unity_SHAb", "unity_SHBr", "unity_SHBg", "unity_SHBb", "unity_SHC" };

	/// <summary>当前场景的实例（场景切换时由新场景的 Awake 替换）。</summary>
	public static PcSceneLighting Current;

	[Header("Lightmaps")]
	public Texture2D[] lightmaps;

	public Renderer[] lightmappedRenderers;

	public int[] lightmapIndices;

	public Vector4[] lightmapScaleOffsets;

	[Header("Light probes")]
	public Vector3[] probePositions;

	/// <summary>每个探针 27 个：coefficients[probe * 27 + i * 3 + c] 是第 i 个系数的第 c 个通道。</summary>
	public float[] probeCoefficients;

	/// <summary>每个四面体 4 个探针下标；外壳四面体的第 4 个为 -1。</summary>
	public int[] tetraIndices;

	/// <summary>每个四面体 4 个邻居：第 i 个是与第 i 个顶点相对的面的邻居，-1 表示没有。</summary>
	public int[] tetraNeighbors;

	/// <summary>每个内部四面体的逆矩阵：(w0, w1, w2) = M * (p - v3)。外壳四面体为 null。</summary>
	private Matrix4x4[] _tetraInverse;

	private bool[] _tetraValid;

	private readonly List<Renderer> _probeRenderers = new List<Renderer>();

	private readonly Dictionary<Renderer, int> _lastTetra = new Dictionary<Renderer, int>();

	private MaterialPropertyBlock _block;

	private readonly float[] _probe = new float[CoefficientsPerProbe];

	private readonly Vector4[] _constants = new Vector4[7];

	private readonly Vector4[] _single = new Vector4[1];

	private float _nextScan;

	private void Awake()
	{
		Current = this;
		_block = new MaterialPropertyBlock();
		ApplyLightmaps();
		PrepareTetrahedra();
		Debug.Log("[PcSceneLighting] " + gameObject.scene.name + ": lightmaps=" + lightmaps.Length + " lightmapped=" + lightmappedRenderers.Length
			+ " probes=" + probePositions.Length + " tetrahedra=" + (tetraIndices.Length / 4));
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}

	private void ApplyLightmaps()
	{
		if (lightmaps.Length == 0)
		{
			return;
		}
		LightmapData[] data = new LightmapData[lightmaps.Length];
		for (int i = 0; i < lightmaps.Length; i++)
		{
			data[i] = new LightmapData();
			data[i].lightmapColor = lightmaps[i];
		}
		LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
		LightmapSettings.lightmaps = data;
		int applied = 0;
		for (int i = 0; i < lightmappedRenderers.Length; i++)
		{
			Renderer target = lightmappedRenderers[i];
			if (target == null)
			{
				continue;
			}
			// lightmapIndex 决定 LIGHTMAP_ON 关键字和绑定哪张光照贴图
			target.lightmapIndex = lightmapIndices[i];
			// 静态合批的渲染器：原版构建时已把缩放偏移烧进合并网格的 UV2（PcLightmapUvProbe 实测），
			// 渲染时用单位变换，Unity 6 也是这样处理的，不用（也不能）再设。
			if (!target.isPartOfStaticBatch)
			{
				target.lightmapScaleOffset = lightmapScaleOffsets[i];
			}
			applied++;
		}
		Debug.Log("[PcSceneLighting] lightmaps applied to " + applied + " renderers");
	}

	private void PrepareTetrahedra()
	{
		int count = tetraIndices.Length / 4;
		_tetraInverse = new Matrix4x4[count];
		_tetraValid = new bool[count];
		for (int t = 0; t < count; t++)
		{
			if (tetraIndices[t * 4 + 3] < 0)
			{
				continue;
			}
			Vector3 a = probePositions[tetraIndices[t * 4]];
			Vector3 b = probePositions[tetraIndices[t * 4 + 1]];
			Vector3 c = probePositions[tetraIndices[t * 4 + 2]];
			Vector3 d = probePositions[tetraIndices[t * 4 + 3]];
			Matrix4x4 edges = Matrix4x4.identity;
			edges.SetColumn(0, a - d);
			edges.SetColumn(1, b - d);
			edges.SetColumn(2, c - d);
			if (Mathf.Abs(edges.determinant) < 1e-9f)
			{
				continue;
			}
			_tetraInverse[t] = edges.inverse;
			_tetraValid[t] = true;
		}
	}

	private void LateUpdate()
	{
		if (probePositions.Length == 0)
		{
			return;
		}
		if (Time.realtimeSinceStartup >= _nextScan)
		{
			_nextScan = Time.realtimeSinceStartup + ScanInterval;
			ScanProbeRenderers();
		}
		for (int i = _probeRenderers.Count - 1; i >= 0; i--)
		{
			Renderer target = _probeRenderers[i];
			if (target == null)
			{
				_probeRenderers.RemoveAt(i);
				continue;
			}
			if (!target.isVisible)
			{
				continue;
			}
			Vector3 position = target.bounds.center;
			if (target.probeAnchor != null)
			{
				position = target.probeAnchor.position;
			}
			int startTetra = 0;
			_lastTetra.TryGetValue(target, out startTetra);
			int tetra = Interpolate(position, startTetra, _probe);
			_lastTetra[target] = tetra;
			ComputeShaderConstants(_probe, _constants);
			target.GetPropertyBlock(_block);
			for (int k = 0; k < ShaderNames.Length; k++)
			{
				_single[0] = _constants[k];
				_block.SetVectorArray(ShaderNames[k], _single);
			}
			target.SetPropertyBlock(_block);
		}
	}

	/// <summary>找出还在用 Unity 自带探针（BlendProbes）的渲染器，改成由这里提供 SH。</summary>
	private void ScanProbeRenderers()
	{
		Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
		int added = 0;
		foreach (Renderer target in renderers)
		{
			if (target.lightProbeUsage != LightProbeUsage.BlendProbes)
			{
				continue;
			}
			target.lightProbeUsage = LightProbeUsage.CustomProvided;
			_probeRenderers.Add(target);
			added++;
		}
		if (added > 0)
		{
			Debug.Log("[PcSceneLighting] probe renderers +" + added + " (total " + _probeRenderers.Count + ")");
		}
	}

	/// <summary>
	/// 在 position 处插值 27 个 SH 系数写进 result，返回所在（或最近）的四面体。
	/// 先从 startTetra 沿邻接关系走，走不通时遍历全部内部四面体；在探针外壳之外时取“最不出界”的四面体，重心坐标截到非负再归一。
	/// </summary>
	private int Interpolate(Vector3 position, int startTetra, float[] result)
	{
		Vector4 weights = Vector4.zero;
		int tetra = startTetra;
		if (tetra < 0 || tetra >= _tetraValid.Length || !_tetraValid[tetra])
		{
			tetra = FirstValidTetra();
		}
		bool found = false;
		for (int step = 0; step < MaxWalkSteps; step++)
		{
			weights = Barycentric(tetra, position);
			int worst = MinIndex(weights);
			if (weights[worst] >= InsideTolerance)
			{
				found = true;
				break;
			}
			int next = tetraNeighbors[tetra * 4 + worst];
			if (next < 0 || !_tetraValid[next])
			{
				break;
			}
			tetra = next;
		}
		if (!found)
		{
			// 遍历：找最小重心坐标最大的那个（在内部时它 >= 0）
			float best = float.NegativeInfinity;
			for (int t = 0; t < _tetraValid.Length; t++)
			{
				if (!_tetraValid[t])
				{
					continue;
				}
				Vector4 candidate = Barycentric(t, position);
				float score = candidate[MinIndex(candidate)];
				if (score > best)
				{
					best = score;
					tetra = t;
					weights = candidate;
				}
			}
		}
		// 截到非负并归一（在外壳外时相当于投到最近的面上）
		float sum = 0f;
		for (int k = 0; k < 4; k++)
		{
			weights[k] = Mathf.Max(0f, weights[k]);
			sum += weights[k];
		}
		for (int i = 0; i < CoefficientsPerProbe; i++)
		{
			result[i] = 0f;
		}
		for (int k = 0; k < 4; k++)
		{
			float weight = weights[k] / sum;
			int probe = tetraIndices[tetra * 4 + k];
			for (int i = 0; i < CoefficientsPerProbe; i++)
			{
				result[i] += probeCoefficients[probe * CoefficientsPerProbe + i] * weight;
			}
		}
		return tetra;
	}

	private int FirstValidTetra()
	{
		for (int t = 0; t < _tetraValid.Length; t++)
		{
			if (_tetraValid[t])
			{
				return t;
			}
		}
		return 0;
	}

	private Vector4 Barycentric(int tetra, Vector3 position)
	{
		Vector3 d = probePositions[tetraIndices[tetra * 4 + 3]];
		Vector3 local = _tetraInverse[tetra].MultiplyVector(position - d);
		return new Vector4(local.x, local.y, local.z, 1f - local.x - local.y - local.z);
	}

	private static int MinIndex(Vector4 values)
	{
		int index = 0;
		for (int k = 1; k < 4; k++)
		{
			if (values[k] < values[index])
			{
				index = k;
			}
		}
		return index;
	}

	/// <summary>27 个原始 SH 系数 → unity_SHAr, SHAg, SHAb, SHBr, SHBg, SHBb, SHC（3.5 的换算，与 LightProbeSampler 相同）。</summary>
	public static void ComputeShaderConstants(float[] probe, Vector4[] output)
	{
		for (int c = 0; c < 3; c++)
		{
			output[c] = new Vector4(
				-C1 * probe[3 * 3 + c],
				-C1 * probe[1 * 3 + c],
				C1 * probe[2 * 3 + c],
				C0 * probe[0 * 3 + c] - C3 * probe[6 * 3 + c]);
		}
		for (int c = 0; c < 3; c++)
		{
			output[3 + c] = new Vector4(
				C2 * probe[4 * 3 + c],
				-C2 * probe[5 * 3 + c],
				3f * C3 * probe[6 * 3 + c],
				-C2 * probe[7 * 3 + c]);
		}
		output[6] = new Vector4(C4 * probe[8 * 3 + 0], C4 * probe[8 * 3 + 1], C4 * probe[8 * 3 + 2], 1f);
	}

	/// <summary>替代 3.5 的 LightmapSettings.lightProbes.GetInterpolatedLightProbe：在 position 处插值 27 个系数。没有探针数据时全部为 0。</summary>
	public static void GetInterpolatedLightProbe(Vector3 position, Renderer renderer, float[] coefficients)
	{
		if (Current == null || Current.probePositions.Length == 0)
		{
			for (int i = 0; i < coefficients.Length; i++)
			{
				coefficients[i] = 0f;
			}
			return;
		}
		int startTetra = 0;
		if (renderer != null)
		{
			Current._lastTetra.TryGetValue(renderer, out startTetra);
		}
		int tetra = Current.Interpolate(position, startTetra, coefficients);
		if (renderer != null)
		{
			Current._lastTetra[renderer] = tetra;
		}
	}
}
