using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[AddComponentMenu("EZ GUI/Controls/List Item Container")]
public class UIListItemContainer : ControlBase, IUIListObject, IEZDragDrop, IUIContainer, IUIObject
{
	protected List<SpriteRoot> uiObjs = new List<SpriteRoot>();

	protected List<SpriteText> textObjs = new List<SpriteText>();

	protected bool m_started;

	protected Camera renderCamera;

	private Vector2 topLeftEdge;

	private Vector2 bottomRightEdge;

	private Rect3D clippingRect;

	private bool clipped;

	private UIScrollList list;

	protected int index;

	private bool m_selected;

	public override void Start()
	{
		if (m_started)
		{
			return;
		}
		m_started = true;
		ScanChildren();
	}

	public void ScanChildren()
	{
		uiObjs.Clear();
		SpriteRoot spriteRoot;
		Component[] componentsInChildren = transform.GetComponentsInChildren(typeof(SpriteRoot), true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] == this)
			{
				continue;
			}
			if (gameObject.layer == UIManager.instance.gameObject.layer)
			{
				UIPanelManager.SetLayerRecursively(componentsInChildren[i].gameObject, gameObject.layer);
			}
			spriteRoot = (SpriteRoot)componentsInChildren[i];
			if (spriteRoot is AutoSpriteControlBase)
			{
				if (((AutoSpriteControlBase)spriteRoot).RequestContainership(this))
				{
					uiObjs.Add(spriteRoot);
				}
			}
			else
			{
				uiObjs.Add(spriteRoot);
			}
			if (container != null)
			{
				container.AddSubject(spriteRoot.gameObject);
			}
			if (renderCamera != null)
			{
				spriteRoot.renderCamera = renderCamera;
			}
		}
		componentsInChildren = transform.GetComponentsInChildren(typeof(ControlBase), true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (gameObject.layer == UIManager.instance.gameObject.layer)
			{
				UIPanelManager.SetLayerRecursively(componentsInChildren[j].gameObject, gameObject.layer);
			}
			((ControlBase)componentsInChildren[j]).RequestContainership(this);
			if (container != null)
			{
				container.AddSubject(componentsInChildren[j].gameObject);
			}
		}
		textObjs.Clear();
		SpriteText spriteText;
		Component[] componentsInChildren2 = transform.GetComponentsInChildren(typeof(SpriteText), true);
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			if (componentsInChildren2[k] == this)
			{
				continue;
			}
			spriteText = (SpriteText)componentsInChildren2[k];
			if (spriteText.Parent != null)
			{
				continue;
			}
			if (gameObject.layer == UIManager.instance.gameObject.layer)
			{
				UIPanelManager.SetLayerRecursively(spriteText.gameObject, gameObject.layer);
			}
			textObjs.Add(spriteText);
			if (container != null)
			{
				container.AddSubject(spriteText.gameObject);
			}
			if (renderCamera != null)
			{
				spriteText.renderCamera = renderCamera;
			}
		}
	}

	public void AddChild(GameObject go)
	{
		SpriteRoot spriteRoot = (SpriteRoot)go.GetComponent(typeof(SpriteRoot));
		if (spriteRoot != null)
		{
			if (spriteRoot is AutoSpriteControlBase)
			{
				if (((AutoSpriteControlBase)spriteRoot).Container != this)
				{
					((AutoSpriteControlBase)spriteRoot).Container = this;
				}
				if (container != null)
				{
					container.AddSubject(go);
				}
			}
			uiObjs.Add(spriteRoot);
		}
		else
		{
			SpriteText spriteText = (SpriteText)go.GetComponent(typeof(SpriteText));
			if (spriteText != null)
			{
				textObjs.Add(spriteText);
				if (container != null)
				{
					container.AddSubject(go);
				}
			}
		}
	}

	public void RemoveChild(GameObject go)
	{
		SpriteRoot spriteRoot = (SpriteRoot)go.GetComponent(typeof(SpriteRoot));
		if (spriteRoot != null)
		{
			for (int i = 0; i < uiObjs.Count; i++)
			{
				if (uiObjs[i] == spriteRoot)
				{
					uiObjs.RemoveAt(i);
					break;
				}
			}
			if (spriteRoot is AutoSpriteControlBase && ((AutoSpriteControlBase)spriteRoot).Container == this)
			{
				((AutoSpriteControlBase)spriteRoot).Container = null;
			}
			if (container != null)
			{
				container.RemoveSubject(go);
			}
		}
		else
		{
			SpriteText spriteText = (SpriteText)go.GetComponent(typeof(SpriteText));
			if (spriteText != null)
			{
				for (int j = 0; j < textObjs.Count; j++)
				{
					if (textObjs[j] == spriteText)
					{
						textObjs.RemoveAt(j);
						break;
					}
				}
				if (container != null)
				{
					container.RemoveSubject(go);
				}
			}
		}
	}

	public void AddSubject(GameObject go)
	{
	}

	public void RemoveSubject(GameObject go)
	{
	}

	public override IUIContainer Container
	{
		get
		{
			return base.Container;
		}
		set
		{
			if (container != null)
			{
				for (int i = 0; i < uiObjs.Count; i++)
				{
					container.RemoveSubject(uiObjs[i].gameObject);
				}
				for (int j = 0; j < textObjs.Count; j++)
				{
					container.RemoveSubject(textObjs[j].gameObject);
				}
			}
			if (value != null)
			{
				for (int k = 0; k < uiObjs.Count; k++)
				{
					value.AddSubject(uiObjs[k].gameObject);
				}
				for (int l = 0; l < textObjs.Count; l++)
				{
					value.AddSubject(textObjs[l].gameObject);
				}
			}
			base.Container = value;
		}
	}

	public void MakeChild(GameObject go)
	{
		go.transform.parent = transform;
		AddChild(go);
	}

	public SpriteRoot GetElement(string elementName)
	{
		if (!m_started)
		{
			Start();
		}
		for (int i = 0; i < uiObjs.Count; i++)
		{
			if (uiObjs[i].name == elementName)
			{
				return uiObjs[i];
			}
		}
		return null;
	}

	public SpriteText GetTextElement(string elementName)
	{
		if (!m_started)
		{
			Start();
		}
		for (int i = 0; i < textObjs.Count; i++)
		{
			if (textObjs[i].name == elementName)
			{
				return textObjs[i];
			}
		}
		return null;
	}

	public override void OnInput(POINTER_INFO ptr)
	{
		if (deleted)
		{
			return;
		}
		if (!m_controlIsEnabled)
		{
			switch (ptr.evt)
			{
			case POINTER_INFO.INPUT_EVENT.NO_CHANGE:
				if (list != null && ptr.active)
				{
					list.ListDragged(ptr);
				}
				break;
			case POINTER_INFO.INPUT_EVENT.DRAG:
				if (list != null && !ptr.isTap)
				{
					list.ListDragged(ptr);
				}
				break;
			case POINTER_INFO.INPUT_EVENT.RELEASE:
			case POINTER_INFO.INPUT_EVENT.TAP:
			case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
				if (list != null)
				{
					list.PointerReleased();
				}
				break;
			}
			if (list != null && ptr.inputDelta.z != 0f && ptr.type != POINTER_INFO.POINTER_TYPE.RAY)
			{
				list.ScrollWheel(ptr.inputDelta.z);
			}
			if (Container != null)
			{
				ptr.callerIsControl = true;
				Container.OnInput(ptr);
			}
			return;
		}
		if (list != null && Vector3.SqrMagnitude(ptr.origPos - ptr.devicePos) > list.dragThreshold * list.dragThreshold)
		{
			ptr.isTap = false;
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
			{
				ptr.evt = POINTER_INFO.INPUT_EVENT.RELEASE;
			}
		}
		else
		{
			ptr.isTap = true;
		}
		if (inputDelegate != null)
		{
			inputDelegate(ref ptr);
		}
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.NO_CHANGE:
			if (list != null && ptr.active)
			{
				list.ListDragged(ptr);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (list != null && !ptr.isTap)
			{
				list.ListDragged(ptr);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (list == null)
			{
				break;
			}
			if (ptr.callerIsControl)
			{
				list.DidClick(ptr.targetObj);
			}
			list.PointerReleased();
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			if (list != null)
			{
				list.PointerReleased();
			}
			break;
		}
		if (list != null && ptr.inputDelta.z != 0f && ptr.type != POINTER_INFO.POINTER_TYPE.RAY)
		{
			list.ScrollWheel(ptr.inputDelta.z);
		}
		base.OnInput(ptr);
	}

	public override EZTransitionList GetTransitions(int index)
	{
		return null;
	}

	public override EZTransitionList[] Transitions
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public override string[] States
	{
		get
		{
			return null;
		}
	}

	public bool IsContainer()
	{
		return true;
	}

	public void FindOuterEdges()
	{
		if (!m_started)
		{
			Start();
		}
		topLeftEdge = Vector2.zero;
		bottomRightEdge = Vector2.zero;
		Matrix4x4 localToWorldMatrix;
		Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
		Vector3 vector;
		Vector3 vector2;
		if (spriteText != null)
		{
			spriteText.Start();
			localToWorldMatrix = spriteText.transform.localToWorldMatrix;
			vector = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(spriteText.UnclippedTopLeft));
			vector2 = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(spriteText.UnclippedBottomRight));
			topLeftEdge.x = Mathf.Min(topLeftEdge.x, vector.x);
			topLeftEdge.y = Mathf.Max(topLeftEdge.y, vector.y);
			bottomRightEdge.x = Mathf.Max(bottomRightEdge.x, vector2.x);
			bottomRightEdge.y = Mathf.Min(bottomRightEdge.y, vector2.y);
		}
		for (int i = 0; i < textObjs.Count; i++)
		{
			textObjs[i].Start();
			localToWorldMatrix = textObjs[i].transform.localToWorldMatrix;
			vector = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(textObjs[i].UnclippedTopLeft));
			vector2 = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(textObjs[i].UnclippedBottomRight));
			topLeftEdge.x = Mathf.Min(topLeftEdge.x, vector.x);
			topLeftEdge.y = Mathf.Max(topLeftEdge.y, vector.y);
			bottomRightEdge.x = Mathf.Max(bottomRightEdge.x, vector2.x);
			bottomRightEdge.y = Mathf.Min(bottomRightEdge.y, vector2.y);
		}
		for (int j = 0; j < uiObjs.Count; j++)
		{
			localToWorldMatrix = uiObjs[j].transform.localToWorldMatrix;
			if (uiObjs[j] is AutoSpriteControlBase)
			{
				((AutoSpriteControlBase)uiObjs[j]).FindOuterEdges();
				vector = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(((AutoSpriteControlBase)uiObjs[j]).TopLeftEdge));
				vector2 = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(((AutoSpriteControlBase)uiObjs[j]).BottomRightEdge));
			}
			else
			{
				vector = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(uiObjs[j].UnclippedTopLeft));
				vector2 = worldToLocalMatrix.MultiplyPoint3x4(localToWorldMatrix.MultiplyPoint3x4(uiObjs[j].UnclippedBottomRight));
			}
			topLeftEdge.x = Mathf.Min(topLeftEdge.x, vector.x);
			topLeftEdge.y = Mathf.Max(topLeftEdge.y, vector.y);
			bottomRightEdge.x = Mathf.Max(bottomRightEdge.x, vector2.x);
			bottomRightEdge.y = Mathf.Min(bottomRightEdge.y, vector2.y);
		}
	}

	public Vector2 TopLeftEdge
	{
		get
		{
			return topLeftEdge;
		}
	}

	public Vector2 BottomRightEdge
	{
		get
		{
			return bottomRightEdge;
		}
	}

	public void Hide(bool tf)
	{
		for (int i = 0; i < uiObjs.Count; i++)
		{
			uiObjs[i].Hide(tf);
		}
		for (int j = 0; j < textObjs.Count; j++)
		{
			textObjs[j].Hide(tf);
		}
		if (spriteText != null)
		{
			spriteText.Hide(tf);
		}
	}

	public bool Managed
	{
		get
		{
			return false;
		}
	}

	public Rect3D ClippingRect
	{
		get
		{
			return clippingRect;
		}
		set
		{
			clipped = true;
			clippingRect = value;
			for (int i = 0; i < uiObjs.Count; i++)
			{
				uiObjs[i].ClippingRect = value;
			}
			for (int j = 0; j < textObjs.Count; j++)
			{
				textObjs[j].ClippingRect = value;
			}
			if (spriteText != null)
			{
				spriteText.ClippingRect = value;
			}
		}
	}

	public bool Clipped
	{
		get
		{
			return clipped;
		}
		set
		{
			if (value && !clipped)
			{
				clipped = true;
				ClippingRect = clippingRect;
			}
			else if (clipped)
			{
				Unclip();
			}
			clipped = value;
		}
	}

	public void Unclip()
	{
		clipped = false;
		for (int i = 0; i < uiObjs.Count; i++)
		{
			uiObjs[i].Unclip();
		}
		for (int j = 0; j < textObjs.Count; j++)
		{
			textObjs[j].Unclip();
		}
		if (spriteText != null)
		{
			spriteText.Unclip();
		}
	}

	public override void UpdateCollider()
	{
		for (int i = 0; i < uiObjs.Count; i++)
		{
			if (uiObjs[i] is AutoSpriteControlBase)
			{
				((AutoSpriteControlBase)uiObjs[i]).UpdateCollider();
			}
		}
	}

	public void SetList(UIScrollList c)
	{
		list = c;
	}

	public virtual UIScrollList GetScrollList()
	{
		return list;
	}

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public override string Text
	{
		set
		{
			base.Text = value;
			FindOuterEdges();
			if (spriteText != null && spriteText.maxWidth > 0f && list != null)
			{
				list.PositionItems();
			}
		}
	}

	public SpriteText TextObj
	{
		get
		{
			return spriteText;
		}
	}

	public bool selected
	{
		get
		{
			return m_selected;
		}
		set
		{
			m_selected = value;
		}
	}

	public void Delete()
	{
		for (int i = 0; i < uiObjs.Count; i++)
		{
			uiObjs[i].Delete();
		}
		for (int j = 0; j < textObjs.Count; j++)
		{
			textObjs[j].Delete();
		}
	}

	public Camera RenderCamera
	{
		get
		{
			return renderCamera;
		}
		set
		{
			renderCamera = value;
			for (int i = 0; i < uiObjs.Count; i++)
			{
				uiObjs[i].RenderCamera = value;
			}
			for (int j = 0; j < textObjs.Count; j++)
			{
				textObjs[j].RenderCamera = value;
			}
			if (spriteText != null)
			{
				spriteText.RenderCamera = value;
			}
		}
	}

	public void UpdateCamera()
	{
		for (int i = 0; i < uiObjs.Count; i++)
		{
			uiObjs[i].UpdateCamera();
		}
		for (int j = 0; j < textObjs.Count; j++)
		{
			textObjs[j].UpdateCamera();
		}
		if (spriteText != null)
		{
			spriteText.UpdateCamera();
		}
	}
}
