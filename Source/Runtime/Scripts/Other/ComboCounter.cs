using System;
using Glu.Localization;
using UnityEngine;

public class ComboCounter : MonoBehaviour
{
	public float duration = 10f;

	public float durationReduction = 0.97f;

	public float counterToRatio = 0.5f;

	public bool showRatio = true;

	private CountdownTimer timer = new CountdownTimer();

	private int counter;

	private void Start()
	{
		Singleton.GameEvents.OnEnemyKill += new Action<Character, Unit, Weapon, Consumable>(OnKill);
	}

	private void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnKill);
		}
	}

	private void OnKill(Character s, Unit u, Weapon w, Consumable c)
	{
		if (!(s is Human))
		{
			return;
		}
		timer.Run(duration * Mathf.Pow(durationReduction, counter));
		counter++;
	}

	private void Update()
	{
		if (!timer.IsRunning())
		{
			counter = 0;
		}
		if (counter > 0)
		{
			Singleton.UI.Hud.comboCounter.Hide(false);
			Singleton.UI.Hud.comboText.Hide(false);
			Singleton.UI.Hud.comboText.Text = (showRatio ? string.Format("{0} {1:0.0}X", Strings.GetString("Combo"), Ratio()) : string.Format("{0} {1:0}", Strings.GetString("Combo"), counter));
			Singleton.UI.Hud.comboCounter.Value = 1f - timer.Progress();
		}
		else
		{
			Singleton.UI.Hud.comboCounter.Hide(true);
			Singleton.UI.Hud.comboText.Hide(true);
		}
	}

	public float Ratio()
	{
		return 1f + (float)counter * counterToRatio;
	}
}
