// PC port: F1 on the city map starts a trial of The Pit, an unused level of the original
// (mission THEPIT: KillAll, Hard, MinLevel 99 above the level cap of 30, so the game never offers it).
//   - Starts from the current save: the state is saved once, then copied in memory (PlayerData.PcTakeSnapshot).
//   - Never saves: PlayerData.Save writes nothing while the trial runs, so quitting the game mid-trial is safe too.
//   - Leaving the level any way (mission won, failed, quit from the pause menu) puts the copy back before the next
//     scene loads (Loader.LoadLevel -> OnLoadLevel): XP, money, ammo, items, statistics and play time are as before.
//     Restarting The Pit from the pause menu stays in the trial.
//   - Only the trimmed scene of The Pit shipped; ResolveScene (called by Loading) falls back to it.
// Settings (volume, sensitivity, ...) are not part of the save and are still saved as usual.
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class PcPitTrial : MonoBehaviour
{
	/// <summary>Key that starts the trial on the city map.</summary>
	private const KeyCode TrialKey = KeyCode.F1;

	/// <summary>GameData.Missions entry of The Pit.</summary>
	private const string MissionDescriptor = "THEPIT";

	/// <summary>Made-up map location holding the trial's mission entry (only exists during the trial).</summary>
	private const string TrialLocation = "PcPitTrial";

	/// <summary>Mission seed: picks the difficulty among the mission's allowed ones (THEPIT only allows Hard).</summary>
	private const int GenerationSeed = 0;

	private const string CityMapSceneName = "CityMap";

	/// <summary>Suffix of the low-detail scenes (same as SceneProcessor.trimPostfix).</summary>
	private const string TrimmedSuffix = "_trimmed";

	/// <summary>Player state from before the trial; null when no trial runs.</summary>
	private static PlayerData.PcSnapshot _snapshot;

	/// <summary>Scene name of the trial level (GameData name, without _trimmed).</summary>
	private static string _trialScene;

	public static bool IsActive
	{
		get
		{
			return _snapshot != null;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		GameObject holder = new GameObject("PcPitTrial");
		DontDestroyOnLoad(holder);
		holder.AddComponent<PcPitTrial>();
		Debug.Log("[PcPitTrial] press " + TrialKey + " on the city map to play The Pit (no saving)");
	}

	private void Update()
	{
		if (Input.GetKeyDown(TrialKey))
		{
			TryStart(TrialKey.ToString());
		}
	}

	/// <summary>
	/// Starts the trial. Every call logs the trigger, then "start succeeded" or "start failed: reason".
	/// trigger: what asked for it, for the log (the key name, or a test argument).
	/// </summary>
	public static bool TryStart(string trigger)
	{
		Debug.Log("[PcPitTrial] " + trigger + " pressed");
		string blockReason = GetBlockReason();
		if (blockReason != null)
		{
			Debug.Log("[PcPitTrial] start failed: " + blockReason);
			return false;
		}
		int missionIndex = FindMissionIndex();
		GameData.MissionData mission = Singleton.GameData.Missions[missionIndex];
		PlayerData player = PlayerData.Instance;

		_snapshot = player.PcTakeSnapshot();
		_trialScene = mission.Scene;
		Debug.Log("[PcPitTrial] mission " + missionIndex + " (" + mission.Descriptor + ") scene " + mission.Scene + ", " + Describe(player));
		Debug.Log("[PcPitTrial] save file before: " + HashSaveFile());

		// The mission entry makes the level load with THEPIT's difficulty and config, as if picked on the map
		player.AddMission(TrialLocation, missionIndex, GenerationSeed);
		player.CurrentLocation = TrialLocation;
		Singleton.Game.GoToGameLevel(mission, true);

		// Loader.LoadLevel silently drops the request while the screen fades; then nothing loads, so undo the trial
		if (Loader.State != Loader.LoaderState.Loading)
		{
			player.PcRestoreSnapshot(_snapshot);
			_snapshot = null;
			_trialScene = null;
			Debug.Log("[PcPitTrial] start failed: the game did not accept the level load, player state put back, press again");
			return false;
		}
		Debug.Log("[PcPitTrial] start succeeded: loading The Pit, nothing is saved until you leave it");
		return true;
	}

	/// <summary>Why the trial cannot start right now, or null when it can.</summary>
	private static string GetBlockReason()
	{
		if (IsActive)
		{
			return "a trial is already running";
		}
		string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		if (activeScene != CityMapSceneName)
		{
			return "only works on the city map (now " + activeScene + ")";
		}
		if (Loader.State == Loader.LoaderState.Loading)
		{
			return "a scene is loading";
		}
		if (Singleton.UI.Fader.IsFading())
		{
			return "the screen is still fading, try again in a moment";
		}
		return null;
	}

	/// <summary>Loader.LoadLevel hook: leaving the trial level for any other scene restores the player state.</summary>
	public static void OnLoadLevel(string sceneName)
	{
		if (!IsActive)
		{
			return;
		}
		if (sceneName == _trialScene)
		{
			Debug.Log("[PcPitTrial] loading " + sceneName + " (trial goes on)");
			return;
		}
		PlayerData player = PlayerData.Instance;
		Debug.Log("[PcPitTrial] leaving for " + sceneName + ", state in the trial: " + Describe(player));
		player.PcRestoreSnapshot(_snapshot);
		_snapshot = null;
		_trialScene = null;
		Debug.Log("[PcPitTrial] restored: " + Describe(player));
		Debug.Log("[PcPitTrial] save file after: " + HashSaveFile());
	}

	/// <summary>
	/// Loading hook: when sceneName is not in the build but its _trimmed scene is, returns the _trimmed one.
	/// Only The Pit needs it (its full scene did not ship); every other level is returned unchanged.
	/// </summary>
	public static string ResolveScene(string sceneName)
	{
		if (Application.CanStreamedLevelBeLoaded(sceneName))
		{
			return sceneName;
		}
		string trimmedScene = sceneName + TrimmedSuffix;
		if (!Application.CanStreamedLevelBeLoaded(trimmedScene))
		{
			return sceneName;
		}
		Debug.Log("[PcPitTrial] scene " + sceneName + " not in the build, loading " + trimmedScene);
		return trimmedScene;
	}

	private static int FindMissionIndex()
	{
		GameData.MissionData[] missions = Singleton.GameData.Missions;
		for (int i = 0; i < missions.Length; i++)
		{
			if (missions[i].Descriptor == MissionDescriptor)
			{
				return i;
			}
		}
		throw new System.Exception("mission " + MissionDescriptor + " not found in GameData");
	}

	/// <summary>Short summary of the player state for the log.</summary>
	private static string Describe(PlayerData player)
	{
		return "level " + player.Level + ", XP " + player.XP + ", soft " + player.SoftCurrency + ", hard " + player.HardCurrency
			+ ", missions completed " + player.PlayerStatistics.NumMissionsCompleted + ", location '" + player.CurrentLocation + "'";
	}

	/// <summary>SHA-256 of the save file, to show in the log that the trial wrote nothing.</summary>
	private static string HashSaveFile()
	{
		string path = Path.Combine(StorageManager.PersistentDataPath, Const.SaveFileName);
		using (SHA256 sha = SHA256.Create())
		{
			byte[] hash = sha.ComputeHash(File.ReadAllBytes(path));
			return System.BitConverter.ToString(hash).Replace("-", string.Empty) + " (" + File.GetLastWriteTime(path).ToString("HH:mm:ss.fff") + ")";
		}
	}
}
