public class IAPBuyButton : IAPStoreFrontItemPlate
{
	protected override void Awake()
	{
		base.Awake();
	}

	public void Init(IAPShopItem iapItem, string infoText)
	{
		base.Init(iapItem);
		PriceText.Text = iapItem.GetPriceAsString();
	}
}
