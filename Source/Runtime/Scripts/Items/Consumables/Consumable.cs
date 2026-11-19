using System;
using Glu.AssetBundles;
using UnityEngine;

public abstract class Consumable : Item
{
	public Action OnConsume = () =>
	{
	};

	public float Cooldown = 1f;

	public string ConsumeSound;

	private float _lastConsumeTime;

	public string HudIconTexturePath;

	public int Count { get; set; }

	public bool SyncWithPlayer { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Count = 0;
		SyncWithPlayer = false;
	}

	public float GetRechargeTimeLeft()
	{
		return Cooldown - (Time.time - _lastConsumeTime);
	}

	public virtual bool IsReadyToConsume(GameObject consumer)
	{
		return GetRechargeTimeLeft() <= 0f;
	}

	public virtual void Consume(GameObject consumer)
	{
		if (!(Count > 0))
		{
			return;
		}
		_lastConsumeTime = Time.time;
		Count--;
		if (ConsumeSound.Length > 0)
		{
			SoundManager.PlayExclusive(ConsumeSound, false);
		}
		OnConsume();
		Sync();
		Singleton.PlayerData.ProgressGoal(Goal.Type.UseConsumables, 1);
	}

	public override void Stack(int count)
	{
		Count += count;
		Sync();
	}

	public override GameObject CreateHudIcon()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(AssetBundles.Load("Assets/Prefabs/Hud/ConsumableScrollItem.prefab")) as GameObject;
		if (HudIconTexturePath != null && HudIconTexturePath.Length > 0)
		{
			gameObject.GetComponentInChildren<SimpleSprite>().gameObject.GetComponent<Renderer>().material.mainTexture = Resources.Load(HudIconTexturePath) as Texture2D;
		}
		gameObject.name = name;
		return gameObject;
	}

	public void Sync()
	{
		if (!(SyncWithPlayer))
		{
			return;
		}
		Singleton.PlayerData.SetConsumableCount(gameObject.name, Count);
	}
}
