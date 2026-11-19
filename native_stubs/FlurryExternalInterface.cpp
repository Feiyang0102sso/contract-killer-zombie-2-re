// Flurry analytics stub (tuned build only). Entry points map one-to-one to the DllImports in Assets/Packages/Flurry/Scripts/CFlurry.cs.
#include <stdlib.h>
#include <string.h>

extern "C"
{

void CFlurry_SetAppVersion(const char* version)
{
}

// The C# declared return type is string: Mono frees this pointer after marshaling, so return a heap copy
const char* CFlurry_GetFlurryAgentVersion()
{
	return strdup("");
}

void CFlurry_SetShowErrorInLogEnabled(int value)
{
}

void CFlurry_SetDebugLogEnabled(int value)
{
}

void CFlurry_SetSessionContinueSeconds(int seconds)
{
}

void CFlurry_SetSecureTransportEnabled(int value)
{
}

// The C# declared return type is IntPtr, read with Marshal.PtrToStringAnsi and never freed
const char* CFlurry_GetUserID()
{
	return "";
}

void CFlurry_StartSession(const char* apiKey, int trusted, const char** keys, const char** values)
{
}

void CFlurry_LogEvent(const char* eventTypeId, const char** keys, const char** values)
{
}

void CFlurry_SetSessionReportsOnCloseEnabled(int sendSessionReportsOnClose)
{
}

void CFlurry_SetSessionReportsOnPauseEnabled(int sendSessionReportsOnPause)
{
}

void CFlurry_SetEventLoggingEnabled(int value)
{
}

}
