using UnityEngine;

public class LoadingRotation : MonoBehaviour
{
	private float delta;

	private float deltaThreshold = 0.2f;

	private void Update()
	{
		// PC port: real time via PcFrameRate.RealDeltaTime; deltaTime / timeScale jumped by 333 s on load frames
		delta += PcFrameRate.RealDeltaTime;
		if (delta > deltaThreshold)
		{
			transform.RotateAroundLocal(Vector3.forward, -0.5235984f);
			delta -= deltaThreshold;
		}
	}
}
