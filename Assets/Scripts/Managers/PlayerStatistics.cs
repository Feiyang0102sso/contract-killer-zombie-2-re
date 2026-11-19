using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Glu.Localization;
using UnityEngine;

public class StatisticsLogger : LoggerSingleton<GUILogger>
{
	public StatisticsLogger()
	{
		LoggerSingleton<GUILogger>.SetLoggerName("App.Stats");
	}
}

[LocalizeAllPropertiesByName]
public class MissionStatistics
{
	private const int SurvivalPerZombieScore = 100;

	private const int SurvivalPerSpecialScore = 500;

	private const int SurvivalPerHeadshotScore = 110;

	private const int SurvivalPerDismembermentScore = 0;

	public XmlDictionary<string, int> NumWeaponShots = new XmlDictionary<string, int>();

	public XmlDictionary<string, int> WeaponDamage = new XmlDictionary<string, int>();

	public XmlDictionary<string, int> NumConsumablesUsed = new XmlDictionary<string, int>();

	public XmlDictionary<string, int> ConsumablesDamage = new XmlDictionary<string, int>();

	public XmlDictionary<KeyValuePair<string, string>, int> NumEnemyHits = new XmlDictionary<KeyValuePair<string, string>, int>();

	public XmlDictionary<KeyValuePair<string, string>, int> NumEnemyHeadshots = new XmlDictionary<KeyValuePair<string, string>, int>();

	public XmlDictionary<KeyValuePair<string, string>, int> NumEnemyDismemberments = new XmlDictionary<KeyValuePair<string, string>, int>();

	public XmlDictionary<string, int> NumEnemyMutilations = new XmlDictionary<string, int>();

	public XmlDictionary<KeyValuePair<string, string>, int> NumEnemyKills = new XmlDictionary<KeyValuePair<string, string>, int>();

	public XmlDictionary<string, int> NumEnemySpawns = new XmlDictionary<string, int>();

	public XmlDictionary<string, int> NumTasksCompleted = new XmlDictionary<string, int>();

	public XmlDictionary<string, int> NumPlayerKills = new XmlDictionary<string, int>();

	public XmlDictionary<string, int> NumAssistedEnemyKills = new XmlDictionary<string, int>();

	public int NumSurvivorsRescued;

	public int NumSuppliesGathered;

	public int NumAmmoResupplied;

	public int NumBonusCollected;

	public int NumHardBonusCollected;

	public int DistanceTravelled;

	public int SurvivalGatesBestScore;

	public int SurvivalArenaBestScore;

	[XmlIgnore]
	public int NumWavesCompleted;

	[XmlIgnore]
	public int NumToolkitsPurchased;

