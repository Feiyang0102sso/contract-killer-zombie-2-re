using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class MemoryPrint : MonoBehaviour
{
	public delegate void GCCompletedDelegate();

	private static MemoryPrint _instance;

	private HashSet<int> _lastAllObjects;

	private int _lastBytes;

	public static MemoryPrint Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("MemoryPrint");
				_instance = gameObject.AddComponent<MemoryPrint>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return _instance;
		}
	}

	private void Log(string str)
	{
	}

	[Conditional("MEMORY_DEBUG")]
	public static void PrintDetails()
	{
		GC(() =>
		{
			Instance.PrintImpl(false);
		});
	}

	[Conditional("MEMORY_DEBUG")]
	public static void PrintAll()
	{
		GC(() =>
		{
			Instance.PrintImpl(true);
		});
	}

	[Conditional("MEMORY_DEBUG")]
	public static void PrintDiff(string diffName)
	{
		GC(() =>
		{
			Instance.PrintDiffImpl(diffName, false);
		});
	}

	[Conditional("MEMORY_DEBUG")]
	public static void PrintDiffAll(string diffName)
	{
		GC(() =>
		{
			Instance.PrintDiffImpl(diffName, true);
		});
	}

	[Conditional("MEMORY_DEBUG")]
	public static void PrintDiffNow(string diffName)
	{
		Instance.PrintDiffImpl(diffName, false);
	}

	[Conditional("MEMORY_DEBUG")]
	public static void PrintDiffAllNow(string diffName)
	{
		Instance.PrintDiffImpl(diffName, true);
	}

	private void PrintImpl(bool fullList)
	{
		Log("\n\n");
		Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
		Log("=======================");
		Log("====  Memory Print ====");
		Log("=======================");
		int currentMemoryBytes = NativeUtils.GetCurrentMemoryBytes();
		Log("Memory " + currentMemoryBytes / 1024 + " kb");
		Log("C# Heap " + System.GC.GetTotalMemory(true) / 1024 + " kb");
		Log("Profiler.usedHeapSize " + UnityEngine.Profiling.Profiler.usedHeapSize / 1024 + " kb");
		UnityEngine.Object[] collection = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		PrintObjectsTypesAndDetails(new HashSet<UnityEngine.Object>(collection), fullList);
	}

	private void PrintDiffImpl(string diffName, bool fullList)
	{
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		HashSet<UnityEngine.Object> hashSet = new HashSet<UnityEngine.Object>(array);
		if (_lastAllObjects != null)
		{
			hashSet.RemoveWhere((UnityEngine.Object o) => _lastAllObjects.Contains(o.GetInstanceID()));
		}
		_lastAllObjects = new HashSet<int>();
		foreach (UnityEngine.Object obj in array)
		{
			_lastAllObjects.Add(obj.GetInstanceID());
		}
		Log("\n\n");
		Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
		Log("=======================");
		Log("======  Diff Print ====");
		Log(diffName);
		Log("=======================");
		Log("=======================");
		int currentMemoryBytes = NativeUtils.GetCurrentMemoryBytes();
		Log("Memory " + currentMemoryBytes / 1024 + " kb");
		Log("Memory Diff " + (currentMemoryBytes - _lastBytes) / 1024 + " kb");
		_lastBytes = currentMemoryBytes;
		PrintObjectsTypesAndDetails(hashSet, fullList);
	}

	private void PrintObjectsTypesAndDetails(HashSet<UnityEngine.Object> all, bool fullList)
	{
		Log("All " + all.Count);
		IEnumerable<Texture> enumerable = new HashSet<Texture>(all.OfType<Texture>());
		Log(string.Format("Textures {0} ({1} kb)", enumerable.Count(), GetObjectsResourceSize(enumerable) / 1024));
		IEnumerable<AudioClip> enumerable2 = new HashSet<AudioClip>(all.OfType<AudioClip>());
		Log(string.Format("AudioClips {0} ({1} kb)", enumerable2.Count(), GetObjectsResourceSize(enumerable2) / 1024));
		IEnumerable<Mesh> enumerable3 = new HashSet<Mesh>(all.OfType<Mesh>());
		Log(string.Format("Meshes {0} ({1} kb)", enumerable3.Count(), GetObjectsResourceSize(enumerable3) / 1024));
		IEnumerable<Material> enumerable4 = new HashSet<Material>(all.OfType<Material>());
		Log(string.Format("Materials {0} ({1} kb)", enumerable4.Count(), GetObjectsResourceSize(enumerable4) / 1024));
		IEnumerable<AnimationClip> enumerable5 = new HashSet<AnimationClip>(all.OfType<AnimationClip>());
		Log(string.Format("AnimationClips {0} ({1} kb)", enumerable5.Count(), GetObjectsResourceSize(enumerable5) / 1024));
		IEnumerable<GameObject> enumerable6 = new HashSet<GameObject>(all.OfType<GameObject>());
		Log(string.Format("GameObjects {0}", enumerable6.Count()));
		IEnumerable<Component> enumerable7 = new HashSet<Component>(all.OfType<Component>());
		Log(string.Format("Components {0}", enumerable7.Count()));
		IEnumerable<Shader> enumerable8 = new HashSet<Shader>(all.OfType<Shader>());
		Log(string.Format("Shaders {0}", enumerable8.Count()));
		IEnumerable<TextAsset> enumerable9 = new HashSet<TextAsset>(all.OfType<TextAsset>());
		Log(string.Format("TextAssets {0}", enumerable9.Count()));
		all.ExceptWith(enumerable.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable2.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable3.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable4.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable5.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable6.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable7.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable8.OfType<UnityEngine.Object>());
		all.ExceptWith(enumerable9.OfType<UnityEngine.Object>());
		Log("Other " + all.Count());
		Log("\n");
		Log("=======================");
		Log("======  Tex Size ======");
		Log("=======================");
		int num = 0;
		foreach (UnityEngine.Object item in enumerable)
		{
			num += CalculateTextureSizeBytes(item as Texture);
		}
		Log("Textures size " + num / 1024 + " kb");
		Log("\n");
		Log("=======================");
		Log("=  Text Assets Size   =");
		Log("=======================");
		int num2 = 0;
		foreach (TextAsset item2 in enumerable9)
		{
			num2 += (item2.bytes == null ? 0 : item2.bytes.Length);
		}
		Log("Text Assets size " + num2 / 1024 + " kb");
		Log("\n");
		Log("=======================");
		Log("====   Mesh info ======");
		Log("=======================");
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		foreach (Mesh item3 in enumerable3)
		{
			num3 += item3.vertexCount;
			num4 += item3.uv.Length;
			num5 += item3.uv2.Length;
			num6 += item3.uv2.Length;
			num7 += item3.normals.Length;
			num8 += item3.tangents.Length;
			num9 += item3.colors.Length;
			num10 += item3.boneWeights.Length;
			num11 += item3.triangles.Length;
		}
		Log("Mesh vertices:" + num3.ToString());
		Log("Mesh uvs:" + num4.ToString());
		Log("Mesh uvs1:" + num5.ToString());
		Log("Mesh uvs2:" + num6.ToString());
		Log("Mesh normals:" + num7.ToString());
		Log("Mesh tangents:" + num8.ToString());
		Log("Mesh colors:" + num9.ToString());
		Log("Mesh bonesWeights:" + num10.ToString());
		Log("Mesh triangles indexes:" + num11.ToString());
		int num12 = (num3 * 3 + num4 * 2 + num6 * 2 + num7 * 3 + num8 * 3 + num9 * 4) * 4 + num11 * 4;
		Log("Mesh ~size:" + num12 / 1024 + " kb");
		if (fullList)
		{
			Log("\n");
			Log("=======================");
			Log("====== Full List ======");
			Log("=======================");
			PrintObjects(enumerable);
			PrintObjects(enumerable2);
			PrintObjects(enumerable3);
			PrintObjects(enumerable4);
			PrintObjects(enumerable5);
			PrintObjects(enumerable6);
			PrintObjects(enumerable7);
			PrintObjects(enumerable8);
			PrintObjects(enumerable9);
			Log("=======================");
			Log("=======================");
			Log("\n");
			Log("Other");
			Log("=======================");
			foreach (UnityEngine.Object item4 in all)
			{
				Log(string.Concat(item4.name, " (", item4.GetType(), ")"));
			}
			Log("=======================");
			Log("=======================");
		}
		Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
	}

	private void PrintObjects<T>(IEnumerable<T> objs) where T : UnityEngine.Object
	{
		Log(string.Empty);
		Log("=============================================");
		Log(typeof(T).Name);
		Log("=============================================");
		foreach (T current in objs)
		{
			string text = string.Format("{0} ({1}) ({2} kb)", current.name, current.GetType(), GetResourceSize(current) / 1024);
			Texture2D texture2D = current as Texture2D;
			if (texture2D != null)
			{
				text += string.Format(" {0}x{1} {2} ({3} kb)", texture2D.width, texture2D.height, texture2D.format, CalculateTextureSizeBytes(texture2D) / 1024);
			}
			Log(text);
		}
		Log("=============================================");
		Log("=============================================");
	}

	private int GetObjectsResourceSize<T>(IEnumerable<T> objs) where T : UnityEngine.Object
	{
		int num = 0;
		foreach (T obj in objs)
		{
			num += GetResourceSize(obj);
		}
		return num;
	}

	private int GetResourceSize<T>(T o) where T : UnityEngine.Object
	{
		return 0;
	}

	private int GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
			return 8;
		case TextureFormat.ARGB4444:
			return 16;
		case TextureFormat.RGB24:
			return 24;
		case TextureFormat.RGBA32:
			return 32;
		case TextureFormat.ARGB32:
			return 32;
		case TextureFormat.RGB565:
			return 16;
		case TextureFormat.DXT1:
			return 4;
		case TextureFormat.DXT5:
			return 8;
		case TextureFormat.ETC_RGB4:
			return 4;
		case TextureFormat.BGRA32:
			return 32;
		default:
			Log("!!! Unknown texture format " + format);
			return 0;
		}
	}

	private int CalculateTextureSizeBytes(Texture tTexture)
	{
		int num = tTexture.width;
		int num2 = tTexture.height;
		if (tTexture is Texture2D)
		{
			Texture2D texture2D = tTexture as Texture2D;
			int bitsPerPixel = GetBitsPerPixel(texture2D.format);
			int mipmapCount = texture2D.mipmapCount;
			int i = 1;
			int num3 = 0;
			for (; i <= mipmapCount; i++)
			{
				num3 += num * num2 * bitsPerPixel / 8;
				num /= 2;
				num2 /= 2;
			}
			return num3;
		}
		if (tTexture is Cubemap)
		{
			Cubemap cubemap = tTexture as Cubemap;
			int bitsPerPixel2 = GetBitsPerPixel(cubemap.format);
			return num * num2 * 6 * bitsPerPixel2 / 8;
		}
		return 0;
	}

	public static void GC()
	{
		Instance.StartGC(null);
	}

	public static void GC(GCCompletedDelegate onGCCompleted)
	{
		Instance.StartGC(onGCCompleted);
	}

	private void StartGC(GCCompletedDelegate onGCCompleted)
	{
		StartCoroutine(ProcessGarbageCollection(onGCCompleted));
	}

	private IEnumerator ProcessGarbageCollection(GCCompletedDelegate onGCCompleted)
	{
		AsyncOperation async = Resources.UnloadUnusedAssets();
		yield return async;
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		if (onGCCompleted != null)
		{
			onGCCompleted();
		}
	}
}
