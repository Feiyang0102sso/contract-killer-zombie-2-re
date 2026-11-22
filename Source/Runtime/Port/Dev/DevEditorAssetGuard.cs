// PC port (editor only): undoes in-memory edits that game code makes to asset materials during Play.
// TexturesReplacer swaps the main texture of shared materials (GUI atlases, skies) for the high-res CopiedTextures.
// In a player that only lives in memory, but in the editor those materials are the project assets: after Play they
// stay modified (dirty), and the next asset save writes them to disk (wrong texture in the .mat, and EZGUI then
// doubles the sprites' pixelsPerUV in every scene opened afterwards).
// Track() records such a material; when the editor is back in edit mode the material is unloaded, so the next use
// reloads the unmodified asset from disk. Does nothing in a build.
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DevEditorAssetGuard
{
#if UNITY_EDITOR
	/// <summary>Asset materials changed during the current Play session.</summary>
	private static readonly HashSet<Material> TrackedMaterials = new HashSet<Material>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Install()
	{
		TrackedMaterials.Clear();
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange change)
	{
		if (change != PlayModeStateChange.EnteredEditMode)
		{
			return;
		}
		int unloaded = 0;
		foreach (Material material in TrackedMaterials)
		{
			if (material == null)
			{
				continue;
			}
			Resources.UnloadAsset(material);
			unloaded++;
		}
		TrackedMaterials.Clear();
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		if (unloaded > 0)
		{
			Debug.Log("[DevEditorAssetGuard] reverted " + unloaded + " asset materials changed during Play");
		}
	}
#endif

	/// <summary>Call after changing a material that may be a project asset (no effect in a build or for runtime-created materials).</summary>
	public static void Track(Material material)
	{
#if UNITY_EDITOR
		if (EditorUtility.IsPersistent(material))
		{
			TrackedMaterials.Add(material);
		}
#endif
	}
}
