using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("EZ GUI/Panels/Interactive Panel")]
public class UIInteractivePanel : UIPanelBase
{
	public enum STATE
	{
		NORMAL,
		OVER,
		DRAGGING
	}

	protected STATE m_panelState;

	[HideInInspector]
	public EZTransitionList transitions = new EZTransitionList(new EZTransition[9]
	{
		new EZTransition("Bring In Forward"),
		new EZTransition("Bring In Back"),
		new EZTransition("Dismiss Forward"),
		new EZTransition("Dismiss Back"),
		new EZTransition("Normal from Over"),
		new EZTransition("Normal from Dragging"),
		new EZTransition("Over from Normal"),
		new EZTransition("Over from Dragging"),
		new EZTransition("Dragging")
	});

	public bool draggable;

	public bool constrainDragArea;

	public Vector3 dragBoundaryMin;

	public Vector3 dragBoundaryMax;

	public STATE State
	{
		get
		{
			return m_panelState;
		}
	}

	public override EZTransitionList Transitions
	{
		get
		{
			return transitions;
		}
	}

	public override void OnInput(POINTER_INFO ptr)
	{
		if (!m_controlIsEnabled)
		{
			return;
		}
		if (inputDelegate != null)
		{
			inputDelegate(ref ptr);
		}
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.MOVE:
			if (m_panelState != STATE.OVER)
			{
				SetPanelState(STATE.OVER);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (draggable && !ptr.callerIsControl)
			{
				if (ptr.inputDelta.sqrMagnitude == 0f)
				{
					break;
				}
				float enter;
				Vector3 vector;
				Vector3 vector2;
				Plane plane = default(Plane);
				plane.SetNormalAndPosition(transform.forward * -1f, transform.position);
				plane.Raycast(ptr.ray, out enter);
				vector = ptr.ray.origin + ptr.ray.direction * enter;
				plane.Raycast(ptr.prevRay, out enter);
				vector2 = ptr.prevRay.origin + ptr.prevRay.direction * enter;
				vector = transform.position + (vector - vector2);
				if (constrainDragArea)
				{
					vector.x = Mathf.Clamp(vector.x, dragBoundaryMin.x, dragBoundaryMax.x);
					vector.y = Mathf.Clamp(vector.y, dragBoundaryMin.y, dragBoundaryMax.y);
					vector.z = Mathf.Clamp(vector.z, dragBoundaryMin.z, dragBoundaryMax.z);
				}
				transform.position = vector;
				SetPanelState(STATE.DRAGGING);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			if (collider != null)
			{
				RaycastHit hitInfo;
				if (!collider.Raycast(ptr.ray, out hitInfo, ptr.rayDepth))
				{
					SetPanelState(STATE.NORMAL);
				}
				else if (ptr.evt == POINTER_INFO.INPUT_EVENT.MOVE_OFF)
				{
					ptr.evt = POINTER_INFO.INPUT_EVENT.MOVE;
				}
				else
				{
					ptr.evt = POINTER_INFO.INPUT_EVENT.RELEASE;
				}
			}
			break;
		}
		base.OnInput(ptr);
	}

	protected void SetPanelState(STATE s)
	{
		if (!(m_panelState != s))
		{
			return;
		}
		STATE panelState = m_panelState;
		m_panelState = s;
		if (prevTransition != null)
		{
			prevTransition.StopSafe();
		}
		StartTransition(s, panelState);
	}

	protected void StartTransition(STATE s, STATE prevState)
	{
		int num;
		switch (s)
		{
		case STATE.NORMAL:
			if (prevState == STATE.OVER)
			{
				num = 4;
			}
			else
			{
				num = 5;
			}
			break;
		case STATE.OVER:
			if (prevState == STATE.NORMAL)
			{
				num = 6;
			}
			else
			{
				num = 7;
			}
			break;
		case STATE.DRAGGING:
			num = 8;
			break;
		default:
			num = 4;
			break;
		}
		prevTransition = transitions.list[num];
		prevTransition.Start();
	}

	public void Hide()
	{
		StartTransition(UIPanelManager.SHOW_MODE.DismissForward);
	}

	public void Reveal()
	{
		StartTransition(UIPanelManager.SHOW_MODE.BringInForward);
	}

	public static UIInteractivePanel Create(string name, Vector3 pos)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.position = pos;
		return (UIInteractivePanel)gameObject.AddComponent(typeof(UIInteractivePanel));
	}

	public static UIInteractivePanel Create(string name, Vector3 pos, Quaternion rotation)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.position = pos;
		gameObject.transform.rotation = rotation;
		return (UIInteractivePanel)gameObject.AddComponent(typeof(UIInteractivePanel));
	}
}
