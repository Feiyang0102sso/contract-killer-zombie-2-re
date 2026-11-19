using System.Linq;

public sealed class CutSceneCommandMoveSurvivor : CutSceneCommandMoveCharacter
{
	public int Index;

	protected override Character Character
	{
		get
		{
			Survivor[] array = Scene.Survivors.ToArray();
			return !(Index >= array.Length) ? array[Index] : null;
		}
	}
}
