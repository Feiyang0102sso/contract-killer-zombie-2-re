using System;
using UnityEngine;

public class SmartSpawnPoint : SpawnPoint
{
	[Serializable]
	private class PathPointInfo
	{
		public enum RelativeState
		{
			AHEAD,
			BEHIND,
			UNDETERMINED
		}

		public bool enabled;

		public float distance;

		public RelativeState relativeState;
	}

	public enum Appearing
	{
		None,
		Floor,
		Wall,
		UnderWall,
		Ceiling,
		Fence
	}

	public const float MinDistanceToBeInvisibleByPlayer = 10f;

	public const float MaxDistanceToBeEnabled = 35f;

	public bool IgnoreMaxDistanceLimitation;

	private PathPointInfo[] _pathPointsInfo;

	public bool Marked;

	private float visibleSpawnDelay;

	private CountdownTimer _visibleSpawnDelayTimer = new CountdownTimer();

	public bool PlayerCanSee { get; private set; }

	public bool IsAheadPlayerPath { get; private set; }

	public bool IsUndeterminedRelativeState { get; private set; }

	public bool Enabled { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		PlayerCanSee = false;
		IsAheadPlayerPath = false;
		IsUndeterminedRelativeState = false;
		if (gameObject.name.StartsWith("SpawnVisible"))
		{
			visibleSpawnDelay = 4f;
		}
	}

	public void Init()
	{
		CalcPathsLength();
	}

	public float GetApproximateDistanceFromPlayer()
	{
		return _pathPointsInfo[Scene.PlayerProgress.CurrentPathIndex].distance;
	}

	public bool IsVisibleSpawn()
	{
		return visibleSpawnDelay > 0f;
	}

	public bool IsVisibleSpawnWorking()
	{
		return !_visibleSpawnDelayTimer.IsRunning();
	}

	public void UpdateData(Player player)
	{
		if (_pathPointsInfo == null)
		{
			return;
		}
		Vector3 vector = transform.position + Vector3.up * 1f;
		Vector3 vector2 = player.transform.position + Vector3.up * player.Height() * 0.75f;
		float magnitude = (vector - vector2).magnitude;
		PathPointInfo pathPointInfo = _pathPointsInfo[Scene.PlayerProgress.CurrentPathIndex];
		Enabled = pathPointInfo.enabled;
		if (!Enabled)
		{
			return;
		}
		PlayerCanSee = magnitude < 10f ? true : !Util.AnyObstacles(vector2, vector);
		IsAheadPlayerPath = pathPointInfo.relativeState == PathPointInfo.RelativeState.AHEAD;
		IsUndeterminedRelativeState = pathPointInfo.relativeState == PathPointInfo.RelativeState.UNDETERMINED;
	}

	public override GameObject Spawn(UnityEngine.Object whatToSpawn)
	{
		if (!IsVisibleSpawn())
		{
			return Spawn(whatToSpawn, 0.5f, true);
		}
		string str = base.gameObject.GetParent().name;
		bool flag = Util.TryRemovePrefix(ref str, "Spawn");
		Appearing appearing = Appearing.None;
		try
		{
			appearing = (Appearing)(int)Enum.Parse(typeof(Appearing), str);
		}
		catch (Exception)
		{
			log.Error(base.gameObject, "unknown appearing type: '{0}'", str);
		}
		_visibleSpawnDelayTimer.Run(visibleSpawnDelay);
		bool rotateRandomly = appearing == Appearing.Ceiling;
		GameObject gameObject = Spawn(whatToSpawn, 0f, rotateRandomly);
		GameObject gameObject2 = base.gameObject.GetParent().FindChildInHierarchy("SpawnDoor");
		if ((bool)gameObject2)
		{
			gameObject2.GetComponent<SpawnDoor>().OnSpawn();
		}
		if (appearing == Appearing.Ceiling)
		{
			gameObject2 = base.gameObject.GetParent().FindCustomChild((GameObject n) => n.name.StartsWith("Door"));
			if ((bool)gameObject2)
			{
				Rigidbody component = gameObject2.GetComponent<Rigidbody>();
				float angularSpeed;
				if (component.isKinematic)
				{
					component.isKinematic = false;
					component.sleepVelocity = 0.2f;
					component.sleepAngularVelocity = 0.2f;
					component.velocity = -Vector3.up * 15f;
					component.angularVelocity = new Vector3(Util.Range(-10f, 10f), 0f, Util.Range(-10f, 10f));
					UnityEngine.Object.Destroy(gameObject2, 10f);
				}
			}
		}
		if ((bool)gameObject.GetComponent<Zombie>())
		{
			gameObject.GetComponent<Zombie>().Appear(appearing);
		}
		else
		{
		}
		return gameObject;
	}

