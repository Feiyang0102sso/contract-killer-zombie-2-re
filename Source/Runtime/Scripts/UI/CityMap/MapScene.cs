using UnityEngine;

public class MapScene : MonoBehaviour
{
	public GameObject MapWindowPrefab;

	private void Start()
	{
		Object.Instantiate(MapWindowPrefab);
	}
}
