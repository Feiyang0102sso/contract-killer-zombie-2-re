using System;
using UnityEngine;

public class KillAmount : MonoBehaviourAndLog
{
	public int amount = 20;

	public float recharge = 10f;

	private int amountLeft;

	private CountdownTimer rechargeTimer = new CountdownTimer();

	private void Awake()
	{
		LogTo("Mission.KillAmount");
		amountLeft = amount;
	}

	public string SubText()
	{
		return string.Format("{0}/{1}", amountLeft, amount);
	}

	private void OnActivate()
	{
		Singleton.GameEvents.OnEnemyKill += new Action<Character, Unit, Weapon, Consumable>(OnKill);
		if (recharge != 0f)
		{
			rechargeTimer.Run(recharge);
		}
	}

	private void OnKill(Character s, Unit u, Weapon w, Consumable c)
	{
		if (!this)
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnKill);
			return;
		}
		amountLeft--;
		if (amountLeft == 0)
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnKill);
			SendMessageUpwards("OnTaskCompleted");
		}
	}

	private void Update()
	{
		if (recharge != 0f && !rechargeTimer.IsRunning())
		{
			rechargeTimer.Run(recharge);
			Util.DoForHierarchy(gameObject, false, (GameObject n) =>
			{
				n.SendMessage("OnActivate");
				return true;
			});
		}
	}
}
