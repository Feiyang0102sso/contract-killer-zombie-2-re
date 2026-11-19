using System;
using System.Collections.Generic;
using UnityEngine;

public class GoalAnimationPopup : UIWindow
{
	private class Action
	{
		public virtual bool Update()
		{
			return true;
		}

		public virtual Action GetNextAction()
		{
			return null;
		}
	}

	private class WaitForTimeAndScrollGoals : Action
	{
		public const float WaitForTime = 1f;

		private float _time = 1f;

		private CommonGUI _commonGUI;

		public WaitForTimeAndScrollGoals(CommonGUI commonGUI)
		{
			_commonGUI = commonGUI;
		}

		public WaitForTimeAndScrollGoals(CommonGUI commonGUI, float waitForTime)
		{
			_commonGUI = commonGUI;
			_time = waitForTime;
		}

		public override bool Update()
		{
			if (_time == 1f)
			{
				MapWindow mapWindow = _commonGUI.gameObject.FindComponentInParents<MapWindow>();
				_commonGUI.SendMessage("RollupAllGoals", SendMessageOptions.RequireReceiver);
			}
			if (_time > 0f && _time - Time.deltaTime <= 0f)
			{
				_commonGUI.SendMessage("ScrollAllGoals", SendMessageOptions.RequireReceiver);
			}
			_time -= Time.deltaTime;
			if (_time > 0f)
			{
				return false;
			}
			return _commonGUI.AreGoalsScrolled;
		}

		public override Action GetNextAction()
		{
			if (PlayerData.Instance.GetCumulativeGoal().IsCompleted)
			{
				return new AnimateCumulativeReward(_commonGUI);
			}
			return null;
		}
	}

	private class PutTheStamp : Action
	{
		public const float AnimationTime = 0.33f;

		public const float StampScale = 2.5f;

		public const float StampRotation = 5f;

		private GameObject _stamp;

		private float _time;

		public PutTheStamp(GameObject goalPanel)
		{
			_stamp = Util.CreateGameObject("GoalCompleted");
			_stamp.transform.position = goalPanel.transform.position;
			_stamp.transform.position -= Vector3.forward * 1.25f;
			_stamp.transform.parent = goalPanel.transform;
			_stamp.transform.localScale = Vector3.one * 2.5f;
			_stamp.transform.Rotate(Vector3.forward, 5f, Space.World);
		}

		public override bool Update()
		{
			if (_time == 0f)
			{
				SoundManager.Play("UI/shop_item_used", false);
			}
			_time += Time.deltaTime;
			float f = Mathf.Clamp01(_time / 0.33f);
			f = Mathf.Pow(f, 2f);
			_stamp.transform.localScale = Vector3.one * (2.5f * (1f - f) + 1f * f);
			return f == 1f;
		}
	}

	private class AnimateGoalReward : Action
	{
		public const float AnimationTime = 1f;

		public const float EffectTime = 0.5f;

		private CommonGUI _commonGUI;

		private GoalGUI _goalGUI;

		private CumulativeGoalGUI _cumulativeGoalGUI;

		private GameObject _goalStar;

		private GameObject _cumulativeGoalStar;

		private float _animationTime;

		private float _effectTime;

		private Action _outOfTurnAction;

		public AnimateGoalReward(CommonGUI commonGUI, GameObject goalPanel)
		{
			_commonGUI = commonGUI;
			_goalGUI = goalPanel.GetComponent<GoalGUI>();
		}

