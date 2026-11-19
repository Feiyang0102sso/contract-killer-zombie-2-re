using UnityEngine;

public class CustomTask : Task
{
	public enum CustomType
	{
		Invalid,
		SurvivalTutorialFirstKill
	}

	public CustomType customType;

	private void Awake()
	{
		LogTo("Mission.Task");
	}

	protected override void OnStarted()
	{
		Character componentInChildren = GetComponentInChildren<Character>();
		HintArrow.Create(componentInChildren.gameObject);
		base.OnStarted();
	}

	protected override void Update()
	{
		if (!Singleton.Tutorial.TutorialStateIs<Tutorial.SurvivalModeIntroduction>())
		{
			Util.DoForHierarchy(gameObject, false, (GameObject node) =>
			{
				Object.DestroyImmediate(node);
				return false;
			});
			SendMessageUpwards("OnTaskCompleted");
			return;
		}
		base.Update();
	}
}
