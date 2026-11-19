using System;
using UnityEngine;

public class ScenarioCustomWave : SpawnScenario
{
	public string BankName = "SV_1";

	public int WaveId = 1;

	public CustomInterpolators.EASING_TYPE TimeDistributionType = CustomInterpolators.EASING_TYPE.ExpInOut3;

	public void OnActivate()
	{
		Generate();
	}

	protected override void Generate()
	{
		foreach (GameData.CustomWaveData customWave in Singleton.GameData.GetCustomWaves(BankName, WaveId))
		{
			int zombieCount = customWave.ZombieCount;
			int num = Mathf.FloorToInt((float)customWave.VisiblePercent * 0.01f * (float)zombieCount);
			int count = zombieCount - num;
			AddSpawnSequence(num, Scene.Spawner.CommonOrUncommonZombie(customWave.UncommonChance, customWave.Tier2Common, customWave.Tier2Uncommon), null, null, Spawner.PointDesc.Visible(customWave.MinDistance, customWave.MaxDistance), customWave.Delay, customWave.Tempo, TimeDistributionType);
			AddSpawnSequence(count, Scene.Spawner.CommonOrUncommonZombie(customWave.UncommonChance, customWave.Tier2Common, customWave.Tier2Uncommon), null, null, Spawner.PointDesc.Invisible(customWave.MinDistance, customWave.MaxDistance), customWave.Delay, customWave.Tempo, TimeDistributionType);
			if (customWave.Special.Length > 0)
			{
				foreach (string item in Util.MakeChoice(customWave.Special))
				{
					Spawner.SpecialType specialType = (Spawner.SpecialType)(int)Enum.Parse(typeof(Spawner.SpecialType), item, true);
					AddSpawnSequence(1, Scene.Spawner.SpecialZombie(specialType, customWave.Tier2Special), null, null, Spawner.PointDesc.Invisible(customWave.MinDistance, customWave.MaxDistance), customWave.Delay, customWave.Tempo, TimeDistributionType);
				}
			}
			if (customWave.Survivor.Length > 0)
			{
				Spawner.SurvivorType survivorType = (Spawner.SurvivorType)(int)Enum.Parse(typeof(Spawner.SurvivorType), customWave.Survivor, true);
				AddSpawnSequence(1, Scene.Spawner.Survivor(), null, PostSpawn(survivorType), Spawner.PointDesc.Invisible(customWave.MinDistance, customWave.MaxDistance), customWave.Delay, customWave.Tempo, TimeDistributionType);
			}
		}
		base.Generate();
	}

	private Action<GameObject> PostSpawn(Spawner.SurvivorType survivorType)
	{
		return (GameObject o) =>
		{
			if (survivorType == Spawner.SurvivorType.SurvivorUnarmed)
			{
				o.GetComponent<HumanInventory>().startWeapons = new HumanInventory.StartWeapon[0];
			}
		};
	}
}
