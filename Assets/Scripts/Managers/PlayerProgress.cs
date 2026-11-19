using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviourAndLog
{
	public const float MAX_PATH_SEGMENT_SIZE = 8f;

	private List<GameObject> _points = new List<GameObject>(16);

	private NavMeshPath _tempPath = new NavMeshPath();

	private List<Vector3> _path = new List<Vector3>();

	private int _currentPathIndex;

	public Vector3 CurrentDirection = Vector3.forward;

	public float DistToCurrentPathPoint;

	public void AddPoint(GameObject point)
	{
		_points.Add(point);
	}

	public List<Vector3> GetPath()
	{
		return _path;
	}

	public int CurrentPathIndex
	{
		get
		{
			return _currentPathIndex;
		}
		private set
		{
			_currentPathIndex = value;
			CurrentDirection = (_path[_currentPathIndex + 1] - _path[_currentPathIndex]).normalized;
		}
	}

	private void Awake()
	{
		LogTo("Player.PlayerProgress");
	}

	public void Init()
	{
		BuildPath();
	}

	private void BuildPath()
	{
		foreach (GameObject point in _points)
		{
			NavMeshHit hit;
			NavMesh.SamplePosition(point.transform.position, out hit, 10f, -1);
			if (hit.hit)
			{
				point.transform.position = hit.position;
			}
			else
			{
				log.Error(point, "Path point is too far from nav mesh");
			}
		}
		_points.Sort((GameObject a, GameObject b) => Util.CompareNatural(a.name, b.name));
		for (int num = 0; num < _points.Count - 1; num++)
		{
			bool flag = NavMesh.CalculatePath(_points[num].transform.position, _points[num + 1].transform.position, -1, _tempPath);
			for (int num2 = 0; num2 < _tempPath.corners.Length; num2++)
			{
				if (_path.Count > 0)
				{
					Vector3 vector = _path[_path.Count - 1];
					float magnitude = (_tempPath.corners[num2] - vector).magnitude;
					int num3 = (int)(magnitude / 8f);
					Vector3 normalized = (_tempPath.corners[num2] - vector).normalized;
					for (int num4 = 0; num4 < num3; num4++)
					{
						float num5 = magnitude / (float)num3;
						Vector3 vector2 = vector + normalized * num5;
						_path.Add(vector2);
						vector = vector2;
					}
				}
				if (_path.Count > 0)
				{
					Vector3 vector3 = _path[_path.Count - 1];
					if ((_tempPath.corners[num2] - vector3).sqrMagnitude < 0.1f)
					{
						continue;
					}
				}
				_path.Add(_tempPath.corners[num2]);
			}
		}
		CurrentPathIndex = 0;
	}

	private float GetPathPartLength(int startIndex)
	{
		float num = 0f;
		for (int i = startIndex; i < _path.Count - 1; i++)
		{
			num += (_path[i + 1] - _path[i]).magnitude;
		}
		return num;
	}

	private void UpdatePlayerProgress()
	{
		Player player = Scene.Player;
		if (!player)
		{
			return;
		}
		if (_currentPathIndex >= _path.Count - 1)
		{
			return;
		}
		_currentPathIndex = DetermineCurrentPointIndexOnPath();
	}

	private int DetermineCurrentPointIndexOnPath()
	{
		Vector3 position = Scene.Player.transform.position;
		int currentPathIndex = _currentPathIndex;
		float num = Util.CalcNavDistance(position, _path[currentPathIndex]);
		int num2 = -1;
		float num3 = float.MaxValue;
		for (int i = 0; i < _path.Count - 1; i++)
		{
			float num4 = Vector3.Distance(position, _path[i]);
			if (num3 > num4)
			{
				num2 = i;
				num3 = num4;
			}
		}
		if (Util.CalcNavDistance(position, _path[num2]) < num)
		{
			return num2;
		}
		return currentPathIndex;
	}

	private void Update()
	{
		if (Util.DoRarelyReal(this, "player_progress", 1f))
		{
			UpdatePlayerProgress();
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (_path.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _path.Count - 1; i++)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(_path[i], _path[i + 1]);
			if (i == _currentPathIndex)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(_path[i], _path[i] + Vector3.up * 15f);
			}
			else
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawSphere(_path[i], 0.2f);
			}
			Util.LabelInEditor(_path[i] + Vector3.up * 0.1f * (1f + (float)i / 3f), i.ToString());
		}
		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(Scene.Player.transform.position, (_path[_currentPathIndex + 1] - _path[_currentPathIndex]).normalized);
	}
}
