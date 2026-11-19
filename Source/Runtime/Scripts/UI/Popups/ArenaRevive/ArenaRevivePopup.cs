using System;
using Glu.Localization;
using UnityEngine;

public class ArenaRevivePopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton ReviveButton;

	public SpriteText ScoreText;

	public SpriteText AdviceText;

	public UIExpandSprite Back;

	public UISingleSprite ImageRight;

	private ConsumableShopItem _consumableItem;

	private int _initialCountOfConsumableItems;

	protected override void Awake()
	{
		base.Awake();
		CloseButton.Text = Strings.GetString("QUIT");
	}

	public void Init(ArenaMissionReward reward)
	{
		_consumableItem = Singleton.ShopData.GetConsumableShopItemByItem("Defibrillator");
		_initialCountOfConsumableItems = _consumableItem.GetCountInPlayersInventory();
		int countInPlayersInventory = _consumableItem.GetCountInPlayersInventory();
		string text = (!(countInPlayersInventory <= 0) ? ("(" + countInPlayersInventory + ")") : GUIUtils.GetPriceStringForCurrency(_consumableItem.Price, _consumableItem.currency));
		ReviveButton.Text = Strings.GetString("REVIVE") + " " + text;
		AdviceText.Text = Strings.GetString("You've been overwhelmed by zombies.\nRevive to earn more rewards!");
		int survivalArenaBestScore = Singleton.PlayerData.PlayerStatistics.GetSurvivalArenaBestScore();
		ScoreText.Text = survivalArenaBestScore.ToString();
		_consumableItem.OnUse += new Action(OnRevive);
		_consumableItem.OnActualPurchase += new Action(OnPurchase);
		GUIUtils.HookButton(ReviveButton, () =>
		{
			if (_consumableItem.GetCountInPlayersInventory() > 0)
			{
				_consumableItem.Use();
			}
			else
			{
				_consumableItem.Purchase();
			}
		});
		GUIUtils.HookButton(CloseButton, () =>
		{
			Close();
			Singleton.UI.PopupManager.ShowArenaModeFinishPopup(reward);
		});
		if (Singleton.Quality.CustomTextureQuality > CustomTextureQuality.Low)
		{
			ImageRight.SetTexture(Resources.Load("LoadingScreens/loading_back_1") as Texture2D);
		}
	}

	private void OnRevive()
	{
		Close();
		if ((bool)Scene.Mission)
		{
			Scene.Mission.OnRevived(true, _initialCountOfConsumableItems == 0);
		}
		Singleton.PlayerData.LastMissionStatistics.NumToolkitsPurchased++;
	}

	private void OnPurchase()
	{
		_consumableItem.Use();
		Close();
	}

	protected override void OnDestroy()
	{
		if (_consumableItem != null)
		{
			_consumableItem.OnUse -= new Action(OnRevive);
			_consumableItem.OnActualPurchase -= new Action(OnPurchase);
		}
		base.OnDestroy();
	}
}
