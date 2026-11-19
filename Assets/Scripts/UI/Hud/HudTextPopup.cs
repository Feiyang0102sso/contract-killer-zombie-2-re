using UnityEngine;

public class HudTextPopup : MonoBehaviour
{
	private SpriteText _label;

	public UISingleSprite Icon;

	private void Awake()
	{
		_label = GetComponent<SpriteText>();
		gameObject.active = false;
	}

	public void Run(string text)
	{
		Run(text, null);
	}

	public void Run(string text, Texture2D icon)
	{
		if (gameObject.active)
		{
			Stop();
		}
		_label.Text = text;
		if (Icon != null)
		{
			Icon.SetTexture(icon);
			Icon.gameObject.active = icon != null;
			Icon.transform.localPosition = Vector3.right * _label.BottomRight.x;
		}
		Run();
	}

	private void Run()
	{
		gameObject.active = true;
		foreach (UIAnimation uIAnimation in GetComponents<UIAnimation>())
		{
			uIAnimation.RunAnimation();
		}
	}

	private void Stop()
	{
		foreach (UIAnimation uIAnimation in GetComponents<UIAnimation>())
		{
			uIAnimation.Finish();
		}
	}
}
