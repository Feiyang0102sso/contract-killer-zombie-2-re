using System;
using System.Collections;
using UnityEngine;

public class DailyBonus : MonoBehaviour
{
	public struct DailyBonusData
	{
		public int ConsequtiveDays;

		public DateTime LastSavedLocalTime;

		public DateTime LastRewardedLocalTime;
	}

	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.DailyBonus");
		}
	}

	private const string _gameObjectName = "DailyBonusObject";

	private const string _saveFileName = "dailybonusdata.dat";

	private const float _updateCoroutineSleepInterval = 5f;

	private static DailyBonus _instance = null;

	private DailyBonusData _data;

	private static DailyBonus instance
	{
		get
		{
			return _instance;
		}
	}

	public static void Init()
	{
		if (_instance == null)
		{
			GameObject gameObject = new GameObject("DailyBonusObject");
			_instance = gameObject.AddComponent<DailyBonus>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static int GetUnclaimedConsecutiveDays()
	{
		if ((_instance._data.LastSavedLocalTime.Date - _instance._data.LastRewardedLocalTime).Days > 0)
		{
			return _instance._data.ConsequtiveDays;
		}
		return 0;
	}

	public static int GetTotalConsecutiveDays()
	{
		return _instance._data.ConsequtiveDays;
	}

	public static void Acknowledge()
	{
		_instance._data.LastRewardedLocalTime = _instance._data.LastSavedLocalTime.Date;
		_instance.SaveToFile();
	}

	public static DateTime GetLastRewardedLocalTime()
	{
		return _instance._data.LastRewardedLocalTime;
	}

	private void Awake()
	{
		try
		{
			_data = (DailyBonusData)StorageManager.ReadXml("dailybonusdata.dat", typeof(DailyBonusData));
		}
		catch (Exception e)
		{
			_data.ConsequtiveDays = 0;
			_data.LastSavedLocalTime = DateTime.MinValue;
			_data.LastRewardedLocalTime = DateTime.MinValue;
		}
	}

	private void Start()
	{
		StartCoroutine(UpdateCoroutine());
	}

	private IEnumerator UpdateCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(5f);
			DateTime localTime = ServerTime.GetCurrentLocalTime();
			if (localTime != DateTime.MinValue)
			{
				bool needToSave = false;
				if (_data.LastSavedLocalTime == DateTime.MinValue)
				{
					_data.LastSavedLocalTime = localTime;
					_data.ConsequtiveDays = 1;
					needToSave = true;
				}
				TimeSpan delta = localTime.Date - _data.LastSavedLocalTime.Date;
				if (delta.Days > 0)
				{
					_data.LastSavedLocalTime = localTime;
					if (delta.Days == 1)
					{
						if ((localTime.Date - _data.LastRewardedLocalTime.Date).Days <= 1)
						{
							_data.ConsequtiveDays++;
						}
						else
						{
							_data.ConsequtiveDays = 1;
						}
					}
					else
					{
						_data.ConsequtiveDays = 1;
					}
					needToSave = true;
				}
				if (needToSave)
				{
					SaveToFile();
				}
			}
		}
	}

	private void SaveToFile()
	{
		try
		{
			StorageManager.WriteXml("dailybonusdata.dat", _data);
		}
		catch (Exception)
		{
		}
	}
}
