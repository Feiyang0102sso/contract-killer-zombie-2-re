using System;
using System.Collections.Generic;
using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.DynamicContentPipeline;
using Glu.Kontagent;
using Glu.Localization;
using Glu.UnityBuildSystem;
using UnityEngine;

public class UI : MonoBehaviour
{
	private class ActionInfo
	{
		public Action Action;

		public float delay;
	}

	public WindowManager WindowManager;

	public PopupManager PopupManager;

	public SpriteFader Fader;

	public HudController Hud;

	public Material PressStateMaterial;

	public Action OnEnterShop = () =>
	{
	};

	public List<UIWindow> OpenedWindows = new List<UIWindow>();

	private StoreScreen _store;

	private TutorialArrow _arrow;

	private Dictionary<string, List<GameObject>> _uiCache = new Dictionary<string, List<GameObject>>();

	private float _nextActionTime = -1f;

	private float _time;

	private Queue<ActionInfo> _actionQueue = new Queue<ActionInfo>();

	public StoreScreen Store
	{
		get
		{
			if (_store == null)
			{
				CreateShop();
			}
			return _store;
		}
		set
		{
			_store = value;
		}
	}

	public TutorialArrow Arrow
	{
		get
		{
			if (_arrow == null)
			{
				_arrow = Util.CreateGameObject("TutorialArrow").GetComponent<TutorialArrow>();
			}
			return _arrow;
		}
	}

	public MapWindow MapWindow { get; set; }

	private void Awake()
	{
	}

	public void Clear()
	{
		Hud = null;
		Store = null;
		MapWindow = null;
		OpenedWindows.Clear();
		PopupManager.Clear();
		_uiCache.Clear();
	}

	public void PreloadWindows()
	{
		Singleton.Cache.PerResourceCache.Load("Store");
	}

	public void CreateShop()
	{
		GameObject gameObject = Singleton.Cache.PerResourceCache.Create("Store");
		gameObject.GetComponent<UIWindow>().Overlay();
		OnEnterShop();
	}

	public void ShowNotEnoughMoneyWindow(CommonShopItem item)
	{
		App.NoNetworkWarningWasShown = false;
		NotEnoughMoneyWindow component = Util.CreateGameObject("NotEnoughMoneyWindow").GetComponent<NotEnoughMoneyWindow>();
		component.Init(item);
		component.WithFade();
		component.WithPause();
	}

	public void ShowAchievementsWindow()
	{
		AchievementsWindow component = Util.CreateGameObject("AchievementsWindow").GetComponent<AchievementsWindow>();
		component.Overlay();
	}

	public void ShowPauseWindow()
	{
		PauseWindow component = Util.CreateGameObject("PauseWindow").GetComponent<PauseWindow>();
		component.WithFade();
		component.WithPause();
	}

	public void ShowSettingsWindow()
	{
		SettingsWindow component = Util.CreateGameObject("SettingsWindow").GetComponent<SettingsWindow>();
		component.WithFade(0f);
		component.WithPause();
	}

	public void ShowCheatsWindow()
	{
		CheatsWindow component = Util.CreateGameObject("CheatsWindow").GetComponent<CheatsWindow>();
		component.WithFade(0f);
		component.WithPause();
	}

	public void ShowHelpWindow()
	{
		TextWindow component = Util.CreateGameObject("TextWindow").GetComponent<TextWindow>();
		component.WithFade(0f);
		component.WithPause();
		component.SetText(Strings.GetString("Help"), Strings.GetString("IDS_HELP_DIALOG_TEXT"));
		component.ContentText.alignment = SpriteText.Alignment_Type.Left;
	}

	public void ShowAboutWindow()
	{
		TextWindow component = Util.CreateGameObject("TextWindow").GetComponent<TextWindow>();
		component.WithFade(0f);
		component.WithPause();
		string format = Strings.GetString("IDS_ABOUT_DIALOG_TEXT");
		format = string.Format(format, BuildInfo.bundleVersion, Application.unityVersion);
		component.ContentText.alignment = SpriteText.Alignment_Type.Center;
		component.ContentText.anchor = SpriteText.Anchor_Pos.Middle_Center;
		component.SetText(Strings.GetString("About"), format);
	}

	public void ShowCreditsWindow()
	{
		TextWindow component = Util.CreateGameObject("TextWindow").GetComponent<TextWindow>();
		component.WithFade(0f);
		component.WithPause();
		string text = Strings.GetString("IDS_CREDITS_DIALOG_TEXT");
		component.List.alignment = UIScrollList.ALIGNMENT.CENTER;
		component.ContentText.alignment = SpriteText.Alignment_Type.Center;
		component.ContentText.anchor = SpriteText.Anchor_Pos.Middle_Center;
		component.SetText(Strings.GetString("Credits"), text + string.Empty);
	}

