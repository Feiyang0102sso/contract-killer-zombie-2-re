using UnityEngine;

public class UIAnimationScale : UIAnimation
{
	public float From = 1f;

	public float To = 1f;

	public UIAnimationScale Setup(float from, float to, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		From = from;
		To = to;
		return Setup(delay, duration, interpolatorType) as UIAnimationScale;
	}

	protected override void Run()
	{
		base.Run();
	}

	public override void Finish()
	{
		SetScale(To);
		base.Finish();
	}

	protected override void UpdateAnimation()
	{
		SetScale(_interpolator(_time, From, To - From, Duration));
	}

	private void SetScale(float s)
	{
		transform.localScale = new Vector3(s, s, s);
	}
}
