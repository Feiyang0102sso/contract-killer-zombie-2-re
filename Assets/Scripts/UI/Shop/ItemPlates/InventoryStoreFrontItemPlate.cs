using System;
using Glu.Localization;
using UnityEngine;

public class InventoryStoreFrontItemPlate : CommonStoreFrontItemPlate
{
	protected enum State
	{
		BUY,
		EQUIP,
		EQUIPPED
	}

	private InventoryShopItem _inventoryShopItem;

	public UIButton EquipButton;

	public UIButton EquippedButton;

	public UIButton StatsButton;

	protected State _state;

	protected override void Awake()
	{
		base.Awake();
		GUIUtils.HookButton(EquipButton, OnEquipButton, false);
		GUIUtils.HookButton(EquippedButton, () =>
		{
			SoundManager.PlayExclusive("UI/shop_locked_item_tapped", false);
		}, false);
		if ((bool)StatsButton)
		{
			GUIUtils.HookButton(StatsButton, OnStatsButton, false);
		}
	}

	protected override void Start()
	{
		base.Start();
		UpdateState();
	}

	protected virtual void UpdateState()
	{
		if (_inventoryShopItem == null)
		{
			return;
		}
		if (_inventoryShopItem.IsEquipped())
		{
			_state = State.EQUIPPED;
		}
		else if (_inventoryShopItem.IsAvailableForEquipment())
		{
			_state = State.EQUIP;
		}
		else
		{
			_state = State.BUY;
		}
		bool flag = _inventoryShopItem.unlockLevel > Singleton.PlayerData.Level;
		bool flag2 = _inventoryShopItem.lockedTextId != null;
		bool showLocked = (flag || flag2) && _state == State.BUY;
		if (showLocked)
		{
			BuyButton.gameObject.SetActiveRecursively(false);
			EquipButton.gameObject.SetActiveRecursively(false);
			EquippedButton.gameObject.SetActiveRecursively(true);
			if (_inventoryShopItem.lockedTextId != null)
			{
				EquippedButton.spriteText.SetCharacterSize(7f);
				EquippedButton.spriteText.maxWidth = 34f;
				EquippedButton.spriteText.Text = Strings.GetString(_inventoryShopItem.lockedTextId);
			}
			else
			{
				EquippedButton.spriteText.Text = string.Format(Strings.GetString("LEVEL {0}"), _inventoryShopItem.unlockLevel);
			}
			EquippedButton.spriteText.Color = new Color(78f / 255f, 114f / 255f, 133f / 255f, 220f / 255f);
			Icon.Color = new Color(0.2f, 0.2f, 0.2f, 1f);
		}
		else
		{
			BuyButton.gameObject.SetActiveRecursively(_state == State.BUY);
			EquipButton.gameObject.SetActiveRecursively(_state == State.EQUIP);
			EquippedButton.spriteText.Text = Strings.GetString("EQUIPPED");
			EquippedButton.gameObject.SetActiveRecursively(_state == State.EQUIPPED);
		}
		PriceText.gameObject.SetActiveRecursively(_state == State.BUY && !flag2);
		GUIUtils.SetupPriceText(PriceText, _inventoryShopItem.Price, _inventoryShopItem.currency);
		if ((bool)transform.parent)
		{
			UIListItemContainer component = transform.parent.GetComponent<UIListItemContainer>();
		}
	}

	public override void Init(ShopConfigItem item)
	{
		base.Init(item);
		if (_inventoryShopItem != null)
		{
			Unsubscribe();
		}
		_inventoryShopItem = item as InventoryShopItem;
		Subscribe();
		UpdateState();
	}

	private void OnEquipButton()
	{
		_inventoryShopItem.Equip();
	}

	protected override void OnEnable()
	{
		UpdateState();
		base.OnEnable();
	}

	private void Subscribe()
	{
		_inventoryShopItem.OnStateChange += new Action(UpdateState);
	}

	private void Unsubscribe()
	{
		_inventoryShopItem.OnStateChange -= new Action(UpdateState);
	}

	private void OnDestroy()
	{
		Unsubscribe();
	}

	public void OnStatsButton()
	{
		GameAnalytics.LogStoreStats(_inventoryShopItem.item);
		Singleton.UI.Store.CompareWeapon(_inventoryShopItem.id);
		SoundManager.PlayExclusive("UI/shop_item_used", false);
	}
}
