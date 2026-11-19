using System.IO;
using UnityEngine;

public class TestICloudScript : MonoBehaviour
{
	public delegate void OnRemoteFileFindCompleteDelegate(string filename, string remotePath, bool success);

	public delegate void OnSaveCompleteDelegate(string filename, bool success);

	public delegate void OnLoadCompleteDelegate(string filename, byte[] data);

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

	private void Awake()
	{
		CheckICloudAvailableAndSupported();
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
		return Path.Combine(Application.persistentDataPath, filename);
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
		Debug.Log("ICloud.OnSaveFileFindComplete " + (result.Data != null));
		if (_onLoadCompleteDelegate != null)
		{
			_onLoadCompleteDelegate(_localSaveFilename, result.Data);
		}
		_isSynchronizationInProgress = false;
		_onLoadCompleteDelegate = null;
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
		FileManager.FindFilePathInCloud(filename, OnFindRemoteFileComplete);
		_localFindFilename = filename;
		_onRemoveFileFindCompleteDelegate = callback;
	}

	public void OnSaveCompl(string filename, bool success)
	{
		Debug.Log("SaveComplete " + success);
	}

	public void OnLoadCompl(string filename, byte[] data)
	{
		Debug.Log("LoadComplete " + (data != null));
	}

	private void OnGUI()
	{
		if (IsSupported && IsAvailable)
		{
			if (GUILayout.Button("Load111", GUILayout.Height(256f)))
			{
				Load("save", OnLoadCompl);
			}
			if (GUILayout.Button("Save111", GUILayout.Height(256f)))
			{
				Save("save", OnSaveCompl);
			}
		}
	}
}
