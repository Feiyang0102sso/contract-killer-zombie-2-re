// PC port: shows a 3D icon's pick-up hint (P and its banner) only while the icon's own text
// ("Collect bonus", "Resupply ammo") is shown, i.e. when the player is close enough to use it.
// ActivateItem.Update shows/hides that text by enabling its renderer; Hide() on the text doesn't reach
// children elsewhere, so this copies the renderer state every frame (LateUpdate runs after ActivateItem.Update).
using UnityEngine;

public class PcPickUpHintVisibility : MonoBehaviour
{
	/// <summary>Renderer of ActivateItem's Text, the visibility source.</summary>
	private Renderer _itemTextRenderer;

	/// <summary>Renderers of the hint text and its banner.</summary>
	private Renderer[] _hintRenderers;

	public void Init(Renderer itemTextRenderer)
	{
		_itemTextRenderer = itemTextRenderer;
		_hintRenderers = GetComponentsInChildren<Renderer>(true);
	}

	private void LateUpdate()
	{
		bool show = _itemTextRenderer.enabled;
		foreach (Renderer hintRenderer in _hintRenderers)
		{
			if (hintRenderer.enabled != show)
			{
				hintRenderer.enabled = show;
			}
		}
	}
}
