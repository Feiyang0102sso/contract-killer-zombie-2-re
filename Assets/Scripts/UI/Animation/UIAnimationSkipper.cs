using UnityEngine;

public class UIAnimationSkipper : MonoBehaviour
{
	private void Start()
	{
		UIManager.instance.AddMouseTouchPtrListener(AllInputDelegate);
	}

	private void OnTap()
	{
	}

	public void AllInputDelegate(POINTER_INFO info)
	{
		if (info.evt == POINTER_INFO.INPUT_EVENT.PRESS || info.evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			SkipAnimations();
		}
	}

	private void SkipAnimations()
	{
		foreach (UIAnimation uIAnimation in GetComponentsInChildren<UIAnimation>(true))
		{
			uIAnimation.Finish();
		}
		Object.Destroy(this);
	}

	private void OnDestroy()
	{
		if (UIManager.Exists())
		{
			UIManager.instance.RemoveMouseTouchPtrListener(AllInputDelegate);
		}
	}
}
