using System.Collections.Generic;
using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class BundleStoreFrontItemPlate : CommonStoreFrontItemPlate
{
	public SpriteText BonusText;

	private BundleShopItem _bundleShopItem;

	public UISingleSprite[] Icons;

	public SpriteText[] ItemNames;

	public SpriteText[] ItemCounts;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void Init(ShopConfigItem item)
	{
		base.Init(item);
		_bundleShopItem = item as BundleShopItem;
		BonusText.Text = Strings.GetString(_bundleShopItem.bonusTextId);
		BonusText.Text = string.Empty;
	}

	protected override void LateInit()
	{
		int i = 0;
		foreach (KeyValuePair<CommonShopItem, int> item in _bundleShopItem.ToItemsList())
		{
			Icons[i].SetTexture(AssetBundles.Load(item.Key.image) as Texture2D);
			if (item.Key is ConsumableShopItem)
			{
				Icons[i].SetSize(30f, 30f);
			}
			else
			{
				Icons[i].SetSize(60f, 30f);
			}
			ItemCounts[i].Text = (!(item.Value <= 1) ? string.Concat(GUIUtils.ColorYellowTitle, "x", item.Value) : string.Empty);
			ItemNames[i].Text = Strings.GetString(item.Key.nameId);
			i++;
		}
		for (; i < Icons.Length; i++)
		{
		}
	}

	public void EnableDemoMode()
	{
		Object.Destroy(BuyButton.gameObject);
		Object.Destroy(PriceText.gameObject);
		Object.Destroy(SaleOldPriceText.gameObject);
		UIExpandSprite componentInChildren = GetComponentInChildren<UIExpandSprite>();
		componentInChildren.SetSize(220f, componentInChildren.DesiredHeight);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}
}
