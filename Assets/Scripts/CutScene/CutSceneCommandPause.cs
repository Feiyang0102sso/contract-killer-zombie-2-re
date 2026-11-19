public sealed class CutSceneCommandPause : CutSceneCommand
{
	public bool Pause = true;

	public override bool Next()
	{
		TimeControl.Pause(Pause, TimeControl.PauseTypes.CutScene);
		return false;
	}
}
