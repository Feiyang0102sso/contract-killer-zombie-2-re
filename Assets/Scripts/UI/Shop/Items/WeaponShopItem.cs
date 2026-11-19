using System.Xml.Serialization;

[XmlType("weaponItem")]
public class WeaponShopItem : InventoryShopItem
{
	[XmlAttribute("power")]
	public int? power;

	[XmlAttribute("dmg")]
	public int? dmg;

	[XmlAttribute("acc")]
	public int? acc;

	[XmlAttribute("range")]
	public int? range;

	[XmlAttribute("rate")]
	public int? rate;

	[XmlAttribute("cap")]
	public int? cap;

	[XmlAttribute("speed")]
	public int? speed;

	public int Power
	{
		get
		{
			int? num = power;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int Dmg
	{
		get
		{
			int? num = dmg;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int Acc
	{
		get
		{
			int? num = acc;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int Range
	{
		get
		{
			int? num = range;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int Rate
	{
		get
		{
			int? num = rate;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int Cap
	{
		get
		{
			int? num = cap;
			return num.HasValue ? num.Value : 0;
		}
	}

	public int Speed
	{
		get
		{
			int? num = speed;
			return num.HasValue ? num.Value : 0;
		}
	}

	public override ShopConfigItem Clone()
	{
		WeaponShopItem weaponShopItem = new WeaponShopItem();
		Override(weaponShopItem);
		return weaponShopItem;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		WeaponShopItem weaponShopItem = other as WeaponShopItem;
		CommonShopItem.OverrideValue(ref weaponShopItem.power, power);
		CommonShopItem.OverrideValue(ref weaponShopItem.dmg, dmg);
		CommonShopItem.OverrideValue(ref weaponShopItem.acc, acc);
		CommonShopItem.OverrideValue(ref weaponShopItem.range, range);
		CommonShopItem.OverrideValue(ref weaponShopItem.rate, rate);
		CommonShopItem.OverrideValue(ref weaponShopItem.cap, cap);
		CommonShopItem.OverrideValue(ref weaponShopItem.speed, speed);
	}
}
