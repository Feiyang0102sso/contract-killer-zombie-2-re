public abstract class UniversalAction : CutSceneCommand
{
	private void OnActivate()
	{
		if (!gameObject.FindComponentInParents<CutSceneTrack>() && !gameObject.FindComponentInParents<Effect>())
		{
			Play();
		}
	}

	public override void Begin()
	{
		Play();
	}

	public override bool Next()
	{
		return false;
	}

	private void OnEffect()
	{
		Play();
	}

	protected abstract void Play();
}
