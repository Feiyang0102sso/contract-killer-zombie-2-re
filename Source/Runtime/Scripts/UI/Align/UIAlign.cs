using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("UI/UIAlign")]
public class UIAlign : MonoBehaviour
{
	public enum Horizontal
	{
		None,
		Left,
		Right,
		Center
	}

	public enum Vertical
	{
		None,
		Top,
		Bottom,
		Center
	}

	public enum Mode
	{
		Screen,
		Parent
	}

	private Camera m_camera;

	private Transform _cachedTransform;

	public Horizontal m_horizontal;

	public Vertical m_vertical;

	public Mode m_mode;

	public Vector2 m_margin = Vector2.zero;

	public Vector2 ScreenPercentMargin = Vector2.zero;

	private Transform m_transform
	{
		get
		{
			if (_cachedTransform == null)
			{
				_cachedTransform = transform;
			}
			return _cachedTransform;
		}
	}

	private void Awake()
	{
		if (enabled)
		{
			UpdateAlignment();
		}
	}

	private void Start()
	{
		if (enabled)
		{
			UpdateAlignment();
		}
	}

	public bool FindCamera()
	{
		if (m_camera == null)
		{
			EZCameraSettings[] uiCameras = UIManager.instance.uiCameras;
			if (uiCameras.Length == 0)
			{
				return false;
			}
			m_camera = uiCameras[0].camera;
		}
		return m_camera != null;
	}

	private Vector2 GetScreenSize()
	{
		Vector3 vector = -2f * m_camera.ScreenToWorldPoint(Vector2.zero);
		return new Vector2(vector.x, vector.y);
	}

	private UIRect GetScreenRect()
	{
		Vector2 screenSize = GetScreenSize();
		return new UIRect(0f, 0f, screenSize.x, screenSize.y);
	}

	private UIRect GetParentRect()
	{
		float x = m_transform.position.x;
		float y = m_transform.position.y;
		float width = 0f;
		float height = 0f;
		Transform parent = m_transform.parent;
		if (parent != null)
		{
			x = parent.position.x;
			y = parent.position.y;
			SpriteRoot component = parent.GetComponent<SpriteRoot>();
			if (component != null)
			{
				Vector3 centerPoint = component.GetCenterPoint();
				x = parent.position.x + centerPoint.x;
				y = parent.position.y + centerPoint.y;
				width = component.width;
				height = component.height;
			}
			UIPackable component2 = parent.GetComponent<UIPackable>();
			if (component2 != null)
			{
				x = parent.position.x;
				y = parent.position.y;
				width = component2.Width;
				height = component2.Height;
			}
		}
		return new UIRect(x, y, width, height);
	}

	private void UpdateAlignment(UIRect rect)
	{
		float num = rect.m_x;
		float num2 = rect.m_y;
		float width = rect.m_width;
		float height = rect.m_height;
		Vector2 margin = m_margin;
		margin.x += ScreenPercentMargin.x * width * 0.01f;
		margin.y += ScreenPercentMargin.y * height * 0.01f;
		margin.x -= width / 2f;
		margin.y -= height / 2f;
		switch (m_horizontal)
		{
		case Horizontal.Left:
			num += margin.x;
			break;
		case Horizontal.Right:
			num += width + margin.x;
			break;
		case Horizontal.Center:
			num += width / 2f + margin.x;
			break;
		case Horizontal.None:
			num = m_transform.position.x;
			break;
		}
		switch (m_vertical)
		{
		case Vertical.Bottom:
			num2 += margin.y;
			break;
		case Vertical.Top:
			num2 += height + margin.y;
			break;
		case Vertical.Center:
			num2 += height / 2f + margin.y;
			break;
		case Vertical.None:
			num2 = m_transform.position.y;
			break;
		}
		float z = m_transform.position.z;
		m_transform.position = new Vector3(num, num2, z);
	}

	public void UpdateAlignment()
	{
		if (!(FindCamera()))
		{
			return;
		}
		UIRect rect = new UIRect();
		switch (m_mode)
		{
		case Mode.Screen:
			rect = GetScreenRect();
			break;
		case Mode.Parent:
			rect = GetParentRect();
			break;
		}
		UpdateAlignment(rect);
	}
}
