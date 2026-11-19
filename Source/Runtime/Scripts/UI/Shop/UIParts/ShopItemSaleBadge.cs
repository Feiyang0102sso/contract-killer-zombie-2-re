using System;
using Glu.Localization;
using UnityEngine;

public class ShopItemSaleBadge : MonoBehaviour
{
	public SpriteText InfoText;

	public SpriteText TimeText;

	public GameObject InfoPart;

	public GameObject TimePart;

	private CommonStoreFrontItemPlate _itemPlate;

	private void Awake()
	{
	}

	private void Start()
	{
		_itemPlate = gameObject.FindComponentInParents<CommonStoreFrontItemPlate>();
		gameObject.SetActiveRecursively(_itemPlate.ShopItem.IsSale);
		InfoText.Text = string.Format(Strings.GetString("{0}%\nSale!"), _itemPlate.ShopItem.SaleDiscount);
		Update();
		MoveOnProperPosition(true);
	}

	private void Update()
	{
		bool flag = _itemPlate.ShopItem.IsOnSale();
		bool activeRecursively = false;
		if (flag && _itemPlate.ShopItem.IsSaleWithTimer)
		{
			long saleTimeLeft = _itemPlate.ShopItem.GetSaleTimeLeft();
			if (saleTimeLeft > 0)
			{
				activeRecursively = true;
				UpdateTimeText(saleTimeLeft);
			}
		}
		InfoPart.SetActiveRecursively(flag);
		TimePart.SetActiveRecursively(activeRecursively);
		_itemPlate.UpdatePrices();
	}

	public void MoveOnProperPosition(bool proper)
	{
		bool flag = _itemPlate is BundleStoreFrontItemPlate;
		if (proper)
		{
			transform.localPosition = new Vector3(flag ? 211f : 10f, flag ? (-7.32f) : (-10f), -3f);
		}
		else
		{
			transform.localPosition = new Vector3(30f, -20f, -3f);
		}
	}

	private void OnEnable()
	{
		if (_itemPlate != null && _itemPlate.ShopItem != null)
		{
			Update();
		}
	}

	private void UpdateTimeText(long delta)
	{
		DateTime dateTime = new DateTime(delta);
		TimeText.Text = (!(dateTime.Day <= 1) ? string.Format(Strings.GetString("{0} days"), dateTime.Day) : dateTime.ToString("HH:mm:ss"));
	}
}
