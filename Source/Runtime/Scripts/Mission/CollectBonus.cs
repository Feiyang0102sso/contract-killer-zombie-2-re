using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

public class CollectBonus : MonoBehaviourAndLog
{
	private void Awake()
	{
		LogTo("Mission.Bonus");
	}

	private void Start()
	{
		if (IsHard())
		{
			(from r in GetComponentsInChildren<Renderer>()
				where r.material.name.StartsWith("boxes")
				select r).ToList().ForEach((Renderer r) =>
			{
				r.material = (Material)Resources.Load("HighlightMat");
			});
		}
	}

	private bool IsHard()
	{
		return Scene.Mission.IsSurvivalArena() ? Scene.Mission.GetComponentInChildren<ArenaWaves>().IsBonusRewardHard() : (Scene.Mission.IsHard() || Scene.Mission.IsStorylineMission());
	}

	private void OnUse()
	{
		if (!Scene.Mission)
		{
			return;
		}
		GameData.CollectBonus collectBonus = Singleton.GameData.collectBonus;
		GameData.CollectBonus.BonusInfo bonusInfo = collectBonus.bonusInfos[IsHard() ? 1 : 0];
		Dictionary<string, float> dictionary = bonusInfo.chances.ToDictionary((GameData.StringAndFloat p) => p.s, (GameData.StringAndFloat p) => p.f);
		Dictionary<string, int> consumables = new Dictionary<string, int>();
		string text = string.Empty;
		int numBonusCollected = Singleton.PlayerData.PlayerStatistics.NumBonusCollected;
		if (numBonusCollected <= collectBonus.firstConsumables.Length - 1)
		{
			for (int num = 0; (float)num < collectBonus.firstConsumables[numBonusCollected].f; num++)
			{
				StackConsumable(collectBonus.firstConsumables[numBonusCollected].s, ref consumables);
			}
		}
		else
		{
			int numHardBonusCollected = Singleton.PlayerData.PlayerStatistics.NumHardBonusCollected;
			if (IsHard() && Util.IsFibonacci(numHardBonusCollected + 1 - Singleton.PlayerData.StartingBonusCollectedNumber))
			{
				Scene.Mission.bonusHardCurrency += collectBonus.hardBonus;
				text += Strings.GetString("You found ") + " " + collectBonus.hardBonus + GUIUtils.ColorYellowHard + GUIUtils.GetSymbolForCurrency("hard") + "\n";
			}
			else
			{
				if (Singleton.PlayerData.Level < bonusInfo.minLevelForXpBonus)
				{
					dictionary.Remove("xp");
				}
				for (int num2 = 0; num2 < bonusInfo.chancesToRepeat.Length && Util.Chance(bonusInfo.chancesToRepeat[num2]); num2++)
				{
					string reward = Util.RandomByWeights(dictionary);
					if (reward == "xp")
					{
						float num5 = Util.RandomByWeights(bonusInfo.distribXp.ToDictionary((GameData.FloatAndFloat p) => p.a, (GameData.FloatAndFloat p) => p.b));
						num5 *= IsHard() ? Singleton.GameData.difficultyHardCollectXpMod : 1f;
						int num6 = Mathf.CeilToInt((float)Scene.Mission.ExperienceBase() * num5 / 100f);
						Scene.Mission.bonusExperiencePercent += num5;
						text += string.Format(Strings.GetString("You found {0} xp bonus"), num6);
						text += "\n";
						continue;
					}
					if (reward == "soft")
					{
						float num3 = Util.RandomByWeights(bonusInfo.distribSoft.ToDictionary((GameData.FloatAndFloat p) => p.a, (GameData.FloatAndFloat p) => p.b));
						num3 *= IsHard() ? Singleton.GameData.difficultyHardCollectScMod : 1f;
						int num4 = Mathf.CeilToInt((float)Scene.Mission.SoftCurrencyBase() * num3 / 100f);
						Scene.Mission.bonusSoftCurrencyPercent += num3;
						text += string.Format(Strings.GetString("You found {0}@"), num4);
						text += "\n";
						continue;
					}
					if (reward == "consumable")
					{
						StackConsumable(Util.RandomByWeights(bonusInfo.distribConsumables.ToDictionary((GameData.StringAndFloat d) => d.s, (GameData.StringAndFloat d) => d.f)), ref consumables);
						continue;
					}
				}
			}
		}
		foreach (KeyValuePair<string, int> item in consumables)
		{
			GiveConsumable(item.Key, item.Value, ref text);
		}
		Scene.Mission.bonusesCollected++;
		Scene.Mission.ReportAction(WaitForAction.Type.CollectBonus, string.Empty);
		Singleton.GameEvents.OnBonusCollected(IsHard());
		Texture2D icon = null;
		if (consumables != null && consumables.Count > 0)
		{
			icon = (Texture2D)Resources.Load(Scene.Player.Inventory.Consumables[consumables.First().Key].GetComponent<Consumable>().HudIconTexturePath);
		}
		Singleton.UI.Hud.ShowBonus(text, icon);
		SoundManager.Play("bonus_pickup", false);
		Invoke("PostGive", 1f);
		GetComponent<ActivateItem>().Disappear();
	}

	private void StackConsumable(string item, ref Dictionary<string, int> consumables)
	{
		if (consumables.ContainsKey(item))
		{
			consumables[item]++;
		}
		else
		{
			consumables[item] = 1;
		}
	}

	private void GiveConsumable(string bonus, int count, ref string text)
	{
		Scene.Player.Inventory.AddConsumable(bonus, count, true, true);
		string text2 = Strings.GetString(Singleton.ShopData.GetCommonShopItemByItem(bonus).nameId);
		text += Color.white + string.Format(Strings.GetString("You found {1} {0}"), GUIUtils.ColorYellowTitle.ToString() + text2 + Color.white, count) + "\n";
	}

	private void PostGive()
	{
	}
}
