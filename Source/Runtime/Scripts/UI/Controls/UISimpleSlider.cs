using System;
using UnityEngine;

public class UISimpleSlider : MonoBehaviour, IEZDragDrop, IUIObject
{
	public UIBorderSprite Back;

	public PackedSprite Knob;

	public float AdditionalWidth;

	public float damping = 5f;

	private bool _dragging;

	private bool _dumping;

	private Vector3 _pos = Vector3.zero;

	private Vector3 _touchPos = Vector3.zero;

	private EZValueChangedDelegate _onValueChanged;

	private void Awake()
	{
		if (gameObject.GetComponent<BoxCollider>() == null)
		{
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(Back.DesiredWidth, Back.Height, 0.1f);
		}
	}

	public virtual void OnInput(POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_dumping = true;
			_touchPos = ptr.ray.origin;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			_dragging = true;
			_dumping = false;
			_touchPos = ptr.ray.origin;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			_touchPos = ptr.ray.origin;
			_dragging = false;
			_dumping = true;
			break;
		}
	}

	private void Update()
	{
		if (_dragging || _dumping)
		{
			// PC port: real time via PcFrameRate.RealDeltaTime; deltaTime / timeScale jumped by 333 s on load frames
			float num = PcFrameRate.RealDeltaTime * damping;
			if (_dragging)
			{
				num *= 2f;
			}
			_pos.x = Mathf.Lerp(_pos.x, _touchPos.x, num);
			SetValue((_pos.x - (Back.transform.position.x - BackWidth * 0.5f)) / BackWidth);
			if (_onValueChanged != null)
			{
				_onValueChanged(this);
			}
		}
	}

	private float BackWidth
	{
		get
		{
			return Back.DesiredWidth - Knob.width + AdditionalWidth;
		}
	}

	public float Value
	{
		get
		{
			return (Knob.transform.localPosition.x + BackWidth * 0.5f) / BackWidth;
		}
		set
		{
			_dumping = false;
			_pos.x = value * BackWidth + (Back.transform.position.x - BackWidth * 0.5f);
			SetValue(value);
		}
	}

	private void SetValue(float value)
	{
		Vector3 localPosition = Knob.transform.localPosition;
		localPosition.x = Mathf.Clamp01(value) * BackWidth - BackWidth * 0.5f;
		Knob.transform.localPosition = localPosition;
	}

	public object Data { get; set; }

	public virtual bool controlIsEnabled { get; set; }

	public virtual bool DetargetOnDisable { get; set; }

	public IUIObject GetControl(ref POINTER_INFO ptr)
	{
		return this;
	}

	public virtual IUIContainer Container { get; set; }

	public bool RequestContainership(IUIContainer cont)
	{
		return false;
	}

	public virtual bool GotFocus()
	{
		return false;
	}

	public virtual void SetInputDelegate(EZInputDelegate del)
	{
	}

	public virtual void AddInputDelegate(EZInputDelegate del)
	{
	}

	public virtual void RemoveInputDelegate(EZInputDelegate del)
	{
	}

	public virtual void SetValueChangedDelegate(EZValueChangedDelegate del)
	{
		_onValueChanged = del;
	}

	public virtual void AddValueChangedDelegate(EZValueChangedDelegate del)
	{
		_onValueChanged += del;
	}

	public virtual void RemoveValueChangedDelegate(EZValueChangedDelegate del)
	{
		_onValueChanged -= del;
	}

	public LayerMask DropMask { get; set; }

	public bool IsDraggable { get; set; }

	public float DragOffset { get; set; }

	public EZAnimation.EASING_TYPE CancelDragEasing { get; set; }

	public float CancelDragDuration { get; set; }

	public bool IsDragging { get; set; }

	public GameObject DropTarget { get; set; }

	public bool DropHandled { get; set; }

	public void DragUpdatePosition(POINTER_INFO ptr)
	{
	}

	public void CancelDrag()
	{
	}

	public void OnEZDragDrop_Internal(EZDragDropParams parms)
	{
	}

	public void AddDragDropDelegate(EZDragDropDelegate del)
	{
	}

	public void RemoveDragDropDelegate(EZDragDropDelegate del)
	{
	}

	public void SetDragDropDelegate(EZDragDropDelegate del)
	{
	}

	public void SetDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public void AddDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public void RemoveDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public EZDragDropHelper.DragDrop_InternalDelegate GetDragDropInternalDelegate()
	{
		return null;
	}
}
