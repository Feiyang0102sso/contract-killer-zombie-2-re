using UnityEngine;

public class MercenaryHiringPopup : UIWindow
{
	public UIButton YesButton;

	public SpriteText YesButtonLabel;

	public UIButton NoButton;

	public UISingleSprite BackgroundImage;

	protected override void Awake()
	{
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			BackgroundImage.SetTexture(Resources.Load("LoadingScreens/MercenaryHiringBg") as Texture2D);
		}
		base.Awake();
	}
}
