using System;
using System.Linq;
using Glu.Localization;
using UnityEngine;

public class ActivateItem : MonoBehaviourAndLog
{
	public const float activateByNpcRange = 0.3f;

	[Localize]
	public string textForUse = "Use";

	public bool disappearsAfterUse;

	public bool activateByNpc;

	public float usageDuration;

	public bool defaultSound = true;

	private bool _entered;

	private UISingleSprite _button;

	private UIProgressBar _bar;

	private SpriteText _text;

	private SpriteText _text2;

	private SpriteFader _fader;

	private HintArrow _hintArrow;

	[NonSerialized]
	public Character beingActivatedBy;

	private Trigger trigger;

	private TimerFromTrigger timerFromTrigger;

	public static int terminalNumber = 0;

	private void Awake()
	{
		LogTo("Mission.ActivateItem");
		trigger = (!(!gameObject.GetParent()) ? gameObject.GetParent().GetComponent<Trigger>() : null);
		timerFromTrigger = GetComponent<TimerFromTrigger>();
	}

	private void Start()
	{
		if (activateByNpc)
		{
			this.QuickFindChild("TriggeringArea").GetComponent<SphereCollider>().radius *= 1f;
		}
		_button = this.QuickFindChild("ButtonCenter/Button").GetComponent<UISingleSprite>();
		_button.hideAtStart = false;
		_button.Hide(false);
		_button.gameObject.active = false;
		string path = GetComponent<ResupplyAmmo>() ? "ResupplyAmmo" : "ActivateItem";
		_button.SetTexture(Resources.Load(path) as Texture2D);
		_fader = _button.gameObject.AddComponent<SpriteFader>();
		GameObject gameObject = this.QuickFindChild("ButtonCenter/Bar");
		if (gameObject != null)
		{
			_bar = gameObject.GetComponent<UIProgressBar>();
			_bar.Color = Color.blue;
			_bar.hideAtStart = false;
			_bar.Hide(false);
			_bar.gameObject.SetActiveRecursively(false);
		}
		string text = Strings.GetString(textForUse);
		if ((bool)GetComponent<ResupplyAmmo>())
		{
			text = text.Replace(" ", "\n");
		}
		_text = this.QuickFindChild("ButtonCenter/Text").GetComponent<SpriteText>();
		_text.Text = "[#FFFF00]" + text;
		_text.transform.position = _text.transform.position + Vector3.up * 0.1f * text.Where((char c) => c == '\n').Count();
		_text2 = this.QuickFindChild("ButtonCenter/Text2").GetComponent<SpriteText>();
		if (activateByNpc)
		{
			GameObject gameObject2 = this.QuickFindChild("ButtonCenter/HintArrow");
			if (!gameObject2)
			{
				gameObject2 = HintArrow.Create(this.FindCustomChild((GameObject n) => n.name.StartsWith("Item_")));
			}
			_hintArrow = gameObject2.GetComponent<HintArrow>();
		}
	}

	public void Disappear()
	{
		Util.DoForHierarchy(gameObject, true, (GameObject node) =>
		{
			if (node.name.StartsWith("Item_") && !disappearsAfterUse)
			{
				return false;
			}
			node.active = false;
			return true;
		});
	}

	private void OnEnterArea(AreaVisitInfo info)
	{
		OnEnterOrLeaveArea(info, true);
		OnEnterOrStayInArea(info);
	}

	private void OnStayInArea(AreaVisitInfo info)
	{
		OnEnterOrStayInArea(info);
	}

	private void OnLeaveArea(AreaVisitInfo info)
	{
		OnEnterOrLeaveArea(info, false);
	}

	private void OnEnterOrLeaveArea(AreaVisitInfo info, bool enteredOrNot)
	{
		if (info.area.name == "TriggeringArea" && info.visitor is Player)
		{
			_entered = enteredOrNot;
		}
	}

