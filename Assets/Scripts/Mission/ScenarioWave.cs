using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class ScenarioWave : SpawnScenario
{
	public enum Difficulty
	{
		Zero,
		One,
		Small,
		Medium
	}

	public float initialDelay;

	public Difficulty difficulty = Difficulty.Medium;

	public float specialQuantity = 0.1f;

	public Spawner.SpecialType specialType;

	public float tempo = 1f;

	public CustomInterpolators.EASING_TYPE TimeDistributionType;

	public bool showNotice = true;

	public SpawnPoint[] specificPoints = new SpawnPoint[0];

	public float PercentFront = 40f;

	public float PercentBack = 30f;

	public float PercentVisible = 20f;

	public int pointsFront = 1;

	public int pointsBack = 1;

	public float rangeMin = 5f;

	private int zombiesQuantity = -1;

	private int specialQuantityActual = -1;

	[NonSerialized]
	public bool manualActivate;

	private Spawner.PointDesc descFront;

	private Spawner.PointDesc descBack;

	private Spawner.PointDesc descVisible;

	private Spawner.PointDesc descOther;

	private Spawner.PointDesc descSpec;

	protected override void Awake()
	{
		base.Awake();
		descFront = Spawner.PointDesc.InvisibleAhead(pointsFront, rangeMin);
		descBack = Spawner.PointDesc.InvisibleBehind(pointsBack, rangeMin);
		descVisible = Spawner.PointDesc.VisibleAhead(-1);
		descOther = Spawner.PointDesc.Invisible(Mathf.Min(pointsFront, pointsBack), rangeMin);
		descSpec = (!(specificPoints.Length <= 0) ? Spawner.PointDesc.SpecificPoints(specificPoints) : null);
	}

	public void OnActivate()
	{
		if (!manualActivate)
		{
			Generate();
		}
	}

	public void OnActivateManual()
	{
		Generate();
	}

	public int GetQuantity()
	{
		if (zombiesQuantity >= 0)
		{
			return zombiesQuantity;
		}
		GameData.WaveData currentPlayerLevelWaveData = Singleton.GameData.GetCurrentPlayerLevelWaveData();
		zombiesQuantity =
			difficulty == Difficulty.Zero ? 0 :
			difficulty == Difficulty.One ? 1 :
			difficulty == Difficulty.Small ? currentPlayerLevelWaveData.SmallCommon :
			difficulty == Difficulty.Medium ? currentPlayerLevelWaveData.MediumCommon : -1;
		return zombiesQuantity;
	}

	public int GetSpecialQuantity()
	{
		if (specialQuantityActual >= 0)
		{
			return specialQuantityActual;
		}
		return specialQuantityActual = Util.RoundToIntProb(specialQuantity);
	}

	protected override void Generate()
	{
		int quantity = GetQuantity();
		int num = GetSpecialQuantity();
		float num2 = 100f - PercentFront - PercentBack - PercentVisible;
		if (num2 < 0f)
		{
			log.Warn(gameObject, "sum of percents above 100 (is {0}) for '{1}'", num2, gameObject.name);
		}
		int num3 = Mathf.FloorToInt(PercentFront / 100f * (float)quantity);
		int num4 = Mathf.FloorToInt(PercentBack / 100f * (float)quantity);
		int num5 = Mathf.FloorToInt(PercentVisible / 100f * (float)quantity);
		int count = quantity - num3 - num4 - num5;
		Invoke("OnSpawn", initialDelay);
		AddSpawnSequence(num3, Scene.Spawner.CommonOrUncommonZombie(), null, null, descSpec != null ? descSpec : descFront, initialDelay, tempo, TimeDistributionType);
		AddSpawnSequence(num4, Scene.Spawner.CommonOrUncommonZombie(), null, null, descSpec != null ? descSpec : descBack, initialDelay, tempo, TimeDistributionType);
		AddSpawnSequence(num5, Scene.Spawner.CommonOrUncommonZombie(), null, null, descSpec != null ? descSpec : descVisible, initialDelay, tempo, TimeDistributionType);
		AddSpawnSequence(count, Scene.Spawner.CommonOrUncommonZombie(), null, null, descSpec != null ? descSpec : descOther, initialDelay, tempo, TimeDistributionType);
		if (specialQuantity > 0f)
		{
			Spawner.PointDesc pointDesc = Util.RandomByWeights(new Dictionary<Spawner.PointDesc, float>
			{
				{ descFront, PercentFront },
				{ descBack, PercentBack },
				{ descVisible, PercentVisible },
				{ descOther, num2 }
			});
			AddSpawnSequence(specialQuantityActual, Scene.Spawner.SpecialZombie(specialType), null, null, descSpec != null ? descSpec : pointDesc, initialDelay, tempo, TimeDistributionType);
		}
		base.Generate();
	}

	private void OnSpawn()
	{
		int unusedLocal1;
		if (zombiesQuantity >= 4)
		{
			if (showNotice)
			{
				Singleton.UI.Hud.ShowAlert(Strings.GetString("Incoming zombies!"));
			}
			SoundManager.Play("Piper/wave", false);
		}
		MusicManager.OnSpawn(zombiesQuantity, (int)specialQuantity);
		foreach (Survivor survivor in Scene.Survivors)
		{
			survivor.OnWaveStarted();
		}
	}
}
