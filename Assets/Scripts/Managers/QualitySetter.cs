using UnityEngine;

public enum CustomTextureQuality
{
	Low,
	Medium,
	High,
	Ultra
}

public class QualitySetter : MonoBehaviourAndLog
{
	public bool TrimmedLevels { get; set; }

	public bool ChromaticAberrations { get; set; }

	public bool BloomEffect { get; set; }

	public bool LowHealthEffect { get; set; }

	public bool VolumeLightOnScene { get; set; }

	public bool GarbageOnScene { get; set; }

	public bool MiscObjectsOnScene { get; set; }

	public bool BloodSplashesOnScene { get; set; }

	public bool DecalsOnScene { get; set; }

	public bool HitEffectsOnScene { get; set; }

	public bool ShellEffectsOnScene { get; set; }

	public bool RagdollsOnScene { get; set; }

	public bool ComplexShadows { get; set; }

	public bool HitSoundsOnScene { get; set; }

	public bool NewZombies { get; set; }

	public bool HudBloodSplashes { get; set; }

	public bool HighQualityAudio { get; set; }

	public bool PrecacheSoundEffects { get; set; }

	public bool AudioLowPassFilterEnabled { get; set; }

	public bool AdvancedUIAnimations { get; set; }

	public float DecalLayerCullDistance { get; private set; }

	public float EyeCandyLayerCullDistance { get; private set; }

	public int ZombiesLimit { get; set; }

	public CustomTextureQuality CustomTextureQuality { get; set; }

	public static bool FinalMissionEnabled
	{
		get
		{
			return iPhone.generation != iPhoneGeneration.iPodTouch4Gen;
		}
	}

	public static bool BootLogoEnabled
	{
		get
		{
			return iPhone.generation != iPhoneGeneration.iPodTouch4Gen;
		}
	}

	public static bool PlayHavenEnabled
	{
		get
		{
			return iPhone.generation != iPhoneGeneration.iPodTouch4Gen;
		}
	}

	public static bool BurstlyEnabled
	{
		get
		{
			return iPhone.generation != iPhoneGeneration.iPodTouch4Gen;
		}
	}

	public static bool TapjoyVideoEnabled
	{
		get
		{
			return iPhone.generation != iPhoneGeneration.iPodTouch4Gen;
		}
	}

	public static bool FacebookEnabled
	{
		get
		{
			return iPhone.generation != iPhoneGeneration.iPodTouch4Gen;
		}
	}

	private void Awake()
	{
		LogTo("Other.Quality");
		Application.targetFrameRate = 60;
		SetupQualitySettings();
		UpdateQualitySetup();
	}

	private void SetupQualitySettings()
	{
		SetupQualitySettingsIPhone();
	}

	private bool ContainsQuality(params string[] list)
	{
		string text = QualitySettings.names[QualitySettings.GetQualityLevel()];
		foreach (string text2 in list)
		{
			if (text2 == text)
			{
				return true;
			}
		}
		return false;
	}

	private bool OffForQualities(params string[] list)
	{
		return !ContainsQuality(list);
	}

	private bool OnForQualities(params string[] list)
	{
		return ContainsQuality(list);
	}

	public void UpdateQualitySetup()
	{
		TrimmedLevels = OnForQualities("iPod4");
		ChromaticAberrations = OnForQualities("iPhone4S", "iPad2", "iPad3", "iPhone5", "AndroidMedium", "AndroidHigh", "PC&Mac");
		BloomEffect = OnForQualities("iPhone4S", "iPad2", "iPad3", "iPhone5", "AndroidMedium", "AndroidHigh", "PC&Mac");
		LowHealthEffect = OnForQualities("iPhone4S", "iPad2", "iPad3", "iPhone5", "AndroidHigh", "PC&Mac");
		VolumeLightOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		GarbageOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		MiscObjectsOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		BloodSplashesOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		DecalsOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		HitEffectsOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		ShellEffectsOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		RagdollsOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		ComplexShadows = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		HitSoundsOnScene = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		NewZombies = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		HudBloodSplashes = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		AdvancedUIAnimations = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		HighQualityAudio = OffForQualities("iPod4");
		PrecacheSoundEffects = OffForQualities("iPod4");
		AudioLowPassFilterEnabled = OffForQualities("iPod4", "iPhone4", "AndroidLow");
		TimeControl.PhysicsFixedTimestep = 0.025f;
		DecalLayerCullDistance = 25f;
		EyeCandyLayerCullDistance = 25f;
		switch (QualitySettings.names[QualitySettings.GetQualityLevel()])
		{
		case "iPod4":
		case "iPad":
			Shader.globalMaximumLOD = 300;
			CustomTextureQuality = CustomTextureQuality.Low;
			ZombiesLimit = 6;
			break;
		case "iPhone4":
		case "AndroidLow":
			Shader.globalMaximumLOD = 400;
			CustomTextureQuality = CustomTextureQuality.Medium;
			ZombiesLimit = 6;
			break;
		case "iPad2":
		case "iPhone4S":
		case "AndroidMedium":
			Shader.globalMaximumLOD = 500;
			CustomTextureQuality = CustomTextureQuality.High;
			ZombiesLimit = 10;
			break;
		case "iPad3":
		case "iPhone5":
		case "AndroidHigh":
		case "PC&Mac":
			Shader.globalMaximumLOD = 600;
			CustomTextureQuality = CustomTextureQuality.Ultra;
			ZombiesLimit = 10;
			break;
		default:
			CustomTextureQuality = CustomTextureQuality.High;
			Shader.globalMaximumLOD = 500;
			ZombiesLimit = 10;
			break;
		}
	}

	private void SetupQualitySettingsEditor()
	{
		SetQualitySettingByName("PC&Mac");
	}

	private void SetupQualitySettingsIPhone()
	{
		switch (iPhone.generation)
		{
		case iPhoneGeneration.iPodTouch4Gen:
			SetQualitySettingByName("iPod4");
			break;
		case iPhoneGeneration.iPad1Gen:
		case iPhoneGeneration.iPhone4:
			SetQualitySettingByName("iPhone4");
			break;
		case iPhoneGeneration.iPhone4S:
		case iPhoneGeneration.iPodTouch5Gen:
		case iPhoneGeneration.iPhoneUnknown:
		case iPhoneGeneration.iPodTouchUnknown:
			SetQualitySettingByName("iPhone4S");
			break;
		case iPhoneGeneration.iPad2Gen:
		case iPhoneGeneration.iPadUnknown:
			SetQualitySettingByName("iPad2");
			break;
		case iPhoneGeneration.iPad3Gen:
			SetQualitySettingByName("iPad3");
			break;
		case iPhoneGeneration.iPhone5:
			SetQualitySettingByName("iPhone5");
			break;
		default:
			SetQualitySettingByName("iPhone4S");
			break;
		}
	}

	private void SetupQualitySettingsAndroid()
	{
		SetQualitySettingByName("AndroidHigh");
	}

	private void SetQualitySettingByName(string qualitySettingName)
	{
		int i;
		for (i = 0; i < QualitySettings.names.Length; i++)
		{
			if (QualitySettings.names[i].Equals(qualitySettingName))
			{
				break;
			}
		}
		if (i >= QualitySettings.names.Length)
		{
			i = 0;
		}
		QualitySettings.SetQualityLevel(i, true);
	}
}
