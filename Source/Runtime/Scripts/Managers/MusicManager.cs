using System;
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	private enum Intensity
	{
		Silence,
		Calmness,
		Suspense,
		Impedence,
		Violence
	}

	public AudioClip AmbienceIndoor;

	public AudioClip AmbienceOutdoor;

	public AudioClip Breaker;

	public AudioClip[] Calmness;

	public AudioClip[] Suspense;

	public AudioClip[] Impedence;

	public AudioClip[] Violence;

	public float AmbienceVolume = 0.75f;

	public float MusicVolume = 0.25f;

	public float BreakerVolume = 0.25f;

	public int HordeTreshold = 4;

	private float _musicVolumeFactor = 1f;

	private AudioClip Ambience;

	private GameObject _audioListenerObject;

	private GameObject _ambienceTrackObject;

	private GameObject _audioTrackObject01;

	private GameObject _audioTrackObject02;

	private AudioSource _ambience;

	private AudioSource _audioSource01;

	private AudioSource _audioSource02;

	private int _syncValue;

	private Intensity _intensity;

	private CountdownTimer _cooldown = new CountdownTimer();

	private static MusicManager _instance;

	private void StopMusic()
	{
		if ((bool)_audioSource01)
		{
			_audioSource01.Stop();
		}
		if ((bool)_audioSource02)
		{
			_audioSource02.Stop();
		}
	}

	private float PlayMusic(AudioClip audioClip, bool loop)
	{
		if (!audioClip)
		{
			return 0f;
		}
		if (_audioSource02.clip != null)
		{
			_audioSource02.loop = false;
			AudioSource audioSource = _audioSource01;
			_audioSource01 = _audioSource02;
			_audioSource02 = audioSource;
		}
		_audioSource02.clip = audioClip;
		_audioSource02.loop = loop;
		_audioSource02.volume = (audioClip == Breaker ? BreakerVolume : MusicVolume) * _musicVolumeFactor;
		_audioSource02.Play();
		return audioClip.length;
	}

	private float PlayMusic(AudioClip[] audioClips, bool loop)
	{
		if (audioClips.Length > 1)
		{
			return PlayMusic(audioClips[UnityEngine.Random.Range(0, audioClips.Length)], loop);
		}
		if (audioClips.Length > 0)
		{
			return PlayMusic(audioClips[0], loop);
		}
		return 0f;
	}

	private float PlayMusic(Intensity intensity)
	{
		_intensity = intensity;
		switch (intensity)
		{
		case Intensity.Silence:
			StopMusic();
			return 10f;
		case Intensity.Calmness:
			StopMusic();
			return PlayMusic(Calmness, false);
		case Intensity.Suspense:
			StopMusic();
			return PlayMusic(Suspense, false);
		case Intensity.Impedence:
			PlayMusic(Breaker, false);
			return PlayMusic(Impedence, true);
		case Intensity.Violence:
			PlayMusic(Breaker, false);
			return PlayMusic(Violence, true);
		default:
			return 0f;
		}
	}

	private IEnumerator OnSpawnDelayed(int numZombies, int numSpecials, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (numSpecials > 0)
		{
			if (_intensity < Intensity.Violence)
			{
				PlayMusic(Intensity.Violence);
			}
		}
		else if (_intensity < Intensity.Impedence)
		{
			PlayMusic(Intensity.Impedence);
		}
		_cooldown.Run(5f);
		yield break;
	}

	private void OnSpawn(int numZombies, int numSpecials, bool flag)
	{
		StopCoroutine("OnSpawnDelayed");
		StartCoroutine(OnSpawnDelayed(numZombies, numSpecials, 2f));
	}

	private void OnPlayerDamage(bool flag)
	{
		if (_intensity == Intensity.Impedence)
		{
			PlayMusic(Intensity.Violence);
		}
	}

	private void UpdateVolume()
	{
		float musicVolume = Singleton.Settings.MusicVolume;
		_musicVolumeFactor = Mathf.Clamp01(musicVolume);
		if ((bool)_audioSource02 && (bool)_audioSource02.clip)
		{
			_audioSource02.volume = (_audioSource02.clip == Breaker ? BreakerVolume : MusicVolume) * _musicVolumeFactor;
		}
	}

	private void Awake()
	{
		_instance = this;
		UpdateVolume();
		Singleton.Settings.OnSettingsChanged += new Action(UpdateVolume);
		if (Singleton.PlayerData.CurrentMissionIndex >= 0)
		{
			GameData.MissionData missionData = Singleton.GameData.Missions[Singleton.PlayerData.CurrentMissionIndex];
			switch (missionData.Location)
			{
			case GameData.MissionLocation.YellowArea:
			case GameData.MissionLocation.OrangeArea:
			case GameData.MissionLocation.TealArea:
			case GameData.MissionLocation.AmberArea:
				Ambience = AmbienceOutdoor;
				break;
			case GameData.MissionLocation.RedLab:
			case GameData.MissionLocation.GreenLab:
			case GameData.MissionLocation.BlueLab:
				Ambience = AmbienceIndoor;
				break;
			default:
				Ambience = AmbienceIndoor;
				break;
			}
		}
		if ((bool)Ambience)
		{
			_ambienceTrackObject = new GameObject("Ambience");
			_ambienceTrackObject.transform.position = transform.position;
			_ambienceTrackObject.transform.parent = transform;
			_ambience = _ambienceTrackObject.AddComponent<AudioSource>();
		}
		_audioTrackObject01 = new GameObject("AudioTrack01");
		_audioTrackObject01.transform.position = transform.position;
		_audioTrackObject01.transform.parent = transform;
		_audioSource01 = _audioTrackObject01.AddComponent<AudioSource>();
		_audioTrackObject02 = new GameObject("AudioTrack02");
		_audioTrackObject02.transform.position = transform.position;
		_audioTrackObject02.transform.parent = transform;
		_audioSource02 = _audioTrackObject02.AddComponent<AudioSource>();
		_cooldown.Run(10f);
	}

	private void Start()
	{
		if ((bool)Ambience)
		{
			_ambience.clip = Ambience;
			_ambience.volume = AmbienceVolume * (!(!Singleton.Exists()) ? Singleton.Settings.MusicVolume : 1f);
			_ambience.loop = true;
			_ambience.Play();
		}
	}

	private void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.Settings.OnSettingsChanged -= new Action(UpdateVolume);
		}
		_instance = null;
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			if ((bool)_ambience)
			{
				_ambience.Stop();
			}
			if ((bool)_audioSource01)
			{
				_audioSource01.Stop();
			}
			if ((bool)_audioSource02)
			{
				_audioSource02.Stop();
			}
		}
		else
		{
			if ((bool)_ambience && _ambience.volume > 0f)
			{
				_ambience.Play();
			}
			if ((bool)_audioSource02)
			{
				_audioSource02.Play();
			}
		}
	}

	private void Update()
	{
		float unusedLocal1;
		if (Util.DoRarely(this, "MM_SC", 0.5f) && _intensity < Intensity.Violence && Scene.Specials.Count > 0)
		{
			PlayMusic(Breaker, false);
			PlayMusic(Intensity.Violence);
			_cooldown.Run(3f);
		}
		if ((bool)_ambience)
		{
			if (_intensity == Intensity.Silence || _intensity == Intensity.Calmness)
			{
				if (!_ambience.isPlaying)
				{
					_ambience.Play();
				}
				_ambience.volume = Mathf.Clamp(_ambience.volume + 0.5f * Time.deltaTime, 0f, AmbienceVolume) * (Singleton.Exists() ? Singleton.Settings.MusicVolume : 1f);
			}
			else if (_ambience.isPlaying)
			{
				_ambience.volume = Mathf.Clamp(_ambience.volume - 0.5f * Time.deltaTime, 0f, AmbienceVolume) * (Singleton.Exists() ? Singleton.Settings.MusicVolume : 1f);
				if (_ambience.volume == 0f)
				{
					_ambience.Stop();
				}
			}
		}
		if (_cooldown.IsRunning())
		{
			return;
		}
		if (_intensity > Intensity.Suspense)
		{
			if (Util.DoRarely(this, "MM_AEC", 0.5f) && Scene.AttackingEnemies.Count == 0)
			{
				if (_syncValue != 0)
				{
					_syncValue = 0;
					_cooldown.Run(3f);
				}
				else
				{
					PlayMusic(Intensity.Silence);
					PlayMusic(Breaker, false);
					_cooldown.Run(10f);
				}
			}
			else
			{
				_syncValue = Scene.AttackingEnemies.Count;
			}
		}
		else
		{
			switch (_intensity)
			{
			case Intensity.Silence:
				_cooldown.Run(PlayMusic(Intensity.Calmness));
				break;
			case Intensity.Calmness:
				_cooldown.Run(PlayMusic(Intensity.Suspense));
				break;
			case Intensity.Suspense:
				_cooldown.Run(PlayMusic(Intensity.Silence));
				break;
			}
		}
	}

	public static void OnSpawn(int numZombies, int numSpecials)
	{
		if ((bool)_instance)
		{
			_instance.OnSpawn(numZombies, numSpecials, false);
		}
	}

	public static void OnPlayerDamage()
	{
		if ((bool)_instance)
		{
			_instance.OnPlayerDamage(false);
		}
	}
}
