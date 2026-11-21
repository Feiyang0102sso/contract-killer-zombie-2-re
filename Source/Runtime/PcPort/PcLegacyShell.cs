// PC port: an object moved out of its parent by PcLegacyActive (characters and effects of a finished mission node, the
// empty crate of a used ammo box). In 3.5 it stayed a child, so destroying that parent (or anything above it, e.g. a
// DestroyObject removing a CreateObject node with what it spawned) also removed the object. This keeps that link.
using UnityEngine;

public class PcLegacyShell : MonoBehaviour
{
	/// <summary>The parent the object was moved out of.</summary>
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
