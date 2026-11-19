using UnityEngine;

public class StickScript : MonoBehaviour
{
	public enum KeyboardType
	{
		None,
		WASD,
		Arrows
	}

	private const float m_moveRadius = 16f;

	private Vector2 m_vector = Vector2.zero;

	private Vector2 m_currentVector = Vector2.one;

	private bool _pointerActive;

	private Vector2[] m_pressPos = new Vector2[2];

	private Vector2[] m_lastPos = new Vector2[2];

	private Vector2 m_delta = Vector2.zero;

	private Vector3 _targetBasePosition = Vector3.zero;

	private float m_dragFilterCoeff;

	private float dragFilterDamping = 0.32f;

	public KeyboardType m_keyboardType;

	private bool _keyboardActive;

	private GameObject _stickSprite;

	private Transform _stickSpriteTransform;

	private Vector2 _basePosition;

	private bool _visible = true;

	public bool IsDynamic = true;

	public Vector2 GetVector()
	{
		return m_currentVector;
	}

	public Vector2 GetDelta()
	{
		return m_delta;
	}

	public void ResetDelta()
	{
		m_delta = Vector2.zero;
	}

	public bool IsPressed()
	{
		return _pointerActive || _keyboardActive;
	}

	public bool HandleSecondFinger { get; set; }

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			_visible = value;
			UpdateVisibility();
		}
	}

	private int GetInternalFingerId(int id)
	{
		return (id > 0) ? 1 : 0;
	}

	private void Awake()
	{
		m_pressPos[0] = Vector2.zero;
		m_pressPos[1] = Vector2.zero;
		m_lastPos[0] = Vector2.zero;
		m_lastPos[1] = Vector2.zero;
		HandleSecondFinger = false;
		UIButton component = GetComponent<UIButton>();
		component.SetInputDelegate(InputDelegate);
		_stickSprite = this.QuickFindChild("StickSprite");
		if ((bool)_stickSprite)
		{
			_stickSpriteTransform = _stickSprite.transform;
			_targetBasePosition = _stickSpriteTransform.localPosition;
		}
	}

	private void OnPointerActive(ref POINTER_INFO pointer)
	{
		Vector2 vector = pointer.ray.origin;
		m_vector = vector - m_pressPos[GetInternalFingerId(pointer.id)];
		Vector2 vector2 = vector - m_lastPos[GetInternalFingerId(pointer.id)];
		vector2 *= m_dragFilterCoeff;
		m_dragFilterCoeff = Mathf.Lerp(m_dragFilterCoeff, 1f, dragFilterDamping);
		m_delta = m_delta + vector2;
		m_lastPos[GetInternalFingerId(pointer.id)] = vector;
	}

	public void InputDelegate(ref POINTER_INFO pointer)
	{
		switch (pointer.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			if (IsDynamic && (bool)_stickSprite)
			{
				Vector3 position = _stickSpriteTransform.position;
				_stickSpriteTransform.position = new Vector3(pointer.ray.origin.x, pointer.ray.origin.y, position.z);
				_targetBasePosition = _stickSpriteTransform.localPosition;
			}
			m_dragFilterCoeff = 0f;
			m_delta = Vector2.zero;
			m_lastPos[GetInternalFingerId(pointer.id)] = pointer.ray.origin;
			m_pressPos[GetInternalFingerId(pointer.id)] = pointer.ray.origin;
			OnPointerActive(ref pointer);
			_pointerActive = true;
			UpdateVisibility();
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			OnPointerActive(ref pointer);
			_pointerActive = true;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			m_dragFilterCoeff = 0f;
			if ((bool)_stickSprite)
			{
				_stickSpriteTransform.localPosition = _targetBasePosition;
			}
			m_vector = Vector2.zero;
			_pointerActive = false;
			UpdateVisibility();
			break;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!_keyboardActive && !_pointerActive)
		{
			m_vector = Vector2.zero;
		}
		if (m_vector != m_currentVector)
		{
			float magnitude = m_vector.magnitude;
			m_vector.Normalize();
			if (magnitude > 16f)
			{
				m_currentVector = m_vector;
			}
			else
			{
				m_currentVector.x = m_vector.x * magnitude / 16f;
				m_currentVector.y = m_vector.y * magnitude / 16f;
			}
			m_vector = m_currentVector;
			Vector3 localPosition = new Vector3();
			localPosition.x = _targetBasePosition.x + m_vector.x * 16f;
			localPosition.y = _targetBasePosition.y + m_vector.y * 16f;
			localPosition.z = _targetBasePosition.z;
			if ((bool)_stickSprite)
			{
				_stickSpriteTransform.localPosition = localPosition;
			}
			UpdateVisibility();
		}
	}

	private void UpdateVisibility()
	{
		bool flag = false;
		if (IsDynamic)
		{
			flag = Visible && _pointerActive;
		}
		else
		{
			flag = Visible;
		}
		if ((bool)_stickSprite)
		{
			_stickSprite.GetComponent<Renderer>().enabled = flag;
		}
	}
}
