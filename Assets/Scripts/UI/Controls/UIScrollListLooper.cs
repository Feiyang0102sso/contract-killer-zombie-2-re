using System;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollListLooper : UIScrollList
{
	public delegate void CurrentItemChangedDelegate(IUIListObject currentItem);

	private List<IUIListObject> _sceneItems = new List<IUIListObject>();

	private IUIListObject _firstCloneItem;

	private IUIListObject _lastCloneItem;

	private bool _inited;

	private int _currentItemIndex;

	private bool _colliderUpdated;

	private float _loopSimulateDelta;

	private CurrentItemChangedDelegate _currentItemChangedDelegate;

	public void AddCurrentItemChangedDelegate(CurrentItemChangedDelegate del)
	{
		_currentItemChangedDelegate += del;
	}

	public float AbsouluteScrollPosition
	{
		get
		{
			return scrollPos + _loopSimulateDelta;
		}
		set
		{
			ScrollListTo_Internal(value - _loopSimulateDelta);
		}
	}

	private void SetClones(IUIListObject first, IUIListObject last)
	{
		if (first != null)
		{
			if (_firstCloneItem != null)
			{
				UnityEngine.Object.Destroy(_firstCloneItem.gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(first.gameObject) as GameObject;
			gameObject.name = first.gameObject.name;
			_firstCloneItem = (IUIListObject)gameObject.GetComponent(typeof(IUIListObject));
		}
		if (last != null)
		{
			if (_lastCloneItem != null)
			{
				UnityEngine.Object.Destroy(_lastCloneItem.gameObject);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(last.gameObject) as GameObject;
			gameObject2.name = last.gameObject.name;
			_lastCloneItem = (IUIListObject)gameObject2.GetComponent(typeof(IUIListObject));
		}
	}

	public void AddSceneItem(GameObject itemGO)
	{
		if (!_inited)
		{
			Awake();
		}
		Util.MarkAllChildrenAsClone(itemGO);
		IUIListObject item = (IUIListObject)itemGO.GetComponent(typeof(IUIListObject));
		_sceneItems.Add(item);
		Rearrange();
	}

	public void Clear()
	{
		foreach (IUIListObject sceneItem in _sceneItems)
		{
			UnityEngine.Object.Destroy(sceneItem.gameObject);
		}
		_sceneItems.Clear();
		Rearrange();
	}

	public void SetCurrentItem(IUIListObject currentItem)
	{
		int num = _sceneItems.IndexOf(currentItem);
		if (!(num >= 0))
		{
			return;
		}
		SetCurrentItem(num);
	}

	public virtual void SetCurrentItem(object data)
	{
		for (int i = 0; i < _sceneItems.Count; i++)
		{
			if (_sceneItems[i].Data as string == data as string)
			{
				SetCurrentItem(i);
				return;
			}
		}
	}

	private void SetCurrentItem(int index)
	{
		_currentItemIndex = index;
		ScrollListTo(GetSelectedItemScrollPosition());
	}

	protected virtual void Update()
	{
		if (!_colliderUpdated && (bool)GetComponent<BoxCollider>())
		{
			Vector3 size = GetComponent<BoxCollider>().size;
			size.x *= 2f;
			size.y *= 2f;
			GetComponent<BoxCollider>().size = size;
			_colliderUpdated = true;
		}
		SimulateLoopPositionChange();
		if (_currentItemIndex != GetCurrentItemIndex())
		{
			_currentItemIndex = GetCurrentItemIndex();
			if (_currentItemChangedDelegate != null)
			{
				_currentItemChangedDelegate(GetCurrentItem());
			}
		}
		for (int i = 0; i < visibleItems.Count; i++)
		{
			Transform transform = visibleItems[i].gameObject.transform;
			float num = (float)visibleItems[i].Index / (float)(items.Count - 1);
			float num2 = Mathf.Clamp01(1f - Mathf.Abs((scrollPos - num) * (float)items.Count));
			for (int j = 0; j < transform.GetChildCount(); j++)
			{
				bool flag = num2 <= 0.001f;
				SimpleSprite component = transform.GetChild(j).GetComponent<SimpleSprite>();
				if ((bool)component)
				{
					component.Hide(flag);
					if (!flag)
					{
						component.SetColor(new Color(1f, 1f, 1f, num2));
					}
				}
				UIPackable component2 = transform.GetChild(j).GetComponent<UIPackable>();
				if ((bool)component2)
				{
					component2.renderer.enabled = !flag;
					if (!flag)
					{
						Color color = component2.Color;
						color.a = num2;
						component2.Color = color;
					}
				}
			}
		}
	}

	public IUIListObject GetCurrentItem()
	{
		if (_sceneItems.Count == 0)
		{
			return null;
		}
		return _sceneItems[_currentItemIndex];
	}

	public IUIListObject GetNextItem(int shift)
	{
		if (_sceneItems.Count == 0)
		{
			return null;
		}
		return _sceneItems[(_currentItemIndex + shift + _sceneItems.Count) % _sceneItems.Count];
	}

	private int GetCurrentItemIndex()
	{
		if (_sceneItems.Count == 0)
		{
			return -1;
		}
		if (_sceneItems.Count == 1)
		{
			return 0;
		}
		int num = Mathf.RoundToInt(ScrollPosition * (float)(GetListItemsCount() - 1));
		if (num == 0)
		{
			num = _sceneItems.Count - 1;
		}
		else if (num == GetListItemsCount() - 1)
		{
			num = 0;
		}
		else
		{
			num = num - 1;
		}
		return num;
	}

	private float GetSelectedItemScrollPosition()
	{
		if (_sceneItems.Count == 0)
		{
			return 0f;
		}
		if (_sceneItems.Count == 1)
		{
			return 0f;
		}
		return (float)(_currentItemIndex + 1) / (float)(GetListItemsCount() - 1);
	}

	private void SimulateLoopPositionChange()
	{
		if (GetListItemsCount() <= 1)
		{
			return;
		}
		float num = 1f / (float)(GetListItemsCount() - 1);
		float num2 = num * (float)_sceneItems.Count;
		float num3 = 0.0001f;
		if (ScrollPosition < num)
		{
			ScrollListTo(ScrollPosition + num2 - num3);
			_loopSimulateDelta += -(num2 - num3);
		}
		else if (ScrollPosition >= 1f)
		{
			ScrollListTo(ScrollPosition - num2 + num3);
			_loopSimulateDelta -= -(num2 - num3);
		}
	}

	private int GetListItemsCount()
	{
		if (_sceneItems.Count <= 1)
		{
			return _sceneItems.Count;
		}
		return _sceneItems.Count + 2;
	}

	private void Rearrange()
	{
		ClearList(false);
		if (_sceneItems.Count == 0)
		{
			return;
		}
		bool flag = true;
		if (_sceneItems.Count == 1)
		{
			flag = false;
		}
		if (flag)
		{
			SetClones(_sceneItems[0], _sceneItems[_sceneItems.Count - 1]);
		}
		else
		{
			SetClones(null, null);
		}
		if (flag)
		{
			InsertItem(_lastCloneItem, GetListItemsCount());
		}
		foreach (IUIListObject sceneItem in _sceneItems)
		{
			InsertItem(sceneItem, GetListItemsCount());
		}
		if (flag)
		{
			InsertItem(_firstCloneItem, GetListItemsCount());
		}
		ScrollToItem(_sceneItems[0], 0f);
		_currentItemIndex = 0;
	}

	private void ScrollListInputDelegate(ref POINTER_INFO info)
	{
		if (info.evt == POINTER_INFO.INPUT_EVENT.RELEASE)
		{
		}
	}
}
