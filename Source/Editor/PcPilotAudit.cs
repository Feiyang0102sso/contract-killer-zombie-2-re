// 试点检查：逐个打开场景和预制体，统计丢失的组件 / 脚本、着色器出错的材质和组件类型分布。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcPilotAudit.Run -logFile <日志>
// 结果写到工程根目录的 PcPilotAudit.txt。
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PcPilotAudit
{
	private const string ReportFileName = "PcPilotAudit.txt";

	/// <summary>单个场景或预制体的统计结果。</summary>
	private class AuditResult
	{
		public int gameObjects;
		public int missingComponents;
		public int brokenShaderMaterials;
		public Dictionary<string, int> componentCounts = new Dictionary<string, int>();
	}

	public static void Run()
	{
		StringBuilder report = new StringBuilder();
		AuditResult total = new AuditResult();
		HashSet<string> brokenShaders = new HashSet<string>();

		string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" });
		report.AppendLine("== Scenes: " + sceneGuids.Length);
		foreach (string guid in sceneGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			// 游戏代码里有同名的全局类 Scene，这里必须写全名
			UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			AuditResult result = new AuditResult();
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				AuditHierarchy(root, result, brokenShaders);
			}
			report.AppendLine(FormatLine(path, result));
			Merge(total, result);
			Debug.Log("[PcPilotAudit] scene done: " + path);
		}

		string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
		report.AppendLine("== Prefabs: " + prefabGuids.Length);
		AuditResult prefabTotal = new AuditResult();
		foreach (string guid in prefabGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
			AuditResult result = new AuditResult();
			AuditHierarchy(prefabRoot, result, brokenShaders);
			PrefabUtility.UnloadPrefabContents(prefabRoot);
			if (result.missingComponents > 0 || result.brokenShaderMaterials > 0)
			{
				report.AppendLine(FormatLine(path, result));
			}
			Merge(prefabTotal, result);
		}
		report.AppendLine("prefab total: " + FormatCounts(prefabTotal));
		Merge(total, prefabTotal);

		report.AppendLine("== Shaders with errors or unsupported (" + brokenShaders.Count + ")");
		foreach (string shaderName in brokenShaders)
		{
			report.AppendLine("  " + shaderName);
		}

		report.AppendLine("== Component types (scenes + prefabs)");
		List<KeyValuePair<string, int>> sorted = new List<KeyValuePair<string, int>>(total.componentCounts);
		sorted.Sort(CompareByCountDescending);
		foreach (KeyValuePair<string, int> pair in sorted)
		{
			report.AppendLine("  " + pair.Value.ToString().PadLeft(7) + " " + pair.Key);
		}
		report.AppendLine("== TOTAL " + FormatCounts(total));

		File.WriteAllText(ReportFileName, report.ToString());
		Debug.Log("[PcPilotAudit] report written: " + Path.GetFullPath(ReportFileName));
	}

	/// <summary>递归统计一个物体及其子物体。</summary>
	private static void AuditHierarchy(GameObject gameObject, AuditResult result, HashSet<string> brokenShaders)
	{
		result.gameObjects++;
		foreach (Component component in gameObject.GetComponents<Component>())
		{
			if (component == null)
			{
				// 丢失的脚本，或 Unity 已不认识的旧组件
				result.missingComponents++;
				continue;
			}
			string typeName = component.GetType().Name;
			int count;
			result.componentCounts.TryGetValue(typeName, out count);
			result.componentCounts[typeName] = count + 1;

			Renderer renderer = component as Renderer;
			if (renderer != null)
			{
				CheckMaterials(renderer, result, brokenShaders);
			}
		}
		foreach (Transform child in gameObject.transform)
		{
			AuditHierarchy(child.gameObject, result, brokenShaders);
		}
	}

	/// <summary>着色器为空、不受支持或有编译错误的材质会显示成粉色。</summary>
	private static void CheckMaterials(Renderer renderer, AuditResult result, HashSet<string> brokenShaders)
	{
		foreach (Material material in renderer.sharedMaterials)
		{
			if (material == null)
			{
				continue;
			}
			Shader shader = material.shader;
			bool broken = shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader);
			if (broken)
			{
				result.brokenShaderMaterials++;
				string shaderName = "<null>";
				if (shader != null)
				{
					shaderName = shader.name;
				}
				brokenShaders.Add(shaderName);
			}
		}
	}

	private static void Merge(AuditResult target, AuditResult source)
	{
		target.gameObjects += source.gameObjects;
		target.missingComponents += source.missingComponents;
		target.brokenShaderMaterials += source.brokenShaderMaterials;
		foreach (KeyValuePair<string, int> pair in source.componentCounts)
		{
			int count;
			target.componentCounts.TryGetValue(pair.Key, out count);
			target.componentCounts[pair.Key] = count + pair.Value;
		}
	}

	private static string FormatLine(string path, AuditResult result)
	{
		return "  " + path + " | " + FormatCounts(result);
	}

	private static string FormatCounts(AuditResult result)
	{
		return "objects=" + result.gameObjects + " missingComponents=" + result.missingComponents + " brokenShaderMaterials=" + result.brokenShaderMaterials;
	}

	private static int CompareByCountDescending(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
	{
		return b.Value.CompareTo(a.Value);
	}
}
