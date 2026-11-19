using System.Collections.Generic;

public class FPSMemRecord
{
	private Dictionary<string, object> _dict = new Dictionary<string, object>();

	public IEnumerable<KeyValuePair<string, object>> Data()
	{
		foreach (KeyValuePair<string, object> pair in _dict)
		{
			yield return pair;
		}
	}

	public void Add(string key, object value)
	{
		_dict.Add(key, value);
	}

	public void Clear()
	{
		_dict.Clear();
	}
}
