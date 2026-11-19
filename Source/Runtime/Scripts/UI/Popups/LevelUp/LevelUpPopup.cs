using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class LevelUpPopup : UIWindow
{
	public const float EvelynDragCoeff = 3f;

	public const float EvelynMaxDragSpeed = 200f;

	public const float EvelynPostDragDeaccelCoeff = 0.993f;

	private const float SlowAppearanceTime = 0.625f;

	private const float SlowDisappearanceTime = 1.25f;

	private const float SlowAppearanceDelay = 3f;

	private const float SlowImageScaleTime = 5f;

	private const float FastAnimMultiplier = 1f / 3f;

	private const float FastAppearanceTime = 0.20833334f;

	private const float FastDisappearanceTime = 0.4166667f;

	private const float FastAppearanceDelay = 0.20833334f;

	private const float FastImageScaleTime = 1.6666667f;

	public UIButton CloseButton;

	public SpriteText CloseButtonLabel;

	public UIButton FacebookButton;

	public SpriteText MainText;

	public UIExpandSprite Back;

	public UIGridLayout RecWeaponVerticalPlacer;

	public SpriteText RewardsText;

	public Camera Camera3D;

	public UISingleSprite ImageLeft;

	public UISingleSprite ImageRight;

	public UIPressableButton RotationInputButton;

	public GameObject AllUI;

	public Animation LevelUpAnimation;

	private Vector3 _prevDragPos;

	private float _dragSpeed;

	private bool _isDragging;

	public GameObject Evelyn;

	public GameObject Head;

	private UnlockableContent _activeUnlockableContent;

	private List<UnlockableContent> _unlockableContent = new List<UnlockableContent>();

	private bool _newItemsUnlocked;

	private string _newItemShopGroupId = string.Empty;

	private float _appearanceTime = 0.625f;

	private float _disappearanceTime = 1.25f;

	private float _appearanceDelay = 3f;

	private float _imageScaleTime = 5f;

	private CountdownTimer _showNextRewardTimer;

	protected override void Awake()
	{
		base.Awake();
		SoundManager.Preload("UI/level_up");
		Object.Destroy(RotationInputButton.GetComponent<Renderer>());
		RotationInputButton.AddInputDelegate(RotationInputButtonDelegate);
	}

	protected override void Start()
	{
		Back.SetSize(GUIUtils.ScreenWidth() * 1.1f, GUIUtils.ScreenHeight() * 1.1f);
		int level = Singleton.PlayerData.Level;
		MainText.Text = string.Concat(Strings.GetString("YOU ARE NOW A LEVEL"), "  ", Color.red, level.ToString(), GUIUtils.ColorYellowHard, "  ", Strings.GetString("ZOMBIE SLAYER"));
		CloseButton.Text = Strings.GetString("Ok");
		GUIUtils.HookButton(CloseButton, () =>
		{
			_appearanceTime = 0.20833334f;
			_disappearanceTime = 0.4166667f;
			_appearanceDelay = 0.20833334f;
			_imageScaleTime = 1.6666667f;
			if (!ShowNextReward())
			{
				if (_newItemsUnlocked)
				{
					Singleton.UI.Store.Show();
					if (_newItemShopGroupId.Length > 0)
					{
						Singleton.UI.Store.SelectGroup(_newItemShopGroupId);
					}
				}
				Close();
			}
		});
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			ImageLeft.SetTexture(Resources.Load("LoadingScreens/loading_back_1") as Texture2D);
			ImageRight.SetTexture(Resources.Load("LoadingScreens/loading_back_r") as Texture2D);
		}
		bool facebookAvailable = QualitySetter.FacebookEnabled && Singleton.PlayerData.Level >= 3;
		if (facebookAvailable)
		{
			GUIUtils.HookButton(FacebookButton, () =>
			{
				if (!Util.DoRarelyReal(this, "fb_antispam", 1.5f))
				{
					return;
				}
				bool posted = FacebookManager.PostLevelUpMessageToWall(level);
				if (posted)
				{
					Object.Destroy(FacebookButton.gameObject);
				}
			});
		}
		else
		{
			Object.Destroy(FacebookButton.gameObject);
		}
		AllUI.SetActiveRecursively(false);
		Head.transform.localScale = Vector3.zero;
		BuildUnlockableContent();
		base.Start();
	}

	protected override void Activate()
	{
		base.Activate();
		Singleton.MainTheme.StopMainTheme();
		gameObject.AddComponent<MiniScript>().Charge(2f, null, null, null, (MiniScript s) =>
		{
			Head.transform.localScale = Vector3.one;
			SoundManager.Play("UI/level_up", false);
			LevelUpAnimation.Play();
		});
	}

	private void AnimateContentCreation(UnlockableContent unlockableContent)
	{
		unlockableContent.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 0f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear);
		unlockableContent.gameObject.AddComponent<UIAnimationScale>().Setup(0f, 0f, 0f, 0.001f, EZAnimation.EASING_TYPE.Linear);
		GUIUtils.RunAllAnimations(unlockableContent.gameObject);
	}

	private void AnimateContentAppearance(UnlockableContent unlockableContent)
	{
		unlockableContent.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, _disappearanceTime * 0.5f, _appearanceTime, EZAnimation.EASING_TYPE.Linear);
		unlockableContent.gameObject.AddComponent<UIAnimationScale>().Setup(0f, 1f, _disappearanceTime * 0.5f, _appearanceTime, EZAnimation.EASING_TYPE.ExponentialIn);
		if ((bool)unlockableContent.Image)
		{
			unlockableContent.Image.gameObject.AddComponent<UIAnimationScale>().Setup(0.75f, 1f, 0f, _imageScaleTime, EZAnimation.EASING_TYPE.ExponentialOut);
		}
		GUIUtils.RunAllAnimations(unlockableContent.gameObject);
	}

	private void AnimateContentDisappearance(UnlockableContent unlockableContent)
	{
		unlockableContent.gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 0f, 0f, _disappearanceTime, EZAnimation.EASING_TYPE.Linear);
		unlockableContent.gameObject.AddComponent<UIAnimationScale>().Setup(1f, 0f, 0f, _disappearanceTime, EZAnimation.EASING_TYPE.ExponentialIn);
		GUIUtils.RunAllAnimations(unlockableContent.gameObject);
	}

	private UnlockableContent CreateUnlockedItem(CommonShopItem item)
	{
		UnlockableContent component = Util.CreateGameObject("WeaponUnlockPlate").GetComponent<UnlockableContent>();
		component.Init(item);
		AnimateContentCreation(component);
		RecWeaponVerticalPlacer.AddItem(component.gameObject, 0, 0);
		return component;
	}

	private UnlockableContent CreateReward(int rewardSoft)
	{
		UnlockableContent component = Util.CreateGameObject("RewardPlate").GetComponent<UnlockableContent>();
		component.Init(GUIUtils.GetPriceStringForCurrency(rewardSoft, "soft"));
		AnimateContentCreation(component);
		RecWeaponVerticalPlacer.AddItem(component.gameObject, 0, 0);
		return component;
	}

	private UnlockableContent CreateUnlockedZombie(int loadingScreenDescriptor)
	{
		UnlockableContent component = Util.CreateGameObject("ZombieUnlockPlate").GetComponent<UnlockableContent>();
		component.Init(loadingScreenDescriptor);
		AnimateContentCreation(component);
		RecWeaponVerticalPlacer.AddItem(component.gameObject, 0, 0);
		return component;
	}

	private UnlockableContent CreateUnlockedLocation(string locationName, string imageName)
	{
		UnlockableContent component = Util.CreateGameObject("LocationUnlockPlate").GetComponent<UnlockableContent>();
		component.Init(locationName, imageName);
		AnimateContentCreation(component);
		RecWeaponVerticalPlacer.AddItem(component.gameObject, 0, 0);
		return component;
	}

	private void BuildUnlockableContent()
	{
		int level = Singleton.PlayerData.Level;
		int levelUpSC = Singleton.GameData.GetPlayerLevelData(level).LevelUpSC;
		int unlockedLocations = Singleton.GameData.GetPlayerLevelData(level).UnlockedLocations;
		MainText.Text = string.Concat(Strings.GetString("YOU ARE NOW A LEVEL"), "  ", Color.red, level.ToString(), GUIUtils.ColorYellowHard, "  ", Strings.GetString("ZOMBIE SLAYER"));
		_unlockableContent.Add(CreateReward(levelUpSC));
		foreach (GameData.SceneData sceneData in Singleton.GameData.Scenes)
		{
			if (sceneData.UnlockLevel == level)
			{
				_unlockableContent.Add(CreateUnlockedLocation(Strings.GetString(sceneData.Title), sceneData.ImagePath));
			}
		}
		if (!Singleton.Quality.TrimmedLevels)
		{
			for (int j = 0; j < Singleton.GameData.LoadingScreenDescriptors.Length; j++)
			{
				if (Singleton.GameData.LoadingScreenDescriptors[j].ShowAtLevelUpPopup && Singleton.GameData.LoadingScreenDescriptors[j].UnlockLevel == level)
				{
					_unlockableContent.Add(CreateUnlockedZombie(j));
				}
			}
		}
		List<ShopConfigItem> itemsUnlockedOnLevel = Singleton.ShopData.GetItemsUnlockedOnLevel(level);
		List<string> list = new List<string>();
		if (itemsUnlockedOnLevel.Count > 0)
		{
			_newItemsUnlocked = true;
			foreach (ShopConfigItem item2 in itemsUnlockedOnLevel)
			{
				string item = (item2 as CommonShopItem).item;
				if (!list.Contains(item))
				{
					list.Add(item);
					_unlockableContent.Add(CreateUnlockedItem(item2 as CommonShopItem));
					if (_newItemShopGroupId.Length == 0)
					{
						ShopConfigGroup shopConfigGroup = Singleton.ShopData.FindGroupWithItem(item2);
						if (shopConfigGroup != null)
						{
							_newItemShopGroupId = shopConfigGroup.id;
						}
					}
				}
			}
		}
	}

	private bool IsRewardAnimationPlaying()
	{
		if ((bool)_activeUnlockableContent && _activeUnlockableContent.GetComponents<UIAnimation>().Length > 0)
		{
			return true;
		}
		return false;
	}

	private bool ShowNextReward()
	{
		if ((bool)_activeUnlockableContent)
		{
			SoundManager.PlayExclusive("UI/level_up_small_screen", false);
		}
		if ((bool)_activeUnlockableContent && _activeUnlockableContent.GetComponents<UIAnimation>().Length > 0)
		{
			return true;
		}
		if ((bool)_activeUnlockableContent && _unlockableContent.Count > 0)
		{
			AnimateContentDisappearance(_activeUnlockableContent);
		}
		if (_unlockableContent.Count > 0)
		{
			_activeUnlockableContent = _unlockableContent[0];
			_unlockableContent.RemoveAt(0);
			AnimateContentAppearance(_activeUnlockableContent);
			if (_unlockableContent.Count == 0 && _newItemsUnlocked)
			{
				CloseButtonLabel.Text = Strings.GetString("CONTINUE");
			}
			else
			{
				CloseButtonLabel.Text = Strings.GetString("NEXT");
			}
			_showNextRewardTimer = new CountdownTimer();
			_showNextRewardTimer.Run(_appearanceDelay);
			return true;
		}
		_activeUnlockableContent = null;
		return false;
	}

	protected override void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.MainTheme.PlayMainTheme();
		}
	}

	private void RotationInputButtonDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_isDragging = true;
			_prevDragPos = info.ray.origin;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
		{
			float num = (_prevDragPos.x - info.ray.origin.x) * 3f;
			Evelyn.transform.Rotate(0f, num, 0f);
			_dragSpeed = num / Time.deltaTime;
			_dragSpeed = Mathf.Clamp(_dragSpeed, -200f, 200f);
			_prevDragPos = info.ray.origin;
			break;
		}
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			_isDragging = false;
			break;
		}
	}

	private void Update()
	{
		if (!_isDragging)
		{
			Evelyn.transform.Rotate(0f, _dragSpeed * Time.deltaTime, 0f);
			_dragSpeed *= 0.993f;
		}
		if (_showNextRewardTimer != null && !_showNextRewardTimer.IsRunning())
		{
			ShowNextReward();
		}
		Camera3D.enabled = IsTopmost();
	}
}
