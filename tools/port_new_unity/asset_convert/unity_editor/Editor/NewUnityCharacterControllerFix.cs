// Fixes CharacterController stepOffset.
// Unity 6 rejects an out-of-range stepOffset with "Step Offset must be less or equal to <scaled Height> + <scaled Radius> * 2"
// and does not create the controller (every later Move logs "called on inactive controller"); Unity 3.5 does not check.
// Measured: height 0.1, radius 0.1, stepOffset 0.3 (Main Camera, which MiniZombieCamera uses for line-of-sight collision)
// is rejected, though the literal formula 0.1 + 0.1 * 2 = 0.3 should pass. Inferred: Unity takes the capsule height as
// max(height, 2 * radius) (a sphere when the height is under twice the radius), so the limit is
// max(scaled height, scaled radius * 2). This finds every controller over the limit (build scenes + all prefabs) and
// lowers stepOffset below it, leaving the other parameters alone. Every change is logged (old value, scale, new value).
// Batch call: Unity.exe -batchmode -quit -projectPath <project> -executeMethod NewUnityCharacterControllerFix.Run -logFile <log>
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NewUnityCharacterControllerFix
{
	/// <summary>Extra margin below the limit, so float error does not trigger the check again.</summary>
	private const float Margin = 0.001f;

	public static void Run()
	{
		int fixedCount = 0;
		HashSet<string> done = new HashSet<string>();
		foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
		{
			if (!done.Add(buildScene.path))
			{
				continue;
			}
			UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
			int sceneFixed = 0;
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				sceneFixed += FixAll(root, buildScene.path);
			}
			if (sceneFixed > 0)
			{
				EditorSceneManager.MarkSceneDirty(scene);
				EditorSceneManager.SaveScene(scene);
			}
			fixedCount += sceneFixed;
		}
		foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (asset == null || asset.GetComponentsInChildren<CharacterController>(true).Length == 0)
			{
				continue;
			}
			GameObject contents = PrefabUtility.LoadPrefabContents(path);
			int prefabFixed = FixAll(contents, path);
			if (prefabFixed > 0)
			{
				PrefabUtility.SaveAsPrefabAsset(contents, path);
			}
			PrefabUtility.UnloadPrefabContents(contents);
			fixedCount += prefabFixed;
		}
		AssetDatabase.SaveAssets();
		Debug.Log("[NewUnityCCFix] done: fixed " + fixedCount);
	}

	/// <summary>Fixes every out-of-range controller under root; returns how many were fixed.</summary>
	private static int FixAll(GameObject root, string assetPath)
	{
		int count = 0;
		foreach (CharacterController controller in root.GetComponentsInChildren<CharacterController>(true))
		{
			Vector3 scale = controller.transform.lossyScale;
			float scaledHeight = controller.height * Mathf.Abs(scale.y);
			float scaledRadius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
			float limit = Mathf.Max(scaledHeight, scaledRadius * 2f);
			// Read the serialized value: the component rejects an out-of-range value, so the property may not hold what the file has
			SerializedObject serialized = new SerializedObject(controller);
			SerializedProperty stepOffset = serialized.FindProperty("m_StepOffset");
			if (stepOffset.floatValue <= limit)
			{
				continue;
			}
			float newValue = Mathf.Max(0f, limit - Margin);
			Debug.Log("[NewUnityCCFix] " + assetPath + " " + GetPath(controller.transform) + ": stepOffset " + stepOffset.floatValue
				+ " height " + controller.height + " radius " + controller.radius + " scale " + scale + " -> " + newValue);
			stepOffset.floatValue = newValue;
			serialized.ApplyModifiedPropertiesWithoutUndo();
			count++;
		}
		return count;
	}

	private static string GetPath(Transform transform)
	{
		string path = transform.name;
		Transform current = transform.parent;
		while (current != null)
		{
			path = current.name + "/" + path;
			current = current.parent;
		}
		return path;
	}
}
