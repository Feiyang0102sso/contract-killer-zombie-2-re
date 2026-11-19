using UnityEngine;

public static class Cheat
{
	public static void InvulnerablePlayer()
	{
		Util.Invert(ref Singleton.Settings.InvulnerablePlayer);
	}

	public static void InvulnerableSurvivor()
	{
		Util.Invert(ref Singleton.Settings.InvulnerableSurvivor);
	}

	public static void KillEnemies()
	{
		foreach (Unit unit in Util.FindObjectsOfType<Unit>())
		{
			unit.OnDamage(new HitInfo(1000000f, (DamageInfo.Type)Random.Range(0, 4), Scene.Player, Util.Range(20f, 800f) * Util.RandomInCircle(), 80f, 20f, 100f, 1f, unit.Center(), null, 80f));
		}
	}

	public static void Resupply()
	{
		ResupplyAmmo.ResupplyAll(Scene.Player.Inventory, 1f);
	}

	public static void CompleteMission()
	{
		Scene.Mission.SendMessage("MissionCompleted");
	}

	public static void CompleteTask()
	{
		Scene.Mission.BroadcastMessage("OnTaskCompleted");
	}

	public static void LaunchWave()
	{
		if (!Scene.inst.gameObject.GetComponent<ScenarioWave>())
		{
			Scene.inst.gameObject.AddComponent(typeof(ScenarioWave));
		}
		Scene.inst.gameObject.SendMessage("OnActivate");
	}

	public static void ShowLog()
	{
	}

	public static void CompleteGoal()
	{
		Goal.Type[] goals = Singleton.PlayerData.GetGoals();
		if (goals.Length > 0)
		{
			Goal goal = Singleton.PlayerData.GetGoal(goals[Random.Range(0, goals.Length)]);
			Singleton.PlayerData.ProgressGoal(goal.GoalType, goal.Target - goal.Progress);
			Singleton.PlayerData.SendMessage("ApplyLocalProgress");
			GameObject gameObject = GameObject.Find("CommonGUI(Clone)");
			if ((bool)gameObject)
			{
				Singleton.UI.PopupManager.ShowGoalAnimationPopup(gameObject.GetComponent<CommonGUI>());
			}
		}
	}
}
