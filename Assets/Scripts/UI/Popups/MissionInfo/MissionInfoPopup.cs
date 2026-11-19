using Glu.Localization;
using UnityEngine;

public class MissionInfoPopup : UIWindow
{
	private const float topOffset = 19f;

	public SpriteText TitleText;

	public PackedSprite TitleLines;

	public UIButton CloseButton;

	public UIButton StartButton;

	public SpriteText ObjectiveText;

	public SpriteText ObjectiveText2;

	public SpriteText RewardCash;

	public SpriteText RewardHard;

	public SpriteText RewardXP;

	public UIGridLayout RewardsGrid;

	public UIGridLayout BonusGrid;

	public ShopItemWidget RecWeapon1Widget;

	public ShopItemWidget RecWeapon2Widget;

	public UISingleSprite ImageLeft;

	public UISingleSprite ImageRight;

	public UISingleSprite LocationImage;

	protected string _voiceOver = string.Empty;

	protected override void Awake()
	{
		_position = new Vector2(0f, -5.94f);
		base.Awake();
		GUIUtils.HookButton(CloseButton, Close);
		GUIUtils.HookButton(CloseButton, () =>
		{
			Singleton.MainTheme.StopVoiceOver();
		});
	}

	protected override void Start()
	{
		base.Start();
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public virtual void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			if (ImageLeft != null)
			{
				ImageLeft.SetTexture(Resources.Load("LoadingScreens/loading_back_1") as Texture2D);
			}
			if (ImageRight != null)
			{
				ImageRight.SetTexture(Resources.Load("LoadingScreens/loading_back_r") as Texture2D);
			}
		}
		GameData.MissionData missionData = mapNarrative.SelectedMission;
		GameData.MissionDescription selectedMissionDescription = mapNarrative.SelectedMissionDescription;
		GameData.SceneData selectedMissionSceneDescription = mapNarrative.SelectedMissionSceneDescription;
		if (ObjectiveText != null)
		{
			ObjectiveText.Text = Strings.GetString(selectedMissionDescription.Description).Replace("[LOCATION]", selectedMissionSceneDescription != null ? Strings.GetString(selectedMissionSceneDescription.Title) : string.Empty);
			ObjectiveText.gameObject.AddComponent<UIAnimationTypingText>().Setup(50f, 0f, 1f, EZAnimation.EASING_TYPE.Linear).RunOnStart = true;
		}
		if (StartButton != null)
		{
			GUIUtils.HookButton(StartButton, () =>
			{
				PlayerData.Instance.CurrentLocation = mapNarrative.SelectedLocation;
				Singleton.Game.GoToGameLevel(missionData.Scene, missionData.Type);
			});
		}
		if (LocationImage != null)
		{
			GameData.SceneData sceneData = Singleton.GameData.GetSceneData(mapLocation.MissionData.Scene);
			Texture2D texture2D = null;
			if (sceneData != null)
			{
				Object obj = Resources.Load(sceneData.ImagePath);
				if (obj != null)
				{
					texture2D = obj as Texture2D;
				}
			}
			if (texture2D != null)
			{
				LocationImage.SetTexture(texture2D);
			}
			else
			{
				Object.Destroy(LocationImage.gameObject);
			}
		}
	}

	private void Update()
	{
		if (_voiceOver != string.Empty && IsTopmost())
		{
			Singleton.MainTheme.PlayVoiceOver(_voiceOver);
			_voiceOver = string.Empty;
		}
	}
}
