using System;
using Glu.Localization;
using UnityEngine;

public class HudAmmoRefill : MonoBehaviour
{
	private const string AmmoRefillItemName = "AmmoRefill";

	public SpriteText AmmoRefillText;

	public GameObject Gold;

	private UIButton _button;

	private bool _isAvailable;

	private ConsumableShopItem _ammoRefillConsumable;

	public Action OnRefill = () =>
	{
	};

	public bool IsAvailable()
	{
		return _isAvailable;
	}

	private void Awake()
	{
		_button = GetComponent<UIButton>();
		_ammoRefillConsumable = Singleton.ShopData.GetConsumableShopItemByItem("AmmoRefill");
		_ammoRefillConsumable.OnActualPurchase += new Action(OnAmmoRefill);
	}

	private void Start()
	{
		if (Singleton.PlayerData.StorylineTicket >= 2)
		{
			GUIUtils.HookButton(_button, OnAmmoRefill, POINTER_INFO.INPUT_EVENT.RELEASE, true, false);
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public float GetAlpha()
	{
		return _isAvailable ? 1f : 0.5f;
	}

	public void Refresh()
	{
		int ammoRefillCount = GetAmmoRefillCount();
		if (ammoRefillCount > 0)
		{
			AmmoRefillText.Text = Strings.GetString("FREE") + " (" + ammoRefillCount + ")";
			AmmoRefillText.transform.localPosition = new Vector3(0f, 0f, -1f);
			Gold.GetComponent<Renderer>().enabled = false;
		}
		else
		{
			AmmoRefillText.Text = GetAmmoRefillPrice().ToString();
			AmmoRefillText.transform.localPosition = new Vector3(7.5f, 0f, -1f);
			Gold.GetComponent<Renderer>().enabled = true;
		}
		_isAvailable = !AmmoRefill.IsAllWeaponsFull(Scene.Player);
		_button.Color = new Color(1f, 1f, 1f, GetAlpha());
		_button.enabled = _isAvailable;
	}

	private void ConsumeAmmoRefill()
	{
		Player player = Scene.Player;
		Consumable consumableByItemName = player.Inventory.GetConsumableByItemName("AmmoRefill");
		if ((bool)consumableByItemName)
		{
			consumableByItemName.Consume(player.gameObject);
		}
		OnRefill();
		Refresh();
	}

	private int GetAmmoRefillCount()
	{
		Consumable consumableByItemName = Scene.Player.Inventory.GetConsumableByItemName("AmmoRefill");
		return consumableByItemName ? consumableByItemName.Count : 0;
	}

	private int GetAmmoRefillPrice()
	{
		ConsumableShopItem consumableShopItemByItem = Singleton.ShopData.GetConsumableShopItemByItem("AmmoRefill");
		return consumableShopItemByItem.Price;
	}

	private void OnAmmoRefill()
	{
		if (!_isAvailable)
		{
			return;
		}
		int refillCount = GetAmmoRefillCount();
		if (refillCount == 0)
		{
			int ammoRefillPrice = GetAmmoRefillPrice();
			if (Singleton.PlayerData.HardCurrency > ammoRefillPrice)
			{
				Singleton.PlayerData.HardCurrency -= ammoRefillPrice;
				GameAnalytics.LogHCSpent(ammoRefillPrice, "AmmoRefill");
				Scene.Player.Inventory.AddConsumable("AmmoRefill", 1, true);
				GameAnalytics.LogHCConsumable("AmmoRefill", ammoRefillPrice, "hard", Singleton.PlayerData.Level);
				ConsumeAmmoRefill();
				SoundManager.PlayExclusive("UI/shop_item_bought", false);
				SoundManager.PlayExclusive("extra_ammo", false);
			}
			else
			{
				SoundManager.PlayExclusive("UI/not_enough_money", false);
				Singleton.UI.ShowNotEnoughMoneyWindow(_ammoRefillConsumable);
			}
		}
		else
		{
			ConsumeAmmoRefill();
			SoundManager.PlayExclusive("extra_ammo", false);
		}
		Refresh();
	}

	private void OnDestroy()
	{
		if (_ammoRefillConsumable != null)
		{
			_ammoRefillConsumable.OnActualPurchase -= new Action(OnAmmoRefill);
		}
	}
}
