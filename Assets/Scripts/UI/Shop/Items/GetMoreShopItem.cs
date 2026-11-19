using System.Xml.Serialization;

[XmlType("getMoreShopItem")]
public class GetMoreShopItem : ShopConfigItem
{
	public override void Override(ShopConfigItem dest)
	{
		base.Override(dest);
	}

	public override ShopConfigItem Clone()
	{
		GetMoreShopItem getMoreShopItem = new GetMoreShopItem();
		Override(getMoreShopItem);
		return getMoreShopItem;
	}

	public override string GetDescription()
	{
		var result = base.GetDescription();
		return result;
	}
}
