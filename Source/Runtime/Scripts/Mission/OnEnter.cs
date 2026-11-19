using UnityEngine;

public class OnEnter : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		Character component = collider.GetComponent<Human>();
		if ((bool)component)
		{
			SendMessageUpwards("OnEnterArea", new AreaVisitInfo(component, gameObject));
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		Character component = collider.GetComponent<Human>();
		if ((bool)component)
		{
			SendMessageUpwards("OnLeaveArea", new AreaVisitInfo(component, gameObject), SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnActivate()
	{
	}
}
