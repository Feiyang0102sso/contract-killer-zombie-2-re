using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CInAppPurchaseCocoa : ICInAppPurchase
{
	public override void Init(string[] products, string marketPublicKey)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(products.GetLength(0));
		foreach (string key in products)
		{
			dictionary.Add(key, string.Empty);
		}
		Init(dictionary, marketPublicKey);
	}

	public override void Init(Dictionary<string, string> products, string marketPublicKey)
	{
		string[] array = new string[products.Keys.Count];
		string[] array2 = new string[products.Values.Count];
		products.Keys.CopyTo(array, 0);
		products.Values.CopyTo(array2, 0);
		InitializeIAP(array, array2);
	}

	public override bool IsTurnedOn()
	{
		return IsIAPEnabled();
	}

	public override bool IsAvailable()
	{
		return IsIAPInitialized();
	}

	public override CInAppPurchaseProduct[] GetAvailableProducts()
	{
		List<CInAppPurchaseProduct> list = new List<CInAppPurchaseProduct>();
		if (IsAvailable())
		{
			string text = Marshal.PtrToStringAnsi(GetValidatedProducts());
			if (text != null && text.Length > 0)
			{
				string[] array = text.Split('\n');
				foreach (string text2 in array)
				{
					string[] array3 = text2.Split('\t');
					if (array3.Length == 7)
					{
						list.Add(new CInAppPurchaseProduct(array3[0], array3[1], array3[2], decimal.Parse(array3[3]), array3[4], array3[5], array3[6]));
					}
					else
					{
						Debug.Log("Incorrect product info parameters count: " + array3.Length.ToString());
					}
				}
			}
		}
		return list.Count > 0 ? list.ToArray() : null;
	}

	public override void BuyProduct(string product)
	{
		if (IsAvailable())
		{
			BuyProductWithID(product);
		}
	}

	public override void RestoreCompletedTransactions()
	{
		if (IsAvailable())
		{
			RestoreTransactions();
		}
	}

	public override RESTORE_STATE GetRestoreStatus()
	{
		return (RESTORE_STATE)GetRestoreCompletedTransactionsStatus();
	}

	public override TRANSACTION_STATE GetPurchaseTransactionStatus()
	{
		return (TRANSACTION_STATE)GetLastKnownStatusOfLatestTransaction();
	}

	public override string RetrieveProduct()
	{
		string result = null;
		if (IsAvailable())
		{
			result = Marshal.PtrToStringAnsi(RetrievePurchasedProduct(0, false));
		}
		return result;
	}

	public override string GetRetrievalQueueItem(int index)
	{
		string result = null;
		if (IsAvailable())
		{
			result = Marshal.PtrToStringAnsi(RetrievePurchasedProduct(index, true));
		}
		return result;
	}

	public override int GetRetrievalQueueCount()
	{
		return GetPurchasedItemsCount();
	}

	public override void RetrievalQueueDispose(int numItems)
	{
		ClearPurchasedItems(numItems);
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void InitializeIAP(string[] productIDs, string[] hashes);
#else
	private static void InitializeIAP(string[] productIDs, string[] hashes)
	{
		NonIosNative.Stub("InitializeIAP");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool IsIAPInitialized();
#else
	private static bool IsIAPInitialized()
	{
		NonIosNative.Stub("IsIAPInitialized");
		return default(bool);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool IsIAPEnabled();
#else
	private static bool IsIAPEnabled()
	{
		NonIosNative.Stub("IsIAPEnabled");
		return default(bool);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern IntPtr GetValidatedProducts();
#else
	private static IntPtr GetValidatedProducts()
	{
		NonIosNative.Stub("GetValidatedProducts");
		return default(IntPtr);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void BuyProductWithID(string product);
#else
	private static void BuyProductWithID(string product)
	{
		NonIosNative.Stub("BuyProductWithID");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void RestoreTransactions();
#else
	private static void RestoreTransactions()
	{
		NonIosNative.Stub("RestoreTransactions");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int GetRestoreCompletedTransactionsStatus();
#else
	private static int GetRestoreCompletedTransactionsStatus()
	{
		NonIosNative.Stub("GetRestoreCompletedTransactionsStatus");
		return default(int);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int GetLastKnownStatusOfLatestTransaction();
#else
	private static int GetLastKnownStatusOfLatestTransaction()
	{
		NonIosNative.Stub("GetLastKnownStatusOfLatestTransaction");
		return default(int);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern IntPtr RetrievePurchasedProduct(int occurence, bool keepProduct);
#else
	private static IntPtr RetrievePurchasedProduct(int occurence, bool keepProduct)
	{
		NonIosNative.Stub("RetrievePurchasedProduct");
		return default(IntPtr);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int GetPurchasedItemsCount();
#else
	private static int GetPurchasedItemsCount()
	{
		NonIosNative.Stub("GetPurchasedItemsCount");
		return default(int);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void ClearPurchasedItems(int count);
#else
	private static void ClearPurchasedItems(int count)
	{
		NonIosNative.Stub("ClearPurchasedItems");
	}
#endif
}
