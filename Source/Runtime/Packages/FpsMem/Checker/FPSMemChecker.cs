using UnityEngine;

public class FPSMemChecker : MonoBehaviour
{
	public enum AlignHorzEnum
	{
		LEFT,
		CENTER,
		RIGHT
	}

	public enum AlignVertEnum
	{
		UP,
		CENTER,
		DOWN
	}

	private const float TEXT_SCALE_COEFF = 2f;

	private const float RECT_WIDTH = 64f;

	private const float RECT_HEIGHT = 72f;

	public float UpdateInterval = 0.5f;

	private float _lastFPSTime;

	private int _frameCount;

	private int _fps;

	private int _curMemory;

	private Vector3 _scaleVector;

	private Texture2D _texture;

	private Rect _rect;

	public AlignHorzEnum AlignHorz;

	public AlignVertEnum AlignVert;

	private float _xPos;

	private float _yPos;

	private float _screenWidth;

	private float _screenHeight;

	public int FPS
	{
		get
		{
			return _fps;
		}
	}

	public int Memory
	{
		get
		{
			return _curMemory;
		}
	}

	private void CalculateRect()
	{
		_xPos = 0f;
		switch (AlignHorz)
		{
		case AlignHorzEnum.CENTER:
			_xPos = 0.5f * ((float)Screen.width - 64f);
			break;
		case AlignHorzEnum.RIGHT:
			_xPos = (float)Screen.width - 64f;
			break;
		}
		_yPos = 0f;
		switch (AlignVert)
		{
		case AlignVertEnum.CENTER:
			_yPos = 0.5f * ((float)Screen.height - 72f);
			break;
		case AlignVertEnum.DOWN:
			_yPos = (float)Screen.height - 72f;
			break;
		}
		_rect = new Rect(_xPos, _yPos, 64f, 72f);
		_screenWidth = Screen.width;
		_screenHeight = Screen.height;
	}

	private void Awake()
	{
		CalculateRect();
		_scaleVector = new Vector3(2f, 2f, 1f);
		_texture = new Texture2D(1, 1);
		_texture.SetPixel(0, 0, Color.black);
		_texture.Apply();
	}

	private void Start()
	{
		_lastFPSTime = Time.realtimeSinceStartup;
		_frameCount = 0;
		_fps = 0;
		_curMemory = 0;
	}

	private void Update()
	{
		_frameCount++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > _lastFPSTime + UpdateInterval)
		{
			_fps = (int)((float)_frameCount / (realtimeSinceStartup - _lastFPSTime));
			_lastFPSTime = realtimeSinceStartup;
			_frameCount = 0;
			_curMemory = NativeUtils.GetCurrentMemoryBytes() >> 20;
		}
		if (_screenHeight != (float)Screen.height || _screenWidth != (float)Screen.width)
		{
			CalculateRect();
		}
	}

	private void OnGUI()
	{
		GUI.DrawTexture(_rect, _texture);
		Color contentColor = GUI.contentColor;
		GUI.contentColor = Color.magenta;
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(new Vector3(_xPos, _yPos, 0f), Quaternion.identity, _scaleVector);
		string text = _fps.ToString("00") + "\n" + _curMemory.ToString("000");
		GUILayout.Label(text);
		GUI.matrix = matrix;
		GUI.contentColor = contentColor;
	}
}
