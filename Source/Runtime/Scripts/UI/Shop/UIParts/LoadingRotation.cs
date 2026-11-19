using UnityEngine;

public class LoadingRotation : MonoBehaviour
{
	private float delta;

	private float deltaThreshold = 0.2f;

	private void Update()
	{
		delta += Time.deltaTime / Time.timeScale;
		if (delta > deltaThreshold)
		{
			transform.RotateAroundLocal(Vector3.forward, -0.5235984f);
			delta -= deltaThreshold;
		}
	}
}
