using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class CommonStoreFrontItemPlate : StoreFrontItem
{
	private CommonShopItem _commonShopItem;

	public UIButton BuyButton;

	public SpriteText TitleText;

	public SpriteText PriceText;

	public UISingleSprite Icon;

	public UIExpandSprite Border;

	public PackedSprite HoneyBack;

	public SpriteText SaleOldPriceText;

	public PackedSprite SaleLine;

	private ShopItemSaleBadge _saleBadge;

	public CommonShopItem ShopItem
	{
		get
		{
			return _commonShopItem;
		}
	}

	protected virtual bool AllowSaleBadge()
	{
		return true;
	}

	protected virtual void Awake()
	{
		if ((bool)BuyButton)
		{
			GUIUtils.HookButton(BuyButton, OnBuyButton, false);
		}
	}

	protected override void Start()
	{
		base.Start();
		UpdatePrices();
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
		_commonShopItem = item as CommonShopItem;
		if (_saleBadge == null && _commonShopItem.IsSale && AllowSaleBadge())
		{
			GameObject gameObject = Util.CreateGameObject("ShopItemSaleBadge");
			gameObject.transform.parent = transform;
			_saleBadge = gameObject.GetComponent<ShopItemSaleBadge>();
		}
		Color color = Util.ColorFromHex(_commonShopItem.Color_ != string.Empty ? _commonShopItem.Color_ : "#5d95cb");
		if (HoneyBack != null)
		{
			HoneyBack.Color = color;
		}
		if (Border != null)
		{
			Border.Color = color;
		}
		TitleText.Text = Strings.GetString(item.nameId);
		UpdatePrices();
	}

	private void OnBuyButton()
	{
		_commonShopItem.Purchase();
	}

	protected override void LateInit()
	{
		if ((bool)Icon)
		{
			Object obj = AssetBundles.Load(_commonShopItem.image);
			Texture2D texture2D = obj as Texture2D;
			float num = (float)texture2D.width / (float)texture2D.height;
			Icon.width = Icon.height * num;
			Icon.SetTexture(texture2D);
		}
	}

	public virtual void UpdatePrices()
	{
		if (_commonShopItem == null)
		{
			return;
		}
		if (PriceText != null)
		{
			GUIUtils.SetupPriceText(PriceText, _commonShopItem.Price, _commonShopItem.currency);
		}
		if ((bool)SaleLine && (bool)SaleOldPriceText)
		{
			bool flag = _commonShopItem.IsOnSale();
			SaleLine.gameObject.active = flag;
			SaleOldPriceText.gameObject.active = flag;
			if (flag)
			{
				GUIUtils.SetupPriceText(SaleOldPriceText, _commonShopItem.BasePrice, _commonShopItem.currency);
				SaleLine.SetSize(30f, 1f);
			}
		}
	}

	protected override void OnEnable()
	{
		UpdatePrices();
		base.OnEnable();
	}
}
