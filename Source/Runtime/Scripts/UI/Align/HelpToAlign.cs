using System;
using UnityEngine;

[ExecuteInEditMode]
public class HelpToAlign : MonoBehaviour
{
	public enum AlignType
	{
		CIRCLE
	}

	public float shiftAngle;

	public float Radius = 30f;

	public float Z;

	public AlignType Type;

	private void Update()
	{
		if (Type == AlignType.CIRCLE)
		{
			float num = (float)Math.PI * 2f / (float)transform.childCount;
			float num2 = (float)Math.PI / 180f * shiftAngle;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				child.localPosition = new Vector3(Radius * Mathf.Cos(num2), Radius * Mathf.Sin(num2), Z);
				num2 += num;
			}
		}
	}
}
