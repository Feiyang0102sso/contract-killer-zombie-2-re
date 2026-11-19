using System;
using System.IO;

public class Settings : MonoBehaviourAndLog
{
	public class SavedData
	{
		public float Sensitivity = 1f;

		public bool MiniZombieControls;

		public bool MeleeButton;

		public float SoundVolume = 1f;

		public float MusicVolume = 1f;

		public bool Notifications = true;

		public bool UselessSwipe;

		public bool InvertYAxis;
	}

	public bool InvulnerablePlayer;

	public bool InvulnerableSurvivor;

	public bool ZombieAnimations = true;

	public int ZombieSkinQuality;

	public bool ZombieRender = true;

	public bool ZombieUpdate = true;

	private SavedData _data = new SavedData();

	public Action OnSettingsChanged = () =>
	{
	};

	public float Sensitivity
	{
		get
		{
			return _data.Sensitivity;
		}
		set
		{
			_data.Sensitivity = value;
		}
	}

	public bool MiniZombieControls
	{
		get
		{
			return _data.MiniZombieControls;
		}
		set
		{
			_data.MiniZombieControls = value;
			OnSettingsChanged();
		}
	}

	public bool MeleeButton
	{
		get
		{
			return _data.MeleeButton;
		}
		set
		{
			_data.MeleeButton = value;
			OnSettingsChanged();
		}
	}

	public float SoundVolume
	{
		get
		{
			return _data.SoundVolume;
		}
		set
		{
			_data.SoundVolume = value;
			OnSettingsChanged();
		}
	}

	public float MusicVolume
	{
		get
		{
			return _data.MusicVolume;
		}
		set
		{
			_data.MusicVolume = value;
			OnSettingsChanged();
		}
	}

	public bool Notifications
	{
		get
		{
			return _data.Notifications;
		}
		set
		{
			_data.Notifications = value;
			OnSettingsChanged();
			GameAnalytics.LogNotificationSettingsChanged(value);
		}
	}

	public bool UselessSwipe
	{
		get
		{
			return _data.UselessSwipe;
		}
		set
		{
			_data.UselessSwipe = value;
			OnSettingsChanged();
		}
	}

	public bool InvertYAxis
	{
		get
		{
			return _data.InvertYAxis;
		}
		set
		{
			_data.InvertYAxis = value;
			OnSettingsChanged();
		}
	}

	public float GetCameraSensitivity()
	{
		float unusedSensitivity;
		float sensitivity = _data.Sensitivity;
		return sensitivity > 1f ? 1f + (sensitivity - 1f) * 4f : 1f / (1f + (1f - sensitivity) * 4f);
	}

	private void Load()
	{
		try
		{
			string pathName = Path.Combine(StorageManager.PersistentDataPath, Const.SettingsFileName);
			_data = (SavedData)StorageManager.ReadXmlFromLocation(pathName, _data.GetType());
		}
		catch (Exception ex)
		{
			if (ex is StorageManager.StorageManagerException)
			{
			}
			else
			{
			}
		}
	}

	public void Save()
	{
		StorageManager.WriteXml(Const.SettingsFileName, _data);
	}

	private void Awake()
	{
		LogTo("Other.Settings");
		Load();
		Save();
	}
}
