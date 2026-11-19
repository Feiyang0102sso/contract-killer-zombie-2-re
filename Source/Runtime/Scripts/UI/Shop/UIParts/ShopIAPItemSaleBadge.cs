using System;
using Glu.Localization;
using UnityEngine;

public class ShopIAPItemSaleBadge : MonoBehaviour
{
	public SpriteText InfoText;

	public GameObject InfoPart;

	private IAPStoreFrontItemPlate _itemPlate;

	private void Start()
	{
		_itemPlate = gameObject.FindComponentInParents<IAPStoreFrontItemPlate>();
		if (_itemPlate is IAPBuyButton)
		{
			transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		}
		gameObject.SetActiveRecursively(_itemPlate.ShopItem.IsOnSale());
		InfoText.Text = string.Format(Strings.GetString("Sale!"));
		_itemPlate.OnLoadingFinished += new Action(UpdateState);
		UpdateState();
		MoveOnProperPosition(true);
	}

	private void UpdateState()
	{
		InfoPart.SetActiveRecursively(_itemPlate.ShopItem.IsOnSale() && !_itemPlate.IsLoading());
	}

	public void MoveOnProperPosition(bool proper)
	{
		bool flag = _itemPlate is IAPBuyButton;
		if (proper)
		{
			transform.localPosition = new Vector3(flag ? (-15.43f) : 10f, flag ? 8.25f : (-10f), -3f);
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
			UpdateState();
		}
	}
}
