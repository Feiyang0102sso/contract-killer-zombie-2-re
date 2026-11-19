// PC port: keyboard/mouse controls for HudController (other half of the partial class: Scripts/UI/Hud/HudController.cs).
// Follows the Mac version: WASD move, mouse look, hold LMB for manual fire (no auto-fire), RMB zoom.
// Other keys call the same methods as the touch buttons; no new gameplay rules. Key table: PcKeyBindings.cs.
//
// Cursor is locked during gameplay (not paused, HUD shown, weapon selector closed);
// otherwise it is released and the mouse drives the UI through PcInput as a fake touch.
//
// Hooks in the original code (marked "PC port"):
//   - HudController.Update: calls UpdatePcControls() after _aimMode is computed;
//   - HudController.GetCharacterMovementInput / GetCameraRotationDeltaAndReset: add WASD and mouse look;
//   - HudController.Start: calls CreatePcKeyHints() at the end;
//   - HudController.OnDestroy: releases the cursor;
//   - Player.Aim: fires when IsPcManualFiring(), like the MiniZombieControls setting.
using System.Collections.Generic;
using UnityEngine;

public partial class HudController
{
	/// <summary>P key search radius (m); matches the distance at which ActivateItem shows its icon.</summary>
	private const float PcPickUpRadius = 20f;

	// ---- Key hint layout (HUD units, relative to the parent). Estimated; tune in game ----
	// z: parent at 0, red banner at -0.5 (in front of parent), text at -1 (in front of banner).
	/// <summary>HUD hint font size (ammo count is 9, RELOAD is 8).</summary>
	private const float PcHintCharacterSize = 7f;
	/// <summary>ESC: below the pause button, text only (as on Mac). Button is 10x10, anchored top-left.</summary>
	private static readonly Vector3 PcHintPauseOffset = new Vector3(5f, -14f, -1f);
	/// <summary>
	/// Moves the ammo row (AmmoBack and the RELOAD blinker) down to make room for the SPACE banner, as on Mac.
	/// Original: ammo panel center (-28.8, -31.2), height 13.
	/// </summary>
	private static readonly Vector3 PcAmmoRowShift = new Vector3(0f, -6f, 0f);
	/// <summary>SPACE: between the weapon icon and the (shifted) ammo row, relative to Weapon.</summary>
	private static readonly Vector3 PcHintWeaponSelectOffset = new Vector3(-28.8f, -22f, -1f);
	/// <summary>R: right below the ammo panel, relative to its center.</summary>
	private static readonly Vector3 PcHintReloadOffset = new Vector3(0f, -12f, -1f);
	/// <summary>SPACE / R banner size (ammo panel is 46 wide).</summary>
	private const float PcBannerWidth = 40f;
	private const float PcBannerHeight = 9f;
	/// <summary>How far the banner sits behind its text (larger z = further back).</summary>
	private const float PcBannerBehindText = 0.5f;
	/// <summary>Q / E: below the item list arrows (arrows are 11 tall).</summary>
	private static readonly Vector3 PcHintItemArrowOffset = new Vector3(0f, -10f, -1f);
	/// <summary>U: below the item icon.</summary>
	private static readonly Vector3 PcHintUseItemOffset = new Vector3(0f, -16f, -1f);
	/// <summary>Weapon wheel slot number: hex slot is 47x43; top-left corner, clear of BUY (top) and the name (bottom).</summary>
	private static readonly Vector3 PcHintWeaponSlotOffset = new Vector3(-16f, 13f, -2f);
	private const float PcHintWeaponSlotCharacterSize = 9f;

	/// <summary>Slot number labels. Slot SetWeapon disables every SpriteText renderer under the slot, so re-show them while the wheel is open.</summary>
	private readonly List<SpriteText> _pcWeaponSlotHints = new List<SpriteText>();

	/// <summary>Manual fire held this frame (read by Player.Aim).</summary>
	private bool _pcManualFire;

	/// <summary>Keyboard/mouse active (cursor locked).</summary>
	private bool _pcCursorLocked;

	/// <summary>For Player.Aim: while manual fire is held, shoot without waiting for a target lock.</summary>
	public bool IsPcManualFiring()
	{
		return _pcManualFire;
	}

