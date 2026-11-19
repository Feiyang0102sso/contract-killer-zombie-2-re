using Glu.Localization;

public class MissionLockedPopup : UsualMissionInfoPopup
{
	public UIPressableButton OkButton;

	public SpriteText DescriptionText;

	protected override void Awake()
	{
		base.Awake();
		_voiceOverEnabled = false;
		GUIUtils.HookButton(OkButton, () =>
		{
			Close();
		});
	}

	public override void Init(MapNarrative mapNarrative, MapLocation mapLocation)
	{
		int num = Singleton.GameData.Acts[Singleton.PlayerData.Act].Required - Singleton.PlayerData.StorylineProgress;
		DescriptionText.Text = (!(num <= 1) ? Strings.GetStringFormat("Complete {0} missions to unlock.", num) : Strings.GetString("Complete 1 mission to unlock."));
		base.Init(mapNarrative, mapLocation);
	}
}
