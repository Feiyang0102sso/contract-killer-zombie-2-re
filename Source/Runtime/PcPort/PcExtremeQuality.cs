// PC port: the Extreme graphics level (launcher GraphDetail 5), above the original top preset (iPad3, "Ultra").
// Same preset as Ultra, plus three things the original held back for mobile:
//   1. 16x anisotropic filtering on every texture (most textures are at aniso level 1-2): sharper floors at grazing angles;
//   2. post effects (low health, chromatic aberration, bloom) render the scene into a 32-bit, MSAA render texture
//      instead of the original RGB565 one without anti-aliasing (color banding and jaggies while an effect is on);
//   3. decals (blood) and eye candy (garbage, small props) stay visible up to 100 m instead of 25 m.
using UnityEngine;

public static class PcExtremeQuality
{
	private const int AnisotropicLevel = 16;

	/// <summary>Cull distance of the Decal and EyeCandy layers (original: 25 m on every preset).</summary>
	public const float DetailCullDistance = 100f;

	private const int PostEffectDepthBits = 24;

	public static bool Enabled
	{
		get
		{
			return PcLaunchArgs.ExtremeQuality;
		}
	}

	/// <summary>Forces 16x anisotropic filtering. Call after QualitySettings.SetQualityLevel, which resets the mode.</summary>
	public static void ApplyTextureFiltering()
	{
		if (!Enabled)
		{
			return;
		}
		QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
		Texture.SetGlobalAnisotropicFilteringLimits(AnisotropicLevel, AnisotropicLevel);
		Debug.Log("[PcExtremeQuality] anisotropic filtering forced to " + AnisotropicLevel + "x");
	}

	/// <summary>Render texture for the camera while a post effect is on: 32-bit color, 24-bit depth, the preset's MSAA.</summary>
	public static RenderTexture CreatePostEffectTexture(int width, int height)
	{
		RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, PostEffectDepthBits);
		descriptor.msaaSamples = Mathf.Max(1, QualitySettings.antiAliasing);
		Debug.Log("[PcExtremeQuality] post effect texture " + width + "x" + height + " ARGB32, MSAA " + descriptor.msaaSamples);
		return new RenderTexture(descriptor);
	}
}
