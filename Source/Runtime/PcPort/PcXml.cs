// PC 桥接：XML 读写的公共辅助方法（供可空字段的 XML 代理属性使用）。
public static class PcXml
{
	/// <summary>宽松解析布尔值：true/false 不区分大小写，也接受 1/0。</summary>
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
