using UnityEngine;

public class MapButton : MonoBehaviour
{
	private UIButton _button;

	public void OnInput(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.DRAG:
		{
			MapScroller mapScroller = gameObject.FindComponentInParents<MapScroller>();
			if ((bool)mapScroller)
			{
				mapScroller.OnInput(ref ptr);
			}
			break;
		}
		case POINTER_INFO.INPUT_EVENT.TAP:
		{
			MapLocation mapLocation = gameObject.FindComponentInParents<MapLocation>();
			if ((bool)mapLocation)
			{
				mapLocation.SelectMission();
			}
			break;
		}
		}
	}

	private void Awake()
	{
		_button = GetComponent<UIButton>();
		if ((bool)_button)
		{
			_button.AddInputDelegate(OnInput);
		}
	}
}