	public bool IsConsumable(string itemName)
	{
		foreach (KeyValuePair<string, int> item in NumConsumablesUsed)
		{
			if (item.Key == itemName)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWeapon(string itemName)
	{
		foreach (KeyValuePair<string, int> numWeaponShot in NumWeaponShots)
		{
			if (numWeaponShot.Key == itemName)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSpecial(string enemyName)
	{
		Type type = Type.GetType(enemyName);
		if (type.Name == "PiperMinion")
		{
			return false;
		}
		return type.BaseType.Name == "Special";
	}

	public bool IsUncommon(string enemyName)
	{
		Type type = Type.GetType(enemyName);
		if (type.Name == "PiperMinion")
		{
			return true;
		}
		return type.Name == "Uncommon" || type.BaseType.Name == "Uncommon";
	}

	public int SurvivorsRescued
	{
		get
		{
			return NumSurvivorsRescued;
		}
	}

	public int SuppliesGathered
	{
		get
		{
			return NumSuppliesGathered;
		}
	}

	public int BonusCollected
	{
		get
		{
			return NumBonusCollected;
		}
	}

	public int TotalNumWeaponShots
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, int> numWeaponShot in NumWeaponShots)
			{
				if (Singleton.ShopData.GetWeaponShopItemByItem(numWeaponShot.Key).slot == PlayerData.Slots.melee)
				{
					continue;
				}
				num += numWeaponShot.Value;
			}
			return num;
		}
	}

	public int TotalNumConsumablesUsed
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, int> item in NumConsumablesUsed)
			{
				num += item.Value;
			}
			return num;
		}
	}

	private void ForEachWeaponAndEnemy(XmlDictionary<string, int> weapons, XmlDictionary<KeyValuePair<string, string>, int> enemies, Action<int> action)
	{
		foreach (KeyValuePair<string, int> weapon in weapons)
		{
			foreach (KeyValuePair<KeyValuePair<string, string>, int> enemy in enemies)
			{
				if (weapon.Key == enemy.Key.Value)
				{
					action(enemy.Value);
				}
			}
		}
	}

	private void ForEachWeaponAndEnemy(XmlDictionary<string, int> weapons, PlayerData.Slots slot, XmlDictionary<KeyValuePair<string, string>, int> enemies, Action<int> action)
	{
		foreach (KeyValuePair<string, int> weapon in weapons)
		{
			if (Singleton.ShopData.GetWeaponShopItemByItem(weapon.Key).slot != slot)
			{
				continue;
			}
			foreach (KeyValuePair<KeyValuePair<string, string>, int> enemy in enemies)
			{
				if (weapon.Key == enemy.Key.Value)
				{
					action(enemy.Value);
				}
			}
		}
	}

	public int TotalNumWeaponHits
	{
		get
		{
			int result = 0;
			PlayerData.Slots[] array = new PlayerData.Slots[5]
			{
				PlayerData.Slots.assault,
				PlayerData.Slots.exotic,
				PlayerData.Slots.handgun,
				PlayerData.Slots.shotgun,
				PlayerData.Slots.sniper
			};
			foreach (PlayerData.Slots slot in array)
			{
				ForEachWeaponAndEnemy(NumWeaponShots, slot, NumEnemyHits, (int x) =>
				{
					result += x;
				});
			}
			return result;
		}
	}

	public int Accuracy
	{
		get
		{
			return Mathf.Min((int)(100f * (float)TotalNumWeaponHits / (float)TotalNumWeaponShots), 100);
		}
	}

	public int TotalNumKills
	{
		get
		{
			return TotalNumWeaponKills + TotalNumConsumableKills + TotalNumAssistedKills;
		}
	}

	public int TotalNumWeaponKills
	{
		get
		{
			int result = 0;
			ForEachWeaponAndEnemy(NumWeaponShots, NumEnemyKills, (int x) =>
			{
				result += x;
			});
			return result;
		}
	}

	public int TotalNumAssistedKills
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, int> numAssistedEnemyKill in NumAssistedEnemyKills)
			{
				num += numAssistedEnemyKill.Value;
			}
			return num;
		}
	}

	public int TotalNumConsumableKills
	{
		get
		{
			int result = 0;
			ForEachWeaponAndEnemy(NumConsumablesUsed, NumEnemyKills, (int x) =>
			{
				result += x;
			});
			return result;
		}
	}

	public int TotalNumWeaponHeadshots
	{
		get
		{
			int result = 0;
			ForEachWeaponAndEnemy(NumWeaponShots, NumEnemyHeadshots, (int x) =>
			{
				result += x;
			});
			return result;
		}
	}

	public int TotalNumWeaponDismemberments
	{
		get
		{
			int result = 0;
			ForEachWeaponAndEnemy(NumWeaponShots, NumEnemyDismemberments, (int x) =>
			{
				result += x;
			});
			return result;
		}
	}

	public int TotalNumPlayerKills
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, int> numPlayerKill in NumPlayerKills)
			{
				num += numPlayerKill.Value;
			}
			return num;
		}
	}

	public int TotalNumSpecialKills
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<KeyValuePair<string, string>, int> numEnemyKill in NumEnemyKills)
			{
				if (IsSpecial(numEnemyKill.Key.Key))
				{
					num += numEnemyKill.Value;
				}
			}
			foreach (KeyValuePair<string, int> numAssistedEnemyKill in NumAssistedEnemyKills)
			{
				if (IsSpecial(numAssistedEnemyKill.Key))
				{
					num += numAssistedEnemyKill.Value;
				}
			}
			return num;
		}
	}

	public int TotalWeaponDamage
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, int> item in WeaponDamage)
			{
				num += item.Value;
			}
			return num;
		}
	}

	public int AverageWeaponFirepower
	{
		get
		{
			return Mathf.Min((int)((float)TotalWeaponDamage / (float)TotalNumWeaponHits), 100);
		}
	}

	public int TotalConsumablesDamage
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, int> item in ConsumablesDamage)
			{
				num += item.Value;
			}
			return num;
		}
	}

	private string GetFavoriteWeaponBySlot(PlayerData.Slots slot)
	{
		int num = -1;
		string text = string.Empty;
		foreach (KeyValuePair<string, int> numWeaponShot in NumWeaponShots)
		{
			if (Singleton.ShopData.GetWeaponShopItemByItem(numWeaponShot.Key).slot != slot)
			{
				continue;
			}
			int num2 = 0;
			foreach (KeyValuePair<KeyValuePair<string, string>, int> numEnemyKill in NumEnemyKills)
			{
				if (numEnemyKill.Key.Value == numWeaponShot.Key)
				{
					num2 += numEnemyKill.Value;
				}
			}
			if (num2 > num)
			{
				num = num2;
				text = numWeaponShot.Key;
			}
		}
		if (text != string.Empty)
		{
			text = Strings.GetString(Singleton.ShopData.GetWeaponShopItemByItem(text).nameId);
		}
		else
		{
			text = "-";
		}
		return text;
	}

	private int GetFavoriteWeaponKillRateBySlot(PlayerData.Slots slot)
	{
		int num = -1;
		foreach (KeyValuePair<string, int> numWeaponShot in NumWeaponShots)
		{
			if (Singleton.ShopData.GetWeaponShopItemByItem(numWeaponShot.Key).slot != slot)
			{
				continue;
			}
			int num2 = 0;
			foreach (KeyValuePair<KeyValuePair<string, string>, int> numEnemyKill in NumEnemyKills)
			{
				if (numEnemyKill.Key.Value == numWeaponShot.Key)
				{
					num2 += numEnemyKill.Value;
				}
			}
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num > 0)
		{
			int killRate = (int)(100f * (float)num / (float)(TotalNumWeaponKills + TotalNumConsumableKills));
			return killRate;
		}
		return 0;
	}

	public string FavoriteMeleeWeapon
	{
		get
		{
			return GetFavoriteWeaponBySlot(PlayerData.Slots.melee);
		}
	}

	public int FavoriteMeleeWeaponKillRate
	{
		get
		{
			return GetFavoriteWeaponKillRateBySlot(PlayerData.Slots.melee);
		}
	}

	public string FavoriteHandgunWeapon
	{
		get
		{
			return GetFavoriteWeaponBySlot(PlayerData.Slots.handgun);
		}
	}

	public int FavoriteHandgunWeaponKillRate
	{
		get
		{
			return GetFavoriteWeaponKillRateBySlot(PlayerData.Slots.handgun);
		}
	}

	public string FavoriteShotgunWeapon
	{
		get
		{
			return GetFavoriteWeaponBySlot(PlayerData.Slots.shotgun);
		}
	}

	public int FavoriteShotgunWeaponKillRate
	{
		get
		{
			return GetFavoriteWeaponKillRateBySlot(PlayerData.Slots.shotgun);
		}
	}

	public string FavoriteAssaultWeapon
	{
		get
		{
			return GetFavoriteWeaponBySlot(PlayerData.Slots.assault);
		}
	}

	public int FavoriteAssaultWeaponKillRate
	{
		get
		{
			return GetFavoriteWeaponKillRateBySlot(PlayerData.Slots.assault);
		}
	}

	public string FavoriteSniperWeapon
	{
		get
		{
			return GetFavoriteWeaponBySlot(PlayerData.Slots.sniper);
		}
	}

	public int FavoriteSniperWeaponKillRate
	{
		get
		{
			return GetFavoriteWeaponKillRateBySlot(PlayerData.Slots.sniper);
		}
	}

	public string FavoriteExoticWeapon
	{
		get
		{
			return GetFavoriteWeaponBySlot(PlayerData.Slots.exotic);
		}
	}

	public int FavoriteExoticWeaponKillRate
	{
		get
		{
			return GetFavoriteWeaponKillRateBySlot(PlayerData.Slots.exotic);
		}
	}

	public int SurvivalScoreZombies()
	{
		return TotalNumKills * 100;
	}

	public int SurvivalScoreSpecials()
	{
		return TotalNumSpecialKills * 500;
	}

	public int SurvivalScoreHeadshots()
	{
		return TotalNumWeaponHeadshots * 110;
	}

	public int SurvivalScoreDismemberments()
	{
		return TotalNumWeaponDismemberments * 0;
	}

	public int SurvivalScoreTotal()
	{
		return SurvivalScoreZombies() + SurvivalScoreSpecials() + SurvivalScoreHeadshots() + SurvivalScoreDismemberments();
	}

	public int ArenaScoreTotal()
	{
		return SurvivalArenaBestScore;
	}

	public int TotalNumSpawns()
	{
		int num = 0;
		foreach (KeyValuePair<string, int> numEnemySpawn in NumEnemySpawns)
		{
			num += numEnemySpawn.Value;
		}
		return num;
	}

	public int GetWeaponShotsBySlot(PlayerData.Slots slot)
	{
		int num = 0;
		foreach (KeyValuePair<string, int> numWeaponShot in NumWeaponShots)
		{
			WeaponShopItem weaponShopItemByItem = Singleton.ShopData.GetWeaponShopItemByItem(numWeaponShot.Key);
			if (weaponShopItemByItem != null && weaponShopItemByItem.Slot == slot)
			{
				num += numWeaponShot.Value;
			}
		}
		return num;
	}

	public int GetEnemyKillsBySlot(PlayerData.Slots slot)
	{
		int num = 0;
		foreach (KeyValuePair<KeyValuePair<string, string>, int> numEnemyKill in NumEnemyKills)
		{
			WeaponShopItem weaponShopItemByItem = Singleton.ShopData.GetWeaponShopItemByItem(numEnemyKill.Key.Value);
			if (weaponShopItemByItem != null && weaponShopItemByItem.Slot == slot)
			{
				num += numEnemyKill.Value;
			}
		}
		return num;
	}

	private void Add(XmlDictionary<string, int> dst, string key, int val)
	{
		if (dst.ContainsKey(key))
		{
			dst[key] = dst[key] + val;
		}
		else
		{
			dst.Add(key, val);
		}
	}

	private void Merge(XmlDictionary<string, int> dst, XmlDictionary<string, int> src)
	{
		foreach (KeyValuePair<string, int> item in src)
		{
			Add(dst, item.Key, item.Value);
		}
	}

	private void Add(XmlDictionary<KeyValuePair<string, string>, int> dst, KeyValuePair<string, string> key, int val)
	{
		if (dst.ContainsKey(key))
		{
			dst[key] = dst[key] + val;
		}
		else
		{
			dst.Add(key, val);
		}
	}

	private void Merge(XmlDictionary<KeyValuePair<string, string>, int> dst, XmlDictionary<KeyValuePair<string, string>, int> src)
	{
		foreach (KeyValuePair<KeyValuePair<string, string>, int> item in src)
		{
			Add(dst, item.Key, item.Value);
		}
	}

	public void Merge(MissionStatistics missionStatistics)
	{
		Merge(NumWeaponShots, missionStatistics.NumWeaponShots);
		Merge(WeaponDamage, missionStatistics.WeaponDamage);
		Merge(NumConsumablesUsed, missionStatistics.NumConsumablesUsed);
		Merge(ConsumablesDamage, missionStatistics.ConsumablesDamage);
		Merge(NumEnemyHits, missionStatistics.NumEnemyHits);
		Merge(NumEnemyKills, missionStatistics.NumEnemyKills);
		Merge(NumEnemyHeadshots, missionStatistics.NumEnemyHeadshots);
		Merge(NumEnemyDismemberments, missionStatistics.NumEnemyDismemberments);
		Merge(NumEnemyMutilations, missionStatistics.NumEnemyMutilations);
		Merge(NumTasksCompleted, missionStatistics.NumTasksCompleted);
		Merge(NumPlayerKills, missionStatistics.NumPlayerKills);
		Merge(NumAssistedEnemyKills, missionStatistics.NumAssistedEnemyKills);
		NumSurvivorsRescued += missionStatistics.NumSurvivorsRescued;
		NumSuppliesGathered += missionStatistics.NumSuppliesGathered;
		NumAmmoResupplied += missionStatistics.NumAmmoResupplied;
		DistanceTravelled += missionStatistics.DistanceTravelled;
		if (Scene.Mission.IsSurvivalGates())
		{
			SurvivalGatesBestScore = Mathf.Max(SurvivalGatesBestScore, missionStatistics.SurvivalScoreTotal());
		}
		else if (Scene.Mission.IsSurvivalArena())
		{
			SurvivalArenaBestScore = Mathf.Max(SurvivalArenaBestScore, missionStatistics.ArenaScoreTotal());
		}
	}

	public virtual void OnWeaponShot(Character source, Weapon weapon, int numPellets)
	{
		if (source is Player && (bool)weapon)
		{
			Add(NumWeaponShots, weapon.gameObject.name, numPellets);
		}
	}

	public virtual void OnConsumableUsed(Character source, Consumable consumable)
	{
		if (source is Player && (bool)consumable)
		{
			Add(NumConsumablesUsed, consumable.GetType().Name, 1);
		}
	}

	public virtual void OnEnemyHit(Character source, Unit unit, Weapon weapon, Consumable consumable, float damage)
	{
		if (source is Player && (bool)unit && ((bool)weapon || (bool)consumable))
		{
			string name = unit.GetType().Name;
			string text = weapon
				? weapon.gameObject.name
				: (consumable ? consumable.GetType().Name : string.Empty);
			Add(NumEnemyHits, new KeyValuePair<string, string>(name, text), 1);
			if ((bool)weapon)
			{
				Add(WeaponDamage, text, Mathf.CeilToInt(damage));
			}
			if ((bool)consumable)
			{
				Add(ConsumablesDamage, text, Mathf.CeilToInt(damage));
			}
		}
	}

	public virtual void OnEnemyKill(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		if ((bool)unit && ((bool)weapon || (bool)consumable))
		{
			string name = unit.GetType().Name;
			string value = weapon
				? weapon.gameObject.name
				: (consumable ? consumable.GetType().Name : string.Empty);
			if (source is Player)
			{
				Add(NumEnemyKills, new KeyValuePair<string, string>(name, value), 1);
			}
			else if (source is Human)
			{
				Add(NumAssistedEnemyKills, name, 1);
			}
		}
	}

	public virtual void OnEnemySpawned(Unit unit)
	{
		if ((bool)unit)
		{
			string name = unit.GetType().Name;
			Add(NumEnemySpawns, name, 1);
		}
	}

	public virtual void OnEnemyHeadshot(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		if (source is Player && (bool)unit && ((bool)weapon || (bool)consumable))
		{
			string name = unit.GetType().Name;
			string value = weapon
				? weapon.gameObject.name
				: (consumable ? consumable.GetType().Name : string.Empty);
			Add(NumEnemyHeadshots, new KeyValuePair<string, string>(name, value), 1);
		}
	}

	public virtual void OnEnemyDismemberment(Character source, Unit unit, Weapon weapon, Consumable consumable, bool isFatal)
	{
		if (source is Player && (bool)unit && unit.NumDismemberedParts == 0 && ((bool)weapon || (bool)consumable))
		{
			string name = unit.GetType().Name;
			string value = weapon
				? weapon.gameObject.name
				: (consumable ? consumable.GetType().Name : string.Empty);
			Add(NumEnemyDismemberments, new KeyValuePair<string, string>(name, value), 1);
		}
	}

	public virtual void OnEnemyMutilated(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		if (source is Player && (bool)unit && ((bool)weapon || (bool)consumable))
		{
			string key = weapon
				? weapon.gameObject.name
				: (consumable ? consumable.GetType().Name : string.Empty);
			Add(NumEnemyMutilations, key, 1);
		}
	}

	public virtual void OnTaskCompleted(Task task)
	{
		if (task.type == Task.Type.RescueSurvivor)
		{
			NumSurvivorsRescued++;
		}
		else if (task.type == Task.Type.KillAll && Scene.Mission.IsSurvival() && !(task is CustomTask))
		{
			NumWavesCompleted++;
		}
		Add(NumTasksCompleted, task.type.ToString(), 1);
	}

	public virtual void OnSupplyGathered(GameObject obj)
	{
		NumSuppliesGathered++;
	}

	public virtual void OnAmmoResupplied(GameObject obj)
	{
		NumAmmoResupplied++;
	}

	public virtual void OnPlayerKill(Player player, Unit unit)
	{
		Add(NumPlayerKills, unit ? unit.GetType().Name : "(unknown)", 1);
	}
}

