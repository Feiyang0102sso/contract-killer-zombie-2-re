// PC port: restores the Unity 3.5 2D/3D sound rule.
//
// In 3.5 a clip was 2D or 3D by its import setting, and AudioSource.panLevel only affected 3D clips.
// Unity 6 decides by AudioSource.spatialBlend alone, and the API updater mapped panLevel straight to it,
// so the GameData presets (PanLevel = 1) made the original 2D sounds fully 3D: UI, mission, power-up and
// voice-over sounds played at a stale position with a 25 m linear rolloff and were silent.
// The 2D table is generated from the original import settings by audio_2d_extract.py (PcAudio.Generated.cs).
using UnityEngine;

public static partial class PcAudio
{
	/// <summary>Default AudioSource.panLevel in 3.5 (a fresh Unity 6 AudioSource has spatialBlend 0 instead).</summary>
	public const float LegacyDefaultPanLevel = 1f;

	/// <summary>Called after the preset is applied: sounds that were 2D in 3.5 ignore panLevel, so play them fully 2D.</summary>
	public static void ApplyLegacyClipMode(AudioSource source, string soundName)
	{
		if (Legacy2DSounds.Contains(soundName.ToLowerInvariant()))
		{
			source.spatialBlend = 0f;
		}
	}
}
