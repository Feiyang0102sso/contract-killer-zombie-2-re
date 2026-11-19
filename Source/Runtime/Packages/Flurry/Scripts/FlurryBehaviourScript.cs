using UnityEngine;

public class FlurryBehaviourScript : MonoBehaviour
{
	private void Awake()
	{
		enabled = false;
		useGUILayout = false;
	}

	private void OnDestroy()
	{
		CFlurry.StopSession();
	}
}
