// PC port: FPS counter in the bottom-left corner (the only HUD-free corner; the radar sits at the top center),
// on when the launcher's "Show FPS" (DrawFPS=1) is set.
// The original FpsCounter (Packages/FpsMem) only measures; its drawing code is not in the shipped game, so this draws
// its own number with IMGUI. Uses real time, so it keeps counting while the game pauses with timeScale 0.
using UnityEngine;

public class PcFpsOverlay : MonoBehaviour
{
	/// <summary>Seconds between updates of the shown number.</summary>
	private const float UpdateInterval = 0.5f;

	/// <summary>Font size as a fraction of the screen height.</summary>
	private const float FontSizeOfScreenHeight = 1f / 40f;

	private const int MinFontSize = 12;

	/// <summary>Distance from the left and bottom screen edges, as a fraction of the screen height.</summary>
	private const float MarginOfScreenHeight = 1f / 100f;

	private int _frames;

	private float _intervalStart;

	private string _text = string.Empty;

	private GUIStyle _style;

	private GUIStyle _shadowStyle;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		if (!PcLaunchArgs.DrawFps)
		{
			return;
		}
		GameObject holder = new GameObject("PcFpsOverlay");
		DontDestroyOnLoad(holder);
		holder.AddComponent<PcFpsOverlay>();
		Debug.Log("[PcFpsOverlay] on");
	}

	private void Start()
	{
		_intervalStart = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		_frames++;
		float now = Time.realtimeSinceStartup;
		float elapsed = now - _intervalStart;
		if (elapsed < UpdateInterval)
		{
			return;
		}
		int fps = Mathf.RoundToInt(_frames / elapsed);
		_text = "FPS " + fps;
		_frames = 0;
		_intervalStart = now;
	}

	private void OnGUI()
	{
		if (_style == null)
		{
			_style = new GUIStyle(GUI.skin.label);
			_style.alignment = TextAnchor.LowerLeft;
			_style.fontStyle = FontStyle.Bold;
			_style.normal.textColor = Color.white;
			_shadowStyle = new GUIStyle(_style);
			_shadowStyle.normal.textColor = Color.black;
		}
		int fontSize = Mathf.Max(MinFontSize, Mathf.RoundToInt(Screen.height * FontSizeOfScreenHeight));
		_style.fontSize = fontSize;
		_shadowStyle.fontSize = fontSize;

		float margin = Screen.height * MarginOfScreenHeight;
		float areaHeight = fontSize * 2f;
		Rect area = new Rect(margin, Screen.height - margin - areaHeight, Screen.width, areaHeight);
		Rect shadowArea = new Rect(area.x + 1f, area.y + 1f, area.width, area.height);
		GUI.Label(shadowArea, _text, _shadowStyle);
		GUI.Label(area, _text, _style);
	}
}
