// PlayHaven stub (tuned build only). Entry points map one-to-one to the DllImports in Assets/Packages/PlayHaven/Scripts/PlayHavenBinding.cs.
// Requests are never sent, so C# never receives PlayHaven callbacks, and neither content nor ads appear.

extern "C"
{

void _PlayHavenOpenRequest(int hash, const char* token, const char* secret, const char* customUDID)
{
}

void _PlayHavenMetadataRequest(int hash, const char* token, const char* secret, const char* placement)
{
}

void _PlayHavenContentRequest(int hash, const char* token, const char* secret, const char* placement, int showsOverlayImmediately)
{
}

void _PlayHavenPreloadRequest(int hash, const char* token, const char* secret, const char* placement)
{
}

void _PlayHavenCancelRequest(int hash)
{
}

void _PlayHavenProductPurchaseResolution(int action)
{
}

void _PlayHavenIAPTrackingRequest(const char* token, const char* secret, const char* productId, int quantity, int resolution)
{
}

int _PlayHavenOptOutStatus()
{
	return 0;
}

void _PlayHavenSetOptOutStatus(int yesOrNo)
{
}

}
