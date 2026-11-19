// Pilot diagnostics (temporary): after launch, prints the state of each stage of the dynamic content pipeline every 2 seconds for 30 seconds.
// Glu's own logging is disabled in release builds; this shows where startup gets stuck.
// Also takes automatic screenshots: launch arguments -pcShots 20,45,70 -pcShotDir <dir> capture the game's own frame at those times
// (not the screen, so it works even when the window is covered and never captures other programs).
// Everything uses real time: the game sets timeScale to 0 while loading.
using System.Collections;
using System.Reflection;
using System.Text;
using Glu.AssetBundles;
using Glu.DynamicContentPipeline;
using UnityEngine;

public class PcBootDiagnostics : MonoBehaviour
{
	private const float IntervalSeconds = 2f;

	private const int Rounds = 15;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		GameObject holder = new GameObject("PcBootDiagnostics");
		DontDestroyOnLoad(holder);
		PcBootDiagnostics diagnostics = holder.AddComponent<PcBootDiagnostics>();
		diagnostics.StartCoroutine(diagnostics.CaptureShots());
		diagnostics.StartCoroutine(diagnostics.AutoTaps());
		diagnostics.StartCoroutine(diagnostics.FinishTutorial());
		diagnostics.StartCoroutine(diagnostics.ForcePostEffects());
		diagnostics.StartCoroutine(diagnostics.StartMission());
		diagnostics.StartCoroutine(diagnostics.EmitTestEffects());
		diagnostics.StartCoroutine(diagnostics.CheckNavigation());
		diagnostics.StartCoroutine(DumpShop());
		diagnostics.StartCoroutine(CheckAudio());
		diagnostics.StartCoroutine(CompleteMission());
		diagnostics.StartCoroutine(AddMoney());
		diagnostics.StartCoroutine(ReportPerformance());
		diagnostics.StartCoroutine(CheckPlayerPath());
		diagnostics.StartCoroutine(WatchUnits());
		diagnostics.StartCoroutine(AutoAim());
		diagnostics.StartCoroutine(WatchCameras());
		diagnostics.StartCoroutine(AutoMove());
		diagnostics.StartCoroutine(TaskTaps());
		diagnostics.StartCoroutine(TapActivateItems());
		diagnostics.StartCoroutine(WatchSurvivalTutorialStart());
		CheckSaveFile();
	}

	/// <summary>
	/// Test: -pcSaveCheck <save file path> deserializes that file as PlayerData.Data right at launch
	/// and prints the full exception chain. PlayerData.Load swallows exceptions and starts a new game; use this to find out why a save does not load.
	/// </summary>
	private static void CheckSaveFile()
	{
		string path = GetArgument("-pcSaveCheck");
		if (path == null)
		{
			return;
		}
		try
		{
			object data = StorageManager.ReadXmlFromLocation(path, typeof(PlayerData.Data));
			Debug.Log("[PcDiag] save check ok: " + path + " -> " + data);
		}
		catch (System.Exception e)
		{
			Debug.Log("[PcDiag] save check failed: " + path + "\n" + e);
		}
		// Round-trip test: XmlDictionary<KeyValuePair<string, string>, int> with two keys (as PlayerStatistics uses it)
		XmlDictionary<System.Collections.Generic.KeyValuePair<string, string>, int> original = new XmlDictionary<System.Collections.Generic.KeyValuePair<string, string>, int>();
		original.Add(new System.Collections.Generic.KeyValuePair<string, string>("Zombie", "Pistols0"), 23);
		original.Add(new System.Collections.Generic.KeyValuePair<string, string>("Zombie", "Melee0"), 4);
		System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(original.GetType());
		System.IO.StringWriter text = new System.IO.StringWriter();
		serializer.Serialize(text, original);
		XmlDictionary<System.Collections.Generic.KeyValuePair<string, string>, int> restored =
			(XmlDictionary<System.Collections.Generic.KeyValuePair<string, string>, int>)serializer.Deserialize(new System.IO.StringReader(text.ToString()));
		StringBuilder keys = new StringBuilder();
		foreach (System.Collections.Generic.KeyValuePair<System.Collections.Generic.KeyValuePair<string, string>, int> item in restored)
		{
			keys.Append(" " + item.Key.Key + "/" + item.Key.Value + "=" + item.Value);
		}
		Debug.Log("[PcDiag] save check pair-key roundtrip count=" + restored.Count + keys);
	}

	/// <summary>
	/// Test: -pcTaskTap "melee=0.746:0.09|melee=0.5:0.3" taps once every 2.5 s (real time) while the current task name contains the text before "=";
	/// several entries matching the same task are tapped in turn. No fixed times, so it suits tutorials whose pace varies. The text cannot contain spaces (launch arguments are split on spaces).
	/// </summary>
	private static IEnumerator TaskTaps()
	{
		string argument = GetArgument("-pcTaskTap");
		if (argument == null)
		{
			yield break;
		}
		string[] entries = argument.Split('|');
		int turn = 0;
		while (true)
		{
			yield return new WaitForSecondsRealtime(2.5f);
			Mission mission = Object.FindAnyObjectByType<Mission>();
			if (mission == null || mission.GetCurrentTask() == null)
			{
				continue;
			}
			string taskName = mission.GetCurrentTask().name;
			System.Collections.Generic.List<string> matched = new System.Collections.Generic.List<string>();
			foreach (string entry in entries)
			{
				string[] parts = entry.Split('=');
				if (taskName.Contains(parts[0]))
				{
					matched.Add(parts[1]);
				}
			}
			if (matched.Count == 0)
			{
				continue;
			}
			string point = matched[turn % matched.Count];
			turn++;
			Vector2 ratio = ParsePoint(point);
			Vector2 pixels = new Vector2(ratio.x * Screen.width, ratio.y * Screen.height);
			Debug.Log("[PcDiag] task tap t=" + Time.realtimeSinceStartup.ToString("F1") + " task=" + taskName + " at " + point);
			PcInput.InjectTap(pixels);
		}
	}

	/// <summary>
	/// Test: -pcAutoMove 90:400 from 90 s to 400 s (real time) holds the left stick with finger 3 (PcInput.MoveFinger)
	/// towards Player.GetCurrentMissionTarget() (the point the HUD mission arrow points at, following the navigation path); releases within 1 m of the target or when there is none.
	/// The stick direction inverts the conversion in Player.UpdateMovement: stick y is camera forward, x is camera right.
	/// </summary>
	private static IEnumerator AutoMove()
	{
		string argument = GetArgument("-pcAutoMove");
		if (argument == null)
		{
			yield break;
		}
		string[] parts = argument.Split(':');
		float start = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
		float end = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < start)
		{
			yield return null;
		}
		Debug.Log("[PcDiag] auto move start t=" + start);
		PcInput.SteeredFinger finger = PcInput.MoveFinger;
		Vector2 pressPoint = Vector2.zero;
		float nextLog = 0f;
		while (Time.realtimeSinceStartup < end)
		{
			yield return null;
			Player player = Object.FindAnyObjectByType<Player>();
			Transform target = null;
			if (player != null && Object.FindAnyObjectByType<Mission>() != null)
			{
				target = player.GetCurrentMissionTarget();
			}
			if (target == null || Scene.MainCamera == null)
			{
				finger.Active = false;
				continue;
			}
			Vector3 toTarget = target.position - player.transform.position;
			toTarget.y = 0f;
			if (toTarget.magnitude < 1f)
			{
				finger.Active = false;
				continue;
			}
			// Follow the navmesh path: head for the first corner more than 0.5 m from the player, so a straight line does not get stuck on a pillar corner
			UnityEngine.AI.NavMeshPath navPath = new UnityEngine.AI.NavMeshPath();
			if (UnityEngine.AI.NavMesh.CalculatePath(player.transform.position, target.position, -1, navPath))
			{
				foreach (Vector3 corner in navPath.corners)
				{
					Vector3 toCorner = corner - player.transform.position;
					toCorner.y = 0f;
					if (toCorner.magnitude > 0.5f)
					{
						toTarget = toCorner;
						break;
					}
				}
			}
			Transform cameraTransform = Scene.MainCamera.transform;
			Quaternion offset = Quaternion.Euler(0f, Scene.Camera.angleYOffset, 0f);
			Vector3 forward = offset * cameraTransform.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 right = offset * cameraTransform.right;
			right.y = 0f;
			right.Normalize();
			Vector3 direction = toTarget.normalized;
			Vector2 stick = new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward));
			if (!finger.Active)
			{
				pressPoint = new Vector2(Screen.width * 0.1f, Screen.height * 0.26f);
				finger.Position = pressPoint;
				finger.Active = true;
				continue;
			}
			finger.Position = pressPoint + stick.normalized * 60f;
			if (Time.realtimeSinceStartup > nextLog)
			{
				nextLog = Time.realtimeSinceStartup + 2f;
				Debug.Log("[PcDiag] auto move t=" + Time.realtimeSinceStartup.ToString("F1") + " player=" + player.transform.position.ToString("F2")
					+ " target=" + target.name + "@" + target.position.ToString("F2") + " stick=" + stick.ToString("F2"));
			}
		}
		finger.Active = false;
		Debug.Log("[PcDiag] auto move end t=" + end);
	}

	/// <summary>
	/// With -pcCameraWatch: on the frame RunSurvivalMode appears and a few frames after, logs the map points found under Singleton.UI.MapWindow
	/// (active only / including inactive / with a mission), to find out why the survival mode tutorial hint does not show.
	/// </summary>
	private static IEnumerator WatchSurvivalTutorialStart()
	{
		if (GetArgument("-pcCameraWatch") == null)
		{
			yield break;
		}
		int reports = 0;
		while (reports < 4)
		{
			yield return null;
			if (!Singleton.Exists() || !(bool)Singleton.Tutorial || Singleton.Tutorial.GetComponent<RunSurvivalMode>() == null)
			{
				continue;
			}
			MapWindow window = Singleton.UI.MapWindow;
			string text = "window=null";
			if (window != null)
			{
				text = "";
				MapLocation[] active = window.GetComponentsInChildren<MapLocation>();
				MapLocation[] all = window.GetComponentsInChildren<MapLocation>(true);
				int withMission = 0;
				string types = "";
				foreach (MapLocation location in active)
				{
					if (location.MissionData != null)
					{
						withMission++;
						types += " " + location.name + ":" + location.MissionData.Type + ":" + location.MissionData.Scene;
					}
				}
				text += " numMissionsCompleted=" + Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted + types;
				text = text + " window#" + window.GetInstanceID() + " activeInHierarchy=" + window.gameObject.activeInHierarchy + " locations=" + active.Length + "/" + all.Length + " withMission=" + withMission;
			}
			Debug.Log("[PcDiag] survival tutorial frame=" + Time.frameCount + " t=" + Time.realtimeSinceStartup.ToString("F2") + " loaderState=" + Loader.State + " " + text);
			reports++;
			if (reports > 1)
			{
				yield return new WaitForSecondsRealtime(1f);
			}
		}
	}

	/// <summary>
	/// Test: -pcCameraWatch 2 lists every enabled camera (depth, clear flags, render target, position) every 2 s (real time)
	/// plus the PostEffectManager state, for problems such as the screen turning white after a cutscene.
	/// </summary>
	private static IEnumerator WatchCameras()
	{
		string argument = GetArgument("-pcCameraWatch");
		if (argument == null)
		{
			yield break;
		}
		float interval = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (true)
		{
			yield return new WaitForSecondsRealtime(interval);
			StringBuilder report = new StringBuilder("[PcDiag] cameras t=" + Time.realtimeSinceStartup.ToString("F1") + " timeScale=" + Time.timeScale);
			// Only levels have missions; Scene.Mission throws a null reference outside a level, so check for a Mission object first
			Mission mission = Object.FindAnyObjectByType<Mission>();
			if (mission != null && mission.GetCurrentTask() != null)
			{
				report.Append(" task=" + mission.GetCurrentTask().name + "/" + mission.GetCurrentTask().type);
			}
			SpriteFader fader = Object.FindAnyObjectByType<SpriteFader>(FindObjectsInactive.Include);
			if (fader != null)
			{
				report.Append(" fader active=" + fader.gameObject.activeInHierarchy + " fading=" + fader.IsFading() + " in=" + fader.IsFadingIn());
			}
			// Tutorial state and the tutorial scripts currently attached (for when the tutorial hint on the map does not show)
			if (Singleton.Exists() && (bool)Singleton.Tutorial)
			{
				report.Append(" tutorialState=" + Singleton.PlayerData.TutorialState);
				foreach (TutorialScript script in Singleton.Tutorial.GetComponents<TutorialScript>())
				{
					report.Append(" [" + script.GetType().Name + "]");
					FieldInfo blinkerField = script.GetType().GetField("_missionBlinker", BindingFlags.Instance | BindingFlags.NonPublic);
					if (blinkerField != null)
					{
						report.Append(" missionBlinker=" + ((Object)blinkerField.GetValue(script) != null));
					}
				}
				int locationsWithMission = 0;
				int locationsActiveSelf = 0;
				int locationsActiveInHierarchy = 0;
				foreach (MapLocation location in Object.FindObjectsByType<MapLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
				{
					if (location.MissionData != null)
					{
						locationsWithMission++;
					}
					if (location.gameObject.activeSelf)
					{
						locationsActiveSelf++;
					}
					if (location.gameObject.activeInHierarchy)
					{
						locationsActiveInHierarchy++;
					}
				}
				report.Append(" mapLocationsWithMission=" + locationsWithMission + " activeSelf=" + locationsActiveSelf + " activeInHierarchy=" + locationsActiveInHierarchy);
				if (Singleton.UI.MapWindow != null)
				{
					report.Append(" mapWindow activeSelf=" + Singleton.UI.MapWindow.gameObject.activeSelf + " inHierarchy=" + Singleton.UI.MapWindow.gameObject.activeInHierarchy);
				}
			}
			// ActivateItems within 30 m (supply points, terminals, etc.): player distance to the trigger sphere, whether it counts as inside, whether the icon button shows
			Player watchPlayer = Object.FindAnyObjectByType<Player>();
			if (watchPlayer != null)
			{
				FieldInfo enteredField = typeof(ActivateItem).GetField("_entered", BindingFlags.Instance | BindingFlags.NonPublic);
				foreach (ActivateItem item in Object.FindObjectsByType<ActivateItem>(FindObjectsSortMode.None))
				{
					float distance = Vector3.Distance(item.transform.position, watchPlayer.transform.position);
					if (distance > 30f)
					{
						continue;
					}
					string areaText = "noArea";
					Transform area = item.transform.Find("TriggeringArea");
					if (area != null)
					{
						SphereCollider sphere = area.GetComponent<SphereCollider>();
						Vector3 center = area.TransformPoint(sphere.center);
						float radius = sphere.radius * Mathf.Max(area.lossyScale.x, Mathf.Max(area.lossyScale.y, area.lossyScale.z));
						areaText = "areaDist=" + Vector3.Distance(center, watchPlayer.transform.position).ToString("F2") + " radius=" + radius.ToString("F2")
							+ " areaActive=" + area.gameObject.activeInHierarchy + " trigger=" + sphere.isTrigger + " layer=" + area.gameObject.layer;
					}
					Transform button = item.transform.Find("ButtonCenter/Button");
					string buttonText = "noButton";
					if (button != null)
					{
						buttonText = "buttonActive=" + button.gameObject.activeInHierarchy + " buttonPos=" + button.position.ToString("F2");
					}
					report.Append("\n  item " + item.name + " dist=" + distance.ToString("F2") + " entered=" + enteredField.GetValue(item) + " " + areaText + " " + buttonText);
				}
			}
			foreach (Camera camera in Camera.allCameras)
			{
				string target = "screen";
				if (camera.targetTexture != null)
				{
					target = camera.targetTexture.name + " " + camera.targetTexture.width + "x" + camera.targetTexture.height;
				}
				report.Append("\n  cam " + camera.name + " depth=" + camera.depth + " clear=" + camera.clearFlags + " bg=" + camera.backgroundColor
					+ " mask=0x" + camera.cullingMask.ToString("X") + " target=" + target + " pos=" + camera.transform.position.ToString("F2")
					+ " near=" + camera.nearClipPlane + " far=" + camera.farClipPlane + " rect=" + camera.rect);
				foreach (MonoBehaviour behaviour in camera.GetComponents<MonoBehaviour>())
				{
					report.Append(" [" + behaviour.GetType().Name + " en=" + behaviour.enabled + "]");
				}
			}
			Debug.Log(report.ToString());
		}
	}

	/// <summary>
	/// Test: -pcAutoAim 45:300 from 45 s to 300 s (real time), while the scene has a living enemy in front of the camera,
	/// holds the right half of the screen with finger 2 (PcInput.AimFinger.Active): first stays still for 0.3 s (the game only fires if the touch does not drag within 0.2 s of going down),
	/// then drags the view in the direction of the target's offset from the screen center, pulling the crosshair onto the enemy nearest the center, and keeps firing.
	/// Releases and presses again when the finger leaves the right half or after 5 s. Releases when there are no enemies. Used to actually play through levels unattended.
	/// </summary>
	private static IEnumerator AutoAim()
	{
		string argument = GetArgument("-pcAutoAim");
		if (argument == null)
		{
			yield break;
		}
		string[] parts = argument.Split(':');
		float start = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
		float end = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < start)
		{
			yield return null;
		}
		Debug.Log("[PcDiag] auto aim start t=" + start);
		float pressTime = 0f;
		int releaseFrames = 0;
		while (Time.realtimeSinceStartup < end)
		{
			yield return null;
			Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
			Vector2 pressPoint = new Vector2(Screen.width * 0.75f, Screen.height * 0.4f);
			Vector2? aimOffset = FindAimOffset(screenCenter);
			if (releaseFrames > 0)
			{
				releaseFrames--;
				PcInput.AimFinger.Active = false;
				continue;
			}
			if (aimOffset == null)
			{
				PcInput.AimFinger.Active = false;
				continue;
			}
			if (!PcInput.AimFinger.Active)
			{
				PcInput.AimFinger.Active = true;
				PcInput.AimFinger.Position = pressPoint;
				pressTime = Time.realtimeSinceStartup;
				continue;
			}
			float held = Time.realtimeSinceStartup - pressTime;
			if (held < 0.3f)
			{
				continue;
			}
			// Drag towards the target: move a small, clamped fraction of the offset each frame to avoid overshooting back and forth
			Vector2 step = aimOffset.Value * 0.05f;
			step = Vector2.ClampMagnitude(step, 12f);
			PcInput.AimFinger.Position += step;
			Vector2 position = PcInput.AimFinger.Position;
			if (Time.frameCount % 60 == 0)
			{
				Debug.Log("[PcDiag] auto aim t=" + Time.realtimeSinceStartup.ToString("F1") + " offset=" + aimOffset.Value.ToString("F0") + " finger=" + position.ToString("F0")
					+ " ammo=" + Scene.Player.Inventory.Weapon.GetAmmoInClip());
			}
			bool outside = position.x < Screen.width * 0.55f || position.x > Screen.width * 0.98f || position.y < Screen.height * 0.1f || position.y > Screen.height * 0.85f;
			if (outside || held > 5f)
			{
				releaseFrames = 2;
				PcInput.AimFinger.Active = false;
			}
		}
		PcInput.AimFinger.Active = false;
		Debug.Log("[PcDiag] auto aim end t=" + end);
	}

	/// <summary>
	/// Picks an enemy to aim at and returns the screen offset of its center from the screen center (pixels); null if there is none.
	/// Prefers enemies that are not occluded (the line from the camera to their center hits them first), then the ones closest to the player; occluded ones are picked only when no enemy is visible.
	/// </summary>
	private static Vector2? FindAimOffset(Vector2 screenCenter)
	{
		Camera camera = Camera.main;
		if (camera == null)
		{
			return null;
		}
		Vector3 playerPosition = camera.transform.position;
		Player player = Object.FindAnyObjectByType<Player>();
		if (player != null)
		{
			playerPosition = player.transform.position;
		}
		Vector2? best = null;
		float bestScore = float.MaxValue;
		foreach (Unit unit in Object.FindObjectsByType<Unit>(FindObjectsSortMode.None))
		{
			if (unit.IsDead() || !unit.gameObject.activeInHierarchy)
			{
				continue;
			}
			Vector3 center = unit.Center();
			Vector3 screen = camera.WorldToScreenPoint(center);
			if (screen.z <= 0f)
			{
				continue;
			}
			float score = Vector3.Distance(center, playerPosition);
			RaycastHit hit;
			bool blocked = Physics.Linecast(camera.transform.position, center, out hit, ~(1 << 8), QueryTriggerInteraction.Ignore)
				&& !hit.collider.transform.IsChildOf(unit.transform);
			if (blocked)
			{
				score += 1000f;
			}
			if (score < bestScore)
			{
				bestScore = score;
				best = new Vector2(screen.x, screen.y) - screenCenter;
			}
		}
		if (best == null)
		{
			// With no enemies, turn the camera towards the icon of an ActivateItem whose trigger the player is in (supply points, etc.), so TapActivateItems can tap it
			Transform button = FindEnteredActivateButton();
			if (button != null)
			{
				Vector3 screen = camera.WorldToScreenPoint(button.position);
				if (screen.z > 0f)
				{
					best = new Vector2(screen.x, screen.y) - screenCenter;
				}
				else
				{
					// Behind: turn right
					best = new Vector2(Screen.width, 0f);
				}
			}
		}
		return best;
	}

	/// <summary>The button of an ActivateItem whose trigger the player is in and whose icon button is showing; null if there is none.</summary>
	private static Transform FindEnteredActivateButton()
	{
		FieldInfo enteredField = typeof(ActivateItem).GetField("_entered", BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (ActivateItem item in Object.FindObjectsByType<ActivateItem>(FindObjectsSortMode.None))
		{
			if (!(bool)enteredField.GetValue(item))
			{
				continue;
			}
			Transform button = item.transform.Find("ButtonCenter/Button");
			if (button != null && button.gameObject.activeInHierarchy)
			{
				return button;
			}
		}
		return null;
	}

	/// <summary>
	/// During -pcAutoAim, every 2.5 s: if the player is inside an ActivateItem's trigger and its icon is on screen, tap the icon (refill ammo, start a terminal, etc.).
	/// </summary>
	private static IEnumerator TapActivateItems()
	{
		string argument = GetArgument("-pcAutoAim");
		if (argument == null)
		{
			yield break;
		}
		string[] parts = argument.Split(':');
		float start = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
		float end = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < end)
		{
			yield return new WaitForSecondsRealtime(2.5f);
			if (Time.realtimeSinceStartup < start || Camera.main == null)
			{
				continue;
			}
			Transform button = FindEnteredActivateButton();
			if (button == null)
			{
				continue;
			}
			Vector3 viewport = Camera.main.WorldToViewportPoint(button.position);
			bool visible = viewport.z > 0f && viewport.x > 0.05f && viewport.x < 0.95f && viewport.y > 0.05f && viewport.y < 0.95f;
			if (!visible)
			{
				continue;
			}
			Vector2 pixels = new Vector2(viewport.x * Screen.width, viewport.y * Screen.height);
			Debug.Log("[PcDiag] tap activate item t=" + Time.realtimeSinceStartup.ToString("F1") + " at " + pixels.ToString("F0"));
			PcInput.InjectTap(pixels);
		}
	}

	/// <summary>
	/// Test: -pcZombieWatch 2 prints, every 2 s (real time), each Unit's (zombies, etc.) state machine state and position,
	/// its NavMeshAgent state (on the mesh, destination, path status, speed) and the current task, to debug "zombies standing still".
	/// </summary>
	private static IEnumerator WatchUnits()
	{
		string argument = GetArgument("-pcZombieWatch");
		if (argument == null)
		{
			yield break;
		}
		float interval = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		FieldInfo stateField = typeof(Character).GetField("state", flags);
		FieldInfo targetField = typeof(Character).GetField("targetToAttack", flags);
		FieldInfo speedField = typeof(Character).GetField("speed", flags);
		// Check the state machine every frame and log each state change (with the target's position and distance), to see which state falls back to Staying
		System.Collections.Generic.Dictionary<int, string> lastStates = new System.Collections.Generic.Dictionary<int, string>();
		float nextReport = Time.realtimeSinceStartup + interval;
		while (true)
		{
			yield return null;
			foreach (Character character in Object.FindObjectsByType<Character>(FindObjectsSortMode.None))
			{
				string current = stateField.GetValue(character).ToString();
				string previous;
				bool known = lastStates.TryGetValue(character.GetInstanceID(), out previous);
				if (known && previous == current)
				{
					continue;
				}
				lastStates[character.GetInstanceID()] = current;
				object stateTarget = targetField.GetValue(character);
				string targetText = "null";
				if (stateTarget != null && (GameObject)stateTarget != null)
				{
					GameObject targetObject = (GameObject)stateTarget;
					targetText = targetObject.name + "@" + targetObject.transform.position.ToString("F2") + " dist="
						+ Vector3.Distance(targetObject.transform.position, character.transform.position).ToString("F2");
				}
				Debug.Log("[PcDiag] state t=" + Time.realtimeSinceStartup.ToString("F2") + " " + character.GetType().Name + "#" + character.GetInstanceID()
					+ " " + previous + " -> " + current + " pos=" + character.transform.position.ToString("F2") + " target=" + targetText);
			}
			if (Time.realtimeSinceStartup < nextReport)
			{
				continue;
			}
			nextReport = Time.realtimeSinceStartup + interval;
			Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
			// Scene.Mission throws a null reference while the level is not loaded yet; skip the report then
			if (units.Length == 0)
			{
				continue;
			}
			string task = "-";
			if (Scene.Mission != null && Scene.Mission.GetCurrentTask() != null)
			{
				task = Scene.Mission.GetCurrentTask().name + "/" + Scene.Mission.GetCurrentTask().type;
			}
			Debug.Log("[PcDiag] units t=" + Time.realtimeSinceStartup.ToString("F1") + " timeScale=" + Time.timeScale + " count=" + units.Length + " task=" + task);
			foreach (Unit unit in units)
			{
				UnityEngine.AI.NavMeshAgent agent = unit.GetComponent<UnityEngine.AI.NavMeshAgent>();
				object target = targetField.GetValue(unit);
				string targetName = "null";
				if (target != null && (GameObject)target != null)
				{
					targetName = ((GameObject)target).name;
				}
				string agentText = "noAgent";
				if (agent != null)
				{
					agentText = "enabled=" + agent.enabled + " onMesh=" + agent.isOnNavMesh;
					if (agent.isOnNavMesh)
					{
						agentText += " stopped=" + agent.isStopped + " hasPath=" + agent.hasPath + " pending=" + agent.pathPending + " status=" + agent.pathStatus
							+ " dest=" + agent.destination.ToString("F2") + " steer=" + agent.steeringTarget.ToString("F2") + " remain=" + agent.remainingDistance.ToString("F2");
					}
					agentText += " vel=" + agent.velocity.magnitude.ToString("F2") + " speed=" + agent.speed.ToString("F2") + " radius=" + agent.radius.ToString("F2")
						+ " updPos=" + agent.updatePosition;
				}
				Debug.Log("[PcDiag]   unit " + unit.name + "#" + unit.GetInstanceID() + " active=" + unit.gameObject.activeInHierarchy + " state=" + stateField.GetValue(unit)
					+ " pos=" + unit.transform.position.ToString("F2") + " charSpeed=" + ((float)speedField.GetValue(unit)).ToString("F2")
					+ " target=" + targetName + " obstacled=" + unit.Obstacled + " " + agentText);
			}
			// Characters that do not use the navmesh and only stand on the ground through a CharacterController (Human, etc.): grounded or not, and which collider is underfoot
			foreach (Character character in Object.FindObjectsByType<Character>(FindObjectsSortMode.None))
			{
				if (character is Unit)
				{
					continue;
				}
				CharacterController controller = character.GetComponent<CharacterController>();
				string controllerText = "noController";
				if (controller != null)
				{
					controllerText = "ccEnabled=" + controller.enabled + " grounded=" + controller.isGrounded + " ccBounds=" + controller.bounds.ToString("F2");
				}
				StringBuilder below = new StringBuilder();
				RaycastHit[] hits = Physics.RaycastAll(character.transform.position + Vector3.up * 2f, Vector3.down, 10f);
				foreach (RaycastHit hit in hits)
				{
					below.Append(" [" + hit.collider.name + " L" + hit.collider.gameObject.layer + " " + hit.collider.GetType().Name + " y=" + hit.point.y.ToString("F2")
						+ " trig=" + hit.collider.isTrigger + "]");
				}
				if (controller != null)
				{
					below.Append(" overlaps:");
					Vector3 center = controller.bounds.center;
					foreach (Collider overlap in Physics.OverlapSphere(center, controller.radius + 0.3f))
					{
						below.Append(" [" + overlap.name + " L" + overlap.gameObject.layer + " " + overlap.GetType().Name + " trig=" + overlap.isTrigger
							+ " b=" + overlap.bounds.min.ToString("F2") + ".." + overlap.bounds.max.ToString("F2") + "]");
					}
				}
				Debug.Log("[PcDiag]   char " + character.GetType().Name + "#" + character.GetInstanceID() + " " + character.name + " layer=" + character.gameObject.layer
					+ " pos=" + character.transform.position.ToString("F2") + " state=" + stateField.GetValue(character) + " " + controllerText + " below:" + below);
			}
		}
	}

	/// <summary>
	/// Test: -pcPathCheck 88 replays each segment of PlayerProgress.BuildPath (CalculatePath between adjacent path nodes) at that time,
	/// logging the status, corner count and how far the nodes are from the navmesh, to find out how off-mesh points end up on the player route.
	/// </summary>
	private static IEnumerator CheckPlayerPath()
	{
		string argument = GetArgument("-pcPathCheck");
		if (argument == null)
		{
			yield break;
		}
		float target = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		PlayerProgress progress = Scene.PlayerProgress;
		FieldInfo pointsField = typeof(PlayerProgress).GetField("_points", BindingFlags.Instance | BindingFlags.NonPublic);
		System.Collections.Generic.List<GameObject> points = (System.Collections.Generic.List<GameObject>)pointsField.GetValue(progress);
		UnityEngine.AI.NavMeshPath navPath = new UnityEngine.AI.NavMeshPath();
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 position = points[i].transform.position;
			UnityEngine.AI.NavMeshHit hit;
			bool onMesh = UnityEngine.AI.NavMesh.SamplePosition(position, out hit, 0.5f, -1);
			string segment = "";
			if (i + 1 < points.Count)
			{
				bool found = UnityEngine.AI.NavMesh.CalculatePath(position, points[i + 1].transform.position, -1, navPath);
				segment = " -> next found=" + found + " status=" + navPath.status + " corners=" + navPath.corners.Length;
				if (navPath.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
				{
					StringBuilder corners = new StringBuilder();
					foreach (Vector3 corner in navPath.corners)
					{
						corners.Append(" " + corner.ToString("F2"));
					}
					segment += " cornersList:" + corners;
				}
			}
			Debug.Log("[PcDiag] path node " + i + " " + points[i].name + " " + position.ToString("F2") + " onMesh(0.5)=" + onMesh + segment);
		}
		System.Collections.Generic.List<Vector3> path = progress.GetPath();
		for (int i = 0; i < path.Count && i < 12; i++)
		{
			Debug.Log("[PcDiag] path point " + i + " " + path[i].ToString("F2"));
		}
	}

	/// <summary>Test: -pcPerf 30 logs the average frame rate, managed heap and Unity-allocated memory for each 30 s (real time) window, to look for leaks and stutter.</summary>
	private static IEnumerator ReportPerformance()
	{
		string argument = GetArgument("-pcPerf");
		if (argument == null)
		{
			yield break;
		}
		float interval = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (true)
		{
			int startFrame = Time.frameCount;
			float startTime = Time.realtimeSinceStartup;
			yield return new WaitForSecondsRealtime(interval);
			float elapsed = Time.realtimeSinceStartup - startTime;
			float fps = (Time.frameCount - startFrame) / elapsed;
			long managed = System.GC.GetTotalMemory(false) / (1024 * 1024);
			long allocated = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
			Debug.Log("[PcDiag] perf t=" + Time.realtimeSinceStartup.ToString("F0") + " fps=" + fps.ToString("F1") + " managedMB=" + managed
				+ " unityMB=" + allocated + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
		}
	}

	/// <summary>Test: -pcAddMoney 45:5000 adds soft currency at that time (same as the add-money button in the cheat window), to test successful purchases and charges.</summary>
	private static IEnumerator AddMoney()
	{
		string argument = GetArgument("-pcAddMoney");
		if (argument == null)
		{
			yield break;
		}
		string[] parts = argument.Split(':');
		float target = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
		int amount = int.Parse(parts[1]);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		Singleton.PlayerData.SoftCurrency += amount;
		Debug.Log("[PcDiag] add money +" + amount + " soft=" + Singleton.PlayerData.SoftCurrency);
	}

	/// <summary>Test: -pcCompleteMission 95 calls Cheat.CompleteMission from the game's cheat window at that time, to test mission results, rewards, saving and the return to the map.</summary>
	private static IEnumerator CompleteMission()
	{
		string argument = GetArgument("-pcCompleteMission");
		if (argument == null)
		{
			yield break;
		}
		float target = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		Debug.Log("[PcDiag] complete mission t=" + target + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
			+ " soft=" + Singleton.PlayerData.SoftCurrency + " xp=" + Singleton.PlayerData.XP);
		Cheat.CompleteMission();
	}

	/// <summary>Test: -pcAudioCheck 60,90 lists the AudioSources playing at those times (sound cannot be heard here, so this is the only way to confirm playback).</summary>
	private static IEnumerator CheckAudio()
	{
		string argument = GetArgument("-pcAudioCheck");
		if (argument == null)
		{
			yield break;
		}
		foreach (string item in argument.Split(','))
		{
			float target = float.Parse(item, System.Globalization.CultureInfo.InvariantCulture);
			while (Time.realtimeSinceStartup < target)
			{
				yield return null;
			}
			AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
			int playing = 0;
			StringBuilder names = new StringBuilder();
			foreach (AudioSource source in sources)
			{
				if (!source.isPlaying)
				{
					continue;
				}
				playing++;
				if (names.Length < 400)
				{
					string clip = "<null>";
					if (source.clip != null)
					{
						clip = source.clip.name;
					}
					names.Append(" " + source.name + ":" + clip + "@" + source.volume.ToString("F2"));
				}
			}
			AudioListener listener = Object.FindAnyObjectByType<AudioListener>();
			Debug.Log("[PcDiag] audio t=" + target + " sources=" + sources.Length + " playing=" + playing + " listenerVolume=" + AudioListener.volume
				+ " paused=" + AudioListener.pause + " listener=" + (listener != null) + " output=" + AudioSettings.outputSampleRate + " |" + names);
		}
	}

	/// <summary>Test: -pcShopDump 50 lists the shop config's groups, entries and readyToShow at that time (for the shop opening with no selectable group).</summary>
	private static IEnumerator DumpShop()
	{
		string argument = GetArgument("-pcShopDump");
		if (argument == null)
		{
			yield break;
		}
		float target = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		ShopConfig config = Singleton.ShopData.shopConfig;
		int rawCount = -1;
		if (config.rawGroups != null)
		{
			rawCount = config.rawGroups.Count;
			foreach (ShopConfigGroup raw in config.rawGroups)
			{
				Debug.Log("[PcDiag] shop raw group " + raw.id + " visible=" + raw.visible + " refs=" + (raw.itemRefs == null ? -1 : raw.itemRefs.Count));
			}
		}
		Debug.Log("[PcDiag] shop: items=" + config.items.Count + " groups=" + config.groups.Count + " rawGroups=" + rawCount + " playerLevel=" + Singleton.PlayerData.Level);
		foreach (ShopConfigGroup group in config.groups)
		{
			int ready = 0;
			int missing = 0;
			StringBuilder samples = new StringBuilder();
			foreach (ShopConfigGroup.Reference reference in group.itemRefs)
			{
				if (reference.item == null)
				{
					missing++;
					continue;
				}
				if (reference.item.readyToShow)
				{
					ready++;
				}
				else if (samples.Length < 300)
				{
					samples.Append(" " + reference.item.id + "(" + reference.item.GetType().Name + Describe(reference.item) + ")");
				}
			}
			Debug.Log("[PcDiag] shop group " + group.id + ": refs=" + group.itemRefs.Count + " ready=" + ready + " missingItem=" + missing + " notReady:" + samples);
		}
	}

	/// <summary>
	/// Test: -pcNavCheck 85 checks pathfinding from every spawn point of the current level to the player's path points at that time (the same call as SmartSpawnPoint.CalcPathsLength);
	/// on failure it also checks how far both ends are from the navmesh and whether a path exists after snapping them onto it, to tell "off the mesh" from "mesh not connected".
	/// </summary>
	private IEnumerator CheckNavigation()
	{
		string argument = GetArgument("-pcNavCheck");
		if (argument == null)
		{
			yield break;
		}
		float target = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		System.Collections.Generic.List<Vector3> pathPoints = Scene.PlayerProgress.GetPath();
		UnityEngine.AI.NavMeshPath navPath = new UnityEngine.AI.NavMeshPath();
		int failed = 0;
		int fixedBySnap = 0;
		int checkedCount = 0;
		foreach (SmartSpawnPoint spawn in Object.FindObjectsByType<SmartSpawnPoint>(FindObjectsSortMode.None))
		{
			Vector3 start = spawn.transform.position;
			foreach (Vector3 point in pathPoints)
			{
				if ((start - point).sqrMagnitude > 1225f)
				{
					continue;
				}
				checkedCount++;
				if (UnityEngine.AI.NavMesh.CalculatePath(start, point, -1, navPath))
				{
					continue;
				}
				failed++;
				UnityEngine.AI.NavMeshHit startHit;
				UnityEngine.AI.NavMeshHit endHit;
				bool startFound = UnityEngine.AI.NavMesh.SamplePosition(start, out startHit, 5f, -1);
				bool endFound = UnityEngine.AI.NavMesh.SamplePosition(point, out endHit, 5f, -1);
				bool snapped = startFound && endFound && UnityEngine.AI.NavMesh.CalculatePath(startHit.position, endHit.position, -1, navPath)
					&& navPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
				if (snapped)
				{
					fixedBySnap++;
				}
				if (failed <= 20)
				{
					Debug.Log("[PcDiag] nav fail spawn=" + spawn.name + " " + start.ToString("F2") + " startOff=" + (startFound ? startHit.distance.ToString("F2") : "none")
						+ " point=" + point.ToString("F2") + " endOff=" + (endFound ? endHit.distance.ToString("F2") : "none")
						+ " snappedPath=" + snapped + " status=" + navPath.status);
				}
			}
		}
		Debug.Log("[PcDiag] nav check: checked=" + checkedCount + " failed=" + failed + " okAfterSnap=" + fixedBySnap);
	}

	/// <summary>
	/// Test: -pcEffects 60 for 3 s from 60 s emits a set of hit effects every 0.1 s at the hit point of a ray through the screen center:
	/// sparks, dust, debris, flashes, shells and blood (EffectsManager's private emit functions, called via reflection), to check the converted legacy particles.
	/// </summary>
	private IEnumerator EmitTestEffects()
	{
		string argument = GetArgument("-pcEffects");
		if (argument == null)
		{
			yield break;
		}
		float start = float.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < start)
		{
			yield return null;
		}
		BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		System.Type type = typeof(EffectsManager);
		MethodInfo sparks = type.GetMethod("EmitSparks", flags);
		MethodInfo dust = type.GetMethod("EmitDust", flags);
		MethodInfo fragments = type.GetMethod("EmitFragments", flags);
		MethodInfo flash = type.GetMethod("EmitFlash", flags);
		MethodInfo shell = type.GetMethod("EmitShell", flags);
		int rounds = 0;
		while (Time.realtimeSinceStartup < start + 3f)
		{
			EffectsManager manager = EffectsManager.instance;
			Camera camera = Camera.main;
			RaycastHit hit;
			if (manager != null && camera != null && Physics.Raycast(camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out hit, 100f))
			{
				Vector3 point = hit.point;
				Vector3 normal = hit.normal;
				sparks.Invoke(manager, new object[] { point, normal, 4 });
				dust.Invoke(manager, new object[] { point, normal, 2 });
				fragments.Invoke(manager, new object[] { point, normal, 2 });
				flash.Invoke(manager, new object[] { point, normal });
				shell.Invoke(manager, new object[] { camera.transform.position + camera.transform.forward, camera.transform.right, Weapon.AmmoType.PistolCartridge });
				manager.DrawBloodSplatter(point + normal * 0.3f);
				rounds++;
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
		Debug.Log("[PcDiag] test effects emitted " + rounds + " rounds");
	}

	/// <summary>
	/// Test: -pcMission 50:27 calls the game's own AppGame.GoToGameLevel(GameData.Missions[27]) at that time,
	/// entering that mission's level directly (as if it were picked on the map), to check levels one by one. Needs a save that is past the tutorial.
	/// </summary>
	private IEnumerator StartMission()
	{
		string argument = GetArgument("-pcMission");
		if (argument == null)
		{
			yield break;
		}
		string[] parts = argument.Split(':');
		float target = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
		int index = int.Parse(parts[1]);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		GameData.MissionData mission = Singleton.GameData.Missions[index];
		Debug.Log("[PcDiag] start mission t=" + target + " index=" + index + " scene=" + mission.Scene + " type=" + mission.Type
			+ " from=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
		Singleton.Game.GoToGameLevel(mission, false);
	}

	/// <summary>
	/// Test: -pcPostEffect 60:LowHealth,80:Dying,100:Normal calls PostEffectManager.SetPostEffect at those times,
	/// triggering full-screen post effects directly (low health, dying chromatic aberration, etc.), to verify PcBlit and the translated post-effect shaders.
	/// </summary>
	private IEnumerator ForcePostEffects()
	{
		string list = GetArgument("-pcPostEffect");
		if (list == null)
		{
			yield break;
		}
		foreach (string item in list.Split(','))
		{
			string[] parts = item.Split(':');
			float target = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
			PostEffectManager.PostEffect effect = (PostEffectManager.PostEffect)System.Enum.Parse(typeof(PostEffectManager.PostEffect), parts[1]);
			while (Time.realtimeSinceStartup < target)
			{
				yield return null;
			}
			PostEffectManager manager = Object.FindAnyObjectByType<PostEffectManager>();
			if (manager == null)
			{
				Debug.Log("[PcDiag] post effect " + effect + ": no PostEffectManager");
				continue;
			}
			Debug.Log("[PcDiag] post effect t=" + target + " " + effect);
			// SetPostEffect does not switch while an effect is active, so switch back to Normal first
			manager.SetPostEffect(PostEffectManager.PostEffect.Normal, 0f);
			manager.SetPostEffect(effect, 5f);
		}
	}

	/// <summary>
	/// Test: -pcFinishTutorial <seconds> calls, at that time, the same interface as "Finish tutorial" in the game's built-in cheat window
	/// (Singleton.Tutorial.Finish + Cheat.CompleteMission), skipping the tutorial level, to test the map, menus and other levels after the tutorial.
	/// </summary>
	private IEnumerator FinishTutorial()
	{
		string seconds = GetArgument("-pcFinishTutorial");
		if (seconds == null)
		{
			yield break;
		}
		float target = float.Parse(seconds, System.Globalization.CultureInfo.InvariantCulture);
		while (Time.realtimeSinceStartup < target)
		{
			yield return null;
		}
		if (!(bool)Singleton.Tutorial)
		{
			Debug.Log("[PcDiag] finish tutorial: no tutorial running");
			yield break;
		}
		Debug.Log("[PcDiag] finish tutorial t=" + target + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
		Singleton.Tutorial.Finish();
		Cheat.CompleteMission();
	}

	private IEnumerator Start()
	{
		for (int round = 0; round < Rounds; round++)
		{
			yield return new WaitForSecondsRealtime(IntervalSeconds);
			StringBuilder report = new StringBuilder();
			report.AppendLine("[PcDiag] t=" + Time.realtimeSinceStartup.ToString("F1") + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
			report.AppendLine("  DynamicContent.Progress=" + DynamicContent.Progress + " inProgress=" + DynamicContent.IsUpdateInProgress());
			Glu.ABTesting.Resolution resolution = DynamicContent.ABTestingResolution;
			report.AppendLine("  ABTesting: " + Describe(resolution));
			IndexInfo indexInfo = DynamicContent.AssetBundlesIndexInfo;
			report.AppendLine("  IndexInfo: " + Describe(indexInfo));
			if (indexInfo != null)
			{
				report.AppendLine("    index download: " + Describe(indexInfo.downloadInfo));
				foreach (object bundleInfo in indexInfo.assetBundleInfo)
				{
					report.AppendLine("    bundle: " + Describe(bundleInfo));
				}
			}
			Debug.Log(report.ToString());
		}
	}

	/// <summary>Takes screenshots at the real times given by the -pcShots / -pcShotDir launch arguments.</summary>
	private IEnumerator CaptureShots()
	{
		string times = GetArgument("-pcShots");
		string directory = GetArgument("-pcShotDir");
		if (times == null || directory == null)
		{
			yield break;
		}
		System.IO.Directory.CreateDirectory(directory);
		foreach (string item in times.Split(','))
		{
			float target = float.Parse(item, System.Globalization.CultureInfo.InvariantCulture);
			while (Time.realtimeSinceStartup < target)
			{
				yield return null;
			}
			// Wait until this frame has finished rendering before capturing
			yield return new WaitForEndOfFrame();
			string path = System.IO.Path.Combine(directory, "shot_" + ((int)target).ToString("D3") + "s.png");
			ScreenCapture.CaptureScreenshot(path);
			Debug.Log("[PcDiag] screenshot " + path + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
		}
	}

	/// <summary>
	/// Injects gestures at the real times given by the -pcAutoTap launch argument (for automated tests, gets through the UI unattended).
	/// Format: comma separated, each item "seconds[@x:y][>x2:y2][~duration]". x and y are screen fractions from 0 to 1 (origin bottom left), default screen center;
	/// ">x2:y2" drags there, "~duration" is the hold time in seconds, default 0 (tap); "~duration+hold" keeps holding that many more seconds after reaching the end point (for sticks).
	/// Example: -pcAutoTap 40,50@0.8:0.15,60@0.5:0.5>0.7:0.5~3
	/// </summary>
	private IEnumerator AutoTaps()
	{
		string taps = GetArgument("-pcAutoTap");
		if (taps == null)
		{
			yield break;
		}
		System.Globalization.CultureInfo invariant = System.Globalization.CultureInfo.InvariantCulture;
		foreach (string item in taps.Split(','))
		{
			string rest = item;
			float duration = 0f;
			float hold = 0f;
			int tilde = rest.IndexOf('~');
			if (tilde >= 0)
			{
				string timing = rest.Substring(tilde + 1);
				rest = rest.Substring(0, tilde);
				// "~move duration+hold duration": keep holding after reaching the end point
				int plus = timing.IndexOf('+');
				if (plus >= 0)
				{
					hold = float.Parse(timing.Substring(plus + 1), invariant);
					timing = timing.Substring(0, plus);
				}
				duration = float.Parse(timing, invariant);
			}
			Vector2 end = new Vector2(-1f, -1f);
			int arrow = rest.IndexOf('>');
			if (arrow >= 0)
			{
				end = ParsePoint(rest.Substring(arrow + 1));
				rest = rest.Substring(0, arrow);
			}
			Vector2 start = new Vector2(0.5f, 0.5f);
			int at = rest.IndexOf('@');
			if (at >= 0)
			{
				start = ParsePoint(rest.Substring(at + 1));
				rest = rest.Substring(0, at);
			}
			if (end.x < 0f)
			{
				end = start;
			}
			float target = float.Parse(rest, invariant);
			while (Time.realtimeSinceStartup < target)
			{
				yield return null;
			}
			Vector2 startPixels = new Vector2(start.x * Screen.width, start.y * Screen.height);
			Vector2 endPixels = new Vector2(end.x * Screen.width, end.y * Screen.height);
			Debug.Log("[PcDiag] auto gesture t=" + target + " " + startPixels + " -> " + endPixels + " ~" + duration + "s scene="
				+ UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
			PcInput.InjectGesture(startPixels, endPixels, duration, hold);
		}
	}

	/// <summary>Parses "x:y" (screen fractions from 0 to 1).</summary>
	private static Vector2 ParsePoint(string text)
	{
		System.Globalization.CultureInfo invariant = System.Globalization.CultureInfo.InvariantCulture;
		string[] parts = text.Split(':');
		return new Vector2(float.Parse(parts[0], invariant), float.Parse(parts[1], invariant));
	}

	private static string GetArgument(string name)
	{
		string[] arguments = System.Environment.GetCommandLineArgs();
		for (int i = 0; i + 1 < arguments.Length; i++)
		{
			if (arguments[i] == name)
			{
				return arguments[i + 1];
			}
		}
		return null;
	}

	/// <summary>Lists the object's simple public properties (enums, strings, numbers, booleans).</summary>
	private static string Describe(object target)
	{
		if (target == null)
		{
			return "<null>";
		}
		StringBuilder text = new StringBuilder(target.GetType().Name + " {");
		foreach (PropertyInfo property in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
		{
			if (property.GetIndexParameters().Length > 0)
			{
				continue;
			}
			System.Type type = property.PropertyType;
			bool simple = type.IsEnum || type.IsPrimitive || type == typeof(string);
			if (!simple)
			{
				continue;
			}
			object value;
			try
			{
				value = property.GetValue(target, null);
			}
			catch (System.Exception e)
			{
				value = "<" + e.GetType().Name + ">";
			}
			text.Append(" " + property.Name + "=" + value);
		}
		text.Append(" }");
		return text.ToString();
	}
}