	/// <summary>
	/// Creates the key hints next to HUD buttons (called once at the end of Start).
	/// Text template is AmmoLabel: a plain SpriteText with no localization or blinker to overwrite it.
	/// Hints are children of the buttons, so they hide whenever the original code hides the button.
	/// </summary>
	private void CreatePcKeyHints()
	{
		// Banner template: an existing HUD UISingleSprite (bonus popup icon); only its components/material are reused
		UISingleSprite bannerTemplate = _bonus.GetComponentInChildren<UISingleSprite>(true);
		AmmoBack.transform.localPosition += PcAmmoRowShift;
		ReloadLabel.transform.localPosition += PcAmmoRowShift;

		PcKeyHints.Attach(AmmoLabel, _settingsButton.transform, KeyCode.Escape, PcHintPauseOffset, PcHintCharacterSize);

		Vector3 bannerShift = new Vector3(0f, 0f, PcBannerBehindText);
		PcKeyHints.AttachBanner(bannerTemplate, _hudWeapon.transform, PcHintWeaponSelectOffset + bannerShift, PcBannerWidth, PcBannerHeight);
		PcKeyHints.Attach(AmmoLabel, _hudWeapon.transform, PcKeyBindings.WeaponSelect, PcHintWeaponSelectOffset, PcHintCharacterSize);
		PcKeyHints.AttachBanner(bannerTemplate, AmmoBack.transform, PcHintReloadOffset + bannerShift, PcBannerWidth, PcBannerHeight);
		PcKeyHints.Attach(AmmoLabel, AmmoBack.transform, PcKeyBindings.WeaponReload, PcHintReloadOffset, PcHintCharacterSize);

		PcKeyHints.Attach(AmmoLabel, ConsumablesPrevButton.transform, PcKeyBindings.PreviousItem, PcHintItemArrowOffset, PcHintCharacterSize);
		PcKeyHints.Attach(AmmoLabel, ConsumablesNextButton.transform, PcKeyBindings.NextItem, PcHintItemArrowOffset, PcHintCharacterSize);
		PcKeyHints.Attach(AmmoLabel, _consumablesScrollList.transform, PcKeyBindings.UseItem, PcHintUseItemOffset, PcHintCharacterSize);

		AttachPcWeaponSlotHint("Melee", PcKeyBindings.Melee);
		AttachPcWeaponSlotHint("Assault", PcKeyBindings.Assault);
		AttachPcWeaponSlotHint("Exotic", PcKeyBindings.Exotic);
		AttachPcWeaponSlotHint("Handgun", PcKeyBindings.Handgun);
		AttachPcWeaponSlotHint("Sniper", PcKeyBindings.Sniper);
		AttachPcWeaponSlotHint("Shotgun", PcKeyBindings.Shotgun);
	}

	/// <summary>Shows the number key in the top-left of wheel slot Hud/WeaponSelector/Slots/&lt;slotName&gt;.</summary>
	private void AttachPcWeaponSlotHint(string slotName, KeyCode key)
	{
		Transform slot = _hudWeaponSelector.Slots.transform.Find(slotName);
		if (slot == null)
		{
			UnityEngine.Debug.LogWarning("[PcKeyHints] weapon slot not found: " + slotName);
			return;
		}
		SpriteText hint = PcKeyHints.Attach(AmmoLabel, slot, key, PcHintWeaponSlotOffset, PcHintWeaponSlotCharacterSize);
		_pcWeaponSlotHints.Add(hint);
	}

	/// <summary>Called every frame while the wheel is open: slot SetWeapon may have hidden the number labels.</summary>
	private void ShowPcWeaponSlotHints()
	{
		foreach (SpriteText hint in _pcWeaponSlotHints)
		{
			hint.Hide(false);
		}
	}

