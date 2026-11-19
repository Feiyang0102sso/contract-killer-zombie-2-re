using UnityEngine;

public class OptionalObject : MonoBehaviour
{
	public enum Type
	{
		VOLUME_LIGHT,
		GARBAGE,
		MISC_OBJECTS,
		BLOOD_SPLASH
	}

	public Type ObjectType;

	private void Awake()
	{
		bool flag = true;
		switch (ObjectType)
		{
		case Type.VOLUME_LIGHT:
			flag = Singleton.Quality.VolumeLightOnScene;
			break;
		case Type.GARBAGE:
			flag = Singleton.Quality.GarbageOnScene;
			break;
		case Type.MISC_OBJECTS:
			flag = Singleton.Quality.MiscObjectsOnScene;
			break;
		case Type.BLOOD_SPLASH:
			flag = Singleton.Quality.BloodSplashesOnScene;
			break;
		}
		if (flag)
		{
			Object.Destroy(this);
		}
		else
		{
			Object.Destroy(gameObject);
		}
	}
}
