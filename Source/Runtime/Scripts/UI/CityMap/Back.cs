using System;
using UnityEngine;

public class Back : MonoBehaviour
{
	public GameObject ButtonBackPrefab;

	public GameObject FromScreen;

	public Type ToScreenType;

	private void Awake()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(ButtonBackPrefab) as GameObject;
		gameObject.transform.parent = transform;
		UIButton component = Util.FindTransform(transform, "BackButton").GetComponent<UIButton>();
		GUIUtils.HookButton(component, OnBackTapped);
	}

	public void OnBackTapped()
	{
	}
}
