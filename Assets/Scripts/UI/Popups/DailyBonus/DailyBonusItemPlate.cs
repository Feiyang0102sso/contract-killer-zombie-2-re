using Glu.Localization;
using UnityEngine;

public class DailyBonusItemPlate : MonoBehaviour
{
	public UISingleSprite Icon;

	public SpriteText DayText;

	public SpriteText NameText;

	public SpriteText TodayText;

	public PackedSprite Back;

	public PackedSprite Crest;

	public PackedSprite TodayFrame;

	private Color _dayTextColor;

	private float _baseAlpha = 1f;

	private bool _alphaOnIcon = true;

	private bool _specialYellowGlowing;

	public bool AlphaOnIcon
	{
		get
		{
			return _alphaOnIcon;
		}
		set
		{
			_alphaOnIcon = value;
		}
	}

	public bool SpecialYellowGlowing
	{
		get
		{
			return _specialYellowGlowing;
		}
		set
		{
			_specialYellowGlowing = value;
		}
	}

	public bool IsToday { get; set; }

	private void Awake()
	{
	}

	public void Init(int day, int currentDay)
	{
		GameData.DailyBonusData data = Singleton.GameData.DailyBonus[day];
		DailyBonusPopup.SetupBonusTexture(Icon, data, true);
		DayText.Text = Strings.GetString("DAY ") + (day + 1);
		bool isMoney = false;
		string bonusText = DailyBonusPopup.GetBonusText(data, out isMoney);
		if (isMoney)
		{
			NameText.SetCharacterSize(12f);
		}
		NameText.Text = bonusText;
		_dayTextColor = (!(day != currentDay) ? GUIUtils.ColorYellowHard : Color.white);
		IsToday = day == currentDay;
		if (IsToday)
		{
			TodayText.Text = Strings.GetString("TODAY");
			Icon.gameObject.AddComponent<UIAnimationScale>().Setup(1.4f, 1f, 0.8f, 1.2f, EZAnimation.EASING_TYPE.BounceInOut);
			TodayFrame.Color = new Color(0.67f, 1f, 0.145f, 1f);
		}
		else
		{
			TodayText.Text = string.Empty;
			if (_specialYellowGlowing)
			{
				TodayFrame.Color = Color.yellow;
			}
			else
			{
				Object.Destroy(TodayFrame.renderer);
			}
		}
		if (day >= currentDay)
		{
			Object.Destroy(Crest.renderer);
		}
		else
		{
			Crest.gameObject.AddComponent<UIAnimationScale>().Setup(0f, 1f, 0.3f, 1f, EZAnimation.EASING_TYPE.BackOut);
		}
		if (day < currentDay)
		{
			_baseAlpha = 0.7f;
		}
		else if (day > currentDay)
		{
			_baseAlpha = 0.7f;
		}
		SetAlpha(1f);
	}

	public void SetAlpha(float alpha)
	{
		Color color = new Color(1f, 1f, 1f, alpha);
		alpha *= _baseAlpha;
		Color color2 = new Color(1f, 1f, 1f, alpha);
		Color colorYellowTitle = GUIUtils.ColorYellowTitle;
		colorYellowTitle.a = alpha;
		_dayTextColor.a = alpha;
		Back.Color = color2;
		DayText.Color = _dayTextColor;
		Icon.Color = (_alphaOnIcon ? color2 : color);
		NameText.Color = new Color(NameText.Color.r, NameText.Color.g, NameText.Color.b, alpha);
		TodayText.Color = new Color(TodayText.Color.r, TodayText.Color.g, TodayText.Color.b, alpha);
		Crest.Color = color;
	}
}
