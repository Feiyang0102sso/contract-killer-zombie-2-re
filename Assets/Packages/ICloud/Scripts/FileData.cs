using System;
using System.Collections.Generic;

namespace NSFilemanager
{
	public class MonoPInvokeCallbackAttribute : Attribute
	{
		private Type type;

		public MonoPInvokeCallbackAttribute(Type t)
		{
			type = t;
		}
	}
}

public class FileData
{
	public bool Exists { get; set; }

	public string Path { get; set; }

	public string ContainerID { get; set; }

	public byte[] Data { get; set; }

	public List<Action<FileData>> OnCompleteActions { get; private set; }

	public FileData(string path, Action<FileData> onComplete)
	{
		Path = path;
		OnCompleteActions = new List<Action<FileData>>();
		if (onComplete != null)
		{
			OnCompleteActions.Add(onComplete);
		}
	}
}
