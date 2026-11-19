using System;
using Glu.Localization;
using UnityEngine;

public class ConsumableStoreFrontItemPlate : CommonStoreFrontItemPlate
{
	private ConsumableShopItem _consumableShopItem;

	public SpriteText PowerText;

	public UIButton UseButton;

	public GameObject CountObject;

	public SpriteText CountText;

	private bool _showUseButton;

	private bool _showCount = true;

	public bool ShowUseButton
	{
		get
		{
			return _showUseButton;
		}
		set
		{
			_showUseButton = value;
			UpdateState();
		}
	}

	public bool ShowCount
	{
		get
		{
			return _showCount;
		}
		set
		{
			_showCount = value;
			UpdateState();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		GUIUtils.HookButton(UseButton, OnUseButton);
	}

	public override void Init(ShopConfigItem item)
	{
		if (_consumableShopItem != null)
		{
			Unsubscribe();
		}
		_consumableShopItem = item as ConsumableShopItem;
		Subscribe();
		UpdateState();
		base.Init(item);
	}

	private void OnUseButton()
	{
		_consumableShopItem.Use();
	}

	protected override void LateInit()
	{
		UpdateState();
		base.LateInit();
	}

	private void Subscribe()
	{
		_consumableShopItem.OnStateChange += new Action(UpdateState);
		_consumableShopItem.OnActualPurchase += new Action(OnActualPurchase);
	}

	private void Unsubscribe()
	{
		_consumableShopItem.OnActualPurchase -= new Action(OnActualPurchase);
		_consumableShopItem.OnStateChange -= new Action(UpdateState);
	}

	private void OnDestroy()
	{
		Unsubscribe();
	}

	protected override void OnEnable()
	{
		UpdateState();
		base.OnEnable();
	}

	private void OnActualPurchase()
	{
		if (_showUseButton)
		{
			_consumableShopItem.Use();
		}
	}

	protected virtual void UpdateState()
	{
		if (_consumableShopItem == null)
		{
			return;
		}
		bool flag = _showUseButton && Singleton.PlayerData.GetConsumableCount(_consumableShopItem.item) > 0;
		UseButton.gameObject.SetActiveRecursively(flag);
		BuyButton.gameObject.SetActiveRecursively(!flag);
		PriceText.gameObject.SetActiveRecursively(!flag);
		PowerText.Text = string.Concat(Strings.GetString("COUNT:"), " ", GUIUtils.ColorGreenUI, Singleton.PlayerData.GetConsumableCount(_consumableShopItem.item));
		CountText.Text = _consumableShopItem.Count > 1 ? "X" + _consumableShopItem.Count : string.Empty;
		CountObject.gameObject.SetActiveRecursively(_showCount);
	}
}
