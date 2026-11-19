public class FacebookRewardPopup : PopupControl
{
	public SpriteText RewardText;

	protected override void Awake()
	{
		base.Awake();
		RewardText.Text = Singleton.GameData.FacebookRewardHC.ToString();
	}
}
