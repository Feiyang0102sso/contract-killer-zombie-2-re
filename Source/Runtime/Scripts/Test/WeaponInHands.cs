using UnityEngine;

public class WeaponInHands : MonoBehaviour
{
	public GameObject WeaponPrefab;

	private void Awake()
	{
		if ((bool)WeaponPrefab)
		{
			GameObject obj = Object.Instantiate(WeaponPrefab) as GameObject;
			GameObject gameObject = obj.FindChildInHierarchy("weapon1");
			GameObject gameObject2 = obj.FindChildInHierarchy("magazine1");
			GameObject gameObject3 = base.gameObject.FindChildInHierarchy("weapon1");
			if ((bool)gameObject3)
			{
				gameObject.transform.position = gameObject3.transform.position;
				gameObject.transform.eulerAngles = gameObject3.transform.eulerAngles;
				gameObject.transform.parent = gameObject3.transform;
			}
			gameObject3 = base.gameObject.FindChildInHierarchy("magazine1");
			if ((bool)gameObject3)
			{
				gameObject2.transform.position = gameObject3.transform.position;
				gameObject2.transform.eulerAngles = gameObject3.transform.eulerAngles;
				gameObject2.transform.parent = gameObject3.transform;
			}
			gameObject = obj.FindChildInHierarchy("weapon2");
			gameObject2 = obj.FindChildInHierarchy("magazine2");
			gameObject3 = base.gameObject.FindChildInHierarchy("weapon2");
			if ((bool)gameObject3)
			{
				gameObject.transform.position = gameObject3.transform.position;
				gameObject.transform.eulerAngles = gameObject3.transform.eulerAngles;
				gameObject.transform.parent = gameObject3.transform;
			}
			gameObject3 = base.gameObject.FindChildInHierarchy("magazine2");
			if ((bool)gameObject3)
			{
				gameObject2.transform.position = gameObject3.transform.position;
				gameObject2.transform.eulerAngles = gameObject3.transform.eulerAngles;
				gameObject2.transform.parent = gameObject3.transform;
			}
		}
	}

	private void Update()
	{
	}
}