	public void ShowSecretWindow()
	{
		TextWindow window = Util.CreateGameObject("TextWindow").GetComponent<TextWindow>();
		window.WithFade(0f);
		window.WithPause();
		string empty = string.Empty;
		empty = empty + "Fps info: " + Singleton.FpsCounter.GetText(true) + "\n";
		empty = empty + "Build tag: " + BuildInfo.buildTag + "\n";
		empty = empty + "Bundle Id: " + BuildInfo.bundleId + "\n";
		IndexInfo assetBundlesIndexInfo = DynamicContent.AssetBundlesIndexInfo;
		if (assetBundlesIndexInfo != null)
		{
			empty = empty + "Bundles TAG: " + assetBundlesIndexInfo.BuildTag + "\n";
		}
		Glu.ABTesting.Resolution aBTestingResolution = DynamicContent.ABTestingResolution;
		if (aBTestingResolution != null)
		{
			empty = empty + "AB group: " + aBTestingResolution.VariantId + "\n";
		}
		window.CloseButton.AddInputDelegate(delegate(ref POINTER_INFO ptr)
		{
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE_OFF && (ptr.ray.origin - window.CloseButton.transform.position).magnitude > 250f)
			{
				Singleton.Game.GoToGameLevel(Singleton.GameData.GetMissionData("FPS_TEST"), true);
			}
		});
		empty += "PH Token: a63d65d508d24eccbe9706766d6ee594\n";
		empty += "PH Secret: aa8d8a9604454629a3a3c74183781e30\n";
		empty += "TJ Id: c631fb4d-a728-4f34-ac42-42f5cb6b9e80\n";
		empty += "TJ App Secret: MCw1DV6xmrO81ah15AAl\n";
		empty += "Flurry key: ZA1BTLN37AZJ25USG9YC\n";
		empty += "Kontagent key: 7a6da10d46fd4f469fa488bbf1c51a06\n";
		empty = empty + "Kontagent user ID: " + Kontagent.GetUserID() + "\n";
		empty += "AssetBundles url: http://glusvn.s3.amazonaws.com/ckzombies2-live\n";
		empty += "Push key: KqYZlpA3QnGPhuAZh-gMqg\n";
		empty += "Push secret: 2WpCQ-X1R_y_ozFwXIxVXg\n";
		empty += "Push master: \n";
		if (PushNotifications.GetInstance().GetDeviceToken() != null)
		{
			empty = empty + "Push token: " + PushNotifications.GetInstance().GetDeviceToken();
		}
		empty += "Rate me url: http://gcs.glu.com/gcs/iphone/iphone.jsp?t=ckzombies2&src=ckzombies2&p=iphone&page=review\n";
		window.SetText(Strings.GetString("Magic"), empty);
		window.ContentText.alignment = SpriteText.Alignment_Type.Left;
		window.OnDestroyDelegate += (Action)(() =>
		{
			Singleton.FpsCounter.Reset();
		});
	}

	public void ShowIAPBuyWindow(IAPShopItem iapItem)
	{
		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			IAPBuyWindow component = Util.CreateGameObject("IAPBuyWindow").GetComponent<IAPBuyWindow>();
			component.WithPause();
			component.WithFade(0f);
			component.Init(iapItem);
		}
		else
		{
			Singleton.UI.PopupManager.ShowIAPNoNetworkPopup();
		}
	}

	public bool Preinstantiate(string resource, int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (!Preinstantiate(resource))
			{
				return false;
			}
		}
		return true;
	}

	public bool Preinstantiate(string resource)
	{
		GameObject gameObject = Util.CreateGameObject(resource);
		if ((bool)gameObject)
		{
			List<GameObject> value = null;
			if (!_uiCache.TryGetValue(resource, out value))
			{
				value = new List<GameObject>();
				_uiCache.Add(resource, value);
			}
			value.Add(gameObject);
		}
		return gameObject != null;
	}

	public int GetNumInstances(string resource)
	{
		List<GameObject> value = null;
		if (_uiCache.TryGetValue(resource, out value))
		{
			return value.Count;
		}
		return 0;
	}

	public GameObject Instantiate(string resource)
	{
		List<GameObject> value = null;
		if (!_uiCache.TryGetValue(resource, out value))
		{
			return Util.CreateGameObject(resource);
		}
		GameObject result = value[0];
		value.RemoveAt(0);
		if (value.Count == 0)
		{
			_uiCache.Remove(resource);
		}
		return result;
	}

	public void AddQueuedAction(Action action, float delay)
	{
		ActionInfo actionInfo = new ActionInfo();
		actionInfo.Action = action;
		actionInfo.delay = delay;
		_actionQueue.Enqueue(actionInfo);
		enabled = true;
	}

	private void Update()
	{
		if (_actionQueue.Count == 0)
		{
			enabled = false;
			return;
		}
		_time += Time.deltaTime / Time.timeScale;
		if (_time > _nextActionTime)
		{
			ActionInfo actionInfo = _actionQueue.Dequeue();
			if (actionInfo != null)
			{
				actionInfo.Action();
				_nextActionTime = _time + actionInfo.delay;
			}
		}
	}
}
