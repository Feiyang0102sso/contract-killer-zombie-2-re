// PC 移植：补上 Unity 3.5 的“非层级 active”语义在任务系统里的效果。
//
// 3.5 的 GameObject.active 只管物体自己：父物体失活，子物体只要自己的 active 是 true，照样运行、渲染、收消息。
// Unity 4 起改成层级失活：父物体一失活，整棵子树都停。
// Mission.DeactivateNode 是照 3.5 语义写的：
//   - [Action] 节点执行完只把节点本身 active = false，子物体（Effect、Fade、刷出来的僵尸等）在 3.5 里继续运行，
//     例如教程关 “3.1) [Action] remove copter & fade-in” 的 Effect 要延时 Invoke 后广播 OnEffect，让白屏淡出；
//   - [Task] 节点结束时逐个关掉子物体，但特意跳过 Character 和 ActivateItem（以及不随使用消失的 Item_*），它们在 3.5 里留在场上。
// 在 Unity 6 里这些子物体都随父节点一起失活：教程爆炸后一直白屏、任务下面的幸存者和僵尸凭空消失。
// 做法：DeactivateNode 结束后，把节点子树里“自己 active 为 true、但祖先被关掉”的物体移到节点的父物体下（保持世界位置），
// 让它们像 3.5 那样继续运行。见 tools/README.md 白天修复第 2 节。
using System.Collections.Generic;
using UnityEngine;

public static class PcLegacyActive
{
	/// <summary>
	/// 在 Mission.DeactivateNode 末尾调用：节点已失活时，把子树里按 3.5 语义仍应运行的物体移出来。
	/// 只移最上层的那个（它的子物体跟着走）；不深入自己 active 为 true 的物体内部。
	/// </summary>
	public static void RescueLegacyActiveChildren(GameObject node)
	{
		if (node.activeSelf)
		{
			return;
		}
		List<Transform> rescued = new List<Transform>();
		CollectActiveUnderInactive(node.transform, rescued);
		Transform newParent = node.transform.parent;
		foreach (Transform child in rescued)
		{
			child.SetParent(newParent, true);
			Debug.Log("[PcLegacyActive] keep '" + child.name + "' running after '" + node.name + "' was deactivated");
		}
	}

	/// <summary>parent 已失活：它的子物体里自己 active 的收进 rescued，失活的继续往下找。</summary>
	private static void CollectActiveUnderInactive(Transform parent, List<Transform> rescued)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				rescued.Add(child);
			}
			else
			{
				CollectActiveUnderInactive(child, rescued);
			}
		}
	}
}
