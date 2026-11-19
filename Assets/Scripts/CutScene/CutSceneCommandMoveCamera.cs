using UnityEngine;

public sealed class CutSceneCommandMoveCamera : CutSceneCommand
{
	public float Speed = 3f;

	private float m_InitialTime;

	private Vector3 m_InitialCameraPosition;

	private Quaternion m_InitialCameraRotation;

	private float m_InvTime;

	private float m_Time;

	public override void Begin()
	{
		m_InitialTime = TimeControl.RealTime;
		m_InitialCameraPosition = Scene.Camera.transform.position;
		m_InitialCameraRotation = Scene.Camera.transform.rotation;
		m_Time = (transform.position - m_InitialCameraPosition).magnitude / Speed;
		m_InvTime = 1f / m_Time;
		Scene.Camera.enabled = false;
	}

	public override bool Next()
	{
		if ((double)m_Time < 0.001)
		{
			Interpolate(1f);
			return false;
		}
		float num = TimeControl.RealTime - m_InitialTime;
		if (num >= m_Time)
		{
			Interpolate(1f);
			return false;
		}
		Interpolate((float)(num * m_InvTime));
		return true;
	}

	private void Interpolate(float t)
	{
		Transform transform = Scene.Camera.transform;
		transform.rotation = Quaternion.Slerp(m_InitialCameraRotation, base.transform.rotation, t);
		transform.position = Vector3.Lerp(m_InitialCameraPosition, base.transform.position, t);
	}
}
