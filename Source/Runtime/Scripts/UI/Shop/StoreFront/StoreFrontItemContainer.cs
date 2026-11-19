using UnityEngine;

public abstract class StoreFrontItemContainer : MonoBehaviour
{
	public abstract void Init(ShopConfig shopConfig);

	public abstract void Clear();

	public abstract void AddItem(StoreFrontItem storeFrontItem);

	public abstract void ShowGroup(ShopConfigGroup shopConfigGroup);
}
