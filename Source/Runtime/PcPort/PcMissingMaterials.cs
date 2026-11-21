// PC port: hides renderers that have no material at all.
// Some original assets carry a renderer whose only material slot is empty, e.g. the eyes (LeftEye) of the scientist zombie's
// ragdoll Uncommon2_Ragdoll. The 3.5 build did not show them; Unity 6 draws them with the magenta error shader (pink eyes
// on scientist corpses). RagdollManager switches every renderer of a ragdoll back on when it hands one out, so this runs
// right after that (RagdollManager.Instantiate).
using UnityEngine;

public static class PcMissingMaterials
{
	/// <summary>Disables every renderer under root whose material slots are all empty. Logs each one it hides.</summary>
	public static void HideRenderersWithoutMaterial(GameObject root)
	{
		if (root == null)
		{
			return;
		}
		Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer target in renderers)
		{
			if (!target.enabled)
			{
				continue;
			}
			if (HasAnyMaterial(target))
			{
				continue;
			}
			target.enabled = false;
			Debug.Log("[PcMissingMaterials] hid '" + target.name + "' in '" + root.name + "' (no material)");
		}
	}

	/// <summary>True when at least one material slot of the renderer is set.</summary>
	private static bool HasAnyMaterial(Renderer target)
	{
		Material[] materials = target.sharedMaterials;
		foreach (Material material in materials)
		{
			if (material != null)
			{
				return true;
			}
		}
		return false;
	}
}
