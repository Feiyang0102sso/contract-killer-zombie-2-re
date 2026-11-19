using System;
using UnityEngine;

[Serializable]
public class GoalIcon
{
	public Goal.Type Type;

	public Texture Image;
}

public class GoalGUI : MonoBehaviour
{
	public GoalIcon[] Icons;

	public GameObject Icon;

	public SpriteText Title;

	public SpriteText Progress;

	public GameObject[] Stars;

	private Goal.Type _goalType;

	private void Awake()
	{
	}

	public Goal.Type GoalType
	{
		get
		{
			return _goalType;
		}
		set
		{
			_goalType = value;
			foreach (GoalIcon goalIcon in Icons)
			{
				if (goalIcon.Type == value)
				{
					Icon.renderer.material.mainTexture = goalIcon.Image;
					break;
				}
			}
		}
	}
}
