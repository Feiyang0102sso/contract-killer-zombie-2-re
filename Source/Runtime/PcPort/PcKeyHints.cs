// PC port: key hint labels (ESC / SPACE / R / P etc. on the Mac HUD).
// Text: clones an original SpriteText as template (keeps font, layer, rendering), parented under the button;
//   the text comes from PcKeyBindings.
// Labels are children of the button, so they hide with it.
//
// Called from:
//   - HUD: HudController.Start -> CreatePcKeyHints() (HudController.Pc.cs, layout constants there);
//   - 3D icons (ammo crate, bonus box): ActivateItem.Start -> AttachPickUpHint.
using UnityEngine;

public static class PcKeyHints
{
	/// <summary>Hint text color: yellow, as on Mac.</summary>
	public static readonly Color HintColor = new Color(1f, 0.85f, 0f, 1f);

	// ---- Pick-up hint under 3D icons (Button local space: height 1, centered, scale 0.7) ----
	/// <summary>Font size: 0.2 x 0.7 ~= the 0.14 text above the icon.</summary>
	private const float PickUpHintCharacterSize = 0.2f;
	/// <summary>Right below the icon.</summary>
	private static readonly Vector3 PickUpHintLocalPosition = new Vector3(0f, -0.65f, 0f);

	/// <summary>
	/// Clones template into a key hint under parent at localPosition.
	/// The clone's name ends with "(Clone)", so SpriteText.Awake marks it isClone and won't delete the template's mesh.
	/// </summary>
	public static SpriteText Attach(SpriteText template, Transform parent, KeyCode key, Vector3 localPosition, float characterSize)
	{
		string label = PcKeyBindings.GetDisplayName(key);
		GameObject hintObject = Object.Instantiate(template.gameObject) as GameObject;
		hintObject.name = "PcKeyHint_" + label + "(Clone)";
		// Activate at the scene root first so Awake runs, then parent (the parent may be inactive)
		hintObject.SetActive(true);
		hintObject.transform.SetParent(parent, false);
		hintObject.transform.localPosition = localPosition;
		hintObject.transform.localRotation = Quaternion.identity;
		hintObject.transform.localScale = Vector3.one;
		SpriteText hint = hintObject.GetComponent<SpriteText>();
		hint.SetCharacterSize(characterSize);
		hint.SetColor(HintColor);
		hint.Text = label;
		hint.Hide(false);
		Debug.Log("[PcKeyHints] '" + label + "' attached to " + parent.name);
		return hint;
	}

	/// <summary>
	/// Pick-up key under a 3D icon (ActivateItem's Button).
	/// Text template is the ActivateItem's own Text (world-space SpriteText on Gui3d).
	/// Parented to the Button, which faces the camera every frame and deactivates when far away.
	/// </summary>
	public static void AttachPickUpHint(SpriteText itemText, UISingleSprite button)
	{
		Attach(itemText, button.transform, PcKeyBindings.PickUpItem, PickUpHintLocalPosition, PickUpHintCharacterSize);
	}
}
