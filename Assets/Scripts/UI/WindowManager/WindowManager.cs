using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : UIPanelManager
{
	[Serializable]
	public struct WindowData
	{
		public GameObject WindowObject;

		public string Name;

		public UIPanel Panel;

		public float Z;
	}

	private float _maxRange = -10f;

	private float _minRange = -1f;

	private float _offsetZ = 0.5f;

	private float _currentZ;

	private BoxCollider _collider;

	private List<WindowData> _listWindows;

	private List<GameObject> _queueWindows;

	private List<PopupInfo> _listPopupInfo;

	private static GameObject _window;

	private static UIPanel _windowPanel;

	private static WindowManager _instance;

	public int OpenedWindowCount()
	{
		return _listWindows.Count;
	}

	public GameObject Window
	{
		get
		{
			return _window;
		}
	}

	public static UIPanel WindowPanel
	{
		get
		{
			return _windowPanel;
		}
	}

	public static WindowManager Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = Util.FindObjectOfType<WindowManager>();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance == null || !_instance)
		{
			_instance = Util.FindObjectOfType<WindowManager>();
		}
		_currentZ = _minRange;
		_listWindows = new List<WindowData>();
		_queueWindows = new List<GameObject>();
		_listPopupInfo = new List<PopupInfo>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void SetTransitionZOrder(UIPanel panel, float posZ)
	{
		EZTransitionList transitions = panel.Transitions;
		foreach (EZTransition eZTransition in transitions.list)
		{
			for (int j = 0; j < eZTransition.animParams.GetLength(0); j++)
			{
				if (eZTransition.animationTypes[j] == EZAnimation.ANIM_TYPE.Translate || eZTransition.animationTypes[j] == EZAnimation.ANIM_TYPE.TranslateScreen)
				{
					if (eZTransition.animParams[j].mode != EZAnimation.ANIM_MODE.By)
					{
						eZTransition.animParams[j].vec = new Vector3(eZTransition.animParams[j].vec.x, eZTransition.animParams[j].vec.y, posZ);
						eZTransition.animParams[j].vec2 = new Vector3(eZTransition.animParams[j].vec2.x, eZTransition.animParams[j].vec2.y, posZ);
					}
					else
					{
						eZTransition.animParams[j].vec = new Vector3(eZTransition.animParams[j].vec.x, eZTransition.animParams[j].vec.y, 0f);
					}
				}
			}
		}
	}

	protected void ReorderWindows()
	{
		float num = _minRange;
		for (int i = 0; i < _listWindows.Count; i++)
		{
			Vector3 position = _listWindows[i].WindowObject.transform.position;
			position.z = num;
			_listWindows[i].WindowObject.transform.position = position;
			WindowData value = _listWindows[i];
			value.Z = num;
			_listWindows[i] = value;
			SetTransitionZOrder(_listWindows[i].Panel, _listWindows[i].Z);
			_currentZ = num;
			num -= _offsetZ;
		}
		UpdateColliderPosition();
	}

	protected void UpdateColliderPosition()
	{
		if (_collider == null)
		{
			gameObject.AddComponent<BoxCollider>();
			_collider = gameObject.GetComponent<BoxCollider>();
			_collider.size = new Vector3(200f, 200f, 0.01f);
			_collider.enabled = true;
		}
		_collider.enabled = true;
		_collider.center = new Vector3(0f, 0f, _currentZ + 0.005f);
	}

	public void DisableCollider()
	{
		if (_collider == null)
		{
			_collider = gameObject.GetComponent<BoxCollider>();
		}
		if (_collider != null)
		{
			_collider.enabled = false;
		}
	}

	public void CloseWindow()
	{
		if (_window == null)
		{
			return;
		}
		if ((bool)_windowPanel)
		{
			if (_windowPanel.IsTransitioning)
			{
				return;
			}
			_windowPanel.AddTempTransitionDelegate(DestroyWindowOnDismissDelegate);
			_windowPanel.StartTransition(SHOW_MODE.DismissForward);
		}
		_listWindows.RemoveAt(_listWindows.Count - 1);
		if (_listWindows.Count > 0)
		{
			_window = _listWindows[_listWindows.Count - 1].WindowObject;
			_windowPanel = _listWindows[_listWindows.Count - 1].Panel;
			_currentZ = _listWindows[_listWindows.Count - 1].Z;
			UpdateColliderPosition();
		}
		else
		{
			_window = null;
			_windowPanel = null;
			DisableCollider();
		}
	}

	public void DestroyWindowOnDismissDelegate(UIPanelBase panel)
	{
		UnityEngine.Object.DestroyObject(panel.gameObject);
		if (_queueWindows.Count > 0)
		{
			CreatePopup(_queueWindows[0], _listPopupInfo[0]);
			_queueWindows.RemoveAt(0);
			_listPopupInfo.RemoveAt(0);
		}
	}

	public void DestroyWindowOnDismissDelegate(UIPanelBase panel, EZTransition transition)
	{
		UnityEngine.Object.DestroyObject(panel.gameObject);
		if (_queueWindows.Count > 0)
		{
			CreatePopup(_queueWindows[0], _listPopupInfo[0]);
			_queueWindows.RemoveAt(0);
			_listPopupInfo.RemoveAt(0);
		}
	}

	public void CloseAllWindows()
	{
		_queueWindows.Clear();
		for (int num = _listWindows.Count - 1; num >= 0; num--)
		{
			if (_listWindows[num].Panel != null)
			{
				_listWindows[num].Panel.AddTempTransitionDelegate(DestroyWindowOnDismissDelegate);
				_listWindows[num].Panel.StartTransition(SHOW_MODE.DismissForward);
			}
			else
			{
				UnityEngine.Object.DestroyObject(_listWindows[num].WindowObject);
			}
		}
		DisableCollider();
		_listWindows.Clear();
		_window = null;
		_windowPanel = null;
	}

	public GameObject CreateWindow(GameObject windowPrefab)
	{
		WindowData item = new WindowData();
		item.WindowObject = UnityEngine.Object.Instantiate(windowPrefab) as GameObject;
		item.Name = item.WindowObject.name;
		item.Panel = item.WindowObject.GetComponent<UIPanel>();
		CloseAllWindows();
		_listWindows.Add(item);
		ReorderWindows();
		if (item.Panel != null)
		{
			item.Panel.StartTransition(SHOW_MODE.BringInForward);
		}
		_window = item.WindowObject;
		_windowPanel = item.Panel;
		return item.WindowObject;
	}

	public GameObject CreatePopup(GameObject windowPrefab, PopupInfo info)
	{
		if (!IsShowing(info.Title))
		{
			return null;
		}
		if (_currentZ - _offsetZ < _maxRange)
		{
			QueuePopup(windowPrefab, info);
			return null;
		}
		WindowData item = new WindowData();
		item.WindowObject = UnityEngine.Object.Instantiate(windowPrefab) as GameObject;
		item.Name = info.Title;
		item.Panel = item.WindowObject.GetComponent<UIPanel>();
		_listWindows.Add(item);
		ReorderWindows();
		if (item.Panel != null)
		{
			item.Panel.BringIn();
		}
		_window = item.WindowObject;
		_windowPanel = item.Panel;
		PopupControl component = _window.GetComponent<PopupControl>();
		component.SetInfo(info);
		return item.WindowObject;
	}

	public GameObject CreatePopup(GameObject windowPrefab)
	{
		if (!IsShowing(windowPrefab.name))
		{
			return null;
		}
		if (_currentZ - _offsetZ < _maxRange)
		{
			return null;
		}
		WindowData item = new WindowData();
		item.WindowObject = UnityEngine.Object.Instantiate(windowPrefab) as GameObject;
		item.Name = windowPrefab.name;
		item.Panel = item.WindowObject.GetComponent<UIPanel>();
		_listWindows.Add(item);
		ReorderWindows();
		if (item.Panel != null)
		{
			item.Panel.BringIn();
		}
		_window = item.WindowObject;
		_windowPanel = item.Panel;
		return item.WindowObject;
	}

	public void QueuePopup(GameObject popup, PopupInfo info)
	{
		_queueWindows.Add(popup);
		_listPopupInfo.Add(info);
	}

	private bool IsShowing(string popupName)
	{
		foreach (WindowData listWindow in _listWindows)
		{
			if (listWindow.Name != null && listWindow.Name.Equals(popupName))
			{
				return false;
			}
		}
		return true;
	}
}
