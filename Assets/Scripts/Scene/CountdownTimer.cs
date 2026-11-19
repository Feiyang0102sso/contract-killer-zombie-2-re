using UnityEngine;

public class CountdownTimer
{
	private float _finishTime;

	private float _totalTime = 1f;

	public CountdownTimer()
	{
	}

	public CountdownTimer(float time)
	{
		Run(time);
	}

	public void Run(float time)
	{
		_finishTime = GetTime() + time;
		_totalTime = time;
	}

	public bool IsRunning()
	{
		return _finishTime >= GetTime();
	}

	public void Reset()
	{
		_finishTime = 0f;
	}

	public float LeftTime()
	{
		return !(!IsRunning()) ? (_finishTime - GetTime()) : 0f;
	}

	public float TotalTime()
	{
		return _totalTime;
	}

	public float Progress()
	{
		return (TotalTime() - LeftTime()) / TotalTime();
	}

	protected virtual float GetTime()
	{
		return Time.time;
	}
}

public class RTCountdownTimer : CountdownTimer
{
	public RTCountdownTimer()
	{
	}

	public RTCountdownTimer(float time)
		: base(time)
	{
	}

	protected override float GetTime()
	{
		return TimeControl.RealTime;
	}
}
