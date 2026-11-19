public class PlayhavenPopup : UIWindow
{
	private bool _activated;

	private float _activationTime;

	protected override void Activate()
	{
		base.Activate();
		_activated = true;
		_activationTime = TimeControl.RealTime;
	}

	private void Update()
	{
		float unusedActivationDelay;
		if (_activated && TimeControl.RealTime - _activationTime > 1.2f)
		{
			if (Singleton.Game.PlayHavenMissionEnd)
			{
				string empty = string.Empty;
				int lastPlayedMissionNumber = Singleton.Game.LastPlayedMissionNumber;
				switch (lastPlayedMissionNumber)
				{
				case 1:
					empty = "tutorial_mission_end_1";
					break;
				case 2:
					empty = "tutorial_end";
					break;
				default:
				{
					int num = lastPlayedMissionNumber - 2;
					empty = "mission_end_" + (num <= 10 ? num.ToString() : "11plus");
					break;
				}
				}
				Singleton.Game.PlayHavenMissionEnd = false;
				if (QualitySetter.PlayHavenEnabled)
				{
					PlayHavenInterface.CancelCurrentContentRequest();
					PlayHavenInterface.StartPublisherContentRequestWithPreload(empty, false, false);
				}
			}
			if (!App.PlayHavenGameLaunch && Singleton.PlayerData.XP >= 200)
			{
				if (QualitySetter.PlayHavenEnabled)
				{
					PlayHavenInterface.StartPublisherContentRequestWithPreload("game_launch", false, false);
				}
				App.PlayHavenGameLaunch = true;
			}
			_activated = false;
			Close();
		}
	}
}
