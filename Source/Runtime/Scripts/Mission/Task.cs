using System;
using System.Collections.Generic;
using System.Linq;
using Glu.Localization;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class LocalizeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class LocalizeByPropertyNameAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class LocalizeAllPropertiesByNameAttribute : Attribute
{
}

public class Task : MonoBehaviourAndLog
{
	public enum Type
	{
		Unknown,
		GetItem,
		ActivateItem,
		GotoArea,
		KillBoss,
		KillAll,
		KillAmount,
		KillQuantity,
		RescueSurvivor,
		WaitForAction,
		CutScene,
		Arena
	}

	[Localize]
	public string text = "do something!";

	public bool soundWhenCompleted = true;

	[NonSerialized]
	public Type type;

	private GameObject target;

	private HintArrow hintArrow;

	private bool isTargetDynamic;

	private bool isTextDynamic;

	private bool isTargetTrackable;

	private HashSet<Character> enemiesToBeKilled = new HashSet<Character>();

	private HashSet<Scenario> scenariosToComplete = new HashSet<Scenario>();

	private bool waitForFollowingSurvivors;

	public GameObject GetTarget()
	{
		return target;
	}

	public HintArrow GetTargetHintArrow()
	{
		return hintArrow;
	}

	public bool IsTargetDynamic()
	{
		return isTargetDynamic;
	}

	public bool IsTextDynamic()
	{
		return isTextDynamic;
	}

	public bool IsTargetTrackable()
	{
		return isTargetTrackable;
	}

	public string GetText()
	{
		if ((bool)GetComponentInChildren<ScenarioCustomWave>())
		{
			return Strings.GetString("Wave") + " " + (Singleton.PlayerData.PlayerStatistics.MissionStatistics.NumWavesCompleted + 1);
		}
		if (type == Type.Arena)
		{
			return target.GetComponent<ArenaWaves>().GetText();
		}
		return SubText(Strings.GetString(text));
	}

	public float GetProgress()
	{
		if (type == Type.ActivateItem && target.GetComponent<ActivateItem>().beingActivatedBy != null && (bool)target.GetComponent<ActivateItem>().beingActivatedBy.GetComponent<Bot>())
		{
			return target.GetComponent<ActivateItem>().beingActivatedBy.GetComponent<Bot>().GetActivationProgress();
		}
		return -1f;
	}

	private void Awake()
	{
		LogTo("Mission.Task");
	}

	protected virtual void OnStarted()
	{
		Restart();
	}

