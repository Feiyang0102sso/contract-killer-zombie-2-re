using UnityEngine;

public class WeaponSellPopup : UIWindow
{
	private const float topOffset = 19f;

	private const int moneyBackWidth = 120;

	private const int moneyBackHeight = 12;

	public UIExpandSprite Back;

	public UIExpandSprite BackMoney;

	public ShopItemWidget RecWeapon1Widget;

	public ShopItemWidget RecWeapon2Widget;

	public ShopItemWidget RecWeapon3Widget;

	public SpriteText TitleText;

	public UIButton CloseButton;

	public SpriteText MainText;

	private CommonShopItem _goodWeapon;

	private CommonShopItem _betterWeapon;

	private CommonShopItem _bestWeapon;

	private bool _salesSimulated;

	private MapLocation _location;

	private int _updatesTopmost;

	protected override void Awake()
	{
		_position = new Vector2(0f, -9.5f);
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		MissionInfoPopup missionInfoPopup = Singleton.UI.OpenedWindows.Find((UIWindow w) => w is MissionInfoPopup) as MissionInfoPopup;
		if ((bool)missionInfoPopup)
		{
			missionInfoPopup.Close();
		}
		Back.SetSize(GUIUtils.ScreenWidth(), GUIUtils.ScreenHeight() - 19f);
	}

	public void Init(MapLocation location, string title, string text, string goodWeapon, string betterWeapon, string bestWeapon, bool canClose)
	{
		_location = location;
		TitleText.Text = title;
		MainText.Text = text;
		BackMoney.SetSize(MainText.TotalWidth + 10f, MainText.LineSpan * ((float)MainText.GetDisplayLineCount() + 0.5f));
		CloseButton.gameObject.SetActiveRecursively(canClose);
		if (canClose)
		{
			GUIUtils.HookButton(CloseButton, () =>
			{
				Close(false);
			});
		}
		GameObject gameObject = new GameObject("Weapons");
		UIGridLayout uIGridLayout = gameObject.AddComponent<UIGridLayout>();
		uIGridLayout.Width = GUIUtils.ScreenWidth();
		uIGridLayout.Height = 1f;
		RecWeapon1Widget.SetWeaponInfoItem(goodWeapon, null);
		RecWeapon2Widget.SetWeaponInfoItem(betterWeapon, goodWeapon);
		RecWeapon3Widget.SetWeaponInfoItem(bestWeapon, goodWeapon);
		uIGridLayout.AddItem(RecWeapon1Widget.gameObject, 0, 0);
		uIGridLayout.AddItem(RecWeapon2Widget.gameObject, 1, 0);
		uIGridLayout.AddItem(RecWeapon3Widget.gameObject, 2, 0);
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = new Vector3(0f, -15f, -5f);
		if (!canClose)
		{
		}
		_goodWeapon = Singleton.ShopData.GetShopItem(goodWeapon) as CommonShopItem;
		_betterWeapon = Singleton.ShopData.GetShopItem(betterWeapon) as CommonShopItem;
		_bestWeapon = Singleton.ShopData.GetShopItem(bestWeapon) as CommonShopItem;
	}

	public void SimulateSale(int discount1, int discount2, int discount3)
	{
		if (discount1 > 0)
		{
			_goodWeapon.isSale = true;
			_goodWeapon.saleDiscount = discount1;
			_goodWeapon.isSaleWithTimer = false;
			_goodWeapon.salePrice = Mathf.RoundToInt((float)_betterWeapon.BasePrice * 0.01f * (float)(100 - discount1));
		}
		if (discount2 > 0)
		{
			_betterWeapon.isSale = true;
			_betterWeapon.saleDiscount = discount2;
			_betterWeapon.isSaleWithTimer = false;
			_betterWeapon.salePrice = Mathf.RoundToInt((float)_betterWeapon.BasePrice * 0.01f * (float)(100 - discount2));
		}
		if (discount3 > 0)
		{
			_bestWeapon.isSale = true;
			_bestWeapon.saleDiscount = discount3;
			_bestWeapon.isSaleWithTimer = false;
			_bestWeapon.salePrice = Mathf.RoundToInt((float)_bestWeapon.BasePrice * 0.01f * (float)(100 - discount3));
		}
		_salesSimulated = true;
	}

	private void Update()
	{
		bool flag = IsTopmost();
		if (flag)
		{
			_updatesTopmost++;
		}
		else
		{
			_updatesTopmost = 0;
		}
		if (!(!flag || _updatesTopmost <= 2))
		{
			bool flag2 = (_goodWeapon as InventoryShopItem).Slot == PlayerData.Slots.shotgun;
			if (Singleton.PlayerData.HasWeapon(_goodWeapon.item))
			{
				if (flag2)
				{
					GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Buy_Shotgun_1);
				}
				Close(true);
			}
			else if (Singleton.PlayerData.HasWeapon(_betterWeapon.item))
			{
				if (flag2)
				{
					GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Buy_Shotgun_2);
				}
				Close(true);
			}
			else if (Singleton.PlayerData.HasWeapon(_bestWeapon.item))
			{
				if (flag2)
				{
					GameAnalytics.LogTutorialStepCompleted(Tutorial.AnalyticEvents.Buy_Shotgun_3);
				}
				Close(true);
			}
		}
	}

	public void Close(bool weaponWasSold)
	{
		if (weaponWasSold && _location != null)
		{
			_location.SelectMission();
		}
		Close();
	}

	protected override void Deactivate()
	{
		if (_salesSimulated)
		{
			Singleton.ShopData.ReloadConfig();
		}
		base.Deactivate();
	}
}
