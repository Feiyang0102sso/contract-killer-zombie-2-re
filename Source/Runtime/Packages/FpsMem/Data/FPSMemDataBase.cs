using System;
using UnityEngine;

[RequireComponent(typeof(FPSMemChecker))]
public class FPSMemDataBase : FPSMemDataAbstract
{
	private FPSMemChecker _memChecker;

	protected override void Fill(FPSMemRecord record)
	{
		record.Add("timestamp", DateTime.Now);
		record.Add("fps", _memChecker.FPS);
		record.Add("mem", _memChecker.Memory);
	}

	private void Awake()
	{
		_memChecker = GetComponent<FPSMemChecker>();
		if (_memChecker == null)
		{
			Debug.LogError("FpsMemDataSimple requires FPSMemChecker component on same GameObject");
		}
	}
}
