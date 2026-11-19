using UnityEngine;

public class SpawnDoor : Door
{
	public float autoCloseTime = 4f;

	public void OnSpawn()
	{
		Open();
	}

	protected override void Update()
	{
		if (lastOpeningTime != 0f && Time.time - lastOpeningTime >= autoCloseTime)
		{
			Close();
			lastOpeningTime = 0f;
		}
		base.Update();
	}
}
