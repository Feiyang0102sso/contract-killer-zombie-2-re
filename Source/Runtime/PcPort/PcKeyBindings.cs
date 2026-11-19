// PC port: keyboard/mouse key table. Defaults follow the Mac Controls menu (Move/Fire/Zoom/Item/Reload/Weapon Select/1-6/Pick Up).
// Keys are defined only here; HudController.Pc.cs reads them. A future rebinding menu would edit this table.
// Not in the table: Esc for pause (fixed), Left Shift to run (hard-coded in the original Player.cs, kept as is).
using UnityEngine;

public static class PcKeyBindings
{
	// ---- Movement ----
	public static KeyCode MoveForward = KeyCode.W;
	public static KeyCode MoveBackward = KeyCode.S;
	public static KeyCode MoveLeft = KeyCode.A;
	public static KeyCode MoveRight = KeyCode.D;

	// ---- Shooting ----
	/// <summary>Manual fire: hold to fire (one shot per click while zoomed).</summary>
	public static KeyCode ManualFire = KeyCode.Mouse0;
	/// <summary>Sniper zoom on/off.</summary>
	public static KeyCode SniperZoom = KeyCode.Mouse1;
	public static KeyCode WeaponReload = KeyCode.R;

	// ---- Weapons ----
	/// <summary>Open/close the weapon wheel.</summary>
	public static KeyCode WeaponSelect = KeyCode.Space;
	public static KeyCode Melee = KeyCode.Alpha1;
	public static KeyCode Assault = KeyCode.Alpha2;
	public static KeyCode Exotic = KeyCode.Alpha3;
	public static KeyCode Handgun = KeyCode.Alpha4;
	public static KeyCode Sniper = KeyCode.Alpha5;
	public static KeyCode Shotgun = KeyCode.Alpha6;

	// ---- Items ----
	public static KeyCode PreviousItem = KeyCode.Q;
	public static KeyCode NextItem = KeyCode.E;
	/// <summary>Use the selected item (medkit, grenade, ...).</summary>
	public static KeyCode UseItem = KeyCode.U;
	/// <summary>Pick up: same as tapping the nearest 3D icon (ammo crate, bonus box, mission item).</summary>
	public static KeyCode PickUpItem = KeyCode.P;

	// ---- Mouse look ----
	/// <summary>Mouse look scale: 1 = one mouse pixel equals dragging a finger one pixel. The in-game sensitivity still applies.</summary>
	public static float MouseLookScale = 1f;

	/// <summary>
	/// Mouse X / Mouse Y sensitivity in InputManager.asset.
	/// GetAxisRaw("Mouse X") = pixel delta x this value; divide to get pixels.
	/// </summary>
	public const float MouseAxisSensitivity = 0.1f;

	/// <summary>Key name shown on hint labels, written like the Mac Controls menu (ESC, SPACE, LEFT MOUSE, digits, capitals).</summary>
	public static string GetDisplayName(KeyCode key)
	{
		switch (key)
		{
		case KeyCode.Escape:
			return "ESC";
		case KeyCode.Space:
			return "SPACE";
		case KeyCode.Mouse0:
			return "LEFT MOUSE";
		case KeyCode.Mouse1:
			return "RIGHT MOUSE";
		case KeyCode.Mouse2:
			return "MIDDLE MOUSE";
		}
		// Alpha0 ~ Alpha9 show as digits
		if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
		{
			int digit = key - KeyCode.Alpha0;
			return digit.ToString();
		}
		return key.ToString().ToUpperInvariant();
	}
}
