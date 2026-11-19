using System;
using System.Reflection;
using Glu.AssetBundles;
using Glu.Localization;
using UnityEngine;

public class AchievementPlate : MonoBehaviour
{
	public UIExpandSprite Back;

	public SpriteText TitleText;

	public SpriteText DescriptionText;

	public SpriteText ProgressText;

	public UISingleSprite Icon;

	private GameData.AchievementData _achievement;

	private bool _completed;

	private void Awake()
	{
	}

	private void Update()
	{
		if (StoreScreen.ShopIsOpen && !AchievementsWindow.IsOpened)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public void Init(GameData.AchievementData achievement)
	{
		_achievement = achievement;
		_completed = Singleton.PlayerData.IsAchievementCompleted(_achievement);
		TitleText.Text = Strings.GetString(_achievement.Name);
		DescriptionText.Text = Strings.GetString(_completed ? _achievement.EarnedDescription : _achievement.Description);
		Texture2D texture = AssetBundles.Load("Assets/UI/Achievements/" + _achievement.Icon) as Texture2D;
		Icon.SetTexture(texture);
		int num = Mathf.RoundToInt(Singleton.Achievements.GetAchievementProgress(achievement) * 100f);
		ProgressText.Text = _completed ? string.Empty : num.ToString() + "%";
		if (!Singleton.Achievements.IsImplemented(achievement))
		{
			TitleText.Text = "NOT IMPLEMENTED YET";
			DescriptionText.Text = "NOT IMPLEMENTED YET";
		}
		SetAlpha(1f);
	}

	public void Init(PlayerStatistics statistics, string field, string title)
	{
		int num = 0;
		FieldInfo field2 = statistics.GetType().GetField(field);
		if (field2 != null)
		{
			num = (int)field2.GetValue(statistics);
		}
		else
		{
			PropertyInfo property = statistics.GetType().GetProperty(field);
			if (property != null)
			{
				num = (int)property.GetValue(statistics, null);
			}
		}
		_achievement = null;
		_completed = false;
		TitleText.Text = title;
		ProgressText.Text = num.ToString();
		SetAlpha(1f);
	}

	public void SetAlpha(float alpha)
	{
		float a = alpha * (_completed ? 1f : 0.5f);
		Color color = new Color(1f, 1f, 1f, alpha);
		Color color2 = new Color(1f, 1f, 1f, a);
		Color colorYellowTitle = GUIUtils.ColorYellowTitle;
		colorYellowTitle.a = a;
		Back.Color = color;
		TitleText.Color = colorYellowTitle;
		if ((bool)DescriptionText)
		{
			DescriptionText.Color = color;
		}
		ProgressText.Color = color;
		if ((bool)Icon)
		{
			Icon.Color = color2;
		}
	}

	public static void PopupComplete(GameData.AchievementData achievement)
	{
		float unusedLocal1;
		float unusedLocal2;
		Action action = () =>
		{
			AchievementPlate plate = Util.CreateGameObject("AchievementPlate").GetComponent<AchievementPlate>();
			plate.Init(achievement);
			plate.transform.localPosition = new Vector3(0f, 0f, -98f);
			UIAlign uIAlign = plate.gameObject.AddComponent<UIAlign>();
			uIAlign.m_horizontal = UIAlign.Horizontal.Center;
			uIAlign.m_vertical = UIAlign.Vertical.Bottom;
			uIAlign.m_margin = new Vector2(-22.8f, 16.5f);
			uIAlign.UpdateAlignment();
			UnityEngine.Object.Destroy(uIAlign);
			plate.gameObject.AddComponent<UIAnimationScale>().Setup(1.5f, 1f, 0f, 1.1f, EZAnimation.EASING_TYPE.ExponentialInOut).RunAnimation();
			plate.gameObject.AddComponent<UIAnimationMove>().Setup(0f, -50f, 0f, 1.1f, EZAnimation.EASING_TYPE.ExponentialInOut).RunAnimation();
			UIAnimation uIAnimation = plate.gameObject.AddComponent<UIAnimationAlpha>().Setup(0.7f, 1f, 0f, 1.1f, EZAnimation.EASING_TYPE.ExponentialIn);
			uIAnimation.OnFinishDelegate = () =>
			{
				UIAnimationAlpha uIAnimationAlpha = plate.gameObject.AddComponent<UIAnimationAlpha>();
				uIAnimationAlpha.Setup(1f, 0f, 3f, 1.1f, EZAnimation.EASING_TYPE.ExponentialIn);
				uIAnimationAlpha.DestroyObjectOnFinish = true;
				uIAnimationAlpha.RunAnimation();
			};
			uIAnimation.RunAnimation();
		};
		Singleton.UI.AddQueuedAction(action, 4.6f);
	}
}
