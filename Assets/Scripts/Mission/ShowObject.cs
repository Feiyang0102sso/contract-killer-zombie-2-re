using UnityEngine;

public class ShowObject : MonoBehaviour
{
	public GameObject what;

	public float liveSpan = -1f;

	private void Awake()
	{
	}

	private void OnActivate()
	{
		what.active = true;
		if (liveSpan >= 0f)
		{
			Object.Destroy(what, liveSpan);
		}
	}
}
