using Glu.Localization;
using UnityEngine;

public class ShopOrExit : MonoBehaviour
{
	private void OnActivate()
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.Title = Strings.GetString("Quit?");
		popupInfo.Message = Strings.GetString("Wanna quit?");
		popupInfo.AddButton(Strings.GetString("Quit"), () =>
		{
			Scene.Mission.ExitMission();
		});
		popupInfo.AddButton(Strings.GetString("Continue"), () =>
		{
		});
		Singleton.UI.PopupManager.ShowPopup(popupInfo);
	}
}
