using UnityEngine;

public class UIAnimationTypingText : UIAnimation
{
	public float Speed = -1f;

	private string _text;

	private int _length;

	private SpriteText _spriteText;

	public override void Finish()
	{
		SetText(_length);
		base.Finish();
	}

	public UIAnimationTypingText Setup(float speed, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		Speed = speed;
		return Setup(delay, duration, interpolatorType) as UIAnimationTypingText;
	}

	protected override void Run()
	{
		_spriteText = GetComponent<SpriteText>();
		_text = _spriteText.DisplayString;
		_length = _text.Length;
		if (Speed > 0f)
		{
			Duration = (float)_length / Speed;
		}
		base.Run();
	}

	protected override void UpdateAnimation()
	{
		if (_length > 0)
		{
			SetText(Mathf.RoundToInt(_interpolator(_time, 0f, _length, Duration)));
		}
	}

	private void SetText(int size)
	{
		string text = _text.Substring(0, size);
		if (size < _length && size > 0 && char.IsLetterOrDigit(_text[size]))
		{
			if (char.IsUpper(_text, size))
			{
				text = text + ((char)Random.Range(65, 90)).ToString();
			}
			else
			{
				text = text + ((char)Random.Range(97, 122)).ToString();
			}
		}
		_spriteText.Text = text;
	}
}
