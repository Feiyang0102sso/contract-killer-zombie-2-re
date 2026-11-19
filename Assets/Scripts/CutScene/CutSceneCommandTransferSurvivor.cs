using System.Linq;

public sealed class CutSceneCommandTransferSurvivor : CutSceneCommand
{
	public int Index;

	private Survivor m_Survivor;

	public override void Begin()
	{
		base.Begin();
		Survivor[] array = Scene.Survivors.ToArray();
		if (Index < array.Length)
		{
			m_Survivor = array[Index];
			m_Survivor.Speed = 0f;
		}
	}

	public override bool Next()
	{
		if (m_Survivor != null)
		{
			m_Survivor.transform.position = transform.position;
			m_Survivor.transform.rotation = transform.rotation;
		}
		return false;
	}
}
