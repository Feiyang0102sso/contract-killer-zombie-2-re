public abstract class CutSceneCommand : MonoBehaviourAndLog
{
	public virtual void Begin()
	{
	}

	public virtual void End()
	{
	}

	public abstract bool Next();
}
