using System.IO;
using UnityEngine;

public class ICloud
{
	public delegate void OnRemoteFileFindCompleteDelegate(string filename, string remotePath, bool success);

	public delegate void OnSaveCompleteDelegate(string filename, bool success);

	public delegate void OnLoadCompleteDelegate(string filename, byte[] data);

	private const float MAX_LOAD_TIME = 30f;

	private OnSaveCompleteDelegate _onSaveCompleteDelegate;

	private OnLoadCompleteDelegate _onLoadCompleteDelegate;

	private OnRemoteFileFindCompleteDelegate _onRemoveFileFindCompleteDelegate;

	private string _localFindFilename = string.Empty;

	private string _localSaveFilename = string.Empty;

	private bool _isSynchronizationInProgress;

	private bool IsNetworkAvailable
	{
		get
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}
	}

	public bool IsSupported { get; private set; }

	public bool IsAvailable { get; private set; }

	public ICloud()
	{
		CheckICloudAvailableAndSupported();
	}

	private void CheckICloudAvailableAndSupported()
	{
		string[] array = SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.LastIndexOf(' ') + 1).Split('.');
		int result = 4;
		int.TryParse(array[0], out result);
		if (result < 5)
		{
			IsAvailable = false;
			IsSupported = false;
		}
		else
		{
			FileManager.CheckCloudStorageAvailability();
			IsAvailable = FileManager.CloudStorageAvailable;
			IsSupported = true;
		}
		Debug.Log("CheckICloud(): " + result + " CloudStorageAvailable: " + FileManager.CloudStorageAvailable);
	}

	private void OnLocalFileLoadComplete(FileData result)
	{
		bool flag = result.Data != null;
		Debug.Log("ICloud.OnLocalFileLoadComplete " + flag);
		if (flag)
		{
			FileManager.SaveFile(GetCloudPath(_localSaveFilename), result.Data, OnSaveComplete);
		}
		else
		{
			OnSaveComplete(result);
		}
	}

	private void OnSaveComplete(FileData result)
	{
		bool flag = result.Data != null;
		Debug.Log("ICloud.OnSaveComplete " + flag);
		if (_onSaveCompleteDelegate != null)
		{
			_onSaveCompleteDelegate(_localSaveFilename, flag);
		}
		_onSaveCompleteDelegate = null;
		_isSynchronizationInProgress = false;
	}

	private string GetAutoSavePath(string filename)
	{
		return Path.Combine(StorageManager.PersistentDataPath, filename);
	}

	private string GetCloudPath(string filename)
	{
		string autoSavePath = GetAutoSavePath(filename);
		var result = FileManager.ConvertLocalPathToCloudPath(autoSavePath);
		return result;
	}

	private void OnFindRemoteFileComplete(FileData result)
	{
		string remotePath = string.Empty;
		if (result.Exists)
		{
			remotePath = result.Path;
		}
		if (_onRemoveFileFindCompleteDelegate != null)
		{
			_onRemoveFileFindCompleteDelegate(_localFindFilename, remotePath, result.Exists);
		}
	}

	private void OnLoadComplete(FileData result)
	{
		Debug.Log("ICloud.OnSaveFileFindComplete " + (result != null && result.Data != null));
		if (_onLoadCompleteDelegate != null)
		{
			_onLoadCompleteDelegate(_localSaveFilename, !(result == null) ? result.Data : null);
		}
		_isSynchronizationInProgress = false;
		_onLoadCompleteDelegate = null;
	}

	private void OnLoadTimerFinished()
	{
		OnLoadComplete(null);
	}

	private void OnSaveFileFindComplete(FileData result)
	{
		Debug.Log("ICloud.OnSaveFileFindComplete " + result.Exists);
		if (result.Exists)
		{
			FileManager.LoadFile(result.Path, OnLoadComplete);
		}
		else
		{
			OnLoadComplete(result);
		}
	}

	public void Load(string filename, OnLoadCompleteDelegate callback)
	{
		if (!IsSupported || !IsAvailable || _isSynchronizationInProgress || !IsNetworkAvailable)
		{
			if (callback != null)
			{
				callback(filename, null);
			}
			return;
		}
		Debug.Log("ICloud.Load ");
		_isSynchronizationInProgress = true;
		_onLoadCompleteDelegate = callback;
		_localSaveFilename = filename;
		new GameObject("TempTimer").AddComponent<MiniScript>().Charge(30f, null, null, null, (MiniScript x) =>
		{
			OnLoadTimerFinished();
		});
		FileManager.FindFilePathInCloud(_localSaveFilename, OnSaveFileFindComplete);
	}

	public void Save(string filename, OnSaveCompleteDelegate callback)
	{
		if (!IsSupported || !IsAvailable || _isSynchronizationInProgress || !IsNetworkAvailable)
		{
			if (callback != null)
			{
				callback(filename, false);
			}
			return;
		}
		Debug.Log("ICloud.Save - start");
		_isSynchronizationInProgress = true;
		_onSaveCompleteDelegate = callback;
		_localSaveFilename = filename;
		string autoSavePath = GetAutoSavePath(_localSaveFilename);
		FileManager.LoadFile(autoSavePath, OnLocalFileLoadComplete);
	}

	public void CheckForRemoteFileExists(string filename, OnRemoteFileFindCompleteDelegate callback)
	{
		if (!IsSupported || !IsAvailable || !IsNetworkAvailable)
		{
			if (callback != null)
			{
				callback(filename, null, false);
			}
			return;
		}
		_localFindFilename = filename;
		_onRemoveFileFindCompleteDelegate = callback;
		FileManager.FindFilePathInCloud(filename, OnFindRemoteFileComplete);
	}
}
