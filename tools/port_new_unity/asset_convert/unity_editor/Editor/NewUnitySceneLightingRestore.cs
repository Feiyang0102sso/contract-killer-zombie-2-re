// Writes the original lightmap assignment and light probes back into the scenes: each scene gets a PcSceneLighting
// object (runtime component: PcPort/PcSceneLighting.cs).
// Data comes from the untouched export via scene_lighting_extract.py, in tools/port_new_unity/asset_convert/data/lighting/<scene name>.json.
// Renderers are matched by component fileID (GlobalObjectId.targetObjectId; it survives Unity 6 re-saving the scene).
// Safe to run again: an existing PcSceneLighting object is reused, so a rerun only changes the fields.
// Batch mode: Unity.exe -batchmode -quit -projectPath <project> -executeMethod NewUnitySceneLightingRestore.RestoreAll -logFile <log>
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NewUnitySceneLightingRestore
{
	private const string JsonDir = "tools/port_new_unity/asset_convert/data/lighting";

	private const string HolderName = "PcSceneLighting";

	/// <summary>JSON layout (matches the output of scene_lighting_extract.py).</summary>
	[System.Serializable]
	private class ProbeJson
	{
		public float[] positions;
		public float[] coefficients;
		public int[] tetraIndices;
		public int[] tetraNeighbors;
		public float[] hullRays;
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

	public static void RestoreAll()
	{
		int scenes = 0;
		foreach (string jsonPath in Directory.GetFiles(JsonDir, "*.json"))
		{
			SceneJson data = JsonUtility.FromJson<SceneJson>(File.ReadAllText(jsonPath));
			if (!File.Exists(data.scenePath))
			{
				Debug.LogWarning("[NewUnityLightingRestore] scene not found: " + data.scenePath);
				continue;
			}
			RestoreScene(data);
			scenes++;
		}
		Debug.Log("[NewUnityLightingRestore] done: " + scenes + " scenes");
	}

	/// <summary>The scene's PcSceneLighting component: the existing one if there is one, otherwise a new holder object.</summary>
	private static PcSceneLighting GetOrCreateHolder(UnityEngine.SceneManagement.Scene scene)
	{
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			if (root.name != HolderName)
			{
				continue;
			}
			PcSceneLighting existing = root.GetComponent<PcSceneLighting>();
			if (existing != null)
			{
				return existing;
			}
		}
		GameObject holder = new GameObject(HolderName);
		return holder.AddComponent<PcSceneLighting>();
	}

	/// <summary>Flat xyz array -> Vector3 array.</summary>
	private static Vector3[] ToVectors(float[] flat)
	{
		int count = flat.Length / 3;
		Vector3[] vectors = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			vectors[i] = new Vector3(flat[i * 3], flat[i * 3 + 1], flat[i * 3 + 2]);
		}
		return vectors;
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

		PcSceneLighting lighting = GetOrCreateHolder(scene);

		List<Texture2D> lightmaps = new List<Texture2D>();
		foreach (string guid in data.lightmapGuids)
		{
			Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
			if (texture == null)
			{
				Debug.LogWarning("[NewUnityLightingRestore] lightmap not found: " + guid + " in " + data.scenePath);
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

		lighting.probePositions = ToVectors(data.probes.positions);
		lighting.probeCoefficients = data.probes.coefficients;
		lighting.tetraIndices = data.probes.tetraIndices;
		lighting.tetraNeighbors = data.probes.tetraNeighbors;
		lighting.hullRays = ToVectors(data.probes.hullRays);

		EditorUtility.SetDirty(lighting);
		EditorSceneManager.MarkSceneDirty(scene);
		EditorSceneManager.SaveScene(scene);
		Debug.Log("[NewUnityLightingRestore] " + data.scenePath + ": lightmaps=" + lightmaps.Count + " renderers=" + renderers.Count
			+ " missing=" + missing + " probes=" + lighting.probePositions.Length + " hullRays=" + lighting.hullRays.Length);
	}
}
