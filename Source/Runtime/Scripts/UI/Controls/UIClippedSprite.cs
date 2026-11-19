using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UIClippedSprite : UIPackable
{
	public Vector2 UVFrom = Vector2.zero;

	public Vector2 UVTo = Vector2.one;

	protected override void Awake()
	{
		if (Application.isPlaying)
		{
			Width = Width * (UVTo.x - UVFrom.x);
			Height = Height * (UVTo.y - UVFrom.y);
		}
		base.Awake();
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		vertices[0].x = 0f;
		vertices[0].y = Height;
		vertices[1].x = 0f;
		vertices[1].y = 0f;
		vertices[2].x = Width;
		vertices[2].y = 0f;
		vertices[3].x = Width;
		vertices[3].y = Height;
	}

	protected override void CalculateTriangles(int[] triangles)
	{
		triangles[0] = 0;
		triangles[1] = 3;
		triangles[2] = 1;
		triangles[3] = 3;
		triangles[4] = 2;
		triangles[5] = 1;
	}

	protected override void CalculateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		if (!(Width * Height != 0f))
		{
			return;
		}
		Rect defaultUVs = DefaultUVs;
		float num = defaultUVs.width * (UVTo.x - UVFrom.x);
		float num2 = defaultUVs.height * (UVTo.y - UVFrom.y);
		float num3 = defaultUVs.x + defaultUVs.width * UVFrom.x;
		float num4 = defaultUVs.y + defaultUVs.height * UVFrom.y;
		for (int i = 0; i < _numVertices; i++)
		{
			uvs[i].x = vertices[i].x / Width * num + num3;
			uvs[i].y = vertices[i].y / Height * num2 + num4;
		}
	}
}
