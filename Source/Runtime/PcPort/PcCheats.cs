// PC port: developer cheat codes, typed on the keyboard anywhere in the game (no text field needed).
//   chxp - level up once: XP is set to exactly the next level's XP through the normal XP setter (level-up reward,
//          LevelUpState = 2). On the city map the level-up popup opens right away (as MapWindow does); elsewhere it
//          opens the next time the city map loads.
// Note: P is also the pick-up key, so typing the code inside a level may pick up a nearby item.
using UnityEngine;

public class PcCheats : MonoBehaviour
{
	/// <summary>Code for one level up.</summary>
	private const string LevelUpCode = "chxp";

	/// <summary>Scene where MapWindow shows the level-up popup.</summary>
	private const string CityMapSceneName = "CityMap";

	/// <summary>Last typed characters (lower case), never longer than the longest code.</summary>
	private string _typed = string.Empty;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		GameObject holder = new GameObject("PcCheats");
		DontDestroyOnLoad(holder);
		holder.AddComponent<PcCheats>();
		Debug.Log("[PcCheats] type '" + LevelUpCode + "' to level up");
	}

	private void Update()
	{
		string input = Input.inputString;
		if (input.Length == 0)
		{
			return;
		}
		_typed = _typed + input.ToLowerInvariant();
		if (_typed.Length > LevelUpCode.Length)
		{
			_typed = _typed.Substring(_typed.Length - LevelUpCode.Length);
		}
		if (_typed != LevelUpCode)
		{
			return;
		}
		_typed = string.Empty;
		// The player data only exists once the game has booted
		if (PlayerData.Instance == null)
		{
			Debug.Log("[PcCheats] chxp ignored: game not started yet");
			return;
		}
		LevelUp();
	}

	private static void LevelUp()
	{
		PlayerData player = PlayerData.Instance;
		int levelBefore = player.Level;
		if (levelBefore >= Singleton.GameData.MaxPlayerLevel)
		{
			Debug.Log("[PcCheats] chxp: already at max level " + levelBefore);
			return;
		}
		player.XP = player.NextLevelXP;
		Debug.Log("[PcCheats] chxp: level " + levelBefore + " -> " + player.Level + ", XP " + player.XP);
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CityMapSceneName && player.LevelUpState == 2)
		{
			// Same as MapWindow.ShowInitialPopups
			player.LevelUpState = 1;
			Singleton.UI.PopupManager.ShowLevelUpPopup();
		}
	}
}
