using System;
using UnityEngine;

public class BlinkVertexColor : MonoBehaviour
{
	private MeshFilter _filter;

	public float Freq = 2f;

	public float VertexPosTimeShiftCoeff = 1f;

	public float TimeShift;

	public Vector2 TimeShiftDir = Vector2.one;

	public bool RandomizeTimeShift = true;

	public int BlinkEvery = 4;

	public float AlphaFrom = 1f;

	public float AlphaTo;

	private void Awake()
	{
		_filter = gameObject.GetComponent<MeshFilter>();
		if (RandomizeTimeShift)
		{
			TimeShift = UnityEngine.Random.Range(0f, 1000f);
		}
	}

	private void Update()
	{
		Color[] colors = _filter.mesh.colors;
		Vector3 center = _filter.mesh.bounds.center;
		for (int i = 0; i < _filter.mesh.colors.Length; i++)
		{
			float num = Vector2.Dot(_filter.mesh.vertices[i] - center, TimeShiftDir) * VertexPosTimeShiftCoeff;
			Color color = _filter.mesh.colors[i];
			float num2 = 1f / Freq;
			float num3 = Time.realtimeSinceStartup + TimeShift + num;
			color.a = (!(Mathf.RoundToInt(num3 / num2) % BlinkEvery != 0) ? Mathf.Lerp(AlphaFrom, AlphaTo, Mathf.Sin(num3 * (float)Math.PI * 2f / num2)) : AlphaFrom);
			colors[i] = color;
		}
		_filter.mesh.colors = colors;
	}
}
