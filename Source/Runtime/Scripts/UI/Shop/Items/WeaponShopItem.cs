using System.Xml;
using System.Xml.Serialization;

[XmlType("weaponItem")]
public class WeaponShopItem : InventoryShopItem
{
	[XmlIgnore]
	public int? power;

	/// <summary>PC 移植：power 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("power")]
	public string power_XmlText
	{
		get
		{
			if (!power.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(power.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				power = null;
				return;
			}
			power = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? dmg;

	/// <summary>PC 移植：dmg 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("dmg")]
	public string dmg_XmlText
	{
		get
		{
			if (!dmg.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(dmg.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				dmg = null;
				return;
			}
			dmg = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? acc;

	/// <summary>PC 移植：acc 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("acc")]
	public string acc_XmlText
	{
		get
		{
			if (!acc.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(acc.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				acc = null;
				return;
			}
			acc = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? range;

	/// <summary>PC 移植：range 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("range")]
	public string range_XmlText
	{
		get
		{
			if (!range.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(range.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				range = null;
				return;
			}
			range = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? rate;

	/// <summary>PC 移植：rate 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("rate")]
	public string rate_XmlText
	{
		get
		{
			if (!rate.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(rate.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				rate = null;
				return;
			}
			rate = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? cap;

	/// <summary>PC 移植：cap 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("cap")]
	public string cap_XmlText
	{
		get
		{
			if (!cap.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(cap.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				cap = null;
				return;
			}
			cap = XmlConvert.ToInt32(value);
		}
	}

	[XmlIgnore]
	public int? speed;

	/// <summary>PC 移植：speed 的 XML 读写代理（新版 XmlSerializer 不支持可空类型的特性）。</summary>
	[XmlAttribute("speed")]
	public string speed_XmlText
	{
		get
		{
			if (!speed.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(speed.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				speed = null;
				return;
			}
			speed = XmlConvert.ToInt32(value);
		}
	}

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
