// Converts legacy particles (Unity 3.5 EllipsoidParticleEmitter / ParticleAnimator / ParticleRenderer / WorldParticleCollider)
// into ParticleSystem + ParticleSystemRenderer + NewUnityLegacyParticleEmitter.
// Legacy parameters are extracted from the untouched export by legacy_particles_extract.py into
// tools/port_new_unity/asset_convert/data/particles/legacy_particles.json, keyed by GameObject fileID (stable even after Unity 6 re-saves the scene).
// Safe to re-run: objects that already have ParticleSystem / NewUnityLegacyParticleEmitter are reconfigured, not duplicated.
// Batch mode: Unity.exe -batchmode -quit -projectPath <project> -executeMethod NewUnityLegacyParticleConvert.ConvertAll -logFile <log>
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NewUnityLegacyParticleConvert
{
	private const string JsonPath = "tools/port_new_unity/asset_convert/data/particles/legacy_particles.json";

	/// <summary>Particle cap: legacy particles had none, so use a large enough value.</summary>
	private const int MaxParticles = 1000;

	/// <summary>The limit-velocity module is only used for damping; the speed limit itself never triggers.</summary>
	private const float NoSpeedLimit = 100000f;

	/// <summary>Legacy damping multiplies velocity by damping per second, i.e. drag = -ln(damping); clamp tiny values.</summary>
	private const float MinDamping = 0.0001f;

	/// <summary>Number of samples on the size growth curve.</summary>
	private const int SizeCurveKeys = 5;

	/// <summary>Smallest growth base (1 + sizeGrow) passed to pow.</summary>
	private const float MinGrowthBase = 0.0001f;

	// Serialized values of the legacy ParticleRenderMode
	private const int LegacyBillboard = 0;
	private const int LegacySortedBillboard = 2;
	private const int LegacyStretch = 3;
	private const int LegacyHorizontalBillboard = 4;
	private const int LegacyVerticalBillboard = 5;

	/// <summary>Legacy particle parameters of one object (matches the output of legacy_particles_extract.py).</summary>
	[System.Serializable]
	private class LegacyEntry
	{
		public string assetPath;
		public string gameObjectFileId;
		public string gameObjectName;
		public bool emitterEnabled;
		public bool emit;
		public float minSize;
		public float maxSize;
		public float minEnergy;
		public float maxEnergy;
		public float minEmission;
		public float maxEmission;
		public float[] worldVelocity;
		public float[] localVelocity;
		public float[] rndVelocity;
		public float emitterVelocityScale;
		public float[] tangentVelocity;
		public float angularVelocity;
		public float rndAngularVelocity;
		public bool rndRotation;
		public bool useWorldSpace;
		public bool oneShot;
		public float[] ellipsoid;
		public float minEmitterRange;
		public bool doesAnimateColor;
		public float[] colorAnimation;
		public float sizeGrow;
		public float[] rndForce;
		public float[] force;
		public float damping;
		public bool autodestruct;
		public bool rendererEnabled;
		public string[] materialGuids;
		public int renderMode;
		public float lengthScale;
		public float velocityScale;
		public float cameraVelocityScale;
		public float maxParticleSize;
		public int uvTilesX;
		public int uvTilesY;
		public float uvCycles;
		public bool hasCollider;
		public float bounceFactor;
		public float collisionEnergyLoss;
		public int collidesWith;
		public float minKillVelocity;
	}

	[System.Serializable]
	private class LegacyFile
	{
		public LegacyEntry[] entries;
	}

	public static void ConvertAll()
	{
		LegacyFile file = JsonUtility.FromJson<LegacyFile>(File.ReadAllText(JsonPath));
		// Group by asset, keeping the JSON order
		Dictionary<string, List<LegacyEntry>> byAsset = new Dictionary<string, List<LegacyEntry>>();
		List<string> assetOrder = new List<string>();
		foreach (LegacyEntry entry in file.entries)
		{
			if (!byAsset.ContainsKey(entry.assetPath))
			{
				byAsset[entry.assetPath] = new List<LegacyEntry>();
				assetOrder.Add(entry.assetPath);
			}
			byAsset[entry.assetPath].Add(entry);
		}

		int converted = 0;
		int missing = 0;
		foreach (string assetPath in assetOrder)
		{
			if (!File.Exists(assetPath))
			{
				Debug.LogWarning("[NewUnityLegacyParticle] asset not found: " + assetPath);
				missing += byAsset[assetPath].Count;
				continue;
			}
			int[] result;
			if (assetPath.EndsWith(".prefab"))
			{
				result = ConvertPrefab(assetPath, byAsset[assetPath]);
			}
			else
			{
				result = ConvertScene(assetPath, byAsset[assetPath]);
			}
			converted += result[0];
			missing += result[1];
			Debug.Log("[NewUnityLegacyParticle] " + assetPath + ": converted " + result[0] + ", missing " + result[1]);
		}
		AssetDatabase.SaveAssets();
		Debug.Log("[NewUnityLegacyParticle] done: converted " + converted + ", missing " + missing);
	}

	/// <summary>Converts one scene; returns [converted, missing].</summary>
	private static int[] ConvertScene(string scenePath, List<LegacyEntry> entries)
	{
		UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		Dictionary<ulong, GameObject> byFileId = new Dictionary<ulong, GameObject>();
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
			{
				GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(child.gameObject);
				byFileId[id.targetObjectId] = child.gameObject;
			}
		}
		int converted = 0;
		int missing = 0;
		foreach (LegacyEntry entry in entries)
		{
			GameObject target;
			if (!byFileId.TryGetValue(ulong.Parse(entry.gameObjectFileId), out target))
			{
				Debug.LogWarning("[NewUnityLegacyParticle] not found in scene: " + scenePath + " " + entry.gameObjectFileId + " " + entry.gameObjectName);
				missing++;
				continue;
			}
			Apply(target, entry);
			converted++;
		}
		EditorSceneManager.MarkSceneDirty(scene);
		EditorSceneManager.SaveScene(scene);
		return new int[] { converted, missing };
	}

	/// <summary>
	/// Converts one prefab; returns [converted, missing].
	/// fileIDs are only known on the prefab asset, but edits must go to the LoadPrefabContents copy,
	/// so objects are matched by their sibling-index path from the root.
	/// </summary>
	private static int[] ConvertPrefab(string prefabPath, List<LegacyEntry> entries)
	{
		Dictionary<long, GameObject> assetByFileId = new Dictionary<long, GameObject>();
		foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(prefabPath))
		{
			GameObject gameObject = asset as GameObject;
			if (gameObject == null)
			{
				continue;
			}
			string guid;
			long localId;
			if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(gameObject, out guid, out localId))
			{
				assetByFileId[localId] = gameObject;
			}
		}
		GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
		int converted = 0;
		int missing = 0;
		foreach (LegacyEntry entry in entries)
		{
			GameObject assetObject;
			if (!assetByFileId.TryGetValue(long.Parse(entry.gameObjectFileId), out assetObject))
			{
				Debug.LogWarning("[NewUnityLegacyParticle] not found in prefab: " + prefabPath + " " + entry.gameObjectFileId + " " + entry.gameObjectName);
				missing++;
				continue;
			}
			Transform target = FollowPath(contents.transform, SiblingPath(assetObject.transform));
			if (target.name != assetObject.name)
			{
				Debug.LogWarning("[NewUnityLegacyParticle] path mismatch in prefab: " + prefabPath + " " + assetObject.name + " -> " + target.name);
				missing++;
				continue;
			}
			Apply(target.gameObject, entry);
			converted++;
		}
		PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
		PrefabUtility.UnloadPrefabContents(contents);
		return new int[] { converted, missing };
	}

	/// <summary>Sibling indices from the root down to this object.</summary>
	private static List<int> SiblingPath(Transform transform)
	{
		List<int> path = new List<int>();
		Transform current = transform;
		while (current.parent != null)
		{
			path.Insert(0, current.GetSiblingIndex());
			current = current.parent;
		}
		return path;
	}

	private static Transform FollowPath(Transform root, List<int> path)
	{
		Transform current = root;
		foreach (int index in path)
		{
			current = current.GetChild(index);
		}
		return current;
	}

	private static Vector3 ToVector(float[] values)
	{
		return new Vector3(values[0], values[1], values[2]);
	}

	/// <summary>Adds (or reconfigures) ParticleSystem, its renderer and NewUnityLegacyParticleEmitter on the object.</summary>
	private static void Apply(GameObject target, LegacyEntry entry)
	{
		ParticleSystem system = target.GetComponent<ParticleSystem>();
		if (system == null)
		{
			system = target.AddComponent<ParticleSystem>();
		}
		system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		ParticleSystem.MainModule main = system.main;
		main.duration = 1f;
		main.loop = true;
		main.playOnAwake = true;
		main.startSpeed = 0f;
		main.startLifetime = entry.maxEnergy;
		main.startSize = entry.maxSize;
		main.maxParticles = MaxParticles;
		if (entry.useWorldSpace)
		{
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.scalingMode = ParticleSystemScalingMode.Shape;
		}
		else
		{
			main.simulationSpace = ParticleSystemSimulationSpace.Local;
			main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		}

		// Emission is driven by NewUnityLegacyParticleEmitter
		ParticleSystem.EmissionModule emission = system.emission;
		emission.enabled = false;
		ParticleSystem.ShapeModule shape = system.shape;
		shape.enabled = false;

		ApplyColorAnimation(system, entry);
		ApplySizeGrow(system, entry);
		ApplyForce(system, entry);
		ApplyDamping(system, entry);
		ApplyUvAnimation(system, entry);
		ApplyCollision(system, entry);
		ApplyRenderer(target, entry);

		NewUnityLegacyParticleEmitter emitter = target.GetComponent<NewUnityLegacyParticleEmitter>();
		if (emitter == null)
		{
			emitter = target.AddComponent<NewUnityLegacyParticleEmitter>();
		}
		emitter.enabled = entry.emitterEnabled;
		emitter.emit = entry.emit;
		emitter.minSize = entry.minSize;
		emitter.maxSize = entry.maxSize;
		emitter.minEnergy = entry.minEnergy;
		emitter.maxEnergy = entry.maxEnergy;
		emitter.minEmission = entry.minEmission;
		emitter.maxEmission = entry.maxEmission;
		emitter.worldVelocity = ToVector(entry.worldVelocity);
		emitter.localVelocity = ToVector(entry.localVelocity);
		emitter.rndVelocity = ToVector(entry.rndVelocity);
		emitter.emitterVelocityScale = entry.emitterVelocityScale;
		emitter.angularVelocity = entry.angularVelocity;
		emitter.rndAngularVelocity = entry.rndAngularVelocity;
		emitter.rndRotation = entry.rndRotation;
		emitter.useWorldSpace = entry.useWorldSpace;
		emitter.oneShot = entry.oneShot;
		emitter.ellipsoid = ToVector(entry.ellipsoid);
		emitter.minEmitterRange = entry.minEmitterRange;
		emitter.doesAnimateColor = entry.doesAnimateColor;
		emitter.autodestruct = entry.autodestruct;
		EditorUtility.SetDirty(target);
	}

	/// <summary>Legacy color animation: 5 colors spread evenly over the lifetime.</summary>
	private static void ApplyColorAnimation(ParticleSystem system, LegacyEntry entry)
	{
		ParticleSystem.ColorOverLifetimeModule colorModule = system.colorOverLifetime;
		colorModule.enabled = entry.doesAnimateColor;
		if (!entry.doesAnimateColor)
		{
			return;
		}
		GradientColorKey[] colorKeys = new GradientColorKey[5];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];
		for (int i = 0; i < 5; i++)
		{
			float time = i / 4f;
			int offset = i * 4;
			Color color = new Color(entry.colorAnimation[offset], entry.colorAnimation[offset + 1], entry.colorAnimation[offset + 2]);
			colorKeys[i] = new GradientColorKey(color, time);
			alphaKeys[i] = new GradientAlphaKey(entry.colorAnimation[offset + 3], time);
		}
		Gradient gradient = new Gradient();
		gradient.SetKeys(colorKeys, alphaKeys);
		colorModule.color = new ParticleSystem.MinMaxGradient(gradient);
	}

	/// <summary>
	/// Legacy sizeGrow: ParticleAnimator::UpdateParticles multiplies size by powf(1 + sizeGrow, deltaTime) every frame
	/// (verified in the original binary), so size(t) = size0 * (1 + sizeGrow)^t.
	/// The size curve uses normalized lifetime, so convert with the mean energy; particles with other lifetimes drift slightly.
	/// </summary>
	private static void ApplySizeGrow(ParticleSystem system, LegacyEntry entry)
	{
		ParticleSystem.SizeOverLifetimeModule sizeModule = system.sizeOverLifetime;
		sizeModule.enabled = entry.sizeGrow != 0f;
		if (entry.sizeGrow == 0f)
		{
			return;
		}
		float meanEnergy = (entry.minEnergy + entry.maxEnergy) * 0.5f;
		// Keep the pow base positive (sizeGrow <= -1)
		float growthBase = Mathf.Max(1f + entry.sizeGrow, MinGrowthBase);
		Keyframe[] keys = new Keyframe[SizeCurveKeys];
		float maxValue = 0f;
		for (int i = 0; i < SizeCurveKeys; i++)
		{
			float time = i / (float)(SizeCurveKeys - 1);
			float value = Mathf.Pow(growthBase, time * meanEnergy);
			keys[i] = new Keyframe(time, value);
			maxValue = Mathf.Max(maxValue, value);
		}
		// Normalize the curve to 0-1 and move the scale into the multiplier
		for (int i = 0; i < SizeCurveKeys; i++)
		{
			keys[i].value = keys[i].value / maxValue;
		}
		AnimationCurve curve = new AnimationCurve(keys);
		for (int i = 0; i < SizeCurveKeys; i++)
		{
			curve.SmoothTangents(i, 0f);
		}
		sizeModule.size = new ParticleSystem.MinMaxCurve(maxValue, curve);
	}

	/// <summary>Legacy force (constant) and rndForce (random per frame, -rnd to +rnd).</summary>
	private static void ApplyForce(ParticleSystem system, LegacyEntry entry)
	{
		Vector3 force = ToVector(entry.force);
		Vector3 rndForce = ToVector(entry.rndForce);
		ParticleSystem.ForceOverLifetimeModule forceModule = system.forceOverLifetime;
		bool hasForce = force != Vector3.zero || rndForce != Vector3.zero;
		forceModule.enabled = hasForce;
		if (!hasForce)
		{
			return;
		}
		forceModule.space = ParticleSystemSimulationSpace.World;
		bool randomized = rndForce != Vector3.zero;
		forceModule.randomized = randomized;
		if (randomized)
		{
			forceModule.x = new ParticleSystem.MinMaxCurve(force.x - rndForce.x, force.x + rndForce.x);
			forceModule.y = new ParticleSystem.MinMaxCurve(force.y - rndForce.y, force.y + rndForce.y);
			forceModule.z = new ParticleSystem.MinMaxCurve(force.z - rndForce.z, force.z + rndForce.z);
		}
		else
		{
			forceModule.x = new ParticleSystem.MinMaxCurve(force.x);
			forceModule.y = new ParticleSystem.MinMaxCurve(force.y);
			forceModule.z = new ParticleSystem.MinMaxCurve(force.z);
		}
	}

	/// <summary>Legacy damping (1 = no slowdown) mapped to linear drag of the limit-velocity module.</summary>
	private static void ApplyDamping(ParticleSystem system, LegacyEntry entry)
	{
		ParticleSystem.LimitVelocityOverLifetimeModule limit = system.limitVelocityOverLifetime;
		bool damped = entry.damping < 1f;
		limit.enabled = damped;
		if (!damped)
		{
			return;
		}
		float damping = Mathf.Max(entry.damping, MinDamping);
		limit.limit = new ParticleSystem.MinMaxCurve(NoSpeedLimit);
		limit.dampen = 0f;
		limit.drag = new ParticleSystem.MinMaxCurve(-Mathf.Log(damping));
		limit.multiplyDragByParticleSize = false;
		limit.multiplyDragByParticleVelocity = false;
	}

	/// <summary>Legacy UV animation: x/y tile counts and cycles per lifetime; 0 cycles keeps the first frame.</summary>
	private static void ApplyUvAnimation(ParticleSystem system, LegacyEntry entry)
	{
		ParticleSystem.TextureSheetAnimationModule sheet = system.textureSheetAnimation;
		bool animated = entry.uvTilesX * entry.uvTilesY > 1;
		sheet.enabled = animated;
		if (!animated)
		{
			return;
		}
		sheet.numTilesX = entry.uvTilesX;
		sheet.numTilesY = entry.uvTilesY;
		sheet.animation = ParticleSystemAnimationType.WholeSheet;
		int cycles = Mathf.RoundToInt(entry.uvCycles);
		if (cycles <= 0)
		{
			sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
			sheet.cycleCount = 1;
		}
		else
		{
			sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
			sheet.cycleCount = cycles;
		}
	}

	/// <summary>Legacy WorldParticleCollider: collide and bounce off scene colliders. Energy loss has no equivalent and is ignored.</summary>
	private static void ApplyCollision(ParticleSystem system, LegacyEntry entry)
	{
		ParticleSystem.CollisionModule collision = system.collision;
		collision.enabled = entry.hasCollider;
		if (!entry.hasCollider)
		{
			return;
		}
		collision.type = ParticleSystemCollisionType.World;
		collision.mode = ParticleSystemCollisionMode.Collision3D;
		collision.bounce = new ParticleSystem.MinMaxCurve(entry.bounceFactor);
		collision.dampen = new ParticleSystem.MinMaxCurve(0f);
		collision.lifetimeLoss = new ParticleSystem.MinMaxCurve(0f);
		collision.minKillSpeed = entry.minKillVelocity;
		collision.collidesWith = entry.collidesWith;
		collision.sendCollisionMessages = false;
	}

	private static void ApplyRenderer(GameObject target, LegacyEntry entry)
	{
		ParticleSystemRenderer renderer = target.GetComponent<ParticleSystemRenderer>();
		renderer.enabled = entry.rendererEnabled;
		List<Material> materials = new List<Material>();
		foreach (string guid in entry.materialGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
			if (material == null)
			{
				Debug.LogWarning("[NewUnityLegacyParticle] material not found: " + guid + " on " + entry.gameObjectName);
			}
			materials.Add(material);
		}
		if (materials.Count > 0)
		{
			renderer.sharedMaterial = materials[0];
		}
		renderer.sortMode = ParticleSystemSortMode.None;
		if (entry.renderMode == LegacyStretch)
		{
			renderer.renderMode = ParticleSystemRenderMode.Stretch;
		}
		else if (entry.renderMode == LegacyHorizontalBillboard)
		{
			renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
		}
		else if (entry.renderMode == LegacyVerticalBillboard)
		{
			renderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
		}
		else if (entry.renderMode == LegacySortedBillboard)
		{
			renderer.renderMode = ParticleSystemRenderMode.Billboard;
			renderer.sortMode = ParticleSystemSortMode.Distance;
		}
		else
		{
			if (entry.renderMode != LegacyBillboard)
			{
				Debug.LogWarning("[NewUnityLegacyParticle] unknown render mode " + entry.renderMode + " on " + entry.gameObjectName);
			}
			renderer.renderMode = ParticleSystemRenderMode.Billboard;
		}
		renderer.lengthScale = entry.lengthScale;
		renderer.velocityScale = entry.velocityScale;
		renderer.cameraVelocityScale = entry.cameraVelocityScale;
		renderer.maxParticleSize = entry.maxParticleSize;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
	}
}
