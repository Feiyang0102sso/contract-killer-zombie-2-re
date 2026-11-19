// 试点诊断（临时）：启动后每 2 秒打印动态内容流程各环节的状态，共 30 秒。
// Glu 自己的日志在发布版里被关掉了，用它来判断启动卡在哪个环节。
// 另外支持自动截图：启动参数 -pcShots 20,45,70 -pcShotDir <目录>，到点截游戏自己的画面
// （不截屏幕，窗口被遮挡也能截到，也不会截到其他程序）。
// 全部用真实时间：游戏加载时会把 timeScale 设成 0。
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
	/// 测试用：-pcSaveCheck <存档文件路径> 启动时不经校验直接用 PlayerData.Data 反序列化这个文件，
	/// 打印完整的异常链。PlayerData.Load 会吞掉异常、当成新档，用这个查存档为什么读不回来。
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
			object data = StorageManager.ReadXmlFromLocationWithoutChecking(path, typeof(PlayerData.Data));
			Debug.Log("[PcDiag] save check ok: " + path + " -> " + data);
		}
		catch (System.Exception e)
		{
			Debug.Log("[PcDiag] save check failed: " + path + "\n" + e);
		}
		// 往返测试：两个键的 XmlDictionary<KeyValuePair<string, string>, int>（PlayerStatistics 的写法）
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
	/// 测试用：-pcTaskTap "melee=0.746:0.09|melee=0.5:0.3" 当前任务名包含“=”前的文字时，每 2.5 秒（真实时间）点一下，
	/// 同一任务匹配多项就轮流点。不用固定时间点，适合流程快慢不定的教程。文字里不能有空格（启动参数按空格拆分）。
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
	/// 测试用：-pcAutoMove 90:400 从 90 秒到 400 秒（真实时间），用手指 3（PcInput.MoveFinger）按住左摇杆，
	/// 朝 Player.GetCurrentMissionTarget()（HUD 任务箭头指的点，沿导航路径前进）走；离目标 1 米以内或没有目标时松开。
	/// 摇杆方向按 Player.UpdateMovement 的换算反过来算：摇杆 y 是镜头前方，x 是镜头右方。
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
			// 沿导航网格路径走：朝第一个离玩家超过 0.5 米的拐点，免得直线撞在柱角上卡住
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
	/// 随 -pcCameraWatch：RunSurvivalMode 出现的那一帧和之后几帧，记录 Singleton.UI.MapWindow 下能找到的地图点
	/// （默认只找 active 的 / 含 inactive 的 / 带任务的），查生存模式教程提示为什么没出现。
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
	/// 测试用：-pcCameraWatch 2 每隔 2 秒（真实时间）列出所有启用的相机（深度、清屏方式、渲染目标、位置）
	/// 和 PostEffectManager 的状态，查过场之后画面全白之类的问题。
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
			// 关卡里才有任务；Scene.Mission 在关卡外会抛空引用，所以先看有没有 Mission 对象
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
			// 教程状态和当前挂着的教程脚本（地图上的教程提示没出现时用）
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
			// 30 米内的 ActivateItem（补给点、终端等）：玩家离触发球多远、是否算进入、图标按钮是否显示
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
	/// 测试用：-pcAutoAim 45:300 从 45 秒到 300 秒（真实时间），场景里有活着、在镜头前的敌人时，
	/// 用手指 2（PcInput.AimFinger.Active）按住屏幕右半边：先静止 0.3 秒（游戏规定按下后 0.2 秒内不拖动才算开火），
	/// 再按目标偏离屏幕中心的方向拖动视角，把准星拉到离中心最近的敌人身上，一直按着开火。
	/// 手指拖出右半边或按满 5 秒就松开重按。没有敌人时松开。用来在无人值守时把关卡真正打一遍。
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
			// 往目标方向拖：每帧移动偏移量的一小部分，限幅，避免来回过冲
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
	/// 选一个敌人瞄准，返回它的中心在屏幕上相对中心的偏移（像素）；没有就返回 null。
	/// 优先选没被遮挡（镜头到它中心的连线先碰到它自己）的，其次离玩家近的；被挡住的只在没有可见敌人时才选。
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
			// 没有敌人时，把镜头转向玩家已进入触发区的 ActivateItem 图标（补给点等），方便 TapActivateItems 点它
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
					// 在身后：往右转
					best = new Vector2(Screen.width, 0f);
				}
			}
		}
		return best;
	}

	/// <summary>玩家已进入触发区、图标按钮正显示的 ActivateItem 的按钮；没有就返回 null。</summary>
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
	/// -pcAutoAim 期间每 2.5 秒：玩家已进入某个 ActivateItem 的触发区、它的图标在画面里，就点一下图标（补给弹药、启动终端等）。
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
	/// 测试用：-pcZombieWatch 2 每隔 2 秒（真实时间）打印场景里每个 Unit（僵尸等）的状态机状态、位置、
	/// NavMeshAgent 的状态（是否在网格上、目标点、路径状态、速度）和当前任务，查“僵尸站着不动”。
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
		// 每帧检查状态机，状态一变就记一条（带目标的位置和距离），查清楚是从哪个状态掉回 Staying 的
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
			// 关卡还没加载时 Scene.Mission 会抛空引用，这时不报告
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
			// 不走导航网格、只靠 CharacterController 站在地上的角色（Human 等）：看是否着地、脚下有什么碰撞体
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
	/// 测试用：-pcPathCheck 88 到点重放 PlayerProgress.BuildPath 的每一段（相邻路径节点之间的 CalculatePath），
	/// 记录状态、拐点数，以及节点离导航网格多远，查玩家路线上落在网格外的点是怎么来的。
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

	/// <summary>测试用：-pcPerf 30 每隔 30 秒（真实时间）记录这段时间的平均帧率、托管堆和 Unity 分配的内存，查泄漏和卡顿。</summary>
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

	/// <summary>测试用：-pcAddMoney 45:5000 到点给软货币加钱（与作弊窗口的加钱按钮相同的做法），测成功购买和扣款。</summary>
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

	/// <summary>测试用：-pcCompleteMission 95 到点调用游戏作弊窗口里的 Cheat.CompleteMission，测任务结算、奖励、写存档和回地图的流程。</summary>
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

	/// <summary>测试用：-pcAudioCheck 60,90 到点列出正在播放的 AudioSource（声音听不到，只能这样确认有在播）。</summary>
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

	/// <summary>测试用：-pcShopDump 50 到点列出商店配置的分组、条目和 readyToShow（查商店打开时没有可选分组的问题）。</summary>
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
	/// 测试用：-pcNavCheck 85 到点检查当前关卡每个出生点到玩家路径点的寻路（与 SmartSpawnPoint.CalcPathsLength 相同的调用），
	/// 失败时再看两端离导航网格多远、贴到网格上以后能否算出路径，用来区分“偏离网格”和“网格不连通”。
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
	/// 测试用：-pcEffects 60 从 60 秒起的 3 秒内，在屏幕中心射线的命中点每 0.1 秒发射一组击中特效：
	/// 火花、灰尘、碎片、闪光、弹壳、血花（EffectsManager 的私有发射函数用反射调用），用来检查旧版粒子的转换结果。
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
	/// 测试用：-pcMission 50:27 到点调用游戏自己的 AppGame.GoToGameLevel(GameData.Missions[27])，
	/// 直接进入某个任务的关卡（相当于在地图上选了这个任务），用来逐个检查各关卡。需要存档已过教程。
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
	/// 测试用：-pcPostEffect 60:LowHealth,80:Dying,100:Normal 到点调用 PostEffectManager.SetPostEffect，
	/// 直接触发全屏后处理（低血量、濒死色差等），验证 PcBlit 与翻译后的后处理着色器。
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
			// SetPostEffect 在已有效果时不切换，所以先切回 Normal
			manager.SetPostEffect(PostEffectManager.PostEffect.Normal, 0f);
			manager.SetPostEffect(effect, 5f);
		}
	}

	/// <summary>
	/// 测试用：-pcFinishTutorial <秒> 到点调用游戏自带作弊窗口里 "Finish tutorial" 的同一套接口
	/// （Singleton.Tutorial.Finish + Cheat.CompleteMission），跳过教程关，用来测教程之后的地图、菜单和其他关卡。
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

	/// <summary>按 -pcShots / -pcShotDir 启动参数在指定的真实时间点截图。</summary>
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
			// 等这一帧渲染完再截
			yield return new WaitForEndOfFrame();
			string path = System.IO.Path.Combine(directory, "shot_" + ((int)target).ToString("D3") + "s.png");
			ScreenCapture.CaptureScreenshot(path);
			Debug.Log("[PcDiag] screenshot " + path + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
		}
	}

	/// <summary>
	/// 按 -pcAutoTap 启动参数在指定的真实时间点注入手势（自动测试用，人不在时也能点过界面）。
	/// 格式：逗号分隔，每项是 "秒[@x:y][>x2:y2][~时长]"。x、y 是 0～1 的屏幕比例（左下角为原点），默认屏幕中心；
	/// ">x2:y2" 表示拖到该处，"~时长" 表示按住的秒数，默认 0（点击）；"~时长+按住" 表示拖到终点后再按住若干秒（摇杆用）。
	/// 例：-pcAutoTap 40,50@0.8:0.15,60@0.5:0.5>0.7:0.5~3
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
				// "~移动时长+按住时长"：拖到终点后继续按住
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

	/// <summary>解析 "x:y"（0～1 的屏幕比例）。</summary>
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

	/// <summary>列出对象的简单公开属性（枚举、字符串、数值、布尔）。</summary>
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
