using UnityEngine;

public class CreateObject : MonoBehaviour
{
	public GameObject whatPrefab;

	public bool rotateRandomly = true;

	public float liveSpan = -1f;

	private void Awake()
	{
	}

	private void OnActivate()
	{
		GameObject gameObject = Object.Instantiate(whatPrefab, transform.position, Quaternion.identity) as GameObject;
		gameObject.transform.parent = transform;
		if (rotateRandomly)
		{
			gameObject.transform.Rotate(Vector3.up, Random.Range(0, 360));
		}
		else
		{
			gameObject.transform.forward = transform.forward;
		}
		Character component = gameObject.GetComponent<Character>();
		if (component != null && component is Unit && Scene.inst != null)
		{
			Scene.Spawner.PostSpawn(component);
		}
		gameObject.SendMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
		if (liveSpan >= 0f)
		{
			Object.Destroy(gameObject, liveSpan);
		}
	}
}