[XmlInclude(typeof(MissionStatistics))]
public class PlayerStatistics : MissionStatistics
{
	[XmlIgnore]
	public MissionStatistics MissionStatistics = new MissionStatistics();

	[XmlIgnore]
	private static string _lastPlayerMurderer = string.Empty;

	[XmlIgnore]
	public bool Flush;

	[XmlIgnore]
	public float MissionStartTime;

	[XmlIgnore]
	public int NumMissionsFailedInRow;

	[XmlIgnore]
	public bool LastPlayedMissionWasCompleted;

	[XmlIgnore]
	public bool LastPlayedMissionWasCompletedWithLowHP;

	public int NumMissionsCompleted;

	public int NumMissionsFailed;

	public int NumWeaponsPurchased;

	public int NumConsumablesPurchased;

	public XmlDictionary<KeyValuePair<int, int>, int> SCPurchases = new XmlDictionary<KeyValuePair<int, int>, int>();

	public XmlDictionary<KeyValuePair<int, int>, int> HCPurchases = new XmlDictionary<KeyValuePair<int, int>, int>();

	[LocalizeByPropertyName]
	public int MissionsCompleted
	{
		get
		{
			return NumMissionsCompleted;
		}
	}

	[LocalizeByPropertyName]
	public int MissionsFailed
	{
		get
		{
			return NumMissionsFailed;
		}
	}

