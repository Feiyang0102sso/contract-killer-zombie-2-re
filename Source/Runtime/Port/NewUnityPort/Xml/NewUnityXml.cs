// New Unity port: shared XML read/write helpers (used by the XML proxy properties of nullable fields).
public static class NewUnityXml
{
	/// <summary>Lenient bool parse: true/false in any case, also 1/0.</summary>
	public static bool ParseBool(string value)
	{
		string trimmed = value.Trim();
		if (trimmed == "1")
		{
			return true;
		}
		if (trimmed == "0")
		{
			return false;
		}
		return bool.Parse(trimmed);
	}
}
