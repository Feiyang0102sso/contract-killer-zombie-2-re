using UnityEngine;

public class CustomInterpolators
{
	public enum EASING_TYPE
	{
		Default = -1,
		Linear,
		CubicIn,
		CubicOut,
		ExponentialIn,
		ExponentialOut,
		QuinticIn,
		QuinticOut,
		ExpInOut1,
		ExpInOut2,
		ExpInOut3,
		ExpInOut4,
		ExpInOut5,
		ExpInOut6,
		Ladder1,
		Ladder2,
		Ladder3,
		Ladder4,
		Ladder5
	}

	public static float ExpInOut1(float time, float start, float delta, float duration)
	{
		return ExpInOutN(1, time, start, delta, duration);
	}

	public static float ExpInOut2(float time, float start, float delta, float duration)
	{
		return ExpInOutN(2, time, start, delta, duration);
	}

	public static float ExpInOut3(float time, float start, float delta, float duration)
	{
		return ExpInOutN(3, time, start, delta, duration);
	}

	public static float ExpInOut4(float time, float start, float delta, float duration)
	{
		return ExpInOutN(4, time, start, delta, duration);
	}

	public static float ExpInOut5(float time, float start, float delta, float duration)
	{
		return ExpInOutN(5, time, start, delta, duration);
	}

	public static float ExpInOut6(float time, float start, float delta, float duration)
	{
		return ExpInOutN(6, time, start, delta, duration);
	}

	public static float ExpInOutN(int n, float time, float start, float delta, float duration)
	{
		float num = Mathf.Min(Mathf.FloorToInt(time / duration * (float)n + 0.95f), n) + 1;
		return EZAnimation.expInOut(time - duration / (float)n * (num - 1f), start, delta / (float)n, duration / (float)n) + duration / (float)n * (num - 1f);
	}

	public static float Ladder1(float time, float start, float delta, float duration)
	{
		return LadderN(1, time, start, delta, duration);
	}

	public static float Ladder2(float time, float start, float delta, float duration)
	{
		return LadderN(2, time, start, delta, duration);
	}

	public static float Ladder3(float time, float start, float delta, float duration)
	{
		return LadderN(3, time, start, delta, duration);
	}

	public static float Ladder4(float time, float start, float delta, float duration)
	{
		return LadderN(4, time, start, delta, duration);
	}

	public static float Ladder5(float time, float start, float delta, float duration)
	{
		return LadderN(5, time, start, delta, duration);
	}

	public static float LadderN(int n, float time, float start, float delta, float duration)
	{
		float num = Mathf.Min(Mathf.FloorToInt(time / duration * (float)n + 0.95f), n);
		return duration / (float)n * num;
	}

	public static EZAnimation.Interpolator GetInterpolator(EASING_TYPE type)
	{
		switch (type)
		{
		case EASING_TYPE.CubicIn:
			return EZAnimation.cubicIn;
		case EASING_TYPE.CubicOut:
			return EZAnimation.cubicOut;
		case EASING_TYPE.ExponentialOut:
			return EZAnimation.expOut;
		case EASING_TYPE.Linear:
			return EZAnimation.linear;
		case EASING_TYPE.QuinticIn:
			return EZAnimation.quinticIn;
		case EASING_TYPE.QuinticOut:
			return EZAnimation.quinticOut;
		case EASING_TYPE.ExpInOut1:
			return ExpInOut1;
		case EASING_TYPE.ExpInOut2:
			return ExpInOut2;
		case EASING_TYPE.ExpInOut3:
			return ExpInOut3;
		case EASING_TYPE.ExpInOut4:
			return ExpInOut4;
		case EASING_TYPE.ExpInOut5:
			return ExpInOut5;
		case EASING_TYPE.ExpInOut6:
			return ExpInOut6;
		case EASING_TYPE.Ladder1:
			return Ladder1;
		case EASING_TYPE.Ladder2:
			return Ladder2;
		case EASING_TYPE.Ladder3:
			return Ladder3;
		case EASING_TYPE.Ladder4:
			return Ladder4;
		case EASING_TYPE.Ladder5:
			return Ladder5;
		default:
			return EZAnimation.linear;
		}
	}
}
