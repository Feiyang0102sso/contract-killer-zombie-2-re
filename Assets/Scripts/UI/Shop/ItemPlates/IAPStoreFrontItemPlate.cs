using System;
using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class IAPStoreFrontItemPlate : StoreFrontItem
{
	protected IAPShopItem _iapShopItem;

	public UIButton BuyButton;

	public SpriteText TitleText;

	public SpriteText PriceText;

	public SpriteText BonusText;

	public SpriteText DescriptionText;

	public GameObject Loading;

	public UISingleSprite Icon;

	private ShopIAPItemSaleBadge _saleBadge;

	private IAPShopConfigItem.State _cachedState;

	private bool _forceUpdate;

	private bool _isLoading = true;

	public Action OnLoadingFinished = () =>
	{
	};

	public IAPShopItem ShopItem
	{
		get
		{
			return _iapShopItem;
		}
	}

	public bool IsLoading()
	{
		return _isLoading;
	}

	protected virtual void Awake()
	{
		GUIUtils.HookButton(BuyButton, OnBuyButton);
	}

	protected override void Start()
	{
		UpdateState(true);
		GameObject gameObject = Util.CreateGameObject("ShopIAPItemSaleBadge");
		gameObject.transform.parent = transform;
		_saleBadge = gameObject.GetComponent<ShopIAPItemSaleBadge>();
		base.Start();
	}

	public override void OnBeforeRecalculatingBounds()
	{
		if ((bool)_saleBadge)
		{
			_saleBadge.MoveOnProperPosition(false);
		}
	}

	public override void OnAfterRecalculatingBounds()
	{
		if ((bool)_saleBadge)
		{
			_saleBadge.MoveOnProperPosition(true);
		}
	}

	public override void Init(ShopConfigItem item)
	{
		_iapShopItem = item as IAPShopItem;
		if ((bool)TitleText)
		{
			TitleText.Text = Strings.GetString(item.nameId);
		}
		PriceText.Text = string.Empty;
		DescriptionText.Text = string.Empty;
		BonusText.Text = string.Empty;
		UpdateState(true);
	}

	private void OnBuyButton()
	{
		_iapShopItem.Buy();
	}

	protected override void LateInit()
	{
		if (Icon != null)
		{
			Texture2D texture = AssetBundles.Load(_iapShopItem.image) as Texture2D;
			Icon.renderer.enabled = true;
			Icon.SetTexture(texture);
		}
	}

	private void ShowLoading()
	{
		_isLoading = true;
		BuyButton.gameObject.SetActiveRecursively(false);
		Loading.SetActiveRecursively(true);
	}

	private void HideLoading()
	{
		_isLoading = false;
		BuyButton.gameObject.SetActiveRecursively(true);
		Loading.SetActiveRecursively(false);
		OnLoadingFinished();
	}

	private void UpdateData()
	{
		GUIUtils.SetupPriceText(DescriptionText, _iapShopItem.Amount, _iapShopItem.currency);
		PriceText.Text = _iapShopItem.GetPriceAsString();
		BonusText.Text = Strings.GetString(_iapShopItem.bonusTextId);
		PriceText.gameObject.AddComponent<UITextAutoScale>();
		PriceText.gameObject.GetComponent<UITextAutoScale>().MaxWidth = BuyButton.width * 0.7f;
	}

	private void UpdateState(bool force)
	{
		if (_iapShopItem == null)
		{
			return;
		}
		if (_cachedState == _iapShopItem.state && !force)
		{
			return;
		}
		_cachedState = _iapShopItem.state;
		switch (_cachedState)
		{
		case IAPShopConfigItem.State.AvailableForPurchase:
			HideLoading();
			UpdateData();
			break;
		case IAPShopConfigItem.State.CheckingAvailability:
			ShowLoading();
			break;
		case IAPShopConfigItem.State.UnavailableForPurchase:
			ShowLoading();
			break;
		case IAPShopConfigItem.State.Undefined:
			ShowLoading();
			break;
		case IAPShopConfigItem.State.WaitingForRetrival:
			ShowLoading();
			break;
		}
	}

	protected override void OnEnable()
	{
		UpdateState(true);
		_forceUpdate = true;
		base.OnEnable();
	}

	private void Update()
	{
		if (_iapShopItem != null && _iapShopItem.state == IAPShopConfigItem.State.CheckingAvailability && App.Instance.GetNetworkReachability() == NetworkReachability.NotReachable && !App.NoNetworkWarningWasShown)
		{
			Singleton.UI.PopupManager.ShowIAPNoNetworkPopup();
			App.NoNetworkWarningWasShown = true;
		}
		UpdateState(_forceUpdate);
		_forceUpdate = false;
	}
}
