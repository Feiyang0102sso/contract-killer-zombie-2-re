public sealed class CutSceneCommandTransferCamera : CutSceneCommand
{
	public override void Begin()
	{
		base.Begin();
		Scene.Camera.enabled = false;
	}

	public override bool Next()
	{
		Scene.Camera.transform.position = transform.position;
		Scene.Camera.transform.rotation = transform.rotation;
		return false;
	}
}
