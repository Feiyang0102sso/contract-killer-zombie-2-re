using System;
using System.Collections;
using UnityEngine;

public class TextureCleaner : MonoBehaviour
{
	private const int CHECKER_SIZE = 4;

	public static void Action()
	{
		GameObject gameObject = new GameObject("TextureCleaner");
		gameObject.AddComponent<TextureCleaner>();
	}

	private void Start()
	{
		Texture2D texture2D = new Texture2D(4, 4);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				texture2D.SetPixel(j, i, ((j + i) & 1) != 0 ? Color.white : Color.black);
			}
		}
		texture2D.Apply();
		Renderer[] array = UnityEngine.Object.FindObjectsOfType(typeof(Renderer)) as Renderer[];
		foreach (Renderer renderer in array)
		{
			foreach (Material material in renderer.materials)
			{
				material.mainTexture = texture2D;
				if (material.HasProperty("_BumpMap"))
				{
					material.SetTexture("_BumpMap", texture2D);
				}
				if (material.HasProperty("_Illum"))
				{
					material.SetTexture("_Illum", texture2D);
				}
			}
		}
		StartCoroutine(ClearMem());
	}

	private IEnumerator ClearMem()
	{
		Debug.Log("*** startign resource unload");
		AsyncOperation async = Resources.UnloadUnusedAssets();
		yield return async;
		Debug.Log("*** resource complete");
		Debug.Log("*** ыефкештп synchronous GC");
		GC.Collect();
		GC.WaitForPendingFinalizers();
		Debug.Log("*** synchronous GC completed");
		UnityEngine.Object.Destroy(gameObject);
	}
}
