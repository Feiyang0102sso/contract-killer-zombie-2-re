using Glu.Localization;
using UnityEngine;

public class RateMePopup : UIWindow
{
	public SpriteText Text;

	public UIButton YesButton;

	public UIButton NoButton;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		Text.Text = Strings.GetString("Do you enjoy killing zombies?\nPlease rate our game.");
		YesButton.spriteText.Text = Strings.GetString("RATE THE\nGAME");
		GUIUtils.HookButton(YesButton, () =>
		{
			GameAnalytics.LogGameRated(true);
			Singleton.PlayerData.RateMeAccepted = true;
			Application.OpenURL("http://gcs.glu.com/gcs/iphone/iphone.jsp?t=ckzombies2&src=ckzombies2&p=iphone&page=review");
			Close();
		});
		GUIUtils.HookButton(NoButton, () =>
		{
			GameAnalytics.LogGameRated(false);
			Close();
		});
	}
}
