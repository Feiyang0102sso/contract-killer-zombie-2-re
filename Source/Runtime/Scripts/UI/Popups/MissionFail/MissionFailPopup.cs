using System;
using Glu.Localization;
using UnityEngine;

public class MissionFailPopup : UIWindow
{
	public UIButton CloseButton;

	public UIButton OkButton;

	public SpriteText RewardCash;

	public SpriteText RewardXP;

	public SpriteText RewardHard;

	public SpriteText AdviceText;

	public SpriteText TitleText;

	public ShopItemWidget ReviveWidget;

	public UIExpandSprite Back;

	private ConsumableShopItem _consumableItem;

	private int _initialCountOfConsumableItems;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		CloseButton.GetComponent<UIAlign>().UpdateAlignment();
	}

	public void Init(GameEvents.MissionFailure failure, BasicMissionReward reward)
	{
		RewardXP.Text = string.Concat(Color.white, Strings.GetString("XP"), " ", GUIUtils.ColorGreyUI, reward.experienceBase, " ", BonusString(reward.experienceBonus));
		RewardCash.Text = string.Concat(Color.white, GUIUtils.GetSymbolForCurrency("soft"), " ", GUIUtils.ColorGreyUI, reward.softCurrencyBase, " ", BonusString(reward.softCurrencyBonus));
		if (reward.hardCurrency > 0)
		{
			RewardHard.Text = string.Concat(GUIUtils.ColorYellowHard, GUIUtils.GetSymbolForCurrency("hard"), " ", BonusStringParensless(reward.hardCurrency));
		}
		else
		{
			RewardHard.gameObject.active = false;
		}
		bool defibrillatorOnFailEnabled = Singleton.GameData.DefibrillatorOnFailEnabled;
		if (defibrillatorOnFailEnabled)
		{
			AdviceText.Text = Strings.GetString("USE DEFIBRILATOR TO REVIVE\n AND CONTINUE ON MISSION");
		}
		else
		{
			AdviceText.Text = Strings.GetString("Equip better weapons to deal with tougher zombies, use medkits and power ups to aid you in battle, throw beacon grenades to distract enemies!");
		}
		TitleText.Text = (!(failure != GameEvents.MissionFailure.PlayerKilled) ? Strings.GetString("Evelyn was killed") : Strings.GetString("Survivor was killed"));
		_consumableItem = Singleton.ShopData.GetConsumableShopItemByItem("Defibrillator");
		_initialCountOfConsumableItems = Singleton.PlayerData.GetConsumableCount(_consumableItem.item);
		ReviveWidget.SetItem(_consumableItem);
		(ReviveWidget.StoreFrontItem as ConsumableStoreFrontItemPlate).ShowUseButton = true;
		_consumableItem.OnUse += new Action(OnRevive);
		ReviveWidget.gameObject.SetActiveRecursively(defibrillatorOnFailEnabled);
		OkButton.gameObject.SetActiveRecursively(!defibrillatorOnFailEnabled);
		CloseButton.gameObject.SetActiveRecursively(defibrillatorOnFailEnabled);
		GUIUtils.HookButton(defibrillatorOnFailEnabled ? CloseButton : OkButton, () =>
		{
			Close();
			if ((bool)Scene.Mission)
			{
				Scene.Mission.OnRevived(false, _initialCountOfConsumableItems == 0);
			}
		});
		float num = (defibrillatorOnFailEnabled ? 0f : 30f);
		Back.SetSize(140f, defibrillatorOnFailEnabled ? 140f : (140f - num));
		Back.transform.Translate(0f, num * 0.5f, 0f);
	}

	private void OnRevive()
	{
		Close();
		if ((bool)Scene.Mission)
		{
			Scene.Mission.OnRevived(true, _initialCountOfConsumableItems == 0);
			SoundManager.PlayExclusive("Evelyn/defib_use_1", false);
		}
	}

	private string BonusString(int bonus)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("(+{0})", bonus));
	}

	private string BonusStringParensless(int bonus)
	{
		return string.Concat(GUIUtils.ColorGreenUI, string.Format("+{0}", bonus));
	}

	protected override void OnDestroy()
	{
		if (_consumableItem != null)
		{
			_consumableItem.OnUse -= new Action(OnRevive);
		}
		base.OnDestroy();
	}
}
