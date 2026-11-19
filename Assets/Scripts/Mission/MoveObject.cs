using UnityEngine;

public class MoveObject : MonoBehaviour
{
	public GameObject what;

	public float duration = 3f;

	public float speed;

	private bool moving;

	private GameObject original;

	private CountdownTimer movementTimer;

	private void Awake()
	{
	}

	private void OnActivate()
	{
		foreach (MoveObject moveObject in Util.FindObjectsOfType<MoveObject>())
		{
			if (moveObject.moving && moveObject.what == what)
			{
				moveObject.Stop();
			}
		}
		moving = true;
		original = new GameObject("OriginalPlace");
		original.transform.parent = transform;
		original.transform.position = what.transform.position;
		original.transform.rotation = what.transform.rotation;
		if (duration == 0f)
		{
			duration = Vector3.Distance(original.transform.position, transform.position) / speed;
		}
		movementTimer = new CountdownTimer(duration);
	}

	private void Move(float progress)
	{
		what.transform.position = Vector3.Lerp(original.transform.position, transform.position, progress);
		what.transform.rotation = Quaternion.Lerp(original.transform.rotation, transform.rotation, progress);
	}

	private void Stop()
	{
		moving = false;
		Object.Destroy(original);
	}

	private void Update()
	{
		if (moving)
		{
			if (movementTimer.IsRunning())
			{
				Move(movementTimer.Progress());
			}
			else
			{
				Move(1f);
				Stop();
			}
		}
	}
}

public class AreaVisitInfo
{
	public Character visitor;

	public GameObject area;

	public AreaVisitInfo(Character visitor_, GameObject area_)
	{
		visitor = visitor_;
		area = area_;
	}
}
