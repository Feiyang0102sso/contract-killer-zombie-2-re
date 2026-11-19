public sealed class CutSceneCommandMovePlayer : CutSceneCommandMoveCharacter
{
	protected override Character Character
	{
		get
		{
			return Scene.Player;
		}
	}
}
