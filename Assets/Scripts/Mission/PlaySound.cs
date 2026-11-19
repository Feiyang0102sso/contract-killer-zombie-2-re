public class PlaySound : UniversalAction
{
	public string sound = string.Empty;

	public bool loop;

	public bool is3d;

	protected override void Play()
	{
		if (!is3d)
		{
			SoundManager.Play(sound, loop);
		}
		else
		{
			SoundManager.Play(sound, transform.position, 5f, 25f, loop);
		}
	}
}
