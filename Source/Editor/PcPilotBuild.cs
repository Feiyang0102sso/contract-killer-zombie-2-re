// 试点构建：先按原版索引重打资源包（StreamingAssets/AssetBundles 不入库），再用 EditorBuildSettings 里的场景（去掉重复项）打 Windows x64 开发版。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcPilotBuild.Build -logFile <日志>
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class PcPilotBuild
{
	/// <summary>输出路径（相对工程根目录）。</summary>
	private const string OutputPath = "Builds/ckz2.exe";

	[MenuItem("PcPilot/Build Windows x64 (Development)")]
	public static void Build()
	{
		// 资源包是构建产物，每次构建前重打，保证与当前素材一致
		PcAssetBundleRebuild.Rebuild();

		// ULegacy 导出时有两个同名关卡，场景列表里出现了重复路径，这里只保留第一次出现的
		List<string> scenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled)
			{
				continue;
			}
			if (scenes.Contains(scene.path))
			{
				Debug.LogWarning("[PcPilotBuild] skip duplicate scene: " + scene.path);
				continue;
			}
			scenes.Add(scene.path);
		}
		Debug.Log("[PcPilotBuild] scenes: " + scenes.Count);

		BuildPlayerOptions options = new BuildPlayerOptions();
		options.scenes = scenes.ToArray();
		options.locationPathName = OutputPath;
		options.target = BuildTarget.StandaloneWindows64;
		options.options = BuildOptions.Development;

		BuildReport report = BuildPipeline.BuildPlayer(options);
		BuildSummary summary = report.summary;
		Debug.Log("[PcPilotBuild] result: " + summary.result + ", errors: " + summary.totalErrors + ", size: " + summary.totalSize + ", time: " + summary.totalTime);
		// 只在批处理模式下用退出码报告失败；在编辑器菜单里调用时不能退出编辑器
		if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
		{
			EditorApplication.Exit(1);
		}
	}
}
