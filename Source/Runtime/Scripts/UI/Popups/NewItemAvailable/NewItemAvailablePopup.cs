using System;
using Glu.Localization;
using UnityEngine;

public class NewItemAvailablePopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton CheckoutButton;

	public SpriteText TitleText;

	public SpriteText BonusText;

	public ShopItemWidget ItemWidget;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		UnityEngine.Object.Destroy(CloseButton);
		Singleton.GameEvents.OnItemPurchase += new Action<string>(OnItemPurchase);
	}

	private void OnItemPurchase(string item)
	{
		Close();
	}

	public void Init(CommonShopItem item)
	{
		TitleText.Text = string.Format(Strings.GetString("{0}\n available!"), Strings.GetString(item.nameId));
		BonusText.Text = item.SaleDiscount > 0
			? string.Format(Strings.GetString("BUY NOW AND SAVE {0}% OFF ALL ITEMS!"), item.SaleDiscount)
			: string.Empty;
		ItemWidget.SetItem(item);
		GUIUtils.HookButton(CheckoutButton, () =>
		{
			Close();
			if (item is WeaponShopItem)
			{
				Singleton.UI.Store.CompareWeapon(item.id);
			}
			else
			{
				Singleton.UI.Store.Show();
				ShopConfigGroup shopConfigGroup = Singleton.ShopData.FindGroupWithItem(item);
				Debug.Log(shopConfigGroup.nameId);
				if (shopConfigGroup != null)
				{
					Singleton.UI.Store.SelectGroup(shopConfigGroup.id);
				}
			}
		});
	}

	protected override void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnItemPurchase -= new Action<string>(OnItemPurchase);
		}
		base.OnDestroy();
	}
}
