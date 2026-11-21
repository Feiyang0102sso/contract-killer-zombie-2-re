// PC port: tutorial texts that name keys instead of touch gestures (only the wording changes, the tutorial logic doesn't).
// The texts live in the localization files (Assets/localization/<locale>.txt, block "PC port: keyboard/mouse tutorial
// texts"), so they are translated like any other string. This class only maps an original msgid to its PC msgid and
// fills the key names ({0}, {1}...) from PcKeyBindings, so the texts follow any change of the key table.
// Hooked into LocalizationManager.GetString, which serves both code strings (HUD tips) and scene Task texts.
// Off when PcKeyBindings.KeyboardUi is false (touch ports keep the original texts).
using UnityEngine;

public static class PcTutorialTexts
{
	// ---- Original msgids (touch wording) ----
	/// <summary>HUD left tip (HudController.Start), prologue and first level.</summary>
	private const string MoveTipId = "LEFT\nSTICK\nTO MOVE";
	/// <summary>HUD right tip (HudController.Start), prologue and first level.</summary>
	private const string AimTipId = "HOLD TO AIM\nAND MOVE CAMERA";
	/// <summary>Prologue task (Outdoor_Store_Tutorial).</summary>
	private const string AutoFireTaskId = "Target zombies to auto fire";
	/// <summary>Prologue task.</summary>
	private const string HealTaskId = "Use medkit to heal yourself and survivor";
	/// <summary>Prologue task.</summary>
	private const string MeleeTaskId = "Change weapon to bat to save ammo";
	/// <summary>First level task (Outdoor_GasStation_DAY_Story2; also Outdoor_GasStation_DAY).</summary>
	private const string AmmoTaskId = "Tap icon to resupply ammo";
	/// <summary>First level task (also Outdoor_GasStation_DAY).</summary>
	private const string BonusTaskId = "Tap icon to collect bonus";
	/// <summary>First level task (BeaconTutorial).</summary>
	private const string BeaconTaskId = "Use beacon grenade to attract and blow up zombies";
	/// <summary>First survival run task (Outdoor_Survival_Mode, IAmASniperTutorial).</summary>
	private const string SniperTaskId = "Kill zombie with sniper rifle";

	// ---- PC msgids (must match the localization files) ----
	private const string PcMoveTipId = "{0}{1}{2}{3}\nTO MOVE";
	private const string PcAimTipId = "MOUSE TO AIM\n{0} TO FIRE";
	private const string PcAutoFireTaskId = "Hold {0} to shoot";
	private const string PcHealTaskId = "Press {0} to use medkit";
	private const string PcMeleeTaskId = "Press {0} or {1} to switch to bat";
	private const string PcAmmoTaskId = "Press {0} to resupply ammo";
	private const string PcBonusTaskId = "Press {0} to collect bonus";
	/// <summary>The tutorial pauses the game here; HudController.Pc lets the use-item key through for this pause only.</summary>
	private const string PcBeaconTaskId = "Press {0} to throw beacon grenade";
	private const string PcSniperTaskId = "Press {0} for sniper, {1} to zoom";

	/// <summary>Returns the PC msgid for id, or id itself when it isn't a tutorial text (or the keyboard UI is off).</summary>
	public static string MapId(string id)
	{
		if (!PcKeyBindings.KeyboardUi)
		{
			return id;
		}
		switch (id)
		{
		case MoveTipId:
			return PcMoveTipId;
		case AimTipId:
			return PcAimTipId;
		case AutoFireTaskId:
			return PcAutoFireTaskId;
		case HealTaskId:
			return PcHealTaskId;
		case MeleeTaskId:
			return PcMeleeTaskId;
		case AmmoTaskId:
			return PcAmmoTaskId;
		case BonusTaskId:
			return PcBonusTaskId;
		case BeaconTaskId:
			return PcBeaconTaskId;
		case SniperTaskId:
			return PcSniperTaskId;
		}
		return id;
	}

	/// <summary>
	/// Fills the key names into text, the translation of the PC msgid pcId (from MapId).
	/// Returns text unchanged for ids without keys.
	/// </summary>
	public static string FillKeys(string pcId, string text)
	{
		switch (pcId)
		{
		case PcMoveTipId:
			return string.Format(text, Key(PcKeyBindings.MoveForward), Key(PcKeyBindings.MoveLeft), Key(PcKeyBindings.MoveBackward), Key(PcKeyBindings.MoveRight));
		case PcAimTipId:
		case PcAutoFireTaskId:
			return string.Format(text, Key(PcKeyBindings.ManualFire));
		case PcHealTaskId:
		case PcBeaconTaskId:
			return string.Format(text, Key(PcKeyBindings.UseItem));
		case PcMeleeTaskId:
			return string.Format(text, Key(PcKeyBindings.Melee), Key(PcKeyBindings.WeaponSelect));
		case PcAmmoTaskId:
		case PcBonusTaskId:
			return string.Format(text, Key(PcKeyBindings.PickUpItem));
		case PcSniperTaskId:
			return string.Format(text, Key(PcKeyBindings.Sniper), Key(PcKeyBindings.SniperZoom));
		}
		return text;
	}

	private static string Key(KeyCode key)
	{
		return PcKeyBindings.GetDisplayName(key);
	}
}
