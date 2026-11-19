// 把旧版粒子（Unity 3.5 的 EllipsoidParticleEmitter / ParticleAnimator / ParticleRenderer / WorldParticleCollider）
// 转成 ParticleSystem + ParticleSystemRenderer + PcPort/LegacyParticleEmitter。
// 旧参数由 tools/legacy_particles_extract.py 从未改动的导出里提取，放在 pc_data/particles/legacy_particles.json，
// 以 GameObject 的 fileID 为键（工程素材是导出的副本，场景被 Unity 6 重存过 fileID 也不变）。
// 可以重复运行：已经有 ParticleSystem / LegacyParticleEmitter 的物体会被重新设置，不会重复添加。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcLegacyParticleConvert.ConvertAll -logFile <日志>
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PcLegacyParticleConvert
{
	private const string JsonPath = "pc_data/particles/legacy_particles.json";

	/// <summary>粒子上限：旧版没有上限，这里给一个足够大的值。</summary>
	private const int MaxParticles = 1000;

	/// <summary>限速模块只用来做阻尼，速度上限给一个不会触发的值。</summary>
	private const float NoSpeedLimit = 100000f;

	/// <summary>旧版阻尼按每秒乘 damping 理解，换成线性阻力 drag = -ln(damping)；damping 过小时截断。</summary>
	private const float MinDamping = 0.0001f;

	/// <summary>尺寸增长曲线的采样点数。</summary>
	private const int SizeCurveKeys = 5;

	// 旧版 ParticleRenderMode 的序列化值
	private const int LegacyBillboard = 0;
	private const int LegacySortedBillboard = 2;
	private const int LegacyStretch = 3;
	private const int LegacyHorizontalBillboard = 4;
	private const int LegacyVerticalBillboard = 5;

	/// <summary>一个物体上的旧版粒子参数（与 legacy_particles_extract.py 的输出对应）。</summary>
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

	[MenuItem("PcPilot/Convert Legacy Particles")]
	public static void ConvertAll()
	{
		LegacyFile file = JsonUtility.FromJson<LegacyFile>(File.ReadAllText(JsonPath));
		// 按资源分组，保持 JSON 里的顺序
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
				Debug.LogWarning("[PcLegacyParticle] asset not found: " + assetPath);
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
			Debug.Log("[PcLegacyParticle] " + assetPath + ": converted " + result[0] + ", missing " + result[1]);
		}
		AssetDatabase.SaveAssets();
		Debug.Log("[PcLegacyParticle] done: converted " + converted + ", missing " + missing);
	}

	/// <summary>转换一个场景，返回 [转换数, 找不到的物体数]。</summary>
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
				Debug.LogWarning("[PcLegacyParticle] not found in scene: " + scenePath + " " + entry.gameObjectFileId + " " + entry.gameObjectName);
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
	/// 转换一个预制体，返回 [转换数, 找不到的物体数]。
	/// 预制体资源里的物体能查到 fileID，但改动要在 LoadPrefabContents 的副本上做，所以用“从根开始的子物体序号路径”对应过去。
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
				Debug.LogWarning("[PcLegacyParticle] not found in prefab: " + prefabPath + " " + entry.gameObjectFileId + " " + entry.gameObjectName);
				missing++;
				continue;
			}
			Transform target = FollowPath(contents.transform, SiblingPath(assetObject.transform));
			if (target.name != assetObject.name)
			{
				Debug.LogWarning("[PcLegacyParticle] path mismatch in prefab: " + prefabPath + " " + assetObject.name + " -> " + target.name);
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

	/// <summary>从根到该物体的子物体序号列表。</summary>
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

	/// <summary>给物体加上（或重新设置）ParticleSystem、渲染器和 LegacyParticleEmitter。</summary>
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

		// 发射交给 LegacyParticleEmitter
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

		LegacyParticleEmitter emitter = target.GetComponent<LegacyParticleEmitter>();
		if (emitter == null)
		{
			emitter = target.AddComponent<LegacyParticleEmitter>();
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

	/// <summary>旧版颜色动画：5 个颜色在寿命里均匀分布。</summary>
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
	/// 旧版 sizeGrow 按每秒指数增长理解：size(t) = size0 * exp(sizeGrow * t)。
	/// ParticleSystem 的尺寸曲线按归一化寿命取值，这里用平均寿命换算，寿命不同的粒子会有少许偏差。
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
		Keyframe[] keys = new Keyframe[SizeCurveKeys];
		float maxValue = 0f;
		for (int i = 0; i < SizeCurveKeys; i++)
		{
			float time = i / (float)(SizeCurveKeys - 1);
			float value = Mathf.Exp(entry.sizeGrow * time * meanEnergy);
			keys[i] = new Keyframe(time, value);
			maxValue = Mathf.Max(maxValue, value);
		}
		// 曲线值归一化到 0～1，倍数放进 multiplier
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

	/// <summary>旧版 force（恒定）和 rndForce（每帧随机，取 -rnd 到 +rnd）。</summary>
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

	/// <summary>旧版 damping（1 = 不减速）换成限速模块的线性阻力。</summary>
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

	/// <summary>旧版 UV 动画：x/y 方向的格数和寿命内的循环次数；循环 0 次表示停在第一帧。</summary>
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

	/// <summary>旧版 WorldParticleCollider：与场景碰撞体碰撞、反弹。能量损失（按秒扣寿命）没有对应项，忽略。</summary>
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
				Debug.LogWarning("[PcLegacyParticle] material not found: " + guid + " on " + entry.gameObjectName);
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
				Debug.LogWarning("[PcLegacyParticle] unknown render mode " + entry.renderMode + " on " + entry.gameObjectName);
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
