using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Integrity
{
	public const string KEY_IS_JAILBROKEN = "KEY_IS_JAILBROKEN";

	public const string KEY_IS_ENCRYPTED = "KEY_IS_ENCRYPTED";

	public static Version Version
	{
		get
		{
			return new Version(1, 0, 1);
		}
	}

	public static bool IsTrusted()
	{
		return IsEncrypted() && !IsJailbroken();
	}

	public static Dictionary<string, string> GetDetails()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("KEY_IS_JAILBROKEN", IsJailbroken().ToString());
		dictionary.Add("KEY_IS_ENCRYPTED", IsEncrypted().ToString());
		return dictionary;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	public static extern bool IsJailbroken();
#else
	public static bool IsJailbroken()
	{
		return PcNative.IsJailbroken();
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	public static extern bool IsEncrypted();
#else
	public static bool IsEncrypted()
	{
		return PcNative.IsEncrypted();
	}
#endif
}
