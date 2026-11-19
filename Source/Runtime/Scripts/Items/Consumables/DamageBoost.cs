using UnityEngine;

public class DamageBoost : Consumable
{
	public float duration = 15f;

	public float damageBoost = 3f;

	public float defenseBoost = 0.2f;

	public override void Consume(GameObject consumer)
	{
		float unusedLocal1;
		foreach (Human item in Scene.Nearby<Human>(consumer.transform.position, 10f))
		{
			DamageBooster damageBooster = item.gameObject.AddComponent<DamageBooster>();
			damageBooster.Setup(this);
		}
		GameAnalytics.LogConsumableUsed("Boost");
		base.Consume(consumer);
	}
}
