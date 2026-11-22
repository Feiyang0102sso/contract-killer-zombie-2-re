// PC build: rebuilds the asset bundles from the original index first (StreamingAssets/AssetBundles is not committed),
// then builds a Windows x64 player from the scenes in EditorBuildSettings (duplicates removed),
// and finally replaces Unity's player stub ckz2.exe with the launcher (Launcher/build_launcher.bat).
// Two flavors with the same code, only Unity's Development flag differs:
//   Dev     -> Builds/Dev/ckz2.exe      (build_dev.bat,     -executeMethod PcBuild.BuildDev)
//   Release -> Builds/Release/ckz2.exe  (build_release.bat, -executeMethod PcBuild.BuildRelease)
// Batch call: Unity.exe -batchmode -quit -projectPath <project> -executeMethod PcBuild.BuildDev -logFile <log>
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class PcBuild
{
	/// <summary>Output folder of the development build, relative to the project root.</summary>
	private const string DevOutputDir = "Builds/Dev";

	/// <summary>Output folder of the release build, relative to the project root.</summary>
	private const string ReleaseOutputDir = "Builds/Release";

	/// <summary>Player exe name; the launcher of the same name replaces it.</summary>
	private const string ExeName = "ckz2.exe";

	/// <summary>Builds the launcher with MSVC and copies it over the player stub, relative to the project root.</summary>
	private const string LauncherBuildScript = "Launcher/build_launcher.bat";

	[MenuItem("CKZ2/Build/Windows x64 (Development)")]
	public static void BuildDev()
	{
		Build(DevOutputDir, BuildOptions.Development);
	}

	[MenuItem("CKZ2/Build/Windows x64 (Release)")]
	public static void BuildRelease()
	{
		Build(ReleaseOutputDir, BuildOptions.None);
	}

	private static void Build(string outputDir, BuildOptions buildOptions)
	{
		Debug.Log("[PcBuild] output: " + outputDir + ", options: " + buildOptions);

		// Asset bundles are build output: rebuilt every time so they match the current assets
		PcAssetBundleRebuild.Rebuild();

		// The ULegacy export has two levels with the same name, so the scene list repeats a path; keep the first one
		List<string> scenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled)
			{
				continue;
			}
			if (scenes.Contains(scene.path))
			{
				Debug.LogWarning("[PcBuild] skip duplicate scene: " + scene.path);
				continue;
			}
			scenes.Add(scene.path);
		}
		Debug.Log("[PcBuild] scenes: " + scenes.Count);

		BuildPlayerOptions options = new BuildPlayerOptions();
		options.scenes = scenes.ToArray();
		options.locationPathName = outputDir + "/" + ExeName;
		options.target = BuildTarget.StandaloneWindows64;
		options.options = buildOptions;

		BuildReport report = BuildPipeline.BuildPlayer(options);
		BuildSummary summary = report.summary;
		Debug.Log("[PcBuild] result: " + summary.result + ", errors: " + summary.totalErrors + ", size: " + summary.totalSize + ", time: " + summary.totalTime);
		bool succeeded = summary.result == BuildResult.Succeeded;
		if (succeeded)
		{
			succeeded = BuildLauncher(outputDir);
		}
		// Report failure through the exit code only in batch mode; the menu call must not quit the editor
		if (!succeeded && Application.isBatchMode)
		{
			EditorApplication.Exit(1);
		}
	}

	/// <summary>Runs Launcher/build_launcher.bat for one build folder; its output goes to the Unity log.</summary>
	private static bool BuildLauncher(string outputDir)
	{
		string projectDir = Directory.GetCurrentDirectory();
		string scriptPath = Path.Combine(projectDir, LauncherBuildScript);
		string targetDir = Path.Combine(projectDir, outputDir);
		// "call" makes cmd return the script's exit /b code; without it a failure inside an if block can come back as 0
		ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c call \"" + scriptPath + "\" \"" + targetDir + "\"");
		startInfo.WorkingDirectory = projectDir;
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;
		startInfo.CreateNoWindow = true;
		using (Process process = Process.Start(startInfo))
		{
			string output = process.StandardOutput.ReadToEnd();
			string errors = process.StandardError.ReadToEnd();
			process.WaitForExit();
			Debug.Log("[PcBuild] launcher build output:\n" + output + errors);
			if (process.ExitCode != 0)
			{
				Debug.LogError("[PcBuild] launcher build failed, exit code " + process.ExitCode);
				return false;
			}
		}
		Debug.Log("[PcBuild] launcher done");
		return true;
	}
}
