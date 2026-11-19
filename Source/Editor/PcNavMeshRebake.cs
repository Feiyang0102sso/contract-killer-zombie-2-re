// 用原版导航网格重新生成 Unity 6 的导航数据。
// 原版的寻路数据（Unity 3.5 的 Detour 二进制）由 tools/navmesh_extract.py 解析成 JSON，
// 放在工程根目录的 pc_data/navmesh/<旧 NavMesh 资源 GUID>.json。这里逐个打开场景：
//   1. 按 pc_data/navmesh/scene_map.tsv（场景 -> 旧 NavMesh 资源 GUID）找到对应 JSON；
//   2. 用射线把顶点贴到场景的真实地面（原版顶点高度是量化过的），再用这些多边形作为唯一输入构建 NavMeshData（代理半径取很小，因为原版多边形已经按半径收缩过）；
//   3. 存成 Assets/NewNavMesh/<场景名>.asset，挂到场景的 NavMeshSettings.m_NavMeshData 上，保存场景。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcNavMeshRebake.RebakeAll -logFile <日志>
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class PcNavMeshRebake
{
	private const string JsonDir = "pc_data/navmesh";

	private const string OutputDir = "Assets/NewNavMesh";

	private const string HolderName = "PcNavMesh";

	/// <summary>重新生成时的代理半径：原版多边形已经收缩过，这里只留一点余量。</summary>
	private const float RebuildAgentRadius = 0.05f;

	/// <summary>体素大小：越小越贴合原版多边形。</summary>
	private const float VoxelSize = 0.05f;

	/// <summary>
	/// 个别场景用更细的体素和更小的半径（体素、半径都是 0.025）。
	/// 加油站白天 / 夜晚 / Story2 三关里连接两片区域的木板坡道、窄通道在 0.05 下被切断，玩家路线算出 PathPartial；
	/// 但 0.025 加上贴地会把教程关的路线切断（PcNavMeshExperiment 实测），所以只对这几个场景用 0.025。
	/// </summary>
	private const float FineSize = 0.025f;

	private static readonly string[] FineScenes =
	{
		"Outdoor_GasStation_DAY",
		"Outdoor_GasStation_NIGHT",
		"Outdoor_GasStation_DAY_trimmed",
		"Outdoor_GasStation_NIGHT_trimmed",
		"Outdoor_GasStation_DAY_Story2",
		"Outdoor_GasStation_DAY_Story2_trimmed",
	};

	/// <summary>
	/// 场景路径 -> 旧 NavMesh 资源 GUID，由原始导出生成（保存过的场景会被 Unity 6 重写，丢掉这条引用）。
	/// </summary>
	private const string SceneMapFile = "pc_data/navmesh/scene_map.tsv";

	private static Dictionary<string, string> LoadSceneMap()
	{
		Dictionary<string, string> map = new Dictionary<string, string>();
		foreach (string line in File.ReadAllLines(SceneMapFile))
		{
			string[] parts = line.Split('	');
			if (parts.Length == 2)
			{
				map[parts[0]] = parts[1];
			}
		}
		return map;
	}

	/// <summary>JSON 结构（与 navmesh_extract.py 的输出对应）。</summary>
	[System.Serializable]
	private class NavMeshJson
	{
		public float[] flatVertices;
		public int[] flatTriangles;
		public float walkableHeight;
		public float walkableRadius;
		public float walkableClimb;
	}

	[MenuItem("PcPilot/Rebake NavMesh From Original")]
	public static void RebakeAll()
	{
		Directory.CreateDirectory(OutputDir);
		Dictionary<string, string> sceneMap = LoadSceneMap();
		HashSet<string> done = new HashSet<string>();
		int baked = 0;
		foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
		{
			string scenePath = buildScene.path;
			if (!done.Add(scenePath))
			{
				continue;
			}
			string navMeshGuid;
			if (!sceneMap.TryGetValue(scenePath, out navMeshGuid))
			{
				Debug.Log("[PcNavMeshRebake] no navmesh: " + scenePath);
				continue;
			}
			string jsonPath = Path.Combine(JsonDir, navMeshGuid + ".json");
			if (!File.Exists(jsonPath))
			{
				Debug.LogWarning("[PcNavMeshRebake] json missing for " + scenePath + ": " + jsonPath);
				continue;
			}
			if (RebakeScene(scenePath, jsonPath))
			{
				baked++;
			}
		}
		AssetDatabase.SaveAssets();
		Debug.Log("[PcNavMeshRebake] done, scenes baked: " + baked);
	}

	private static bool RebakeScene(string scenePath, string jsonPath)
	{
		NavMeshJson source = JsonUtility.FromJson<NavMeshJson>(File.ReadAllText(jsonPath));
		// 先打开场景：要用场景里的碰撞体校正顶点高度
		UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		Physics.SyncTransforms();
		int snapped = SnapToGround(source.flatVertices);
		Mesh mesh = BuildMesh(source);

		float agentRadius = RebuildAgentRadius;
		float voxelSize = VoxelSize;
		if (System.Array.IndexOf(FineScenes, Path.GetFileNameWithoutExtension(scenePath)) >= 0)
		{
			agentRadius = FineSize;
			voxelSize = FineSize;
		}
		NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
		settings.agentRadius = agentRadius;
		settings.agentHeight = source.walkableHeight;
		settings.agentClimb = source.walkableClimb;
		settings.agentSlope = 60f;
		settings.overrideVoxelSize = true;
		settings.voxelSize = voxelSize;
		settings.minRegionArea = 0f;

		NavMeshBuildSource buildSource = new NavMeshBuildSource();
		buildSource.shape = NavMeshBuildSourceShape.Mesh;
		buildSource.sourceObject = mesh;
		buildSource.transform = Matrix4x4.identity;
		buildSource.area = 0;
		List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
		sources.Add(buildSource);

		Bounds bounds = mesh.bounds;
		bounds.Expand(2f);
		NavMeshData data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
		if (data == null)
		{
			Debug.LogError("[PcNavMeshRebake] build failed: " + scenePath);
			return false;
		}

		string sceneName = Path.GetFileNameWithoutExtension(scenePath);
		string assetPath = OutputDir + "/" + sceneName + ".asset";
		AssetDatabase.DeleteAsset(assetPath);
		AssetDatabase.CreateAsset(data, assetPath);

		// 早先的版本用 NavMeshSurface 挂数据，但它在 OnEnable 才注册，晚于场景里 NavMeshAgent 的创建，删掉
		GameObject holder = GameObject.Find(HolderName);
		if (holder != null)
		{
			Object.DestroyImmediate(holder);
		}
		// 挂到场景自身的导航设置上（与原版一样）：场景加载时先载入导航数据，再创建代理
		SerializedObject navMeshSettings = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);
		navMeshSettings.FindProperty("m_NavMeshData").objectReferenceValue = data;
		navMeshSettings.ApplyModifiedPropertiesWithoutUndo();
		EditorSceneManager.MarkSceneDirty(scene);
		EditorSceneManager.SaveScene(scene);
		Debug.Log("[PcNavMeshRebake] baked " + scenePath + " voxel=" + voxelSize + " triangles=" + (source.flatTriangles.Length / 3) + " snapped=" + snapped + "/" + (source.flatVertices.Length / 3) + " -> " + assetPath);
		return true;
	}

	/// <summary>核对：每个场景新导航网格的面积与原版多边形面积之比（应接近 1）。</summary>
	[MenuItem("PcPilot/Verify Rebaked NavMesh")]
	public static void VerifyAll()
	{
		Dictionary<string, string> sceneMap = LoadSceneMap();
		HashSet<string> done = new HashSet<string>();
		foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
		{
			string scenePath = buildScene.path;
			if (!done.Add(scenePath))
			{
				continue;
			}
			string navMeshGuid;
			if (!sceneMap.TryGetValue(scenePath, out navMeshGuid))
			{
				continue;
			}
			NavMeshJson source = JsonUtility.FromJson<NavMeshJson>(File.ReadAllText(Path.Combine(JsonDir, navMeshGuid + ".json")));
			Mesh original = BuildMesh(source);
			float originalArea = MeshArea(original.vertices, original.triangles);

			EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
			float rebakedArea = MeshArea(triangulation.vertices, triangulation.indices);
			float ratio = 0f;
			if (originalArea > 0f)
			{
				ratio = rebakedArea / originalArea;
			}
			Debug.Log("[PcNavMeshVerify] " + Path.GetFileNameWithoutExtension(scenePath) + " original=" + originalArea.ToString("F1") + " rebaked=" + rebakedArea.ToString("F1") + " ratio=" + ratio.ToString("F3"));
			// 面积差太多的场景导出新网格，便于和原版对比
			if (ratio < 0.9f)
			{
				string objPath = Path.Combine(JsonDir, "rebaked_" + Path.GetFileNameWithoutExtension(scenePath) + ".obj");
				WriteObj(objPath, triangulation.vertices, triangulation.indices);
				Debug.Log("[PcNavMeshVerify] dumped " + objPath);
			}
		}
	}

	private static void WriteObj(string path, Vector3[] vertices, int[] triangles)
	{
		System.Text.StringBuilder text = new System.Text.StringBuilder();
		foreach (Vector3 vertex in vertices)
		{
			text.AppendLine("v " + vertex.x.ToString("R") + " " + vertex.y.ToString("R") + " " + vertex.z.ToString("R"));
		}
		for (int i = 0; i + 2 < triangles.Length; i += 3)
		{
			text.AppendLine("f " + (triangles[i] + 1) + " " + (triangles[i + 1] + 1) + " " + (triangles[i + 2] + 1));
		}
		File.WriteAllText(path, text.ToString());
	}

	private static float MeshArea(Vector3[] vertices, int[] triangles)
	{
		float area = 0f;
		for (int i = 0; i + 2 < triangles.Length; i += 3)
		{
			Vector3 a = vertices[triangles[i]];
			Vector3 b = vertices[triangles[i + 1]];
			Vector3 c = vertices[triangles[i + 2]];
			area += Vector3.Cross(b - a, c - a).magnitude * 0.5f;
		}
		return area;
	}

	/// <summary>射线起点高于顶点的距离（原版顶点高度的量化误差约 0.2 米）。</summary>
	private const float SnapRayStartAbove = 0.5f;

	/// <summary>射线最大长度。</summary>
	private const float SnapRayLength = 1.0f;

	/// <summary>
	/// 原版 Detour 多边形顶点的高度是量化过的，细长多边形两条长边的高度可能差 0.2 米左右，
	/// 重新生成时会被判成陡坡而丢掉。这里从每个顶点上方向下打射线，命中场景碰撞体就改用实际地面高度。
	/// 返回校正的顶点数。
	/// </summary>
	private static int SnapToGround(float[] flatVertices)
	{
		int snapped = 0;
		int vertexCount = flatVertices.Length / 3;
		for (int i = 0; i < vertexCount; i++)
		{
			Vector3 origin = new Vector3(flatVertices[i * 3], flatVertices[i * 3 + 1] + SnapRayStartAbove, flatVertices[i * 3 + 2]);
			RaycastHit hit;
			bool found = Physics.Raycast(origin, Vector3.down, out hit, SnapRayLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
			if (found)
			{
				flatVertices[i * 3 + 1] = hit.point.y;
				snapped++;
			}
		}
		return snapped;
	}

	private static Mesh BuildMesh(NavMeshJson source)
	{
		int vertexCount = source.flatVertices.Length / 3;
		Vector3[] vertices = new Vector3[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			vertices[i] = new Vector3(source.flatVertices[i * 3], source.flatVertices[i * 3 + 1], source.flatVertices[i * 3 + 2]);
		}
		Mesh mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		mesh.vertices = vertices;
		mesh.triangles = source.flatTriangles;
		mesh.RecalculateBounds();
		return mesh;
	}
}
