using System.Linq;
using UnityEngine;

public class CutSceneCommandPushBox : CutSceneCommand
{
	private const string start = "box_move1";

	private const string loop = "box_move2";

	private const float speed = 0.5f;

	public int SurvivorIndex;

	public GameObject Box;

	private Character m_Character;

	private HumanWeapon m_Weapon;

	private GameObject m_Mount;

	private bool moving;

	private float m_InitialTime;

	private Vector3 m_InitialDelta;

	private Vector3 m_InitialPosition;

	private Quaternion m_InitialRotation;

	private float m_InvTime;

	public override void Begin()
	{
		Survivor[] array = Scene.Survivors.ToArray();
		m_Character = (!(SurvivorIndex >= array.Length) ? array[SurvivorIndex] : null);
		m_Character.Speed = 0f;
		m_Weapon = m_Character.GetComponent<HumanWeapon>();
		if (m_Weapon != null)
		{
			m_Weapon.ShowWeapon(false);
		}
		m_Mount = m_Character.FindChildInHierarchy("weapon1");
		m_Character.GetComponent<Animation>()["box_move1"].layer = 100;
		m_Character.GetComponent<Animation>()["box_move1"].wrapMode = WrapMode.Once;
		m_Character.GetComponent<Animation>()["box_move2"].layer = 100;
		m_Character.GetComponent<Animation>()["box_move2"].wrapMode = WrapMode.Loop;
		m_Character.GetComponent<Animation>().CrossFade("box_move1");
		m_Character.GetComponent<Animation>().CrossFadeQueued("box_move2");
		Box.BroadcastMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
	}

	public override void End()
	{
		m_Character.GetComponent<Animation>().Stop("box_move2");
		if (m_Weapon != null)
		{
			m_Weapon.ShowWeapon(true);
		}
		Box.GetComponentsInChildren<Effect>().ToList().ForEach((Effect e) =>
		{
			Object.Destroy(e.gameObject);
		});
		base.End();
	}

	public override bool Next()
	{
		if (!moving && !m_Character.GetComponent<Animation>().IsPlaying("box_move1"))
		{
			moving = true;
			m_InitialTime = TimeControl.RealTime;
			m_InitialDelta = GetDelta();
			m_InitialPosition = m_Character.transform.position;
			m_InitialRotation = m_Character.transform.rotation;
			m_InvTime = 0.5f / m_InitialDelta.magnitude;
			return true;
		}
		if (moving)
		{
			float num = TimeControl.RealTime - m_InitialTime;
			m_Character.transform.position = Vector3.Lerp(m_InitialPosition, transform.position, num * m_InvTime);
			m_Character.transform.rotation = transform.rotation;
			Vector3 vector = m_Character.transform.position + m_Character.transform.forward * Vector3.Dot(m_Mount.transform.position - m_Character.transform.position, m_Character.transform.forward);
			Box.transform.position = new Vector3(vector.x, Box.transform.position.y, vector.z);
			Box.transform.rotation = Quaternion.Euler(Box.transform.eulerAngles.x, m_Character.transform.eulerAngles.y, Box.transform.eulerAngles.z);
			return (double)Vector3.Dot(m_InitialDelta, GetDelta()) >= 0.01;
		}
		m_Character.transform.rotation = Quaternion.Slerp(m_InitialRotation, transform.rotation, m_Character.GetComponent<Animation>()["box_move1"].normalizedTime);
		return true;
	}

	private Vector3 GetDelta()
	{
		return Util.ZeroY(transform.position - m_Character.transform.position);
	}
}
