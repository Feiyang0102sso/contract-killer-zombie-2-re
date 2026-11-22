// PC port: tutorial texts that name keys instead of touch gestures (only the wording changes, the tutorial logic doesn't).
// The texts live in every localization file (block "PC port: keyboard/mouse tutorial texts"), so they are translated
// like any other string; only en and zh-Hans are filled in, the other languages hold English placeholders.
// This class only maps an original msgid to its PC msgid and fills the key names ({0}, {1}...) from PcKeyBindings,
// so the texts follow any change of the key table.
// Hooked into LocalizationManager.GetString, which serves both code strings (HUD tips) and scene Task texts.
// Off when PcKeyBindings.KeyboardUi is false (touch ports keep the original texts).
using System;
using Glu.Localization;
using UnityEngine;

public static class PcTutorialTexts
{
	/// <summary>
	/// Key names (PcKeyBindings.GetDisplayName) that are words, translated inside the sentences.
	/// Translated where the localization file has them (zh-Hans.txt, block "Key name inside the tutorial texts");
	/// other languages show the English name.
	/// </summary>
	private static readonly string[] LocalizedKeyNames = new string[] { "SPACE", "LEFT MOUSE", "RIGHT MOUSE", "MIDDLE MOUSE" };

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
	/// Word key names are translated with manager (LocalizedKeyNames). Returns text unchanged for ids without keys.
	/// </summary>
	public static string FillKeys(string pcId, string text, LocalizationManager manager)
	{
		switch (pcId)
		{
		case PcMoveTipId:
			return string.Format(text, Key(manager, PcKeyBindings.MoveForward), Key(manager, PcKeyBindings.MoveLeft), Key(manager, PcKeyBindings.MoveBackward), Key(manager, PcKeyBindings.MoveRight));
		case PcAimTipId:
		case PcAutoFireTaskId:
			return string.Format(text, Key(manager, PcKeyBindings.ManualFire));
		case PcHealTaskId:
		case PcBeaconTaskId:
			return string.Format(text, Key(manager, PcKeyBindings.UseItem));
		case PcMeleeTaskId:
			return string.Format(text, Key(manager, PcKeyBindings.Melee), Key(manager, PcKeyBindings.WeaponSelect));
		case PcAmmoTaskId:
		case PcBonusTaskId:
			return string.Format(text, Key(manager, PcKeyBindings.PickUpItem));
		case PcSniperTaskId:
			return string.Format(text, Key(manager, PcKeyBindings.Sniper), Key(manager, PcKeyBindings.SniperZoom));
		}
		return text;
	}

	/// <summary>
	/// Key name for a sentence: word names (SPACE, LEFT MOUSE...) come from the localization files (translated in
	/// zh-Hans.txt); letters and digits stay as they are. The HUD key labels keep the English names (PcKeyHints).
	/// </summary>
	private static string Key(LocalizationManager manager, KeyCode key)
	{
		string name = PcKeyBindings.GetDisplayName(key);
		if (Array.IndexOf(LocalizedKeyNames, name) < 0)
		{
			return name;
		}
		// Only zh-Hans.txt has the key names; elsewhere keep the English name instead of "ID ... is not found"
		if (!manager.PcHasString(name))
		{
			return name;
		}
		return manager.GetString(name);
	}
}
