using UnityEngine;

public class BulletCamera : MonoBehaviour
{
	private Camera _camera;

	private int _projectileId;

	public void Init(int projectileId)
	{
		_projectileId = projectileId;
		_camera = GetComponent<Camera>();
		Scene.Camera.enabled = false;
		camera.fieldOfView = 50f;
		Singleton.TimeControl.RunBulletTime(1f, 0.05f);
	}

	private void LateUpdate()
	{
		ProjectileManager.Projectile projectileById = Scene.ProjectileManager.GetProjectileById(_projectileId);
		if (Scene.Camera.enabled || !projectileById.enabled)
		{
			Object.Destroy(this);
		}
		transform.position = projectileById.pos;
		transform.rotation = Quaternion.LookRotation(projectileById.vel.normalized);
	}

	private void OnDestroy()
	{
		if (Scene.Exists())
		{
			Scene.Camera.enabled = true;
			Scene.Camera.ResetOnEnableDamping();
		}
		Singleton.TimeControl.StopBulletTime();
	}
}
