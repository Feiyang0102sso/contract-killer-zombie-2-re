using UnityEngine;

public class Grenade : Consumable
{
	public GameObject GrenadePrefab;

	public override bool IsReadyToConsume(GameObject consumer)
	{
		Player component = consumer.GetComponent<Player>();
		return base.IsReadyToConsume(consumer) && (component ? component.IsReadyToThrow() : true);
	}

	public override void Consume(GameObject consumer)
	{
		base.Consume(consumer);
		Player component = consumer.GetComponent<Player>();
		if ((bool)component)
		{
			component.Throw(GrenadePrefab, this);
		}
		bool isBeacon = GrenadePrefab.GetComponent<GrenadeBeacon>() != null;
		GameAnalytics.LogConsumableUsed(isBeacon ? "Beacon" : "Grenade");
	}
}
