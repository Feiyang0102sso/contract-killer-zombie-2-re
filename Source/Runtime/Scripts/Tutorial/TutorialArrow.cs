using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
	private const float ArrowOffsetY = -10f;

	private const float defaultDamping = 15f;

	public GameObject Arrow;

	private Vector3 _targetPosition;

	private float _targetAngle;

	private float _distance;

	private float _damping;

	private bool _show = true;

	private void Awake()
	{
	}

	public void SetTarget(UIButton button, float angle, float distance)
	{
		SetTarget(button.transform.position + button.GetCenterPoint(), angle, distance);
	}

	public void SetTarget(Vector3 position, float angle)
	{
		SetTarget(position, angle, 0f);
	}

	public void SetTarget(Vector3 position, float angle, float distance)
	{
		SetTarget(position, angle, distance, 15f);
	}

	public void SetTarget(Vector3 position, float angle, float distance, float damping)
	{
		Show(true);
		_targetPosition = position;
		_targetAngle = angle;
		_distance = distance;
		_damping = damping;
	}

	public void Hide()
	{
		Show(false);
	}

	public void Show(bool show)
	{
		if (!(_show != show))
		{
			return;
		}
		_show = show;
		gameObject.SetActiveRecursively(_show);
	}

	private void Update()
	{
		// PC port: real time via PcFrameRate.RealDeltaTime; deltaTime / timeScale jumped by 333 s on load frames
		float t = PcFrameRate.RealDeltaTime * _damping;
		transform.position = Vector3.Lerp(transform.position, _targetPosition, t);
		transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(transform.localEulerAngles.z, _targetAngle, t));
		float unusedOffset1;
		float unusedOffset2;
		float num = (Mathf.Sin(Time.realtimeSinceStartup * 12f) + 1f) * 0.5f * 7f;
		Arrow.transform.localPosition = new Vector3(0f, -10f - _distance - num, -2.1f);
	}
}
