using System;
using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

public class Loading : MonoBehaviourAndLog
{
	public const float FadeInTime = 0.5f;

	public const float FadeOutTime = 1.5f;

	public static bool IsReady = true;

	private bool _isWaitingForInput;

	private bool _waitFewUpdates;

	private int _updatesBeforeTakingInput;

	public Texture2D DefaultBackgroundTexture;

	public string ImageLeftTextureName;

	public string ImageRightTextureName;

	public Texture2D ImageRadarTexure;

	public Texture2D ImageConsumableTexture;

	public Texture2D ImageArrowTexture;

	public UISingleSprite ImageLeft;

	public UISingleSprite ImageRight;

	public UISingleSprite ImageConsumable;

	public UISingleSprite ImageLeftArrow;

	public UISingleSprite ImageRightArrow;

	public UISingleSprite ImageCharacter;

	public UISingleSprite ImageLoadingLogo1;

	public UISingleSprite ImageLoadingLogo2;

	public UISingleSprite ImageLoadingGlow;

	public SpriteText TipLabel;

	public SpriteText RedHeader;

	public SpriteText RedText;

	public SpriteText WhiteHeader;

	public SpriteText WhiteText;

	public SpriteText LoadingText;

	public SpriteText StoryProgressText;

	public SpriteText StoryProgressTitle;

	private BoxCollider _box;

	private AsyncOperation _loading;

	private float _loadingTime;

	private float _prevLogoRotationTime;

	private float _prevGlowFadeTime;

