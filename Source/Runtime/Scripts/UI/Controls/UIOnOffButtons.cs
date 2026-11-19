using Glu.Localization;
using UnityEngine;

public class UIOnOffButtons : MonoBehaviour
{
	public UIButton OnButton;

	public UIButton OffButton;

	private bool _toggle;

	private Vector3 _pos1;

	private Vector3 _pos2;

	public bool Toggle
	{
		get
		{
			return _toggle;
		}
		set
		{
			_toggle = value;
			UpdateToggle();
		}
	}

	private void Awake()
	{
		_pos1 = OnButton.transform.localPosition;
		_pos2 = OffButton.transform.localPosition;
		GUIUtils.HookButton(OffButton, () =>
		{
			Toggle = !Toggle;
		});
		UpdateToggle();
	}

	private void UpdateToggle()
	{
		OnButton.transform.localPosition = (_toggle ? _pos1 : _pos2);
		OffButton.transform.localPosition = (_toggle ? _pos2 : _pos1);
		string text = Strings.GetString("ON");
		string text2 = Strings.GetString("OFF");
		OnButton.Text = (_toggle ? text : text2);
		OffButton.Text = (_toggle ? text2 : text);
	}
}
