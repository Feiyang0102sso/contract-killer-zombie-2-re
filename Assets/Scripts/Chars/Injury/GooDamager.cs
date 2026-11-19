using System;
using UnityEngine;

public class GooDamager : MonoBehaviour
{
	private const float radius = 2f;

	[NonSerialized]
	public Character source;

	private void Awake()
	{
		SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
		sphereCollider.radius = 2f;
		sphereCollider.gameObject.layer = Const.LayerTransparentFX;
		sphereCollider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider collider)
	{
		Hit(collider);
	}

	private void OnTriggerStay(Collider collider)
	{
		Hit(collider);
	}

	public void Hit(Collider collider)
	{
		GameObject gameObject = collider.gameObject;
		if (gameObject.layer != Const.LayerHitable)
		{
			return;
		}
		Character component = gameObject.GetComponent<Character>();
		if (component == null || component == source)
		{
			return;
		}
		if (!Util.DoRarely(component, "goo_damage", 0.25f))
		{
			return;
		}
		SoundManager.PlayExclusive("Zombie/Spitter/spitter_acid_1", transform.position, 5f, 25f, false);
		float unusedDamage;
		float unusedRate;
		component.OnDamage(new ExplosionInfo((source is Spitter ? (source as Spitter).gooDamage : 50f) / 4f,
			DamageInfo.Type.Explosion, source, (component.transform.position - transform.position).normalized * 0.1f,
			0f, 0f, 0f, transform.position, 2f, null));
	}
}
