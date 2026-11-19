using System.Reflection;
using UnityEngine;

public class StatisticsPlate : MonoBehaviour
{
	public UIExpandSprite Back;

	public SpriteText TitleText;

	public SpriteText ProgressText;

	private void Awake()
	{
	}

	public void Init(PlayerStatistics statistics, string property, string title)
	{
		TitleText.Text = title;
		PropertyInfo property2 = statistics.GetType().GetProperty(property);
		if (property2 != null)
		{
			object value = property2.GetValue(statistics, null);
			bool flag = property.ToLower().Contains("rate");
			ProgressText.Text = value.ToString() + (flag ? " %" : string.Empty);
		}
		SetAlpha(1f);
	}

	public void SetAlpha(float alpha)
	{
		float titleAlpha = alpha;
		Color color = new Color(1f, 1f, 1f, alpha);
		Color color2 = new Color(1f, 1f, 1f, titleAlpha);
		Color colorYellowTitle = GUIUtils.ColorYellowTitle;
		colorYellowTitle.a = titleAlpha;
		Back.Color = color;
		TitleText.Color = colorYellowTitle;
		ProgressText.Color = color;
	}
}
