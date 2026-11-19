using System.Collections.Generic;
using UnityEngine;

public class DVStoreFrontItemContainer : MonoBehaviour
{
	private const float itemWidth = 127f;

	public float width;

	public float height;

	private UIScrollList _list;

	public GameObject FillerPrefab;

	private float _itemSpacingX;

	private string lastShopConfigGroupId;

	private List<ShopConfigItem> _shownItems = new List<ShopConfigItem>(32);

	private void Awake()
	{
		_itemSpacingX = 3f;
		_list = UIScrollList.Create("internal_UIScrollList", default(Vector3));
		_list.orientation = UIScrollList.ORIENTATION.VERTICAL;
		_list.transform.parent = transform;
		_list.transform.localPosition = new Vector3(0f, 0f, 0f);
		_list.alignment = UIScrollList.ALIGNMENT.LEFT_TOP;
		_list.viewableArea.x = _itemSpacingX + 254f;
		_list.viewableArea.y = height;
		_list.activateWhenAdding = false;
		_list.clipWhenMoving = false;
		_list.extraEndSpacing = 5f;
		_list.clipContents = false;
		_list.itemSpacing = 3f;
	}

	public void Clear()
	{
		foreach (ShopConfigItem shownItem in _shownItems)
		{
			Singleton.Cache.PerStringCache.Release(shownItem.id);
		}
		_shownItems.Clear();
		_list.ClearList(true);
		GameObject gameObject = new GameObject("stubPair");
		UIListItemContainer uIListItemContainer = gameObject.AddComponent<UIListItemContainer>();
		_list.AddItem(uIListItemContainer.gameObject);
	}

	public void AddItem(StoreFrontItem storeFrontItem, int col)
	{
		UIListItemContainer uIListItemContainer = _list.GetItem(_list.Count - 1) as UIListItemContainer;
		UIListItemContainer uIListItemContainer2 = _list.GetItem(_list.Count - 2) as UIListItemContainer;
		if (col == 0)
		{
			GameObject gameObject = new GameObject("pair");
			uIListItemContainer2 = gameObject.AddComponent<UIListItemContainer>();
			storeFrontItem.transform.parent = uIListItemContainer2.transform;
			storeFrontItem.transform.localPosition = default(Vector3);
			uIListItemContainer2.FindOuterEdges();
			Vector2 bottomRightEdge = uIListItemContainer2.BottomRightEdge;
			_list.InsertItem(uIListItemContainer2.gameObject.GetComponent(typeof(IUIListObject)) as IUIListObject, _list.Count - 1);
			uIListItemContainer2.ScanChildren();
		}
		else
		{
			Vector2 bottomRightEdge2 = uIListItemContainer2.BottomRightEdge;
			storeFrontItem.transform.parent = uIListItemContainer2.transform;
			storeFrontItem.transform.localPosition = new Vector3(bottomRightEdge2.x + _itemSpacingX, 0f, 0f);
			uIListItemContainer2.ScanChildren();
			uIListItemContainer.FindOuterEdges();
			Vector2 bottomRightEdge3 = uIListItemContainer.BottomRightEdge;
			uIListItemContainer.ScanChildren();
		}
	}

	public void ShowGroup(ShopConfigGroup shopConfigGroup)
	{
		Singleton.UI.Store.SwitchWeaponCompareMode(false);
		Clear();
		List<StoreFrontItem> list = new List<StoreFrontItem>();
		int num = 0;
		int num2 = 0;
		foreach (ShopConfigGroup.Reference itemRef in shopConfigGroup.itemRefs)
		{
			ShopConfigItem item = itemRef.item;
			if (item == null)
			{
				continue;
			}
			if (!item.readyToShow)
			{
				continue;
			}
			GameObject gameObject = Singleton.Cache.PerStringCache.Load(item.id, item.prefab);
			_shownItems.Add(item);
			StoreFrontItem component = gameObject.GetComponent<StoreFrontItem>();
			component.Init(item);
			component.OnBeforeRecalculatingBounds();
			AddItem(component, num2);
			list.Add(component);
			if (num2 == 1 || item is BundleShopItem)
			{
				num2 = 0;
				num++;
			}
			else
			{
				num2++;
			}
		}
		foreach (StoreFrontItem item2 in list)
		{
			item2.OnAfterRecalculatingBounds();
		}
		if (Singleton.Quality.AdvancedUIAnimations)
		{
			base.gameObject.AddComponent<UIAnimationAlpha>().Setup(0f, 1f, 0f, 0.7f, EZAnimation.EASING_TYPE.Linear, false).RunAnimation();
		}
		if (shopConfigGroup.id == "currency" && shopConfigGroup.id != lastShopConfigGroupId)
		{
			Debug.Log("Bank playhaven popup");
			if (QualitySetter.PlayHavenEnabled)
			{
				PlayHavenInterface.StartPublisherContentRequestWithPreload("bank_launch", false, false);
			}
		}
		lastShopConfigGroupId = shopConfigGroup.id;
	}
}
