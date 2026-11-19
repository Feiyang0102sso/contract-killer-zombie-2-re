using UnityEngine;

public class Special : Unit
{
	protected override void OnDying(DamageInfo info)
	{
		if (info != null && info.source is Human)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.KillSpecialInfected);
			if (this is PiperMinion)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.KillPiperMinion);
			}
			else if (this is Piper)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.KillPiper);
			}
			else if (this is Thrower)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.KillThrower);
			}
			else if (this is Bomber)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.KillBomber);
			}
			else if (this is Charger)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.KillCharger);
			}
			else if (this is Spitter)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.KillSpitter);
			}
			if (info.type == DamageInfo.Type.Melee || info.type == DamageInfo.Type.Laceration)
			{
				PlayerData.Instance.ProgressGoal(Goal.Type.MeleeKillSpecial);
			}
		}
		base.OnDying(info);
	}

	public virtual void UpdateTarget(GameObject newTarget)
	{
	}
}