	[LocalizeByPropertyName]
	public int WeaponsPurchased
	{
		get
		{
			return NumWeaponsPurchased;
		}
	}

	[LocalizeByPropertyName]
	public int ConsumablesPurchased
	{
		get
		{
			return NumConsumablesPurchased;
		}
	}

	public int TotalSCPurchased()
	{
		int num = 0;
		foreach (KeyValuePair<KeyValuePair<int, int>, int> sCPurchase in SCPurchases)
		{
			num += sCPurchase.Key.Key * sCPurchase.Value;
		}
		return num;
	}

	public int TotalHCPurchased()
	{
		int num = 0;
		foreach (KeyValuePair<KeyValuePair<int, int>, int> hCPurchase in HCPurchases)
		{
			num += hCPurchase.Key.Key * hCPurchase.Value;
		}
		return num;
	}

	public float TotalPricePaid()
	{
		float result = 0f;
		Action<KeyValuePair<KeyValuePair<int, int>, int>> action = (KeyValuePair<KeyValuePair<int, int>, int> x) =>
		{
			result += x.Key.Value / 100 * x.Value;
		};
		foreach (KeyValuePair<KeyValuePair<int, int>, int> sCPurchase in SCPurchases)
		{
			action(sCPurchase);
		}
		foreach (KeyValuePair<KeyValuePair<int, int>, int> hCPurchase in HCPurchases)
		{
			action(hCPurchase);
		}
		return result;
	}

