using UnityEngine;

public class HudArrow : MonoBehaviour
{
	public bool fadeOut;

	public float RotationDamping = 10f;

	public PackedSprite ArrowSprite;

	private Transform _targetTransform;

	private Transform _centerTransform;

	private CountdownTimer _lifeTimer = new CountdownTimer();

	private Transform _transform;

	private Color _color;

	private Color _blinkColor;

	private float _blinkInterval = 1f;

	private bool _blinking;

	private float _radius;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
			ArrowSprite.transform.localPosition = new Vector3(0f, _radius, 0f);
		}
	}

	public void SetTargetTransform(Transform targetTransform)
	{
		_targetTransform = targetTransform;
	}

	public void SetCenterTransform(Transform centerTransform)
	{
		_centerTransform = centerTransform;
	}

	public static float Angle(Vector3 center, Vector3 danger)
	{
		Vector3 forward = Scene.MainCamera.transform.forward;
		Vector3 lhs = danger - center;
		return Vector2.Angle(new Vector2(forward.x, forward.z), new Vector2(lhs.x, lhs.z)) * Mathf.Sign(Vector3.Dot(lhs, Scene.MainCamera.transform.right));
	}

	public float Range(Vector3 center)
	{
		return (_targetTransform.position - center).magnitude;
	}

	public void Charge(float time)
	{
		_lifeTimer.Run(time);
	}

	public bool IsExpired()
	{
		return !_lifeTimer.IsRunning();
	}

	public void SetColor(Color c)
	{
		_color = c;
		SetArrowColor(_color);
	}

	private void SetArrowColor(Color c)
	{
		ArrowSprite.Color = c;
	}

	public void SetBlinking(bool blinking)
	{
		if (_blinking && !blinking)
		{
			SetArrowColor(_color);
		}
		_blinking = blinking;
	}

	public void SetBlinkingParams(Color blinkColor, float interval)
	{
		_blinkColor = blinkColor;
		_blinkInterval = interval;
	}

	private void Awake()
	{
		_transform = transform;
	}

	private void Update()
	{
		if (_targetTransform == null)
		{
			ArrowSprite.Hide(true);
			return;
		}
		if (_blinking)
		{
			float t = Mathf.Abs(_blinkInterval * 0.5f - Time.time % _blinkInterval) / (_blinkInterval * 0.5f);
			SetArrowColor(Color.Lerp(_color, _blinkColor, t));
		}
		float num = Angle(_centerTransform.position, _targetTransform.position);
		Quaternion to = Quaternion.Euler(0f, 0f, -num);
		ArrowSprite.Hide(false);
		_transform.rotation = Quaternion.Lerp(_transform.rotation, to, Time.deltaTime * RotationDamping);
		if (fadeOut)
		{
			ArrowSprite.SetColor(new Color(ArrowSprite.Color.r, ArrowSprite.Color.g, ArrowSprite.Color.b, 1f - _lifeTimer.Progress()));
		}
		Camera camera = UIManager.instance.uiCameras[0].camera;
	}
}
