using UnityEngine;

public class UIAnimationRotate : UIAnimation
{
	public float From = -180f;

	public float To;

	public Vector3 Axis = Vector3.right;

	public UIAnimationRotate Setup(float from, float to, Vector3 axis, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		From = from;
		To = to;
		Axis = axis;
		return Setup(delay, duration, interpolatorType) as UIAnimationRotate;
	}

	protected override void Run()
	{
		base.Run();
	}

	public override void Finish()
	{
		SetRotation(To);
		base.Finish();
	}

	protected override void UpdateAnimation()
	{
		SetRotation(_interpolator(_time, From, To - From, Duration));
	}

	private void SetRotation(float r)
	{
		transform.localRotation = Quaternion.AngleAxis(r, Axis);
	}
}
