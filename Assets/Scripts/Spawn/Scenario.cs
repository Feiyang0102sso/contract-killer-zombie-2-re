using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class Scenario : MonoBehaviourAndLog
{
	protected bool completed = true;

	public bool Looped;

	public bool Completed
	{
		get
		{
			return completed;
		}
	}

	protected virtual void Awake()
	{
		LogTo("Spawn.Scenario");
		enabled = false;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected abstract void Generate();

	protected virtual void Complete()
	{
		if (Looped)
		{
			Generate();
			return;
		}
		if (completed)
		{
			return;
		}
		log.Info(gameObject, "scenario completed");
		completed = true;
		enabled = false;
		SendMessageUpwards("OnScenarioCompleted", gameObject, SendMessageOptions.DontRequireReceiver);
	}
}

public abstract class SpawnScenario : Scenario
{
	private class SpawnSequence
	{
		public int count;

		public Func<Spawner.SpawnInfo> whatToSpawn;

		public Action preSpawn;

		public Action<GameObject> postSpawn;

		public Spawner.PointDesc pointDescription;

		public float initialDelay;

		public float duration;

		public EZAnimation.Interpolator timeInterpolator;

		private float _lastSpawnTime;

		private float _timeBetweenSpawns;

		public SpawnPoint point;

		public void SetupTime(int count_, float duration_, CustomInterpolators.EASING_TYPE timeInterpolatorType)
		{
			count = count_;
			duration = duration_;
			timeInterpolator = CustomInterpolators.GetInterpolator(timeInterpolatorType);
			_lastSpawnTime = 0f;
			_timeBetweenSpawns = duration / (float)count;
		}

		public bool CheckTime(float spawnTime)
		{
			float num = timeInterpolator(spawnTime, 0f, duration, duration);
			return num > _lastSpawnTime + _timeBetweenSpawns;
		}

		public void OnSpawned()
		{
			count--;
			_lastSpawnTime += _timeBetweenSpawns;
		}
	}

	private List<SpawnSequence> _seqs = new List<SpawnSequence>();

	private float _timeElapsed;

	protected void AddSpawnSequence(int count, Func<Spawner.SpawnInfo> whatToSpawn, Action preSpawn, Action<GameObject> postSpawn, Spawner.PointDesc pointDescription, float initialDelay_, float tempo, CustomInterpolators.EASING_TYPE timeInterpolatorType)
	{
		SpawnSequence spawnSequence = new SpawnSequence();
		spawnSequence.whatToSpawn = whatToSpawn;
		spawnSequence.preSpawn = preSpawn;
		spawnSequence.postSpawn = postSpawn;
		spawnSequence.pointDescription = pointDescription;
		spawnSequence.initialDelay = initialDelay_;
		float duration_ = (float)count / tempo;
		spawnSequence.SetupTime(count, duration_, timeInterpolatorType);
		_seqs.Add(spawnSequence);
	}

	protected override void Generate()
	{
		completed = false;
		enabled = true;
	}

	protected override void Update()
	{
		if (Completed)
		{
			return;
		}
		_timeElapsed += Time.deltaTime;
		if (Scene.Spawner.IsOverpopulated())
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < _seqs.Count; i++)
		{
			SpawnSequence spawnSequence = _seqs[i];
			if (spawnSequence == null)
			{
				continue;
			}
			flag = false;
			if (_timeElapsed < spawnSequence.initialDelay)
			{
				continue;
			}
			if (spawnSequence.count <= 0)
			{
				_seqs[i] = null;
			}
			else
			{
				if (!spawnSequence.CheckTime(_timeElapsed - spawnSequence.initialDelay))
				{
					continue;
				}
				if (!Util.DoRarely(Scene.Spawner, "spawn_throttle", 0.2f))
				{
					break;
				}
				if (!spawnSequence.point && spawnSequence.pointDescription.GetSpecificPoints().Length > 0)
				{
					spawnSequence.point = Util.RandomFrom(spawnSequence.pointDescription.GetSpecificPoints());
				}
				if (!spawnSequence.point)
				{
					spawnSequence.point = Scene.Spawner.FindSpawnPoint(spawnSequence.pointDescription);
				}
				if (!spawnSequence.point)
				{
					if (_timeElapsed - spawnSequence.initialDelay > spawnSequence.duration + 2f)
					{
						SendMessageUpwards("OnCanceledSpawn", spawnSequence.count, SendMessageOptions.DontRequireReceiver);
						_seqs[i] = null;
						continue;
					}
					else
					{
						spawnSequence.pointDescription.ExpandConditions();
						continue;
					}
				}
				float unusedSpawnDelay;
				bool spawned = Scene.Spawner.Spawn(this, spawnSequence.point, spawnSequence.whatToSpawn(), spawnSequence.preSpawn, spawnSequence.postSpawn, true, GetComponent<Piper>() != null);
				if (spawned)
				{
					_seqs[i].OnSpawned();
					log.Info(spawnSequence.point.gameObject, "Spawning in '{0}', left {1}, delay {2}, time {3}, duration {4} ", spawnSequence.point.name, spawnSequence.count, spawnSequence.initialDelay, _timeElapsed, spawnSequence.duration);
				}
				spawnSequence.point = null;
				break;
			}
		}
		if (flag)
		{
			Complete();
			_timeElapsed = 0f;
		}
		base.Update();
	}
}
