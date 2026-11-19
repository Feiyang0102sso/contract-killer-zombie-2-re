// 列出工程里每个着色器的编译错误（ShaderUtil.GetShaderMessages），用来验证 gles_to_hlsl.py 的翻译结果。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcShaderCheck.Run -logFile <日志>
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public static class PcShaderCheck
{
	/// <summary>只检查这些目录下的着色器（原版的着色器都在这里）。</summary>
	private static readonly string[] Folders = { "Assets/Resources/shaders", "Assets/Shader" };

	[MenuItem("PcPilot/Check Shaders")]
	public static void Run()
	{
		int broken = 0;
		int total = 0;
		foreach (string guid in AssetDatabase.FindAssets("t:Shader", Folders))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			// 强制重新导入，确保拿到的是最新文件的编译结果
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
					Debug.Log("[PcShaderCheck] error " + path + " line " + message.line + " (" + message.platform + "): " + message.message);
				}
			}
			bool hasError = ShaderUtil.ShaderHasError(shader);
			if (hasError || errors > 0)
			{
				broken++;
			}
			Debug.Log("[PcShaderCheck] " + (hasError ? "BROKEN " : "ok     ") + path + " name=" + shader.name + " errors=" + errors + " passes=" + shader.passCount);
		}
		Debug.Log("[PcShaderCheck] done: broken " + broken + " / " + total);
	}
}
