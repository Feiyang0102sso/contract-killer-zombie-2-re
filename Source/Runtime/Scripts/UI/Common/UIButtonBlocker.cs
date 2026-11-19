using UnityEngine;

public class UIButtonBlocker : MonoBehaviour
{
	private Color _color;

	private UIButton _button;

	private static Color BlockColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private void Awake()
	{
		_button = GetComponent<UIButton>();
		_color = _button.Color;
		SetBlock(true);
	}

	public void SetBlock(bool block)
	{
		if ((bool)_button)
		{
			_button.controlIsEnabled = !block;
			_button.Color = (block ? BlockColor : _color);
		}
	}

	public void Unblock()
	{
		SetBlock(false);
		Object.Destroy(this);
	}

	private void OnDestroy()
	{
		SetBlock(false);
	}
}
