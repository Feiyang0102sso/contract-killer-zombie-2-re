using UnityEngine;

public class MiniScript : MonoBehaviour
{
	public delegate void Action(MiniScript script);

	private Action init;

	private Action updater;

	private Action timeout;

	private CountdownTimer timer;

	private object param;

	public CountdownTimer Timer
	{
		get
		{
			return timer;
		}
	}

	public T Param<T>()
	{
		return (T)param;
	}

	public void Charge(float time, object param_, Action init_, Action updater_, Action timeout_)
	{
		if (time != 0f)
		{
			timer = CreateTimer(time);
		}
		param = param_;
		init = init_;
		updater = updater_;
		timeout = timeout_;
	}

	protected virtual CountdownTimer CreateTimer(float time)
	{
		return new CountdownTimer(time);
	}

	private void Start()
	{
		if (init != null)
		{
			init(this);
		}
	}

	private void Update()
	{
		if (updater != null)
		{
			updater(this);
		}
		if (timer != null && !timer.IsRunning())
		{
			if (timeout != null)
			{
				timeout(this);
			}
			Object.Destroy(this);
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		Object.Destroy(gameObject);
	}
}

public class RTMiniScript : MiniScript
{
	protected override CountdownTimer CreateTimer(float time)
	{
		return new RTCountdownTimer(time);
	}
}
