using UnityEngine;

public class SendAnalytics : MonoBehaviour
{
	public enum AnalyticsEventTypes
	{
		TutorialStepCompleted
	}

	public AnalyticsEventTypes AnalyticsEventType;

	public Tutorial.AnalyticEvents TutorialStep;

	private void Awake()
	{
	}

	private void OnActivate()
	{
		switch (AnalyticsEventType)
		{
		case AnalyticsEventTypes.TutorialStepCompleted:
			GameAnalytics.LogTutorialStepCompleted(TutorialStep);
			break;
		}
	}
}
