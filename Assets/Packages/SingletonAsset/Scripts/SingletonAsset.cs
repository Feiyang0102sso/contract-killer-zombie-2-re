using System;
using UnityEngine;

public class SingletonAsset<T> where T : ScriptableObject
{
	private const string assetPath = "Assets/Resources/SingletonAssets/";

	private static T instance = null;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				Type typeFromHandle = typeof(T);
				string path = "SingletonAssets/" + typeFromHandle.Name;
				instance = Resources.Load(path) as T;
			}
			return instance;
		}
	}
}
