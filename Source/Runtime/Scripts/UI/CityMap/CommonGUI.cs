using System;
using System.Collections;
using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class CommonGUI : MonoBehaviour
{
	public const int MaxGoalPanels = 4;

	private const float GoalPanelOffset = -8f;

	public UIButton LeftButton;

	public UIButton BackButton;

	public UIButton GetFreeGoldButton;

	public UISingleSprite GetFreeTutorialIcon;

	public UIButton AchievementsButton;

	public UIButton FacebookLoginButton;

	public SpriteText SoftCurrencyText;

	public SpriteText HardCurrencyText;

	public GameObject HardCurrencyPanel;

	public UIButton AddButton1;

	public UIButton AddButton2;

	public SpriteText LevelText;

	public UIProgressBar XPBar;

	public UIButton SettingsButton;

	public UIButton GoalButton;

	public GameObject BottomPanel;

	private GameObject _cumulativeGoalPanel;

	private GameObject[] _goalPanels;

	private bool _rollupGoals;

	private static bool _showTutorialGetFreeIcon = true;

	private bool _showFacebookButtons = true;

	private bool _uiAlignHackFix;

	private UIAlign[] _aligns;

	private Vector2 CumulativeGoalRollupSpeedAndDelay = new Vector2(25f, 4f / 15f);

	private Vector2[] GoalRollupSpeedAndDelay = new Vector2[4]
	{
		new Vector2(25f, 3f / 14f),
		new Vector2(25f, 1f / 6f),
		new Vector2(25f, 0.125f),
		new Vector2(25f, 0f)
	};

	private Vector2 CumulativeGoalMargin = new Vector3(-61f, -41f);

	private Vector2[] GoalMargin = new Vector2[4]
	{
		new Vector2(-56f, -76f),
		new Vector2(-56f, -101f),
		new Vector2(-56f, -126f),
		new Vector2(-56f, -151f)
	};

	private Vector2 CumulativeGoalScrollSpeedAndDelay = new Vector2(10f, 0f);

	private Vector2[] GoalScrollSpeedAndDelay = new Vector2[4]
	{
		new Vector2(10f, 0.125f),
		new Vector2(10f, 1f / 6f),
		new Vector2(10f, 3f / 14f),
		new Vector2(10f, 4f / 15f)
	};

	private Vector2 GoalRemoveSpeedAndDelay = new Vector2(10f, 0f);

	private Vector2 GoalUpdateSpeedAndDelay = new Vector2(5f, 0.125f);

	private void Awake()
	{
		if ((bool)transform.parent)
		{
			UIWindow component = transform.parent.GetComponent<UIWindow>();
			if (component.LayerNum > 0)
			{
				GUIUtils.HookButton(BackButton, transform.parent.GetComponent<UIWindow>().Close);
			}
			else
			{
				BackButton.gameObject.SetActiveRecursively(false);
			}
		}
		GUIUtils.HookButton(AddButton1, () =>
		{
			Singleton.UI.Store.SelectCurrencyGroup();
		});
		GUIUtils.HookButton(AddButton2, () =>
		{
			Singleton.UI.Store.SelectCurrencyGroup();
		});
		GUIUtils.HookButton(GoalButton, () =>
		{
			ScrollAllGoals();
		});
		GUIUtils.HookButton(SettingsButton, Singleton.UI.ShowPauseWindow);
		_aligns = GetComponentsInChildren<UIAlign>(false);
		GUIUtils.HookButton(GetFreeGoldButton, App.Instance.GetFreeGold);
		GUIUtils.HookButton(AchievementsButton, Singleton.UI.ShowAchievementsWindow);
		Singleton.PlayerData.OnHardCurrencyChanged += new PlayerData.OnCurrencyChangedDelegate(OnHardCurrencyChanged);
		Singleton.PlayerData.OnSoftCurrencyChanged += new PlayerData.OnCurrencyChangedDelegate(OnSoftCurrencyChanged);
		Singleton.PlayerData.OnXPChanged += new PlayerData.OnXPChangedDelegate(OnXPChanged);
		UpdateSoftCurrency(Singleton.PlayerData.SoftCurrency);
		UpdateHardCurrency(Singleton.PlayerData.HardCurrency);
		UpdatePlayerInfo();
		LeftButton.gameObject.SetActiveRecursively(false);
		ShowBottomControls(false);
		GoalButton.Text = Strings.GetString("GOALS");
		if (Tutorial.IsRunning() && _showTutorialGetFreeIcon)
		{
			ShowGetFreeTutorialIcon(true);
		}
		else
		{
			ShowGetFreeTutorialIcon(false);
		}
		GUIUtils.HookButton(FacebookLoginButton, () =>
		{
			if (!Util.DoRarelyReal(this, "fb_antispam", 1.5f))
			{
				return;
			}
			FacebookManager.Login();
		});
		StartCoroutine(PreloadGoalUI());
	}

	private void SetLocalZ(Transform transform, float localZ)
	{
		Vector3 localPosition = transform.localPosition;
		localPosition.z = localZ;
		transform.localPosition = localPosition;
	}

	private void Update()
	{
		if (_rollupGoals)
		{
			RollupAllGoals();
		}
		if (!_uiAlignHackFix)
		{
			foreach (UIAlign uIAlign in _aligns)
			{
				if (uIAlign != null)
				{
					uIAlign.UpdateAlignment();
				}
			}
			_uiAlignHackFix = true;
		}
		if (_showTutorialGetFreeIcon)
		{
			ShowGetFreeTutorialIcon(true);
		}
		else
		{
			ShowGetFreeTutorialIcon(false);
		}
		if (Util.DoRarely(this, "facebook_update", 1f))
		{
			UpdateFacebookButtons();
		}
	}

	public void OnSoftCurrencyChanged(int before, int now)
	{
		UpdateSoftCurrency(before);
	}

	public void OnHardCurrencyChanged(int before, int now)
	{
		UpdateHardCurrency(before);
	}

	public void OnXPChanged(int before, int now)
	{
		UpdatePlayerInfo();
	}

	private void UpdateSoftCurrency(int before)
	{
		UIAnimationCountText component = SoftCurrencyText.gameObject.GetComponent<UIAnimationCountText>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		component = SoftCurrencyText.gameObject.AddComponent<UIAnimationCountText>();
		component.Setup(before, Singleton.PlayerData.SoftCurrency, 0f, 1f, EZAnimation.EASING_TYPE.ExponentialOut, GUIUtils.FormatSoftCurrencyString);
		component.RunAnimation();
	}

	private void UpdateHardCurrency(int before)
	{
		UIAnimationCountText component = HardCurrencyText.gameObject.GetComponent<UIAnimationCountText>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		component = HardCurrencyText.gameObject.AddComponent<UIAnimationCountText>();
		component.Setup(before, Singleton.PlayerData.HardCurrency, 0f, 1f, EZAnimation.EASING_TYPE.ExponentialOut, GUIUtils.FormatHardCurrencyString);
		component.RunAnimation();
	}

	private void UpdatePlayerInfo()
	{
		LevelText.Text = Strings.GetString("Level") + " " + Singleton.PlayerData.Level.ToString();
		XPBar.Value = (float)(PlayerData.Instance.XP - PlayerData.Instance.CurrLevelXP) / (float)(PlayerData.Instance.NextLevelXP - PlayerData.Instance.CurrLevelXP);
	}

	private void UpdateFacebookButtons()
	{
		bool flag = FacebookManager.WasLoggedIn();
		bool flag2 = Singleton.PlayerData.Level >= 3;
		FacebookLoginButton.gameObject.SetActiveRecursively(!flag && flag2 && _showFacebookButtons);
	}

	public bool AreGoalsScrolled
	{
		get
		{
			bool flag = false;
			if ((bool)_cumulativeGoalPanel)
			{
				flag = true;
				if ((bool)_cumulativeGoalPanel.GetComponent<GoalGUIAnimation>())
				{
					flag = false;
				}
				if (flag)
				{
					foreach (GameObject gameObject in _goalPanels)
					{
						if ((bool)gameObject.GetComponent<GoalGUIAnimation>())
						{
							flag = false;
							break;
						}
					}
				}
			}
			return flag;
		}
	}

	public GameObject GetCumulativeGoalPanel()
	{
		return _cumulativeGoalPanel;
	}

	public GameObject GetGoalPanel(Goal.Type goalType)
	{
		if ((bool)_cumulativeGoalPanel)
		{
			foreach (GameObject gameObject in _goalPanels)
			{
				if (gameObject.GetComponent<GoalGUI>().GoalType == goalType)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	private void SetGoalAnimation(GameObject panel, float animationSpeed, float animationDelay)
	{
		GoalGUIAnimation goalGUIAnimation = panel.GetComponent<GoalGUIAnimation>();
		if (!goalGUIAnimation)
		{
			goalGUIAnimation = panel.AddComponent<GoalGUIAnimation>();
		}
		goalGUIAnimation.Animate(animationSpeed, animationDelay);
	}

	private void SetGoalAlignment(GameObject panel, Vector2 margin, float animationSpeed, float animationDelay)
	{
		UIAlign uIAlign = panel.AddComponent<UIAlign>();
		uIAlign.m_horizontal = UIAlign.Horizontal.Left;
		uIAlign.m_vertical = UIAlign.Vertical.Top;
		uIAlign.m_mode = UIAlign.Mode.Screen;
		uIAlign.m_margin = margin;
		SetGoalAnimation(panel, animationSpeed, animationDelay);
	}

	private IEnumerator PreloadGoalUI()
	{
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		while (true)
		{
			while (Singleton.UI.GetNumInstances("CumulativeGoalPanel") < 2)
			{
				Singleton.UI.Preinstantiate("CumulativeGoalPanel");
				yield return waitForEndOfFrame;
			}
			while (Singleton.UI.GetNumInstances("GoalPanel") < 8)
			{
				Singleton.UI.Preinstantiate("GoalPanel");
				yield return waitForEndOfFrame;
			}
			yield return waitForEndOfFrame;
		}
	}

	private void RollupCumulativeGoal(Vector2 speedAndDelay)
	{
		if ((bool)_cumulativeGoalPanel)
		{
			bool flag = false;
			GoalGUIAnimation component = _cumulativeGoalPanel.GetComponent<GoalGUIAnimation>();
			if ((bool)component && component.IsActive)
			{
				flag = true;
			}
			if (!flag)
			{
				SetGoalAnimation(_cumulativeGoalPanel, speedAndDelay.x, speedAndDelay.y);
				UnityEngine.Object.Destroy(_cumulativeGoalPanel, 2f);
				_cumulativeGoalPanel = null;
				SoundManager.PlayExclusive("UI/collection_traded", false);
			}
		}
	}

	private void RollupOnlyGoals()
	{
		if (_cumulativeGoalPanel)
		{
			bool flag = false;
			foreach (GameObject gameObject in _goalPanels)
			{
				GoalGUIAnimation component = gameObject.GetComponent<GoalGUIAnimation>();
				if ((bool)component && component.IsActive)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < _goalPanels.Length; j++)
				{
					SetGoalAnimation(_goalPanels[j], GoalRollupSpeedAndDelay[j].x, GoalRollupSpeedAndDelay[j].y);
					UnityEngine.Object.Destroy(_goalPanels[j], 2f);
				}
				Array.Resize(ref _goalPanels, 0);
			}
		}
	}

	private void RollupAllGoals()
	{
		if (_cumulativeGoalPanel)
		{
			bool flag = false;
			GoalGUIAnimation component = _cumulativeGoalPanel.GetComponent<GoalGUIAnimation>();
			if ((bool)component && component.IsActive)
			{
				flag = true;
			}
			foreach (GameObject gameObject in _goalPanels)
			{
				GoalGUIAnimation component2 = gameObject.GetComponent<GoalGUIAnimation>();
				if ((bool)component2 && component2.IsActive)
				{
					flag = true;
				}
			}
			if (flag)
			{
				_rollupGoals = true;
				return;
			}
			_rollupGoals = false;
			SetGoalAnimation(_cumulativeGoalPanel, CumulativeGoalRollupSpeedAndDelay.x, CumulativeGoalRollupSpeedAndDelay.y);
			for (int j = 0; j < _goalPanels.Length; j++)
			{
				SetGoalAnimation(_goalPanels[j], GoalRollupSpeedAndDelay[j].x, GoalRollupSpeedAndDelay[j].y);
			}
			UnityEngine.Object.Destroy(_cumulativeGoalPanel, 2f);
			_cumulativeGoalPanel = null;
			foreach (GameObject obj in _goalPanels)
			{
				UnityEngine.Object.Destroy(obj, 2f);
			}
			Array.Resize(ref _goalPanels, 0);
			SoundManager.PlayExclusive("UI/collection_traded", false);
		}
	}

	private void ScrollCumulativeGoal(Vector2 speedAndDelay)
	{
		if (!_cumulativeGoalPanel && PlayerData.Instance.HasCumulativeGoal())
		{
			_cumulativeGoalPanel = Singleton.UI.Instantiate("CumulativeGoalPanel");
			_cumulativeGoalPanel.transform.parent = transform;
			_cumulativeGoalPanel.transform.position = transform.position + Vector3.forward * -8f;
			SetGoalAlignment(_cumulativeGoalPanel, CumulativeGoalMargin, speedAndDelay.x, speedAndDelay.y);
			CumulativeGoal cumulativeGoal = PlayerData.Instance.GetCumulativeGoal();
			int num = PlayerData.Instance.CumulativeTicket - 1;
			CumulativeGoalGUI component = _cumulativeGoalPanel.GetComponent<CumulativeGoalGUI>();
			component.Title.Text = Strings.GetString(Singleton.GameData.CumulativeGoals[num].Title);
			component.RewardMoney.Text = GUIUtils.GetPriceStringForCurrency(cumulativeGoal.Reward, "hard");
			float unusedLocal1;
			float width = component.EmptyStar.GetComponent<PackedSprite>().width;
			float num2 = width * (float)cumulativeGoal.Target;
			num2 = num2 > 100f ? 100f : num2;
			float num3 = num2 / (float)cumulativeGoal.Target;
			GameObject[] array = new GameObject[cumulativeGoal.Target];
			for (int i = 0; i < cumulativeGoal.Target; i++)
			{
				if (i + 1 <= cumulativeGoal.Progress)
				{
					array[i] = UnityEngine.Object.Instantiate(component.FullStar) as GameObject;
					array[i].name = "FullStar" + i.ToString();
					array[i].transform.position = component.FullStar.transform.position;
					array[i].transform.parent = component.FullStar.transform.parent;
					component.AddStar(array[i], true);
				}
				else
				{
					array[i] = UnityEngine.Object.Instantiate(component.EmptyStar) as GameObject;
					array[i].name = "EmptyStar" + i.ToString();
					array[i].transform.position = component.EmptyStar.transform.position;
					array[i].transform.parent = component.EmptyStar.transform.parent;
					component.AddStar(array[i], false);
				}
				UIAlign component2 = array[i].GetComponent<UIAlign>();
				if (cumulativeGoal.Target == 1)
				{
					component2.m_margin = Vector2.zero;
				}
				else
				{
					component2.m_margin = new Vector2(-num2 / 2f + width / 4f + (float)i * num3, 0f);
				}
			}
			component.EmptyStar.active = false;
			component.FullStar.active = false;
			SoundManager.PlayExclusive("UI/collection_traded", false);
		}
	}

	private void ScrollOnlyGoals()
	{
		Goal.Type[] goals = PlayerData.Instance.GetGoals();
		int num = (!(goals.Length <= 4) ? 4 : goals.Length);
		_goalPanels = new GameObject[num];
		int num2 = 0;
		foreach (Goal.Type goalType in goals)
		{
			Goal goal = PlayerData.Instance.GetGoal(goalType);
			_goalPanels[num2] = Singleton.UI.Instantiate("GoalPanel");
			_goalPanels[num2].transform.parent = transform;
			_goalPanels[num2].transform.position = transform.position + Vector3.forward * -8f;
			SetGoalAlignment(_goalPanels[num2], GoalMargin[num2], GoalScrollSpeedAndDelay[num2].x, GoalScrollSpeedAndDelay[num2].y);
			GoalGUI component = _goalPanels[num2].GetComponent<GoalGUI>();
			component.Title.Text = goal.Description;
			component.Progress.Text = string.Format(Strings.GetString("{0} more to go"), goal.Target - goal.Progress);
			component.GoalType = goalType;
			UITextAutoScale uITextAutoScale = component.Title.gameObject.AddComponent<UITextAutoScale>();
			uITextAutoScale.MaxWidth = 83f;
			uITextAutoScale.MaxHeight = 1000f;
			for (int j = 0; j < component.Stars.Length; j++)
			{
				component.Stars[j].active = j + 1 <= goal.Reward;
			}
			num2++;
			if (num2 >= num)
			{
				break;
			}
		}
	}

	private void UpdateGoalPanels()
	{
		int num = -1;
		for (int i = 0; i < _goalPanels.Length; i++)
		{
			Goal.Type goalType = _goalPanels[i].GetComponent<GoalGUI>().GoalType;
			if (!PlayerData.Instance.HasGoal(goalType) || (PlayerData.Instance.HasGoal(goalType) && PlayerData.Instance.GetGoal(goalType).IsCompleted))
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			Vector3 vector = _goalPanels[num].GetComponent<UIAlign>().m_margin;
			SetGoalAnimation(_goalPanels[num], GoalRemoveSpeedAndDelay.x, GoalRemoveSpeedAndDelay.y);
			UnityEngine.Object.Destroy(_goalPanels[num], 2f);
			_goalPanels[num] = null;
			for (int j = num + 1; j < _goalPanels.Length; j++)
			{
				GoalGUIAnimation goalGUIAnimation = _goalPanels[j].AddComponent<GoalGUIAnimation>();
				goalGUIAnimation.Animate(vector, GoalUpdateSpeedAndDelay.x, GoalUpdateSpeedAndDelay.y);
				vector = _goalPanels[j].GetComponent<UIAlign>().m_margin;
				_goalPanels[j - 1] = _goalPanels[j];
			}
			Array.Resize(ref _goalPanels, _goalPanels.Length - 1);
			transform.root.BroadcastMessage("GenerateGoals", SendMessageOptions.RequireReceiver);
			Goal.Type[] goals = PlayerData.Instance.GetGoals();
			if (goals != null)
			{
				foreach (Goal.Type type in goals)
				{
					bool flag = false;
					for (int l = 0; l < _goalPanels.Length; l++)
					{
						if (_goalPanels[l].GetComponent<GoalGUI>().GoalType == type)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Goal goal = PlayerData.Instance.GetGoal(type);
						Array.Resize(ref _goalPanels, _goalPanels.Length + 1);
						int num2 = _goalPanels.Length - 1;
						_goalPanels[num2] = Util.CreateGameObject("GoalPanel");
						_goalPanels[num2].transform.parent = transform;
						_goalPanels[num2].transform.position = transform.position + Vector3.forward * -8f;
						SetGoalAlignment(_goalPanels[num2], GoalMargin[num2], GoalScrollSpeedAndDelay[num2].x, GoalScrollSpeedAndDelay[num2].y);
						GoalGUI component = _goalPanels[num2].GetComponent<GoalGUI>();
						component.Title.Text = goal.Description;
						component.Progress.Text = string.Format(Strings.GetString("{0} more to go"), goal.Target - goal.Progress);
						component.GoalType = type;
						for (int m = 0; m < component.Stars.Length; m++)
						{
							component.Stars[m].active = m + 1 <= goal.Reward;
						}
						break;
					}
				}
			}
		}
	}

	private void ScrollAllGoals()
	{
		if ((bool)_cumulativeGoalPanel)
		{
			RollupAllGoals();
		}
		else if (PlayerData.Instance.HasCumulativeGoal() && PlayerData.Instance.HasGoals())
		{
			_rollupGoals = false;
			ScrollCumulativeGoal(CumulativeGoalScrollSpeedAndDelay);
			ScrollOnlyGoals();
			SoundManager.PlayExclusive("UI/collection_traded", false);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		if (Singleton.Exists())
		{
			Singleton.PlayerData.OnHardCurrencyChanged -= new PlayerData.OnCurrencyChangedDelegate(OnHardCurrencyChanged);
			Singleton.PlayerData.OnSoftCurrencyChanged -= new PlayerData.OnCurrencyChangedDelegate(OnSoftCurrencyChanged);
			Singleton.PlayerData.OnXPChanged -= new PlayerData.OnXPChangedDelegate(OnXPChanged);
		}
	}

	public void ShowGoals(bool flag)
	{
		GoalButton.gameObject.SetActiveRecursively(flag);
	}

	public void ShowBottomControls(bool flag)
	{
		BottomPanel.SetActiveRecursively(flag);
		BackButton.gameObject.SetActiveRecursively(flag);
		AchievementsButton.gameObject.SetActiveRecursively(!flag);
		_showFacebookButtons = !flag && QualitySetter.FacebookEnabled;
		UpdateFacebookButtons();
	}

	private void ShowAchievements()
	{
	}

	public void ShowGetFreeTutorialIcon(bool show)
	{
		MeshRenderer component = GetFreeTutorialIcon.GetComponent<MeshRenderer>();
		if (show)
		{
			component.enabled = true;
			GetFreeTutorialIcon.SetTexture(AssetBundles.Load("Assets/UI/Shop/Special/button_get_free_gold_2.png") as Texture2D);
			_showTutorialGetFreeIcon = true;
		}
		else
		{
			component.enabled = false;
			_showTutorialGetFreeIcon = false;
		}
	}

	public static bool IsShowTutorialGetFreeIcon
	{
		get
		{
			return _showTutorialGetFreeIcon;
		}
	}
}