	public int GetSurvivalGatesBestScore()
	{
		return Mathf.Max(SurvivalGatesBestScore, MissionStatistics.SurvivalScoreTotal());
	}

	public int GetSurvivalArenaBestScore()
	{
		return Mathf.Max(SurvivalArenaBestScore, MissionStatistics.ArenaScoreTotal());
	}

	public override void OnWeaponShot(Character source, Weapon weapon, int numPellets)
	{
		MissionStatistics.OnWeaponShot(source, weapon, numPellets);
	}

	public override void OnConsumableUsed(Character source, Consumable consumable)
	{
		MissionStatistics.OnConsumableUsed(source, consumable);
	}

	public override void OnEnemyHit(Character source, Unit unit, Weapon weapon, Consumable consumable, float damage)
	{
		MissionStatistics.OnEnemyHit(source, unit, weapon, consumable, damage);
	}

	public override void OnEnemyKill(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		MissionStatistics.OnEnemyKill(source, unit, weapon, consumable);
	}

	public override void OnEnemySpawned(Unit unit)
	{
		MissionStatistics.OnEnemySpawned(unit);
	}

	public override void OnEnemyHeadshot(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		MissionStatistics.OnEnemyHeadshot(source, unit, weapon, consumable);
	}

	public override void OnEnemyDismemberment(Character source, Unit unit, Weapon weapon, Consumable consumable, bool isFatal)
	{
		MissionStatistics.OnEnemyDismemberment(source, unit, weapon, consumable, isFatal);
	}

