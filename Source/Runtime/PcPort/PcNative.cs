// PC 桥接：替代 iOS 原生插件（DllImport("__Internal")）的 C# 实现。
// 游戏源码中的 extern 声明在 PC 上改为转调这里的同名函数，调用处不变。
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class PcNative
{
	/// <summary>钥匙串替代品的存放目录（每个 key 一个文件）。</summary>
	private const string SafeDataDirName = "SafeData";

	// ---------------- Localization ----------------

	/// <summary>
	/// 原生返回系统语言代码；返回 0 表示拿不到，游戏会退回用 Application.systemLanguage。
	/// PC 直接走游戏自带的退路。
	/// </summary>
	public static int Glu_Localization_GetLocale(int size, byte[] buf)
	{
		return 0;
	}

	// ---------------- StorageManager（iOS 钥匙串） ----------------

	/// <summary>读取 key 对应的数据；没有时返回空字符串（与 iOS 原生一致，调用方用 Equals(string.Empty) 判断）。</summary>
	public static IntPtr GetSafeDataForKey(string key)
	{
		string path = SafeDataPath(key);
		string data = string.Empty;
		if (File.Exists(path))
		{
			data = File.ReadAllText(path, Encoding.ASCII);
		}
		// 与原生一样返回 char*；字符串很短且读取次数少，不释放
		return Marshal.StringToHGlobalAnsi(data);
	}

	public static bool SetSafeDataForKey(string key, string data)
	{
		string path = SafeDataPath(key);
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		File.WriteAllText(path, data, Encoding.ASCII);
		return true;
	}

	/// <summary>
	/// 替代 StorageManager.PathToKey：iOS 要求存档在沙盒的 /Documents/、/Library/、/tmp/ 下，
	/// 以沙盒内的相对路径作键。PC 上把存档目录和临时目录映射成同样形式的键。
	/// </summary>
	public static string StoragePathToKey(string path)
	{
		string normalized = path.Replace('\\', '/');
		string persistent = Application.persistentDataPath.Replace('\\', '/');
		string temporary = Application.temporaryCachePath.Replace('\\', '/');
		if (normalized.StartsWith(persistent))
		{
			return "/Documents" + normalized.Substring(persistent.Length);
		}
		if (normalized.StartsWith(temporary))
		{
			return "/tmp" + normalized.Substring(temporary.Length);
		}
		return normalized;
	}

	/// <summary>key 是文件路径一类的字符串，转成十六进制作文件名，避免非法字符。</summary>
	private static string SafeDataPath(string key)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(key);
		StringBuilder name = new StringBuilder(bytes.Length * 2);
		foreach (byte value in bytes)
		{
			name.Append(value.ToString("x2"));
		}
		return Path.Combine(Path.Combine(Application.persistentDataPath, SafeDataDirName), name.ToString());
	}

	// ---------------- Integrity ----------------

	/// <summary>PC 视为未越狱：游戏会据此开启“可信”统计、放行内购等。</summary>
	public static bool IsJailbroken()
	{
		return false;
	}

	/// <summary>iOS 上检查主程序是否经 App Store 加密；PC 视为正版。</summary>
	public static bool IsEncrypted()
	{
		return true;
	}

	// ---------------- UnityBuildSystem / BuildInfo ----------------

	/// <summary>与原版 Info.plist 一致。</summary>
	private const string BundleId = "com.glu.ckzombies2";

	private const string BundleVersion = "2.0.2";

	public static void GetBundleId(StringBuilder sb, int size)
	{
		sb.Append(BundleId);
	}

	public static void GetBundleVersion(StringBuilder sb, int size)
	{
		sb.Append(BundleVersion);
	}

	// ---------------- 其余插件的空实现只打一次日志 ----------------

	private static readonly System.Collections.Generic.HashSet<string> LoggedStubs = new System.Collections.Generic.HashSet<string>();

	/// <summary>空实现被第一次调用时记一笔，便于看出游戏实际用到哪些原生接口。</summary>
	public static void Stub(string name)
	{
		if (LoggedStubs.Add(name))
		{
			UnityEngine.Debug.Log("[PcPort] native stub called: " + name);
		}
	}

	// ---------------- ServerTime ----------------

	/// <summary>单调时钟，毫秒（iOS 原生同样返回开机以来的单调毫秒数）。</summary>
	public static long ServerTime_getTicks()
	{
		return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
	}
}
