using System.Collections.Generic;
using UnityEngine;

public class StoreGroupContainer : MonoBehaviour
{
	public enum State
	{
		Folding,
		Unfolding
	}

	public GameObject ShadowPrefab;

	public GameObject SelectedGroupPrefab;

	private GameObject _selectedGroupButton;

	private ShopConfigGroup _selectedShopConfigGroup;

	private State _state;

	private List<StoreGroupButton> _groupButtons;

	private DVStoreFrontItemContainer _itemContainer;

	private ShopConfig _shopConfig;

	private bool _allInputListener;

	public GameObject SelectedGroupButton
	{
		get
		{
			return _selectedGroupButton;
		}
	}

	public ShopConfigGroup GetSelectedGroup()
	{
		return _selectedShopConfigGroup;
	}

	public State GetState()
	{
		return _state;
	}

	public List<StoreGroupButton> groupButtons
	{
		get
		{
			return _groupButtons;
		}
	}

	private void Awake()
	{
		_state = State.Folding;
	}

	public void Init(ShopConfig shopConfig, DVStoreFrontItemContainer itemContainer)
	{
		_shopConfig = shopConfig;
		_itemContainer = itemContainer;
		Clear();
	}

	public void Clear()
	{
		_groupButtons = null;
		_state = State.Folding;
	}

	public void SelectGroup(ShopConfigGroup shopConfigGroup)
	{
		Fold(shopConfigGroup, null);
	}

	private void AddGroupButton(StoreGroupButton storeGroupButton)
	{
		if (_groupButtons == null)
		{
			_groupButtons = new List<StoreGroupButton>();
		}
		storeGroupButton.transform.parent = transform;
		storeGroupButton.transform.localPosition = default(Vector3);
		storeGroupButton.SetOnClickDelegate(OnGroupClicked);
		_groupButtons.Add(storeGroupButton);
	}

	private void RemoveGroupButtons()
	{
		if (!(_groupButtons == null))
		{
			int num = 0;
			foreach (StoreGroupButton groupButton in _groupButtons)
			{
				groupButton.FadeOut();
				num++;
			}
			_groupButtons = null;
		}
	}

	private void Unshadow()
	{
		Transform transform = base.transform.Find("__storeShadowSprite");
		if (transform != null)
		{
			transform.parent = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public void Fold(ShopConfigGroup shopConfigGroup, StoreGroupButton groupButton)
	{
		_state = State.Folding;
		if (shopConfigGroup != null)
		{
			_selectedShopConfigGroup = shopConfigGroup;
			_itemContainer.ShowGroup(_selectedShopConfigGroup);
			if (_selectedGroupButton == null)
			{
				_selectedGroupButton = Object.Instantiate(SelectedGroupPrefab) as GameObject;
				_selectedGroupButton.transform.parent = gameObject.transform;
				_selectedGroupButton.transform.localPosition = new Vector3(0f, 0f, -1.1f);
				_selectedGroupButton.name = _selectedShopConfigGroup.id;
				StoreGroupButton component = _selectedGroupButton.GetComponent<StoreGroupButton>();
				component.SetOnClickDelegate(OnSelectedGroupButtonClicked);
			}
			_selectedGroupButton.GetComponent<StoreGroupButton>().Init(_selectedShopConfigGroup, this, groupButton == null ? 0f : (groupButton.transform.position.y - _selectedGroupButton.transform.position.y), false);
		}
		RemoveGroupButtons();
		Unshadow();
		SwitchAllInputListener(false);
	}

	private void SwitchAllInputListener(bool on)
	{
		if (!(_allInputListener != on))
		{
			return;
		}
		if (on)
		{
			UIManager.instance.AddMouseTouchPtrListener(AllInputDelegate);
		}
		else
		{
			UIManager.instance.RemoveMouseTouchPtrListener(AllInputDelegate);
		}
		_allInputListener = on;
	}

	private void Shadow()
	{
		if (ShadowPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(ShadowPrefab) as GameObject;
			gameObject.name = "__storeShadowSprite";
			SimpleSprite component = gameObject.GetComponent<SimpleSprite>();
			gameObject.transform.parent = base.gameObject.transform;
			component.Start();
		}
	}

	public void Unfold()
	{
		_state = State.Unfolding;
		float num = -1f * _selectedGroupButton.GetComponent<StoreGroupButton>().height;
		int num2 = 0;
		foreach (ShopConfigGroup group in _shopConfig.groups)
		{
			if (!(!Singleton.ShopData.IsShopGroupEmpty(group)))
			{
				continue;
			}
			GameObject original = Singleton.Cache.PerResourceCache.Load(group.prefab);
			GameObject gameObject = Object.Instantiate(original) as GameObject;
			gameObject.name = group.id;
			StoreGroupButton component = gameObject.GetComponent<StoreGroupButton>();
			component.Init(group, this, 0f, true);
			AddGroupButton(component);
			Vector3 localPosition = new Vector3(0f, num, 0f);
			component.transform.localPosition = localPosition;
			num -= component.height;
			num2++;
		}
		foreach (StoreGroupButton groupButton in _groupButtons)
		{
			groupButton.BringIn();
		}
		Shadow();
		SwitchAllInputListener(true);
	}

	private void OnSelectedGroupButtonClicked(StoreGroupButton groupButton)
	{
		switch (_state)
		{
		case State.Folding:
			Unfold();
			break;
		case State.Unfolding:
			Fold(groupButton.ShopConfigGroup, groupButton);
			break;
		}
	}

	public void AllInputDelegate(POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			foreach (StoreGroupButton groupButton in _groupButtons)
			{
				if ((bool)groupButton.GetComponent<UIAnimation>())
				{
					return;
				}
			}
			if (info.targetObj != null && info.targetObj.gameObject && info.targetObj.gameObject.transform.parent && info.targetObj.gameObject.transform.parent.GetComponent<StoreGroupButton>())
			{
				return;
			}
			Fold(null, null);
			break;
		}
	}

	private void OnGroupClicked(StoreGroupButton groupButton)
	{
		Fold(groupButton.ShopConfigGroup, groupButton);
	}

	private void OnDestroy()
	{
		SwitchAllInputListener(false);
	}
}
