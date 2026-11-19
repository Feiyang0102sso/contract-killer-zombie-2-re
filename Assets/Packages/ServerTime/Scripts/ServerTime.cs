using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class ServerTime : MonoBehaviour
{
	public interface SNTPClientInterface
	{
		DateTime GetServerTime();
	}

	public struct ServerTimeData
	{
		public DateTime _trustedUTCTime;

		public DateTime _trustedDeviceUTCTime;

		public long _trustedTimeTicks;
	}

	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.ServerTime");
		}
	}

	private const string GAME_OBJECT_NAME = "ServerTimeObject";

	private const float COROUTINE_SLEEP_INTERVAL = 1f;

	private const int _maxValidDeltaMillis = 5000;

	private const bool _useTickCount = true;

	private const string DLLIMPORT_NAME = "__Internal";

	private static ServerTime _instance = null;

	private static bool _useServerTime;

	private static bool _enablePending = true;

	private Thread _pendingThread;

	private bool _pending;

	private bool _requestCoroutineIsExecuting;

	private bool _repeatRequest;

	private static string _localSaveFilePath = Path.Combine(Application.temporaryCachePath, "servertimedata.dat");

	private ServerTimeData _data;

	private DateTime _sheduledTimeToSendNewRequest;

	private SNTPClientInterface _SNTPClient;

	public static void Init(bool useServerTime)
	{
		if (_instance == null)
		{
			Init(useServerTime, new ServerTimeDefaultSNTPClient());
		}
	}

	public static void Init(bool useServerTime, SNTPClientInterface SNTPClient)
	{
		if (_instance == null)
		{
			_useServerTime = useServerTime;
			GameObject gameObject = new GameObject("ServerTimeObject");
			_instance = gameObject.AddComponent<ServerTime>();
			_instance._SNTPClient = SNTPClient;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static void EnablePending(bool enable)
	{
		if (!(_enablePending != enable))
		{
			return;
		}
		_enablePending = enable;
		if (enable && _instance._data._trustedUTCTime == DateTime.MinValue)
		{
			_instance.StartPending();
		}
	}

	public static DateTime GetCurrentLocalTime()
	{
		if (_instance._data._trustedUTCTime != DateTime.MinValue)
		{
			return GetCurrentUTCTime() + (DateTime.Now - DateTime.UtcNow);
		}
		return DateTime.MinValue;
	}

	public static DateTime GetCurrentUTCTime()
	{
		if (_instance._data._trustedUTCTime != DateTime.MinValue)
		{
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(GetTicksCountInMillis() - _instance._data._trustedTimeTicks);
			return _instance._data._trustedUTCTime + timeSpan;
		}
		return DateTime.MinValue;
	}

	public static void ResetAndRequestServerTime()
	{
		_instance._data._trustedUTCTime = DateTime.MinValue;
		if (_enablePending)
		{
			_instance._data._trustedUTCTime = DateTime.MinValue;
			_instance.StartPending();
		}
		else
		{
		}
	}

	private void Start()
	{
		_data._trustedUTCTime = DateTime.MinValue;
		TryToLoadData();
		if (_data._trustedUTCTime != DateTime.MinValue)
		{
			ValidateAndSyncData();
		}
		if (_data._trustedUTCTime == DateTime.MinValue && _enablePending)
		{
			StartPending();
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause && _instance != null)
		{
			if (_data._trustedUTCTime != DateTime.MinValue)
			{
				ValidateAndSyncData();
			}
			if (_data._trustedUTCTime == DateTime.MinValue && _enablePending)
			{
				StartPending();
			}
		}
	}

	private void Update()
	{
		if (_repeatRequest && !_requestCoroutineIsExecuting)
		{
			_repeatRequest = false;
			SheduleNewServerRequest();
		}
	}

	private void TimeRecievedCallback(DateTime trueTime)
	{
		_pendingThread = null;
		if (trueTime == DateTime.MinValue)
		{
			if (_enablePending)
			{
				SheduleNewServerRequest();
			}
			else
			{
				_pending = false;
			}
		}
		else
		{
			_pending = false;
			_data._trustedUTCTime = trueTime;
			SyncTimeData();
			SaveData();
		}
	}

	private IEnumerator RequestCoroutine()
	{
		_requestCoroutineIsExecuting = true;
		RequestTrueUTCTime(TimeRecievedCallback);
		while (_pendingThread != null)
		{
			yield return new WaitForSeconds(1f);
			if (_pendingThread != null)
			{
			}
		}
		_requestCoroutineIsExecuting = false;
	}

	private void SheduleNewServerRequest()
	{
		if (_requestCoroutineIsExecuting)
		{
			_repeatRequest = true;
		}
		else
		{
			StartCoroutine(RequestCoroutine());
		}
	}

	private void StartPending()
	{
		if (false || _data._trustedUTCTime == DateTime.MinValue)
		{
			if (!_pending)
			{
				_pending = true;
				SheduleNewServerRequest();
			}
			else
			{
			}
		}
		else
		{
		}
	}

	private static void RequestTrueUTCTime(Action<DateTime> callback)
	{
		if (_useServerTime)
		{
			_instance._pendingThread = new Thread(GetTimeStart);
			_instance._pendingThread.Start(callback);
		}
		else
		{
			callback(DateTime.UtcNow);
		}
	}

	private static void GetTimeStart(object obj)
	{
		DateTime serverTime = _instance._SNTPClient.GetServerTime();
		Action<DateTime> action = (Action<DateTime>)obj;
		action(serverTime);
	}

	private static long GetTicksCountInMillis()
	{
		return ServerTime_getTicks();
	}

	private void SaveData()
	{
		try
		{
			StorageManager.WriteXmlToLocation(_localSaveFilePath, _localSaveFilePath, _data);
		}
		catch (Exception e)
		{
		}
	}

	private void TryToLoadData()
	{
		try
		{
			_data = (ServerTimeData)StorageManager.ReadXmlFromLocation(_localSaveFilePath, typeof(ServerTimeData));
		}
		catch (Exception)
		{
			_data._trustedUTCTime = DateTime.MinValue;
		}
	}

	private void ValidateAndSyncData()
	{
		TimeSpan timeSpan = DateTime.UtcNow - _data._trustedDeviceUTCTime;
		TimeSpan timeSpan2 = TimeSpan.FromMilliseconds(GetTicksCountInMillis() - _data._trustedTimeTicks);
		long value = (long)(timeSpan - timeSpan2).TotalMilliseconds;
		if (Math.Abs(value) > 5000)
		{
			_data._trustedUTCTime = DateTime.MinValue;
		}
		else
		{
			_data._trustedUTCTime += timeSpan;
			SyncTimeData();
			SaveData();
		}
	}

	private void SyncTimeData()
	{
		_data._trustedTimeTicks = GetTicksCountInMillis();
		_data._trustedDeviceUTCTime = DateTime.UtcNow;
	}

	[DllImport("__Internal")]
	private static extern long ServerTime_getTicks();
}