		public override bool Update()
		{
			if (_cumulativeGoalGUI == null)
			{
				GameObject cumulativeGoalPanel = _commonGUI.GetCumulativeGoalPanel();
				_cumulativeGoalGUI = cumulativeGoalPanel.GetComponent<CumulativeGoalGUI>();
				for (int num = _goalGUI.Stars.Length - 1; num >= 0; num--)
				{
					if (_goalGUI.Stars[num].active)
					{
						_goalStar = _goalGUI.Stars[num];
						break;
					}
				}
				_cumulativeGoalStar = _cumulativeGoalGUI.GetNextEmptyStar();
				_goalStar.transform.parent = null;
				UnityEngine.Object.Destroy(_goalStar.GetComponent<UIAlign>());
			}
			if (_animationTime < 1f)
			{
				float num2 = Mathf.Pow(_animationTime / 1f, 4f);
				Vector3 vector = _cumulativeGoalStar.transform.position - _goalStar.transform.position;
				Vector3 vector2 = vector / (1f - _animationTime) * num2;
				Vector3 vector3 = vector2 * Time.deltaTime;
				if (vector3.magnitude > vector.magnitude)
				{
					vector3 = vector;
				}
				_goalStar.transform.position += vector3;
				_animationTime += Time.deltaTime;
				if (_animationTime >= 0.5f && _animationTime - Time.deltaTime < 0.5f)
				{
					SoundManager.Play("achievement_unlocked", false);
				}
				if (_animationTime >= 1f)
				{
					_goalStar.transform.parent = _cumulativeGoalStar.transform;
					_goalStar.active = false;
					_cumulativeGoalGUI.FillTheStar(_cumulativeGoalStar);
				}
				return false;
			}
			if (_effectTime < 0.5f)
			{
				float f = _effectTime * 2f * (float)Math.PI / 0.5f;
				float num3 = Mathf.Exp(-Mathf.Pow(f, 2f));
				_cumulativeGoalStar.transform.localScale = Vector3.one * (1f + num3);
				_effectTime += Time.deltaTime;
				if (_effectTime >= 0.5f)
				{
					PlayerData.Instance.NotifyGoalRewardReceived(_goalGUI.GoalType, 1);
					if (PlayerData.Instance.GetGoal(_goalGUI.GoalType).Reward == 0)
					{
						GameAnalytics.LogGoalCompleted(PlayerData.Instance.Level, PlayerData.Instance.GetGoal(_goalGUI.GoalType).Ticket);
						PlayerData.Instance.RemoveGoal(_goalGUI.GoalType);
					}
					PlayerData.Instance.ProgressCumulativeGoal(1);
					if (PlayerData.Instance.GetCumulativeGoal().IsCompleted)
					{
						_outOfTurnAction = new AnimateCumulativeReward(_commonGUI);
					}
				}
				return false;
			}
			return true;
		}

		public override Action GetNextAction()
		{
			return _outOfTurnAction;
		}
	}

	private class AnimateCumulativeReward : Action
	{
		public const float AnimationTime = 0.33f;

		public const float StampScale = 2.5f;

		public const float StampRotation = 5f;

		public const float EffectTime = 0.66f;

		private CommonGUI _commonGUI;

		private CumulativeGoalGUI _cumulativeGoalGUI;

		private GameObject _stamp;

		private float _animationTime;

		private float _effectTime;

		private bool _rewarded;

		public AnimateCumulativeReward(CommonGUI commonGUI)
		{
			_commonGUI = commonGUI;
			_cumulativeGoalGUI = _commonGUI.GetCumulativeGoalPanel().GetComponent<CumulativeGoalGUI>();
			_stamp = Util.CreateGameObject("PunchcardCompleted");
			_stamp.transform.position = _cumulativeGoalGUI.gameObject.transform.position;
			_stamp.transform.position -= Vector3.forward * 1.25f;
			_stamp.transform.parent = _cumulativeGoalGUI.gameObject.transform;
			_stamp.transform.localScale = Vector3.one * 2.5f;
			_stamp.transform.Rotate(Vector3.forward, 5f, Space.World);
		}

