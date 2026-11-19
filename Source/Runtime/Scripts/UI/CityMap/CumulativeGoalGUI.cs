using System.Collections.Generic;
using UnityEngine;

public class CumulativeGoalGUI : MonoBehaviour
{
	public SpriteText Title;

	public SpriteText RewardMoney;

	public GameObject EmptyStar;

	public GameObject FullStar;

	private List<GameObject> _emptyStars = new List<GameObject>();

	private List<GameObject> _fullStars = new List<GameObject>();

	private void Awake()
	{
	}

	public void AddStar(GameObject obj, bool isFull)
	{
		if (isFull)
		{
			_fullStars.Add(obj);
		}
		else
		{
			_emptyStars.Add(obj);
		}
	}

	public GameObject GetNextEmptyStar()
	{
		return !(_emptyStars.Count <= 0) ? _emptyStars[0] : null;
	}

	public void FillTheStar(GameObject obj)
	{
		bool flag = _emptyStars.Remove(obj);
		_fullStars.Add(obj);
		GameObject gameObject = Object.Instantiate(FullStar) as GameObject;
		gameObject.active = true;
		gameObject.transform.position = obj.transform.position;
		gameObject.transform.parent = obj.transform;
		gameObject.transform.position -= Vector3.forward * 0.25f;
		Object.Destroy(gameObject.GetComponent<UIAlign>());
	}
}
