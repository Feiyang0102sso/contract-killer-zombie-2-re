using System.Globalization;
using Glu.Localization;
using UnityEngine;

public class UnlockableContent : MonoBehaviour
{
	public SpriteText Title;

	public SpriteText Text;

	public UISingleSprite Image;

	public CommonStoreFrontItemPlate WeaponWidget;

	public void Init(CommonShopItem item)
	{
		WeaponWidget.Init(item);
	}

	public void Init(string text)
	{
		Text.Text = text;
	}

	public void Init(string text, string imageResource)
	{
		Text.Text = text.ToUpper(CultureInfo.CurrentCulture);
		Texture2D texture2D = Resources.Load(imageResource) as Texture2D;
		float num = (float)texture2D.width / (float)texture2D.height;
		Image.width = Image.height * num;
		Image.SetTexture(texture2D);
	}

	public void Init(int loadingDescriptorId)
	{
		if (Singleton.GameData.LoadingScreenDescriptors[loadingDescriptorId].IsSpecial)
		{
			Title.Text = Strings.GetString("NEW ZOMBIE BOSS");
		}
		else
		{
			Title.Text = Strings.GetString("NEW ZOMBIE");
		}
		Init(Strings.GetString(Singleton.GameData.LoadingScreenDescriptors[loadingDescriptorId].WhiteHeader), Singleton.GameData.LoadingScreenDescriptors[loadingDescriptorId].PicturePath);
	}
}
