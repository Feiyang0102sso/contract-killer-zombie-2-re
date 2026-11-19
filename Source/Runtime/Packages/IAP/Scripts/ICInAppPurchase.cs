using System.Collections.Generic;

public abstract class ICInAppPurchase
{
	public enum TRANSACTION_STATE
	{
		NONE,
		ACTIVE,
		SUCCESS,
		FAILED,
		CANCELLED,
		INTERRUPTED
	}

	public enum RESTORE_STATE
	{
		NONE,
		ACTIVE,
		SUCCESS_EMPTY,
		SUCCESS_DELIVERY,
		FAILED
	}

	protected const string DEBUG_TAG = "[Glu IAP]";

	private static ICInAppPurchase m_instance = null;

	public static ICInAppPurchase GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new CInAppPurchaseCocoa();
		}
		return m_instance;
	}

	public void Init(string[] products)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(products.GetLength(0));
		foreach (string key in products)
		{
			dictionary.Add(key, string.Empty);
		}
		Init(dictionary, null);
	}

	public abstract void Init(Dictionary<string, string> products, string marketPublicKey);

	public abstract void Init(string[] products, string marketPublicKey);

	public abstract bool IsTurnedOn();

	public abstract bool IsAvailable();

	public abstract CInAppPurchaseProduct[] GetAvailableProducts();

	public abstract void BuyProduct(string product);

	public abstract void RestoreCompletedTransactions();

	public abstract RESTORE_STATE GetRestoreStatus();

	public abstract TRANSACTION_STATE GetPurchaseTransactionStatus();

	public abstract string RetrieveProduct();

	public abstract int GetRetrievalQueueCount();

	public abstract string GetRetrievalQueueItem(int index);

	public abstract void RetrievalQueueDispose(int numItems);
}
