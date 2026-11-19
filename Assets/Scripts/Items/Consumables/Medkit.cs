using Glu.Localization;
using UnityEngine;

public class Medkit : Consumable
{
	private const float healDistance = 10f;

	public ParticleSystem HealEffect;

	public int HealthToHeal = 100;

	public float Duration = 1f;

	public override void Consume(GameObject consumer)
	{
		base.Consume(consumer);
		foreach (Human item in Scene.Nearby<Human>(consumer.transform.position, 10f))
		{
			HumanHealer humanHealer = item.gameObject.AddComponent<HumanHealer>();
			humanHealer.HealthToHeal = HealthToHeal;
			humanHealer.Duration = Duration;
			humanHealer.HealEffect = HealEffect;
		}
		Singleton.UI.Hud.ShowInfo(string.Format(Strings.GetString("Healing {0} hp"), HealthToHeal));
		Singleton.PlayerData.ProgressGoal(Goal.Type.UseHealing, 1);
		GameAnalytics.LogConsumableUsed("Medkit");
		SoundManager.PlayExclusive("medkit_used", false);
	}

	public override bool IsReadyToConsume(GameObject consumer)
	{
		bool flag = false;
		foreach (Human item in Scene.Nearby<Human>(consumer.transform.position, 10f))
		{
			if (item.Health < item.healthTotal * 0.99f)
			{
				flag = true;
				break;
			}
		}
		return base.IsReadyToConsume(consumer) && flag;
	}
}
