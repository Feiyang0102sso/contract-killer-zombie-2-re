using System.Collections.Generic;
using UnityEngine;

public class FPSMemCollector : MonoBehaviour
{
	public enum CollectorType
	{
		PER_FRAME,
		PER_SECOND
	}

	public CollectorType CollectorTypeValue = CollectorType.PER_SECOND;

	public int CollectPeriod = 5;

	public int CacheSize = 1;

	public List<RuntimePlatform> Platforms = new List<RuntimePlatform>
	{
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.OSXEditor,
		RuntimePlatform.IPhonePlayer,
		RuntimePlatform.Android
	};

	private FPSMemDataAbstract[] _datas;

	private FPSMemStoreAbstract[] _stores;

	private FPSMemRecord[] _records;

	private int _currentCachePosition;

	private int _framesLeftToCollect;

	private float _lastCollectTime;

	private void Awake()
	{
		FindComponents();
		if (!CheckPlatform())
		{
			RemoveComponents();
			return;
		}
		InitCache();
		_framesLeftToCollect = CollectPeriod;
		_lastCollectTime = -1f;
	}

	private bool CheckPlatform()
	{
		return Platforms.Contains(Application.platform);
	}

	private void RemoveComponents()
	{
		Object.Destroy(this);
		foreach (FPSMemDataAbstract obj in _datas)
		{
			Object.Destroy(obj);
		}
		foreach (FPSMemStoreAbstract obj2 in _stores)
		{
			Object.Destroy(obj2);
		}
	}

	private void FindComponents()
	{
		_datas = GetComponents<FPSMemDataAbstract>();
		if (_datas == null || _datas.Length <= 0)
		{
			Debug.LogError("FpsMemCollector requires FPSMemData component on same GameObject");
		}
		_stores = GetComponents<FPSMemStoreAbstract>();
		if (_stores == null || _stores.Length <= 0)
		{
			Debug.LogError("FpsMemCollector requires FPSMemStore component on same GameObject");
		}
	}

	private void InitCache()
	{
		_records = new FPSMemRecord[CacheSize];
		for (int i = 0; i < _records.Length; i++)
		{
			_records[i] = new FPSMemRecord();
		}
		_currentCachePosition = 0;
	}

	private void Update()
	{
		switch (CollectorTypeValue)
		{
		case CollectorType.PER_FRAME:
			UpdateFrames();
			break;
		case CollectorType.PER_SECOND:
			UpdateSeconds();
			break;
		}
	}

	private void UpdateFrames()
	{
		_framesLeftToCollect--;
		if (_framesLeftToCollect <= 0)
		{
			Collect();
			_framesLeftToCollect = CollectPeriod;
		}
	}

	private void UpdateSeconds()
	{
		if (_lastCollectTime < 0f)
		{
			_lastCollectTime = Time.realtimeSinceStartup;
		}
		if (Time.realtimeSinceStartup - _lastCollectTime >= (float)CollectPeriod)
		{
			Collect();
			_lastCollectTime = Time.realtimeSinceStartup;
		}
	}

	private void OnDestroy()
	{
	}

	private void Collect()
	{
		_records[_currentCachePosition].Clear();
		foreach (FPSMemDataAbstract fPSMemDataAbstract in _datas)
		{
			fPSMemDataAbstract.Collect(_records[_currentCachePosition]);
		}
		_currentCachePosition++;
		if (_currentCachePosition >= CacheSize)
		{
			_currentCachePosition = 0;
			Store();
		}
	}

	private void Store()
	{
		foreach (FPSMemStoreAbstract fPSMemStoreAbstract in _stores)
		{
			fPSMemStoreAbstract.Store(_records);
		}
	}
}
