using UnityEngine;

public class RigidbodyFixer : MonoBehaviour
{
	private Rigidbody _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

	private void OnCollisionEnter(Collision collisionInfo)
	{
		_rigidbody.angularDrag *= 2f;
		_rigidbody.drag *= 2f;
	}
}
