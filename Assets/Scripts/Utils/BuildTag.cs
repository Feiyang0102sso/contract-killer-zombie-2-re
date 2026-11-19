using UnityEngine;

public class BuildTag
{
	private static string _buildTag;

	public static string Get()
	{
		if (_buildTag == null)
		{
			TextAsset textAsset = Resources.Load("build_tag") as TextAsset;
			if (textAsset != null)
			{
				_buildTag = textAsset.text;
			}
			if (_buildTag == null)
			{
				_buildTag = string.Empty;
			}
			_buildTag = _buildTag.Trim();
		}
		return _buildTag;
	}
}
