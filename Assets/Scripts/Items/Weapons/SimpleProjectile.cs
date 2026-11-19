using UnityEngine;

public class SimpleProjectile : AbstractProjectile
{
	public float Lifetime = 1f;

	protected override void FixedUpdate()
	{
		if (NumCollisions > 0)
		{
			Lifetime -= Time.fixedDeltaTime;
			if (Lifetime <= 0f)
			{
				Active = false;
				Object.Destroy(gameObject);
			}
		}
		base.FixedUpdate();
	}
}
