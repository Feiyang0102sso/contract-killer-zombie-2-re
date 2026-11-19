using System;
using System.Collections.Generic;
using Glu.Kontagent;
using Glu.MobileAppTracking;
using UnityEngine;

public static class GameAnalytics
{
	public enum NotificationType
	{
		Local,
		Push
	}

	public enum IntegrityType
	{
		Genuine,
		Jailbroken
	}

	public enum HCSourceType
	{
		IAP,
		Game,
		Tapjoy
	}

	public enum SCSourceType
	{
		IAP,
		Other
	}

	public enum IAPFailed
	{
		ResumeTimeoutFailed,
		ParentialSettingsOn,
		Other
	}

	public enum OfferChoice
	{
		Accepted,
		Rejected,
		Unlucky
	}

	public enum PurchaseScreenType
	{
		Shop,
		RecommendedMission,
		RecommendedSurvival,
		RecommendedHardMission,
		Upsell,
		ItemUnlock,
		ConsumablesOffer,
		TooDifficult,
		HCSample,
		UncertainGameplay,
		UncertainMenu
	}

	public enum AfterTutorialEvent
	{
		InternalSystemInitialized,
		FirstPoolMissionStarted,
		FirstPoolMissionCompleted,
		TapjoyButtonTapped,
		ReturnedFromTJOfferwall,
		SurvivalModeStarted,
		SurvivalModeCompleted,
		ArenaModeStarted,
		ArenaModeCompleted,
		SpecialHuntStarted,
		SpecialHuntCompleted
	}

	private static string _abTestingGroupID = string.Empty;

	private static float _ABDownloadingStartTime = 0f;

	private static SCSourceType _nextSCSourceType = SCSourceType.Other;

	private static List<KeyValuePair<string, Dictionary<string, object>>> _cachedEvents = new List<KeyValuePair<string, Dictionary<string, object>>>();

	private static void LogEvent(string id, Dictionary<string, object> data)
	{
		CFlurry.LogEvent(id, data);
	}

	private static int GetPlayerLevel()
	{
		return Singleton.Exists() ? Singleton.PlayerData.Level : 0;
	}

	private static Mission.Type GetNextStorylineMission()
	{
		if (Singleton.Exists() && PlayerData.Instance.StorylineTicket < Singleton.GameData.GetStorylineLength())
		{
			return (Mission.Type)(5 + PlayerData.Instance.StorylineTicket);
		}
		return Mission.Type.Story1;
	}

	public static void LogAppStart()
	{
		LogEvent("APP_START", new Dictionary<string, object>());
	}

	public static void LogJailbrokenAppStart()
	{
		LogEvent("JB_APP_START", new Dictionary<string, object>());
	}

