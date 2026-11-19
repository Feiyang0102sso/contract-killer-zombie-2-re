using UnityEngine;

public abstract class DamageInfo
{
	public enum Type
	{
		Melee,
		Laceration,
		Bullet,
		Explosion,
		Count
	}

	public enum HitZone
	{
		Default,
		Head
	}

	public float damage;

	public float actualDamage;

	public Type type;

	public Character source;

	public Vector3 momentum;

	public float defragChance;

	private float _mutilationChance;

	public bool mutilated;

	public float ragdollChance;

	private float _stunChance;

	public bool stunned;

	public HitZone hitZone;

	public float mutilationChance
	{
		get
		{
			return _mutilationChance;
		}
		set
		{
			_mutilationChance = value;
			mutilated = Util.Chance(mutilationChance);
		}
	}

	public float stunChance
	{
		get
		{
			return _stunChance;
		}
		set
		{
			_stunChance = value;
			stunned = Util.Chance(stunChance);
		}
	}

	public DamageInfo(float damage_, Type type_, Character source_, Vector3 momentum_, float defragChance_, float mutilationChance_, float stunChance_, float ragdollChance_)
	{
		damage = damage_;
		actualDamage = damage;
		type = type_;
		source = source_;
		momentum = momentum_;
		defragChance = defragChance_;
		mutilationChance = mutilationChance_;
		stunChance = stunChance_;
		hitZone = HitZone.Default;
		ragdollChance = ragdollChance_;
	}
}

public class HitInfo : DamageInfo
{
	public float criticalMod;

	public Vector3 hitPoint;

	public GameObject hitObject;

	public HitInfo(float damage_, Type type_, Character source_, Vector3 momentum_, float defragChance_, float mutilationChance_, float stunChance_, float criticalMod_, Vector3 hitPoint_, GameObject hitObject_, float ragdollChance_)
		: base(damage_, type_, source_, momentum_, defragChance_, mutilationChance_, stunChance_, ragdollChance_)
	{
		criticalMod = criticalMod_;
		hitPoint = hitPoint_;
		hitObject = hitObject_;
	}
}
