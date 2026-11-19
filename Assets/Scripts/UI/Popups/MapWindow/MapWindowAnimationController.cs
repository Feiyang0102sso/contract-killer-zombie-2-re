using UnityEngine;

public class MapWindowAnimationController : UIWindow
{
	private bool _isRunning;

	protected override void Activate()
	{
		base.Activate();
	}

	private void Update()
	{
		if (!Hidden)
		{
			bool flag = !StoreScreen.ShopIsOpen;
			if (flag && !_isRunning)
			{
				Debug.Log("Animate MapWindow...");
				_isRunning = true;
				CommonGUI componentInChildren = Singleton.UI.MapWindow.GetComponentInChildren<CommonGUI>();
				if ((bool)componentInChildren)
				{
					componentInChildren.SendMessage("RollupAllGoals", SendMessageOptions.RequireReceiver);
				}
				MapWindow mapWindow = Singleton.UI.MapWindow;
				MapLocation[] componentsInChildren = mapWindow.GetComponentsInChildren<MapLocation>();
				foreach (MapLocation mapLocation in componentsInChildren)
				{
					GUIUtils.ResumeAllAnimations(mapLocation.gameObject);
				}
				MapLocation.AnimIndex = 0;
			}
			else if (flag && _isRunning && Util.DoRarely(this, "ANIM_CHECK", 1f))
			{
				bool flag2 = false;
				MapWindow mapWindow2 = Singleton.UI.MapWindow;
				MapLocation[] componentsInChildren2 = mapWindow2.GetComponentsInChildren<MapLocation>();
				foreach (MapLocation mapLocation2 in componentsInChildren2)
				{
					if (mapLocation2.GetComponentsInChildren<UIAnimation>().Length > 0)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					Close();
					Debug.Log("MapWindow animation completed...");
				}
			}
		}
	}
}
