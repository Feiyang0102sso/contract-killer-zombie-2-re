using System.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ShopConfigGroup
{
	public class Reference
	{
		public ShopConfigGroup group;

		[XmlAttribute("id")]
		public string id;

		[XmlAttribute("itemId")]
		public string itemId;

		[XmlIgnore]
		public double? order;

		/// <summary>New Unity port: XML read/write proxy for order (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
		[XmlAttribute("order")]
		public string order_XmlText
		{
			get
			{
				if (!order.HasValue)
				{
					return null;
				}
				return XmlConvert.ToString(order.Value);
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					order = null;
					return;
				}
				order = XmlConvert.ToDouble(value);
			}
		}

		[XmlIgnore]
		public bool? visible;

		/// <summary>New Unity port: XML read/write proxy for visible (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
		[XmlAttribute("visible")]
		public string visible_XmlText
		{
			get
			{
				if (!visible.HasValue)
				{
					return null;
				}
				return XmlConvert.ToString(visible.Value);
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					visible = null;
					return;
				}
				visible = NewUnityXml.ParseBool(value);
			}
		}

		public ShopConfigItem item;

		public Reference()
		{
			id = string.Empty;
			itemId = string.Empty;
			item = null;
		}

		public void Override(Reference dest)
		{
			if (!string.IsNullOrEmpty(id))
			{
				dest.id = string.Copy(id);
			}
			if (!string.IsNullOrEmpty(itemId))
			{
				dest.itemId = string.Copy(itemId);
			}
			if (order.HasValue)
			{
				dest.order = order.Value;
			}
			if (visible.HasValue)
			{
				dest.visible = visible.Value;
			}
		}

		public Reference CloneUnbound()
		{
			Reference reference = new Reference();
			Override(reference);
			return reference;
		}
	}

	private List<Reference> _itemRefs;

	[XmlAttribute("id")]
	public string id;

	[XmlAttribute("nameId")]
	public string nameId;

	[XmlAttribute("prefab")]
	public string prefab;

	[XmlIgnore]
	public bool? visible;

	/// <summary>New Unity port: XML read/write proxy for visible (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
	[XmlAttribute("visible")]
	public string visible_XmlText
	{
		get
		{
			if (!visible.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(visible.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				visible = null;
				return;
			}
			visible = NewUnityXml.ParseBool(value);
		}
	}

	[XmlIgnore]
	public double? order;

	/// <summary>New Unity port: XML read/write proxy for order (the newer XmlSerializer rejects [XmlAttribute] on nullable types).</summary>
	[XmlAttribute("order")]
	public string order_XmlText
	{
		get
		{
			if (!order.HasValue)
			{
				return null;
			}
			return XmlConvert.ToString(order.Value);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				order = null;
				return;
			}
			order = XmlConvert.ToDouble(value);
		}
	}

	[XmlIgnore]
	public List<Reference> itemRefs
	{
		get
		{
			return _itemRefs == null ? null : _itemRefs.FindAll((Reference x) => x.visible != false);
		}
		set
		{
			_itemRefs = value;
		}
	}

	[XmlArray("itemRefs")]
	[XmlArrayItem("itemRef")]
	public List<Reference> rawItemRefs
	{
		get
		{
			return _itemRefs;
		}
		set
		{
			_itemRefs = value;
		}
	}

	public ShopConfigGroup()
	{
		id = string.Empty;
		nameId = string.Empty;
		prefab = string.Empty;
	}

	public void SetDefaultOrder()
	{
		if (!(_itemRefs == null))
		{
			int num = 0;
			foreach (Reference itemRef in _itemRefs)
			{
				if (!itemRef.order.HasValue)
				{
					itemRef.order = num;
				}
				num++;
			}
		}
	}

	public void Supplement(ShopConfigGroup dest)
	{
		if (!string.IsNullOrEmpty(id))
		{
			dest.id = string.Copy(id);
		}
		if (!string.IsNullOrEmpty(nameId))
		{
			dest.nameId = string.Copy(nameId);
		}
		if (visible.HasValue)
		{
			dest.visible = visible.Value;
		}
		if (order.HasValue)
		{
			dest.order = order.Value;
		}
		if (!string.IsNullOrEmpty(prefab))
		{
			dest.prefab = string.Copy(prefab);
		}
		if (!(_itemRefs == null))
		{
			foreach (Reference itemRef in _itemRefs)
			{
				Reference reference = dest.FindReference(itemRef.id);
				if (reference != null)
				{
					itemRef.Override(reference);
					continue;
				}
				if (dest._itemRefs == null)
				{
					dest._itemRefs = new List<Reference>();
				}
				dest._itemRefs.Add(itemRef.CloneUnbound());
			}
		}
	}

	public ShopConfigGroup CloneUnbound()
	{
		ShopConfigGroup shopConfigGroup = new ShopConfigGroup();
		Supplement(shopConfigGroup);
		return shopConfigGroup;
	}

	public void Sort()
	{
		if (!(_itemRefs == null))
		{
			Comparison<Reference> comparison = (Reference a, Reference b) =>
			{
				if (a.order < b.order)
				{
					return -1;
				}
				if (a.order > b.order)
				{
					return 1;
				}
				return 0;
			};
			_itemRefs.Sort(comparison);
		}
	}

	public Reference FindReference(string id)
	{
		return _itemRefs != null ? _itemRefs.Find((Reference x) => x.id.Equals(id)) : null;
	}
}
