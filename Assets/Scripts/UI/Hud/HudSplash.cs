using UnityEngine;

public class HudSplash : MonoBehaviour
{
	public static int CurrentSplashCount = 0;

	private UIAlign _align;

	private PackedSprite _sprite;

	private bool _isRunning;

	private void Awake()
	{
		_align = GetComponent<UIAlign>();
		_sprite = GetComponent<PackedSprite>();
	}

	public void Init(int orientation)
	{
		if (_isRunning)
		{
			return;
		}
		CurrentSplashCount++;
		_isRunning = true;
		transform.localPosition = new Vector3(0f, 0f, 10f);
		switch (orientation)
		{
		case 0:
			_align.m_horizontal = UIAlign.Horizontal.Left;
			_align.m_vertical = UIAlign.Vertical.Top;
			_sprite.Anchor = SpriteRoot.ANCHOR_METHOD.UPPER_LEFT;
			transform.localScale = new Vector3(1f, 1f, 1f);
			break;
		case 1:
			_align.m_horizontal = UIAlign.Horizontal.Right;
			_align.m_vertical = UIAlign.Vertical.Top;
			_sprite.Anchor = SpriteRoot.ANCHOR_METHOD.UPPER_LEFT;
			transform.localScale = new Vector3(-1f, 1f, 1f);
			break;
		case 2:
			_align.m_horizontal = UIAlign.Horizontal.Right;
			_align.m_vertical = UIAlign.Vertical.Bottom;
			_sprite.Anchor = SpriteRoot.ANCHOR_METHOD.UPPER_LEFT;
			transform.localScale = new Vector3(-1f, -1f, 1f);
			break;
		case 3:
			_align.m_horizontal = UIAlign.Horizontal.Left;
			_align.m_vertical = UIAlign.Vertical.Bottom;
			_sprite.Anchor = SpriteRoot.ANCHOR_METHOD.UPPER_LEFT;
			transform.localScale = new Vector3(1f, -1f, 1f);
			break;
		}
		_align.UpdateAlignment();
		UIAnimationAlpha uIAnimationAlpha = gameObject.AddComponent<UIAnimationAlpha>().Setup(1f, 0f, 3f, 2f, EZAnimation.EASING_TYPE.BackOut);
		uIAnimationAlpha.OnFinishDelegate = () =>
		{
			_isRunning = false;
			Singleton.Cache.PerPrefabCache.Destroy(gameObject);
			CurrentSplashCount--;
		};
		uIAnimationAlpha.RunAnimation();
	}
}
