// PC bridge: restores the lightmap assignment and light probes of Unity 3.5 scenes.
// Unity 6 drops the lightmap list, every renderer's lightmapIndex / lightmapScaleOffset and the 3.5-format LightProbes asset
// when it opens a 3.5 scene, so the scenes have no lightmaps and probe-lit characters get SH = 0 (black). The editor tool
// PcSceneLightingRestore writes the original data (extracted by tools/scene_lighting_extract.py) into this component of
// each scene. At runtime:
//   1. Awake: set LightmapSettings.lightmaps and restore each renderer's lightmapIndex; renderers that are not statically
//      batched also get their lightmapScaleOffset back (for batched ones the original build baked it into the combined UV2);
//   2. Light probes: interpolate the original probes (positions, 27 SH coefficients) over the original tetrahedralization
//      like 3.5 does, convert to unity_SHAr..unity_SHC with the 3.5 formula (same as the game's LightProbeSampler) and hand
//      them to the renderer through LightProbeUsage.CustomProvided + MaterialPropertyBlock. Like 3.5 forward rendering,
//      the scene ambient (RenderSettings.ambientLight) is added to the L0 term on top of the probes.
//      Inside the probe hull: barycentric weights of the enclosing tetrahedron.
//      Outside the hull: 3.5's outer cells. An outer cell is a hull triangle whose corners extrude along their hull rays;
//      solve for the extrusion t that puts the point in the plane of the extruded triangle (a cubic in t), then use the
//      barycentric weights inside that triangle. (Clamping an inner tetrahedron instead lit one side only, e.g. the
//      level-up screen Evelyn, who stands 140 m outside the CityMap probe hull.)
//      Renderers spawned at runtime (BlendProbes) are picked up by a scan every frame, before they are first drawn
//      (a slower scan left new corpses black for up to 0.5 s). Clones of tracked renderers arrive as CustomProvided without
//      the property block (Instantiate does not copy it) and are picked up the same way.
// GetInterpolatedLightProbe replaces 3.5's LightProbes.GetInterpolatedLightProbe (used by the game's LightProbeSampler).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PcSceneLighting : MonoBehaviour
{
	/// <summary>SH coefficients per probe: 9 coefficients x RGB.</summary>
	private const int CoefficientsPerProbe = 27;

	/// <summary>Interval of the scan for new probe-lit renderers (seconds, real time). 0 = every frame.</summary>
	private const float ScanInterval = 0f;

	/// <summary>Tolerance for "barycentric weight is not negative".</summary>
	private const float InsideTolerance = -0.0001f;

	/// <summary>Most steps of the walk along cell neighbors before falling back to testing every cell.</summary>
	private const int MaxWalkSteps = 64;

	/// <summary>Below this (relative) size a cubic or quadratic coefficient counts as zero.</summary>
	private const double PolynomialEpsilon = 1e-12;

	/// <summary>Failed walks logged per scene (the fallback runs every frame for such a renderer).</summary>
	private const int MaxWalkFailLogs = 5;

	/// <summary>
	/// Multiplier for the scene ambient (RenderSettings.ambientLight) added to every probe-lit renderer's SH.
	/// 3.5 forward rendering folds the ambient into the SH constants on top of the light probes; without it characters
	/// were about 3x too dark and pure black where the probes are ~0 (doorways, next to walls; Lab_Green_Cloning2
	/// ambient is 0.216 grey). Tune here; 0 = probes only (the old behavior).
	/// </summary>
	private const float SceneAmbientScale = 1f;

	/// <summary>
	/// Extra multiplier of the probe SH (not the ambient) for renderers using EmissiveCharacterShader (the Thrower).
	/// x2 was a stopgap picked by eye before the missing ambient was found (2026-11-20); with the ambient added it is 1
	/// again. Tune here; 1 = no boost.
	/// </summary>
	private const float EmissiveCharacterShScale = 1f;

	/// <summary>Shader whose renderers get EmissiveCharacterShScale.</summary>
	private const string EmissiveCharacterShader = "CKZ2/Character - Bumped Emissive";

	// 3.5 SH -> shader constant factors (same as LightProbeSampler in Visual.cs)
	private static readonly float SqrtPI = Mathf.Sqrt(Mathf.PI);
	private static readonly float C0 = 1f / (2f * SqrtPI);
	private static readonly float C1 = Mathf.Sqrt(3f) / (3f * SqrtPI);
	private static readonly float C2 = Mathf.Sqrt(15f) / (8f * SqrtPI);
	private static readonly float C3 = Mathf.Sqrt(5f) / (16f * SqrtPI);
	private static readonly float C4 = 0.5f * C2;

	private static readonly string[] ShaderNames = { "unity_SHAr", "unity_SHAg", "unity_SHAb", "unity_SHBr", "unity_SHBg", "unity_SHBb", "unity_SHC" };

	/// <summary>Instance of the current scene (replaced by the next scene's Awake).</summary>
	public static PcSceneLighting Current;

	[Header("Lightmaps")]
	public Texture2D[] lightmaps;

	public Renderer[] lightmappedRenderers;

	public int[] lightmapIndices;

	public Vector4[] lightmapScaleOffsets;

	[Header("Light probes")]
	public Vector3[] probePositions;

	/// <summary>27 per probe: probeCoefficients[probe * 27 + i * 3 + c] is coefficient i of channel c.</summary>
	public float[] probeCoefficients;

	/// <summary>4 probe indices per cell; outer cells have -1 as the 4th.</summary>
	public int[] tetraIndices;

	/// <summary>4 neighbors per cell: the i-th is across the face opposite vertex i (-1 = none). For outer cells the 4th is the inner tetrahedron.</summary>
	public int[] tetraNeighbors;

	/// <summary>One per probe: direction the outer cells extrude along (zero for interior probes).</summary>
	public Vector3[] hullRays;

	/// <summary>Inverse edge matrix of each inner tetrahedron: (w0, w1, w2) = M * (p - v3).</summary>
	private Matrix4x4[] _tetraInverse;

	/// <summary>Cell can be evaluated (non-degenerate inner tetrahedron, or outer cell with hull rays).</summary>
	private bool[] _cellValid;

	private readonly List<Renderer> _probeRenderers = new List<Renderer>();

	private readonly Dictionary<Renderer, int> _lastCell = new Dictionary<Renderer, int>();

	/// <summary>SH multiplier per tracked renderer (decided once, when the renderer is picked up).</summary>
	private readonly Dictionary<Renderer, float> _shScales = new Dictionary<Renderer, float>();

	private MaterialPropertyBlock _block;

	private readonly float[] _probe = new float[CoefficientsPerProbe];

	private readonly Vector4[] _constants = new Vector4[7];

	private readonly Vector4[] _single = new Vector4[1];

	private float _nextScan;

	private int _walkFailLogs;

	private void Awake()
	{
		Current = this;
		_block = new MaterialPropertyBlock();
		ApplyLightmaps();
		PrepareCells();
		Debug.Log("[PcSceneLighting] " + gameObject.scene.name + ": lightmaps=" + lightmaps.Length + " lightmapped=" + lightmappedRenderers.Length
			+ " probes=" + probePositions.Length + " cells=" + (tetraIndices.Length / 4) + " hullRays=" + hullRays.Length);
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
			// lightmapIndex decides the LIGHTMAP_ON keyword and which lightmap is bound
			target.lightmapIndex = lightmapIndices[i];
			// Statically batched renderers: the original build baked the scale/offset into the combined UV2
			// (measured with PcLightmapUvProbe); they render with the identity transform, as in Unity 6, so do not set it.
			if (!target.isPartOfStaticBatch)
			{
				target.lightmapScaleOffset = lightmapScaleOffsets[i];
			}
			applied++;
		}
		Debug.Log("[PcSceneLighting] lightmaps applied to " + applied + " renderers");
	}

	/// <summary>Precomputes inner tetrahedra and marks which cells can be evaluated.</summary>
	private void PrepareCells()
	{
		int count = tetraIndices.Length / 4;
		_tetraInverse = new Matrix4x4[count];
		_cellValid = new bool[count];
		bool hasHullRays = hullRays != null && hullRays.Length == probePositions.Length;
		if (!hasHullRays && count > 0)
		{
			Debug.LogWarning("[PcSceneLighting] " + gameObject.scene.name + ": no hull rays, points outside the probe hull use the nearest inner tetrahedron");
		}
		for (int t = 0; t < count; t++)
		{
			if (tetraIndices[t * 4 + 3] < 0)
			{
				_cellValid[t] = hasHullRays;
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
			_cellValid[t] = true;
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
		// 3.5 adds the scene ambient to the L0 term of the SH constants (SHAr/g/b.w), so it lights every normal equally
		Color ambient = RenderSettings.ambientLight * SceneAmbientScale;
		for (int i = _probeRenderers.Count - 1; i >= 0; i--)
		{
			Renderer target = _probeRenderers[i];
			if (target == null)
			{
				_probeRenderers.RemoveAt(i);
				_lastCell.Remove(target);
				_shScales.Remove(target);
				continue;
			}
			// No isVisible check: a renderer coming into view must not be drawn once with stale SH
			Vector3 position = target.bounds.center;
			if (target.probeAnchor != null)
			{
				position = target.probeAnchor.position;
			}
			int startCell = 0;
			_lastCell.TryGetValue(target, out startCell);
			int cell = Interpolate(position, startCell, _probe);
			_lastCell[target] = cell;
			ComputeShaderConstants(_probe, _constants);
			float shScale;
			if (!_shScales.TryGetValue(target, out shScale))
			{
				shScale = 1f;
			}
			target.GetPropertyBlock(_block);
			for (int k = 0; k < ShaderNames.Length; k++)
			{
				// SHC.w is unused by the shaders, scaling it is harmless
				_single[0] = _constants[k] * shScale;
				if (k < 3)
				{
					// k = 0, 1, 2: unity_SHAr, SHAg, SHAb (red, green, blue)
					_single[0].w += ambient[k];
				}
				_block.SetVectorArray(ShaderNames[k], _single);
			}
			target.SetPropertyBlock(_block);
		}
	}

	/// <summary>EmissiveCharacterShScale for renderers whose material uses EmissiveCharacterShader, otherwise 1.</summary>
	private static float GetShScale(Renderer target)
	{
		Material material = target.sharedMaterial;
		if (material != null && material.shader != null && material.shader.name == EmissiveCharacterShader)
		{
			Debug.Log("[PcSceneLighting] SH x" + EmissiveCharacterShScale + " for '" + target.name + "' (" + material.name + ")");
			return EmissiveCharacterShScale;
		}
		return 1f;
	}

	/// <summary>
	/// Finds renderers still on Unity's own probes (BlendProbes) and switches them to SH provided here.
	/// Also takes over untracked CustomProvided renderers: Instantiate of a live tracked renderer (Dismemberment clones the
	/// zombie's head bone) copies lightProbeUsage but not the MaterialPropertyBlock, so the clone would render with SH = 0 (black).
	/// </summary>
	private void ScanProbeRenderers()
	{
		Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
		int added = 0;
		foreach (Renderer target in renderers)
		{
			bool isBlendProbes = target.lightProbeUsage == LightProbeUsage.BlendProbes;
			bool isUntrackedCustom = target.lightProbeUsage == LightProbeUsage.CustomProvided && !_shScales.ContainsKey(target);
			if (!isBlendProbes && !isUntrackedCustom)
			{
				continue;
			}
			target.lightProbeUsage = LightProbeUsage.CustomProvided;
			_probeRenderers.Add(target);
			_shScales[target] = GetShScale(target);
			added++;
		}
		if (added > 0)
		{
			Debug.Log("[PcSceneLighting] probe renderers +" + added + " (total " + _probeRenderers.Count + ")");
		}
	}

	/// <summary>
	/// Interpolates the 27 SH coefficients at position into result and returns the cell used.
	/// Walks from startCell along cell neighbors; if the walk fails, tests every cell and takes the one whose smallest
	/// weight is largest. Weights are then clamped to non-negative and normalized.
	/// </summary>
	private int Interpolate(Vector3 position, int startCell, float[] result)
	{
		Vector4 weights = Vector4.zero;
		int cell = startCell;
		if (cell < 0 || cell >= _cellValid.Length || !_cellValid[cell])
		{
			cell = FirstValidCell();
		}
		bool found = false;
		for (int step = 0; step < MaxWalkSteps; step++)
		{
			int next;
			if (EvaluateCell(cell, position, out weights, out next))
			{
				found = true;
				break;
			}
			if (next < 0 || next >= _cellValid.Length || !_cellValid[next])
			{
				break;
			}
			cell = next;
		}
		if (!found)
		{
			float best = float.NegativeInfinity;
			for (int t = 0; t < _cellValid.Length; t++)
			{
				if (!_cellValid[t])
				{
					continue;
				}
				Vector4 candidate;
				int unusedNext;
				EvaluateCell(t, position, out candidate, out unusedNext);
				float score = SmallestWeight(t, candidate);
				if (score > best)
				{
					best = score;
					cell = t;
					weights = candidate;
				}
			}
			if (_walkFailLogs < MaxWalkFailLogs)
			{
				_walkFailLogs++;
				Debug.Log("[PcSceneLighting] walk failed at " + position + ", nearest cell " + cell + " (smallest weight " + best + ")");
			}
		}
		AccumulateProbes(cell, weights, result);
		return cell;
	}

	/// <summary>
	/// Barycentric weights of position in a cell. Returns true when the position is inside the cell; otherwise next is
	/// the neighbor to walk to. Outer cells give 3 weights (w = 0); an outer cell that cannot project the point
	/// (point on the inner side of the hull) gives weights of -infinity and points next to its inner tetrahedron.
	/// </summary>
	private bool EvaluateCell(int cell, Vector3 position, out Vector4 weights, out int next)
	{
		bool isOuter = tetraIndices[cell * 4 + 3] < 0;
		if (isOuter)
		{
			if (!OuterCellWeights(cell, position, out weights))
			{
				weights = new Vector4(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, 0f);
				next = tetraNeighbors[cell * 4 + 3];
				return false;
			}
		}
		else
		{
			weights = Barycentric(cell, position);
		}
		int worst = MinIndex(weights, isOuter);
		if (weights[worst] >= InsideTolerance)
		{
			next = cell;
			return true;
		}
		next = tetraNeighbors[cell * 4 + worst];
		return false;
	}

	/// <summary>Smallest weight that counts for the cell (3 for outer cells, 4 for inner ones).</summary>
	private float SmallestWeight(int cell, Vector4 weights)
	{
		bool isOuter = tetraIndices[cell * 4 + 3] < 0;
		return weights[MinIndex(weights, isOuter)];
	}

	/// <summary>Clamps the weights to non-negative, normalizes them and sums the weighted probe coefficients into result.</summary>
	private void AccumulateProbes(int cell, Vector4 weights, float[] result)
	{
		float sum = 0f;
		for (int k = 0; k < 4; k++)
		{
			if (tetraIndices[cell * 4 + k] < 0)
			{
				weights[k] = 0f;
			}
			weights[k] = Mathf.Max(0f, weights[k]);
			sum += weights[k];
		}
		for (int i = 0; i < CoefficientsPerProbe; i++)
		{
			result[i] = 0f;
		}
		if (sum <= 0f)
		{
			return;
		}
		for (int k = 0; k < 4; k++)
		{
			int probe = tetraIndices[cell * 4 + k];
			if (probe < 0)
			{
				continue;
			}
			float weight = weights[k] / sum;
			for (int i = 0; i < CoefficientsPerProbe; i++)
			{
				result[i] += probeCoefficients[probe * CoefficientsPerProbe + i] * weight;
			}
		}
	}

	private int FirstValidCell()
	{
		for (int t = 0; t < _cellValid.Length; t++)
		{
			if (_cellValid[t])
			{
				return t;
			}
		}
		return 0;
	}

	/// <summary>Barycentric weights of position in an inner tetrahedron.</summary>
	private Vector4 Barycentric(int tetra, Vector3 position)
	{
		Vector3 d = probePositions[tetraIndices[tetra * 4 + 3]];
		Vector3 local = _tetraInverse[tetra].MultiplyVector(position - d);
		return new Vector4(local.x, local.y, local.z, 1f - local.x - local.y - local.z);
	}

	/// <summary>
	/// Outer cell: corners V0..V2 of a hull triangle extruded along their hull rays N0..N2. Finds the smallest t >= 0 with
	/// det(V0 + t N0 - P, V1 + t N1 - P, V2 + t N2 - P) = 0 (P in the plane of the extruded triangle) and returns P's
	/// barycentric weights in that triangle. False when there is no such t (P lies on the inner side of the hull face).
	/// </summary>
	private bool OuterCellWeights(int cell, Vector3 position, out Vector4 weights)
	{
		weights = Vector4.zero;
		int i0 = tetraIndices[cell * 4];
		int i1 = tetraIndices[cell * 4 + 1];
		int i2 = tetraIndices[cell * 4 + 2];
		Vector3 a0 = probePositions[i0] - position;
		Vector3 a1 = probePositions[i1] - position;
		Vector3 a2 = probePositions[i2] - position;
		Vector3 n0 = hullRays[i0];
		Vector3 n1 = hullRays[i1];
		Vector3 n2 = hullRays[i2];
		// Expand det(a0 + t n0, a1 + t n1, a2 + t n2) = c3 t^3 + c2 t^2 + c1 t + c0
		double c3 = Determinant(n0, n1, n2);
		double c2 = Determinant(a0, n1, n2) + Determinant(n0, a1, n2) + Determinant(n0, n1, a2);
		double c1 = Determinant(n0, a1, a2) + Determinant(a0, n1, a2) + Determinant(a0, a1, n2);
		double c0 = Determinant(a0, a1, a2);
		double t = SmallestNonNegativeRoot(c3, c2, c1, c0);
		if (t < 0.0)
		{
			return false;
		}
		float extrusion = (float)t;
		Vector3 corner0 = a0 + n0 * extrusion;
		Vector3 corner1 = a1 + n1 * extrusion;
		Vector3 corner2 = a2 + n2 * extrusion;
		// Corners are relative to the point, so the point is the origin here
		Vector3 normal = Vector3.Cross(corner1 - corner0, corner2 - corner0);
		float normalSquared = Vector3.Dot(normal, normal);
		if (normalSquared < 1e-12f)
		{
			return false;
		}
		float w0 = Vector3.Dot(Vector3.Cross(corner1, corner2), normal) / normalSquared;
		float w1 = Vector3.Dot(Vector3.Cross(corner2, corner0), normal) / normalSquared;
		weights = new Vector4(w0, w1, 1f - w0 - w1, 0f);
		return true;
	}

	/// <summary>det of the 3x3 matrix with rows a, b, c.</summary>
	private static double Determinant(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Dot(a, Vector3.Cross(b, c));
	}

	/// <summary>Smallest real root t >= 0 of a t^3 + b t^2 + c t + d = 0, or -1 when there is none.</summary>
	private static double SmallestNonNegativeRoot(double a, double b, double c, double d)
	{
		double[] roots = new double[3];
		int count = SolveCubic(a, b, c, d, roots);
		double best = -1.0;
		for (int i = 0; i < count; i++)
		{
			double root = PolishRoot(a, b, c, d, roots[i]);
			// A root a hair below zero means the point is on the hull face itself
			if (root < 0.0 && root > -1e-6)
			{
				root = 0.0;
			}
			if (root < 0.0)
			{
				continue;
			}
			if (best < 0.0 || root < best)
			{
				best = root;
			}
		}
		return best;
	}

	/// <summary>Real roots of a t^3 + b t^2 + c t + d = 0 into roots; returns how many. Falls back to lower degrees when a (or b) vanishes.</summary>
	private static int SolveCubic(double a, double b, double c, double d, double[] roots)
	{
		double scale = System.Math.Max(System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)), System.Math.Max(System.Math.Abs(c), System.Math.Abs(d)));
		if (scale == 0.0)
		{
			return 0;
		}
		if (System.Math.Abs(a) <= PolynomialEpsilon * scale)
		{
			return SolveQuadratic(b, c, d, roots);
		}
		// Normalize, then substitute t = x - b/3 to get x^3 + p x + q = 0
		double nb = b / a;
		double nc = c / a;
		double nd = d / a;
		double shift = nb / 3.0;
		double p = nc - nb * nb / 3.0;
		double q = 2.0 * nb * nb * nb / 27.0 - nb * nc / 3.0 + nd;
		double discriminant = q * q / 4.0 + p * p * p / 27.0;
		if (discriminant > 0.0)
		{
			double root = System.Math.Sqrt(discriminant);
			double u = CubeRoot(-q / 2.0 + root);
			double v = CubeRoot(-q / 2.0 - root);
			roots[0] = u + v - shift;
			return 1;
		}
		if (p == 0.0)
		{
			roots[0] = -shift;
			return 1;
		}
		// Three real roots (trigonometric form); discriminant == 0 gives a repeated root here as well
		double radius = System.Math.Sqrt(-p / 3.0);
		double cosine = (3.0 * q / (2.0 * p)) * System.Math.Sqrt(-3.0 / p);
		if (cosine > 1.0)
		{
			cosine = 1.0;
		}
		if (cosine < -1.0)
		{
			cosine = -1.0;
		}
		double phi = System.Math.Acos(cosine) / 3.0;
		for (int k = 0; k < 3; k++)
		{
			roots[k] = 2.0 * radius * System.Math.Cos(phi - 2.0 * System.Math.PI * k / 3.0) - shift;
		}
		return 3;
	}

	/// <summary>Real roots of a t^2 + b t + c = 0 (linear when a vanishes).</summary>
	private static int SolveQuadratic(double a, double b, double c, double[] roots)
	{
		double scale = System.Math.Max(System.Math.Abs(a), System.Math.Max(System.Math.Abs(b), System.Math.Abs(c)));
		if (scale == 0.0)
		{
			return 0;
		}
		if (System.Math.Abs(a) <= PolynomialEpsilon * scale)
		{
			if (b == 0.0)
			{
				return 0;
			}
			roots[0] = -c / b;
			return 1;
		}
		double discriminant = b * b - 4.0 * a * c;
		if (discriminant < 0.0)
		{
			return 0;
		}
		double root = System.Math.Sqrt(discriminant);
		roots[0] = (-b - root) / (2.0 * a);
		roots[1] = (-b + root) / (2.0 * a);
		return 2;
	}

	private static double CubeRoot(double value)
	{
		if (value < 0.0)
		{
			return -System.Math.Pow(-value, 1.0 / 3.0);
		}
		return System.Math.Pow(value, 1.0 / 3.0);
	}

	/// <summary>Two Newton steps on the original cubic to clean up rounding of the closed-form root.</summary>
	private static double PolishRoot(double a, double b, double c, double d, double t)
	{
		for (int i = 0; i < 2; i++)
		{
			double value = ((a * t + b) * t + c) * t + d;
			double slope = (3.0 * a * t + 2.0 * b) * t + c;
			if (slope == 0.0)
			{
				break;
			}
			t -= value / slope;
		}
		return t;
	}

	/// <summary>Index of the smallest weight (only the first 3 for outer cells).</summary>
	private static int MinIndex(Vector4 values, bool firstThreeOnly)
	{
		int count = 4;
		if (firstThreeOnly)
		{
			count = 3;
		}
		int index = 0;
		for (int k = 1; k < count; k++)
		{
			if (values[k] < values[index])
			{
				index = k;
			}
		}
		return index;
	}

	/// <summary>27 raw SH coefficients -> unity_SHAr, SHAg, SHAb, SHBr, SHBg, SHBb, SHC (3.5 conversion, same as LightProbeSampler).</summary>
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

	/// <summary>Replaces 3.5's LightmapSettings.lightProbes.GetInterpolatedLightProbe: the 27 coefficients at position. All 0 without probe data.</summary>
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
		int startCell = 0;
		if (renderer != null)
		{
			Current._lastCell.TryGetValue(renderer, out startCell);
		}
		int cell = Current.Interpolate(position, startCell, coefficients);
		if (renderer != null)
		{
			Current._lastCell[renderer] = cell;
		}
	}
}