		public override bool Update()
		{
			if (_animationTime < 0.33f)
			{
				if (_animationTime == 0f)
				{
					SoundManager.Play("UI/shop_item_used", false);
				}
				_animationTime += Time.deltaTime;
				float f = Mathf.Clamp01(_animationTime / 0.33f);
				f = Mathf.Pow(f, 2f);
				_stamp.transform.localScale = Vector3.one * (2.5f * (1f - f) + 1f * f);
				return false;
			}
			if (!_rewarded)
			{
				SoundManager.Play("UI/collection_cant_be traded", false);
				_rewarded = true;
				CumulativeGoal cumulativeGoal = PlayerData.Instance.GetCumulativeGoal();
				PlayerData.Instance.HardCurrency += cumulativeGoal.Reward;
				GameAnalytics.LogHCReceived(cumulativeGoal.Reward, GameAnalytics.HCSourceType.Game);
				PlayerData.Instance.CleanupCumulativeGoal();
				_commonGUI.HardCurrencyText.Text = PlayerData.Instance.HardCurrency.ToString();
				GameAnalytics.LogPunchcardCompleted(PlayerData.Instance.Level, PlayerData.Instance.CumulativeTicket - 1);
			}
			if (_effectTime < 0.66f)
			{
				float f2 = _effectTime * 2f * (float)Math.PI / 0.66f;
				float num = 1f + Mathf.Exp(-Mathf.Pow(f2, 2f));
				_cumulativeGoalGUI.RewardMoney.transform.localScale = Vector3.one * num;
				_commonGUI.HardCurrencyText.transform.localScale = Vector3.one * num;
				_effectTime += Time.deltaTime;
				if (_effectTime > 0.66f)
				{
					_cumulativeGoalGUI.RewardMoney.transform.localScale = Vector3.one;
					_commonGUI.HardCurrencyText.transform.localScale = Vector3.one;
				}
				return false;
			}
			return true;
		}

		public override Action GetNextAction()
		{
			return new AnimateCumulativeGoalGeneration(_commonGUI);
		}
	}

	private class AnimateCumulativeGoalGeneration : Action
	{
		public Vector2 RollupSpeedAndDelay = new Vector2(5f, 0f);

		public Vector2 ScrollSpeedAndDelay = new Vector2(10f, 0.125f);

		private CommonGUI _commonGUI;

		private CumulativeGoalGUI _cumulativeGoalGUI;

		public AnimateCumulativeGoalGeneration(CommonGUI commonGUI)
		{
			_commonGUI = commonGUI;
		}

		public override bool Update()
		{
			if ((bool)_cumulativeGoalGUI)
			{
				return _commonGUI.AreGoalsScrolled;
			}
			_cumulativeGoalGUI = _commonGUI.GetCumulativeGoalPanel().GetComponent<CumulativeGoalGUI>();
			_commonGUI.transform.root.BroadcastMessage("GenerateCumulativeGoal", SendMessageOptions.RequireReceiver);
			_commonGUI.SendMessage("RollupCumulativeGoal", RollupSpeedAndDelay, SendMessageOptions.RequireReceiver);
			_commonGUI.SendMessage("ScrollCumulativeGoal", ScrollSpeedAndDelay, SendMessageOptions.RequireReceiver);
			GameObject cumulativeGoalPanel = _commonGUI.GetCumulativeGoalPanel();
			if ((bool)cumulativeGoalPanel)
			{
				_cumulativeGoalGUI = cumulativeGoalPanel.GetComponent<CumulativeGoalGUI>();
			}
			else
			{
				_commonGUI.SendMessage("RollupAllGoals");
			}
			return _cumulativeGoalGUI != null;
		}
	}

	private class AnimateGoalGeneration : Action
	{
		private CommonGUI _commonGUI;

		private GoalGUI _goalGUI;

		private bool _done;

		public AnimateGoalGeneration(CommonGUI commonGUI, GameObject goalPanel)
		{
			_commonGUI = commonGUI;
			_goalGUI = goalPanel.GetComponent<GoalGUI>();
		}

		public override bool Update()
		{
			if (!_done)
			{
				_done = true;
				_commonGUI.SendMessage("UpdateGoalPanels", SendMessageOptions.RequireReceiver);
			}
			return Singleton.PlayerData.GetNumGoals() == 0 ? true : _commonGUI.AreGoalsScrolled;
		}
	}

	private CommonGUI _commonGUI;

	private List<Goal.Type> _completedGoals = new List<Goal.Type>();

	private List<Action> _actions = new List<Action>();

	private bool _run;

	private bool _close;

