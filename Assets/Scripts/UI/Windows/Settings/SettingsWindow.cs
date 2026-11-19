using System;
using Glu.Localization;
using UnityEngine;

public class SettingsWindow : UIWindow
{
	private const float textPosX = -72.5f;

	public UIButton AcceptButton;

	public UIButton CancelButton;

	public UIButton HelpButton;

	public UIGridLayout ElementsLayout;

	private int _settingsCount;

	private Action OnSave = () =>
	{
	};

	private Action OnCancel = () =>
	{
	};

	protected override void Awake()
	{
		base.Awake();
		AddSlider(Strings.GetString("Sound"), Singleton.Settings.SoundVolume, (float val) =>
		{
			Singleton.Settings.SoundVolume = val;
		});
		AddSlider(Strings.GetString("Music"), Singleton.Settings.MusicVolume, (float val) =>
		{
			Singleton.Settings.MusicVolume = val;
		});
		AddSlider(Strings.GetString("Sensitivity"), Singleton.Settings.Sensitivity / 2f, (float val) =>
		{
			Singleton.Settings.Sensitivity = val * 2f;
		});
		AddToggle(Strings.GetString("Notifications"), Singleton.Settings.Notifications, (bool val) =>
		{
			Singleton.Settings.Notifications = val;
		});
		AddToggle(Strings.GetString("Alternative fire"), Singleton.Settings.MiniZombieControls, (bool val) =>
		{
			Singleton.Settings.MiniZombieControls = val;
		});
		AddToggle(Strings.GetString("Left side swipe to turn"), Singleton.Settings.UselessSwipe, (bool val) =>
		{
			Singleton.Settings.UselessSwipe = val;
		});
		AddToggle(Strings.GetString("Invert Y axis"), Singleton.Settings.InvertYAxis, (bool val) =>
		{
			Singleton.Settings.InvertYAxis = val;
		});
		GUIUtils.HookButton(CancelButton, () =>
		{
			OnCancel();
			Close();
			Singleton.UI.ShowPauseWindow();
		});
		GUIUtils.HookButton(HelpButton, () =>
		{
			OnCancel();
			Close();
			Singleton.UI.ShowHelpWindow();
		});
		GUIUtils.HookButton(AcceptButton, () =>
		{
			OnSave();
			Singleton.Settings.Save();
			Close();
			Singleton.UI.ShowPauseWindow();
		});
	}

	private void AddSlider(string title, float value, Action<float> valueSetter)
	{
		GameObject gameObject = new GameObject("setting" + _settingsCount);
		UISimpleSlider slider = Util.CreateGameObject("SettingSlider").GetComponent<UISimpleSlider>();
		slider.transform.parent = gameObject.transform;
		slider.transform.localPosition = new Vector3(40f, 0f, 0f);
		slider.Value = value;
		slider.AddValueChangedDelegate((IUIObject uiObject) =>
		{
			valueSetter((uiObject as UISimpleSlider).Value);
		});
		SpriteText component = Util.CreateGameObject("SettingText").GetComponent<SpriteText>();
		component.transform.parent = gameObject.transform;
		component.transform.localPosition = new Vector3(-72.5f, 0f, 0f);
		component.Text = title;
		ElementsLayout.AddItem(gameObject, 0, _settingsCount++);
		OnSave += (Action)(() =>
		{
			valueSetter(slider.Value);
		});
		OnCancel += (Action)(() =>
		{
			slider.SetValueChangedDelegate(null);
			valueSetter(value);
		});
	}

	private void AddToggle(string title, bool toggle, Action<bool> valueSetter)
	{
		GameObject gameObject = new GameObject("setting" + _settingsCount);
		UIOnOffButtons buttons = Util.CreateGameObject("OnOffButtons").GetComponent<UIOnOffButtons>();
		buttons.transform.parent = gameObject.transform;
		buttons.transform.localPosition = new Vector3(47.5f, 0f, 0f);
		buttons.Toggle = toggle;
		SpriteText component = Util.CreateGameObject("SettingText").GetComponent<SpriteText>();
		component.transform.parent = gameObject.transform;
		component.transform.localPosition = new Vector3(-72.5f, 0f, 0f);
		component.Text = title;
		ElementsLayout.AddItem(gameObject, 0, _settingsCount++);
		OnSave += (Action)(() =>
		{
			valueSetter(buttons.Toggle);
		});
		OnCancel += (Action)(() =>
		{
			valueSetter(toggle);
		});
	}
}
