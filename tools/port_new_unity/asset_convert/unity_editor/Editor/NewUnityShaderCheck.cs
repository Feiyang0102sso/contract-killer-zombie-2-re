// Lists the compile errors of every shader in the project (ShaderUtil.GetShaderMessages), to verify what gles_to_hlsl.py translated.
// Batch call: Unity.exe -batchmode -quit -projectPath <project> -executeMethod NewUnityShaderCheck.Run -logFile <log>
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public static class NewUnityShaderCheck
{
	/// <summary>Only shaders in these folders are checked (all the original shaders are here).</summary>
	private static readonly string[] Folders = { "Assets/Resources/shaders", "Assets/Shader" };

	public static void Run()
	{
		int broken = 0;
		int total = 0;
		foreach (string guid in AssetDatabase.FindAssets("t:Shader", Folders))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			// Force a reimport so the result is from the latest file
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
			total++;
			ShaderMessage[] messages = ShaderUtil.GetShaderMessages(shader);
			int errors = 0;
			foreach (ShaderMessage message in messages)
			{
				if (message.severity == ShaderCompilerMessageSeverity.Error)
				{
					errors++;
					Debug.Log("[NewUnityShaderCheck] error " + path + " line " + message.line + " (" + message.platform + "): " + message.message);
				}
			}
			bool hasError = ShaderUtil.ShaderHasError(shader);
			if (hasError || errors > 0)
			{
				broken++;
			}
			Debug.Log("[NewUnityShaderCheck] " + (hasError ? "BROKEN " : "ok     ") + path + " name=" + shader.name + " errors=" + errors + " passes=" + shader.passCount);
		}
		Debug.Log("[NewUnityShaderCheck] done: broken " + broken + " / " + total);
	}
}
