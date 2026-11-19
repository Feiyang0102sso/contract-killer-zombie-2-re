using System.Collections.Generic;
using System.Reflection;
using Glu.Localization;
using UnityEngine;

public class AchievementsWindow : UIWindow
{
	private const float topOffset = 18.5f;

	public UIExpandSprite Back;

	public UIButton LeaderboardsButton;

	public UIButton StatisticsButton;

	public UIButton AchievementsButton;

	public UISingleSprite Background;

	public SpriteText TitleText;

	public UIScrollList List;

	public UIPressableButton ChallengeButton;

	public float ScaleFrom = 1f;

	public float ScaleTo = 0.5f;

	public float ScaleFactor = 3.5f;

	public float AlphaFrom = 1f;

	public float AlphaTo;

	public float AlphaFactor = 1.5f;

	private List<AchievementPlate> _items = new List<AchievementPlate>();

	private List<StatisticsPlate> _stats = new List<StatisticsPlate>();

	private static bool _isOpened = false;

	protected override void Awake()
	{
		_position = new Vector2(0f, -9.25f);
		base.Awake();
		TexturesReplacer.ReplaceTexturesAccordingToQuality();
	}

	protected override void Start()
	{
		base.Start();
		Back.SetSize(GUIUtils.ScreenWidth(), GUIUtils.ScreenHeight() - 18.5f);
		Background.transform.localPosition = new Vector3(0f, (GUIUtils.ScreenHeight() - 18.5f - Background.height) * 0.5f, -0.5f);
		CommonGUI componentInChildren = GetComponentInChildren<CommonGUI>();
		componentInChildren.ShowGoals(false);
		componentInChildren.ShowBottomControls(true);
		GUIUtils.HookButton(componentInChildren.BackButton, Close);
		GUIUtils.HookButton(LeaderboardsButton, OnLeaderboardsButtonTap);
		GUIUtils.HookButton(StatisticsButton, OnStatisticsButtonTap);
		GUIUtils.HookButton(AchievementsButton, OnAchievementsButtonTap);
		GUIUtils.HookButton(ChallengeButton, OnChallengeButtonTap);
		OnAchievementsButtonTap();
		UpdateItemsScaleEffect(true);
		_isOpened = true;
	}

	protected override void OnDestroy()
	{
		_isOpened = false;
	}

	private void AddItem(AchievementPlate plate)
	{
		AddItem(plate.gameObject);
		_items.Add(plate);
	}

	private void AddItem(StatisticsPlate plate)
	{
		AddItem(plate.gameObject);
		_stats.Add(plate);
	}

	private void AddItem(GameObject plate)
	{
		GameObject gameObject = new GameObject("element");
		UIListItemContainer uIListItemContainer = gameObject.AddComponent<UIListItemContainer>();
		plate.transform.parent = uIListItemContainer.transform;
		plate.transform.localPosition = Vector3.zero;
		uIListItemContainer.FindOuterEdges();
		Vector2 bottomRightEdge = uIListItemContainer.BottomRightEdge;
		List.InsertItem(uIListItemContainer.gameObject.GetComponent(typeof(IUIListObject)) as IUIListObject, List.Count);
		uIListItemContainer.ScanChildren();
	}

	private void Update()
	{
		UpdateItemsScaleEffect(false);
	}

	private void UpdateItemsScaleEffect(bool all)
	{
		foreach (AchievementPlate item in _items)
		{
			if (!(item.gameObject.active || all))
			{
				continue;
			}
			Transform transform = item.transform;
			float num = Mathf.Lerp(ScaleFrom, ScaleTo, Mathf.Pow(Mathf.Abs(transform.position.y / (List.viewableArea.y * 0.5f)), ScaleFactor));
			transform.localScale = new Vector3(num, num, num);
			float alpha = Mathf.Lerp(AlphaFrom, AlphaTo, Mathf.Pow(Mathf.Abs(transform.position.y / (List.viewableArea.y * 0.5f)), AlphaFactor));
			item.SetAlpha(alpha);
		}
		foreach (StatisticsPlate stat in _stats)
		{
			if (!(stat.gameObject.active || all))
			{
				continue;
			}
			Transform transform2 = stat.transform;
			float num2 = Mathf.Lerp(ScaleFrom, ScaleTo, Mathf.Pow(Mathf.Abs(transform2.position.y / (List.viewableArea.y * 0.5f)), ScaleFactor));
			transform2.localScale = new Vector3(num2, num2, num2);
			float alpha2 = Mathf.Lerp(AlphaFrom, AlphaTo, Mathf.Pow(Mathf.Abs(transform2.position.y / (List.viewableArea.y * 0.5f)), AlphaFactor));
			stat.SetAlpha(alpha2);
		}
	}

	private void OnChallengeButtonTap()
	{
		if (Social.localUser.authenticated)
		{
			Social.ShowAchievementsUI();
		}
		else
		{
			Social.localUser.Authenticate((bool success) =>
			{
			});
		}
	}

	private void OnLeaderboardsButtonTap()
	{
		if (Social.localUser.authenticated)
		{
			Social.ShowLeaderboardUI();
		}
		else
		{
			Social.localUser.Authenticate((bool success) =>
			{
			});
		}
	}

	private void OnStatisticsButtonTap()
	{
		bool unusedLocal1;
		AchievementsButton.gameObject.active = true;
		StatisticsButton.gameObject.active = false;
		TitleText.Text = Strings.GetString("Statistics");
		List.ClearList(true);
		_items.Clear();
		_stats.Clear();
		PropertyInfo[] properties = Singleton.PlayerData.PlayerStatistics.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			GameObject original = Resources.Load("StatisticsPlate") as GameObject;
			StatisticsPlate component = (Object.Instantiate(original) as GameObject).GetComponent<StatisticsPlate>();
			component.Init(Singleton.PlayerData.PlayerStatistics, propertyInfo.Name, Strings.GetParticularString(propertyInfo.DeclaringType.Name, propertyInfo.Name));
			AddItem(component);
		}
		List.ScrollListTo(2f / (float)List.Count);
	}

	private void OnAchievementsButtonTap()
	{
		bool implemented;
		StatisticsButton.gameObject.active = true;
		AchievementsButton.gameObject.active = false;
		TitleText.Text = Strings.GetString("Achievements");
		List.ClearList(true);
		_items.Clear();
		_stats.Clear();
		GameObject original = Resources.Load("AchievementPlate") as GameObject;
		foreach (GameData.AchievementData achievement in Singleton.GameData.Achievements)
		{
			if (!Singleton.Achievements.IsImplemented(achievement))
			{
				continue;
			}
			AchievementPlate component = (Object.Instantiate(original) as GameObject).GetComponent<AchievementPlate>();
			component.Init(achievement);
			AddItem(component);
		}
		List.ScrollListTo(1f / (float)List.Count);
	}

	public static bool IsOpened
	{
		get
		{
			return _isOpened;
		}
	}
}
