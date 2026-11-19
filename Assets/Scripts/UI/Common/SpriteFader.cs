using System;
using UnityEngine;

public class SpriteFader : MonoBehaviour
{
	public enum State
	{
		FadeIn,
		FadeOut
	}

	private SpriteRoot _sprite;

	private float _fadeSpeed;

	private float _alpha;

	public Action<State> OnFadeComplete;

	public bool IsFadingIn()
	{
		return _fadeSpeed > 0f;
	}

	public bool IsFadingOut()
	{
		return _fadeSpeed < 0f;
	}

	public bool IsFading()
	{
		return Mathf.Abs(_fadeSpeed) > 0f;
	}

	private void Awake()
	{
		_sprite = GetComponent<SimpleSprite>();
		if (!_sprite)
		{
			_sprite = GetComponent<PackedSprite>();
		}
		if (!_sprite)
		{
			_sprite = GetComponent<UISingleSprite>();
		}
		_alpha = 0f;
		_fadeSpeed = 0f;
	}

	public void FadeOut(float fadeTime, Color color)
	{
		_sprite.SetColor(color);
		FadeOut(fadeTime);
	}

	public void FadeOut(float fadeTime)
	{
		GetComponent<BoxCollider>().enabled = false;
		gameObject.active = true;
		_alpha = 1f;
		_fadeSpeed = -1f / fadeTime;
	}

	public void FadeIn(float fadeTime, Color color)
	{
		_sprite.SetColor(color);
		FadeIn(fadeTime);
	}

	public void FadeIn(float fadeTime)
	{
		GetComponent<BoxCollider>().enabled = true;
		gameObject.active = true;
		_alpha = 0f;
		_fadeSpeed = (fadeTime > 0f ? (1f / fadeTime) : float.MaxValue);
	}

	private void Update()
	{
		if (!(_fadeSpeed == 0f))
		{
			Color color = _sprite.Color;
			_alpha += _fadeSpeed * Time.deltaTime / Time.timeScale;
			color.a = Mathf.Clamp01(_alpha);
			_sprite.SetColor(color);
			if (_alpha >= 1f && _fadeSpeed > 0f)
			{
				_fadeSpeed = 0f;
				if (OnFadeComplete != null)
				{
					OnFadeComplete(State.FadeIn);
				}
			}
			else if (_alpha <= 0f && _fadeSpeed < 0f)
			{
				_fadeSpeed = 0f;
				if (OnFadeComplete != null)
				{
					OnFadeComplete(State.FadeOut);
				}
				gameObject.active = false;
			}
		}
	}
}
