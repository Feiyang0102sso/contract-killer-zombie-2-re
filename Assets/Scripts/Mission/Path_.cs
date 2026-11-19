using UnityEngine;

public class Path_ : MonoBehaviour
{
	private void OnActivate()
	{
		if (gameObject.name == "Path")
		{
			Util.DoForHierarchy(gameObject, false, (GameObject node) =>
			{
				Scene.PlayerProgress.AddPoint(node);
				return false;
			});
		}
		else if (gameObject.name == "$LevelStart" || gameObject.name == "~LevelEnd")
		{
			Scene.PlayerProgress.AddPoint(gameObject);
		}
		else
		{
		}
	}
}
