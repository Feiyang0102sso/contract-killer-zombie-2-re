public class IAPBuyWindow : UIWindow
{
	private IAPShopItem _item;

	public void Init(IAPShopItem item)
	{
		_item = item;
	}

	private void Update()
	{
		if (_item == null)
		{
			return;
		}
		IAPShopConfigItem.State state = _item.state;
		if (state != IAPShopConfigItem.State.WaitingForRetrival)
		{
			ICInAppPurchase instance = ICInAppPurchase.GetInstance();
			ICInAppPurchase.TRANSACTION_STATE purchaseTransactionStatus = instance.GetPurchaseTransactionStatus();
			if (purchaseTransactionStatus != ICInAppPurchase.TRANSACTION_STATE.SUCCESS)
			{
				Close();
				if (purchaseTransactionStatus == ICInAppPurchase.TRANSACTION_STATE.CANCELLED)
				{
					Singleton.UI.PopupManager.ShowIAPCanceledPopup();
					_item.OnPurchaseCanceled();
				}
				else
				{
					GameAnalytics.IAPFailed reason = GameAnalytics.IAPFailed.Other;
					if (ICInAppPurchase.GetInstance().IsTurnedOn())
					{
						reason = GameAnalytics.IAPFailed.ParentialSettingsOn;
					}
					if (purchaseTransactionStatus == ICInAppPurchase.TRANSACTION_STATE.INTERRUPTED)
					{
						reason = GameAnalytics.IAPFailed.ResumeTimeoutFailed;
					}
					Singleton.UI.PopupManager.ShowIAPFailedPopup();
					_item.OnPurchaseFailed(reason);
				}
			}
			else if (_item.Delivered)
			{
				Close();
			}
		}
	}
}
