using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryFiller : MonoBehaviour
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("App.Memory");
			LoggerSingleton<Logger>.SetLevel(10);
		}
	}

	private const int KILO_STEP = 1024;

	private const float TIME_STEP_SEC = 0.5f;

	private const string BUTTON_SPACE = "\n\n\n\n";

	private List<byte[]> _fill = new List<byte[]>();

	private int _fillMeg
	{
		get
		{
			return _fill.Count * 1024 / 1024;
		}
	}

	private int _totalMeg
	{
		get
		{
			return NativeUtils.GetCurrentMemoryBytes() >> 20;
		}
	}

	private IEnumerator FillMemoryCoroutine()
	{
		while (true)
		{
			AddMoreMemory();
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void AddMoreMemory()
	{
		int num = 1048576;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (byte)i;
		}
		_fill.Add(array);
	}
}