	private void CalcPathsLength()
	{
		int count = Scene.PlayerProgress.GetPath().Count;
		if (count == 0)
		{
			return;
		}
		_pathPointsInfo = new PathPointInfo[count];
		NavMeshPath path = new NavMeshPath();
		for (int i = 0; i < count; i++)
		{
			_pathPointsInfo[i] = new PathPointInfo();
			if (!IgnoreMaxDistanceLimitation && (transform.position - Scene.PlayerProgress.GetPath()[i]).sqrMagnitude > 1225f)
			{
				_pathPointsInfo[i].distance = float.PositiveInfinity;
				_pathPointsInfo[i].enabled = false;
				continue;
			}
			_pathPointsInfo[i].enabled = true;
			bool pathFound = NavMesh.CalculatePath(transform.position, Scene.PlayerProgress.GetPath()[i], -1, path);
			if (!pathFound)
			{
				log.Error(gameObject, "no path to path point {0}", Scene.PlayerProgress.GetPath()[i]);
				_pathPointsInfo[i].distance = float.PositiveInfinity;
			}
			else
			{
				_pathPointsInfo[i].distance = PathHelper.GetPathLength(path);
			}
		}
		_pathPointsInfo[0].relativeState = PathPointInfo.RelativeState.AHEAD;
		_pathPointsInfo[count - 1].relativeState = PathPointInfo.RelativeState.BEHIND;
		for (int j = 1; j < count - 1; j++)
		{
			float distance = _pathPointsInfo[j - 1].distance;
			float distance2 = _pathPointsInfo[j + 1].distance;
			bool flag = _pathPointsInfo[j].distance - distance > 0f;
			bool flag2 = distance2 - _pathPointsInfo[j].distance > 0f;
			if (flag && flag2)
			{
				_pathPointsInfo[j].relativeState = PathPointInfo.RelativeState.BEHIND;
			}
			else if (!flag && !flag2)
			{
				_pathPointsInfo[j].relativeState = PathPointInfo.RelativeState.AHEAD;
			}
			else
			{
				_pathPointsInfo[j].relativeState = PathPointInfo.RelativeState.UNDETERMINED;
			}
		}
	}

	protected override void OnDrawGizmos()
	{
		float unusedLocal1;
		if (!Enabled && Application.isPlaying)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawCube(transform.position + 0.5f * Vector3.up * 0.5f, Vector3.one * 0.5f);
			return;
		}
		if (IsUndeterminedRelativeState)
		{
			Gizmos.color = new Color(1f, 1f, 1f);
		}
		else if (IsAheadPlayerPath)
		{
			Gizmos.color = new Color(0.3f, 0.2f, 1f);
		}
		else
		{
			Gizmos.color = new Color(1f, 0.5f, 0.2f);
		}
		Gizmos.DrawCube(transform.position + 0.5f * Vector3.up * 0.5f, Vector3.one * 0.5f);
		if (Marked)
		{
			Gizmos.DrawLine(transform.position, transform.position + 5f * Vector3.up);
			Gizmos.DrawWireSphere(transform.position + 5f * Vector3.up, 1f);
		}
		if (PlayerCanSee)
		{
			Gizmos.color = Color.green;
		}
		else
		{
			Gizmos.color = Color.red;
		}
		Gizmos.DrawCube(transform.position + 1.5f * Vector3.up * 0.5f, Vector3.one * 0.5f);
	}
}
