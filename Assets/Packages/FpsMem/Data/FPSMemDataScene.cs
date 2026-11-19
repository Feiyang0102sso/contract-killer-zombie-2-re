using UnityEngine;

public class FPSMemDataScene : FPSMemDataAbstract
{
	protected override void Fill(FPSMemRecord record)
	{
		record.Add("scene", Application.loadedLevelName);
	}
}
