using UnityEngine;

public class ExplosionInfo : DamageInfo
{
	public Consumable consumable;

	public Vector3 epicenter;

	public float radius;

	public ExplosionInfo(float damage_, Type type_, Character source_, Vector3 momentum_, float defragChance_, float mutilationChance_, float stunChance_, Vector3 epicenter_, float radius_, Consumable consumable_)
		: base(damage_, type_, source_, momentum_, defragChance_, mutilationChance_, stunChance_, 100f)
	{
		consumable = consumable_;
		epicenter = epicenter_;
		radius = radius_;
	}
}
