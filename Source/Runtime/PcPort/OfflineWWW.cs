// PC 桥接：断网版的 WWW。
// 定义在全局命名空间：C# 查找类型名时，全局命名空间里的类型优先于 using UnityEngine 引入的同名类型，
// 所以游戏代码里所有的 WWW 都会自动用这个类，源码不用改。
// - http:// 和 https:// 请求立刻以错误结束（PC 版不访问 Glu 的服务器），游戏走自己原有的失败分支；
// - file:// 等本地请求照常交给 UnityEngine.WWW（读取本地索引和资源包）。
#pragma warning disable 618
using System;
using System.Collections.Generic;
using UnityEngine;

public class WWW : CustomYieldInstruction, IDisposable
{
	/// <summary>断网时返回给游戏的错误信息。</summary>
	public const string OfflineError = "PcPort: network disabled";

	private readonly string _url;

	/// <summary>本地请求的真实 WWW；联网请求为 null。</summary>
	private readonly UnityEngine.WWW _inner;

	public WWW(string url)
	{
		_url = url;
		if (IsNetworkUrl(url))
		{
			Debug.Log("[PcPort] WWW blocked (offline): " + url);
			return;
		}
		Debug.Log("[PcPort] WWW local: " + url);
		_inner = new UnityEngine.WWW(url);
	}

	/// <summary>原版用于带缓存地下载资源包；PC 只读本地文件，不需要缓存，按普通请求处理。</summary>
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

	/// <summary>供 yield return 使用：请求完成前一直等待。</summary>
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
				Debug.LogWarning("[PcPort] WWW local failed: " + _url + " -> " + innerError);
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
