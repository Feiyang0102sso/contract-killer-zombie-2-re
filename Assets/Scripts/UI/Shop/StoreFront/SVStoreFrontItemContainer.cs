using Glu.AssetBundles;
using UnityEngine;

public class SVStoreFrontItemContainer : StoreFrontItemContainer
{
	public float width;

	public float height;

	private UIScrollList _list;

	private void Awake()
	{
		_list = UIScrollList.Create("internal_UIScrollList", default(Vector3));
		_list.orientation = UIScrollList.ORIENTATION.VERTICAL;
		_list.transform.parent = transform;
		_list.transform.localPosition = new Vector3(0f, 0f, 0f);
		_list.alignment = UIScrollList.ALIGNMENT.LEFT_TOP;
		_list.viewableArea.x = width;
		_list.viewableArea.y = height;
		_list.activateWhenAdding = false;
		_list.clipContents = false;
	}

	public override void Init(ShopConfig shopConfig)
	{
	}

	public override void Clear()
	{
		_list.ClearList(true);
	}

	public override void AddItem(StoreFrontItem storeFrontItem)
	{
		GameObject gameObject = new GameObject("item");
		gameObject.AddComponent<UIListItemContainer>();
		storeFrontItem.transform.parent = gameObject.transform;
		storeFrontItem.transform.localPosition = Vector3.zero;
		_list.AddItem(gameObject);
	}

	public override void ShowGroup(ShopConfigGroup shopConfigGroup)
	{
		Clear();
		foreach (ShopConfigGroup.Reference itemRef in shopConfigGroup.itemRefs)
		{
			ShopConfigItem item = itemRef.item;
			Object obj = Resources.Load(item.prefab);
			if (obj == null)
			{
				obj = AssetBundles.Load(item.prefab);
			}
			GameObject gameObject = Object.Instantiate(obj) as GameObject;
			StoreFrontItem component = gameObject.GetComponent<StoreFrontItem>();
			component.Init(item);
			AddItem(component);
		}
	}
}
