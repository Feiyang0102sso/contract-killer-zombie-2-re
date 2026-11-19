using System;
using UnityEngine;

public class UIAnimationCountText : UIAnimation
{
	public int From;

	public int To;

	private Func<int, string> _formatter;

	public UIAnimationCountText Setup(int from, int to, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		return Setup(from, to, delay, duration, interpolatorType, null);
	}

	public UIAnimationCountText Setup(int from, int to, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType, Func<int, string> formatter)
	{
		From = from;
		To = to;
		_formatter = formatter;
		return Setup(delay, duration, interpolatorType) as UIAnimationCountText;
	}

	protected override void Run()
	{
		base.Run();
	}

	public override void Finish()
	{
		SetNumber(To);
		base.Finish();
	}

	protected override void UpdateAnimation()
	{
		SetNumber(Mathf.RoundToInt(_interpolator(_time, From, To - From, Duration)));
	}

	private void SetNumber(int n)
	{
		foreach (SpriteText spriteText in GetComponentsInChildren<SpriteText>(false))
		{
			if (_formatter != null)
			{
				spriteText.Text = _formatter(n);
			}
			else
			{
				spriteText.Text = n.ToString();
			}
		}
	}
}
