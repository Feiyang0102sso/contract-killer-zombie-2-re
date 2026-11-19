public class ManualDoor : Door
{
	public bool openedFromStart = true;

	protected override void Start()
	{
		base.Start();
		if (openedFromStart)
		{
			Open();
		}
	}
}
