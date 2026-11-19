using UnityEngine;

public class Effect : MonoBehaviour
{
	public float initialDelay;

	private void OnActivate()
	{
		Invoke("Play", initialDelay);
	}

	private void Play()
	{
		BroadcastMessage("OnEffect", SendMessageOptions.DontRequireReceiver);
		foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>())
		{
			particleSystem.enableEmission = true;
			particleSystem.Play();
		}
	}
}
