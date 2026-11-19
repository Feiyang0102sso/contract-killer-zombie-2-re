using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("UI/UIHorizontalScaleSprite")]
public class UIHorizontalScaleSprite : MonoBehaviour
{
	public enum SizeMode
	{
		FIXED,
		PERCENT
	}

	public enum AlignType
	{
		Left,
		Right
	}

	private const int _numVertices = 8;

	private const int _numTriangles = 6;

	private const int _numLines = 4;

	private const int _numSections = 3;

	public SizeMode Mode;

	public float ScreenWidth;

	public float Width;

	public float Height;

	public float BorderLeft = 5f;

	public float BorderRight = 5f;

	private float CenterWidth;

	public AlignType Align;

	public Vector2 TextureScale = new Vector2(1f, 1f);

	private Vector3[] _vertices;

	private Vector3[] _cachedVertices;

	private Mesh _mesh;

	private float _totalWidth;

	private Camera m_camera;

	public float GetTotalWidth()
	{
		return _totalWidth;
	}

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		_mesh = CreateMesh();
		MeshFilter component = GetComponent<MeshFilter>();
		component.mesh = _mesh;
		UpdateSize();
	}

	private void UpdateSize()
	{
		Camera camera = GUIUtils.UICamera();
		if ((bool)camera)
		{
			if (Mode == SizeMode.PERCENT)
			{
				Vector3 screenCorner = -2f * camera.ScreenToWorldPoint(Vector2.zero);
				SetTotalWidth(screenCorner.x * ScreenWidth / 100f);
			}
			else
			{
				SetTotalWidth(ScreenWidth);
			}
		}
	}

	public void SetWidth(float width)
	{
		if (!(CenterWidth != width))
		{
			return;
		}
		CenterWidth = width;
		_vertices.CopyTo(_cachedVertices, 0);
		CalculateVertices(_cachedVertices, width);
		CalculateAlign(_cachedVertices);
		_mesh.vertices = _cachedVertices;
	}

	public void SetTotalWidth(float width)
	{
		width -= BorderLeft + BorderRight;
		if (width < 0f)
		{
			width = 0f;
		}
		SetWidth(width);
	}

	private void CalculateAlign(Vector3[] vertices)
	{
		_totalWidth = CenterWidth + BorderLeft + BorderRight;
		if (Align == AlignType.Right)
		{
			int num = vertices.Length;
			for (int i = 0; i < num; i++)
			{
				vertices[i].x -= _totalWidth;
			}
		}
	}

	private void CalculateVertices(Vector3[] vertices, float width)
	{
		vertices[0].x = 0f;
		vertices[1].x = 0f;
		vertices[2].x = vertices[0].x + BorderLeft;
		vertices[3].x = vertices[1].x + BorderLeft;
		vertices[4].x = vertices[2].x + width;
		vertices[5].x = vertices[3].x + width;
		vertices[6].x = vertices[4].x + BorderRight;
		vertices[7].x = vertices[5].x + BorderRight;
	}

	private Mesh CreateMesh()
	{
		CenterWidth = Width;
		int[] array = new int[18];
		Vector2[] array2 = new Vector2[8];
		_vertices = new Vector3[8];
		_cachedVertices = new Vector3[8];
		for (int i = 0; i < 4; i++)
		{
			int num = i * 2;
			_vertices[num].x = 0f;
			_vertices[num].y = 0f;
			_vertices[num].z = 0f;
			_vertices[num + 1].x = 0f;
			_vertices[num + 1].y = Height;
			_vertices[num + 1].z = 0f;
		}
		CalculateVertices(_vertices, CenterWidth);
		float num2 = CenterWidth + BorderLeft + BorderRight;
		float height = Height;
		if (num2 * height != 0f)
		{
			num2 *= TextureScale.x;
			height *= TextureScale.y;
			for (int j = 0; j < 8; j++)
			{
				array2[j].x = _vertices[j].x / num2;
				array2[j].y = _vertices[j].y / height;
			}
		}
		CalculateAlign(_vertices);
		for (int k = 0; k < 3; k++)
		{
			int num3 = k * 3 * 2;
			int num4 = k * 2;
			array[num3] = num4;
			array[num3 + 1] = num4 + 1;
			array[num3 + 2] = num4 + 2;
			array[num3 + 3] = num4 + 2;
			array[num3 + 4] = num4 + 1;
			array[num3 + 5] = num4 + 3;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = _vertices;
		mesh.uv = array2;
		mesh.triangles = array;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return mesh;
	}
}