	public void Init(CommonGUI gui)
	{
		OnShowDelegate += new System.Action(Run);
		_commonGUI = gui;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	private void Run()
	{
		Singleton.Achievements.CheckAchievements();
		if ((bool)gameObject.FindComponentInParents<StoreScreen>())
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Goal.Type[] goals = PlayerData.Instance.GetGoals();
		if (goals != null)
		{
			foreach (Goal.Type type in goals)
			{
				if (PlayerData.Instance.GetGoal(type).IsCompleted)
				{
					_completedGoals.Add(type);
				}
			}
		}
		if (_completedGoals.Count > 0 || (PlayerData.Instance.HasCumulativeGoal() && PlayerData.Instance.GetCumulativeGoal().IsCompleted))
		{
			_actions.Add(new WaitForTimeAndScrollGoals(_commonGUI));
		}
		else
		{
			_close = true;
			return;
		}
		_run = true;
	}

	private void Update()
	{
		if (_close)
		{
			Close();
		}
		if (!_run)
		{
			return;
		}
		if (StoreScreen.ShopIsOpen)
		{
			return;
		}
		if (_actions.Count > 0)
		{
			if (_actions[0].Update())
			{
				Action nextAction = _actions[0].GetNextAction();
				_actions.RemoveAt(0);
				if (nextAction != null)
				{
					_actions.Insert(0, nextAction);
				}
			}
			return;
		}
		if (!_commonGUI.GetCumulativeGoalPanel())
		{
			_actions.Insert(0, new WaitForTimeAndScrollGoals(_commonGUI, 0f));
		}
		if (_completedGoals.Count > 0 && PlayerData.Instance.HasCumulativeGoal())
		{
			GameObject goalPanel = _commonGUI.GetGoalPanel(_completedGoals[0]);
			Goal goal = PlayerData.Instance.GetGoal(goalPanel.GetComponent<GoalGUI>().GoalType);
			_actions.Add(new PutTheStamp(goalPanel));
			for (int i = 0; i < goal.Reward; i++)
			{
				_actions.Add(new AnimateGoalReward(_commonGUI, goalPanel));
			}
			_actions.Add(new AnimateGoalGeneration(_commonGUI, goalPanel));
			_completedGoals.RemoveAt(0);
			return;
		}
		_commonGUI.SendMessage("RollupAllGoals");
		Close();
	}
}

public class GoalGUIAnimation : MonoBehaviour
{
	private const float AutoDestroyPhase = 0.999f;

	private bool _active;

	private Vector2 _startMargin = Vector2.zero;

	private Vector2 _targetMargin = Vector2.zero;

	private float _animationPhase;

	private float _animationSpeed;

	private float _animationDelay;

	private UIAlign _alignment;

	private void Awake()
	{
		_alignment = GetComponent<UIAlign>();
	}

	private void Update()
	{
		if (_active)
		{
			_animationDelay -= Time.deltaTime;
			if (_animationDelay <= 0f)
			{
				_animationPhase += _animationSpeed * (1f - _animationPhase) * Time.deltaTime;
				_animationPhase = Mathf.Clamp01(_animationPhase);
				_alignment.m_margin = _startMargin * (1f - _animationPhase) + _targetMargin * _animationPhase;
				_alignment.UpdateAlignment();
				if (_animationPhase >= 0.999f)
				{
					UnityEngine.Object.Destroy(this);
				}
			}
		}
	}

	public void Animate(float speed, float delay)
	{
		_active = true;
		_startMargin = _alignment.m_margin;
		_targetMargin = _startMargin;
		_targetMargin.x = -_targetMargin.x;
		_animationPhase = 0f;
		_animationSpeed = speed;
		_animationDelay = delay;
	}

	public void Animate(Vector2 targetMargin, float speed, float delay)
	{
		_active = true;
		_startMargin = _alignment.m_margin;
		_targetMargin = targetMargin;
		_animationPhase = 0f;
		_animationSpeed = speed;
		_animationDelay = delay;
	}

	public bool IsActive
	{
		get
		{
			return _active;
		}
	}
}