	public static void LogAppStop(int playerLevel, float activityTime, int missionsCompleted)
	{
		LogEvent("APP_STOP", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "ActivityTime", activityTime },
			{ "MissionsCompleted", missionsCompleted }
		});
	}

	public static void LogGameStart()
	{
		LogEvent("GAME_START", new Dictionary<string, object>());
	}

	public static void LogGameStop(int playerLevel, float activityTime, int missionsCompleted)
	{
		LogEvent("GAME_STOP", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "ActivityTime", activityTime },
			{ "MissionsCompleted", missionsCompleted }
		});
	}

	public static void LogNotificationStart(NotificationType notificationType)
	{
		LogEvent("NOTIFICATION_START", new Dictionary<string, object> { { "NotificationType", notificationType } });
	}

	public static void LogNotificationSettingsChanged(bool enabled)
	{
		Kontagent.LogEvent("Push_enabled", "PUSH_TOKEN", string.Empty, _abTestingGroupID, GetPlayerLevel(), enabled ? 1 : 0);
	}

	public static void LogCrash()
	{
		LogEvent("CRASH", new Dictionary<string, object>
		{
			{
				"Year",
				DateTime.Now.Year
			},
			{
				"Month",
				DateTime.Now.Month
			},
			{
				"Day",
				DateTime.Now.Day
			},
			{
				"Hour",
				DateTime.Now.Hour
			},
			{
				"Minute",
				DateTime.Now.Minute
			},
			{
				"Second",
				DateTime.Now.Second
			}
		});
	}

	public static void LogJailbroken(IntegrityType integrity)
	{
		LogEvent("JAILBROKEN", new Dictionary<string, object> { { "IntegrityType", integrity } });
	}

	public static void LogPushToken(string deviceToken, bool enabled)
	{
		LogEvent("PUSH_NOTIFICATION", new Dictionary<string, object>
		{
			{ "DeviceToken", deviceToken },
			{ "Enabled", enabled }
		});
	}

	public static void LogABTesting(string groupID)
	{
		if (groupID != null)
		{
			_abTestingGroupID = groupID;
			Kontagent.LogEvent(groupID, "AB_TESTING", string.Empty, _abTestingGroupID, GetPlayerLevel(), 0);
		}
	}

	public static void LogFirstStart(string meleeWeapon, string handgunWeapon, string consumable, int consumableCount)
	{
		LogEvent("FIRST_START", new Dictionary<string, object>
		{
			{ "MeleeWeapon", meleeWeapon },
			{ "HandgunWeapon", handgunWeapon },
			{ "Consumable", consumable },
			{ "ConsumableCount", consumableCount }
		});
	}

	public static void LogAssetBundlesStarted()
	{
		_ABDownloadingStartTime = Time.realtimeSinceStartup;
		Kontagent.LogEvent("Bundles_load_start", "ASSET_BUNDLE", string.Empty, _abTestingGroupID, 0, 0);
	}

	public static void LogAssetBundlesCompleted(bool success)
	{
		int value = (int)(Time.realtimeSinceStartup - _ABDownloadingStartTime);
		Kontagent.LogEvent("Bundles_load_end", "ASSET_BUNDLE", string.Empty, _abTestingGroupID, 0, value);
	}

	public static void LogGameRated(bool isRated)
	{
		LogEvent("GAME_RATED", new Dictionary<string, object> { 
		{
			"IsRated",
			isRated ? 1 : 0
		} });
	}

	public static void LogDailyBonus(int daysPlayed, string giftReceived)
	{
		LogEvent("DAILY_BONUS", new Dictionary<string, object>
		{
			{ "DaysPlayed", daysPlayed },
			{ "GiftReceived", giftReceived }
		});
		Kontagent.LogEvent("Bonus_received", "DAILY_BONUS", string.Empty, _abTestingGroupID, GetPlayerLevel(), daysPlayed);
	}

	public static void LogTapjoyPointsReceived(int numberOfUnits, string currencyType)
	{
		LogEvent("TAPJOY_HC_RECEIVED", new Dictionary<string, object>
		{
			{ "NumberOfUnits", numberOfUnits },
			{ "CurrencyType", currencyType }
		});
	}

	public static void LogHCReceived(int amount, HCSourceType sourceType)
	{
		switch (sourceType)
		{
		case HCSourceType.IAP:
			Kontagent.LogEvent("Source", "SINK_SOURCE", "HC_IAP", _abTestingGroupID, GetPlayerLevel(), amount);
			break;
		case HCSourceType.Game:
			Kontagent.LogEvent("Source", "SINK_SOURCE", "HC_GAME", _abTestingGroupID, GetPlayerLevel(), amount);
			break;
		case HCSourceType.Tapjoy:
			Kontagent.LogEvent("Source", "SINK_SOURCE", "HC_TAPJOY", _abTestingGroupID, GetPlayerLevel(), amount);
			break;
		}
	}

	public static void SetNextSCSourceType(SCSourceType sourceType)
	{
		_nextSCSourceType = sourceType;
	}

	public static void LogSCReceived(int amount)
	{
		switch (_nextSCSourceType)
		{
		case SCSourceType.IAP:
			Kontagent.LogEvent("Source", "SINK_SOURCE", "SC_IAP", _abTestingGroupID, GetPlayerLevel(), amount);
			break;
		case SCSourceType.Other:
			Kontagent.LogEvent("Source", "SINK_SOURCE", "SC", _abTestingGroupID, GetPlayerLevel(), amount);
			break;
		}
		_nextSCSourceType = SCSourceType.Other;
	}

	public static void LogSCSpent(int amount, string itemName)
	{
		Kontagent.LogEvent("Sink", "SINK_SOURCE", "SC", _abTestingGroupID, GetPlayerLevel(), amount, new Dictionary<string, string> { { "itemName", itemName } });
	}

	public static void LogHCSpent(int amount, string itemName)
	{
		Kontagent.LogEvent("Sink", "SINK_SOURCE", "HC", _abTestingGroupID, GetPlayerLevel(), amount, new Dictionary<string, string> { { "itemName", itemName } });
	}

	public static void LogIAPBuyClicked(string itemName, int numberOfUnits, string currencyType, int playerLevel, decimal price)
	{
		LogEvent("IAP_BUY_CLICKED", new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "NumberOfUnits", numberOfUnits },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{ "Price", price }
		});
		PurchaseScreenType purchaseScreenType = GetPurchaseScreenType();
		Kontagent.LogEvent("IAP_clicked", "ADD_CURRENCY", purchaseScreenType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
		Kontagent.LogEvent(itemName, "ECONOMY", "IAP_CLICKED", _abTestingGroupID, GetPlayerLevel(), numberOfUnits);
	}

	public static void LogIAPCompleted(string itemName, int numberOfUnits, string currencyType, int playerLevel, decimal price, bool limitedTimeOffer)
	{
		MobileAppTracking.TrackAction("iap_purchased", itemName, (float)price, "USD");
		if (!Integrity.IsJailbroken())
		{
			Kontagent.RevenueTracking((int)(price * 100m));
		}
		else
		{
			Debug.Log("Revenue tracking cancelled (jailbroken device)");
		}
		LogEvent("IAP_COMPLETED", new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "NumberOfUnits", numberOfUnits },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{ "Price", price },
			{
				"LimitedTimeOffer",
				limitedTimeOffer ? 1 : 0
			}
		});
		Kontagent.LogEvent(itemName, "ECONOMY", "IAP_SUCCESS", _abTestingGroupID, GetPlayerLevel(), numberOfUnits);
	}

	public static void LogJBIAPCompleted(string itemName, int numberOfUnits, string currencyType, int playerLevel, decimal price, bool limitedTimeOffer)
	{
		LogEvent("JB_IAP_COMPLETED", new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "NumberOfUnits", numberOfUnits },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{ "Price", price },
			{
				"LimitedTimeOffer",
				limitedTimeOffer ? 1 : 0
			}
		});
	}

	public static void LogIAPPurchaseCancelled(string itemName, int numberOfUnits, string currencyType, int playerLevel, decimal price)
	{
		LogEvent("IAP_PURCHASE_CANCELLED", new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "NumberOfUnits", numberOfUnits },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{ "Price", price }
		});
		Kontagent.LogEvent(itemName, "ECONOMY", "IAP_CANCELLED", _abTestingGroupID, GetPlayerLevel(), numberOfUnits);
	}

	public static void LogIAPFailed(string itemName, int numberOfUnits, string currencyType, int playerLevel, decimal price, IAPFailed reason)
	{
		LogEvent("IAP_FAILED", new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "NumberOfUnits", numberOfUnits },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{ "Price", price },
			{ "Reason", reason }
		});
		Kontagent.LogEvent(itemName, "ECONOMY", "IAP_FAILED", _abTestingGroupID, GetPlayerLevel(), numberOfUnits);
	}

	public static void LogOneTimeOffer(string iapName, OfferChoice choice, int playerLevel)
	{
		LogEvent("ONE_TIME_OFFER", new Dictionary<string, object>
		{
			{ "IAPName", iapName },
			{ "UserNumber", 0 },
			{
				"UserChoice",
				(int)choice
			},
			{ "PlayerLevel", playerLevel }
		});
	}

	public static void LogDailyDeal(string offerID, OfferChoice choice, int playerLevel)
	{
		LogEvent("DAILY_DEAL", new Dictionary<string, object>
		{
			{ "OfferID", offerID },
			{
				"UserChoice",
				(int)choice
			},
			{ "PlayerLevel", playerLevel }
		});
	}

	public static void LogAdShown(string adId)
	{
		LogEvent("EVT_TYPE_AD_SHOWN", new Dictionary<string, object>
		{
			{ "always", 1 },
			{ "AdID", adId }
		});
	}

	public static void LogItemBuyClicked(string itemName, int price, string currencyType)
	{
		LogEvent("ITEM_BUY_CLICKED", new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "Price", price },
			{ "CurrencyType", currencyType }
		});
		if (currencyType == "soft")
		{
			LogSCSpent(price, itemName);
		}
		else
		{
			LogHCSpent(price, itemName);
		}
	}

	public static void LogStoreStats(string itemName)
	{
		LogEvent("STORE_STATS", new Dictionary<string, object> { { "ItemName", itemName } });
		Kontagent.LogEvent(itemName, "STORE_STATS", string.Empty, _abTestingGroupID, GetPlayerLevel(), 1);
	}

	private static PurchaseScreenType GetPurchaseScreenType()
	{
		PurchaseScreenType purchaseScreenTypeEx = GetPurchaseScreenTypeEx();
		Debug.Log("PurchaseScreenType: " + purchaseScreenTypeEx);
		return purchaseScreenTypeEx;
	}

	private static PurchaseScreenType GetPurchaseScreenTypeEx()
	{
		if (StoreScreen.ShopIsOpen)
		{
			return PurchaseScreenType.Shop;
		}
		if (Loader.IsGameScene())
		{
			if (Mission.IsSurvivalGates(PlayerData.Instance.CurrentMissionType))
			{
				return PurchaseScreenType.RecommendedSurvival;
			}
			return PurchaseScreenType.UncertainGameplay;
		}
		NewItemAvailablePopup newItemAvailablePopup = Util.FindObjectOfType<NewItemAvailablePopup>();
		WeaponSellPopup weaponSellPopup = Util.FindObjectOfType<WeaponSellPopup>();
		ProposalAfterDeathPopup proposalAfterDeathPopup = Util.FindObjectOfType<ProposalAfterDeathPopup>();
		if ((bool)MapNarrative.Instance && MapNarrative.Instance.SelectedLocation != string.Empty && !newItemAvailablePopup && !weaponSellPopup)
		{
			if (MapNarrative.Instance.SelectedMissionDifficulty == Mission.Difficulty.Hard)
			{
				return PurchaseScreenType.RecommendedHardMission;
			}
			return PurchaseScreenType.RecommendedMission;
		}
		if ((bool)newItemAvailablePopup)
		{
			return PurchaseScreenType.ItemUnlock;
		}
		if ((bool)weaponSellPopup)
		{
			return PurchaseScreenType.Upsell;
		}
		if ((bool)proposalAfterDeathPopup)
		{
			switch (proposalAfterDeathPopup.PopupType)
			{
			case ProposalAfterDeathPopup.ProposalType.SpecialOffer:
				return PurchaseScreenType.HCSample;
			case ProposalAfterDeathPopup.ProposalType.ConsumablesOffer:
				return PurchaseScreenType.ConsumablesOffer;
			case ProposalAfterDeathPopup.ProposalType.TooDifficult:
				return PurchaseScreenType.TooDifficult;
			default:
				return PurchaseScreenType.UncertainMenu;
			}
		}
		return PurchaseScreenType.UncertainMenu;
	}

	private static int GetInfectionLevel()
	{
		if (Singleton.GameData.InfectionLevel.Enabled && Singleton.GameData.InfectionLevel.InfectionAffectsDifficulty && PlayerData.Instance.IsInfectionLevelCritical())
		{
			return 1;
		}
		return 0;
	}

	public static void LogHCConsumable(string itemName, int price, string currencyType, int playerLevel)
	{
		LogEvent("HC_CONSUMABLE_PURCHASED_" + itemName, new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "Price", price },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{
				"ScreenType",
				(int)GetPurchaseScreenType()
			},
			{
				"InfectionLevel",
				GetInfectionLevel()
			}
		});
		Kontagent.LogEvent(itemName, "HC_CONSUMABLE_PURCHASED", string.Empty, _abTestingGroupID, GetPlayerLevel(), 0, new Dictionary<string, string>
		{
			{
				"next_smission",
				GetNextStorylineMission().ToString()
			},
			{
				"menu",
				GetPurchaseScreenType().ToString()
			}
		});
	}

	public static void LogSCConsumable(string itemName, int price, string currencyType, int playerLevel)
	{
		LogEvent("SC_CONSUMABLE_PURCHASED_" + itemName, new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "Price", price },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{
				"ScreenType",
				(int)GetPurchaseScreenType()
			},
			{
				"InfectionLevel",
				GetInfectionLevel()
			}
		});
		Kontagent.LogEvent(itemName, "SC_CONSUMABLE_PURCHASED", string.Empty, _abTestingGroupID, GetPlayerLevel(), 1, new Dictionary<string, string>
		{
			{
				"next_smission",
				GetNextStorylineMission().ToString()
			},
			{
				"menu",
				GetPurchaseScreenType().ToString()
			}
		});
	}

	public static void LogHCDurable(string itemName, int price, string currencyType, int playerLevel)
	{
		LogEvent("HC_DURABLE_PURCHASED_" + itemName, new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "Price", price },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{
				"ScreenType",
				(int)GetPurchaseScreenType()
			},
			{
				"InfectionLevel",
				GetInfectionLevel()
			}
		});
		Kontagent.LogEvent(itemName, "HC_DURABLE_PURCHASED", string.Empty, _abTestingGroupID, GetPlayerLevel(), 1, new Dictionary<string, string>
		{
			{
				"next_smission",
				GetNextStorylineMission().ToString()
			},
			{
				"menu",
				GetPurchaseScreenType().ToString()
			}
		});
	}

	public static void LogSCDurable(string itemName, int price, string currencyType, int playerLevel)
	{
		LogEvent("SC_DURABLE_PURCHASED_" + itemName, new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "Price", price },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{
				"ScreenType",
				(int)GetPurchaseScreenType()
			},
			{
				"InfectionLevel",
				GetInfectionLevel()
			}
		});
		Kontagent.LogEvent(itemName, "SC_DURABLE_PURCHASED", string.Empty, _abTestingGroupID, GetPlayerLevel(), 1, new Dictionary<string, string>
		{
			{
				"next_smission",
				GetNextStorylineMission().ToString()
			},
			{
				"menu",
				GetPurchaseScreenType().ToString()
			}
		});
	}

	public static void LogHCBundle(string itemName, int price, string currencyType, int playerLevel)
	{
		LogEvent("HC_BUNDLE_PURCHASED_" + itemName, new Dictionary<string, object>
		{
			{ "ItemName", itemName },
			{ "Price", price },
			{ "CurrencyType", currencyType },
			{ "PlayerLevel", playerLevel },
			{
				"ScreenType",
				(int)GetPurchaseScreenType()
			},
			{
				"InfectionLevel",
				GetInfectionLevel()
			}
		});
		Kontagent.LogEvent(itemName, "HC_BUNDLE_PURCHASED", string.Empty, _abTestingGroupID, GetPlayerLevel(), 1, new Dictionary<string, string>
		{
			{
				"next_smission",
				GetNextStorylineMission().ToString()
			},
			{
				"menu",
				GetPurchaseScreenType().ToString()
			}
		});
	}

	public static void LogLevelAchieved(int level, int scAmount, int hcAmount)
	{
		LogEvent("LEVEL_ACHIEVED", new Dictionary<string, object>
		{
			{ "Level", level },
			{ "SCAmount", scAmount },
			{ "HCAmount", hcAmount }
		});
		Kontagent.LogEvent("LEVEL_ACHIEVED", "LEVEL_UP", string.Empty, _abTestingGroupID, GetPlayerLevel(), 1);
	}

	public static void LogLevelTime(int level, float time)
	{
		LogEvent("LEVEL_TIME", new Dictionary<string, object>
		{
			{ "Level", level },
			{ "Time", time }
		});
	}

	public static void LogAchievementCompleted(string achievement)
	{
		Kontagent.LogEvent(achievement, "CHALLENGE", "CHALLENGE_UNLOCKED", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogGoalCompleted(int playerLevel, int ticket)
	{
		LogEvent("GOAL_COMPLETED", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "GoalID", ticket }
		});
		GameData.GoalData goalData = Singleton.GameData.Goals[ticket];
		string name = goalData.Type.ToString() + "_" + goalData.Target.ToString();
		Kontagent.LogEvent(name, "CHALLENGE", "CHALLENGE_COMPLETED", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogPunchcardCompleted(int playerLevel, int ticket)
	{
		LogEvent("PUNCHCARD_COMPLETED", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "PunchcardID", ticket }
		});
		GameData.CumulativeGoalData cumulativeGoalData = Singleton.GameData.CumulativeGoals[ticket];
		string title = cumulativeGoalData.Title;
		Kontagent.LogEvent(title, "CHALLENGE", "PUNCHCARD_COMPLETED", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogKillUncommonZombie(string zombie, string weaponOrConsumable, bool isConsumable, int numKills)
	{
		LogEvent("KILL_UNCOMMON_ZOMBIE_" + zombie, new Dictionary<string, object>
		{
			{ "Weapon", weaponOrConsumable },
			{
				"WeaponType",
				isConsumable ? 1 : 0
			},
			{ "NumberOfKills", numKills }
		});
	}

	public static void LogKillSpecialZombie(string zombie, string weaponOrConsumable, bool isConsumable, int numKills)
	{
		LogEvent("KILL_SPECIAL_ZOMBIE_" + zombie, new Dictionary<string, object>
		{
			{ "Weapon", weaponOrConsumable },
			{
				"WeaponType",
				isConsumable ? 1 : 0
			},
			{ "NumberOfKills", numKills }
		});
	}

	public static void LogHeadshot(string zombie, string weaponOrConsumable, bool isConsumable, int numHeadshots)
	{
		LogEvent("HEADSHOT_" + zombie, new Dictionary<string, object>
		{
			{ "Weapon", weaponOrConsumable },
			{
				"WeaponType",
				isConsumable ? 1 : 0
			},
			{ "NumberOfHeadshots", numHeadshots }
		});
	}

	public static void LogReviveUsed(Mission.Type missionType, int playerLevel, bool isPurchased)
	{
		LogEvent("MISSION_REVIVE_USED", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "PlayerLevel", playerLevel },
			{
				"Purchased",
				isPurchased ? 1 : 0
			}
		});
		Kontagent.LogEvent("Revive", "CONSUMABLE_USED", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), isPurchased ? 1 : 0);
	}

	public static void LogConsumableUsed(string itemName)
	{
		Mission.Type currentMissionType = Singleton.PlayerData.CurrentMissionType;
		Kontagent.LogEvent(itemName, "CONSUMABLE_USED", currentMissionType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogSurvivalWaveCompleted(int playerLevel, int wave, int score, int toolkits)
	{
		LogEvent("SM_FINISHED_WAVE", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "Wave", wave },
			{ "Score", score },
			{ "Toolkits", toolkits }
		});
		Kontagent.LogEvent("Wave_finished", "SM_WAVE", string.Empty, _abTestingGroupID, GetPlayerLevel(), wave);
	}

	public static void LogTutorialStepCompleted(Tutorial.AnalyticEvents tutorialStep)
	{
		int num = (int)tutorialStep;
		if (Singleton.Exists() && Singleton.PlayerData.TutorialAnalyticsStep < num)
		{
			string text = num.ToString("00") + "_" + tutorialStep.ToString();
			Debug.Log("LOG TUTORIAL STEP: " + text);
			LogEvent("TUTORIAL_STEP_COMPLETED", new Dictionary<string, object>
			{
				{ "StepID", text },
				{
					"Mission#",
					Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
				}
			});
			Kontagent.LogEvent(num.ToString(), "TUTORIAL_STEPS", string.Empty, _abTestingGroupID, GetPlayerLevel(), 0);
			Singleton.PlayerData.TutorialAnalyticsStep = num;
		}
	}

	public static void LogMissionStarted(Mission.Type missionType, int missionID, int playerLevel, Mission.Difficulty difficulty)
	{
		LogEvent("MISSION_STARTED", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{
				"InfectionLevel",
				GetInfectionLevel()
			},
			{
				"Difficulty",
				(int)difficulty
			},
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		});
		Kontagent.LogEvent(difficulty.ToString(), "MISSION_START", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogMissionCompleted(Mission.Type missionType, int missionID, int playerLevel, Mission.Difficulty difficulty)
	{
		LogEvent("MISSION_COMPLETED", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{
				"Difficulty",
				(int)difficulty
			},
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		});
		Kontagent.LogEvent(difficulty.ToString(), "MISSION_COMPLETE", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
		Kontagent.LogEvent(string.Empty, "LAST_MISSION_PLAYED", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogMissionFailed(Mission.Type missionType, int missionID, int playerLevel, GameEvents.MissionFailure failureType, string playerKiller, Mission.Difficulty difficulty)
	{
		LogEvent("MISSION_FAILED", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{
				"FailureType",
				(int)failureType
			},
			{ "PlayerKiller", playerKiller },
			{
				"InfectionLevel",
				GetInfectionLevel()
			},
			{
				"Difficulty",
				(int)difficulty
			},
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		});
		int value = (!(failureType != GameEvents.MissionFailure.PlayerKilled) ? 0 : 1);
		Kontagent.LogEvent(difficulty.ToString(), "MISSION_FAILED", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), value);
		Kontagent.LogEvent(string.Empty, "LAST_MISSION_PLAYED", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogAllBonusCratesFound(Mission.Type missionType, int missionID, int playerLevel)
	{
		LogEvent("ALL_BONUS_CRATES_FOUND", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		});
	}

	public static void LogSurvivorDelivered(int playerLevel, int terminalNumber)
	{
		LogEvent("MISSION_SURVIVOR_DELIVERED", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "TerminalNumber", terminalNumber },
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		});
	}

	public static void LogInfectionLevelPopup(int playerLevel, bool bombWasUsed, int hcAmount)
	{
		LogEvent("INFECTION_LEVEL_BOMB_POPUP", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{
				"PopupCancelled",
				!bombWasUsed
			},
			{ "BombUsed", bombWasUsed },
			{ "AmountOfHC", hcAmount }
		});
	}

	public static void LogCompletelyOutOfAmmo(Mission.Type missionType, Mission.Difficulty difficulty)
	{
		Kontagent.LogEvent(difficulty.ToString(), "MISSION_NO_AMMO", missionType.ToString(), _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogNextStoryMission()
	{
		Kontagent.LogEvent(string.Empty, "NEXT_STORY_MISSION", GetNextStorylineMission().ToString(), _abTestingGroupID, GetPlayerLevel(), 1);
	}

	private static string WeaponName(List<Weapon> weapons, int index)
	{
		return !(index >= weapons.Count) ? weapons[index].gameObject.name : string.Empty;
	}

	public static int NumCachedEvents
	{
		get
		{
			return _cachedEvents.Count;
		}
	}

	public static void FlushCachedEvents(int maxNumEvents)
	{
		while (maxNumEvents > 0 && _cachedEvents.Count > 0)
		{
			string text = "FLUSH:" + _cachedEvents[0].Key + "(";
			foreach (KeyValuePair<string, object> item in _cachedEvents[0].Value)
			{
				text = text + item.Key + ":" + item.Value.ToString() + ", ";
			}
			text += ")";
			Debug.Log(text);
			LogEvent(_cachedEvents[0].Key, _cachedEvents[0].Value);
			_cachedEvents.RemoveAt(0);
			if (_cachedEvents.Count == 0)
			{
				break;
			}
		}
	}

	public static void CacheLogMissionWeaponsAtEnd(List<Weapon> weapons)
	{
		_cachedEvents.Add(new KeyValuePair<string, Dictionary<string, object>>("MISSION_WEAPONS_AT_END", new Dictionary<string, object>
		{
			{
				"Weapon1",
				WeaponName(weapons, 0)
			},
			{
				"Weapon2",
				WeaponName(weapons, 1)
			},
			{
				"Weapon3",
				WeaponName(weapons, 2)
			},
			{
				"Weapon4",
				WeaponName(weapons, 3)
			},
			{
				"Weapon5",
				WeaponName(weapons, 4)
			}
		}));
	}

	public static void CacheLogWeaponSwitched(Mission.Type missionType, int missionID, int playerLevel, string switchedTo, string switchedFrom)
	{
		_cachedEvents.Add(new KeyValuePair<string, Dictionary<string, object>>("MISSION_WEAPON_SWITCHED", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{ "SwitchedTo", switchedTo },
			{ "SwitchedFrom", switchedFrom },
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		}));
	}

	public static void CacheLogWeaponOutOfAmmo(Mission.Type missionType, int missionID, int playerLevel, string itemName)
	{
		_cachedEvents.Add(new KeyValuePair<string, Dictionary<string, object>>("MISSION_OUT_OF_AMMO", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{ "ItemName", itemName },
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		}));
	}

	public static void CacheLogWeaponOutOfAmmoCompletely(Mission.Type missionType, int missionID, int playerLevel, string meeleeWeapon)
	{
		_cachedEvents.Add(new KeyValuePair<string, Dictionary<string, object>>("MISSION_OUT_OF_AMMO_COMPLETELY", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{ "MeeleeWeapon", meeleeWeapon },
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		}));
	}

	public static void CacheLogAmmoCrateFound(Mission.Type missionType, int missionID, int playerLevel)
	{
		_cachedEvents.Add(new KeyValuePair<string, Dictionary<string, object>>("MISSION_AMMO_CRATE_FOUND", new Dictionary<string, object>
		{
			{ "MissionType", missionType },
			{ "MissionID", missionID },
			{ "PlayerLevel", playerLevel },
			{
				"Mission#",
				Singleton.PlayerData.PlayerStatistics.NumMissionsCompleted
			}
		}));
	}

	public static void CacheLogAutofireWrongUsage(int playerLevel)
	{
		_cachedEvents.Add(new KeyValuePair<string, Dictionary<string, object>>("AUTOFIRE_IS_UNCLEAR", new Dictionary<string, object>
		{
			{ "Always", 1 },
			{ "PlayerLevel", playerLevel }
		}));
	}

	public static void LogAMFinishedWave(int playerLevel, int wave, int score, int revives, int mercenaries)
	{
		LogEvent("AM_FINISHED_WAVE", new Dictionary<string, object>
		{
			{ "PlayerLevel", playerLevel },
			{ "Wave", wave },
			{ "Score", score },
			{ "Revives", revives },
			{ "Mercenaries", mercenaries }
		});
		Kontagent.LogEvent("Wave_finished", "AM_WAVE", string.Empty, _abTestingGroupID, GetPlayerLevel(), wave);
	}

	public static void LogFacebookLogin()
	{
		LogEvent("FACEBOOK_LOGIN", new Dictionary<string, object>());
		Kontagent.LogEvent("PRESSED", "FACEBOOK", "LOGIN", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogFacebookLoginCompleted()
	{
		Kontagent.LogEvent("COMPLETED", "FACEBOOK", "LOGIN", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogFacebookSurvivalPressed()
	{
		Kontagent.LogEvent("PRESSED", "FACEBOOK", "SURVIVAL", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogFacebookLevelUpPressed()
	{
		Kontagent.LogEvent("PRESSED", "FACEBOOK", "LEVELUP", _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogICloudButtonPressed()
	{
		Kontagent.LogEvent("PRESSED", "ICLOUD", string.Empty, _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogICloudSaveButtonPressed()
	{
		Kontagent.LogEvent("SAVE", "ICLOUD", string.Empty, _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static void LogICloudLoadButtonPressed()
	{
		Kontagent.LogEvent("LOAD", "ICLOUD", string.Empty, _abTestingGroupID, GetPlayerLevel(), 0);
	}

	public static bool IsRepeatableAfterTutorialEvent(AfterTutorialEvent afterTutorialEvent)
	{
		switch (afterTutorialEvent)
		{
		case AfterTutorialEvent.TapjoyButtonTapped:
		case AfterTutorialEvent.ReturnedFromTJOfferwall:
			return true;
		}
		return false;
	}

	public static void LogAfterTutorial(AfterTutorialEvent afterTutorialEvent)
	{
		LogEvent("AFTER_TUTORIAL", new Dictionary<string, object> { { "Event", afterTutorialEvent } });
	}
}
