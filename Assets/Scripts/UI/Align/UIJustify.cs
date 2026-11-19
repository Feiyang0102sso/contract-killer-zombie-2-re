using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("UI/UIJustify")]
public class UIJustify : MonoBehaviour
{
	public enum Mode
	{
		Screen,
		Parent
	}

	[Serializable]
	public class Margin
	{
		public float m_left;

		public float m_top;

		public float m_right;

		public float m_bottom;
	}

	private Camera m_camera;

	private Transform m_transform;

	public bool m_horizontal;

	public bool m_vertical;

	public bool ChangePosition = true;

	public float PercentHorizontal = 100f;

	public float PercentVertical = 100f;

	private SpriteRoot m_root;

	public Mode m_mode;

	public Margin m_margin;

	private void Awake()
	{
		m_transform = GetComponent<Transform>();
		m_root = GetComponent<SpriteRoot>();
	}

	private void Start()
	{
		if (enabled)
		{
			UpdateJustification();
		}
	}

	private void OnEnable()
	{
		UpdateJustification();
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
		float x = 0f;
		float y = 0f;
		float width = 0f;
		float height = 0f;
		Transform parent = m_transform.parent;
		if (parent != null)
		{
			SpriteRoot component = parent.GetComponent<SpriteRoot>();
			if (component != null)
			{
				Vector3 centerPoint = component.GetCenterPoint();
				x = centerPoint.x;
				y = centerPoint.y;
				width = component.width;
				height = component.height;
			}
		}
		return new UIRect(x, y, width, height);
	}

	public void UpdateJustification(UIRect rect)
	{
		float x = rect.m_x;
		float y = rect.m_y;
		float num = m_root.width;
		float num2 = m_root.height;
		if (m_horizontal)
		{
			num = rect.m_width - m_margin.m_left - m_margin.m_right;
			x += (m_margin.m_left - m_margin.m_right) / 2f;
		}
		else
		{
			x = m_transform.position.x;
		}
		if (m_vertical)
		{
			num2 = rect.m_height - m_margin.m_top - m_margin.m_bottom;
			y += (m_margin.m_bottom - m_margin.m_top) / 2f;
		}
		else
		{
			y = m_transform.position.y;
		}
		if (num <= 0f)
		{
			num = m_root.width;
		}
		if (num2 <= 0f)
		{
			num2 = m_root.height;
		}
		float z = m_transform.position.z;
		if (ChangePosition)
		{
			m_transform.position = new Vector3(x, y, z);
		}
		m_root.SetSize(num * PercentHorizontal * 0.01f, num2 * PercentVertical * 0.01f);
	}

	public void UpdateJustification()
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
		UpdateJustification(rect);
	}
}
