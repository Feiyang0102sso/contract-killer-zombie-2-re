using UnityEngine;

public class PopupControl : UIWindow
{
	public UIGridLayout ButtonLayout;

	public UIButton Button;

	public SpriteText _titleLabel;

	public SpriteText _msgLabel;

	public UIButton CloseButton;

	private PopupInfo _popupInfo;

	private int _buttonsCount;

	public void SetInfo(PopupInfo popupInfo)
	{
		_popupInfo = popupInfo;
		if (popupInfo.Title != null && _titleLabel != null)
		{
			_titleLabel.Text = popupInfo.Title;
		}
		if (popupInfo.Message != null && _msgLabel != null)
		{
			_msgLabel.Text = popupInfo.Message;
		}
		for (int i = 0; i < popupInfo.ButtonLabels.Count; i++)
		{
			UIButton uIButton = Button;
			if (_buttonsCount > 0)
			{
				uIButton = (Object.Instantiate(uIButton.gameObject) as GameObject).GetComponent<UIButton>();
				if ((bool)uIButton.spriteText)
				{
					uIButton.spriteText.isClone = true;
				}
			}
			int localI = i;
			GUIUtils.HookButton(uIButton, () =>
			{
				if (_popupInfo.ButtonActions[localI] != null)
				{
					_popupInfo.ButtonActions[localI]();
				}
				if (_popupInfo.ButtonClose[localI])
				{
					Close();
				}
			});
			uIButton.Text = _popupInfo.ButtonLabels[i];
			ButtonLayout.AddItem(uIButton.gameObject, _buttonsCount++, 0);
		}
		if (_buttonsCount == 0)
		{
			Object.Destroy(Button.gameObject);
		}
		if (!popupInfo.CloseButtonEnabled)
		{
			Object.Destroy(CloseButton);
		}
		else
		{
			GUIUtils.HookButton(CloseButton, Close);
		}
	}
}