	private void Restart()
	{
		target = null;
		if ((bool)(target = this.QuickFindChild("GetItem")))
		{
			type = Type.GetItem;
		}
		else if ((bool)(target = this.QuickFindChild("ActivateItem")))
		{
			type = Type.ActivateItem;
		}
		else if ((bool)(target = this.QuickFindChild("GotoArea")))
		{
			type = Type.GotoArea;
		}
		else if ((bool)(target = this.QuickFindChild("KillAmount")))
		{
			type = Type.KillAmount;
		}
		else if ((bool)(target = this.QuickFindChild("KillQuantity")))
		{
			type = Type.KillQuantity;
		}
		else if ((bool)(target = this.QuickFindChild("CutScene")))
		{
			type = Type.CutScene;
			target = null;
		}
		else if ((bool)(target = this.FindChildWithComponent<KillBoss>()))
		{
			type = Type.KillBoss;
		}
		else if ((bool)(target = this.FindChildWithComponent<Survivor>()))
		{
			type = Type.RescueSurvivor;
		}
		else if ((bool)(target = this.FindChildWithComponent<WaitForAction>()))
		{
			type = Type.WaitForAction;
		}
		else if ((bool)(target = this.FindChildWithComponent<ArenaWaves>()))
		{
			type = Type.Arena;
		}
		else if ((bool)this.FindChildWithComponent<Scenario>() || (bool)this.FindChildWithComponent<Unit>())
		{
			type = Type.KillAll;
		}
		else
		{
			log.Warn(base.gameObject, "unknown task's type for '{0}'", base.gameObject.name);
		}
		isTargetDynamic = type == Type.KillBoss;
		isTextDynamic = type == Type.KillAmount || type == Type.KillQuantity || type == Type.Arena;
		isTargetTrackable = type == Type.GetItem || type == Type.ActivateItem || (type == Type.GotoArea && (bool)target.GetComponent<GotoArea>() && target.GetComponent<GotoArea>().showArrow);
		if (type == Type.KillAll)
		{
			scenariosToComplete = new HashSet<Scenario>(GetComponentsInChildren<Scenario>());
			enemiesToBeKilled = new HashSet<Character>(GetComponentsInChildren<Unit>());
		}
		if (type == Type.GetItem || (type == Type.GotoArea && (bool)target.GetComponent<GotoArea>() && target.GetComponent<GotoArea>().showArrow))
		{
			GameObject parent = target.FindCustomChild((GameObject n) => n.name.StartsWith("Item_")) ?? target;
			GameObject gameObject = HintArrow.Create(parent);
		}
		hintArrow = GetComponentInChildren<HintArrow>();
		if (type == Type.GetItem)
		{
			Singleton.Visual.ShowOutline(target, new Color(1f, 1f, 1f), 0);
		}
		else if (type == Type.ActivateItem)
		{
			GameObject obj;
			if ((bool)(obj = this.FindCustomChild((GameObject n) => n.name.StartsWith("Item_"))))
			{
				Singleton.Visual.ShowOutline(obj, new Color(1f, 1f, 0f), 0);
			}
		}
		else if (type == Type.KillBoss)
		{
			Singleton.Visual.ShowOutline(target, new Color(1f, 0f, 0f), 1);
		}
		else if (type == Type.KillAll)
		{
			foreach (Character item in enemiesToBeKilled)
			{
				Singleton.Visual.ShowOutline(item.gameObject, new Color(1f, 0f, 0f), 0);
			}
		}
		else if (type == Type.RescueSurvivor)
		{
			GetComponentsInChildren<Survivor>().ToList().ForEach((Survivor s) =>
			{
				Singleton.Visual.ShowOutline(s.gameObject, new Color(0f, 1f, 0f), 0);
			});
		}
	}

