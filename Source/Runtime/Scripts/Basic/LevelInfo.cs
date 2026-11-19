using System;

[Serializable]
public class LevelInfo
{
	public string title;

	public string description;

	public string sceneName;

	public Mission.Type missionType;

	public LevelInfo(string sceneName_)
	{
		sceneName = sceneName_;
	}

	public LevelInfo()
	{
	}
}
