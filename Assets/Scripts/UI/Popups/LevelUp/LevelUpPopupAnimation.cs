using System;
using UnityEngine;

public class LevelUpPopupAnimation : MonoBehaviour
{
	public GameObject Evelyn;

	public LevelUpPopup LevelUpPopup;

	private void Awake()
	{
	}

	private void Start()
	{
		Evelyn.transform.localScale = Vector3.zero;
	}

	private void BringInEvelyn()
	{
		Evelyn.transform.localScale = Vector3.one;
		Animation component = Evelyn.GetComponent<Animation>();
		component["lvlup2"].wrapMode = WrapMode.Loop;
		component.Play("lvlup1");
		component.PlayQueued("lvlup2", QueueMode.CompleteOthers);
	}

	private void ShowUI()
	{
		LevelUpPopup.AllUI.SetActiveRecursively(true);
		Array.ForEach(LevelUpPopup.AllUI.GetComponentsInChildren<UIAnimation>(true), (UIAnimation c) =>
		{
			c.RunAnimation();
		});
		LevelUpPopup.SendMessage("ShowNextReward", SendMessageOptions.RequireReceiver);
	}
}
