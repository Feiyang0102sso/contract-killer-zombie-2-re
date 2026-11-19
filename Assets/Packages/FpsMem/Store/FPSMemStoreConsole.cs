using System.Collections.Generic;
using UnityEngine;

public class FPSMemStoreConsole : FPSMemStoreAbstract
{
	public override void Store(FPSMemRecord[] records)
	{
		foreach (FPSMemRecord record in records)
		{
			Log(record);
		}
	}

	private void Log(FPSMemRecord record)
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, object> item in record.Data())
		{
			string text2 = text;
			text = text2 + item.Key + ": " + item.Value.ToString() + ", ";
		}
		Debug.Log(text.Substring(0, text.Length - 2));
	}
}
