// PC port: a model moved out of a used ActivateItem by PcLegacyActive.RescueAfterDisappear (e.g. the empty ammo crate).
// In 3.5 the model stayed a child of the item, so destroying the item (or a node above it, e.g. the tutorial's
// DestroyObject removing a CreateObject node with its spawned crate) also removed the model. This keeps that link.
using UnityEngine;

public class PcLegacyShell : MonoBehaviour
{
	/// <summary>The item the model was moved out of.</summary>
	private GameObject _formerParent;

	public void Init(GameObject formerParent)
	{
		_formerParent = formerParent;
	}

	private void Update()
	{
		// Unity's == null is true once the object is destroyed
		if (_formerParent == null)
		{
			Debug.Log("[PcLegacyShell] '" + name + "' destroyed with its former parent");
			Destroy(gameObject);
		}
	}
}
