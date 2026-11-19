// 修正 CharacterController 的 stepOffset。
// Unity 6 对超限的 stepOffset 报 "Step Offset must be less or equal to <scaled Height> + <scaled Radius> * 2"，
// 并且不创建这个控制器（之后每次 Move 都报 "called on inactive controller"）；Unity 3.5 不检查。
// 实测：height 0.1、radius 0.1、stepOffset 0.3（Main Camera，MiniZombieCamera 用它做视线碰撞）被拒，
// 按字面公式 0.1 + 0.1 * 2 = 0.3 本应通过。推断 Unity 把胶囊总高取 max(height, 2 * radius)（高度不足两倍半径时按球处理），
// 上限是 max(缩放后的 height, 缩放后的 radius * 2)。这里找出所有超限的控制器（构建场景 + 全部预制体），
// 把 stepOffset 压到上限以下，其余参数不动。每一处改动都打日志（原值、缩放、新值）。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcCharacterControllerFix.Run -logFile <日志>
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PcCharacterControllerFix
{
	/// <summary>压到上限时再留一点余量，避免浮点误差再次触发检查。</summary>
	private const float Margin = 0.001f;

	[MenuItem("PcPilot/Fix CharacterController Step Offset")]
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
		Debug.Log("[PcCCFix] done: fixed " + fixedCount);
	}

	/// <summary>修正 root 下所有超限的控制器，返回修正个数。</summary>
	private static int FixAll(GameObject root, string assetPath)
	{
		int count = 0;
		foreach (CharacterController controller in root.GetComponentsInChildren<CharacterController>(true))
		{
			Vector3 scale = controller.transform.lossyScale;
			float scaledHeight = controller.height * Mathf.Abs(scale.y);
			float scaledRadius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
			float limit = Mathf.Max(scaledHeight, scaledRadius * 2f);
			// 读序列化值：超限的值不会被组件接受，属性读到的可能已不是文件里的值
			SerializedObject serialized = new SerializedObject(controller);
			SerializedProperty stepOffset = serialized.FindProperty("m_StepOffset");
			if (stepOffset.floatValue <= limit)
			{
				continue;
			}
			float newValue = Mathf.Max(0f, limit - Margin);
			Debug.Log("[PcCCFix] " + assetPath + " " + GetPath(controller.transform) + ": stepOffset " + stepOffset.floatValue
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
