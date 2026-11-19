using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class PauseWindow : UIWindow
{
	public UIButton Button;

	public SpriteText Title;

	public UIAlign HoneyAlign;

	public UIGridLayout ButtonsGrid;

	private int _buttonsCount;

	private float spacingY = 6f;

	public SpriteText MissionInfo;

	private HashSet<int> _buttons = new HashSet<int>();

	private float _lastMagic;

	protected override void Awake()
	{
		base.Awake();
		OnShowDelegate += (Action)(() =>
		{
			Array.ForEach(GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
			{
				c.RunAnimation();
			});
		});
	}

	protected override void Start()
	{
		bool isGameScene = Loader.IsGameScene();
		bool flag = false;
		if (isGameScene && Singleton.PlayerData.HasMission(Singleton.PlayerData.CurrentLocation))
		{
			int mission = Singleton.PlayerData.GetMission(Singleton.PlayerData.CurrentLocation);
			flag = !Singleton.PlayerData.IsSpecialHuntReceived && mission == Singleton.ShopData.SpecialHunt.missionIndex;
		}
		AddButton(Strings.GetString("Continue"), () =>
		{
			if (isGameScene)
			{
				Singleton.UI.Hud.SetMissionInfo(null, false);
			}
		});
		AddButton(Strings.GetString("Settings"), () =>
		{
			Singleton.UI.ShowSettingsWindow();
		});
		if (!isGameScene)
		{
			AddButton(Strings.GetString("ICloud"), Singleton.UI.PopupManager.ShowICloudPopup);
		}
		if (!isGameScene)
		{
			AddButton(Strings.GetString("About"), Singleton.UI.ShowAboutWindow);
		}
		if (!isGameScene)
		{
			AddButton(Strings.GetString("Credits"), Singleton.UI.ShowCreditsWindow);
		}
		if (!isGameScene)
		{
			AddButton(Strings.GetString("Privacy/Terms"), () =>
			{
				Application.OpenURL("http://www.glu.com/legal");
			});
		}
		if (isGameScene && Singleton.PlayerData.TutorialState > 2 && !flag)
		{
			AddButton(Strings.GetString("Quit"), Singleton.UI.PopupManager.ShowQuitConfirmationPopup);
		}
		if (!isGameScene || true)
		{
			UnityEngine.Object.Destroy(Title.gameObject);
		}
		float num = (spacingY + Button.height) * (float)_buttonsCount + spacingY;
		ButtonsGrid.Height = num;
		ButtonsGrid.UpdateLayout();
		UIExpandSprite componentInChildren = GetComponentInChildren<UIExpandSprite>();
		componentInChildren.SetSize(componentInChildren.DesiredWidth, num + spacingY * 2f);
		HoneyAlign.UpdateAlignment();
		if (isGameScene && Scene.Mission.GetCurrentTask() != null)
		{
			MissionInfo.Text = Scene.Mission.GetCurrentTask().GetText();
		}
		else
		{
			MissionInfo.Text = string.Empty;
		}
		base.Start();
	}

	private void Update()
	{
	}

	private void AddButton(string text, Action action)
	{
		UIButton uIButton = Button;
		if (_buttonsCount > 0)
		{
			uIButton = (UnityEngine.Object.Instantiate(Button.gameObject) as GameObject).GetComponent<UIButton>();
			uIButton.spriteText.isClone = true;
		}
		ButtonsGrid.AddItem(uIButton.gameObject, 0, _buttonsCount);
		uIButton.spriteText.Text = text;
		GUIUtils.HookButton(uIButton, () =>
		{
			Close();
			action();
		});
		int buttonNum = _buttonsCount;
		GUIUtils.HookButton(uIButton, () =>
		{
			Magic(buttonNum);
		}, POINTER_INFO.INPUT_EVENT.RELEASE_OFF, true);
		_buttonsCount++;
	}

	private void Magic(int buttonNum)
	{
		if (Time.realtimeSinceStartup - _lastMagic > 1f)
		{
			_buttons.Clear();
		}
		_lastMagic = Time.realtimeSinceStartup;
		_buttons.Add(buttonNum);
		if (_buttons.Count == _buttonsCount)
		{
			Close();
			Singleton.UI.ShowSecretWindow();
		}
	}
}