	private void OnEnterOrStayInArea(AreaVisitInfo info)
	{
		if (info.area.name == "TriggeringArea" && info.visitor is Bot && activateByNpc)
		{
			(info.visitor as Bot).OnEnteredActivateItem(this);
		}
	}

	private bool CanBeActivated()
	{
		return (_entered || activateByNpc) && (!trigger || trigger.Charged);
	}

	private void Update()
	{
		float unusedLocal1;
		if (!activateByNpc)
		{
			if (Vector3.Distance(transform.position, Scene.Player.transform.position) <= 20f)
			{
				if (!_fader.gameObject.active && !_fader.IsFadingIn())
				{
					_fader.FadeIn(1f);
				}
				Util.RotateToCamera(_button.transform);
			}
			else if (_fader.gameObject.active && !_fader.IsFadingOut())
			{
				_fader.FadeOut(1f);
			}
			if (CanBeActivated())
			{
				if (!_text.renderer.enabled)
				{
					_text.Hide(false);
				}
				Util.RotateToCamera(_text.transform);
			}
			else if (_text.renderer.enabled)
			{
				_text.Hide(true);
			}
			if (!CanBeActivated() && timerFromTrigger != null && timerFromTrigger.GetText() != string.Empty)
			{
				if (!_text2.renderer.enabled)
				{
					_text2.Hide(false);
				}
				_text2.Text = timerFromTrigger.GetText();
				Util.RotateToCamera(_text2.transform);
			}
			else if (_text2.renderer.enabled)
			{
				_text2.Hide(true);
			}
		}
		else if (beingActivatedBy != null)
		{
			Util.RotateToCamera(_text.transform);
		}
	}

	private void OnUiTap()
	{
		if (!activateByNpc)
		{
			Use();
		}
	}

	public void Use()
	{
		if (CanBeActivated())
		{
			if (!activateByNpc)
			{
				BeginUse();
				if (defaultSound)
				{
					SoundManager.Play("button_pressed", transform.position, 2.5f, 25f, false);
				}
			}
			else
			{
				beingActivatedBy = null;
				GameAnalytics.LogSurvivorDelivered(Singleton.PlayerData.Level, ++terminalNumber);
			}
			SendMessageUpwards("OnUse", gameObject);
		}
		else
		{
			log.Warn(gameObject, "un-activatable");
		}
	}

	public void BeginUse()
	{
		if (beingActivatedBy != null && usageDuration > 0f && _bar != null)
		{
			_text.Text = Strings.GetString("Using, hold on...");
			_text.transform.position = beingActivatedBy.transform.position + Vector3.up * beingActivatedBy.Height() * 1.3f;
			gameObject.AddComponent<MiniScript>().Charge(usageDuration, null, (MiniScript script) =>
			{
				_bar.transform.position = beingActivatedBy.transform.position + Vector3.up * beingActivatedBy.Height() * 1.2f;
				_bar.gameObject.SetActiveRecursively(true);
				_text.gameObject.renderer.enabled = true;
				_bar.Value = 0f;
			}, (MiniScript script) =>
			{
				_bar.Value = script.Timer.Progress();
				Util.RotateToCamera(_bar.transform);
			}, (MiniScript script) =>
			{
				_bar.gameObject.SetActiveRecursively(false);
				_text.gameObject.renderer.enabled = true;
			});
		}
		if ((bool)_hintArrow)
		{
			_hintArrow.Show(false);
		}
		(from an in GetComponentsInChildren<Animation>()
			where an["activate_item"] != null
			select an).ToList().ForEach((Animation an) =>
		{
			an.Play("activate_item");
			if ((bool)an["activate_item_loop"])
			{
				an.PlayQueued("activate_item_loop");
			}
		});
	}

	public bool ActivateByNpc(Character activator)
	{
		if (beingActivatedBy != null)
		{
			return false;
		}
		beingActivatedBy = activator;
		return true;
	}
}
