using System;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
	protected const float LayerDelta = -10f;

	protected static Action<int> OnAddOverlayGlobal = delegate
	{
	};

	protected static Action<int> OnRemoveOverlayGlobal = delegate
	{
	};

	private static int PausesCount = 0;

	[HideInInspector]
	public int LayerNum = -1;

	private int _overlayLayer = -1;

	private bool _overlay;

	private bool _hidden;

	private bool _activated;

	private bool _deactivated;

	private bool _withPause;

	private UIAnimationAlpha _fader;

	protected bool _started;

	protected Transform _transform;

	protected BoxCollider _boxCollider;

	protected Vector2 _position = Vector2.zero;

	public Action OnShowDelegate = () =>
	{
	};

	public Action OnDestroyDelegate = () =>
	{
	};

	public Vector2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			UpdatePosition();
		}
	}

	public static void Restart()
	{
		OnAddOverlayGlobal = delegate
		{
		};
		OnRemoveOverlayGlobal = delegate
		{
		};
		PausesCount = 0;
	}

	protected virtual void Awake()
	{
		_transform = GetComponent<Transform>();
		_boxCollider = GetComponent<BoxCollider>();
		if (_boxCollider == null)
		{
			_boxCollider = gameObject.AddComponent<BoxCollider>();
			_boxCollider.size = new Vector3(400f, 250f, 0.01f);
			_boxCollider.center = Vector3.zero;
		}
		_boxCollider.enabled = true;
		UpdatePosition();
		OnAddOverlayGlobal = (Action<int>)Delegate.Combine(OnAddOverlayGlobal, new Action<int>(OnAddOverlay));
		OnRemoveOverlayGlobal = (Action<int>)Delegate.Combine(OnRemoveOverlayGlobal, new Action<int>(OnRemoveOverlay));
	}

	protected virtual void Start()
	{
		if (!_hidden && !_activated)
		{
			Activate();
		}
		_started = true;
	}

	protected virtual void Activate()
	{
		if (_activated)
		{
			return;
		}
		int num = 0;
		if (Singleton.Exists())
		{
			foreach (UIWindow openedWindow in Singleton.UI.OpenedWindows)
			{
				num = Mathf.Max(num, openedWindow.LayerNum);
			}
		}
		LayerNum = num + 1;
		if (_overlay)
		{
			OnAddOverlayGlobal(LayerNum);
		}
		UpdatePosition();
		OnShowDelegate();
		if (Singleton.Exists())
		{
			Singleton.UI.OpenedWindows.Add(this);
		}
		if ((bool)_fader)
		{
			_fader.RunAnimation();
		}
		_activated = true;
	}

	protected virtual void Deactivate()
	{
		if (!_activated)
		{
			Activate();
		}
		if (_deactivated)
		{
			return;
		}
		if (Singleton.Exists())
		{
			Singleton.UI.OpenedWindows.Remove(this);
		}
		if (_overlay)
		{
			OnRemoveOverlayGlobal(LayerNum);
		}
		OnAddOverlayGlobal = (Action<int>)Delegate.Remove(OnAddOverlayGlobal, new Action<int>(OnAddOverlay));
		OnRemoveOverlayGlobal = (Action<int>)Delegate.Remove(OnRemoveOverlayGlobal, new Action<int>(OnRemoveOverlay));
		OnDestroyDelegate();
		_deactivated = true;
	}

	protected virtual void OnDestroy()
	{
		Deactivate();
	}

	public virtual void Close()
	{
		Deactivate();
		UnityEngine.Object.Destroy(gameObject);
	}

	public void WithFade()
	{
		WithFade(0.4f);
	}

	public void WithFade(float animDuration)
	{
		if (_fader)
		{
			return;
		}
		GameObject gameObject = Util.CreateGameObject("PopupFader");
		gameObject.GetComponent<SimpleSprite>().Color = new Color(0f, 0f, 0f, 0f);
		_fader = gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 0.8f, 0f, animDuration, EZAnimation.EASING_TYPE.Linear, false);
		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0.1f);
	}

	public void WithPause()
	{
		if (_withPause)
		{
			return;
		}
		OnShowDelegate += new Action(Pause);
		OnDestroyDelegate += new Action(Unpause);
		_withPause = true;
	}

	private static void Pause()
	{
		if (PausesCount == 0)
		{
			TimeControl.Pause(true, TimeControl.PauseTypes.Dialog);
			if (Loader.IsGameScene() && Singleton.Exists() && Singleton.UI.Hud != null)
			{
				Singleton.UI.Hud.Show(false);
			}
		}
		PausesCount++;
	}

	private static void Unpause()
	{
		PausesCount--;
		if (PausesCount == 0)
		{
			TimeControl.Pause(false, TimeControl.PauseTypes.Dialog);
			if (Loader.IsGameScene() && Singleton.Exists() && Singleton.UI.Hud != null)
			{
				Singleton.UI.Hud.Show(true);
			}
		}
	}

	public void Show()
	{
		Hidden = false;
	}

	public void Hide()
	{
		Hidden = true;
	}

	public bool Hidden
	{
		get
		{
			return _hidden;
		}
		set
		{
			_hidden = value;
			if (!_hidden && !_activated && _started)
			{
				Activate();
			}
		}
	}

	public bool IsOverlayed()
	{
		return _overlayLayer >= 0;
	}

	public bool IsTopmost()
	{
		foreach (UIWindow openedWindow in Singleton.UI.OpenedWindows)
		{
			if (openedWindow.LayerNum > LayerNum)
			{
				return false;
			}
		}
		return true;
	}

	public void Overlay()
	{
		_overlay = true;
	}

	public void UpdatePosition()
	{
		if (_hidden || IsOverlayed())
		{
			MoveOutsideCulling();
		}
		else
		{
			MoveOnProperLayer();
		}
	}

	private void MoveOnProperLayer()
	{
		_transform.position = new Vector3(_position.x, _position.y, (float)LayerNum * -10f);
	}

	private void MoveOutsideCulling()
	{
		_transform.position = new Vector3(_position.x, _position.y, 100000f);
	}

	private void OnAddOverlay(int overlayLayerNum)
	{
		if (LayerNum < overlayLayerNum)
		{
			_overlayLayer = overlayLayerNum;
			UpdatePosition();
		}
	}

	private void OnRemoveOverlay(int overlayLayerNum)
	{
		if (_overlayLayer == overlayLayerNum)
		{
			_overlayLayer = -1;
			UpdatePosition();
		}
	}
}
