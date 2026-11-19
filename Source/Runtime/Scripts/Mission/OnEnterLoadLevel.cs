using UnityEngine;

public class OnEnterLoadLevel : MonoBehaviour
{
	public string levelToLoad = string.Empty;

	public bool WillLoad
	{
		get
		{
			return enabled && levelToLoad != string.Empty;
		}
	}

	private void Start()
	{
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!WillLoad)
		{
			return;
		}
		if ((bool)collider.gameObject.GetComponent<Player>())
		{
			Singleton.Loader.LoadLevel(levelToLoad);
		}
	}
}
