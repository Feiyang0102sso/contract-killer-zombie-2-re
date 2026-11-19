using UnityEngine;

public class ShowPopup : MonoBehaviour
{
	public enum MissionPopupType
	{
		SurvivalBetweenWaves
	}

	public MissionPopupType PopupType;

	private void Awake()
	{
	}

	private void OnActivate()
	{
		switch (PopupType)
		{
		case MissionPopupType.SurvivalBetweenWaves:
			Singleton.UI.PopupManager.ShowSurvivalModeBetweenPopup(new SurvivalGatesMissionReward());
			break;
		}
	}
}
