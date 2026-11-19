using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

public class FPSMemStoreWeb : FPSMemStoreAbstract
{
	public string WebStoreAddress;

	public override void Store(FPSMemRecord[] records)
	{
		foreach (FPSMemRecord record in records)
		{
			Store(record);
		}
	}

	private void Store(FPSMemRecord record)
	{
		CreateWebClient().UploadValuesAsync(new Uri(WebStoreAddress), "POST", CreateParams(record), Guid.NewGuid().ToString());
	}

	private WebClient CreateWebClient()
	{
		var result = new WebClient();
		return result;
	}

	private NameValueCollection CreateParams(FPSMemRecord record)
	{
		NameValueCollection nameValueCollection = new NameValueCollection();
		foreach (KeyValuePair<string, object> item in record.Data())
		{
			nameValueCollection.Add(item.Key, item.Value.ToString());
		}
		return nameValueCollection;
	}
}
