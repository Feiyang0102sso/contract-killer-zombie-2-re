using UnityEngine;

public abstract class TutorialScript : MonoBehaviour
{
	protected const int HardCurrencyAmount = 25;

	protected static Color UIBlinkColor1 = new Color(1f, 145f / 255f, 145f / 255f, 107f / 255f);

	protected void GoToNextScript<T>() where T : TutorialScript
	{
		gameObject.AddComponent<T>();
		Object.Destroy(this);
	}

	protected virtual void LateUpdate()
	{
		if (Loader.State == Loader.LoaderState.Loading)
		{
			Object.Destroy(this);
		}
	}

	protected abstract void Finish();

	protected static void CheckHardCurrencySpent()
	{
	}
}
