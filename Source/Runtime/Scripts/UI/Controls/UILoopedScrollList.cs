using System;
using System.Collections.Generic;
using UnityEngine;

public class UILoopedScrollList : MonoBehaviour, IEZDragDrop, IUIObject
{
	public float ItemWidth;

	public float MinDragToAutoScroll;

	public float MaxDragToConsiderDragAsTap;

	public float FadeOutCoeff = 0.05f;

	public Action OnTapDelegate = () =>
	{
	};

	private List<GameObject> _items = new List<GameObject>();

	private Transform _mover;

	private bool _isDragging;

	private bool _isAutoScrolling;

	private float _autoScrollX;

	private int _indexBeforeScroll;

	private float _posXBeforeScroll;

	private bool _moveEnabled;

	private Vector3 _lastPos;

	public List<GameObject> Items
	{
		get
		{
			return _items;
		}
	}

	public virtual void OnInput(POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_lastPos = ptr.ray.origin;
			_isDragging = true;
			_isAutoScrolling = false;
			_indexBeforeScroll = GetCurrentItemIndex();
			_posXBeforeScroll = _mover.position.x;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (_moveEnabled)
			{
				_mover.Translate((ptr.ray.origin - _lastPos).x, 0f, 0f);
				_lastPos = (_lastPos = ptr.ray.origin);
				UpdateItemsPositions();
			}
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			_isDragging = false;
			_isAutoScrolling = false;
			if (_indexBeforeScroll == GetCurrentItemIndex() && Mathf.Abs(_mover.position.x - _posXBeforeScroll) > MinDragToAutoScroll && _moveEnabled)
			{
				ScrollTo((_mover.position.x > _posXBeforeScroll) ? 1 : (-1));
			}
			else if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP || Mathf.Abs(_mover.position.x - _posXBeforeScroll) < MaxDragToConsiderDragAsTap)
			{
				OnTapDelegate();
			}
			break;
		}
	}

	private void Awake()
	{
		_mover = new GameObject("Mover").transform;
		_mover.parent = transform;
		_mover.localPosition = Vector3.zero;
		if (gameObject.GetComponent<BoxCollider>() == null)
		{
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(ItemWidth, ItemWidth, 0.1f);
		}
	}

	private void Update()
	{
		if (!(_isDragging))
		{
			float x = _mover.localPosition.x;
			float num = Mathf.Round(x / ItemWidth) * ItemWidth;
			if (_isAutoScrolling)
			{
				num = _autoScrollX;
				if (Mathf.Abs(x - _autoScrollX) < 0.001f)
				{
					_isAutoScrolling = false;
				}
			}
			if ((double)Mathf.Abs(x - num) > 0.001)
			{
				_mover.localPosition = new Vector3(Mathf.Lerp(x, num, Time.deltaTime * 10f), 0f, 0f);
				UpdateItemsPositions();
			}
		}
	}

	public void ScrollTo(int relativeIndex)
	{
		if (_isDragging)
		{
			return;
		}
		if (!_moveEnabled)
		{
			return;
		}
		float x = _mover.localPosition.x;
		_autoScrollX = Mathf.Round(x / ItemWidth + (float)relativeIndex) * ItemWidth;
		_isAutoScrolling = true;
	}

	public void AddSceneItem(GameObject item)
	{
		_items.Add(item);
		item.transform.parent = _mover;
		UpdateItemsPositions();
		_moveEnabled = _items.Count > 1;
	}

	public GameObject GetCurrentItem()
	{
		if (_items.Count == 0)
		{
			return null;
		}
		return _items[GetCurrentItemIndex()];
	}

	private int GetCurrentItemIndex()
	{
		if (_items.Count == 0)
		{
			return -1;
		}
		float num = ItemWidth * (float)_items.Count;
		float num2 = _mover.localPosition.x + ItemWidth;
		int num3 = -Mathf.RoundToInt(num2 / ItemWidth) % _items.Count;
		num3 = (num3 + _items.Count) % _items.Count;
		return num3;
	}

	private void UpdateItemsPositions()
	{
		float num = ItemWidth * (float)_items.Count;
		for (int i = 0; i < _items.Count; i++)
		{
			float num2 = ItemWidth * (float)i;
			if (_items.Count == 1)
			{
				num2 = 0f;
			}
			else
			{
				num2 = num2 - Mathf.Ceil((_mover.localPosition.x + num2) / num) * num + ItemWidth;
			}
			_items[i].transform.localPosition = new Vector3(num2, 0f, 0f);
			float num3 = Mathf.Clamp01(1f - Mathf.Abs((_mover.localPosition.x + num2) * FadeOutCoeff));
			Transform transform = _items[i].transform;
			for (int j = 0; j < transform.childCount; j++)
			{
				bool flag = num3 <= 0.001f;
				SimpleSprite component = transform.GetChild(j).GetComponent<SimpleSprite>();
				if ((bool)component)
				{
					component.Hide(flag);
					if (!flag)
					{
						component.SetColor(new Color(1f, 1f, 1f, num3));
					}
				}
				UIPackable component2 = transform.GetChild(j).GetComponent<UIPackable>();
				if ((bool)component2)
				{
					component2.GetComponent<Renderer>().enabled = !flag;
					if (!flag)
					{
						Color color = component2.Color;
						color.a = num3;
						component2.Color = color;
					}
				}
			}
		}
	}

	public void SetCurrentItem(int index)
	{
		if (!(_items.Count != 0))
		{
			return;
		}
		index = (_items.Count - Mathf.Clamp(index, 0, _items.Count) - 1) % _items.Count;
		float x = (float)index * ItemWidth;
		_mover.localPosition = new Vector3(x, 0f, 0f);
		UpdateItemsPositions();
	}

	public void Clear()
	{
		foreach (GameObject item in _items)
		{
			UnityEngine.Object.Destroy(item);
		}
		_items.Clear();
		_moveEnabled = false;
	}

	public object Data { get; set; }

	public virtual bool controlIsEnabled { get; set; }

	public virtual bool DetargetOnDisable { get; set; }

	public IUIObject GetControl(ref POINTER_INFO ptr)
	{
		return this;
	}

	public virtual IUIContainer Container { get; set; }

	public bool RequestContainership(IUIContainer cont)
	{
		return false;
	}

	public virtual bool GotFocus()
	{
		return false;
	}

	public virtual void SetInputDelegate(EZInputDelegate del)
	{
	}

	public virtual void AddInputDelegate(EZInputDelegate del)
	{
	}

	public virtual void RemoveInputDelegate(EZInputDelegate del)
	{
	}

	public virtual void SetValueChangedDelegate(EZValueChangedDelegate del)
	{
	}

	public virtual void AddValueChangedDelegate(EZValueChangedDelegate del)
	{
	}

	public virtual void RemoveValueChangedDelegate(EZValueChangedDelegate del)
	{
	}

	public LayerMask DropMask { get; set; }

	public bool IsDraggable { get; set; }

	public float DragOffset { get; set; }

	public EZAnimation.EASING_TYPE CancelDragEasing { get; set; }

	public float CancelDragDuration { get; set; }

	public bool IsDragging { get; set; }

	public GameObject DropTarget { get; set; }

	public bool DropHandled { get; set; }

	public void DragUpdatePosition(POINTER_INFO ptr)
	{
	}

	public void CancelDrag()
	{
	}

	public void OnEZDragDrop_Internal(EZDragDropParams parms)
	{
	}

	public void AddDragDropDelegate(EZDragDropDelegate del)
	{
	}

	public void RemoveDragDropDelegate(EZDragDropDelegate del)
	{
	}

	public void SetDragDropDelegate(EZDragDropDelegate del)
	{
	}

	public void SetDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public void AddDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public void RemoveDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public EZDragDropHelper.DragDrop_InternalDelegate GetDragDropInternalDelegate()
	{
		return null;
	}
}
