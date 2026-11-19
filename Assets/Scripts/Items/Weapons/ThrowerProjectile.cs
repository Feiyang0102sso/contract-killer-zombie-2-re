using UnityEngine;

public class ThrowerProjectile : ExplosiveProjectile
{
	protected override void Action()
	{
		if ((bool)Smoke)
		{
			Smoke.transform.parent = transform.parent;
			Object.Destroy(Smoke, SmokeLifetime);
		}
		if ((bool)EffectsManager.instance)
		{
			EffectsManager.instance.OnGreenGooHit(transform.position);
		}
		ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
		if ((bool)componentInChildren)
		{
			GetComponentInChildren<ParticleSystem>().enableEmission = false;
		}
		Util.ExplosionDealsDamage(MaxDamage, MinDamage, DamageRandomness, transform.position, ActionRadius, Source, Consumable, MaxImpulse, (Character c) => true);
		Object.Destroy(gameObject.GetComponent<MeshRenderer>());
		Object.Destroy(gameObject.rigidbody);
		Object.Destroy(gameObject, 1.5f);
		if (ActionSound != string.Empty)
		{
			SoundManager.Play(ActionSound, transform.position, 25f, 150f, false);
		}
		if ((bool)rigidbody)
		{
			rigidbody.detectCollisions = false;
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.Sleep();
		}
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = false;
		}
		Active = false;
	}
}
