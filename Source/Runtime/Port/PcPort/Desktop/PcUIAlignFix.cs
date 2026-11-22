// PC port: realigns every UIAlign (screen-edge anchored UI group) when the screen size changes.
// UIAlign (screen mode) places a UI group at a screen edge once, in Awake/Start. iOS screens never change size, so the
// original never realigns; on PC a window resize or resolution switch left edge-aligned UI (city map shop button,
// HUD corners, popup close buttons, ...) at the old edges.
// UIAlign.Start registers itself here; a hidden instance of this component watches Screen.width/height and calls
// UpdateAlignment on every registered UIAlign that is still alive, enabled and active when the size changes.
using System.Collections.Generic;
using UnityEngine;

public class PcUIAlignFix : MonoBehaviour
{
	private static readonly List<UIAlign> _aligns = new List<UIAlign>();

	private static PcUIAlignFix _watcher;

	private int _lastWidth;

	private int _lastHeight;

	/// <summary>Called from UIAlign.Start: realign this one whenever the screen size changes.</summary>
	public static void Register(UIAlign align)
	{
		// UIAlign also runs in edit mode ([ExecuteInEditMode]); only watch in play mode
		if (!Application.isPlaying)
		{
			return;
		}
		_aligns.Add(align);
		if (_watcher == null)
		{
			GameObject host = new GameObject("PcUIAlignFix");
			Object.DontDestroyOnLoad(host);
			_watcher = host.AddComponent<PcUIAlignFix>();
			_watcher._lastWidth = Screen.width;
			_watcher._lastHeight = Screen.height;
		}
	}

	private void LateUpdate()
	{
		if (Screen.width == _lastWidth && Screen.height == _lastHeight)
		{
			return;
		}
		Debug.Log("[PcUIAlignFix] screen " + _lastWidth + "x" + _lastHeight + " -> " + Screen.width + "x" + Screen.height + ", realigning UI");
		_lastWidth = Screen.width;
		_lastHeight = Screen.height;
		for (int i = _aligns.Count - 1; i >= 0; i--)
		{
			UIAlign align = _aligns[i];
			// destroyed with its screen
			if (align == null)
			{
				_aligns.RemoveAt(i);
				continue;
			}
			if (align.enabled && align.gameObject.activeInHierarchy)
			{
				align.UpdateAlignment();
			}
		}
	}
}
