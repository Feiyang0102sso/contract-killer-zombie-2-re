using System;
using UnityEngine;

public abstract class UIAnimation : MonoBehaviour
{
	public bool RunOnStart;

	public float Duration = 1f;

	public float Delay;

	public bool DestroyOnFinish = true;

	public bool DisableOnFinish;

	public bool DestroyObjectOnFinish;

	public Action OnFinishDelegate = () =>
	{
	};

	public EZAnimation.EASING_TYPE InterpolatorType = EZAnimation.EASING_TYPE.ExponentialIn;

	protected float _time;

	protected float _delay;

	protected bool _isEnabled;

	protected EZAnimation.Interpolator _interpolator;

	public UIAnimation Setup(float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		Delay = delay;
		Duration = duration;
		InterpolatorType = interpolatorType;
		return this;
	}

	private void Awake()
	{
		if (!RunOnStart)
		{
			enabled = false;
		}
	}

	private void Start()
	{
		if (RunOnStart)
		{
			RunAnimation();
		}
	}

	public UIAnimation RunAnimation()
	{
		if (enabled || _isEnabled)
		{
			return this;
		}
		Run();
		return this;
	}

	protected virtual void Run()
	{
		_interpolator = EZAnimation.GetInterpolator(InterpolatorType);
		enabled = true;
		_isEnabled = true;
		_time = 0f;
		_delay = Delay;
		UpdateAnimation();
	}

	public virtual void Finish()
	{
		OnFinishDelegate();
		if (DestroyOnFinish)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			_isEnabled = false;
		}
		if (DisableOnFinish)
		{
			gameObject.active = false;
		}
		enabled = false;
		if (DestroyObjectOnFinish)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	protected abstract void UpdateAnimation();

	protected virtual void Update()
	{
		if (_delay > 0f)
		{
			_delay -= Time.deltaTime / Time.timeScale;
		}
		else
		{
			if (_time > Duration)
			{
				Finish();
				return;
			}
			UpdateAnimation();
			_time += Time.deltaTime / Time.timeScale;
		}
	}
}
