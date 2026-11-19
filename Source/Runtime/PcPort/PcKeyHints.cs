// PC port: key hint labels (ESC / SPACE / R / P etc. on the Mac HUD).
// Text: clones an original SpriteText as template (keeps font, layer, rendering), parented under the button;
//   the text comes from PcKeyBindings.
// Red banner: clones an original UISingleSprite and swaps in the red brush from the GUICommon atlas
//   (original splash_blood_settings.png), cut into a horizontal strip; the left half is mirrored so both
//   ends are ragged. Approximates the Mac banner (which is new art not in the original assets).
// Labels and banners are children of the button, so they hide with it.
//
// Called from:
//   - HUD: HudController.Start -> CreatePcKeyHints() (HudController.Pc.cs, layout constants there);
//   - 3D icons (ammo crate, bonus box): ActivateItem.Start -> AttachPickUpHint.
using UnityEngine;

public static class PcKeyHints
{
	/// <summary>Hint text color: yellow, as on Mac.</summary>
	public static readonly Color HintColor = new Color(1f, 0.85f, 0f, 1f);

	/// <summary>Atlas holding the red brush (Resources path).</summary>
	private const string BannerAtlasPath = "copiedtextures/GUICommon";

	/// <summary>
	/// Strip cut from the brush (UV, origin bottom-left). Whole brush: x 0~0.313, y 0.286~0.853;
	/// straight left edge (atlas border), ragged right edge. x 0.157~0.313 is the right half; the left half mirrors it.
	/// </summary>
	private const float BannerUvInnerX = 0.157f;
	private const float BannerUvOuterX = 0.313f;
	private const float BannerUvY = 0.53f;
	private const float BannerUvHeight = 0.06f;

	// ---- Pick-up hint under 3D icons (Button local space: height 1, centered, scale 0.7) ----
	/// <summary>Font size: 0.2 x 0.7 ~= the 0.14 text above the icon.</summary>
	private const float PickUpHintCharacterSize = 0.2f;
	/// <summary>Right below the icon.</summary>
	private static readonly Vector3 PickUpHintLocalPosition = new Vector3(0f, -0.65f, 0f);
	/// <summary>Pick-up banner size.</summary>
	private const float PickUpBannerWidth = 0.9f;
	private const float PickUpBannerHeight = 0.2f;
	/// <summary>Banner slightly behind the text (Button +z faces away from the camera; z scale 0.05, so 0.2 ~= 1 cm).</summary>
	private const float PickUpBannerBehindZ = 0.2f;

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
	/// Puts a red banner at center under parent (width/height in the parent's local units).
	/// Built from two halves: right half is the brush as is (ragged right end), left half mirrors it (ragged left end);
	/// UVs meet at the seam. template is any original UISingleSprite; only its components/material are reused.
	/// </summary>
	public static void AttachBanner(UISingleSprite template, Transform parent, Vector3 center, float width, float height)
	{
		Texture2D atlas = Resources.Load(BannerAtlasPath) as Texture2D;
		if (atlas == null)
		{
			Debug.LogWarning("[PcKeyHints] banner atlas not found: " + BannerAtlasPath);
			return;
		}
		float halfWidth = width * 0.5f;
		float uvHalfWidth = BannerUvOuterX - BannerUvInnerX;
		// Left half: UV runs outer -> inner (negative width = mirrored), ragged edge on the left
		Rect leftUv = new Rect(BannerUvOuterX, BannerUvY, -uvHalfWidth, BannerUvHeight);
		// Right half: UV runs inner -> outer, ragged edge on the right
		Rect rightUv = new Rect(BannerUvInnerX, BannerUvY, uvHalfWidth, BannerUvHeight);
		Vector3 leftCenter = center + new Vector3(-halfWidth * 0.5f, 0f, 0f);
		Vector3 rightCenter = center + new Vector3(halfWidth * 0.5f, 0f, 0f);
		CreateBannerHalf(template, parent, atlas, leftUv, leftCenter, halfWidth, height);
		CreateBannerHalf(template, parent, atlas, rightUv, rightCenter, halfWidth, height);
		Debug.Log("[PcKeyHints] banner attached to " + parent.name);
	}

	private static void CreateBannerHalf(UISingleSprite template, Transform parent, Texture2D atlas, Rect uv, Vector3 localPosition, float width, float height)
	{
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
		// Activate at the scene root first so Awake runs (the template may be inactive, e.g. a hidden icon Button), then parent
		bannerObject.SetActive(true);
		bannerObject.transform.SetParent(parent, false);
		bannerObject.transform.localPosition = localPosition;
		bannerObject.transform.localRotation = Quaternion.identity;
		bannerObject.transform.localScale = Vector3.one;
		UISingleSprite banner = bannerObject.GetComponent<UISingleSprite>();
		// Size comes from SetSize below, not from texture pixels
		banner.pixelPerfect = false;
		banner.autoResize = false;
		banner.SetTexture(atlas);
		banner.SetAnchor(SpriteRoot.ANCHOR_METHOD.MIDDLE_CENTER);
		banner.SetUVs(uv);
		banner.SetSize(width, height);
		banner.SetColor(Color.white);
		banner.Hide(false);
	}

	private static void RemoveComponents<T>(GameObject target) where T : Component
	{
		foreach (T component in target.GetComponents<T>())
		{
			Object.DestroyImmediate(component);
		}
	}

	/// <summary>
	/// Pick-up key and red banner under a 3D icon (ActivateItem's Button).
	/// Text template is the ActivateItem's own Text (world-space SpriteText on Gui3d); banner template is the Button.
	/// Both are parented to the Button, which faces the camera every frame and deactivates when far away.
	/// </summary>
	public static void AttachPickUpHint(SpriteText itemText, UISingleSprite button)
	{
		Vector3 bannerCenter = PickUpHintLocalPosition + new Vector3(0f, 0f, PickUpBannerBehindZ);
		AttachBanner(button, button.transform, bannerCenter, PickUpBannerWidth, PickUpBannerHeight);
		Attach(itemText, button.transform, PcKeyBindings.PickUpItem, PickUpHintLocalPosition, PickUpHintCharacterSize);
	}
}
