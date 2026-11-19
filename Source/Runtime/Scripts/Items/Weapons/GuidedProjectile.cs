using UnityEngine;

public class GuidedProjectile : AbstractProjectile
{
	public bool GuidanceEnabled = true;

	public float GuidanceTimeout;

	public float GuidanceBearing = 0.707f;

	public float GuidanceAngularVel = 90f;

	public float GuidanceToHeadChance = 50f;

	public float GuidancePrecisionOverRealism;

	public bool GuidanceUseDynamicAngularVel;

	protected bool _guidanceToHead;

	protected float _guidanceToPartialHeight = 0.5f;

	protected override void Awake()
	{
		_guidanceToHead = Util.Chance(GuidanceToHeadChance);
		_guidanceToPartialHeight = Util.Range(0.5f, 0.75f);
		base.Awake();
	}

	protected Vector3 GetTargetPosition(Unit enemy)
	{
		return !(!enemy.HeadHitzone || !_guidanceToHead) ? enemy.HeadHitzone.transform.position : enemy.PartialHeight(_guidanceToPartialHeight);
	}

	protected override void FixedUpdate()
	{
		if (GuidanceEnabled && Active && LifeTime > GuidanceTimeout)
		{
			Unit unit = null;
			float num = GuidanceBearing;
			Vector3 vector = transform.forward;
			foreach (Unit enemy in Scene.Enemies)
			{
				if (!((bool)enemy))
				{
					continue;
				}
				Vector3 targetPosition = GetTargetPosition(enemy);
				Vector3 normalized = (targetPosition - transform.position).normalized;
				float num2 = Vector3.Dot(normalized, transform.forward);
				if (num2 > num)
				{
					unit = enemy;
					num = num2;
					vector = normalized;
				}
			}
			if ((bool)unit)
			{
				float num3 = GuidanceAngularVel;
				if (GuidanceUseDynamicAngularVel)
				{
					float num4 = 0f;
					float num5 = 1440f;
					float magnitude = GetComponent<Rigidbody>().linearVelocity.magnitude;
					float guidanceAngularVel = GuidanceAngularVel;
					float magnitude2 = (GetTargetPosition(unit) - transform.position).magnitude;
					magnitude2 = Mathf.Clamp01((magnitude2 - num4) / (magnitude - num4));
					num3 = num5 * (1f - magnitude2) + guidanceAngularVel * magnitude2;
				}
				Vector3 axis = Vector3.Cross(transform.forward, vector);
				float angle = Firearm.GetAngle(transform.forward, vector, axis);
				float num6 = Mathf.Sign(angle) * num3 * Time.fixedDeltaTime;
				if (Mathf.Abs(num6) > Mathf.Abs(angle))
				{
					num6 = angle;
				}
				transform.Rotate(axis, num6, Space.World);
				if (GuidancePrecisionOverRealism > 0f)
				{
					Vector3 normalized2 = (GetComponent<Rigidbody>().linearVelocity.normalized * (1f - GuidancePrecisionOverRealism) + transform.forward * GuidancePrecisionOverRealism).normalized;
					GetComponent<Rigidbody>().linearVelocity = normalized2 * GetComponent<Rigidbody>().linearVelocity.magnitude;
				}
			}
		}
		base.FixedUpdate();
	}

	public virtual void InitiateGuidance(GameObject shootingToObject, Vector3 shootingTo)
	{
	}
}
