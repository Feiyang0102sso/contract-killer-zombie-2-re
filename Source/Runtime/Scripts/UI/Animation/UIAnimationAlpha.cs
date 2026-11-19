using UnityEngine;

public class UIAnimationAlpha : UIAnimation
{
	public float From = 1f;

	public float To;

	public bool AllowHide = true;

	public bool AnimateInactive;

	private Renderer[] renderers;

	private SpriteRoot[] spriteRoots;

	private SimpleSprite[] simpleSprites;

	private UIPackable[] uiPackables;

	private SpriteText[] spriteTexts;

	private bool _precached;

	public UIAnimationAlpha Setup(float from, float to, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType, bool allowHide, bool animateInactive)
	{
		From = from;
		To = to;
		AllowHide = allowHide;
		AnimateInactive = animateInactive;
		return Setup(delay, duration, interpolatorType) as UIAnimationAlpha;
	}

	public UIAnimationAlpha Setup(float from, float to, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType, bool allowHide)
	{
		return Setup(from, to, delay, duration, interpolatorType, allowHide, false);
	}

	public UIAnimationAlpha Setup(float from, float to, float delay, float duration, EZAnimation.EASING_TYPE interpolatorType)
	{
		return Setup(from, to, delay, duration, interpolatorType, true);
	}

	protected override void Run()
	{
		_precached = false;
		base.Run();
	}

	private void PrecacheComponents()
	{
		if (_precached)
		{
			return;
		}
		renderers = GetComponentsInChildren<Renderer>(AnimateInactive);
		spriteRoots = GetComponentsInChildren<SpriteRoot>(AnimateInactive);
		simpleSprites = GetComponentsInChildren<SimpleSprite>(AnimateInactive);
		uiPackables = GetComponentsInChildren<UIPackable>(AnimateInactive);
		spriteTexts = GetComponentsInChildren<SpriteText>(AnimateInactive);
		_precached = true;
	}

	public override void Finish()
	{
		SetAlpha(To);
		base.Finish();
	}

	protected override void UpdateAnimation()
	{
		SetAlpha(_interpolator(_time, From, To - From, Duration));
	}

	private void SetAlpha(float a)
	{
		PrecacheComponents();
		float num = Mathf.Clamp01(a);
		bool flag = num <= 0.001f;
		if (AllowHide)
		{
			foreach (Renderer renderer in renderers)
			{
				if (renderer == null)
				{
					continue;
				}
				renderer.enabled = !flag;
			}
		}
		foreach (SpriteRoot spriteRoot in spriteRoots)
		{
			if (spriteRoot == null)
			{
				continue;
			}
			Color color = spriteRoot.Color;
			color.a = num;
			spriteRoot.SetColor(color);
		}
		foreach (SimpleSprite simpleSprite in simpleSprites)
		{
			if (simpleSprite == null)
			{
				continue;
			}
			Color color2 = simpleSprite.Color;
			color2.a = num;
			simpleSprite.SetColor(color2);
		}
		foreach (UIPackable uIPackable in uiPackables)
		{
			if (uIPackable == null)
			{
				continue;
			}
			Color color3 = uIPackable.Color;
			color3.a = num;
			uIPackable.Color = color3;
		}
		foreach (SpriteText spriteText in spriteTexts)
		{
			if (spriteText == null)
			{
				continue;
			}
			Color color4 = spriteText.Color;
			color4.a = num;
			spriteText.Color = color4;
		}
	}
}
