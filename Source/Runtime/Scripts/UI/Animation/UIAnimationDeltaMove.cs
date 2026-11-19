using UnityEngine;

public class UIAnimationDeltaMove : UIAnimation
{
	public Vector3 MoveDelta = Vector3.zero;

	private Vector3 _initialPosition;

	public UIAnimationDeltaMove Setup(float dx, float dy, float dz, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		MoveDelta = new Vector3(dx, dy, dz);
		return Setup(delay, duration, interpolatorType) as UIAnimationDeltaMove;
	}

	protected override void Run()
	{
		_initialPosition = transform.localPosition;
		base.Run();
	}

	public override void Finish()
	{
		transform.localPosition = _initialPosition + MoveDelta;
		base.Finish();
	}

	protected override void UpdateAnimation()
	{
		float x = _interpolator(_time, _initialPosition.x, MoveDelta.x, Duration);
		float y = _interpolator(_time, _initialPosition.y, MoveDelta.y, Duration);
		float z = _interpolator(_time, _initialPosition.z, MoveDelta.z, Duration);
		transform.localPosition = new Vector3(x, y, z);
	}
}
