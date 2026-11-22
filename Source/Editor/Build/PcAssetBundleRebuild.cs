// Rebuilds the Windows asset bundles from the original bundle index (Assets/index.txt) into StreamingAssets/AssetBundles.
// - Grouping and file names (<name>_<ContentHash>.unity3d) match the original, so the game's loading code is unchanged;
// - each asset's addressable name is its original path in the index (lower case), which the game passes to LoadAsset;
// - the index itself goes into its own index_<hash>.unity3d, the hash taken from the original index.version.
// Batch call: Unity.exe -batchmode -quit -projectPath <project> -executeMethod PcAssetBundleRebuild.Rebuild -logFile <log>
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public static class PcAssetBundleRebuild
{
	private const string IndexAssetPath = "Assets/index.txt";

	/// <summary>Content of the original index.version (hash of the index bundle).</summary>
	private const string IndexHash = "81A8FFF6E96349FAA9381ED91B3A88EC";

	private const string IndexBundleName = "index";

	private const string OutputDir = "Assets/StreamingAssets/AssetBundles";

	[MenuItem("CKZ2/Build/Rebuild AssetBundles (Windows x64)")]
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
					// The index also lists folders (e.g. Assets/Prefabs/Hud); folders are not assets, skip them
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

		// Index bundle: holds only the index text itself
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

	/// <summary>Project assets: lower-case path -> actual path, plus an entry without extension for assets whose extension changed (e.g. tga exported as png).</summary>
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
