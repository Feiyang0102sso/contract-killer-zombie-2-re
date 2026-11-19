using UnityEngine;

public class SpawnPoint : NavMeshPoint
{
	protected GameObject Spawn(Object whatToSpawn, float radius, bool rotateRandomly)
	{
		log.Debug(base.gameObject, "spawned '{0}'", whatToSpawn.name);
		Vector3 vector = transform.position + Util.RandomInCircle() * Random.Range(0f, radius);
		GameObject gameObject = Scene.Spawner.GetFromHell(whatToSpawn.name, null, vector);
		if (!gameObject)
		{
			gameObject = Object.Instantiate(whatToSpawn, vector, Quaternion.identity) as GameObject;
		}
		gameObject.transform.parent = Scene.Groups.Spawned;
		if (rotateRandomly)
		{
			gameObject.transform.Rotate(Vector3.up, Random.Range(0, 360));
		}
		else
		{
			gameObject.transform.forward = transform.forward;
		}
		return gameObject;
	}

	public virtual GameObject Spawn(Object whatToSpawn)
	{
		return Spawn(whatToSpawn, 1f, true);
	}

	protected override void OnDrawGizmos()
	{
		float unusedLocal1;
		Gizmos.color = new Color(0.13f, 0.7f, 0f);
		Gizmos.DrawCube(transform.position + 0.5f * Vector3.up * 0.5f, Vector3.one * 0.5f);
	}
}
