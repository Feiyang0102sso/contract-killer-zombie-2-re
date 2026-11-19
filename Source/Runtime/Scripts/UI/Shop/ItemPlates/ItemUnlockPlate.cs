using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class ItemUnlockPlate : MonoBehaviour
{
	public UISingleSprite Icon;

	public SpriteText Name;

	public UIButton Button;

	private void Awake()
	{
	}

	public void Init(CommonShopItem item)
	{
		Texture2D texture2D = AssetBundles.Load(item.image) as Texture2D;
		float num = (float)texture2D.width / (float)texture2D.height;
		Icon.width = Icon.height * num;
		Icon.SetTexture(texture2D);
		Name.Text = Strings.GetString(item.nameId);
	}
}
