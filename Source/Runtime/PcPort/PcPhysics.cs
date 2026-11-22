// PC port: makes CharacterController behave like Unity 3.5 (PhysX 2.8).
//
// The 3.5 controller differs from Unity 5+ in two ways:
// 1. It does not push an already overlapping controller out (newer versions have overlap recovery on by default);
// 2. When it starts inside static geometry, the Move sweep is blocked at distance 0 and the controller stays put
//    (the newer sweep ignores initial overlaps and passes straight through). Point 2 is inferred: the original data
//    starts overlapped and the tutorial works on iOS; no PhysX 2.8 documentation was found.
// The tutorial's TargetToAttack (the dummy in the crashed helicopter, the zombies' attack target) starts 0.09 m below
// the road and overlapping the helicopter mesh.
// In the original it stays stuck in place; in the new version it is either flung away or falls through the ground, the
// zombies lose their target and the tutorial stalls. See pc_pilot_tools/README.md, day fixes section 1.
using System.Collections.Generic;
using UnityEngine;

public static class PcPhysics
{
	/// <summary>
	/// Result decided at each controller's first Move (instance ID -> pinned in place or not).
	/// Only controllers placed inside geometry from the start count: one not overlapping at its first Move always moves normally.
	/// Without this limit, a player squeezed slightly into a pillar would never move again (day fixes section 6, player82).
	/// </summary>
	private static readonly Dictionary<int, bool> _pinned = new Dictionary<int, bool>();

	/// <summary>
	/// The capsule radius is shrunk by this much when testing for overlap with static geometry.
	/// A controller standing on the ground has only about 0.01 m of gap to it; the margin keeps normally standing
	/// characters from being judged stuck. TargetToAttack is 0.09 m into the road, so it is still caught.
	/// </summary>
	private const float OverlapTolerance = 0.05f;

	/// <summary>Turns off the CharacterController's overlap recovery (3.5 has no such feature).</summary>
	public static void MatchLegacyController(CharacterController controller)
	{
		if (controller == null)
		{
			return;
		}
		controller.enableOverlapRecovery = false;
	}

	/// <summary>
	/// Replaces controller.Move(motion): a controller that overlaps static geometry at its first Move (a dummy placed
	/// inside geometry in the scene) stays in place forever, as in 3.5; otherwise it moves normally. Characters that move
	/// by themselves are excluded (see CanBePinned).
	/// owner is the character root; colliders under it (hit zones etc.) do not count.
	/// </summary>
	public static CollisionFlags LegacyMove(CharacterController controller, Vector3 motion, Transform owner)
	{
		int id = controller.GetInstanceID();
		bool pinned;
		if (!_pinned.TryGetValue(id, out pinned))
		{
			pinned = CanBePinned(owner) && IsStuckInStaticGeometry(controller, owner);
			_pinned[id] = pinned;
			if (pinned)
			{
				Debug.Log("[PcPhysics] '" + owner.name + "' starts inside static geometry, pinned in place like Unity 3.5");
			}
		}
		if (pinned)
		{
			return CollisionFlags.None;
		}
		return controller.Move(motion);
	}

	/// <summary>
	/// Characters that move by themselves (Player, Bot / survivors, Animal / crows) are never pinned: in the all-level tour
	/// (tour5) survivors and crows also start overlapping geometry, and pinning them stalls escort tasks and grounds the
	/// crows, while both work in the original. That leaves dummies that never move, such as HumanInHelicopter.
	/// </summary>
	private static bool CanBePinned(Transform owner)
	{
		if (owner.GetComponent<Player>() != null || owner.GetComponent<Bot>() != null || owner.GetComponent<Animal>() != null)
		{
			return false;
		}
		return true;
	}

	/// <summary>Whether the controller's capsule (scaled to world space, radius minus the tolerance) overlaps a static collider it collides with.</summary>
	private static bool IsStuckInStaticGeometry(CharacterController controller, Transform owner)
	{
		Transform transform = controller.transform;
		Vector3 scale = transform.lossyScale;
		float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
		float height = Mathf.Max(controller.height * Mathf.Abs(scale.y), radius * 2f);
		Vector3 center = transform.TransformPoint(controller.center);
		float halfSegment = height * 0.5f - radius;
		Vector3 top = center + Vector3.up * halfSegment;
		Vector3 bottom = center - Vector3.up * halfSegment;
		float checkRadius = radius - OverlapTolerance;
		if (checkRadius <= 0f)
		{
			return false;
		}
		Collider[] overlaps = Physics.OverlapCapsule(top, bottom, checkRadius, CollisionMask(controller.gameObject.layer), QueryTriggerInteraction.Ignore);
		foreach (Collider overlap in overlaps)
		{
			// Own colliders, other controllers and objects with a rigidbody are not static geometry
			if (overlap == controller || overlap.transform.IsChildOf(owner))
			{
				continue;
			}
			if (overlap is CharacterController || overlap.attachedRigidbody != null)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	/// <summary>Mask of the layers that collide with layer, from the physics layer collision matrix.</summary>
	private static int CollisionMask(int layer)
	{
		int mask = 0;
		for (int other = 0; other < 32; other++)
		{
			if (!Physics.GetIgnoreLayerCollision(layer, other))
			{
				mask |= 1 << other;
			}
		}
		return mask;
	}
}
