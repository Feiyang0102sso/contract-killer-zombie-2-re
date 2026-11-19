using UnityEngine;

public class GrenadeBeacon : MonoBehaviour
{
	public float attractionDistance = 12f;

	public GameObject beaconEffect;

	private GameObject effect;

	private void Start()
	{
		effect = Object.Instantiate(beaconEffect) as GameObject;
		effect.transform.parent = transform;
		effect.transform.localPosition = Vector3.zero;
	}

	private void Update()
	{
		if (Util.DoRarely(this, "attract_zombies", 0.3f))
		{
			foreach (Zombie item in Scene.Nearby<Zombie>(transform.position, attractionDistance))
			{
				if (Util.DoRarely(item, "change_target", 2f) && !item.IsAttacking(gameObject))
				{
					item.Run(gameObject);
				}
			}
		}
		if (!(!Util.DoRarely(this, "attract_specials", 1f)))
		{
			foreach (Special item2 in Scene.Nearby<Special>(transform.position, attractionDistance))
			{
				item2.UpdateTarget(gameObject);
			}
		}
	}
}
