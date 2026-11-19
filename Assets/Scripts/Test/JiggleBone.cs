using UnityEngine;

public class JiggleBone : MonoBehaviour
{
	public Transform syncWith;

	private Vector3 parentPosLastFrame = Vector3.zero;

	private void LateUpdate()
	{
		transform.rotation = syncWith.rotation;
		syncWith.rigidbody.AddForce((parentPosLastFrame - syncWith.transform.parent.position) * 30f);
		parentPosLastFrame = syncWith.transform.parent.position;
	}
}
