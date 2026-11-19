using UnityEngine;

public class Intro : MonoBehaviour
{
	private bool _unsupportedDevice;

	private bool IsDeviceUnsupported()
	{
		return false;
	}

	private void OnUnsupportedDeviceDialogTapped(int buttonID)
	{
		Application.Quit();
	}

	private void Start()
	{
		_unsupportedDevice = IsDeviceUnsupported();
		if (_unsupportedDevice)
		{
			NativeMessage.OnButtonTapped = OnUnsupportedDeviceDialogTapped;
			NativeMessage.ShowMessageBox(Localization.GetString("This device is not supported."));
			return;
		}
		MoviePlayer.PlayMovie("Scene_01.mov", true, () =>
		{
			Application.LoadLevel("_Start");
		});
	}
}
