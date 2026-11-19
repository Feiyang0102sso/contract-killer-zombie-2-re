using UnityEngine;

public class DestroyObject : MonoBehaviour
{
	public GameObject what;

	private void Awake()
	{
	}

	private void OnActivate()
	{
		Object.Destroy(what);
	}
}
