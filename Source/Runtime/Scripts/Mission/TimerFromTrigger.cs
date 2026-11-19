using UnityEngine;

public class TimerFromTrigger : MonoBehaviour
{
	public float rechargeIncrease = 5f;

	private Trigger trigger;

	private void Awake()
	{
		trigger = (!(!gameObject.GetParent()) ? gameObject.GetParent().GetComponent<Trigger>() : null);
	}

	public string GetText()
	{
		if (trigger.rechargeLeftTime() == 0f)
		{
			return string.Empty;
		}
		int num = Mathf.FloorToInt(trigger.rechargeLeftTime());
		return string.Format("{0}:{1:00}", num / 60, num % 60);
	}

	private void OnUse()
	{
		trigger.rechargeDuration += rechargeIncrease;
	}
}
