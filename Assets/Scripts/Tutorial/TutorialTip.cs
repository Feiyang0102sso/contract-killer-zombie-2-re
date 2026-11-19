using UnityEngine;

public class TutorialTip : MonoBehaviour
{
	public SpriteText Text;

	public UIExpandSprite Back;

	public bool AutoDestroy = true;

	private bool _show = true;

	private void Awake()
	{
	}

	public void SetTip(string tip, Vector3 pos)
	{
		Show(true);
		Text.Text = tip;
		transform.position = pos;
	}

	public void Hide()
	{
		Show(false);
	}

	public void Show(bool show)
	{
		if (!(_show != show))
		{
			return;
		}
		_show = show;
		gameObject.SetActiveRecursively(_show);
	}

	private void Update()
	{
		if (AutoDestroy && Singleton.Tutorial == null)
		{
			Object.Destroy(gameObject);
		}
	}
}
