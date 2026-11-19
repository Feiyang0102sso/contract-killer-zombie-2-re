using System.Collections;
using UnityEngine;

public class PartLoader : MonoBehaviour
{
	private CountdownTimer timer;

	public GameObject Elevator;

	private GameObject movingCharacter;

	private IEnumerator LoadPart()
	{
		yield return new WaitForSeconds(3f);
		Elevator.transform.parent = null;
		transform.parent = Elevator.transform;
		movingCharacter.transform.parent = Elevator.transform;
		Object.DontDestroyOnLoad(Elevator);
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		yield return Application.LoadLevelAsync("GasStation2");
		Debug.Log("part loaded");
		Object.Destroy(this);
		foreach (Player player in Object.FindObjectsOfType(typeof(Player)) as Player[])
		{
			if (player.gameObject != movingCharacter)
			{
				Object.DestroyImmediate(player.gameObject);
			}
		}
		Elevator.animation["Take 001"].speed = 0.5f;
		Elevator.animation.Play("Take 001");
		Elevator.transform.position = Util.Find("/ElevatorArrivesHere").transform.position;
		Elevator.transform.rotation = Util.Find("/ElevatorArrivesHere").transform.rotation;
		Object.DestroyImmediate(Util.Find("WeightPan"));
		Object.DestroyImmediate(Util.Find("WeightPan"));
		movingCharacter.transform.parent = null;
		movingCharacter.transform.Rotate(0f, 180f, 0f);
		Scene.Camera.LookAt(movingCharacter.transform);
	}

	private void OnTriggerEnter(Collider collider)
	{
		Character component = collider.gameObject.GetComponent<Character>();
		if (!((bool)component))
		{
			return;
		}
		movingCharacter = component.gameObject;
		if (Elevator.animation["Take 001"].time == 0f)
		{
			Elevator.animation["Take 001"].time = Elevator.animation["Take 001"].length;
		}
		Elevator.animation["Take 001"].speed = -0.5f;
		Elevator.animation.Play("Take 001");
		StartCoroutine(LoadPart());
	}
}
