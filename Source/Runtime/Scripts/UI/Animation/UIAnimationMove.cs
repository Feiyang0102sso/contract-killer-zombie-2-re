using UnityEngine;

public class UIAnimationMove : UIAnimation
{
	public float DeltaX;

	public float DeltaY;

	private Vector3 _initialPosition;

	public UIAnimationMove Setup(float deltaX, float deltaY, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		DeltaX = deltaX;
		DeltaY = deltaY;
		return Setup(delay, duration, interpolatorType) as UIAnimationMove;
	}

	protected override void Run()
	{
		_initialPosition = transform.localPosition;
		base.Run();
	}

	public override void Finish()
	{
		transform.localPosition = _initialPosition;
		base.Finish();
	}

	protected override void UpdateAnimation()
	{
		float x = _interpolator(_time, _initialPosition.x + DeltaX, -DeltaX, Duration);
		float y = _interpolator(_time, _initialPosition.y + DeltaY, -DeltaY, Duration);
		transform.localPosition = new Vector3(x, y, transform.localPosition.z);
	}
}
