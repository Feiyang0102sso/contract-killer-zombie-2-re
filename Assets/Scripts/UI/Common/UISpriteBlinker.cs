using UnityEngine;

public class UISpriteBlinker : MonoBehaviour
{
	public Color BlinkColor = Color.blue;

	public float Freq = 12f;

	private Color _color;

	public bool AnimateScale;

	public Vector3 BlinkScale = new Vector3(1.2f, 1.2f, 1.2f);

	private SpriteRoot _sprite;

	private Vector3 _scale;

	private void Awake()
	{
		_sprite = GetComponent<SpriteRoot>();
		_color = _sprite.Color;
		_scale = transform.localScale;
	}

	private void Update()
	{
		float t = (Mathf.Sin(Time.realtimeSinceStartup * Freq) + 1f) * 0.5f;
		_sprite.Color = Color.Lerp(_color, BlinkColor, t);
		if (AnimateScale)
		{
			transform.localScale = Vector3.Lerp(_scale, BlinkScale, t);
		}
	}

	public void SetOriginalColor(Color c)
	{
		_color = c;
	}

	public void Play(bool play)
	{
		if (play == enabled)
		{
			return;
		}
		if (!play)
		{
			if ((bool)_sprite)
			{
				_sprite.Color = _color;
			}
			if (AnimateScale)
			{
				transform.localScale = _scale;
			}
		}
		enabled = play;
	}

	public void Finish()
	{
		Play(false);
		Object.Destroy(this);
	}

	private void OnDestroy()
	{
		Play(false);
	}
}
