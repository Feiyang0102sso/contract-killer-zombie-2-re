using UnityEngine;

public class ShopWindow : MonoBehaviour
{
	public UIButton BackButton;

	public UIButton StartButton;

	private void Awake()
	{
		StartButton.gameObject.active = false;
		BackButton.scriptWithMethodToInvoke = this;
		BackButton.methodToInvoke = "OnBackButton";
	}

	private void Start()
	{
	}

	private void OnBackButton()
	{
		WindowManager.Instance.CloseWindow();
	}
}
