// PC port: graphics upgrade for The Pit (unused level, see PcPitTrial). The original level was never finished: its
// "toxic water" is a 128x128 Thrower smoke sprite stretched over 30 m and the glass around the psycho's platform shows the
// Thrower explosion texture. This is the same in every version that shipped the level (1.1.0 to 2.0.2).
// When the level loads, this replaces them with the game's own clone-lab assets and adds particle effects:
//   1. pool: the Water sprite is hidden and a new surface with the clone-tube fluid shader (scrolling, wobbling, additive)
//      takes its place, tiled so the texture keeps its detail (the shader ignores material tiling, so the mesh UVs do it);
//   2. glass: the platform glass gets the clone-tube glass material;
//   3. mist: slow green smoke rising from the pool;
//   4. bubbles: small bright blobs popping on the surface;
//   5. walls: the wall shader's fake highlight is computed per vertex, and on the Pit's sparse walls the shiny trim strips
//      flickered as the camera moved. This level's copies of those materials use a per-pixel version of the same formula
//      (PcPitPixelGlossy.shader). Other levels keep the original shader.
// The scene file is not changed. Materials and shader: Assets/Resources/PcPort/PcPit*.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PcPitEnhance
{
	// ---- Scenes and original objects ----
	private const string TrimmedSceneName = "ThePit_trimmed";
	private const string FullSceneName = "ThePit";
	private const string WaterObjectName = "Water";
	private const string WaterMaterialName = "FX_Throver_Smoke";
	private const string GlassMaterialName = "Fx_Throver_Explosion";

	// ---- Materials (Resources paths) ----
	private const string PoolMaterialPath = "PcPort/PcPitPool";
	private const string GlassMaterialPath = "PcPort/PcPitGlass";
	private const string MistMaterialPath = "PcPort/PcPitMist";
	private const string BubbleMaterialPath = "PcPort/PcPitBubbles";

	// ---- Wall highlight ----
	private const string VertexGlossyShaderName = "CKZ2/Lightmap + Unlit + VirtualVertexGlossy-Add";
	private const string PixelGlossyShaderPath = "PcPort/PcPitPixelGlossy";

	// ---- Pool surface ----
	/// <summary>Texture repeats across the 30 m pool (6 looked like a speckled carpet in game).</summary>
	private const float PoolUvTiling = 4f;
	/// <summary>Lift above the old sprite, so the two never fight while one is hidden.</summary>
	private const float PoolSurfaceLift = 0.01f;

	// ---- Particle area: the open floor inside the ring of pipes, as a fraction of the 30 m sprite ----
	private const float EffectAreaFraction = 0.6f;

	// ---- Mist: a thin layer hugging the pool (alpha 0.35 and faster rise hid the lockers in game) ----
	private const float MistRate = 10f;
	private const float MistLifetimeMin = 5f;
	private const float MistLifetimeMax = 8f;
	private const float MistSizeMin = 2.5f;
	private const float MistSizeMax = 5f;
	private const float MistRiseMin = 0.1f;
	private const float MistRiseMax = 0.25f;
	private const float MistMaxAlpha = 0.12f;
	private const int MistMaxParticles = 160;
	private static readonly Color MistColor = new Color(0.45f, 1f, 0.3f, 1f);

	// ---- Bubbles ----
	private const float BubbleRate = 25f;
	private const float BubbleLifetimeMin = 0.6f;
	private const float BubbleLifetimeMax = 1.2f;
	private const float BubbleSizeMin = 0.2f;
	private const float BubbleSizeMax = 0.6f;
	private const float BubbleRiseMin = 0.2f;
	private const float BubbleRiseMax = 0.6f;
	private const int BubbleMaxParticles = 60;
	private static readonly Color BubbleColor = new Color(0.6f, 1f, 0.35f, 0.8f);

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
	{
		if (scene.name != TrimmedSceneName && scene.name != FullSceneName)
		{
			return;
		}
		Debug.Log("[PcPitEnhance] upgrading " + scene.name);
		Renderer water = FindWater(scene);
		if (water == null)
		{
			Debug.LogWarning("[PcPitEnhance] water sprite not found, pool and effects skipped");
		}
		else
		{
			Bounds pool = water.bounds;
			CreatePoolSurface(water, pool);
			Vector3 center = pool.center;
			float areaSize = pool.size.x * EffectAreaFraction;
			CreateMist(water.gameObject.layer, center, areaSize);
			CreateBubbles(water.gameObject.layer, center, areaSize);
		}
		ReplaceGlass(scene);
		UsePixelGlossy(scene);
	}

	/// <summary>
	/// Gives this level's walls the per-pixel highlight: each material with the vertex-glossy shader is copied once
	/// (so other levels sharing it are untouched) and the copy gets the per-pixel shader; properties stay the same.
	/// </summary>
	private static void UsePixelGlossy(UnityEngine.SceneManagement.Scene scene)
	{
		Shader pixelGlossy = Resources.Load<Shader>(PixelGlossyShaderPath);
		if (pixelGlossy == null)
		{
			throw new System.Exception("[PcPitEnhance] shader missing: Resources/" + PixelGlossyShaderPath);
		}
		Dictionary<Material, Material> copies = new Dictionary<Material, Material>();
		int renderers = 0;
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
			{
				Material[] materials = renderer.sharedMaterials;
				bool changed = false;
				for (int i = 0; i < materials.Length; i++)
				{
					Material original = materials[i];
					if (original == null || original.shader.name != VertexGlossyShaderName)
					{
						continue;
					}
					if (!copies.ContainsKey(original))
					{
						Material copy = new Material(original);
						copy.name = original.name + " (PcPitPixelGlossy)";
						copy.shader = pixelGlossy;
						copies.Add(original, copy);
					}
					materials[i] = copies[original];
					changed = true;
				}
				if (changed)
				{
					renderer.sharedMaterials = materials;
					renderers++;
				}
			}
		}
		Debug.Log("[PcPitEnhance] per-pixel wall highlight: " + copies.Count + " materials, " + renderers + " renderers, shader supported " + pixelGlossy.isSupported);
	}

	/// <summary>The Water sprite that still uses the placeholder smoke material.</summary>
	private static Renderer FindWater(UnityEngine.SceneManagement.Scene scene)
	{
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
			{
				if (renderer.gameObject.name != WaterObjectName)
				{
					continue;
				}
				if (renderer.sharedMaterial != null && renderer.sharedMaterial.name == WaterMaterialName)
				{
					return renderer;
				}
			}
		}
		return null;
	}

	/// <summary>Hides the water sprite and puts a tiled fluid surface of the same size in its place.</summary>
	private static void CreatePoolSurface(Renderer water, Bounds pool)
	{
		Material material = LoadMaterial(PoolMaterialPath);
		float halfX = pool.extents.x;
		float halfZ = pool.extents.z;

		Mesh mesh = new Mesh();
		mesh.name = "PcPitPoolSurface";
		mesh.vertices = new Vector3[]
		{
			new Vector3(-halfX, 0f, -halfZ),
			new Vector3(-halfX, 0f, halfZ),
			new Vector3(halfX, 0f, halfZ),
			new Vector3(halfX, 0f, -halfZ)
		};
		mesh.uv = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, PoolUvTiling),
			new Vector2(PoolUvTiling, PoolUvTiling),
			new Vector2(PoolUvTiling, 0f)
		};
		// Clockwise seen from above, so the face points up
		mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		GameObject surface = new GameObject("PcPitPoolSurface");
		surface.layer = water.gameObject.layer;
		surface.transform.position = pool.center + Vector3.up * PoolSurfaceLift;
		surface.AddComponent<MeshFilter>().sharedMesh = mesh;
		MeshRenderer renderer = surface.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;

		water.enabled = false;
		Debug.Log("[PcPitEnhance] pool surface " + pool.size.x + " x " + pool.size.z + " m at " + surface.transform.position + ", tiling " + PoolUvTiling);
	}

	/// <summary>Slow green smoke rising from the pool.</summary>
	private static void CreateMist(int layer, Vector3 center, float areaSize)
	{
		ParticleSystem system = CreateParticleSystem("PcPitMist", layer, center, areaSize, LoadMaterial(MistMaterialPath));
		ParticleSystem.MainModule main = system.main;
		main.startLifetime = new ParticleSystem.MinMaxCurve(MistLifetimeMin, MistLifetimeMax);
		main.startSize = new ParticleSystem.MinMaxCurve(MistSizeMin, MistSizeMax);
		main.startColor = MistColor;
		main.maxParticles = MistMaxParticles;
		main.prewarm = true;

		ParticleSystem.EmissionModule emission = system.emission;
		emission.rateOverTime = MistRate;

		SetRise(system, MistRiseMin, MistRiseMax);
		SetFade(system, MistMaxAlpha);

		ParticleSystem.RotationOverLifetimeModule rotation = system.rotationOverLifetime;
		rotation.enabled = true;
		rotation.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

		system.Play();
		Debug.Log("[PcPitEnhance] mist over " + areaSize + " m");
	}

	/// <summary>Small bright blobs popping on the pool surface.</summary>
	private static void CreateBubbles(int layer, Vector3 center, float areaSize)
	{
		ParticleSystem system = CreateParticleSystem("PcPitBubbles", layer, center, areaSize, LoadMaterial(BubbleMaterialPath));
		ParticleSystem.MainModule main = system.main;
		main.startLifetime = new ParticleSystem.MinMaxCurve(BubbleLifetimeMin, BubbleLifetimeMax);
		main.startSize = new ParticleSystem.MinMaxCurve(BubbleSizeMin, BubbleSizeMax);
		main.startColor = BubbleColor;
		main.maxParticles = BubbleMaxParticles;

		ParticleSystem.EmissionModule emission = system.emission;
		emission.rateOverTime = BubbleRate;

		SetRise(system, BubbleRiseMin, BubbleRiseMax);
		SetFade(system, 1f);

		// Grow from nothing, like a bubble swelling before it pops
		ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
		size.enabled = true;
		size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f));

		system.Play();
		Debug.Log("[PcPitEnhance] bubbles over " + areaSize + " m");
	}

	/// <summary>A stopped, looping world-space particle system emitting from a flat box over the pool.</summary>
	private static ParticleSystem CreateParticleSystem(string name, int layer, Vector3 center, float areaSize, Material material)
	{
		GameObject holder = new GameObject(name);
		holder.layer = layer;
		holder.transform.position = center;
		ParticleSystem system = holder.AddComponent<ParticleSystem>();
		// A new system starts playing; most settings can only change while stopped
		system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		ParticleSystem.MainModule main = system.main;
		main.loop = true;
		main.duration = 10f;
		main.startSpeed = 0f;
		main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.scalingMode = ParticleSystemScalingMode.Shape;

		ParticleSystem.ShapeModule shape = system.shape;
		shape.enabled = true;
		shape.shapeType = ParticleSystemShapeType.Box;
		shape.scale = new Vector3(areaSize, 0.1f, areaSize);

		ParticleSystemRenderer renderer = holder.GetComponent<ParticleSystemRenderer>();
		renderer.renderMode = ParticleSystemRenderMode.Billboard;
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		return system;
	}

	/// <summary>Particles drift straight up at a random speed in [min, max] m/s.</summary>
	private static void SetRise(ParticleSystem system, float min, float max)
	{
		ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
		velocity.enabled = true;
		velocity.space = ParticleSystemSimulationSpace.World;
		velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
		velocity.y = new ParticleSystem.MinMaxCurve(min, max);
		velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
	}

	/// <summary>Alpha fades in, holds at maxAlpha, then fades out over each particle's life.</summary>
	private static void SetFade(ParticleSystem system, float maxAlpha)
	{
		Gradient gradient = new Gradient();
		gradient.SetKeys(
			new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
			new GradientAlphaKey[]
			{
				new GradientAlphaKey(0f, 0f),
				new GradientAlphaKey(maxAlpha, 0.3f),
				new GradientAlphaKey(maxAlpha, 0.6f),
				new GradientAlphaKey(0f, 1f)
			});
		ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
		color.enabled = true;
		color.color = gradient;
	}

	/// <summary>Swaps the explosion texture on the platform glass for the clone-tube glass.</summary>
	private static void ReplaceGlass(UnityEngine.SceneManagement.Scene scene)
	{
		Material glass = LoadMaterial(GlassMaterialPath);
		int replaced = 0;
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
			{
				Material[] materials = renderer.sharedMaterials;
				bool changed = false;
				for (int i = 0; i < materials.Length; i++)
				{
					if (materials[i] != null && materials[i].name == GlassMaterialName)
					{
						materials[i] = glass;
						changed = true;
						replaced++;
					}
				}
				if (changed)
				{
					renderer.sharedMaterials = materials;
				}
			}
		}
		Debug.Log("[PcPitEnhance] glass materials replaced: " + replaced);
	}

	private static Material LoadMaterial(string path)
	{
		Material material = Resources.Load<Material>(path);
		if (material == null)
		{
			throw new System.Exception("[PcPitEnhance] material missing: Resources/" + path);
		}
		return material;
	}
}
