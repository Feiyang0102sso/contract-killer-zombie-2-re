using System;
using System.Collections;
using Glu;
using Glu.DynamicContentPipeline;
using Glu.Localization;
using UnityEngine;

public class MapWindow : UIWindow
{
	private struct ButtonData
	{
		public OnButtonTap onButtonTap;

		public object data;

		private ButtonData(OnButtonTap onButtonTap_, object data_)
		{
			onButtonTap = onButtonTap_;
			data = data_;
		}
	}

	private delegate void OnButtonTap(object data);

	public GameObject ShopButtonSaleBadge;

	public SpriteText SaleText;

	public UIButton ShopButton;

	public UIButton ShieldButton;

	private SoundManager _soundManager;

	private TutorialTip _tip;

	protected override void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.MainTheme.StopMainTheme();
		}
		if (App.Exists() && App.Instance.FlushCachedAnalytics)
		{
			App.Instance.FlushCachedAnalytics = false;
		}
		base.OnDestroy();
	}

	protected override void Awake()
	{
		base.Awake();
		Singleton.UI.MapWindow = this;
		_soundManager = Util.CreateGameObject("SoundManager").GetComponent<SoundManager>();
		_soundManager.transform.parent = transform;
		foreach (EZCameraSettings eZCameraSettings in UIManager.instance.uiCameras)
		{
			AudioListener component = eZCameraSettings.camera.GetComponent<AudioListener>();
			if ((bool)component)
			{
				component.enabled = true;
			}
		}
		if (!App.Instance.FlushCachedAnalytics)
		{
			App.Instance.FlushCachedAnalytics = true;
		}
		Singleton.ShopData.offersConfig.specialHunt.UpdateState();
	}

	protected override void Start()
	{
		base.Start();
		GUIUtils.HookButton(ShopButton, Singleton.UI.CreateShop);
		StartCoroutine("LevelUp");
		Singleton.MainTheme.PlayMainTheme();
		Singleton.Leaderboards.UpdateLeaderboards();
		StartCoroutine(DynamicContentCoroutine());
	}

	private IEnumerator LevelUp()
	{
		while (true)
		{
			if (Loading.IsReady)
			{
				ShowInitialPopups();
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GUIUtils.InputEnabled = true;
	}

	private IEnumerator DynamicContentCoroutine()
	{
		while (true)
		{
			if (!DynamicContent.IsUpdateInProgress() && DynamicContent.CheckForUpdates())
			{
				switch (DynamicContent.LastCheckReport.updateScope)
				{
				case DynamicContent.CheckReport.UpdateScope.ForcedUpdate:
					PopupManager.ShowForcedUpdateAppDialog();
					yield break;
				case DynamicContent.CheckReport.UpdateScope.CustomDynamicContent:
					DynamicContent.OnForcedBinariesUpdate = null;
					DynamicContent.OnSuccess = null;
					DynamicContent.OnFail = null;
					DynamicContent.StartContentUpdate();
					break;
				default:
					Singleton.UI.PopupManager.ShowRestartForUpdateAppDialog();
					yield break;
				}
			}
			float t1 = Time.realtimeSinceStartup + 10f;
			while (t1 > Time.realtimeSinceStartup)
			{
				yield return null;
			}
		}
	}

	private void ShowGoalAnimation()
	{
		Singleton.UI.PopupManager.ShowGoalAnimationPopup(GetComponentInChildren<CommonGUI>());
	}

	private void ShowInitialPopups()
	{
		Singleton.UI.PopupManager.ClearQueue();
		if (Tutorial.IsRunning())
		{
			Singleton.Tutorial.State.ShowCityMapPopup();
		}
		if (PlayerData.Instance.LevelUpState == 2)
		{
			PlayerData.Instance.LevelUpState = 1;
			Singleton.UI.PopupManager.ShowLevelUpPopup();
			ShowGoalAnimation();
			if (!Singleton.PlayerData.RateMeAccepted && Array.Exists(Singleton.GameData.RateMeLevels, (int l) => l == Singleton.PlayerData.Level))
			{
				Singleton.UI.PopupManager.ShowRateMePopup();
			}
		}
		else if (PlayerData.Instance.LevelUpState == 1)
		{
			PlayerData.Instance.LevelUpState = 0;
			ShowGoalAnimation();
			CommonShopItem newAvailableItemToNotify = Singleton.ShopData.GetNewAvailableItemToNotify();
			if (newAvailableItemToNotify != null)
			{
				Singleton.UI.PopupManager.ShowNewItemAvailablePopup(newAvailableItemToNotify);
			}
			if (QualitySetter.FacebookEnabled && Singleton.PlayerData.Level == 3 && !FacebookManager.WasLoggedIn())
			{
				Singleton.UI.PopupManager.ShowFacebookLoginForRewardPopup();
			}
		}
		else
		{
			ShowGoalAnimation();
		}
		if (Singleton.PlayerData.StorylineTicket > 1 && Singleton.Game.PlayerWasInGame && DailyBonus.GetUnclaimedConsecutiveDays() > 0)
		{
			int unclaimedConsecutiveDays = DailyBonus.GetUnclaimedConsecutiveDays();
			Singleton.UI.PopupManager.ShowDailyBonusPopup(unclaimedConsecutiveDays - 1);
			DailyBonus.Acknowledge();
		}
		if (!Singleton.PlayerData.IsSpecialHuntReceived && Singleton.PlayerData.IsSpecialHuntPlayed)
		{
			Singleton.UI.PopupManager.ShowProposalAfterDeathPopup(Singleton.ShopData.SpecialHunt.saleWeapon, Singleton.ShopData.SpecialHunt.discountPercents);
			Singleton.PlayerData.IsSpecialHuntReceived = true;
			BroadcastMessage("SpecialHuntStopped", SendMessageOptions.RequireReceiver);
		}
		else if (Singleton.Game.PlayerWasInGame && ProposalAfterDeathPopup.ShouldBeShown())
		{
			Singleton.UI.PopupManager.ShowProposalAfterDeathPopup();
		}
		int lastPlayedMissionIndex = Singleton.PlayerData.LastPlayedMissionIndex;
		if (lastPlayedMissionIndex >= 0)
		{
			GameData.MissionData missionData = Singleton.GameData.Missions[lastPlayedMissionIndex];
			if (Singleton.GameData.Ckz1BannerShowAfterLevel == missionData.Type)
			{
				Singleton.UI.PopupManager.ShowCkz1BannerPopup();
			}
		}
		if (Singleton.ShopData.LimitedTimeOffer.IsRunning() && !Singleton.PlayerData.IsLimitedTimeOfferPopupShown)
		{
			Singleton.UI.PopupManager.ShowLimitedTimeOfferWelcomePopup();
		}
		Singleton.UI.PopupManager.ShowMapWindowAnimationController();
		Singleton.UI.PopupManager.ShowPlayhavenPopup();
	}

	private void OnTapjoyPointsReceived(int points)
	{
		Singleton.PlayerData.HardCurrency += points;
		Singleton.UI.PopupManager.ShowInfoPopup(string.Format(Strings.GetString("You received {0}."), GUIUtils.GetPriceStringForCurrency(points, "hard")));
		GameAnalytics.LogTapjoyPointsReceived(points, "Hard");
		GameAnalytics.LogHCReceived(points, GameAnalytics.HCSourceType.Tapjoy);
	}

	private void Update()
	{
		uint points = Tapjoy.GetPoints();
		if (points > 0)
		{
			int points2 = (int)points;
			OnTapjoyPointsReceived(points2);
			Tapjoy.ConsumePoints(points);
		}
		if (Util.DoRarely(this, "check_sale", 1f))
		{
			int itemsCountOnSaleWithTimer = Singleton.ShopData.GetItemsCountOnSaleWithTimer();
			ShopButtonSaleBadge.gameObject.SetActiveRecursively(itemsCountOnSaleWithTimer > 0);
			SaleText.Text = itemsCountOnSaleWithTimer.ToString();
		}
		if (Util.DoRarely(this, "lto_state_update", 1f))
		{
			Singleton.ShopData.offersConfig.limitedTimeOffer.UpdateState();
		}
		if (Util.DoRarely(this, "check_special_hunt", 1f))
		{
			OffersConfig.SpecialHunt.UpdateStateResult updateStateResult = Singleton.ShopData.offersConfig.specialHunt.UpdateState();
			if (updateStateResult == OffersConfig.SpecialHunt.UpdateStateResult.HuntStopped)
			{
				BroadcastMessage("SpecialHuntStopped", SendMessageOptions.RequireReceiver);
			}
		}
	}

	public void ShowTip(string text)
	{
		if (_tip == null)
		{
			_tip = Util.CreateGameObject("TutorialTip").GetComponent<TutorialTip>();
			_tip.transform.parent = transform;
			_tip.AutoDestroy = false;
			_tip.GetComponentInChildren<UITextBlinker>().Freq = 3f;
		}
		_tip.SetTip(text, new Vector3(-39.52f, -88.8f, transform.position.z - 8f));
	}

	public void HideTip()
	{
		if ((bool)_tip)
		{
			UnityEngine.Object.Destroy(_tip.gameObject);
		}
	}
}
