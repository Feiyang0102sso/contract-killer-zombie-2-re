using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

// Two changes for the PC port:
// 1. The original save verification is gone. The original stored the SHA-256 of every save in the iOS keychain
//    (keyed by the sandbox-relative path) and compared it on load; on a mismatch it threw StorageManagerException
//    and PlayerData treated the save as a new game. Neither the PC port nor later ports need this, so reading and
//    writing now touch only the file itself. Writes still go to a temporary file first and are then renamed,
//    so an interrupted write cannot corrupt the save.
// 2. Saves live in a Save folder next to the exe (portable) instead of the system folder persistentDataPath
//    (AppData\LocalLow\<company>\<product>).
public class StorageManager
{
	/// <summary>Name of the save folder, next to the exe.</summary>
	private const string SaveDirName = "Save";

	public static Version Version
	{
		get
		{
			return new Version(1, 1, 1);
		}
	}

	/// <summary>
	/// Save folder: the Save folder in the exe's directory (the Save folder in the project root when playing in the editor).
	/// How: in a build Application.dataPath points to the ckz2_Data folder next to the exe, in the editor to the project's
	/// Assets folder; its parent is therefore the exe's directory or the project root. Created if missing.
	/// </summary>
	public static string PersistentDataPath
	{
		get
		{
			string rootDir = Path.GetDirectoryName(Application.dataPath);
			string saveDir = Path.Combine(rootDir, SaveDirName);
			Directory.CreateDirectory(saveDir);
			return saveDir;
		}
	}

	/// <summary>PC port: a Cache folder next to the exe instead of Application.temporaryCachePath (see PcPaths).</summary>
	public static string TemporaryCachePath
	{
		get
		{
			return PcPaths.CacheDir;
		}
	}

	public static void WriteToLocation(string pathName, string temporaryPathName, byte[] data)
	{
		temporaryPathName += ".tmp";
		using (FileStream fileStream = File.Create(temporaryPathName))
		{
			fileStream.Write(data, 0, data.Length);
		}
		if (File.Exists(pathName))
		{
			File.Delete(pathName);
		}
		File.Move(temporaryPathName, pathName);
	}

	public static void Write(string fileName, byte[] data)
	{
		// Keep the temporary file in the same folder as the save: the system temp folder may be on another drive,
		// and a rename across drives degrades into a copy
		string pathName = Path.Combine(PersistentDataPath, fileName);
		WriteToLocation(pathName, pathName, data);
	}

	public static byte[] ReadFromLocation(string pathName)
	{
		return File.ReadAllBytes(pathName);
	}

	public static byte[] Read(string fileName)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadFromLocation(pathName);
	}

	public static void WriteXmlToLocation(string pathName, string temporaryPathName, object data)
	{
		temporaryPathName += ".tmp";
		MemoryStream memoryStream = new MemoryStream();
#if UNITY_IOS
		XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());
		xmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);
#else
		PcXmlPairs.Serialize(XmlWriter.Create(memoryStream), data);
#endif
		using (FileStream stream = File.Create(temporaryPathName))
		{
			memoryStream.WriteTo(stream);
		}
		if (File.Exists(pathName))
		{
			File.Delete(pathName);
		}
		File.Move(temporaryPathName, pathName);
	}

	public static void WriteXml(string fileName, object data)
	{
		// Temporary file next to the save, same reason as in Write
		string pathName = Path.Combine(PersistentDataPath, fileName);
		WriteXmlToLocation(pathName, pathName, data);
	}

	public static object ReadXmlFromLocation(string pathName, Type type)
	{
		object obj = null;
		MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(pathName));
#if UNITY_IOS
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		obj = xmlSerializer.Deserialize(XmlReader.Create(memoryStream));
#else
		obj = PcXmlPairs.Deserialize(XmlReader.Create(memoryStream), type);
#endif
		return obj;
	}

	public static object ReadXml(string fileName, Type type)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadXmlFromLocation(pathName, type);
	}
}
