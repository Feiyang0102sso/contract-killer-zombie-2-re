using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class Gate : Human
{
	private class PartInfo
	{
		public Vector3 position;

		public Quaternion rotation;
	}

	private const float repairDuration = 4f;

	private List<GameObject> breakableParts = new List<GameObject>();

	private Dictionary<GameObject, PartInfo> brokenParts = new Dictionary<GameObject, PartInfo>();

	[NonSerialized]
	public int repairedTimes;

	protected override void Awake()
	{
		base.Awake();
		ChangeState(State.Staying);
		_localHitEffects = false;
	}

	protected override void Start()
	{
		base.Start();
		showHealth = ShowHealth.Always;
		updateCharInfoPosition = false;
		GameObject root = this.QuickFindChild("Fence");
		Util.DoForHierarchy(root, false, (GameObject node) =>
		{
			breakableParts.Add(node);
			return false;
		});
	}

	protected override void Update()
	{
		if (Util.DoRarely(this, "aggravation_pulse", 1.5f))
		{
			foreach (Zombie zombie in Scene.Zombies)
			{
				if (!zombie.IsAttacking(null) && zombie.speedBase != 0f)
				{
					zombie.Chase(gameObject);
				}
			}
		}
		if (state == State.Appearing && stateProgress > 0.5f)
		{
			float f = (stateProgress - 0.5f) * 2f;
			f = Mathf.Pow(f, 10f);
			foreach (KeyValuePair<GameObject, PartInfo> brokenPart in brokenParts)
			{
				brokenPart.Key.transform.position = Vector3.Slerp(brokenPart.Key.transform.position, brokenPart.Value.position, f);
				brokenPart.Key.transform.rotation = Quaternion.Slerp(brokenPart.Key.transform.rotation, brokenPart.Value.rotation, f);
			}
		}
		_charInfo.transform.localPosition = Vector3.up * 3f;
		_charInfo.transform.localRotation = Quaternion.identity;
		base.Update();
	}

	protected override void UpdateMovement()
	{
	}

	protected override void ChangeState(State newState, State nextState_, float nextStateDelay_)
	{
		if (newState == State.Appearing)
		{
			foreach (KeyValuePair<GameObject, PartInfo> brokenPart in brokenParts)
			{
				brokenPart.Key.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
		if (state == State.Appearing)
		{
			foreach (KeyValuePair<GameObject, PartInfo> brokenPart2 in brokenParts)
			{
				brokenPart2.Key.transform.position = brokenPart2.Value.position;
				brokenPart2.Key.transform.rotation = brokenPart2.Value.rotation;
			}
			brokenParts.Clear();
		}
		base.ChangeState(newState, nextState_, nextStateDelay_);
	}

	protected override void OnHurt(DamageInfo info)
	{
		if (PainSound != null && PainSound.Length > 0 && Util.DoRarely(this, "SURUGADOI", 0.33f))
		{
			SoundManager.PlayExclusive(PainSound[UnityEngine.Random.Range(0, PainSound.Length)], transform.position + Util.RandomInCircle() * 1.5f, 10f, 50f, false);
		}
		if (Scene.Player.IsZoomMode() && Vector3.Angle(Scene.Camera.transform.forward, Center() - Scene.Camera.transform.position) > 15f && Util.DoRarely(this, "gates_under_attack", 5f))
		{
			Singleton.UI.Hud.ShowInfo(Strings.GetString("The gates are under attack!"));
		}
		if (state != State.Appearing && (float)brokenParts.Count / (float)breakableParts.Count < (healthTotal - health) / healthTotal)
		{
			foreach (GameObject item in breakableParts.Shuffled())
			{
				if (brokenParts.ContainsKey(item))
				{
					continue;
				}
				int num = breakableParts.Count - brokenParts.Count;
				if ((item.name.EndsWith("Last3") && num != 3) || (item.name.EndsWith("Last2") && num != 2) || (item.name.EndsWith("Last1") && num != 1))
				{
					continue;
				}
				Rigidbody rigidbody = item.GetComponent<Rigidbody>();
				rigidbody.isKinematic = false;
				rigidbody.linearVelocity = new Vector3(Util.Range(0f, 0.5f), Util.Range(0f, 0.5f), Util.Range(0f, 0.5f));
				rigidbody.angularVelocity = new Vector3(Util.Range(0f, 60f), Util.Range(0f, 60f), Util.Range(0f, 60f));
				float unusedVelocity;
				float unusedAngularVelocity;
				brokenParts.Add(item, new PartInfo
				{
					position = item.transform.position,
					rotation = item.transform.rotation
				});
				break;
			}
		}
	}

	protected override void Animate(bool firstRun)
	{
		if (state == State.Staying)
		{
		}
		else if (state == State.Appearing)
		{
			if (firstRun)
			{
				nextStateDelay += 4f;
			}
		}
		else if (state == State.Pain)
		{
		}
		else if (state == State.Dying)
		{
		}
		else if (state == State.Dead)
		{
		}
		else
		{
			base.Animate(firstRun);
		}
	}
}
