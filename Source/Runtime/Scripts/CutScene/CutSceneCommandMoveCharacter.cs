using UnityEngine;

public abstract class CutSceneCommandMoveCharacter : CutSceneCommand
{
	public float Speed = 3f;

	private Character m_Character;

	private float m_InitialTime;

	private Vector3 m_InitialDelta;

	private Quaternion m_InitialRotation;

	private float m_InvTime;

	public override void Begin()
	{
		m_Character = Character;
		if (m_Character != null)
		{
			m_InitialTime = TimeControl.RealTime;
			m_InitialDelta = GetDelta();
			m_InitialRotation = m_Character.transform.rotation;
			m_InvTime = Speed / m_InitialDelta.magnitude;
			m_Character.Direction = m_InitialDelta.normalized;
			m_Character.Speed = Speed;
		}
	}

	public override void End()
	{
		base.End();
		if (m_Character != null)
		{
			m_Character.Speed = 0f;
		}
	}

	public override bool Next()
	{
		if (m_Character == null)
		{
			return false;
		}
		float num = TimeControl.RealTime - m_InitialTime;
		m_Character.transform.rotation = Quaternion.Slerp(m_InitialRotation, transform.rotation, num * m_InvTime);
		return (double)Vector3.Dot(m_InitialDelta, GetDelta()) >= 0.01;
	}

	protected abstract Character Character { get; }

	private Vector3 GetDelta()
	{
		return ProjectToXZ(transform.position) - ProjectToXZ(m_Character.transform.position);
	}

	private static Vector3 ProjectToXZ(Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}
}
