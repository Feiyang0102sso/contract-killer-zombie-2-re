using Glu;
using UnityEngine;

public class MainTheme : MonoBehaviour
{
	private class PlaybackController
	{
		private GameObject _owner;

		private AudioSource _audioSource;

		private bool _play;

		private RTCountdownTimer _timer;

		private RTCountdownTimer _stopTimer;

		private float _fadeInTime;

		private float _fadeOutTime;

		private bool _isMusic;

		private float _volumeFactor = 1f;

		private float _appPauseTime;

		public void GameObjectDestructor()
		{
			Object.Destroy(_audioSource.gameObject);
		}

		public float VolumeFactor
		{
			get
			{
				return _volumeFactor;
			}
			set
			{
				_volumeFactor = value;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return _play;
			}
		}

		public PlaybackController(GameObject owner, string soundName, bool loop, bool isMusic, float fadeInTime, float fadeOutTime)
		{
			_owner = owner;
			_fadeInTime = fadeInTime;
			_fadeOutTime = fadeOutTime;
			_isMusic = isMusic;
			GameObject gameObject = new GameObject();
			gameObject.name = "PlaybackObject_" + soundName;
			gameObject.transform.parent = owner.transform;
			_audioSource = gameObject.AddComponent<AudioSource>();
			AudioClip audioClip = Resources.Load(soundName, typeof(AudioClip)) as AudioClip;
			audioClip.name = soundName;
			_audioSource.clip = audioClip;
			_audioSource.loop = loop;
			_audioSource.volume = 0f;
		}

		public float Volume
		{
			get
			{
				return _audioSource.volume;
			}
		}

		public void Play()
		{
			_play = true;
			_timer = new RTCountdownTimer(0.1f);
			_audioSource.Play();
			if (!_audioSource.loop)
			{
				_stopTimer = new RTCountdownTimer(_audioSource.clip.length - _fadeOutTime);
			}
		}

		public void Stop()
		{
			_play = false;
			_timer = new RTCountdownTimer(0.1f);
			_stopTimer = null;
		}

		public void Update()
		{
			float num = _volumeFactor * (_isMusic ? (Singleton.inst ? Singleton.Settings.MusicVolume : 1f) : (Singleton.inst ? Singleton.Settings.SoundVolume : 1f));
			if (Tapjoy.isOfferwallOpen)
			{
				num = 0f;
			}
			if (_timer != null && !_timer.IsRunning())
			{
				if (_play)
				{
					float num2 = 1f / _fadeInTime * 0.1f;
					num2 = num2 + _audioSource.volume > num ? num - _audioSource.volume : num2;
					_audioSource.volume = _audioSource.volume + num2;
					if (_stopTimer != null && !_stopTimer.IsRunning())
					{
						_play = false;
						_stopTimer = null;
					}
				}
				else
				{
					float num3 = 1f / _fadeOutTime * 0.1f;
					num3 = _audioSource.volume - num3 < 0f ? _audioSource.volume : num3;
					_audioSource.volume = _audioSource.volume - num3;
					if (_audioSource.volume == 0f)
					{
						_audioSource.Stop();
						_audioSource.time = 0f;
					}
				}
				_timer.Run(0.1f);
			}
		}

		public void OnApplicationPause(bool pause)
		{
			if (pause)
			{
				_appPauseTime = _audioSource.time;
				_audioSource.Stop();
			}
			else if (_play)
			{
				_audioSource.time = _appPauseTime;
				_audioSource.Play();
			}
		}
	}

	private const float MusicFadeInTime = 1.5f;

	private const float MusicFadeOutTime = 2.5f;

	private const float VoiceFadeInTime = 0f;

	private const float VoiceFadeOutTime = 0f;

	private const float MutedMusicVolume = 0.25f;

	private const float FadeStepTime = 0.1f;

	private PlaybackController _mainTheme;

	private PlaybackController _voiceOver;

	private void Start()
	{
	}

	private void Update()
	{
		if (_mainTheme != null)
		{
			_mainTheme.Update();
			if (!_mainTheme.IsPlaying && _mainTheme.Volume <= 1.1754944E-38f)
			{
				Debug.Log("Main theme music is stopped");
				_mainTheme.GameObjectDestructor();
				_mainTheme = null;
			}
		}
		if (_voiceOver != null)
		{
			_voiceOver.Update();
			if (_mainTheme != null && _mainTheme.IsPlaying)
			{
				_mainTheme.VolumeFactor = 0.25f * _voiceOver.Volume + 1f * (1f - _voiceOver.Volume);
			}
			if (!_voiceOver.IsPlaying && _voiceOver.Volume <= 1.1754944E-38f)
			{
				Debug.Log("Voiceover is stopped");
				_voiceOver.GameObjectDestructor();
				_voiceOver = null;
			}
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (_mainTheme != null)
		{
			_mainTheme.OnApplicationPause(pause);
		}
		if (_voiceOver != null)
		{
			_voiceOver.OnApplicationPause(pause);
		}
	}

	public bool MainThemeIsPlaying
	{
		get
		{
			return _mainTheme != null && _mainTheme.IsPlaying;
		}
	}

	public void PlayMainTheme()
	{
		bool unusedLocal1;
		bool unusedLocal2;
		if (_mainTheme == null)
		{
			_mainTheme = new PlaybackController(gameObject, "22Khz/main_theme", true, true, 1.5f, 2.5f);
			_mainTheme.Play();
		}
	}

	public void StopMainTheme()
	{
		if (_mainTheme != null && _mainTheme.IsPlaying)
		{
			_mainTheme.Stop();
		}
	}

	public bool VoiceOverIsPlaying
	{
		get
		{
			return _voiceOver != null && _voiceOver.IsPlaying;
		}
	}

	public void PlayVoiceOver(string resourceName)
	{
		bool unusedLocal1;
		bool unusedLocal2;
		_voiceOver = new PlaybackController(gameObject, "22Khz/Voiceover/" + resourceName, false, false, 0f, 0f);
		_voiceOver.Play();
	}

	public void StopVoiceOver()
	{
		if (_voiceOver != null && _voiceOver.IsPlaying)
		{
			_voiceOver.Stop();
		}
	}
}
