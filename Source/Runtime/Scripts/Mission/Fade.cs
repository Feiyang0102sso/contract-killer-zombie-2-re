using UnityEngine;

public class Fade : MonoBehaviour
{
	public bool inOrOut = true;

	public float duration = 3f;

	public Color color = Color.white;

	private void OnEffect()
	{
		if (inOrOut)
		{
			Singleton.UI.Fader.FadeIn(duration, color);
		}
		else
		{
			Singleton.UI.Fader.FadeOut(duration, color);
		}
	}
}
