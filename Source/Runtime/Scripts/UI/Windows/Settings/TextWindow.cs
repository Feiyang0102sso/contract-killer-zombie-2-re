using UnityEngine;

public class TextWindow : UIWindow
{
	public UIButton CloseButton;

	public UIExpandSprite Back;

	public SpriteText TitleText;

	public SpriteText ContentText;

	public UIScrollList List;

	protected override void Start()
	{
		base.Start();
		GUIUtils.HookButton(CloseButton, () =>
		{
			Close();
			Singleton.UI.ShowPauseWindow();
		});
	}

	public void SetText(string title, string text)
	{
		TitleText.Text = title;
		List.ClearList(false);
		UIListItemContainer uIListItemContainer = new GameObject("Container").AddComponent<UIListItemContainer>();
		ContentText.maxWidth = List.viewableArea.x;
		ContentText.transform.parent = uIListItemContainer.transform;
		ContentText.transform.localPosition = new Vector3(0f, 0f, -2f);
		ContentText.Text = text;
		uIListItemContainer.ScanChildren();
		List.AddItem(uIListItemContainer);
	}
}
