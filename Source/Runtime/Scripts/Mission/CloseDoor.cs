using UnityEngine;

public class CloseDoor : MonoBehaviour
{
	public ManualDoor door;

	private void OnActivate()
	{
		door.Close();
	}
}
