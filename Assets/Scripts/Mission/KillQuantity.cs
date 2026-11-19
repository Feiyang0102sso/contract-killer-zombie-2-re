using System;

public class KillQuantity : MonoBehaviourAndLog
{
	[NonSerialized]
	public int quantity = -1;

	private int quantityLeft;

	private void Awake()
	{
		LogTo("Mission.KillQuantity");
	}

	public int PreCalcQuantity()
	{
		quantity = 0;
		foreach (ScenarioWave scenarioWave in GetComponentsInChildren<ScenarioWave>(true))
		{
			quantity += scenarioWave.GetQuantity();
		}
		quantityLeft = quantity;
		return quantity;
	}

	public string SubText()
	{
		return string.Format("{0}", Scene.Mission.killQuantityLeft);
	}

	private void OnActivate()
	{
		Singleton.GameEvents.OnEnemyKill += new Action<Character, Unit, Weapon, Consumable>(OnKill);
	}

	private void OnKill(Character s, Unit u, Weapon w, Consumable c)
	{
		if (!this)
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnKill);
			return;
		}
		RemoveSingleEnemy();
	}

	private void OnCanceledSpawn(int quantity)
	{
		for (int i = 0; i < quantity; i++)
		{
			RemoveSingleEnemy();
		}
	}

	private void RemoveSingleEnemy()
	{
		Scene.Mission.killQuantityLeft--;
		quantityLeft--;
		if (quantityLeft == 0)
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnKill);
			SendMessageUpwards("OnTaskCompleted");
		}
	}
}