	public override void OnEnemyMutilated(Character source, Unit unit, Weapon weapon, Consumable consumable)
	{
		MissionStatistics.OnEnemyMutilated(source, unit, weapon, consumable);
	}

	public override void OnTaskCompleted(Task task)
	{
		MissionStatistics.OnTaskCompleted(task);
	}

	public override void OnSupplyGathered(GameObject obj)
	{
		MissionStatistics.OnSupplyGathered(obj);
	}

	public override void OnPlayerKill(Player player, Unit unit)
	{
		_lastPlayerMurderer = (unit ? unit.GetType().Name : "(unknown)");
		MissionStatistics.OnPlayerKill(player, unit);
	}

	public void OnBonusCollected(bool isHard)
	{
		NumBonusCollected++;
		MissionStatistics.NumBonusCollected++;
		if (isHard)
		{
			NumHardBonusCollected++;
			MissionStatistics.NumHardBonusCollected++;
		}
	}

	public void OnFootstep()
	{
		DistanceTravelled++;
		MissionStatistics.DistanceTravelled++;
	}

	public void OnMissionStarted(string location)
	{
		MissionStatistics = new MissionStatistics();
		LastPlayedMissionWasCompleted = false;
		LastPlayedMissionWasCompletedWithLowHP = false;
		if (Singleton.PlayerData.HasMission(location))
		{
			Singleton.PlayerData.LastPlayedMissionIndex = Singleton.PlayerData.GetMission(location);
		}
		MissionStartTime = Singleton.PlayerData.PlayingTime;
		if (Singleton.PlayerData.HasMission(location))
		{
			GameAnalytics.LogMissionStarted(Singleton.GameData.Missions[Singleton.PlayerData.GetMission(location)].Type, Singleton.PlayerData.GetMission(location), Singleton.PlayerData.Level, Singleton.PlayerData.GetMissionDifficulty(location));
		}
	}

