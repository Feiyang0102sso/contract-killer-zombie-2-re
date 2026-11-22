// Offline: WWW with the network cut off, on every platform.
// Defined in the global namespace: when C# looks up a type name, a global type wins over the same-named type brought in
// by using UnityEngine, so every WWW in the game code uses this class without source changes.
// - http:// and https:// requests end at once with an error (the port never contacts Glu's servers), and the game
//   takes its own existing failure path;
// - file:// and other local requests go to UnityEngine.WWW as usual (reading the local index and asset bundles).
#pragma warning disable 618
using System;
using System.Collections.Generic;
using UnityEngine;

public class WWW : CustomYieldInstruction, IDisposable
{
	/// <summary>Error text returned to the game for blocked requests.</summary>
	public const string OfflineError = "Offline: network disabled";

	private readonly string _url;

	/// <summary>The real WWW of a local request; null for a network request.</summary>
	private readonly UnityEngine.WWW _inner;

	public WWW(string url)
	{
		_url = url;
		if (IsNetworkUrl(url))
		{
			Debug.Log("[OfflineWWW] blocked: " + url);
			return;
		}
		Debug.Log("[OfflineWWW] local: " + url);
		_inner = new UnityEngine.WWW(url);
	}

	/// <summary>The original downloads asset bundles with caching; only local files are read here, so no cache: a plain request.</summary>
	public static WWW LoadFromCacheOrDownload(string url, int version)
	{
		return new WWW(url);
	}

	private static bool IsNetworkUrl(string url)
	{
		if (url == null)
		{
			return false;
		}
		string lower = url.ToLowerInvariant();
		return lower.StartsWith("http://") || lower.StartsWith("https://");
	}

	/// <summary>For yield return: keeps waiting until the request is done.</summary>
	public override bool keepWaiting
	{
		get
		{
			return !isDone;
		}
	}

	public string url
	{
		get
		{
			return _url;
		}
	}

	public bool isDone
	{
		get
		{
			if (_inner == null)
			{
				return true;
			}
			return _inner.isDone;
		}
	}

	public string error
	{
		get
		{
			if (_inner == null)
			{
				return OfflineError;
			}
			string innerError = _inner.error;
			if (innerError != null && _inner.isDone)
			{
				Debug.LogWarning("[OfflineWWW] local failed: " + _url + " -> " + innerError);
			}
			return innerError;
		}
	}

	public float progress
	{
		get
		{
			if (_inner == null)
			{
				return 1f;
			}
			return _inner.progress;
		}
	}

	public float uploadProgress
	{
		get
		{
			if (_inner == null)
			{
				return 1f;
			}
			return _inner.uploadProgress;
		}
	}

	public byte[] bytes
	{
		get
		{
			if (_inner == null)
			{
				return new byte[0];
			}
			return _inner.bytes;
		}
	}

	public string text
	{
		get
		{
			if (_inner == null)
			{
				return string.Empty;
			}
			return _inner.text;
		}
	}

	public Dictionary<string, string> responseHeaders
	{
		get
		{
			if (_inner == null)
			{
				return new Dictionary<string, string>();
			}
			return _inner.responseHeaders;
		}
	}

	public AssetBundle assetBundle
	{
		get
		{
			if (_inner == null)
			{
				return null;
			}
			return _inner.assetBundle;
		}
	}

	public void Dispose()
	{
		if (_inner != null)
		{
			_inner.Dispose();
		}
	}
}
