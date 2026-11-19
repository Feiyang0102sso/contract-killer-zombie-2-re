using UnityEngine;

public class NavMeshPoint : MonoBehaviourAndLog
{
	protected virtual void Awake()
	{
		LogTo("Spawn.SpawnPoint");
		BindToNavMesh();
	}

	protected virtual void Start()
	{
	}

	protected void BindToNavMesh()
	{
		UnityEngine.AI.NavMeshHit hit;
		UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 10f, -1);
		if (hit.hit)
		{
			transform.position = hit.position;
		}
		else
		{
		}
	}

	protected virtual void OnDrawGizmos()
	{
		float unusedLocal1;
		Gizmos.color = Color.grey;
		Gizmos.DrawCube(transform.position + 0.5f * Vector3.up * 2.5f, new Vector3(1f, 2.5f, 1f));
	}
}
