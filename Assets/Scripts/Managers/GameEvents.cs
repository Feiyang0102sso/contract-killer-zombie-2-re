using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
	public enum MissionFailure
	{
		PlayerKilled,
		SurvivorKilled
	}

	public Action<Character, Weapon, int> OnWeaponShot;

	public Action<Character, Consumable> OnConsumableUsed;

	public Action<Character, Unit, Weapon, Consumable, float> OnEnemyHit;

	public Action<Character, Unit, Weapon, Consumable> OnEnemyKill;

	public Action<Unit> OnEnemySpawned;

	public Action<Character, Unit, Weapon, Consumable> OnEnemyHeadshot;

	public Action<Character, Unit, Weapon, Consumable, bool> OnEnemyDismemberment;

	public Action<Character, Unit, Weapon, Consumable> OnEnemyMutilated;

	public Action<Task> OnTaskCompleted;

	public Action<GameObject> OnSupplyGathered;

	public Action<GameObject> OnAmmoResupplied;

	public Action<Player, Unit> OnPlayerKill;

	public Action<bool> OnBonusCollected;

	public Action<string> OnMissionStarted;

	public Action<string> OnMissionCompleted;

	public Action<string, MissionFailure, Mission.Type> OnMissionFailed;

	public Action<bool, int, decimal> OnIAPPurchase;

	public Action<string> OnItemPurchase;

	public Action<int> OnFPSDecreased;

	public Action<int> OnFPSIncreased;
}
