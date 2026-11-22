// PC port: key hint labels (ESC / SPACE / R / P etc. on the Mac HUD).
// Text: clones an original SpriteText as template (keeps font, layer, rendering), parented under the button;
//   the text comes from PcKeyBindings.
// Red banner (optional): a sprite from the banner sheet (Resources/PortAddon/PcKeyBanners.png, made by
//   make_key_banners.py) placed as a child of the text, centered behind it and sized to it.
//   Moving the text moves the banner; PcKeyHintRoot makes a Scene-view click on the banner select the text.
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
	/// <summary>
	/// Right below the blue icon ring (the ring is smaller than the 1 x 1 quad; -0.65 sat on the crate lid and
	/// inside the taller ammo crate). Shared by ammo crates and bonus boxes; tune with CKZ2/HUD Layout Tool.
	/// z: pulled toward the camera (Button -z faces the camera; z scale 0.05, so -8 ~= 0.4 m) so the crate model
	/// right under the icon doesn't cover the hint. The Button faces the camera, so this barely moves it on screen.
	/// </summary>
	private static readonly Vector3 PickUpHintLocalPosition = new Vector3(0f, -0.45f, -0.73f);
	/// <summary>Pick-up banner behind the text (Button +z faces away from the camera; z scale 0.05, so 0.2 ~= 1 cm).</summary>
	private const float PickUpBannerBehindZ = 0.2f;

	// ---- Banner sheet (layout must match make_key_banners.py) ----
	/// <summary>Banner sheet (Resources path).</summary>
	private const string BannerSheetPath = "PortAddon/PcKeyBanners";
	private const int BannerSheetColumns = 4;
	private const int BannerSheetRows = 5;
	/// <summary>Banner used for every hint: number N in the sheet (1-based, reading order). 3 = short brush.</summary>
	private const int BannerIndex = 3;
	/// <summary>
	/// Banner quad size in units of the text's character size. The quad covers the whole sheet cell, which has a
	/// transparent margin around the brush, so these are larger than the visible red part.
	/// Width = text width + BannerExtraWidth; never narrower than BannerMinWidth (single letters).
	/// </summary>
	private const float BannerExtraWidth = 2.2f;
	private const float BannerMinWidth = 3.2f;
	private const float BannerHeight = 2.2f;
	/// <summary>
	/// Alpha-blended sprite shader of the HUD. Banners always use it: the 3D icon Button (pick-up banner template)
	/// is additive, which tints the red banner by whatever is behind it (orange over the green ammo crate).
	/// </summary>
	private const string BannerShaderName = "Sprite/Vertex Colored, Fast";

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
		hintObject.AddComponent<PcKeyHintRoot>();
		Debug.Log("[PcKeyHints] '" + label + "' attached to " + parent.name);
		return hint;
	}

	/// <summary>
	/// Puts the red banner behind hint, as a child of the hint text (so it follows the text).
	/// template is any original UISingleSprite; only its components and material are reused.
	/// behindZ: local z offset from the text (positive = further from the camera).
	/// </summary>
	public static void AttachBanner(UISingleSprite template, SpriteText hint, float behindZ)
	{
		Texture2D sheet = Resources.Load(BannerSheetPath) as Texture2D;
		if (sheet == null)
		{
			Debug.LogWarning("[PcKeyHints] banner sheet not found: " + BannerSheetPath);
			return;
		}
		GameObject bannerObject = Object.Instantiate(template.gameObject) as GameObject;
		bannerObject.name = "PcKeyBanner(Clone)";
		// The template may carry tap/fade components (e.g. the 3D icon Button); a banner only displays
		RemoveComponents<Collider>(bannerObject);
		RemoveComponents<Rigidbody>(bannerObject);
		RemoveComponents<SpriteFader>(bannerObject);
		// Drop anything already attached under the template (hints, banners)
		for (int i = bannerObject.transform.childCount - 1; i >= 0; i--)
		{
			Object.DestroyImmediate(bannerObject.transform.GetChild(i).gameObject);
		}
		// Activate at the scene root first so Awake runs (the template may be inactive), then parent
		bannerObject.SetActive(true);
		bannerObject.transform.SetParent(hint.transform, false);
		bannerObject.transform.localPosition = new Vector3(0f, 0f, behindZ);
		bannerObject.transform.localRotation = Quaternion.identity;
		bannerObject.transform.localScale = Vector3.one;

		// Size from the text: the hint text is centered, so the banner is centered on it
		float characterSize = hint.characterSize;
		float width = hint.TotalWidth + characterSize * BannerExtraWidth;
		float minWidth = characterSize * BannerMinWidth;
		if (width < minWidth)
		{
			width = minWidth;
		}
		float height = characterSize * BannerHeight;

		UISingleSprite banner = bannerObject.GetComponent<UISingleSprite>();
		// Size comes from SetSize below, not from texture pixels
		banner.pixelPerfect = false;
		banner.autoResize = false;
		banner.SetTexture(sheet);
		// SetTexture already gave the banner its own material instance, so changing the shader affects only it
		Shader bannerShader = Shader.Find(BannerShaderName);
		if (bannerShader != null)
		{
			bannerObject.GetComponent<Renderer>().material.shader = bannerShader;
		}
		else
		{
			Debug.LogWarning("[PcKeyHints] banner shader not found: " + BannerShaderName);
		}
		banner.SetAnchor(SpriteRoot.ANCHOR_METHOD.MIDDLE_CENTER);
		banner.SetUVs(GetBannerUv(BannerIndex));
		banner.SetSize(width, height);
		banner.SetColor(Color.white);
		banner.Hide(false);
		Debug.Log("[PcKeyHints] banner " + BannerIndex + " under '" + hint.Text + "' (" + width.ToString("F2") + " x " + height.ToString("F2") + ")");
	}

	/// <summary>UV rect of banner number (1-based, reading order from the top-left); UV origin is bottom-left.</summary>
	private static Rect GetBannerUv(int number)
	{
		int cell = number - 1;
		int column = cell % BannerSheetColumns;
		int row = cell / BannerSheetColumns;
		float cellWidth = 1f / BannerSheetColumns;
		float cellHeight = 1f / BannerSheetRows;
		float x = column * cellWidth;
		float y = 1f - (row + 1) * cellHeight;
		return new Rect(x, y, cellWidth, cellHeight);
	}

	private static void RemoveComponents<T>(GameObject target) where T : Component
	{
		foreach (T component in target.GetComponents<T>())
		{
			Object.DestroyImmediate(component);
		}
	}

	/// <summary>
	/// Pick-up key under a 3D icon (ActivateItem's Button).
	/// Text template is the ActivateItem's own Text (world-space SpriteText on Gui3d).
	/// Parented to the Button, which faces the camera every frame and deactivates when far away.
	/// Banner template is the Button itself.
	/// Shown only together with itemText ("Collect bonus" etc.), not whenever the icon is visible.
	/// </summary>
	public static void AttachPickUpHint(SpriteText itemText, UISingleSprite button)
	{
		SpriteText hint = Attach(itemText, button.transform, PcKeyBindings.PickUpItem, PickUpHintLocalPosition, PickUpHintCharacterSize);
		AttachBanner(button, hint, PickUpBannerBehindZ);
		// After the banner exists, so its renderer is included
		PcPickUpHintVisibility visibility = hint.gameObject.AddComponent<PcPickUpHintVisibility>();
		visibility.Init(itemText.GetComponent<Renderer>());
	}
}
