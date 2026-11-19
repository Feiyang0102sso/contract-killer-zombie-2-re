using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : Door
{
	public bool canClose = true;

	public float pauseBeforeClosing = 2f;

	private CountdownTimer closingTimer;

	private HashSet<Character> nearbyChars = new HashSet<Character>();

	protected override void Awake()
	{
		base.Awake();
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!enabled)
		{
			return;
		}
		Character component = collider.GetComponent<Character>();
		if (!((bool)component))
		{
			return;
		}
		if (!nearbyChars.Contains(component))
		{
			nearbyChars.Add(component);
		}
		closingTimer = null;
		Open();
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!enabled)
		{
			return;
		}
		Character component = collider.GetComponent<Character>();
		if (!component)
		{
			return;
		}
		if (nearbyChars.Contains(component))
		{
			nearbyChars.Remove(component);
		}
	}

	protected override void Update()
	{
		if (nearbyChars.Count > 0 && Util.DoRarely(this, "recheck_list", 1.5f))
		{
			nearbyChars.RemoveWhere((Character c) => !c);
		}
		if (canClose && state == State.Opened && nearbyChars.Count == 0 && closingTimer == null)
		{
			closingTimer = new CountdownTimer(pauseBeforeClosing);
		}
		if (closingTimer != null && !closingTimer.IsRunning())
		{
			Close();
			closingTimer = null;
		}
		base.Update();
	}
}
