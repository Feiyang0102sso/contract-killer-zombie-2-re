using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UICircularBar : UIPackable
{
	private const int SEGMENTS_COUNT = 20;

	public float InnerRadius = 100f;

	public float OuterRadius = 120f;

	private float _value = 1f;

	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = Mathf.Clamp01(value);
				UpdateMesh();
			}
		}
	}

	protected override void Awake()
	{
		_numVertices = 42;
		_numTriangles = 120;
		base.Awake();
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		float num = 0f;
		float num2 = (float)Math.PI * -2f * Value / 20f;
		Vector3 p = new Vector3(0f, InnerRadius, 0f);
		Vector3 p2 = new Vector3(0f, OuterRadius, 0f);
		for (int i = 0; i < _numVertices / 2; i++)
		{
			vertices[i * 2] = RotatePoint(p, num);
			vertices[i * 2 + 1] = RotatePoint(p2, num);
			num += num2;
		}
	}

	protected override void CalculateTriangles(int[] triangles)
	{
		int num = 0;
		for (int i = 0; i < _numTriangles / 6; i++)
		{
			int num2 = i * 2;
			triangles[num++] = num2;
			triangles[num++] = num2 + 1;
			triangles[num++] = num2 + 2;
			triangles[num++] = num2 + 2;
			triangles[num++] = num2 + 1;
			triangles[num++] = num2 + 3;
		}
	}

	protected override void CalculateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		Rect defaultUVs = DefaultUVs;
		float width = defaultUVs.width;
		float height = defaultUVs.height;
		float num = 0.05f;
		float num2 = 0f;
		int num3 = 0;
		while (num3 < _numVertices / 2)
		{
			uvs[num3 * 2] = new Vector2(num2 * width + defaultUVs.x, 1f * height + defaultUVs.y);
			uvs[num3 * 2 + 1] = new Vector2(num2 * width + defaultUVs.x, defaultUVs.y);
			num3++;
			num2 += num;
		}
	}

	private void UpdateMesh()
	{
		Vector3[] vertices = _mesh.vertices;
		CalculateVertices(vertices);
		_mesh.vertices = vertices;
	}

	private static Vector3 RotatePoint(Vector3 p, float angle)
	{
		float num = Mathf.Sin(angle);
		float num2 = Mathf.Cos(angle);
		return new Vector3(p.x * num2 - p.y * num, p.y * num2 + p.x * num, 0f);
	}
}
