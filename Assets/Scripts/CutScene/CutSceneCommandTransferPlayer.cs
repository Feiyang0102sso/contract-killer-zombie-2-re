public sealed class CutSceneCommandTransferPlayer : CutSceneCommand
{
	public bool standUp;

	public override bool Next()
	{
		Scene.Player.transform.position = transform.position;
		Scene.Player.transform.rotation = transform.rotation;
		if (standUp)
		{
			Scene.Player.StandUp();
		}
		return false;
	}
}