	protected virtual void OnFinished()
	{
		Singleton.GameEvents.OnTaskCompleted(this);
		if (type == Type.GetItem)
		{
			SoundManager.PlayExclusive("task_item_pickup", false);
		}
		if (type == Type.GetItem)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.CollectItems);
			Singleton.GameEvents.OnSupplyGathered(gameObject);
		}
		else if (type == Type.RescueSurvivor)
		{
			PlayerData.Instance.ProgressGoal(Goal.Type.RescueSurvivors);
		}
		if (type == Type.RescueSurvivor)
		{
			GetComponentsInChildren<Survivor>().ToList().ForEach((Survivor s) =>
			{
				Singleton.Visual.HideOutline(s.gameObject);
				Singleton.Visual.ShowOutline(s.gameObject, new Color(0f, 1f, 0f), 1);
			});
		}
		else
		{
			Singleton.Visual.HideOutline(gameObject);
		}
	}

	private void OnEnterArea(AreaVisitInfo info)
	{
		if (info.visitor is Player && type == Type.GetItem && info.area.name == "TriggeringArea")
		{
			SendMessageUpwards("OnTaskCompleted");
		}
		else if (info.visitor is Player && type == Type.GotoArea && info.area.name == "GotoArea")
		{
			waitForFollowingSurvivors = true;
		}
		else if (info.visitor is Player && type == Type.RescueSurvivor && info.area.name == "RescueArea")
		{
			SendMessageUpwards("OnTaskCompleted");
		}
	}

	private void OnUiTap()
	{
		float unusedLocal1;
		if (type == Type.GetItem && Vector3.Distance(Scene.Player.transform.position, target.transform.position) < 4f)
		{
			SendMessageUpwards("OnTaskCompleted");
		}
	}

	private string SubText(string text)
	{
		string text2 = type == Type.KillAmount ? target.GetComponent<KillAmount>().SubText()
			: type == Type.KillQuantity ? target.GetComponent<KillQuantity>().SubText() : string.Empty;
		if (text.Contains("{0}") && text2 != string.Empty)
		{
			return string.Format(text, text2);
		}
		if (text2 != string.Empty)
		{
			return string.Format("{0} ({1})", text, text2);
		}
		return text;
	}

	public string DebugInfo()
	{
		string arg = string.Empty;
		if (type == Type.GetItem || type == Type.ActivateItem)
		{
			GameObject gameObject = this.FindCustomChild((GameObject n) => n.name.StartsWith("Item_"));
			if (gameObject != null)
			{
				arg = string.Format("{0} at {1}", gameObject.name, gameObject.transform.position.ToString("0"));
			}
		}
		else
		{
			if (type == Type.GotoArea)
			{
				arg = string.Format("to {0}", transform.position.ToString("0"));
			}
			else if (type == Type.KillBoss)
			{
				arg = string.Format("{0} at {1}", target.name, target.transform.position);
			}
			else if (type == Type.KillAll)
			{
				arg = string.Format("left - scenarios: {0}, enemies: {1}, attacking: {2}, specials: {3}", scenariosToComplete.Count, enemiesToBeKilled.Count, Scene.AttackingZombies.Count, Scene.Specials.Count);
			}
			else if (type == Type.KillAmount)
			{
				arg = target.GetComponent<KillAmount>().SubText();
			}
			else if (type == Type.KillQuantity)
			{
				arg = target.GetComponent<KillQuantity>().SubText();
			}
			else if (type == Type.RescueSurvivor)
			{
				arg = string.Format("at {0}", target.transform.position.ToString("0"));
			}
			else if (type == Type.WaitForAction)
			{
				arg = string.Format("for {0}", target.GetComponent<WaitForAction>().type.ToString());
			}
			else if (type == Type.CutScene)
			{
				arg = "-";
			}
			else
			{
				arg = "?";
			}
		}
		return string.Format("{0}, {1}", type.ToString(), arg);
	}

	private void OnUse(GameObject obj)
	{
		SendMessageUpwards("OnTaskCompleted");
	}

	private void OnScenarioCompleted(GameObject scenario)
	{
		if (type == Type.KillAll && scenariosToComplete.Contains(scenario.GetComponent<Scenario>()))
		{
			scenariosToComplete.Remove(scenario.GetComponent<Scenario>());
		}
	}

	private void OnCharacterDying(GameObject character)
	{
		if (type == Type.KillBoss && (bool)character.GetComponent<KillBoss>())
		{
			SendMessageUpwards("OnTaskCompleted");
		}
		if (type == Type.KillAll && enemiesToBeKilled.Contains(character.GetComponent<Character>()))
		{
			enemiesToBeKilled.Remove(character.GetComponent<Character>());
		}
	}

	private void OnSpawnedKillBoss(GameObject obj)
	{
		if (type == Type.KillAll && !Scene.Mission.IsSurvival())
		{
			Restart();
		}
	}

	public void ReportAction(WaitForAction.Type type, string argument)
	{
		if (this.type == Type.WaitForAction)
		{
			GetComponentInChildren<WaitForAction>().ReportAction(type, argument);
		}
	}

	protected virtual void Update()
	{
		float unusedLocal1;
		if (type == Type.KillAll && Util.DoRarely(this, "recheck_all_killed", 1f))
		{
			do
			{
				if (enemiesToBeKilled.Count > 0)
				{
					break;
				}
				if (scenariosToComplete.Count > 0)
				{
					break;
				}
				if (Scene.AttackingEnemies.Count > 0)
				{
					break;
				}
				SendMessageUpwards("OnTaskCompleted");
			}
			while (false);
		}
		if (waitForFollowingSurvivors && Util.DoRarely(this, "wait_for_survivor", 0.5f))
		{
			if (!Scene.Survivors.Any((Survivor s) => s.IsFollowing() && Vector3.Distance(s.transform.position, Scene.Player.transform.position) > 8f))
			{
				waitForFollowingSurvivors = false;
				SendMessageUpwards("OnTaskCompleted");
			}
			else if (Util.DoRarely(this, "survivor_too_far_info", 5f))
			{
				Singleton.UI.Hud.ShowInfo(Strings.GetString("Survivor is too far from you..."));
			}
		}
	}
}
