using System;
using System.Collections.Generic;

public class PopupInfo
{
	public int PopupID;

	public string Title;

	public string Message;

	public int IconOneID;

	public int IconTwoID;

	public bool CloseButtonEnabled;

	public List<string> ButtonLabels = new List<string>();

	public List<Action> ButtonActions = new List<Action>();

	public List<bool> ButtonClose = new List<bool>();

	public void SetTexts(string title, string message)
	{
		Title = title;
		Message = message;
	}

	public void AddButton(string label, Action action)
	{
		AddButton(label, action, true);
	}

	public void AddButton(string label, Action action, bool close)
	{
		ButtonLabels.Add(label);
		ButtonActions.Add(action);
		ButtonClose.Add(close);
	}
}
