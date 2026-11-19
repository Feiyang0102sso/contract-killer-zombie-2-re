using UnityEngine;

public class WaitForAction : MonoBehaviour
{
	public enum Type
	{
		UsePowerUp,
		ChangeWeapon,
		PerformAttack,
		CollectBonus,
		CollectAmmo,
		EnterZoomMode
	}

	public Type type;

	public string argument = string.Empty;

	private void OnActivate()
	{
	}

	public void ReportAction(Type type_, string argument_)
	{
		if (type == type_ && (argument == string.Empty || argument == argument_))
		{
			SendMessageUpwards("OnTaskCompleted");
		}
	}
}
