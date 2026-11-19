using System.Collections.Generic;
using UnityEngine;

public class GameObjectsCache : MonoBehaviourAndLog
{
	public class PrefabsCache
	{
		private class Pool
		{
			private Stack<GameObject> _stack;

			private int _size;

			public Pool(int size)
			{
				_stack = new Stack<GameObject>(size);
				_size = size;
			}

			public bool IsEmpty()
			{
				return _stack.Count == 0;
			}

			public GameObject Pop()
			{
				if (IsEmpty())
				{
					return null;
				}
				return _stack.Pop();
			}

			public bool Push(GameObject go)
			{
				if (_stack.Count >= _size)
				{
					return false;
				}
				_stack.Push(go);
				return true;
			}
		}

		private Transform _vault;

		private Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

		private Dictionary<GameObject, Pool> _instancePoolMap = new Dictionary<GameObject, Pool>();

		public PrefabsCache(Transform vault)
		{
			_vault = vault;
		}

		public void PreLoad(GameObject prefab, int poolSize, int count)
		{
			GameObject[] array = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = Instantiate(prefab, poolSize);
			}
			for (int j = 0; j < count; j++)
			{
				Destroy(array[j]);
			}
		}

		public GameObject Instantiate(GameObject prefab, int poolSize)
		{
			Pool value = null;
			_pools.TryGetValue(prefab, out value);
			if (value == null)
			{
				value = new Pool(poolSize);
				_pools.Add(prefab, value);
			}
			GameObject gameObject = value.Pop();
			if (gameObject == null)
			{
				gameObject = Object.Instantiate(prefab) as GameObject;
			}
			else
			{
				gameObject.SetActiveRecursively(true);
				gameObject.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
			}
			gameObject.transform.parent = null;
			_instancePoolMap.Add(gameObject, value);
			return gameObject;
		}

		public void Destroy(GameObject go)
		{
			Pool pool = _instancePoolMap[go];
			_instancePoolMap.Remove(go);
			if (!pool.Push(go))
			{
				Object.Destroy(go);
				return;
			}
			go.transform.parent = _vault;
			go.SetActiveRecursively(false);
		}

		public void Clear()
		{
			foreach (Pool value in _pools.Values)
			{
				while (!value.IsEmpty())
				{
					GameObject gameObject = value.Pop();
					if (gameObject != null)
					{
						Object.Destroy(gameObject);
					}
				}
			}
			_pools.Clear();
		}
	}

	public class ResourcesCache
	{
		private Dictionary<string, GameObject> _cache = new Dictionary<string, GameObject>();

		public GameObject Create(string name)
		{
			return Object.Instantiate(Load(name)) as GameObject;
		}

		public GameObject Load(string name)
		{
			GameObject value;
			_cache.TryGetValue(name, out value);
			if (value == null)
			{
				value = Resources.Load(name) as GameObject;
				_cache[name] = value;
			}
			return value;
		}

		public void Clear()
		{
			_cache.Clear();
		}
	}

	public class StringKeyCache
	{
		private Dictionary<string, GameObject> _objectCache = new Dictionary<string, GameObject>();

		private Transform _vault;

		public StringKeyCache(Transform vault)
		{
			_vault = vault;
		}

		public GameObject Load(string key, string path)
		{
			GameObject value;
			_objectCache.TryGetValue(key, out value);
			if (value == null)
			{
				value = Util.CreateGameObject(path);
				if (value == null)
				{
					value = Util.CreateGameObject(path, false);
				}
				_objectCache[key] = value;
			}
			value.SetActiveRecursively(true);
			return value;
		}

		public void Release(string key)
		{
			GameObject value;
			_objectCache.TryGetValue(key, out value);
			if (value != null)
			{
				value.transform.parent = _vault;
				value.transform.localPosition = Vector3.zero;
				value.SetActiveRecursively(false);
			}
		}

		public void Clear()
		{
			foreach (GameObject value in _objectCache.Values)
			{
				if (value != null)
				{
					Object.Destroy(value);
				}
			}
			_objectCache.Clear();
		}
	}

	private StringKeyCache _stringsCache;

	private PrefabsCache _prefabsCache;

	private ResourcesCache _resourcesCache;

	public StringKeyCache PerStringCache
	{
		get
		{
			return _stringsCache;
		}
	}

	public PrefabsCache PerPrefabCache
	{
		get
		{
			return _prefabsCache;
		}
	}

	public ResourcesCache PerResourceCache
	{
		get
		{
			return _resourcesCache;
		}
	}

	private void Awake()
	{
		LogTo("Other.GameObjectsPool");
		_stringsCache = new StringKeyCache(transform);
		_prefabsCache = new PrefabsCache(transform);
		_resourcesCache = new ResourcesCache();
		transform.position = new Vector3(1000f, 0f, 1000f);
	}
}
