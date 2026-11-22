// PC port: custom mouse cursor (translucent cyan arrow with glow, drawn by make_cursor.py) instead of the
// system arrow. Uses the hardware cursor (CursorMode.Auto), so it follows the mouse without frame lag.
// The hardware cursor is drawn in physical pixels and is not scaled by Windows, so the size closest to
// 32 px times the display scale is picked (100% -> 32, 150% -> 48, 200% -> 64).
using UnityEngine;

public static class PcCursor
{
	private const string ResourcePrefix = "PortAddon/PcCursor";

	/// <summary>Available sizes (Resources/PortAddon/PcCursor&lt;size&gt;.png), smallest first.</summary>
	private static readonly int[] Sizes = new int[] { 32, 48, 64 };

	/// <summary>Cursor size at 100% display scale.</summary>
	private const float BaseSize = 32f;

	/// <summary>Windows DPI at 100% display scale.</summary>
	private const float BaseDpi = 96f;

	/// <summary>Grid the shape was designed on, and the arrow tip on it (must match make_cursor.py).</summary>
	private const float DesignGrid = 64f;

	private static readonly Vector2 HotspotOnGrid = new Vector2(6f, 5f);

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		int size = PickSize();
		Texture2D texture = Resources.Load<Texture2D>(ResourcePrefix + size);
		if (texture == null)
		{
			Debug.LogError("[PcCursor] missing cursor texture Resources/" + ResourcePrefix + size);
			return;
		}
		Vector2 hotspot = HotspotOnGrid * (size / DesignGrid);
		Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
		Debug.Log("[PcCursor] cursor " + size + " px, hotspot " + hotspot + ", dpi " + Screen.dpi);
	}

	/// <summary>The size closest to BaseSize times the display scale (Screen.dpi; 0 when unknown means 100%).</summary>
	private static int PickSize()
	{
		float scale = 1f;
		if (Screen.dpi > 0f)
		{
			scale = Screen.dpi / BaseDpi;
		}
		float wanted = BaseSize * scale;
		int best = Sizes[0];
		for (int i = 1; i < Sizes.Length; i++)
		{
			if (Mathf.Abs(Sizes[i] - wanted) < Mathf.Abs(best - wanted))
			{
				best = Sizes[i];
			}
		}
		return best;
	}
}
