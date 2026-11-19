using System.Linq;
using UnityEngine;

public class PeriodicMovement : MonoBehaviour
{
	public float stayForwardTimeMin = 3f;

	public float stayForwardTimeMax = 10f;

	public float stayBackwardTimeMin = 10f;

	public float stayBackwardTimeMax = 30f;

	public float firstStayTime = 5f;

	private bool moving;

	private bool forward;

	private CountdownTimer timer;

	private void Start()
	{
		forward = true;
		timer = new CountdownTimer(firstStayTime);
		foreach (Survivor survivor in gameObject.GetComponentsInChildren<Survivor>())
		{
			Singleton.Visual.HideOutline(survivor.gameObject);
		}
		gameObject.GetComponentsInChildren<Survivor>().ToList().ForEach((Survivor s) =>
		{
			Physics.IgnoreCollision(s.collider, Util.Find("Building_Side_Windows").collider);
		});
	}

	private void Update()
	{
		if (moving)
		{
			float num = (float)(forward ? 1 : (-1)) * Time.deltaTime * 1f;
			gameObject.GetComponentsInChildren<Survivor>().ToList().ForEach((Survivor s) =>
			{
				s.Speed = 1f;
				s.Direction = (forward ? 1 : (-1)) * transform.forward;
			});
		}
		if (!moving && !timer.IsRunning())
		{
			moving = true;
			timer.Run(4f);
			forward = !forward;
			EnableAi(false);
		}
		if (moving && !timer.IsRunning())
		{
			moving = false;
			timer.Run(forward ? Util.Range(stayForwardTimeMin, stayForwardTimeMax) : Util.Range(stayBackwardTimeMin, stayBackwardTimeMax));
			EnableAi(forward);
		}
	}

	private void EnableAi(bool enable)
	{
		foreach (Survivor survivor in gameObject.GetComponentsInChildren<Survivor>())
		{
			survivor.EnableAi(enable);
		}
	}
}
