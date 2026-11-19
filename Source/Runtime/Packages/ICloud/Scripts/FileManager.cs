using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NSFilemanager;
using UnityEngine;

public static class FileManager
{
	private delegate void FindFilePathInCloudDelegate(string fileName, string cloudPath);

	private delegate void SaveFileDelegate(string path, bool success);

	private delegate void LoadFileDelegate(string path, IntPtr data, uint length);

	private const int kMaxFilePathLength = 256;

	private static Dictionary<string, FileData> findFilePathInCloudActions;

	private static Dictionary<string, FileData> saveFileActions;

	private static Dictionary<string, FileData> loadFileActions;

	private static bool useNativeFileIO;

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool _GetCloudContainerDirectoryPath(string containerID, StringBuilder buffer, uint bufferLength);
#else
	private static bool _GetCloudContainerDirectoryPath(string containerID, StringBuilder buffer, uint bufferLength)
	{
		PcNative.Stub("_GetCloudContainerDirectoryPath");
		return default(bool);
	}
#endif

	static FileManager()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string[] array = SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.LastIndexOf(' ') + 1).Split('.');
			int result = 4;
			if (array != null && array.Length > 0)
			{
				int.TryParse(array[0], out result);
			}
			useNativeFileIO = result >= 5;
			Debug.Log("iOS Version = " + result.ToString());
		}
		if (useNativeFileIO)
		{
			findFilePathInCloudActions = new Dictionary<string, FileData>();
			saveFileActions = new Dictionary<string, FileData>();
			loadFileActions = new Dictionary<string, FileData>();
		}
		CheckCloudStorageAvailability();
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool _ConvertLocalPathToCloudPath(string containerID, string localPath, StringBuilder buffer, uint bufferLength);
#else
	private static bool _ConvertLocalPathToCloudPath(string containerID, string localPath, StringBuilder buffer, uint bufferLength)
	{
		PcNative.Stub("_ConvertLocalPathToCloudPath");
		return default(bool);
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _FindFilePathInCloud(string containerID, string fileName, FindFilePathInCloudDelegate callback);
#else
	private static void _FindFilePathInCloud(string containerID, string fileName, FindFilePathInCloudDelegate callback)
	{
		PcNative.Stub("_FindFilePathInCloud");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _SaveDataToFile(string path, byte[] data, uint length, SaveFileDelegate callback);
#else
	private static void _SaveDataToFile(string path, byte[] data, uint length, SaveFileDelegate callback)
	{
		PcNative.Stub("_SaveDataToFile");
	}
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _LoadDataFromFile(string path, LoadFileDelegate callback);
#else
	private static void _LoadDataFromFile(string path, LoadFileDelegate callback)
	{
		PcNative.Stub("_LoadDataFromFile");
	}
#endif

	public static bool CloudStorageAvailable { get; private set; }

	public static bool CheckCloudStorageAvailability()
	{
		CloudStorageAvailable = !string.IsNullOrEmpty(GetCloudContainerDirectoryPath());
		return CloudStorageAvailable;
	}

	public static string GetCloudContainerDirectoryPath()
	{
		return GetCloudContainerDirectoryPath(null);
	}

	public static string GetCloudContainerDirectoryPath(string containerID)
	{
		if (useNativeFileIO)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (_GetCloudContainerDirectoryPath(containerID, stringBuilder, (uint)stringBuilder.Capacity))
			{
				Debug.Log(">>>>>>>>" + stringBuilder.ToString());
				return stringBuilder.ToString();
			}
		}
		Debug.Log("OK");
		return null;
	}

	public static void FindFilePathInCloud(string path, Action<FileData> onComplete)
	{
		FindFilePathInCloud(path, null, onComplete);
	}

	public static void FindFilePathInCloud(string fileName, string containerID, Action<FileData> onComplete)
	{
		FileData fileData = new FileData(fileName, onComplete);
		fileData.ContainerID = containerID;
		if (useNativeFileIO)
		{
			if (AddAction(findFilePathInCloudActions, fileData))
			{
				_FindFilePathInCloud(containerID, fileName, FindFilePathInCloudComplete);
			}
		}
		else if (onComplete != null)
		{
			onComplete(fileData);
		}
	}

	public static string ConvertLocalPathToCloudPath(string localPath)
	{
		return ConvertLocalPathToCloudPath(localPath, null);
	}

	public static string ConvertLocalPathToCloudPath(string localPath, string containerID)
	{
		if (useNativeFileIO)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (_ConvertLocalPathToCloudPath(containerID, localPath, stringBuilder, (uint)stringBuilder.Capacity))
			{
				return stringBuilder.ToString();
			}
		}
		return null;
	}

	public static void SaveFile(string path, byte[] data)
	{
		SaveFile(path, data, null);
	}

	public static void SaveFile(string path, byte[] data, Action<FileData> onComplete)
	{
		FileData fileData = new FileData(path, onComplete);
		fileData.Data = data;
		Debug.Log("FIleManagerSaveFile - step1 ");
		if (useNativeFileIO)
		{
			if (AddAction(saveFileActions, fileData))
			{
				Debug.Log("FIleManagerSaveFile - step2 ");
				_SaveDataToFile(path, data, (uint)data.Length, SaveFileComplete);
			}
		}
		else
		{
			Debug.Log("FIleManagerSaveFile - step3 ");
			using (FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(data);
				}
			}
			if (onComplete != null)
			{
				fileData.Exists = true;
				onComplete(fileData);
			}
		}
	}

	public static void LoadFile(string path, Action<FileData> onComplete)
	{
		FileData fileData = new FileData(path, onComplete);
		if (useNativeFileIO)
		{
			if (AddAction(loadFileActions, fileData))
			{
				_LoadDataFromFile(path, LoadFileComplete);
			}
		}
		else
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					fileData.Data = binaryReader.ReadBytes((int)fileStream.Length);
				}
			}
			if (onComplete != null)
			{
				fileData.Exists = true;
				onComplete(fileData);
			}
		}
	}

	private static bool AddAction(Dictionary<string, FileData> actions, FileData data)
	{
		if (data != null && !string.IsNullOrEmpty(data.Path))
		{
			if (!actions.ContainsKey(data.Path))
			{
				actions.Add(data.Path, data);
				return true;
			}
			actions[data.Path].OnCompleteActions.AddRange(data.OnCompleteActions);
		}
		return false;
	}

	private static FileData RemoveAction(Dictionary<string, FileData> actions, string path)
	{
		FileData value = null;
		if (!string.IsNullOrEmpty(path) && actions.TryGetValue(path, out value))
		{
			actions.Remove(path);
		}
		return value;
	}

	[MonoPInvokeCallback(typeof(FindFilePathInCloudDelegate))]
	private static void FindFilePathInCloudComplete(string fileName, string cloudPath)
	{
		Debug.Log(string.Format("FindFilePathInCloudComplete: {0} = {1}", fileName, cloudPath));
		FileData fileData = RemoveAction(findFilePathInCloudActions, fileName);
		if (!(fileData == null))
		{
			if (!string.IsNullOrEmpty(cloudPath))
			{
				Debug.Log(">>>>>>>> cloudPath... OK");
				if (fileData.Data != null)
				{
					Debug.Log("aaaa" + fileData.Data.Length);
				}
				else
				{
					Debug.Log("sad");
				}
				fileData.Path = cloudPath;
				fileData.Exists = true;
			}
			foreach (Action<FileData> onCompleteAction in fileData.OnCompleteActions)
			{
				onCompleteAction(fileData);
			}
		}
	}

	[MonoPInvokeCallback(typeof(SaveFileDelegate))]
	private static void SaveFileComplete(string path, bool success)
	{
		Debug.Log(string.Format("SaveFileComplete: {0} exists = {1}", path, success.ToString()));
		FileData fileData = RemoveAction(saveFileActions, path);
		if (!(fileData == null))
		{
			fileData.Exists = success;
			foreach (Action<FileData> onCompleteAction in fileData.OnCompleteActions)
			{
				onCompleteAction(fileData);
			}
		}
	}

	[MonoPInvokeCallback(typeof(LoadFileDelegate))]
	private static void LoadFileComplete(string path, IntPtr data, uint length)
	{
		Debug.Log(string.Format("LoadFileComplete: {0} = {1} bytes", path, length));
		FileData fileData = RemoveAction(loadFileActions, path);
		if (fileData != null)
		{
			Debug.Log("result not null");
			if (length > 0 && data != IntPtr.Zero)
			{
				fileData.Data = new byte[length];
				Marshal.Copy(data, fileData.Data, 0, (int)length);
				fileData.Exists = true;
			}
			foreach (Action<FileData> onCompleteAction in fileData.OnCompleteActions)
			{
				Debug.Log("onComplete called");
				onCompleteAction(fileData);
			}
		}
	}
}
