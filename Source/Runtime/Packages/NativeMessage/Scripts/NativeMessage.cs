using System.Runtime.InteropServices;
using UnityEngine;

public class NativeMessage : MonoBehaviour
{
	public delegate void ButtonTappedEventHandler(int buttonIndex);

	private const string GAME_OBJECT_NAME = "NativeMessageGameObject";

	private const string DLLIMPORT_NAME = "__Internal";

	private static NativeMessage _instance;

	public static ButtonTappedEventHandler OnButtonTapped;

	private static NativeMessage Instance
	{
		get
		{
			Init();
			return _instance;
		}
	}

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern void NM_showAlertWithTitleMessageAndButton(string title, string message, string buttonTitle);
#else
	private static void NM_showAlertWithTitleMessageAndButton(string title, string message, string buttonTitle)
	{
		NonIosNative.Stub("NM_showAlertWithTitleMessageAndButton");
	}
#endif

	public static void ShowMessageBox(string message)
	{
		ShowAlertWithTitleMessageAndButton(string.Empty, message, "OK");
	}

	public static void ShowAlertWithTitleMessageAndButton(string title, string message, string buttonTitle)
	{
		Init();
		NM_showAlertWithTitleMessageAndButton(title, message, buttonTitle);
	}

	private static void Init()
	{
		if (_instance == null)
		{
			GameObject gameObject = new GameObject("NativeMessageGameObject");
			Object.DontDestroyOnLoad(gameObject);
			_instance = gameObject.AddComponent<NativeMessage>();
		}
	}

	private void ButtonTapped(string buttonId)
	{
		if (OnButtonTapped != null)
		{
			int buttonIndex = 0;
			try
			{
				buttonIndex = int.Parse(buttonId);
			}
			catch
			{
			}
			OnButtonTapped(buttonIndex);
		}
	}
}
