using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviourAndLog
{
	private struct ManagedAudioSource
	{
		public AudioSource audioSource;

		public Transform transform;

		public float startTime;

		public int priority;

		public float volume;

		public float fadeIn;

		public float fadeOut;
	}

	public const int PriorityAmbient = 0;

	public const int PriorityMusic = 1000;

	public const int PriorityPlayer = 2000;

	public const int PriorityScene = 3000;

	public const int PriorityDefault = 2000;

	public int MaxAudioSources = 8;

	private ManagedAudioSource[] _managed;

	private Dictionary<string, int> _preloadingQueue = new Dictionary<string, int>();

	private Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();

	private Dictionary<string, int> _cacheHits = new Dictionary<string, int>();

	private float _dynamicVolume = 1f;

	private GameData.AudioSourcePreset _defaultPreset;

	private bool _preloadingLock;

	private static SoundManager _instance = null;

	private void OnMissionCompleted(string param)
	{
		_CleanupCache();
	}

	private void OnMissionFailed(string param1, GameEvents.MissionFailure param2, Mission.Type param3)
	{
		_CleanupCache();
	}

	private void Awake()
	{
		LogTo("Other.SoundManager");
		_managed = new ManagedAudioSource[MaxAudioSources];
		for (int i = 0; i < MaxAudioSources; i++)
		{
			GameObject gameObject = new GameObject(string.Format("AudioSource_{0}", i + 1));
			gameObject.transform.position = transform.position;
			gameObject.transform.parent = transform;
			_managed[i].audioSource = gameObject.AddComponent<AudioSource>();
			_managed[i].transform = null;
			_managed[i].startTime = 0f;
			_managed[i].volume = 0f;
			_managed[i].priority = 0;
			_managed[i].fadeIn = 0f;
			_managed[i].fadeOut = 0f;
		}
		_defaultPreset = new GameData.AudioSourcePreset();
		_defaultPreset.Capture(_managed[0].audioSource);
		Singleton.GameEvents.OnMissionCompleted += new Action<string>(OnMissionCompleted);
		Singleton.GameEvents.OnMissionFailed += new Action<string, GameEvents.MissionFailure, Mission.Type>(OnMissionFailed);
		_instance = this;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnMissionCompleted -= new Action<string>(OnMissionCompleted);
		}
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnMissionFailed -= new Action<string, GameEvents.MissionFailure, Mission.Type>(OnMissionFailed);
		}
		_instance = null;
	}

	private void Update()
	{
		float unusedLocal1;
		float dynamicVolume = DynamicVolume;
		if (dynamicVolume < 1f)
		{
			for (int i = 0; i < MaxAudioSources; i++)
			{
				if (_managed[i].audioSource.clip && _managed[i].audioSource.clip.name.Contains("slow_mo"))
				{
					continue;
				}
				float num = dynamicVolume - _managed[i].audioSource.volume;
				float num2 = Mathf.Sign(num) * 2f * Time.deltaTime / Time.timeScale;
				if (Mathf.Abs(num2) > Mathf.Abs(num))
				{
					num2 = num;
				}
				if (num2 < 0f)
				{
					num2 *= Mathf.Abs(Mathf.Sign(num2));
				}
				_managed[i].audioSource.volume += num2;
			}
		}
		else
		{
			for (int j = 0; j < MaxAudioSources; j++)
			{
				if ((bool)_managed[j].audioSource.clip)
				{
					float num3 = Time.time - _managed[j].startTime;
					if (_managed[j].fadeIn > 0f && num3 <= _managed[j].fadeIn)
					{
						float num4 = Mathf.Clamp01(num3 / _managed[j].fadeIn);
						_managed[j].audioSource.volume = _managed[j].volume * num4;
					}
					else if (_managed[j].fadeOut > 0f)
					{
						float num5 = Mathf.Clamp01((_managed[j].audioSource.clip.length - num3) / _managed[j].fadeOut);
						_managed[j].audioSource.volume = _managed[j].volume * num5;
					}
					else if (_managed[j].audioSource.volume != _managed[j].volume)
					{
						_managed[j].audioSource.volume = _managed[j].volume;
					}
				}
			}
		}
		for (int k = 0; k < MaxAudioSources; k++)
		{
			if ((bool)_managed[k].audioSource.clip && (bool)_managed[k].transform)
			{
				_managed[k].audioSource.transform.position = _managed[k].transform.position;
				if (!_managed[k].audioSource.isPlaying)
				{
					_managed[k].transform = null;
				}
			}
		}
	}

	private string GetFullSoundName(string soundName)
	{
		return "22Khz/" + soundName;
	}

	private AudioClip LoadAudioClip(string soundName)
	{
		if (soundName == null)
		{
			return null;
		}
		if (soundName.Length == 0)
		{
			return null;
		}
		soundName = GetFullSoundName(soundName);
		if (_cache.ContainsKey(soundName))
		{
			int num = _cacheHits[soundName];
			_cacheHits[soundName] = _cacheHits[soundName] + 1;
			return _cache[soundName];
		}
		AudioClip audioClip = Resources.Load(soundName, typeof(AudioClip)) as AudioClip;
		if (!audioClip)
		{
			return null;
		}
		audioClip.name = soundName;
		_cache.Add(soundName, audioClip);
		_cacheHits.Add(soundName, 1);
		return audioClip;
	}

	private int GetManagedIndex(int priority, Vector3 position, AudioClip audioClip)
	{
		int num = -1;
		float num2 = 0f;
		Vector3 vector = Scene.Exists() ? (Scene.Camera ? Scene.Camera.transform.position : Vector3.zero) : Vector3.zero;
		for (int i = 0; i < MaxAudioSources; i++)
		{
			float num3 = 0f;
			if ((bool)_managed[i].audioSource.clip)
			{
				if (_managed[i].audioSource.clip == audioClip)
				{
					num3 = float.PositiveInfinity;
				}
				else if (_managed[i].priority >= priority)
				{
					num3 = Time.time - _managed[i].startTime;
					if (priority >= 3000 && _managed[i].audioSource.isPlaying)
					{
						num3 = (_managed[i].audioSource.transform.position - vector).magnitude / (position - vector).magnitude;
					}
				}
				else if (_managed[i].priority < priority && !_managed[i].audioSource.isPlaying)
				{
					num3 = Time.time - _managed[i].startTime;
				}
			}
			else
			{
				num3 = float.PositiveInfinity;
			}
			if (num < 0 || num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		return num;
	}

	private int GetCacheMemorySize()
	{
		int num = 0;
		foreach (KeyValuePair<string, AudioClip> item in _cache)
		{
			num += item.Value.samples * item.Value.channels * 2;
		}
		return num;
	}

	private void _CleanupCache()
	{
		int cacheMemorySize = GetCacheMemorySize();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AudioClip> item in _cache)
		{
			if (!_IsPlaying(item.Key))
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			_cache.Remove(item2);
			_cacheHits.Remove(item2);
		}
		int cacheMemorySize2 = GetCacheMemorySize();
	}

	private AudioClip _Preload(string soundName)
	{
		return LoadAudioClip(soundName);
	}

	private void _PreloadInBackground(string soundName)
	{
		int value = 0;
		if (_preloadingQueue.TryGetValue(soundName, out value))
		{
			_preloadingQueue.Remove(soundName);
		}
		_preloadingQueue.Add(soundName, value + 1);
		if (!_preloadingLock)
		{
			StartCoroutine("Preloading");
		}
	}

	private IEnumerator Preloading()
	{
		_preloadingLock = true;
		while (_preloadingQueue.Count > 0)
		{
			string mostUrgentSoundName = string.Empty;
			int maxNumberOfRequests = 0;
			foreach (KeyValuePair<string, int> request in _preloadingQueue)
			{
				if (request.Value > maxNumberOfRequests)
				{
					maxNumberOfRequests = request.Value;
					mostUrgentSoundName = request.Key;
				}
			}
			_preloadingQueue.Remove(mostUrgentSoundName);
			if (_cache.ContainsKey(GetFullSoundName(mostUrgentSoundName)))
			{
				yield return new WaitForEndOfFrame();
				continue;
			}
			AudioClip audioClip = LoadAudioClip(mostUrgentSoundName);
			yield return new WaitForSeconds(audioClip.length);
		}
		_preloadingLock = false;
		yield break;
	}

	private bool _IsPlaying(string soundName)
	{
		soundName = GetFullSoundName(soundName);
		for (int i = 0; i < MaxAudioSources; i++)
		{
			if ((bool)_managed[i].audioSource.clip && _managed[i].audioSource.clip.name == soundName)
			{
				return _managed[i].audioSource.isPlaying == true;
			}
		}
		return false;
	}

	private void _Stop(string soundName)
	{
		soundName = GetFullSoundName(soundName);
		for (int i = 0; i < MaxAudioSources; i++)
		{
			if ((bool)_managed[i].audioSource.clip && _managed[i].audioSource.clip.name == soundName)
			{
				_managed[i].audioSource.Stop();
				break;
			}
		}
	}

	private AudioClip _Play(string soundName, int priority, float volume, float pan, bool loop, float fadeIn, float fadeOut)
	{
		AudioClip audioClip = LoadAudioClip(soundName);
		if ((bool)audioClip)
		{
			int managedIndex = GetManagedIndex(priority, Vector3.zero, audioClip);
			if (managedIndex >= 0)
			{
				if (_managed[managedIndex].audioSource.isPlaying)
				{
					_managed[managedIndex].audioSource.Stop();
				}
				_managed[managedIndex].audioSource.clip = audioClip;
				_managed[managedIndex].transform = null;
				_managed[managedIndex].startTime = Time.time;
				_managed[managedIndex].priority = priority;
				_managed[managedIndex].volume = volume;
				_managed[managedIndex].fadeIn = fadeIn;
				_managed[managedIndex].fadeOut = fadeOut;
				_managed[managedIndex].audioSource.volume = Mathf.Min(volume, DynamicVolume);
				_managed[managedIndex].audioSource.loop = loop;
				_managed[managedIndex].audioSource.panStereo = pan;
				GameData.AudioSourcePreset audioSourcePreset = Singleton.GameData.GetAudioSourcePreset(soundName);
				audioSourcePreset = (!(audioSourcePreset != null) ? _defaultPreset : audioSourcePreset);
				audioSourcePreset.Apply(_managed[managedIndex].audioSource);
				_managed[managedIndex].priority += audioSourcePreset.Priority;
				_managed[managedIndex].audioSource.Play();
				return audioClip;
			}
		}
		return null;
	}

	private AudioClip _PlayExclusive(string soundName, int priority, float volume, float pan, bool loop, float fadeIn, float fadeOut)
	{
		if (!IsPlaying(soundName))
		{
			return Play(soundName, priority, volume, pan, loop, fadeIn, fadeOut);
		}
		return LoadAudioClip(soundName);
	}

	private AudioClip _Play(string soundName, Vector3 position, Transform t, float minDistance, float maxDistance, bool loop)
	{
		AudioClip audioClip = LoadAudioClip(soundName);
		if ((bool)audioClip)
		{
			int managedIndex = GetManagedIndex(3000, position, audioClip);
			if (managedIndex >= 0)
			{
				if (_managed[managedIndex].audioSource.isPlaying)
				{
					_managed[managedIndex].audioSource.Stop();
				}
				_managed[managedIndex].audioSource.gameObject.transform.position = (t ? t.position : position);
				_managed[managedIndex].audioSource.clip = audioClip;
				_managed[managedIndex].transform = t;
				_managed[managedIndex].startTime = Time.time;
				_managed[managedIndex].priority = 3000;
				_managed[managedIndex].volume = 1f;
				_managed[managedIndex].fadeIn = 0f;
				_managed[managedIndex].fadeOut = 0f;
				_managed[managedIndex].audioSource.volume = Mathf.Min(1f, DynamicVolume);
				_managed[managedIndex].audioSource.panStereo = 0f;
				_managed[managedIndex].audioSource.loop = loop;
				_managed[managedIndex].audioSource.minDistance = minDistance;
				_managed[managedIndex].audioSource.maxDistance = maxDistance;
				GameData.AudioSourcePreset audioSourcePreset = Singleton.GameData.GetAudioSourcePreset(soundName);
				audioSourcePreset = (!(audioSourcePreset != null) ? _defaultPreset : audioSourcePreset);
				audioSourcePreset.Apply(_managed[managedIndex].audioSource);
				_managed[managedIndex].audioSource.Play();
				return audioClip;
			}
		}
		return null;
	}

	private AudioClip _PlayExclusive(string soundName, Vector3 position, Transform t, float minDistance, float maxDistance, bool loop)
	{
		if (!IsPlaying(soundName))
		{
			return _Play(soundName, position, t, minDistance, maxDistance, loop);
		}
		return LoadAudioClip(soundName);
	}

	public static float DynamicVolume
	{
		get
		{
			return (_instance ? _instance._dynamicVolume : 1f) * (Singleton.Exists() ? Singleton.Settings.SoundVolume : 1f);
		}
		set
		{
			if ((bool)_instance)
			{
				_instance._dynamicVolume = value;
			}
		}
	}

	public static AudioClip Preload(string soundName)
	{
		return _instance ? _instance._Preload(soundName) : null;
	}

	public static void PreloadInBackground(string soundName)
	{
		if ((bool)_instance)
		{
			_instance._PreloadInBackground(soundName);
		}
	}

	public static bool IsPlaying(string soundName)
	{
		return (bool)_instance ? _instance._IsPlaying(soundName) : false;
	}

	public static void Stop(string soundName)
	{
		if ((bool)_instance)
		{
			_instance._Stop(soundName);
		}
	}

	public static AudioClip Play(string soundName, int priority, float volume, float pan, bool loop, float fadeIn, float fadeOut)
	{
		return _instance ? _instance._Play(soundName, priority, volume, pan, loop, fadeIn, fadeOut) : null;
	}

	public static AudioClip Play(string soundName, int priority, float volume, float pan, bool loop)
	{
		return _instance ? _instance._Play(soundName, priority, volume, pan, loop, 0f, 0f) : null;
	}

	public static AudioClip Play(string soundName, int priority, bool loop)
	{
		return _instance ? _instance._Play(soundName, priority, 1f, 0f, loop, 0f, 0f) : null;
	}

	public static AudioClip Play(string soundName, bool loop)
	{
		return _instance ? _instance._Play(soundName, 2000, 1f, 0f, loop, 0f, 0f) : null;
	}

	public static AudioClip PlayExclusive(string soundName, int priority, float volume, float pan, bool loop, float fadeIn, float fadeOut)
	{
		return _instance ? _instance._PlayExclusive(soundName, priority, volume, pan, loop, fadeIn, fadeOut) : null;
	}

	public static AudioClip PlayExclusive(string soundName, int priority, float volume, float pan, bool loop)
	{
		return _instance ? _instance._PlayExclusive(soundName, priority, volume, pan, loop, 0f, 0f) : null;
	}

	public static AudioClip PlayExclusive(string soundName, int priority, bool loop)
	{
		return _instance ? _instance._PlayExclusive(soundName, priority, 1f, 0f, loop, 0f, 0f) : null;
	}

	public static AudioClip PlayExclusive(string soundName, bool loop)
	{
		return _instance ? _instance._PlayExclusive(soundName, 2000, 1f, 0f, loop, 0f, 0f) : null;
	}

	public static AudioClip Play(string soundName, Vector3 position, float minDistance, float maxDistance, bool loop)
	{
		return _instance ? _instance._Play(soundName, position, null, minDistance, maxDistance, loop) : null;
	}

	public static AudioClip PlayExclusive(string soundName, Vector3 position, float minDistance, float maxDistance, bool loop)
	{
		return _instance ? _instance._PlayExclusive(soundName, position, null, minDistance, maxDistance, loop) : null;
	}

	public static AudioClip Play(string soundName, Transform transform, float minDistance, float maxDistance, bool loop)
	{
		return _instance ? _instance._Play(soundName, Vector2.zero, transform, minDistance, maxDistance, loop) : null;
	}

	public static AudioClip PlayExclusive(string soundName, Transform transform, float minDistance, float maxDistance, bool loop)
	{
		return _instance ? _instance._PlayExclusive(soundName, Vector3.zero, transform, minDistance, maxDistance, loop) : null;
	}
}
