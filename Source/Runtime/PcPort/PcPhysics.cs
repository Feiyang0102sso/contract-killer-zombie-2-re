// PC 移植：CharacterController 的行为向 Unity 3.5（PhysX 2.8）看齐。
//
// 3.5 的控制器有两个特点，Unity 5 以后都变了：
// 1. 不会把已经重叠的控制器推开（新版默认开着 overlap recovery）；
// 2. 一开始就和静态几何体重叠时，Move 的扫掠在距离 0 处就被挡住，控制器原地不动（新版的扫掠忽略初始重叠，直接穿过去）。
// 教程关的 TargetToAttack（坠毁直升机里的假人，僵尸的攻击目标）就是一开始插在路面下 0.09 m、和直升机网格重叠的。
// 原版里它被卡在原地；新版要么被推飞，要么穿地掉下去，僵尸失去目标，教程卡住。见 tools/README.md 白天修复第 1 节。
using System.Collections.Generic;
using UnityEngine;

public static class PcPhysics
{
	/// <summary>
	/// 每个控制器第一次 Move 时的判定结果（实例 ID → 是否钉在原地）。
	/// 只认“一开始就摆在几何体里”的情况：第一次 Move 时没重叠的控制器，以后永远照常 Move。
	/// 不这样限制的话，玩家在柱子边被挤进去一点就会永远走不动（白天修复第 6 节，player82）。
	/// </summary>
	private static readonly Dictionary<int, bool> _pinned = new Dictionary<int, bool>();

	/// <summary>
	/// 判定“和静态几何体重叠”时，胶囊半径先缩小这么多。
	/// 站在地上的控制器和地面之间只有约 0.01 m 的间隙，留出余量，免得把正常站着的角色误判成卡住；
	/// TargetToAttack 插进路面 0.09 m，仍能判出来。
	/// </summary>
	private const float OverlapTolerance = 0.05f;

	/// <summary>关掉 CharacterController 的 overlap recovery（3.5 没有这个功能）。</summary>
	public static void MatchLegacyController(CharacterController controller)
	{
		if (controller == null)
		{
			return;
		}
		controller.enableOverlapRecovery = false;
	}

	/// <summary>
	/// 代替 controller.Move(motion)：控制器第一次 Move 时就和静态几何体重叠（场景里摆进几何体的假人），
	/// 像 3.5 一样永远原地不动；否则照常 Move。会主动移动的角色不参与（见 CanBePinned）。
	/// owner 是角色根物体，它下面的碰撞体（受击区等）不算。
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
	/// 会主动移动的角色（玩家、Bot / 幸存者、Animal / 乌鸦）不参与“钉住”：全关卡巡检（tour5）里幸存者和乌鸦一开始也和几何体重叠，
	/// 钉住它们会让护送任务走不动、乌鸦飞不起来，而原版这些都是正常的。只剩 HumanInHelicopter 这类不主动移动的假人。
	/// </summary>
	private static bool CanBePinned(Transform owner)
	{
		if (owner.GetComponent<Player>() != null || owner.GetComponent<Bot>() != null || owner.GetComponent<Animal>() != null)
		{
			return false;
		}
		return true;
	}

	/// <summary>控制器的胶囊（按缩放换算到世界空间，半径减去容差）是否和会与它碰撞的静态碰撞体重叠。</summary>
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
			// 自己的碰撞体、其他控制器、带刚体的物体都不算静态几何体
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

	/// <summary>按物理设置的层碰撞矩阵，返回与 layer 会发生碰撞的层的掩码。</summary>
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
