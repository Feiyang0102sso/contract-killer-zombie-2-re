public class Uncommon : Zombie
{
	protected override void OnDying(DamageInfo info)
	{
		if (info != null && info.source is Human)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.KillUncommonInfected);
		}
		base.OnDying(info);
	}
}
