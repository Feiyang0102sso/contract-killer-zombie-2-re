using System;
using UnityEngine;

public class LimitedTimeOfferIndicator : MonoBehaviour
{
	public GameObject Root;

	public SpriteText TimerText;

	public UIButton OpenButton;

	private void Awake()
	{
		GUIUtils.HookButton(OpenButton, Singleton.UI.PopupManager.ShowLimitedTimeOfferWelcomePopup);
	}

	private void Update()
	{
		if (Util.DoRarely(this, "update_lto", 1f))
		{
			bool flag = Singleton.ShopData.LimitedTimeOffer.IsRunning();
			Root.gameObject.SetActiveRecursively(flag);
			if (flag)
			{
				TimeSpan timeLeft = Singleton.ShopData.LimitedTimeOffer.GetTimeLeft();
				TimerText.Text = string.Format("{0:00}:{1:00}:{2:00}", (int)timeLeft.TotalHours, timeLeft.Minutes, timeLeft.Seconds);
			}
		}
	}
}
