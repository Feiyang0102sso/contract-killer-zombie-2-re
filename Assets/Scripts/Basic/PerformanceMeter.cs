using System;
using UnityEngine;

public class PerformanceMeter : MonoBehaviour
{
	private const int BufferSize = 10;

	private float[] _frameTime;

	private int _frameIndex;

	private float _prevRealtime;

	private int _prevFPS;

	private GameEvents _gameEvents;

	private void Awake()
	{
		_gameEvents = GetComponent<GameEvents>();
		_frameTime = new float[10];
		_frameIndex = 0;
		_prevRealtime = Time.realtimeSinceStartup;
		_prevFPS = -1;
		_gameEvents.OnFPSDecreased += (Action<int>)((int x) =>
		{
		});
		_gameEvents.OnFPSIncreased += (Action<int>)((int x) =>
		{
		});
		for (int num = 0; num < 10; num++)
		{
			_frameTime[num] = 0f;
		}
	}

	private void Update()
	{
		_frameTime[_frameIndex] = Time.realtimeSinceStartup - _prevRealtime;
		_prevRealtime = Time.realtimeSinceStartup;
		_frameIndex++;
		if (!(_frameIndex < 10))
		{
			float num = 0f;
			for (int i = 0; i < 10; i++)
			{
				num += _frameTime[i];
			}
			num /= 10f;
			int num2 = (int)(1f / num);
			if (_prevFPS >= 0)
			{
				if (num2 < _prevFPS)
				{
					_gameEvents.OnFPSDecreased(num2);
				}
				if (num2 > _prevFPS)
				{
					_gameEvents.OnFPSIncreased(num2);
				}
			}
			_prevFPS = num2;
			_frameIndex = 0;
		}
	}
}