	public void OnMissionCompleted(string location)
	{
		bool flag = NumMissionsCompleted == 0;
		if (Singleton.PlayerData.HasMission(location))
		{
			int mission = Singleton.PlayerData.GetMission(location);
			flag = flag || Singleton.GameData.Missions[mission].Type > Mission.Type.Story1;
		}
		if (flag)
		{
			Singleton.PlayerData.StorylineProgress = 0;
			Singleton.PlayerData.StorylineTicket++;
		}
		else
		{
			Singleton.PlayerData.StorylineProgress++;
		}
		NumMissionsCompleted++;
		NumMissionsFailedInRow = 0;
		LastPlayedMissionWasCompleted = true;
		LastPlayedMissionWasCompletedWithLowHP = (bool)Scene.Player && Scene.Player.Health / Scene.Player.healthTotal < 0.2f;
		if (flag)
		{
			GameAnalytics.LogNextStoryMission();
		}
		foreach (KeyValuePair<KeyValuePair<string, string>, int> numEnemyKill in MissionStatistics.NumEnemyKills)
		{
			string key = numEnemyKill.Key.Key;
			string value = numEnemyKill.Key.Value;
			if (IsSpecial(key))
			{
				GameAnalytics.LogKillSpecialZombie(key, value, MissionStatistics.IsConsumable(value), numEnemyKill.Value);
			}
			else if (IsUncommon(key))
			{
				GameAnalytics.LogKillUncommonZombie(key, value, MissionStatistics.IsConsumable(value), numEnemyKill.Value);
			}
		}
		foreach (KeyValuePair<KeyValuePair<string, string>, int> numEnemyHeadshot in MissionStatistics.NumEnemyHeadshots)
		{
			string key2 = numEnemyHeadshot.Key.Key;
			string value2 = numEnemyHeadshot.Key.Value;
			GameAnalytics.LogHeadshot(key2, value2, MissionStatistics.IsConsumable(value2), numEnemyHeadshot.Value);
		}
		if (Singleton.PlayerData.HasMission(location))
		{
			GameAnalytics.LogMissionCompleted(Singleton.GameData.Missions[Singleton.PlayerData.GetMission(location)].Type, Singleton.PlayerData.GetMission(location), Singleton.PlayerData.Level, Singleton.PlayerData.GetMissionDifficulty(location));
		}
		Merge(MissionStatistics);
		Flush = true;
	}

