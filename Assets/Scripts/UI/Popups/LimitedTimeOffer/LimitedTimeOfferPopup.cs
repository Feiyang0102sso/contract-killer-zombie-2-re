using System;
using Glu.Localization;
using UnityEngine;

public class LimitedTimeOfferPopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton BuyButton;

	public SpriteText MainText;

	public SpriteText TimerText;

	public ShopItemWidget ItemWidget;

	private bool _isWelcome;

	protected override void Awake()
	{
		base.Awake();
		GUIUtils.HookButton(CloseButton, Close);
	}

	protected override void Start()
	{
		base.Start();
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public void Init(bool isWelcome, BundleShopItem item, int price)
	{
		if (item == null)
		{
			return;
		}
		_isWelcome = isWelcome;
		if (isWelcome)
		{
			Singleton.PlayerData.IsLimitedTimeOfferPopupShown = true;
			GUIUtils.HookButton(BuyButton, () =>
			{
				Singleton.UI.Store.SelectCurrencyGroup();
				Close();
			});
			int bundleValue = price;
			BuyButton.spriteText.Text = Strings.GetString("BUY");
			MainText.Text = string.Format(Strings.GetString("Complete any purchase in Bank and get {0}{1} worth of items FREE!!!"), GUIUtils.FormatHardCurrencyString(bundleValue), Color.white);
		}
		else
		{
			GUIUtils.HookButton(BuyButton, Close);
			BuyButton.spriteText.Text = Strings.GetString("OK");
			TimerText.Text = string.Empty;
			MainText.Text = Strings.GetString("CONGRATULATION!\nYou have completed the purchase from the Bank and receive following items for FREE!");
		}
		ItemWidget.SetItem(item, false);
		BundleStoreFrontItemPlate bundleStoreFrontItemPlate = ItemWidget.StoreFrontItem as BundleStoreFrontItemPlate;
		if (bundleStoreFrontItemPlate != null)
		{
			bundleStoreFrontItemPlate.EnableDemoMode();
			bundleStoreFrontItemPlate.transform.Translate(20f, 0f, 0f);
		}
	}

	private void Update()
	{
		if (_isWelcome && Util.DoRarely(this, "update_lto", 1f))
		{
			if (!Singleton.ShopData.LimitedTimeOffer.IsRunning())
			{
				return;
			}
			TimeSpan timeLeft = Singleton.ShopData.LimitedTimeOffer.GetTimeLeft();
			TimerText.Text = string.Format(Strings.GetString("Offer ends in: {0}{1}"), Color.red, string.Format("{0:00}:{1:00}:{2:00}", (int)timeLeft.TotalHours, timeLeft.Minutes, timeLeft.Seconds));
		}
	}
}
