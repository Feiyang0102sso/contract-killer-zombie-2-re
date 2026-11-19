// 把原版的光照贴图分配和光照探针写回场景：每个场景加一个 PcSceneLighting 物体（运行时组件见 PcPort/PcSceneLighting.cs）。
// 数据由 tools/scene_lighting_extract.py 从未改动的导出提取，放在 pc_data/lighting/<场景名>.json。
// 渲染器按组件的 fileID 对应（GlobalObjectId.targetObjectId；场景被 Unity 6 重存过 fileID 也不变）。可以重复运行。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcSceneLightingRestore.RestoreAll -logFile <日志>
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PcSceneLightingRestore
{
	private const string JsonDir = "pc_data/lighting";

	private const string HolderName = "PcSceneLighting";

	/// <summary>JSON 结构（与 scene_lighting_extract.py 的输出对应）。</summary>
	[System.Serializable]
	private class ProbeJson
	{
		public float[] positions;
		public float[] coefficients;
		public int[] tetraIndices;
		public int[] tetraNeighbors;
	}

	[System.Serializable]
	private class SceneJson
	{
		public string scenePath;
		public string[] lightmapGuids;
		public string[] rendererFileIds;
		public int[] rendererLightmapIndices;
		public float[] rendererScaleOffsets;
		public string[] probeRendererFileIds;
		public ProbeJson probes;
	}

	[MenuItem("PcPilot/Restore Scene Lighting From Original")]
	public static void RestoreAll()
	{
		int scenes = 0;
		foreach (string jsonPath in Directory.GetFiles(JsonDir, "*.json"))
		{
			SceneJson data = JsonUtility.FromJson<SceneJson>(File.ReadAllText(jsonPath));
			if (!File.Exists(data.scenePath))
			{
				Debug.LogWarning("[PcLightingRestore] scene not found: " + data.scenePath);
				continue;
			}
			RestoreScene(data);
			scenes++;
		}
		Debug.Log("[PcLightingRestore] done: " + scenes + " scenes");
	}

	private static void RestoreScene(SceneJson data)
	{
		UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(data.scenePath, OpenSceneMode.Single);
		Dictionary<ulong, Renderer> byFileId = new Dictionary<ulong, Renderer>();
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
			{
				GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(renderer);
				byFileId[id.targetObjectId] = renderer;
			}
		}

		// 旧的同名物体先删掉，保证可以重复运行
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			if (root.name == HolderName)
			{
				Object.DestroyImmediate(root);
			}
		}
		GameObject holder = new GameObject(HolderName);
		PcSceneLighting lighting = holder.AddComponent<PcSceneLighting>();

		List<Texture2D> lightmaps = new List<Texture2D>();
		foreach (string guid in data.lightmapGuids)
		{
			Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
			if (texture == null)
			{
				Debug.LogWarning("[PcLightingRestore] lightmap not found: " + guid + " in " + data.scenePath);
			}
			lightmaps.Add(texture);
		}
		lighting.lightmaps = lightmaps.ToArray();

		List<Renderer> renderers = new List<Renderer>();
		List<int> indices = new List<int>();
		List<Vector4> scaleOffsets = new List<Vector4>();
		int missing = 0;
		for (int i = 0; i < data.rendererFileIds.Length; i++)
		{
			Renderer renderer;
			if (!byFileId.TryGetValue(ulong.Parse(data.rendererFileIds[i]), out renderer))
			{
				missing++;
				continue;
			}
			renderers.Add(renderer);
			indices.Add(data.rendererLightmapIndices[i]);
			int offset = i * 4;
			scaleOffsets.Add(new Vector4(data.rendererScaleOffsets[offset], data.rendererScaleOffsets[offset + 1],
				data.rendererScaleOffsets[offset + 2], data.rendererScaleOffsets[offset + 3]));
		}
		lighting.lightmappedRenderers = renderers.ToArray();
		lighting.lightmapIndices = indices.ToArray();
		lighting.lightmapScaleOffsets = scaleOffsets.ToArray();

		int probeCount = data.probes.positions.Length / 3;
		Vector3[] positions = new Vector3[probeCount];
		for (int p = 0; p < probeCount; p++)
		{
			positions[p] = new Vector3(data.probes.positions[p * 3], data.probes.positions[p * 3 + 1], data.probes.positions[p * 3 + 2]);
		}
		lighting.probePositions = positions;
		lighting.probeCoefficients = data.probes.coefficients;
		lighting.tetraIndices = data.probes.tetraIndices;
		lighting.tetraNeighbors = data.probes.tetraNeighbors;

		EditorSceneManager.MarkSceneDirty(scene);
		EditorSceneManager.SaveScene(scene);
		Debug.Log("[PcLightingRestore] " + data.scenePath + ": lightmaps=" + lightmaps.Count + " renderers=" + renderers.Count
			+ " missing=" + missing + " probes=" + probeCount);
	}
}
