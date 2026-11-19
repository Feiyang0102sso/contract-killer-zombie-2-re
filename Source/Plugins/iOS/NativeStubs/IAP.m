// In-app purchase stub (tuned build only). Entry points map one-to-one to the DllImports in Assets/Packages/IAP/Scripts/CInAppPurchaseCocoa.cs.
// While IsIAPInitialized() is false, C# never queries products or starts a purchase or restore.

void InitializeIAP(const char** productIDs, const char** hashes)
{
}

int IsIAPInitialized(void)
{
	return 0;
}

int IsIAPEnabled(void)
{
	return 0;
}

// C# reads it with Marshal.PtrToStringAnsi; return an empty string rather than NULL
const char* GetValidatedProducts(void)
{
	return "";
}

void BuyProductWithID(const char* product)
{
}

void RestoreTransactions(void)
{
}

int GetRestoreCompletedTransactionsStatus(void)
{
	return 0;
}

int GetLastKnownStatusOfLatestTransaction(void)
{
	return 0;
}

const char* RetrievePurchasedProduct(int occurence, int keepProduct)
{
	return "";
}

int GetPurchasedItemsCount(void)
{
	return 0;
}

void ClearPurchasedItems(int count)
{
}
