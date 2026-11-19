using UnityEngine;

public class AddChildPrefab : MonoBehaviour
{
	public GameObject Prefab;

	private void Awake()
	{
		GameObject gameObject = Object.Instantiate(Prefab) as GameObject;
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = Vector3.zero;
		enabled = false;
	}
}
