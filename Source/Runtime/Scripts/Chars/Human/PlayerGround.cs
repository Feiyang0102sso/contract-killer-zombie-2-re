using UnityEngine;

public class PlayerGround : MonoBehaviour
{
	public Collider LeftFoot;

	public Collider RightFoot;

	private Player _owner;

	private void Start()
	{
		_owner = gameObject.FindComponentInParents<Player>();
	}

	private void OnTriggerEnter(Collider collider)
	{
		if ((bool)_owner && (collider == LeftFoot || collider == RightFoot))
		{
			_owner.OnFootstep(collider);
		}
	}
}
