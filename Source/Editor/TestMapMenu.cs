// Test Map Menu: lists the test maps (every scene in Assets/Scenes/_TestMaps) with Play and Open buttons.
// Menu: CKZ2/Test Map Menu.
//
// Play (or pressing Play with a test map open): a level can't start on its own (weapons and configs come from
// asset bundles loaded during boot), so Play starts from _Intro and PcTestMap (Boot.Done hook) goes straight
// into the test map, with a temporary save and no tutorial.
// The map is added to Build Settings if missing, since the game loads levels by scene name.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class TestMapMenu : EditorWindow
{
	/// <summary>Folder holding the test map scenes.</summary>
	private const string TestMapFolder = "Assets/Scenes/_TestMaps";

	/// <summary>Boot scene; Play always starts here.</summary>
	private const string BootScenePath = "Assets/Scenes/_Basic/_Intro.unity";

	/// <summary>
	/// Pressing Play while a test map is open also works: Play then starts from _Intro (playModeStartScene)
	/// and the open map is loaded after boot.
	/// </summary>
	static TestMapMenu()
	{
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange change)
	{
		if (change == PlayModeStateChange.ExitingEditMode)
		{
			string activePath = EditorSceneManager.GetActiveScene().path;
			if (activePath.StartsWith(TestMapFolder + "/"))
			{
				AddToBuildSettings(activePath);
				SessionState.SetString(PcTestMap.SceneKey, Path.GetFileNameWithoutExtension(activePath));
				EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
				Debug.Log("[TestMapMenu] play " + activePath + " (starting from _Intro)");
			}
		}
		else if (change == PlayModeStateChange.EnteredEditMode)
		{
			EditorSceneManager.playModeStartScene = null;
		}
	}

	[MenuItem("CKZ2/Test Map Menu")]
	public static void Open()
	{
		GetWindow<TestMapMenu>("Test Maps");
	}

	private void OnGUI()
	{
		EditorGUILayout.HelpBox("Play boots the game from _Intro and then loads the map. Open edits the map scene.\nMaps: every scene in " + TestMapFolder + ".", MessageType.Info);
		List<string> mapPaths = FindTestMaps();
		if (mapPaths.Count == 0)
		{
			EditorGUILayout.LabelField("No test maps found.");
			return;
		}
		GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
		foreach (string mapPath in mapPaths)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(mapPath));
			if (GUILayout.Button("Play", GUILayout.Width(60)))
			{
				PlayTestMap(mapPath);
			}
			if (GUILayout.Button("Open", GUILayout.Width(60)))
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(mapPath);
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		GUI.enabled = true;
	}

	private static List<string> FindTestMaps()
	{
		List<string> mapPaths = new List<string>();
		if (!AssetDatabase.IsValidFolder(TestMapFolder))
		{
			return mapPaths;
		}
		foreach (string guid in AssetDatabase.FindAssets("t:Scene", new string[] { TestMapFolder }))
		{
			mapPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
		}
		mapPaths.Sort();
		return mapPaths;
	}

	private static void PlayTestMap(string mapPath)
	{
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			return;
		}
		// Same as opening the map and pressing Play (see OnPlayModeStateChanged)
		EditorSceneManager.OpenScene(mapPath);
		EditorApplication.EnterPlaymode();
	}

	private static void AddToBuildSettings(string mapPath)
	{
		List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
		foreach (EditorBuildSettingsScene scene in scenes)
		{
			if (scene.path == mapPath)
			{
				return;
			}
		}
		scenes.Add(new EditorBuildSettingsScene(mapPath, true));
		EditorBuildSettings.scenes = scenes.ToArray();
		Debug.Log("[TestMapMenu] added to Build Settings: " + mapPath);
	}
}
