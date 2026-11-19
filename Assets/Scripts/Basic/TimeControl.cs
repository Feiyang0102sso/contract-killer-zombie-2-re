using UnityEngine;

public class TimeControl : MonoBehaviour
{
	public enum PauseTypes
	{
		Dialog = 1,
		Loading,
		CutScene,
		Misc
	}

	private const float bulletTimeDelay = 20f;

	private const float minRangeForIdleEnemyForSlowMotion = 10f;

	private const float EffectTime = 1.25f;

	private const float EffectFadeOutTime = 0.5f;

	private const float EffectFadeInTime = 0.25f;

	private const float BulletTimeDefaultTimeScale = 0.1f;

	private const float DefaultTimeScale = 1f;

	private static bool _updateScale;

	private static float _timeScale = 1f;

	private static int _pauseMask;

	private static float _physicsFixedTimestep = -1f;

	private static float _realTime;

	private static float _gameTime;

	private static float _bulletTimeLeft;

	private float _bulletTimeTimeScale = 0.1f;

	public static void Pause(bool pause, PauseTypes type)
	{
		if (pause)
		{
			_pauseMask |= 1 << (int)type;
		}
		else
		{
			_pauseMask &= ~(1 << (int)type);
		}
		Pause(_pauseMask > 0);
	}

	private static void Pause(bool pauseOrNot)
	{
		_timeScale = (pauseOrNot ? 0.001f : 1f);
		_updateScale = true;
	}

	public static bool IsPaused()
	{
		return _timeScale < 0.01f;
	}

	public static void TimeScale(float scale)
	{
		_timeScale = scale;
		_updateScale = true;
	}

	public static float Scale
	{
		get
		{
			return _timeScale;
		}
	}

	public static float PhysicsFixedTimestep
	{
		get
		{
			return (_physicsFixedTimestep >= 0f) ? _physicsFixedTimestep : (_physicsFixedTimestep = Time.fixedDeltaTime);
		}
		set
		{
			_physicsFixedTimestep = value;
			_updateScale = true;
		}
	}

	public static float RealTime
	{
		get
		{
			return _realTime;
		}
	}

	public static float GameTime
	{
		get
		{
			return _gameTime;
		}
	}

	public static float UnityTime
	{
		get
		{
			return Time.time;
		}
	}

	private void LateUpdate()
	{
		if (_updateScale)
		{
			Time.timeScale = _timeScale;
			Time.fixedDeltaTime = PhysicsFixedTimestep * _timeScale;
			_updateScale = false;
		}
	}

	public bool IsBulletTime()
	{
		return _bulletTimeLeft > 0f;
	}

	private void Update()
	{
		float num = Time.deltaTime / Time.timeScale;
		if (_bulletTimeLeft > 0f && !IsPaused())
		{
			_bulletTimeLeft -= num;
			if (_bulletTimeLeft < 0f)
			{
				_bulletTimeLeft = 0f;
			}
			if (_bulletTimeLeft > 1.75f)
			{
				float num2 = (_bulletTimeLeft - 1.25f - 0.5f) / 0.25f;
				TimeScale(_bulletTimeTimeScale * (1f - num2) + 1f * num2);
				SoundManager.DynamicVolume = 0f;
			}
			else if (_bulletTimeLeft < 0.5f)
			{
				float num3 = _bulletTimeLeft / 0.5f;
				TimeScale(_bulletTimeTimeScale * num3 + 1f * (1f - num3));
				SoundManager.DynamicVolume = 1f;
			}
			else
			{
				TimeScale(_bulletTimeTimeScale);
				SoundManager.DynamicVolume = 0f;
			}
		}
		_realTime += num;
		if (!IsPaused())
		{
			_gameTime += num;
		}
	}

	public void RunBulletTime()
	{
		RunBulletTime(1.25f, 0.1f);
	}

	public void RunBulletTime(float duration, float timeScale)
	{
		SoundManager.PlayExclusive("PowerUps/slow_mo", false);
		_bulletTimeLeft = duration + 0.5f + 0.25f;
		_bulletTimeTimeScale = timeScale;
	}

	public void StopBulletTime()
	{
		if (IsBulletTime())
		{
			_bulletTimeLeft = 0.5f;
		}
	}

	public void ThisIsMassEffect()
	{
		if (!IsBulletTime() && (Scene.AttackingEnemies.Count > 0 || (bool)Scene.NearbyAny<Unit>(Scene.Player.transform.position, 10f)))
		{
			RunBulletTime();
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		Util.DoRarely(this, "bullet_time_delay", 20f);
	}

	public void ThisIsSparta(float distanceToEvent)
	{
		float unusedLocal1;
		float unusedLocal2;
		float unusedLocal3;
		float unusedLocal4;
		float unusedLocal5;
		float num = Mathf.Pow(Mathf.Clamp01((distanceToEvent - 5f) / 20f), 0.75f);
		float percent = 10f * (1f - num) + 5f * num;
		if (!IsBulletTime() && !IsPaused() && Singleton.Exists() && Singleton.PlayerData.TutorialState > 0 && (Util.Chance(percent) || (Scene.AttackingEnemies.Count == 0 && !Scene.NearbyAny<Unit>(Scene.Player.transform.position, 10f))) && Util.DoRarely(this, "bullet_time_delay", 20f))
		{
			RunBulletTime();
		}
	}

	public static void Restart()
	{
		_updateScale = false;
		_timeScale = 1f;
		_pauseMask = 0;
		_physicsFixedTimestep = -1f;
		_realTime = 0f;
		_gameTime = 0f;
		_bulletTimeLeft = 0f;
	}
}
