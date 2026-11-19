using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class StorageManager
{
	public class StorageManagerException : Exception
	{
		public StorageManagerException()
		{
		}

		public StorageManagerException(string message)
			: base(message)
		{
		}
	}

	public static Version Version
	{
		get
		{
			return new Version(1, 1, 1);
		}
	}

	public static string PersistentDataPath
	{
		get
		{
			return Application.persistentDataPath;
		}
	}

	public static string TemporaryCachePath
	{
		get
		{
			return Application.temporaryCachePath;
		}
	}

	public static void WriteToLocation(string pathName, string temporaryPathName, byte[] data)
	{
		string text = pathName + ".check";
		string text2 = temporaryPathName + ".check.tmp";
		temporaryPathName += ".tmp";
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] inArray = sHA256Managed.ComputeHash(data);
		using (FileStream fileStream = File.Create(temporaryPathName))
		{
			fileStream.Write(data, 0, data.Length);
		}
		string data2 = Convert.ToBase64String(inArray);
		string key = PathToKey(pathName);
		if (!SetSafeDataForKey(key, data2))
		{
			throw new StorageManagerException("Error setting keychain value");
		}
		if (File.Exists(pathName))
		{
			File.Delete(pathName);
		}
		File.Move(temporaryPathName, pathName);
	}

	public static void Write(string fileName, byte[] data)
	{
		string temporaryPathName = Path.Combine(TemporaryCachePath, fileName);
		string pathName = Path.Combine(PersistentDataPath, fileName);
		WriteToLocation(pathName, temporaryPathName, data);
	}

	public static byte[] ReadFromLocation(string pathName)
	{
		byte[] array = File.ReadAllBytes(pathName);
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] strB = sHA256Managed.ComputeHash(array);
		byte[] array2 = null;
		string key = PathToKey(pathName);
		string text = Marshal.PtrToStringAnsi(GetSafeDataForKey(key));
		if (text.Equals(string.Empty))
		{
			array2 = File.ReadAllBytes(pathName + ".check");
			sHA256Managed = null;
		}
		else
		{
			array2 = Convert.FromBase64String(text);
		}
		if (!ByteArrayEquals(array2, strB))
		{
			throw new StorageManagerException("File " + pathName + " is corrupted");
		}
		if (sHA256Managed == null)
		{
			SetSafeDataForKey(key, Convert.ToBase64String(array2));
		}
		return array;
	}

	public static byte[] ReadFromLocationWithoutChecking(string pathName)
	{
		return File.ReadAllBytes(pathName);
	}

	public static byte[] Read(string fileName)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadFromLocation(pathName);
	}

	public static byte[] ReadWithoutChecking(string fileName)
	{
		string path = Path.Combine(PersistentDataPath, fileName);
		return File.ReadAllBytes(path);
	}

	public static void WriteXmlToLocation(string pathName, string temporaryPathName, object data)
	{
		temporaryPathName += ".tmp";
		MemoryStream memoryStream = new MemoryStream();
		XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());
		xmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);
		SHA256Managed sHA256Managed = new SHA256Managed();
		memoryStream.Position = 0L;
		byte[] inArray = sHA256Managed.ComputeHash(memoryStream);
		using (FileStream stream = File.Create(temporaryPathName))
		{
			memoryStream.WriteTo(stream);
		}
		string data2 = Convert.ToBase64String(inArray);
		string key = PathToKey(pathName);
		if (!SetSafeDataForKey(key, data2))
		{
			throw new StorageManagerException("Error setting keychain value");
		}
		if (File.Exists(pathName))
		{
			File.Delete(pathName);
		}
		File.Move(temporaryPathName, pathName);
	}

	public static void WriteXml(string fileName, object data)
	{
		string temporaryPathName = Path.Combine(TemporaryCachePath, fileName);
		string pathName = Path.Combine(PersistentDataPath, fileName);
		WriteXmlToLocation(pathName, temporaryPathName, data);
	}

	public static object ReadXmlFromLocation(string pathName, Type type)
	{
		object obj = null;
		MemoryStream memoryStream = null;
		using (FileStream fileStream = File.OpenRead(pathName))
		{
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			memoryStream = new MemoryStream(array);
		}
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] strB = sHA256Managed.ComputeHash(memoryStream);
		byte[] array2 = null;
		string key = PathToKey(pathName);
		string text = Marshal.PtrToStringAnsi(GetSafeDataForKey(key));
		if (text.Equals(string.Empty))
		{
			array2 = File.ReadAllBytes(pathName + ".check");
			sHA256Managed = null;
		}
		else
		{
			array2 = Convert.FromBase64String(text);
		}
		if (!ByteArrayEquals(array2, strB))
		{
			throw new StorageManagerException("File " + pathName + " is corrupted");
		}
		memoryStream.Position = 0L;
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		obj = xmlSerializer.Deserialize(XmlReader.Create(memoryStream));
		if (sHA256Managed == null)
		{
			SetSafeDataForKey(key, Convert.ToBase64String(array2));
		}
		return obj;
	}

	public static object ReadXml(string fileName, Type type)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadXmlFromLocation(pathName, type);
	}

	public static object ReadXmlFromLocationWithoutChecking(string pathName, Type type)
	{
		object obj = null;
		using (StreamReader textReader = new FileInfo(pathName).OpenText())
		{
			XmlSerializer xmlSerializer = new XmlSerializer(type);
			obj = xmlSerializer.Deserialize(textReader);
		}
		return obj;
	}

	public static object ReadXmlWithoutChecking(string fileName, Type type)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadXmlFromLocationWithoutChecking(pathName, type);
	}

	private static bool ByteArrayEquals(byte[] strA, byte[] strB)
	{
		int num = strA.Length;
		if (num != strB.Length)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (strA[i] != strB[i])
			{
				return false;
			}
		}
		return true;
	}

	private static string PathToKey(string path)
	{
		if (Path.IsPathRooted(path))
		{
			string[] array = new string[3] { "/Documents/", "/Library/", "/tmp/" };
			int num = -1;
			foreach (string value in array)
			{
				num = path.IndexOf(value);
				if (num >= 0)
				{
					break;
				}
			}
			if (num == -1)
			{
				throw new StorageManagerException("The file path " + path + " is invalid on iOS");
			}
			return path.Substring(num);
		}
		throw new StorageManagerException("Cannot use relative file path for access on iOS");
	}

	[DllImport("__Internal")]
	private static extern IntPtr GetSafeDataForKey(string key);

	[DllImport("__Internal")]
	private static extern bool SetSafeDataForKey(string key, string data);
}
