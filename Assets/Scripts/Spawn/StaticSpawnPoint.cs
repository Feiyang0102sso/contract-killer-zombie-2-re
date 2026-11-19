using UnityEngine;

public class StaticSpawnPoint : SpawnPoint
{
	public float SpawnChance = 80f;

	public GameObject SpawnUnit(Object whatToSpawn)
	{
		return Spawn(whatToSpawn);
	}

	protected override void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(gameObject.transform.position, Vector3.one * 0.5f);
	}
}
