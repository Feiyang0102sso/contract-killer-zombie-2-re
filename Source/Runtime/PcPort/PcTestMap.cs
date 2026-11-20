// PC port test maps (editor only): Play with a test map open (Assets/Scenes/_TestMaps) boots from _Intro, then:
//   - uses its own temporary save, Save/TestMap, wiped at every launch, so the real save is never touched;
//   - skips the tutorial and the city map: boot goes straight into the test map.
// The editor side (Source/Editor/TestMapMenu.cs) passes the map name in SessionState.
// Hooks: StorageManager.PersistentDataPath (save folder), Boot.Done (start the map). In a build this does nothing.
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PcTestMap
{
	/// <summary>SessionState key holding the test map scene name; set by the editor when Play starts.</summary>
	public const string SceneKey = "CKZ2.TestMapScene";

	/// <summary>Temporary save folder, inside the normal save folder.</summary>
	private const string SaveDirName = "TestMap";

	/// <summary>
	/// Test maps use a "usual" mission type: survival types add infection to the save on every launch.
	/// The test map's mission branch is named {Type:KillAll} to match.
	/// </summary>
	private const Mission.Type TestMissionType = Mission.Type.KillAll;

	/// <summary>Test map to start, or null when Play is a normal game start.</summary>
	private static string _sceneName;

	/// <summary>The temporary save was already cleared in this run.</summary>
	private static bool _saveWiped;

	public static bool IsActive
	{
		get
		{
			return _sceneName != null;
		}
	}

#if UNITY_EDITOR
	/// <summary>Runs before the first scene loads, so the save folder is decided before anything reads it.</summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ReadRequest()
	{
		_sceneName = null;
		_saveWiped = false;
		string sceneName = SessionState.GetString(SceneKey, string.Empty);
		if (sceneName.Length == 0)
		{
			return;
		}
		SessionState.EraseString(SceneKey);
		_sceneName = sceneName;
		Debug.Log("[PcTestMap] test map " + sceneName + ", fresh temporary save");
	}
#endif

	/// <summary>
	/// Save folder for StorageManager: normalSaveDir, or its TestMap subfolder while a test map runs.
	/// The subfolder is deleted on first use in a run, so every launch starts from a new save.
	/// </summary>
	public static string GetSaveDir(string normalSaveDir)
	{
		if (!IsActive)
		{
			return normalSaveDir;
		}
		string testSaveDir = Path.Combine(normalSaveDir, SaveDirName);
		if (!_saveWiped)
		{
			_saveWiped = true;
			if (Directory.Exists(testSaveDir))
			{
				Directory.Delete(testSaveDir, true);
			}
		}
		return testSaveDir;
	}

	/// <summary>
	/// Called by Boot.Done in place of the tutorial / city map. Returns false when no test map was requested.
	/// </summary>
	public static bool TryStart()
	{
		if (!IsActive)
		{
			return false;
		}
		Singleton.Tutorial.Finish();
		Debug.Log("[PcTestMap] tutorial skipped, loading " + _sceneName);
		Singleton.Game.GoToGameLevel(_sceneName, TestMissionType, false);
		return true;
	}
}
