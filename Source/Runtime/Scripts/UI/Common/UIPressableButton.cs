using UnityEngine;

public class UIPressableButton : UIButton
{
	public Color OnPressColor = Color.white;

	public Vector3 PressedScale = Vector3.one;

	public float _OnPressMultiplier = 1.5f;

	private Color _color;

	private Vector3 _scale;

	private Material _sharedMaterialTemp;

	private bool _pressed;

	private Renderer _renderer;

	protected override void Awake()
	{
		base.Awake();
		AddInputDelegate(InputDelegate);
		_color = Color;
		_scale = transform.localScale;
		_renderer = GetComponent<Renderer>();
	}

	public void InputDelegate(ref POINTER_INFO info)
	{
		switch (info.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			SetPressed(true);
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			SetPressed(false);
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
		{
			Vector2 vector = TopLeft + transform.position;
			if (!new Rect(vector.x, vector.y - height, width, height).Contains(info.ray.origin))
			{
				SetPressed(false);
			}
			else
			{
				SetPressed(true);
			}
			break;
		}
		}
	}

	private void SetPressed(bool pressed)
	{
		if (pressed == _pressed || !enabled)
		{
			return;
		}
		if (pressed)
		{
			if (_renderer != null)
			{
				_sharedMaterialTemp = _renderer.sharedMaterial;
				if (Singleton.Exists())
				{
					_renderer.material.shader = Singleton.UI.PressStateMaterial.shader;
				}
				_renderer.material.SetFloat("_Multiplier", _OnPressMultiplier);
			}
			Color = OnPressColor;
			transform.localScale = PressedScale;
		}
		else
		{
			if ((bool)_sharedMaterialTemp && (bool)_renderer)
			{
				_renderer.material = _sharedMaterialTemp;
				_renderer.sharedMaterial = _sharedMaterialTemp;
			}
			Color = _color;
			transform.localScale = _scale;
		}
		_pressed = pressed;
	}

	public void SetBaseColor(Color color)
	{
		_color = color;
	}
}
