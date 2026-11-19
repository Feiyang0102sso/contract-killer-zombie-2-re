using UnityEngine;

public class ShakeCamera : MonoBehaviour
{
	private void OnEffect()
	{
		Scene.Camera.Shake(2f, 0.3f, 5f, 2f);
	}
}
