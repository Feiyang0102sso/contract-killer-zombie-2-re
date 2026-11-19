using Glu.Localization;

public sealed class CutSceneCommandShowBossName : CutSceneCommand
{
	[Localize]
	public string Name;

	public float FadeTime = 1f;

	public override bool Next()
	{
		bool flag = string.IsNullOrEmpty(Name);
		if (!flag)
		{
			Scene.Camera.HUD.SetBossName(Strings.GetString(Name));
		}
		Scene.Camera.HUD.FadeBossName(flag, FadeTime);
		return false;
	}
}
