using System;
using UnityEngine;

public sealed class CutSceneCommandFollowUpPlayerCamera : CutSceneCommand
{
	public float Time = 3f;

	public float AimSpeedDegreePerSecond = 15f;

	public Vector3 AimPointOffset;

	private float m_InitialTime;

	public override void Begin()
	{
		base.Begin();
		m_InitialTime = TimeControl.RealTime;
		Scene.Camera.enabled = false;
	}

	public override bool Next()
	{
		float realTime = TimeControl.RealTime;
		if (realTime > m_InitialTime + Time)
		{
			return false;
		}
		Vector3 target = Target;
		float angle = GetAngle();
		if ((double)angle > 0.01)
		{
			float num = realTime - m_InitialTime;
			float num2 = AimSpeedDegreePerSecond * ((float)Math.PI / 180f);
			float val = num2 * num;
			float num3 = Math.Min(angle, val);
			Quaternion rotation = CameraTransform.rotation;
			CameraTransform.LookAt(target);
			Quaternion rotation2 = CameraTransform.rotation;
			CameraTransform.rotation = Quaternion.Slerp(rotation, rotation2, num3 / angle);
		}
		else
		{
			CameraTransform.LookAt(target);
		}
		return true;
	}

	private Transform CameraTransform
	{
		get
		{
			return Scene.Camera.transform;
		}
	}

	private Vector3 Target
	{
		get
		{
			return Scene.Player.transform.position + AimPointOffset;
		}
	}

	private float GetAngle()
	{
		Vector3 forward = CameraTransform.forward;
		Vector3 normalized = (Target - CameraTransform.position).normalized;
		float num = Vector3.Dot(forward, normalized);
		return (float)Math.Acos(num);
	}
}
