using System;
using Glu.Localization;
using UnityEngine;

public class DamageBooster : Powerup
{
	[NonSerialized]
	public DamageBoost conf;

	public void Setup(DamageBoost conf_)
	{
		conf = conf_;
	}

	private void Start()
	{
		Scene.Camera.OnDamageBoost(conf.duration);
		Scene.ProjectileManager.OnDamageBoost(conf.duration);
		Weapon.OnDamageBoost(conf.duration);
		if ((bool)GetComponent<Player>())
		{
			Singleton.UI.Hud.ShowInfo(Strings.GetString("Rampage!"));
			SoundManager.Play("PowerUps/damage_boost", false);
		}
		Invoke("Remove", conf.duration);
	}

	private void Remove()
	{
		if ((bool)GetComponent<Player>())
		{
			Singleton.UI.Hud.ShowInfo(Strings.GetString("You feel normal again..."));
		}
		UnityEngine.Object.Destroy(this);
	}
}
