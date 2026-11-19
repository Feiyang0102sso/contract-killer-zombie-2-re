using UnityEngine;

public class ArbalestProjectile : GuidedProjectile
{
	public GameObject ArbalestProjectileDummy;

	public GameObject Trail;

	public float TrailLifetime;

	public float MinDamage;

	public float MaxDamage = 100f;

	protected override void Awake()
	{
		if ((bool)ArbalestProjectileDummy && (bool)RagdollManager.instance)
		{
			RagdollManager.instance.Preinstance(ArbalestProjectileDummy);
		}
		base.Awake();
	}

	protected override void OnCollision(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, CollisionMode collisionMode)
	{
		if (!Active)
		{
			return;
		}
		GameObject gameObject = ArbalestProjectileDummy ? RagdollManager.instance.Instantiate(ArbalestProjectileDummy) : null;
		Character character = hitObject ? hitObject.FindComponentUpwards<Character>() : null;
		if ((bool)character)
		{
			bool pierced = gameObject ? character.Pierce(gameObject, hitPoint) : false;
			if (pierced)
			{
				gameObject = null;
			}
			character.OnDamage(new HitInfo(Random.Range(MinDamage, MaxDamage), DamageInfo.Type.Bullet, Source, rigidbody.velocity * rigidbody.mass, 0f, 0f, 100f, 1f, hitPoint, hitObject, 50f));
			if ((bool)Trail)
			{
				Trail.transform.parent = null;
				Object.Destroy(Trail, TrailLifetime);
				Trail = null;
			}
			Object.Destroy(base.gameObject);
		}
		else if ((bool)hitObject)
		{
			Collider collider = hitObject.GetComponent<Collider>();
			if (!collider)
			{
				collider = hitObject.GetComponentInChildren<Collider>();
			}
			if ((bool)collider)
			{
				Scene.EffectsManager.OnHit(collider, hitPoint, hitNormal, rigidbody.velocity, false, false, new HitInfo(Random.Range(MinDamage, MaxDamage), DamageInfo.Type.Bullet, Source, rigidbody.velocity * rigidbody.mass, 0f, 0f, 100f, 1f, hitPoint, hitObject, 50f));
			}
			Object.Destroy(base.gameObject);
			gameObject.transform.position = hitPoint;
			gameObject.transform.parent = hitObject.transform;
			gameObject = null;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		if ((bool)gameObject)
		{
			Object.Destroy(gameObject);
		}
		Active = false;
	}

	public override void InitiateGuidance(GameObject shootingToObject, Vector3 shootingTo)
	{
		Unit unit = shootingToObject.FindComponentUpwards<Unit>();
		Vector3 vector = (unit ? GetTargetPosition(unit) : shootingTo);
		transform.forward = (vector - transform.position).normalized;
		rigidbody.velocity = transform.forward * rigidbody.velocity.magnitude;
	}
}
