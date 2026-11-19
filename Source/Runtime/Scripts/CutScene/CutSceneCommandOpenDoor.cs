public sealed class CutSceneCommandOpenDoor : CutSceneCommand
{
	public ManualDoor Door;

	public bool Open = true;

	public override bool Next()
	{
		if (Door != null)
		{
			if (Open)
			{
				Door.Open();
			}
			else
			{
				Door.Close();
			}
		}
		return false;
	}
}