	private void Awake()
	{
		WindowManager.Instance.CloseAllWindows();
		LogTo("Other.Loader");
		UIManager.instance.AddMouseTouchPtrListener(AllInputDelegate);
		_box = gameObject.AddComponent<BoxCollider>();
		_box.size = new Vector3(500f, 500f, 100f);
		_box.center = new Vector3(0f, 0f, 0f);
		_box.isTrigger = true;
	}

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		CheckLoadTipTextMargin();
		float num = GUIUtils.ScreenWidth();
		GameData.LoadingScreenDescriptor loadingScreenDescriptor = ChooseDescriptor();
		float x = -num / 2f + ImageLeft.width / 2f;
		float x2 = num / 2f - ImageRight.width / 2f;
		Texture2D texture2D = DefaultBackgroundTexture;
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			texture2D = Resources.Load(IPadPathHack(loadingScreenDescriptor.PicturePath)) as Texture2D;
			if (texture2D == null)
			{
				throw new Exception(string.Format("Picture \"{0}\" not found.", loadingScreenDescriptor.PicturePath));
			}
		}
		Texture2D texture = DefaultBackgroundTexture;
		Texture2D texture2 = DefaultBackgroundTexture;
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			texture = Resources.Load("LoadingScreens/" + ImageLeftTextureName) as Texture2D;
			texture2 = Resources.Load("LoadingScreens/" + ImageRightTextureName) as Texture2D;
		}
		ImageConsumable.Hide(true);
		InitImage(ImageLeft, texture, x);
		InitImage(ImageRight, texture2, x2);
		InitConsumableImage(loadingScreenDescriptor.WhiteHeader);
		InitCharacImage(ImageCharacter, texture2D, loadingScreenDescriptor.PicturePath);
		InitLogoImage(ImageLoadingLogo1, (Texture2D)Resources.Load("LoadingScreens/loading_logo_frame_01"), x2);
		InitLogoImage(ImageLoadingLogo2, (Texture2D)Resources.Load("LoadingScreens/loading_logo_frame_02"), x2);
		InitLogoImage(ImageLoadingGlow, (Texture2D)Resources.Load("LoadingScreens/loading_glow"), x2);
		InitText(WhiteHeader, loadingScreenDescriptor.WhiteHeader);
		InitText(WhiteText, loadingScreenDescriptor.WhiteText);
		InitText(RedHeader, loadingScreenDescriptor.RedHeader);
		InitText(RedText, loadingScreenDescriptor.RedText);
		InitDynamicText(StoryProgressText, string.Format("{0}/{1}", Singleton.PlayerData.StorylineTicket, Singleton.GameData.GetStorylineLength()));
		TipLabel.Hide(true);
		IsReady = false;
		Singleton.Loader.PreLoad();
		_loading = Application.LoadLevelAsync(Singleton.Loader.LevelToLoad.sceneName + (Singleton.Quality.TrimmedLevels && Loader.IsGameScene(new LevelInfo(Singleton.Loader.LevelToLoad.sceneName)) ? "_trimmed" : string.Empty));
		TimeControl.Pause(true, TimeControl.PauseTypes.Loading);
	}

	private string IPadPathHack(string texturePath)
	{
		bool isIPad = UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad1Gen || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad2Gen ||
			UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad3Gen || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadUnknown;
		if (isIPad && texturePath.Contains("Hold"))
		{
			return texturePath.Replace("Hold", "Hold_IPad");
		}
		return texturePath;
	}

	private void Update()
	{
		float unusedLocal1;
		float unusedLocal2;
		_loadingTime += Time.deltaTime / Time.timeScale;
		if (_waitFewUpdates && _updatesBeforeTakingInput > 0)
		{
			_updatesBeforeTakingInput--;
			if (_updatesBeforeTakingInput <= 0)
			{
				ActivateInputWait();
				_waitFewUpdates = false;
			}
		}
		if (_loading != null)
		{
			int num = (int)Math.Round(_loadingTime / 0.1f);
			int num2 = 60 * num % 360;
			ImageLoadingLogo1.transform.rotation = Quaternion.Euler(0f, 0f, -num2);
			ImageLoadingGlow.Color = CalculateFadeAlpha(0.5f);
			if (_loading.isDone)
			{
				OnLoadingFinished();
			}
		}
		else
		{
			TipLabel.Color = CalculateFadeAlpha(0.5f);
		}
		float unusedLocal5;
	}

	private Color CalculateFadeAlpha(float fadeTime)
	{
		float num = 1f / fadeTime;
		int num2 = (int)Math.Floor(_loadingTime * num);
		float num3 = _loadingTime - fadeTime * (float)num2;
		float num4 = num3 * num;
		return new Color(1f, 1f, 1f, !(num2 % 2 != 0) ? num4 : (1f - num4));
	}

	public void AllInputDelegate(POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			if (_isWaitingForInput)
			{
				_isWaitingForInput = false;
				Singleton.UI.Fader.FadeOut(1.5f, Color.black);
				UnityEngine.Object.DestroyObject(gameObject, 0f);
				_box.size = Vector3.zero;
				TimeControl.Pause(false, TimeControl.PauseTypes.Loading);
				IsReady = true;
			}
			break;
		}
	}

	private void OnLoadingFinished()
	{
		if (Singleton.Loader.CustomLoadSteps != null)
		{
			foreach (Loader.CustomLoadStep customLoadStep in Singleton.Loader.CustomLoadSteps)
			{
				customLoadStep();
			}
			Singleton.Loader.CustomLoadSteps = null;
		}
		TimeControl.Pause(true, TimeControl.PauseTypes.Loading);
		Singleton.Loader.PostLoad();
		Singleton.Loader.OnLoadingFinished();
		_updatesBeforeTakingInput = 5;
		_waitFewUpdates = true;
		_loading = null;
	}

	private void ActivateInputWait()
	{
		TipLabel.Hide(false);
		ImageLoadingLogo1.Hide(true);
		ImageLoadingLogo2.Hide(true);
		ImageLoadingGlow.Hide(true);
		LoadingText.Hide(true);
		_isWaitingForInput = true;
	}

	private GameData.LoadingScreenDescriptor ChooseDescriptor()
	{
		Mission.Type missionType = Singleton.MetaMission.missionType;
		GameData.LoadingScreenType currentScreenType = (Singleton.Loader.LevelToLoad.sceneName == "CityMap" ? GameData.LoadingScreenType.AfterLevel : GameData.LoadingScreenType.BeforeLevel);
		GameData.LoadingScreenDescriptor loadingScreenDescriptor = Singleton.GameData.LoadingScreenDescriptors.Where((GameData.LoadingScreenDescriptor desc) => desc.IsMissionPredefined && desc.PredefinedMission == missionType && desc.Type == currentScreenType).FirstOrDefault();
		if (loadingScreenDescriptor != null)
		{
			return loadingScreenDescriptor;
		}
		Mission.Type ticketMission;
		try
		{
			ticketMission = (Mission.Type)(int)Enum.Parse(typeof(Mission.Type), "Story" + (Singleton.PlayerData.StorylineTicket + 1));
		}
		catch
		{
			ticketMission = Mission.Type.Story25;
		}
		IEnumerable<GameData.LoadingScreenDescriptor> first = Singleton.GameData.LoadingScreenDescriptors.Where((GameData.LoadingScreenDescriptor desc) => !desc.IsMissionPredefined);
		IEnumerable<GameData.LoadingScreenDescriptor> second = Singleton.GameData.LoadingScreenDescriptors.Where((GameData.LoadingScreenDescriptor desc) => desc.IsMissionPredefined && desc.PredefinedMission < ticketMission);
		GameData.LoadingScreenDescriptor[] array = first.Concat(second).ToArray();
		return Util.RandomFrom(array);
	}

	private void InitDynamicText(SpriteText text, string value)
	{
		text.maxWidth = Math.Max(GUIUtils.ScreenWidth() / 2f, GUIUtils.ScreenWidth() - ImageCharacter.width);
		text.Text = value;
	}

	private void InitText(SpriteText text, string value)
	{
		UIAlign component = text.GetComponent<UIAlign>();
		text.maxWidth = GUIUtils.ScreenWidth() / 2f - 10f;
		text.Text = Strings.GetString(value);
		if (GUIUtils.Aspect() > 1.5f)
		{
			component.m_margin.x -= 20f;
			component.UpdateAlignment();
		}
	}

	private static void InitImage(UISingleSprite image, Texture2D texture, float x)
	{
		Vector3 position = image.transform.position;
		image.transform.position = new Vector3(x, position.y, position.z);
		image.SetTexture(texture);
	}

	private void InitLogoImage(UISingleSprite image, Texture2D texture, float x)
	{
		Vector3 position = image.transform.position;
		if (GUIUtils.Aspect() > 1.5f)
		{
			x -= 20f;
		}
		image.transform.position = new Vector3(x, position.y, position.z);
		image.SetTexture(texture);
	}

	private void CheckLoadTipTextMargin()
	{
		if (GUIUtils.Aspect() > 1.5f)
		{
			UIAlign component = LoadingText.GetComponent<UIAlign>();
			UIAlign component2 = TipLabel.GetComponent<UIAlign>();
			component.m_margin.x -= 20f;
			component2.m_margin.x -= 20f;
			component.UpdateAlignment();
			component2.UpdateAlignment();
		}
	}

	private void InitCharacImage(UISingleSprite image, Texture2D texture, string name)
	{
		UIAlign component = image.GetComponent<UIAlign>();
		image.SetTexture(texture);
		if (name.IndexOf("Thrower") > -1)
		{
			component.m_margin = new Vector2(30f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Bomber") > -1)
		{
			component.m_margin = new Vector2(20f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Charger") > -1)
		{
			component.m_margin = new Vector2(10f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Evelyn") > -1)
		{
			component.m_margin = new Vector2(20f, 0f);
			image.SetSize(0.95f * ToScreenDimensions(texture.width), 0.95f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Piper") > -1)
		{
			component.m_margin = new Vector2(5f, 15f);
			image.SetSize(ToScreenDimensions(texture.width), ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Shhazamat") > -1)
		{
			component.m_margin = new Vector2(30f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Shsecurity") > -1)
		{
			component.m_margin = new Vector2(30f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Spitter") > -1)
		{
			component.m_margin = new Vector2(30f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Wallis") > -1)
		{
			component.m_margin = new Vector2(30f, 0f);
			image.SetSize(0.95f * ToScreenDimensions(texture.width), 0.95f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Fireman") > -1)
		{
			component.m_margin = new Vector2(30f, 10f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Hold") > -1)
		{
			component.m_margin = new Vector2(5f, 5f);
			image.SetSize(ToScreenDimensions(texture.width), ToScreenDimensions(texture.height));
		}
		else if (name.IndexOf("Wheel") > -1)
		{
			component.m_margin = new Vector2(5f, 35f);
			image.SetSize(0.9f * ToScreenDimensions(texture.width), 0.9f * ToScreenDimensions(texture.height));
		}
		if (GUIUtils.Aspect() > 1.5f)
		{
			component.m_margin.x += 20f;
		}
		component.UpdateAlignment();
	}

	private void InitConsumableImage(string header)
	{
		if (header == "CONSUMABLE")
		{
			ImageConsumable.Hide(false);
			ImageLeftArrow.Hide(false);
			ImageRightArrow.Hide(false);
			InitImage(ImageConsumable, ImageConsumableTexture, ImageConsumable.transform.position.x);
			InitImage(ImageLeftArrow, ImageArrowTexture, ImageLeftArrow.transform.position.x);
			InitImage(ImageRightArrow, ImageArrowTexture, ImageRightArrow.transform.position.x);
		}
		else if (header == "NAVIGATION")
		{
			ImageConsumable.Hide(false);
			ImageRightArrow.Hide(true);
			ImageLeftArrow.Hide(true);
			ImageConsumable.width = 45f;
			InitImage(ImageConsumable, ImageRadarTexure, ImageConsumable.transform.position.x);
		}
		else
		{
			ImageConsumable.Hide(true);
			ImageRightArrow.Hide(true);
			ImageLeftArrow.Hide(true);
		}
	}

	private static float ToScreenDimensions(float size)
	{
		float unusedLocal1;
		return GUIUtils.ScreenHeight() * size / 1024f;
	}
}
