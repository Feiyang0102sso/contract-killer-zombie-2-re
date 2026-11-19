using Glu.AssetBundles;
using UnityEngine;

public class GetMoreStoreFrontItemPlate : StoreFrontItem
{
	public UIButton GamesButton;

	public UIButton GoldButton;

	public UISingleSprite GamesIcon;

	public UISingleSprite GoldIcon;

	protected virtual void Awake()
	{
		GUIUtils.HookButton(GamesButton, App.Instance.GetMoreGames);
		GUIUtils.HookButton(GoldButton, App.Instance.GetFreeGold);
		Object.Destroy(GamesButton.GetComponent<Renderer>());
		Object.Destroy(GoldButton.GetComponent<Renderer>());
		if (!QualitySetter.PlayHavenEnabled)
		{
			GamesIcon.Color = new Color(0.2f, 0.2f, 0.2f, 0.2f);
			GUIUtils.BlockButton(GamesButton, true);
			Object.Destroy(GamesButton.GetComponentInChildren<SpriteText>().gameObject);
		}
	}

	public override void Init(ShopConfigItem item)
	{
	}

	protected override void LateInit()
	{
		GoldIcon.SetTexture(AssetBundles.Load("Assets/UI/Shop/IAPs/gold_01.png") as Texture2D);
		GamesIcon.SetTexture(AssetBundles.Load("Assets/UI/Shop/Special/gamepad.png") as Texture2D);
	}
}
