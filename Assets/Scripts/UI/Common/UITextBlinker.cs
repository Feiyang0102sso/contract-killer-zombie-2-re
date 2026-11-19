using UnityEngine;

public class UITextBlinker : MonoBehaviour
{
	public Color BlinkColor = Color.blue;

	public float Freq = 12f;

	private Color _color;

	private SpriteText _text;

	private void Awake()
	{
		_text = GetComponent<SpriteText>();
		_color = _text.Color;
	}

	private void Update()
	{
		float t = (Mathf.Sin(Time.realtimeSinceStartup * Freq) + 1f) * 0.5f;
		_text.Color = Color.Lerp(_color, BlinkColor, t);
	}

	public void SetOriginalColor(Color c)
	{
		_color = c;
	}

	public void Play(bool play)
	{
		if (!(play != enabled))
		{
			return;
		}
		if (!play)
		{
			_text.Color = _color;
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
