// PC port: hides the touch-only HUD controls in the lower half of the screen (sticks, switch / zoom / fire / melee
// buttons) and the touch parts of the tutorial tips (half-screen backs, finger icons, tap splashes).
// Only rendering is switched off: the objects stay active, because the original code re-activates them on every
// weapon change (UpdateSticksSetup) and both the PC keys and the tutorials read their active state
// ("can switch / zoom / melee now").
// Renderers are switched off every LateUpdate: StickScript and EZ GUI's Hide() switch them back on in Update.
// Colliders are kept; in gameplay the cursor is locked, so nothing taps them.
// TutorialArrow asks IsHidden() and doesn't point at a hidden button.
// Added by HudController.Start (HudController.Pc.cs) when PcKeyBindings.KeyboardUi is on.
using System.Collections.Generic;
using UnityEngine;

public class PcTouchControlsHider : MonoBehaviour
{
	/// <summary>Hidden roots of the current HUD (empty when there is none).</summary>
	private static readonly List<Transform> _hiddenRoots = new List<Transform>();

	/// <summary>Renderers kept off, collected once (inactive ones included).</summary>
	private readonly List<Renderer> _renderers = new List<Renderer>();

	/// <summary>
	/// Collects the renderers under roots, except those under keep (tip labels stay visible).
	/// </summary>
	public void Init(GameObject[] roots, GameObject[] keep)
	{
		_hiddenRoots.Clear();
		foreach (GameObject root in roots)
		{
			_hiddenRoots.Add(root.transform);
			foreach (Renderer candidate in root.GetComponentsInChildren<Renderer>(true))
			{
				if (IsUnderAny(candidate.transform, keep))
				{
					continue;
				}
				_renderers.Add(candidate);
			}
		}
		Debug.Log("[PcTouchControls] hiding " + roots.Length + " controls (" + _renderers.Count + " renderers)");
	}

	/// <summary>Whether target is one of the hidden controls or inside one.</summary>
	public static bool IsHidden(GameObject target)
	{
		foreach (Transform root in _hiddenRoots)
		{
			if (root != null && target.transform.IsChildOf(root))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsUnderAny(Transform target, GameObject[] parents)
	{
		foreach (GameObject parent in parents)
		{
			if (target.IsChildOf(parent.transform))
			{
				return true;
			}
		}
		return false;
	}

	private void LateUpdate()
	{
		foreach (Renderer hidden in _renderers)
		{
			if (hidden != null && hidden.enabled)
			{
				hidden.enabled = false;
			}
		}
	}

	private void OnDestroy()
	{
		_hiddenRoots.Clear();
	}
}
