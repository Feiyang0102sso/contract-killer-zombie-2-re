// 按原版资源包索引（Assets/index.txt）重新打 Windows 版资源包，输出到 StreamingAssets/AssetBundles。
// - 分组、文件名（<名称>_<ContentHash>.unity3d）与原版一致，游戏的加载逻辑不用改；
// - 每个资源的寻址名设成索引里的原始路径（小写），游戏用这个路径 LoadAsset；
// - 索引本身单独打成 index_<哈希>.unity3d，哈希取自原版的 index.version。
// 批处理调用：Unity.exe -batchmode -quit -projectPath <工程> -executeMethod PcAssetBundleRebuild.Rebuild -logFile <日志>
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public static class PcAssetBundleRebuild
{
	private const string IndexAssetPath = "Assets/index.txt";

	/// <summary>原版 index.version 的内容（索引包的哈希）。</summary>
	private const string IndexHash = "81A8FFF6E96349FAA9381ED91B3A88EC";

	private const string IndexBundleName = "index";

	private const string OutputDir = "Assets/StreamingAssets/AssetBundles";

	[MenuItem("PcPilot/Rebuild AssetBundles (Windows x64)")]
	public static void Rebuild()
	{
		Dictionary<string, string> projectPaths = BuildProjectPathMap();
		List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
		int missing = 0;

		XmlDocument document = new XmlDocument();
		document.Load(IndexAssetPath);
		foreach (XmlElement bundleElement in document.SelectNodes("/Index/AssetBundles/AssetBundle"))
		{
			string bundleName = bundleElement.GetAttribute("Filename");
			string contentHash = bundleElement.GetAttribute("ContentHash");
			List<string> assetNames = new List<string>();
			List<string> addressableNames = new List<string>();
			foreach (XmlElement assetElement in bundleElement.SelectNodes("Assets/Asset"))
			{
				string originalPath = assetElement.GetAttribute("Filename");
				string projectPath = ResolveProjectPath(originalPath, projectPaths);
				if (projectPath == null)
				{
					// 索引里也列了目录（如 Assets/Prefabs/Hud），目录不是资源，跳过
					if (!AssetDatabase.IsValidFolder(originalPath) && Path.HasExtension(originalPath))
					{
						Debug.LogWarning("[PcAssetBundleRebuild] asset not found: " + originalPath);
						missing++;
					}
					continue;
				}
				assetNames.Add(projectPath);
				addressableNames.Add(originalPath.ToLowerInvariant());
			}

			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = bundleName + "_" + contentHash + ".unity3d";
			build.assetNames = assetNames.ToArray();
			build.addressableNames = addressableNames.ToArray();
			builds.Add(build);
			Debug.Log("[PcAssetBundleRebuild] bundle " + build.assetBundleName + ": " + assetNames.Count + " assets");
		}

		// 索引包：只含索引文本本身
		AssetBundleBuild indexBuild = new AssetBundleBuild();
		indexBuild.assetBundleName = IndexBundleName + "_" + IndexHash + ".unity3d";
		indexBuild.assetNames = new string[] { IndexAssetPath };
		indexBuild.addressableNames = new string[] { IndexBundleName };
		builds.Add(indexBuild);

		Directory.CreateDirectory(OutputDir);
		AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(OutputDir, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
		File.WriteAllText(Path.Combine(OutputDir, "index.version"), IndexHash);

		if (manifest == null)
		{
			Debug.LogError("[PcAssetBundleRebuild] build failed");
			if (Application.isBatchMode)
			{
				EditorApplication.Exit(1);
			}
			return;
		}
		Debug.Log("[PcAssetBundleRebuild] done: " + builds.Count + " bundles, missing assets: " + missing);
	}

	/// <summary>工程内资源：小写路径 -> 实际路径；另外登记一份去掉扩展名的，用于扩展名变了的资源（如 tga 导出成 png）。</summary>
	private static Dictionary<string, string> BuildProjectPathMap()
	{
		Dictionary<string, string> map = new Dictionary<string, string>();
		foreach (string path in AssetDatabase.GetAllAssetPaths())
		{
			if (!path.StartsWith("Assets/") || AssetDatabase.IsValidFolder(path))
			{
				continue;
			}
			string lower = path.ToLowerInvariant();
			map[lower] = path;
			string withoutExtension = "noext:" + StripExtension(lower);
			if (!map.ContainsKey(withoutExtension))
			{
				map[withoutExtension] = path;
			}
		}
		return map;
	}

	private static string ResolveProjectPath(string originalPath, Dictionary<string, string> projectPaths)
	{
		string lower = originalPath.ToLowerInvariant();
		string found;
		if (projectPaths.TryGetValue(lower, out found))
		{
			return found;
		}
		if (projectPaths.TryGetValue("noext:" + StripExtension(lower), out found))
		{
			Debug.Log("[PcAssetBundleRebuild] matched by name only: " + originalPath + " -> " + found);
			return found;
		}
		return null;
	}

	private static string StripExtension(string path)
	{
		string extension = Path.GetExtension(path);
		if (string.IsNullOrEmpty(extension))
		{
			return path;
		}
		return path.Substring(0, path.Length - extension.Length);
	}
}
