using UnityEngine;

public abstract class FPSMemDataAbstract : MonoBehaviour
{
	public void Collect(FPSMemRecord record)
	{
		Fill(record);
	}

	protected abstract void Fill(FPSMemRecord record);
}
