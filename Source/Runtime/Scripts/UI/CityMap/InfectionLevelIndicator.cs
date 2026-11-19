using System;
using UnityEngine;

public class InfectionLevelIndicator : MonoBehaviour
{
	private const float damping = 1f;

	public UIProgressBar ProgressBar;

	public SpriteText TimerText;

	public UISpriteBlinker Blinker;

	private void Awake()
	{
		gameObject.SetActiveRecursively(Singleton.GameData.InfectionLevel.Enabled);
		Blinker.SetOriginalColor(new Color(1f, 1f, 1f, 0f));
	}

	private void Start()
	{
		UpdateValue(false);
	}

	private void Update()
	{
		UpdateValue(true);
	}

	private void UpdateValue(bool smooth)
	{
		Blinker.Play(Singleton.PlayerData.IsInfectionLevelCritical());
		float infectionLevel = Singleton.PlayerData.InfectionLevel;
		float value = Mathf.Ceil(Singleton.PlayerData.InfectionLevel);
		float num = Util.FractionInInterval(infectionLevel, 0f, Singleton.GameData.InfectionLevel.Max);
		float num2 = Util.FractionInInterval(value, 0f, Singleton.GameData.InfectionLevel.Max);
		ProgressBar.Value = smooth ? Mathf.Lerp(ProgressBar.Value, num2, Time.deltaTime / Time.timeScale * 1f) : num2;
		float num3 = Singleton.PlayerData.MinutesLeftTillInfectionLevel(Mathf.Floor(infectionLevel));
		TimerText.Text = new DateTime(TimeSpan.FromMinutes(num3).Ticks).ToString("mm:ss");
		TimerText.transform.parent.gameObject.SetActiveRecursively(infectionLevel > 0f);
	}
}
