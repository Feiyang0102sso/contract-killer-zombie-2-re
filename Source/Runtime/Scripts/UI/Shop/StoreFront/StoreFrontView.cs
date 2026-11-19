using UnityEngine;

public class StoreFrontView : MonoBehaviour
{
	private StoreGroupContainer _groupContainer;

	private DVStoreFrontItemContainer _itemContainer;

	private ShopConfig _shopConfig;

	public StoreGroupContainer groupContainer
	{
		get
		{
			return _groupContainer;
		}
	}

	public DVStoreFrontItemContainer itemContainer
	{
		get
		{
			return _itemContainer;
		}
	}

	public ShopConfig shopConfig
	{
		get
		{
			return _shopConfig;
		}
	}

	protected void Awake()
	{
		_groupContainer = GetComponentInChildren<StoreGroupContainer>();
		_itemContainer = GetComponentInChildren<DVStoreFrontItemContainer>();
	}

	public void Init(ShopConfig shopConfig)
	{
		_shopConfig = shopConfig;
		_groupContainer.Init(shopConfig, _itemContainer);
	}
}
