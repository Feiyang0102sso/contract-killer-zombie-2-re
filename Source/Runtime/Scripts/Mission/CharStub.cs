using UnityEngine;

public class CharStub : MonoBehaviour
{
	public string character = string.Empty;

	private bool activated;

	private void Awake()
	{
	}

	private void Start()
	{
		OnActivate();
	}

	private void OnActivate()
	{
		if (activated)
		{
			return;
		}
		activated = true;
		string path = Util.RandomFrom(character.Split("|"));
		GameObject original = Resources.Load(path) as GameObject;
		GameObject gameObject = Object.Instantiate(original, transform.position, Quaternion.identity) as GameObject;
		gameObject.RemoveClonePostfix();
		gameObject.SetParent(base.gameObject.GetParent());
		gameObject.transform.forward = transform.forward;
		SendMessage("OnUnstub", gameObject, SendMessageOptions.DontRequireReceiver);
		Object.Destroy(base.gameObject);
	}
}
