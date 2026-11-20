// PC port: restores the effect of Unity 3.5's non-hierarchical "active" where the game relies on it.
//
// In 3.5, GameObject.active only affects the object itself: children whose own flag is true keep running,
// rendering and receiving messages under an inactive parent. From Unity 4 on, deactivating a parent stops the whole subtree.
// Code written against the 3.5 semantics:
//   - Mission.DeactivateNode: an [Action] node only sets itself inactive when done; its children (Effect, Fade,
//     spawned zombies, ...) keep running in 3.5. E.g. the Effect of the tutorial's "3.1) [Action] remove copter & fade-in"
//     must Invoke OnEffect later to fade the white screen out.
//     A [Task] node switches its children off one by one but skips Character and ActivateItem (and Item_* that stay after use).
//   - ActivateItem.Disappear: switches the item off but keeps its Item_* model (when !disappearsAfterUse),
//     so a used one-shot ammo crate leaves its empty crate in place in 3.5.
// In Unity 6 those children go inactive with their parent: white screen after the tutorial explosion, survivors and
// zombies under a task vanish, used ammo crates vanish.
// Fix: after the original code runs, move objects that are active themselves but have a deactivated ancestor
// out of the deactivated subtree (keeping world position), so they keep running as in 3.5.
// See tools/README.md, daytime fixes section 2.
using System.Collections.Generic;
using UnityEngine;

public static class PcLegacyActive
{
	/// <summary>
	/// Called at the end of Mission.DeactivateNode: if the node is inactive, moves out the objects that should keep
	/// running under 3.5 semantics. Only the topmost one is moved (its children go with it).
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

	/// <summary>
	/// Called at the end of ActivateItem.Disappear: keeps the Item_* model of a used item visible (e.g. the empty ammo crate).
	/// Moved to the scene root, not the item's parent: in 3.5 nothing switches it off later (DeactivateNode stops at the
	/// ActivateItem), but under the parent a later DeactivateNode of an enclosing [Task] would reach it. The root also keeps
	/// it out of the child list DeactivateNode may be iterating when it calls Disappear.
	/// </summary>
	public static void RescueAfterDisappear(GameObject item)
	{
		if (item.activeSelf)
		{
			return;
		}
		List<Transform> rescued = new List<Transform>();
		CollectActiveUnderInactive(item.transform, rescued);
		foreach (Transform child in rescued)
		{
			child.SetParent(null, true);
			// still goes away when the item is destroyed, as it did as a child in 3.5
			child.gameObject.AddComponent<PcLegacyShell>().Init(item);
			Debug.Log("[PcLegacyActive] keep '" + child.name + "' after '" + item.name + "' disappeared");
		}
	}

	/// <summary>parent is inactive: collects its children that are active themselves, searches deeper under inactive ones.</summary>
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