	/// <summary>Called every frame from Update, after the original _aimMode is computed.</summary>
	private void UpdatePcControls()
	{
		bool inGameplay = !TimeControl.IsPaused() && IsPcHudShown();
		bool selectorShowing = _hudWeaponSelector.IsShowing();
		SetPcCursorLocked(inGameplay && !selectorShowing);
		if (selectorShowing)
		{
			ShowPcWeaponSlotHints();
		}
		_pcManualFire = false;
		if (!inGameplay)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			UnityEngine.Debug.Log("[PcControls] pause");
			Singleton.UI.ShowPauseWindow();
			return;
		}
		UpdatePcWeaponKeys(selectorShowing);
		if (selectorShowing)
		{
			return;
		}
		UpdatePcFire();
		UpdatePcItemKeys();
	}

	/// <summary>Whether the HUD is shown: cutscenes call Show(false), which moves the HUD to z = -1000.</summary>
	private bool IsPcHudShown()
	{
		return transform.localPosition.z > 0f;
	}

	private void SetPcCursorLocked(bool locked)
	{
		if (locked)
		{
			// Set every frame: the OS releases the cursor after alt-tab
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else if (_pcCursorLocked)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		if (locked != _pcCursorLocked)
		{
			UnityEngine.Debug.Log("[PcControls] cursor " + (locked ? "locked" : "released"));
		}
		_pcCursorLocked = locked;
	}

	/// <summary>Fire and zoom.</summary>
	private void UpdatePcFire()
	{
		if (_zoomMode)
		{
			// Zoomed in: one click = one shot, like the touch fire button
			if (Input.GetKeyDown(PcKeyBindings.ManualFire))
			{
				_shootTap++;
			}
		}
		else if (Input.GetKey(PcKeyBindings.ManualFire))
		{
			_pcManualFire = true;
			_aimMode = true;
		}
		// The zoom button only shows for scoped weapons (or while zoomed); the key follows the same rule
		if (Input.GetKeyDown(PcKeyBindings.SniperZoom) && _zoomModeButton.activeSelf)
		{
			SetZoomMode(!_zoomMode);
		}
	}

	/// <summary>Reload, items, pick up.</summary>
	private void UpdatePcItemKeys()
	{
		if (Input.GetKeyDown(PcKeyBindings.WeaponReload))
		{
			_player.Reload();
		}
		if (Input.GetKeyDown(PcKeyBindings.PreviousItem) && ConsumablesPrevButton.gameObject.activeSelf)
		{
			NextConsumable(-1);
		}
		if (Input.GetKeyDown(PcKeyBindings.NextItem) && ConsumablesNextButton.gameObject.activeSelf)
		{
			NextConsumable(1);
		}
		if (Input.GetKeyDown(PcKeyBindings.UseItem) && _consumablesScrollList.gameObject.activeSelf)
		{
			OnConsumableScrollTap();
		}
		if (Input.GetKeyDown(PcKeyBindings.PickUpItem))
		{
			PcPickUpNearest();
		}
	}

	/// <summary>Weapon wheel and number keys.</summary>
	private void UpdatePcWeaponKeys(bool selectorShowing)
	{
		if (Input.GetKeyDown(PcKeyBindings.WeaponSelect))
		{
			if (selectorShowing)
			{
				_hudWeaponSelector.Close();
			}
			else if (_switchButton.activeSelf)
			{
				// Same as the touch switch button (SwitchButtonInputDelegate)
				_hudWeaponSelector.Show();
				UpdateSticksSetup();
			}
			return;
		}
		TryPcSelectWeapon(PcKeyBindings.Melee, PlayerData.Slots.melee, selectorShowing);
		TryPcSelectWeapon(PcKeyBindings.Assault, PlayerData.Slots.assault, selectorShowing);
		TryPcSelectWeapon(PcKeyBindings.Exotic, PlayerData.Slots.exotic, selectorShowing);
		TryPcSelectWeapon(PcKeyBindings.Handgun, PlayerData.Slots.handgun, selectorShowing);
		TryPcSelectWeapon(PcKeyBindings.Sniper, PlayerData.Slots.sniper, selectorShowing);
		TryPcSelectWeapon(PcKeyBindings.Shotgun, PlayerData.Slots.shotgun, selectorShowing);
	}

	private void TryPcSelectWeapon(KeyCode key, PlayerData.Slots slot, bool selectorShowing)
	{
		if (!Input.GetKeyDown(key))
		{
			return;
		}
		// Same conditions as the switch button, but also allowed while zoomed (SelectWeapon leaves zoom itself)
		if (_switchButtonBlocked || _player.IsDyingOrAppearing() || _player.IsMeleeing())
		{
			return;
		}
		Weapon weapon = _player.Inventory.WeaponsBySlot[(int)slot];
		// Same rule as the weapon wheel: weapons with no ammo can't be selected
		if (weapon == null || weapon.IsAmmoEmpty())
		{
			UnityEngine.Debug.Log("[PcControls] slot " + slot + " not selectable");
			return;
		}
		UnityEngine.Debug.Log("[PcControls] select weapon slot " + slot);
		SelectWeapon(slot);
		if (selectorShowing)
		{
			_hudWeaponSelector.Close();
		}
	}

	/// <summary>
	/// P key: same as tapping the nearest 3D icon. Crate/bonus box/mission item models and icons are on the
	/// Gui3d layer; the original tap raycasts them and sends OnUiTap (see AllInputDelegate).
	/// Only colliders with a receiver above them count: player-usable ActivateItem first, then Task (pick-up tasks).
	/// Why: when a tutorial task node ends, PcLegacyActive moves Item_AmmoBox out of its ActivateItem, leaving an
	/// orphan crate model at the same spot; picking by distance alone often hit it and nothing happened.
	/// Range checks are left to ActivateItem / Task.
	/// </summary>
	private void PcPickUpNearest()
	{
		Vector3 playerPosition = _player.transform.position;
		Collider[] colliders = Physics.OverlapSphere(playerPosition, PcPickUpRadius, 1 << Const.LayerGui3d, QueryTriggerInteraction.Collide);
		Collider nearestItem = null;
		float nearestItemSqrDistance = float.PositiveInfinity;
		Collider nearestTask = null;
		float nearestTaskSqrDistance = float.PositiveInfinity;
		foreach (Collider candidate in colliders)
		{
			// Distance to the bounds: Collider.ClosestPoint doesn't support non-convex MeshCollider
			float sqrDistance = (candidate.bounds.ClosestPoint(playerPosition) - playerPosition).sqrMagnitude;
			ActivateItem item = candidate.GetComponentInParent<ActivateItem>();
			if (item != null && !item.activateByNpc)
			{
				if (sqrDistance < nearestItemSqrDistance)
				{
					nearestItemSqrDistance = sqrDistance;
					nearestItem = candidate;
				}
			}
			else if (candidate.GetComponentInParent<Task>() != null)
			{
				if (sqrDistance < nearestTaskSqrDistance)
				{
					nearestTaskSqrDistance = sqrDistance;
					nearestTask = candidate;
				}
			}
		}
		Collider target = nearestItem;
		float targetSqrDistance = nearestItemSqrDistance;
		if (target == null)
		{
			target = nearestTask;
			targetSqrDistance = nearestTaskSqrDistance;
		}
		if (target == null)
		{
			UnityEngine.Debug.Log("[PcControls] pick up: nothing nearby (" + colliders.Length + " Gui3d colliders, none with a receiver)");
			return;
		}
		UnityEngine.Debug.Log("[PcControls] pick up: tap " + target.name + " at " + Mathf.Sqrt(targetSqrDistance).ToString("F1") + "m");
		target.SendMessageUpwards("OnUiTap", target, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>WASD movement, same shape as the left stick: x right, y forward, length at most 1. Zero when no key is held.</summary>
	private Vector2 GetPcMovementInput()
	{
		if (!_pcCursorLocked)
		{
			return Vector2.zero;
		}
		Vector2 movement = Vector2.zero;
		if (Input.GetKey(PcKeyBindings.MoveForward))
		{
			movement.y += 1f;
		}
		if (Input.GetKey(PcKeyBindings.MoveBackward))
		{
			movement.y -= 1f;
		}
		if (Input.GetKey(PcKeyBindings.MoveRight))
		{
			movement.x += 1f;
		}
		if (Input.GetKey(PcKeyBindings.MoveLeft))
		{
			movement.x -= 1f;
		}
		// Diagonal must not be faster than straight
		if (movement.sqrMagnitude > 1f)
		{
			movement.Normalize();
		}
		return movement;
	}

	/// <summary>Mouse look: converts mouse pixels to the stick's UI units (same as dragging a finger the same distance).</summary>
	private Vector2 GetPcMouseLookDelta()
	{
		if (!_pcCursorLocked || _camera == null)
		{
			return Vector2.zero;
		}
		Vector2 pixels = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) / PcKeyBindings.MouseAxisSensitivity;
		// UI camera world width / screen pixel width = UI units per pixel (UpdateSticksSetup measures the screen the same way)
		float uiWidth = -_camera.ScreenToWorldPoint(Vector3.zero).x * 2f;
		float uiUnitsPerPixel = uiWidth / Screen.width;
		return pixels * uiUnitsPerPixel * PcKeyBindings.MouseLookScale;
	}
}