	public void OnMissionFailed(string location, GameEvents.MissionFailure failureType, Mission.Type missionType)
	{
		Singleton.PlayerData.IsSpecialHuntPlayed = false;
		if (!Mission.IsSurvival(missionType))
		{
			NumMissionsFailed++;
		}
		NumMissionsFailedInRow++;
		if (Singleton.PlayerData.HasMission(location))
		{
			if (Mission.IsSurvivalGates(Singleton.GameData.Missions[Singleton.PlayerData.GetMission(location)].Type))
			{
				GameAnalytics.LogSurvivalWaveCompleted(Singleton.PlayerData.Level, Singleton.PlayerData.LastMissionStatistics.NumWavesCompleted, Singleton.PlayerData.LastMissionStatistics.SurvivalScoreTotal(), Singleton.PlayerData.LastMissionStatistics.NumToolkitsPurchased);
			}
			GameAnalytics.LogMissionFailed(Singleton.GameData.Missions[Singleton.PlayerData.GetMission(location)].Type, Singleton.PlayerData.GetMission(location), Singleton.PlayerData.Level, failureType, _lastPlayerMurderer, Singleton.PlayerData.GetMissionDifficulty(location));
		}
		Merge(MissionStatistics);
		Flush = true;
	}

	public void OnIAPPurchase(bool isHC, int numberOfUnits, decimal price)
	{
		XmlDictionary<KeyValuePair<int, int>, int> xmlDictionary = isHC ? HCPurchases : SCPurchases;
		KeyValuePair<int, int> key = new KeyValuePair<int, int>(numberOfUnits, (int)(price * 100m));
		if (xmlDictionary.ContainsKey(key))
		{
			xmlDictionary[key] = xmlDictionary[key] + 1;
		}
		else
		{
			xmlDictionary.Add(key, 1);
		}
		Flush = true;
	}

	public void OnItemPurchase(string item)
	{
		CommonShopItem commonShopItemByItem = Singleton.ShopData.GetCommonShopItemByItem(item);
		if (commonShopItemByItem != null && commonShopItemByItem is WeaponShopItem)
		{
			NumWeaponsPurchased++;
		}
		else if (commonShopItemByItem != null && commonShopItemByItem is ConsumableShopItem)
		{
			NumConsumablesPurchased++;
		}
		Flush = true;
	}
}
