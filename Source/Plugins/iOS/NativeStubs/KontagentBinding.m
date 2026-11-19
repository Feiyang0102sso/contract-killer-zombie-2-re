// Kontagent analytics stub (tuned build only). Entry points map one-to-one to the DllImports in Assets/Packages/Kontagent/Scripts/Kontagent.cs.

void KontagentStartSession(const char* apiKey, int testMode, int trusted, const char* integrityDetails, const char* appVersion)
{
}

void KontagentCustomEvent(const char* name, const char* st1, const char* st2, const char* st3, const char* level, const char* val, const char* data)
{
}

void KontagentRevenueTracking(int val)
{
}

// C# reads it with Marshal.PtrToStringAnsi; return an empty string rather than NULL
const char* KontagentGetUserID(void)
{
	return "";
}
