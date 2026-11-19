public class Explode : UniversalAction
{
	public float damage = 500f;

	public float radius = 5f;

	public float momentum = 100f;

	protected override void Play()
	{
		Util.ExplosionDealsDamage(damage, 0f, 0f, transform.position, radius, null, null, momentum, null);
	}
}
