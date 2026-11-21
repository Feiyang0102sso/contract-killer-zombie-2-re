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
			return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen;
		}
	}

	public static bool BootLogoEnabled
	{
		get
		{
			return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen;
		}
	}

	public static bool PlayHavenEnabled
	{
		get
		{
			return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen;
		}
	}

	public static bool BurstlyEnabled
	{
		get
		{
			return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen;
		}
	}

	public static bool TapjoyVideoEnabled
	{
		get
		{
			return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen;
		}
	}

	public static bool FacebookEnabled
	{
		get
		{
			return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen;
		}
	}

	private void Awake()
	{
		LogTo("Other.Quality");
		Application.targetFrameRate = 60;
		SetupQualitySettings();
#if UNITY_STANDALONE
		// PC port: no frame cap and VSync from the launcher (after SetQualityLevel, which resets vSyncCount)
		PcLaunchArgs.ApplyFrameSettings();
		// PC port: Extreme level, also after SetQualityLevel
		PcExtremeQuality.ApplyTextureFiltering();
#endif
		UpdateQualitySetup();
	}

	private void SetupQualitySettings()
	{
#if UNITY_STANDALONE
		// PC port: the preset comes from the launcher's GraphDetail instead of the device generation
		SetQualitySettingByName(PcLaunchArgs.QualityPresetName);
#else
		SetupQualitySettingsIPhone();
#endif
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
#if UNITY_STANDALONE
		// PC port: Extreme level draws decals and eye candy farther
		if (PcExtremeQuality.Enabled)
		{
			DecalLayerCullDistance = PcExtremeQuality.DetailCullDistance;
			EyeCandyLayerCullDistance = PcExtremeQuality.DetailCullDistance;
		}
#endif
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
		switch (UnityEngine.iOS.Device.generation)
		{
		case UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen:
			SetQualitySettingByName("iPod4");
			break;
		case UnityEngine.iOS.DeviceGeneration.iPad1Gen:
		case UnityEngine.iOS.DeviceGeneration.iPhone4:
			SetQualitySettingByName("iPhone4");
			break;
		case UnityEngine.iOS.DeviceGeneration.iPhone4S:
		case UnityEngine.iOS.DeviceGeneration.iPodTouch5Gen:
		case UnityEngine.iOS.DeviceGeneration.iPhoneUnknown:
		case UnityEngine.iOS.DeviceGeneration.iPodTouchUnknown:
			SetQualitySettingByName("iPhone4S");
			break;
		case UnityEngine.iOS.DeviceGeneration.iPad2Gen:
		case UnityEngine.iOS.DeviceGeneration.iPadUnknown:
			SetQualitySettingByName("iPad2");
			break;
		case UnityEngine.iOS.DeviceGeneration.iPad3Gen:
			SetQualitySettingByName("iPad3");
			break;
		case UnityEngine.iOS.DeviceGeneration.iPhone5:
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
