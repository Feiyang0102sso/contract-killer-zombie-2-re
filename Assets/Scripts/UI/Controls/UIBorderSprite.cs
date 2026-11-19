using UnityEngine;

public class UIBorderSprite : UIPackable
{
	public float BorderLeft;

	public float BorderRight;

	public float DesiredWidth;

	public bool CollapseOnSmallWidth = true;

	private Vector3[] _vertices;

	private int _numLines;

	private int _numSections;

	public void SetWidth(float width)
	{
		if (!(Width != width))
		{
			return;
		}
		Width = width;
		Vector3[] vertices = _mesh.vertices;
		_vertices.CopyTo(vertices, 0);
		CalculateVertices(vertices);
		UpdateAlign(vertices);
		_mesh.vertices = vertices;
		UpdateBounds(_mesh);
	}

	public void SetInternalWidth(float width)
	{
		width += BorderLeft + BorderRight;
		if (width < 0f)
		{
			width = 0f;
		}
		SetWidth(width);
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		vertices[0].x = 0f;
		vertices[1].x = 0f;
		float num = Width - BorderLeft - BorderRight;
		float num2 = Mathf.Max(0f, !(!(num < 0f) || !CollapseOnSmallWidth) ? (BorderLeft + num / 2f) : BorderLeft);
		float num3 = Mathf.Max(0f, !(!(num < 0f) || !CollapseOnSmallWidth) ? (BorderRight + num / 2f) : BorderRight);
		num = Mathf.Max(0f, num);
		vertices[2].x = vertices[0].x + num2;
		vertices[3].x = vertices[1].x + num2;
		vertices[4].x = vertices[2].x + num;
		vertices[5].x = vertices[3].x + num;
		vertices[6].x = vertices[4].x + num3;
		vertices[7].x = vertices[5].x + num3;
		for (int i = 0; i < _numLines; i++)
		{
			vertices[i * 2].y = 0f;
			vertices[i * 2 + 1].y = Height;
		}
	}

	protected override void CalculateTriangles(int[] triangles)
	{
		for (int i = 0; i < _numSections; i++)
		{
			int num = i * 3 * 2;
			int num2 = i * 2;
			triangles[num] = num2;
			triangles[num + 1] = num2 + 1;
			triangles[num + 2] = num2 + 2;
			triangles[num + 3] = num2 + 2;
			triangles[num + 4] = num2 + 1;
			triangles[num + 5] = num2 + 3;
		}
	}

	protected override Mesh CreateMesh()
	{
		_numVertices = 8;
		_numTriangles = 6;
		_numLines = _numVertices / 2;
		_numSections = _numLines - 1;
		Mesh mesh = base.CreateMesh();
		_vertices = mesh.vertices;
		return mesh;
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			SetWidth(DesiredWidth);
		}
	}
}
