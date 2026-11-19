using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class NotEnoughMoneyWindow : UIWindow
{
	public IAPBuyButton IAPBuyButtonPrefab;

	public UIButton CloseButton;

	public SpriteText Title;

	public UIGridLayout IAPContainer;

	public SpriteText Text1;

	public SpriteText TextIAP;

	public SpriteText Text2;

	public ShopItemWidget ItemWidget;

	public UISingleSprite ImageLeft;

	private CommonShopItem _item;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			ImageLeft.SetTexture(Resources.Load("LoadingScreens/loading_back_1") as Texture2D);
		}
		GUIUtils.HookButton(CloseButton, Close);
	}

	public void Init(CommonShopItem item)
	{
		_item = item;
		int num = -Singleton.PlayerData.MoneyDelta(item.currency, item.Price);
		Title.Text = string.Format(Strings.GetString("YOU NEED {0}{1} MORE"), GUIUtils.GetPriceStringForCurrency(num, item.currency), Color.white);
		ItemWidget.SetItem(item);
		foreach (UIButton uIButton in ItemWidget.StoreFrontItem.gameObject.GetComponentsInChildren<UIButton>())
		{
			uIButton.transform.localScale = Vector3.zero;
		}
		List<ShopConfigGroup.Reference> itemRefs = Singleton.ShopData.shopConfig.FindGroup("currency").itemRefs;
		List<IAPShopItem> list = new List<IAPShopItem>(itemRefs.Count);
		foreach (ShopConfigGroup.Reference item2 in itemRefs)
		{
			IAPShopItem iAPShopItem = item2.item as IAPShopItem;
			if (iAPShopItem.currency != item.currency)
			{
				continue;
			}
			if (iAPShopItem.amount < num)
			{
				continue;
			}
			list.Add(iAPShopItem);
		}
		list.Sort((IAPShopItem a, IAPShopItem b) => a.Amount - b.Amount);
		if (list.Count == 0)
		{
			return;
		}
		Text1.Text = string.Empty;
		TextIAP.Text = string.Empty;
		Text2.Text = string.Empty;
		double num2 = (list.Count > 0 && list[0].defaultPrice.HasValue) ? (double)list[0].Amount / (double)list[0].defaultPrice.Value : 0.0;
		for (int num3 = 0; num3 < 3 && num3 < list.Count; num3++)
		{
			double num4 = list[num3].defaultPrice.HasValue ? (double)list[num3].Amount / (double)list[num3].defaultPrice.Value : 0.0;
			string infoText = string.Empty;
			if (num4 > num2)
			{
				infoText = string.Format(Strings.GetString("({0}% more)"), Mathf.FloorToInt((float)(num4 / num2) * 100f - 100f));
			}
			IAPBuyButton component = (Object.Instantiate(IAPBuyButtonPrefab.gameObject) as GameObject).GetComponent<IAPBuyButton>();
			component.Init(list[num3], infoText);
			IAPContainer.AddItem(component.gameObject, num3, 0);
		}
	}

	private void Update()
	{
		if (_item != null)
		{
			int num = Singleton.PlayerData.MoneyDelta(_item.currency, _item.Price);
			if (num >= 0)
			{
				Close();
				_item.Purchase();
			}
		}
	}
}
