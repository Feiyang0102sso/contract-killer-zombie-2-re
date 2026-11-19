using UnityEngine;

public class Trigger : MonoBehaviourAndLog
{
	public float rechargeDuration;

	private bool charged = true;

	private CountdownTimer rechargeTimer = new CountdownTimer();

	public bool Charged
	{
		get
		{
			return charged;
		}
	}

	public float rechargeLeftTime()
	{
		return rechargeTimer.LeftTime();
	}

	private void Awake()
	{
		LogTo("Mission.Trigger");
	}

	private void OnEnterArea(AreaVisitInfo info)
	{
		OnEnterOrStayInArea(info);
	}

	private void OnStayInArea(AreaVisitInfo info)
	{
		OnEnterOrStayInArea(info);
	}

	private void OnEnterOrStayInArea(AreaVisitInfo info)
	{
		if (info.visitor is Player && (info.area == gameObject || (bool)this.QuickFindChild("GetItem")))
		{
			Triggered();
		}
	}

	private void OnUse(GameObject obj)
	{
		Triggered();
	}

	private void ActivateChildren(bool activate)
	{
		foreach (string text in new string[2] { "GetItem", "ActivateItem" })
		{
			GameObject gameObject = this.QuickFindChild(text);
			if (gameObject != null && (!gameObject.GetComponent<ActivateItem>() || gameObject.GetComponent<ActivateItem>().disappearsAfterUse))
			{
				Util.DoForHierarchy(gameObject, (GameObject node) =>
				{
					node.active = activate;
					return true;
				});
			}
		}
		if (activate)
		{
			foreach (string child in new string[1] { "AmmoBox" })
			{
				GameObject gameObject2 = this.FindCustomChild((GameObject n) => n.name == child || n.name.StartsWith(child + " "));
				if (gameObject2 != null)
				{
					Util.DoForHierarchy(gameObject2, (GameObject node) =>
					{
						node.active = activate;
						return true;
					});
				}
			}
		}
	}

	public void Triggered()
	{
		if (!(charged))
		{
			return;
		}
		log.Info(gameObject, "triggered '{0}'", gameObject.name);
		charged = false;
		if (rechargeDuration != 0f)
		{
			rechargeTimer.Run(rechargeDuration);
		}
		BroadcastMessage("OnActivate");
		ActivateChildren(false);
	}

	private void Update()
	{
		if (!charged && rechargeDuration != 0f && !rechargeTimer.IsRunning())
		{
			charged = true;
			ActivateChildren(true);
		}
	}
}
