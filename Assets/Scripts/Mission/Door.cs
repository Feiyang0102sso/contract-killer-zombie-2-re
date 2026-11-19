using UnityEngine;

public class Door : MonoBehaviourAndLog
{
	protected enum State
	{
		Opened,
		Closed,
		Opening,
		Closing
	}

	private const string open = "open";

	public GameObject target;

	public float openingDuration = 1f;

	public string sound = "door_lids";

	private new Animation animation;

	protected float lastOpeningTime;

	protected State state = State.Closed;

	private float progress;

	protected virtual void Awake()
	{
		LogTo("Mission.Door");
		if (!target)
		{
			target = gameObject.GetParent();
		}
	}

	protected virtual void Start()
	{
		animation = target.animation;
		if (!animation || !animation["open"])
		{
			log.Error(gameObject, "attached not to door: '{0}'", target.name);
			return;
		}
		animation.Play("open");
		animation.Sample();
		animation.Stop();
	}

	protected virtual void Update()
	{
		if (state == State.Opening || state == State.Closing)
		{
			progress += (float)((state == State.Opening) ? 1 : (-1)) * Time.deltaTime / openingDuration;
			if (progress < 0f)
			{
				progress = 0f;
				state = State.Closed;
			}
			else if (progress > 1f)
			{
				progress = 1f;
				state = State.Opened;
			}
			animation["open"].normalizedTime = progress;
			animation.Play("open");
			animation.Sample();
			animation.Stop();
		}
	}

	public void Open()
	{
		if (!(state != State.Opening && state != State.Opened))
		{
			return;
		}
		log.Debug(gameObject, "door opens");
		if (state == State.Closed)
		{
			SoundManager.PlayExclusive(sound, transform.position, 2.5f, 25f, false);
		}
		state = State.Opening;
		lastOpeningTime = Time.time;
	}

	public void Close()
	{
		if (!(state != State.Closing && state != State.Closed))
		{
			return;
		}
		log.Debug(gameObject, "door closes");
		if (state == State.Opened)
		{
			SoundManager.PlayExclusive(sound, transform.position, 2.5f, 25f, false);
		}
		state = State.Closing;
	}
}
