using Glu.Localization;
using UnityEngine;

public class Ckz1BannerPopup : UIWindow
{
	public UIButton CloseButton;

	public SpriteText EnjoyedPlayingLabel;

	public SpriteText Ckz2Label;

	public SpriteText DownloadLabel;

	public UISingleSprite Picture;

	public Texture2D PictureTexture;

	public UIPressableButton DownloadButton;

	protected override void Awake()
	{
		base.Awake();
		Picture.SetTexture(PictureTexture);
		GUIUtils.HookButton(CloseButton, Close);
	}

	protected override void Start()
	{
		base.Start();
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
		DownloadButton.Text = Strings.GetString("DOWNLOAD");
		GUIUtils.HookButton(DownloadButton, Download);
	}

	private void Download()
	{
		Close();
		Application.OpenURL("http://gcs.glu.com/gcs/iphone/iphone.jsp?t=zombsniper&src=zombsniper&p=iphone");
	}
}
