using UnityEngine;

public class OpenDoor : MonoBehaviour
{
	public ManualDoor door;

	private void OnActivate()
	{
		door.Open();
	}
}
