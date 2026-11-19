// Tapjoy stub (tuned build only). Entry points map one-to-one to the DllImports in Assets/Packages/Tapjoy/Scripts/TapjoyPluginIOS.cs.
// The connection never succeeds, points queries return 0, and neither ads nor the offer wall appear.

extern "C"
{

void _SetCallbackHandler(const char* handlerName)
{
}

void _RequestTapjoyConnect(const char* appID, const char* secretKey)
{
}

void _SetFlagKeyValue(const char* flagKey, const char* flagValue)
{
}

void _EnableLogging(int enable)
{
}

void _ActionComplete(const char* actionID)
{
}

void _SetUserID(const char* userID)
{
}

void _ShowOffers()
{
}

void _ShowOffersWithCurrencyID(const char* currencyID, int isSelectorVisible)
{
}

void _GetFullScreenAd()
{
}

void _GetFullScreenAdWithCurrencyID(const char* currencyID)
{
}

void _SetCurrencyMultiplier(float multiplier)
{
}

void _ShowFullScreenAd()
{
}

void _GetDailyRewardAd()
{
}

void _GetDailyRewardAdWithCurrencyID(const char* currencyID)
{
}

void _ShowDailyRewardAd()
{
}

void _ShowDefaultEarnedCurrencyAlert()
{
}

void _GetTapPoints()
{
}

void _SpendTapPoints(int points)
{
}

void _AwardTapPoints(int points)
{
}

int _QueryTapPoints()
{
	return 0;
}

void _GetDisplayAd()
{
}

void _ShowDisplayAd()
{
}

void _HideDisplayAd()
{
}

void _GetDisplayAdWithCurrencyID(const char* currencyID)
{
}

void _SetDisplayAdSize(const char* size)
{
}

void _EnableDisplayAdAutoRefresh(int enable)
{
}

void _MoveDisplayAd(int x, int y)
{
}

void _SetTransitionEffect(int transition)
{
}

void _SendIAPEvent(const char* name, float price, int quantity, const char* currencyCode)
{
}

}
