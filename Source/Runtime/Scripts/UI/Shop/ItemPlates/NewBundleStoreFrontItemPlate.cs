using System.Collections.Generic;
using Glu.Localization;

public class NewBundleStoreFrontItemPlate : CommonStoreFrontItemPlate
{
	private BundleShopItem _bundleShopItem;

	public SpriteText[] ItemNames;

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
	}

	protected override void LateInit()
	{
		int num = 0;
		foreach (KeyValuePair<CommonShopItem, int> item in _bundleShopItem.ToItemsList())
		{
			string text = (!(item.Value <= 1) ? string.Concat(GUIUtils.ColorYellowTitle, "x", item.Value) : string.Empty);
			ItemNames[num].Text = Strings.GetString(item.Key.nameId) + text;
			num++;
		}
	}
}
