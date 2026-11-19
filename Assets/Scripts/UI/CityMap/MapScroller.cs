using UnityEngine;

public class MapScroller : MonoBehaviour
{
	private const float AutoCenteringTime = 1.5f;

	private const float AutoCenteringNonlinearity = 0.75f;

	public float Sensitivity = 1f;

	private Vector3 _x0 = Vector3.zero;

	private Vector2 _viewport = Vector2.zero;

	private float _mapWidth;

	private float _mapHeight;

	private Vector3 _dx = Vector3.zero;

	private int _id = -1;

	private Vector3 _jxLimit = new Vector3(8f, 6f, 0f);

	private Vector3 _jx = Vector3.zero;

	private Vector3 _jdx = Vector3.zero;

	private float _autoCenteringTimeLeft = 1.5f;

	private bool _autoCenteringInitialized;

	private Vector3 _autoCenteringTarget = Vector3.zero;

	public void OnInput(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.DRAG:
			transform.parent.gameObject.BroadcastMessage("RollupAllGoals");
			_dx = ptr.inputDelta * Sensitivity;
			_id = ptr.id;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
			if (_id == ptr.id)
			{
				_id = -1;
			}
			break;
		}
	}

	private void Start()
	{
		GetComponent<UIButton>().AddInputDelegate(OnInput);
		_x0 = transform.position;
		_mapWidth = GetComponent<UIButton>().width;
		_mapHeight = GetComponent<UIButton>().height;
		foreach (Camera camera in Camera.allCameras)
		{
			if (camera.isOrthoGraphic)
			{
				_viewport = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
				break;
			}
		}
	}

	private Vector3 GetScrollCorrection(Vector3 offset)
	{
		float num = _mapWidth / 2.1f - _viewport.x;
		float num2 = _mapHeight / 2.3f - _viewport.y;
		float num3 = _mapWidth / 2.2f - _viewport.x;
		float num4 = _mapHeight / 2f - _viewport.y;
		return new Vector3(
			offset.x > num ? offset.x - num : offset.x < -num3 ? offset.x + num3 : 0f,
			offset.y > num2 ? offset.y - num2 : offset.y < -num4 ? offset.y + num4 : 0f,
			0f);
	}

	private void FixedUpdate()
	{
		float unusedLocal1;
		float unusedLocal2;
		float unusedLocal3;
		float unusedLocal4;
		float unusedLocal5;
		float unusedLocal6;
		Vector3 vector = -_jx * 2f - _jdx * 0.75f;
		_jdx += vector * Time.deltaTime;
		_jx += _jdx * Time.deltaTime;
		_dx += _jdx * Time.deltaTime;
		float value = (_jdx.magnitude - 1f) / 15f;
		value = Mathf.Clamp01(value);
		value = Mathf.Pow(1f - value, 0.25f);
		if (Random.Range(0f, 1f) < value)
		{
			_jdx += new Vector3(Random.Range(-24f, 24f), Random.Range(-24f, 24f), 0f) * Time.deltaTime;
		}
	}

	private void Update()
	{
		transform.position += _dx;
		_dx = Vector3.zero;
		Vector3 offset = transform.position - _x0;
		Vector3 scrollCorrection = GetScrollCorrection(offset);
		float num = _mapWidth / 10f;
		float num2 = 0f;
		float num3 = -_mapHeight / 10f;
		Vector3 vector = new Vector3(
			scrollCorrection.x > num ? scrollCorrection.x - num : scrollCorrection.x < -num ? scrollCorrection.x + num : 0f,
			scrollCorrection.y > num2 ? scrollCorrection.y - num2 : scrollCorrection.y < num3 ? scrollCorrection.y - num3 : 0f,
			0f);
		transform.position -= vector;
		scrollCorrection -= vector;
		if (_id == -1)
		{
			float unusedLocal1;
			Vector3 vector2 = -scrollCorrection * Time.deltaTime * 3f;
			if (vector2.magnitude > scrollCorrection.magnitude)
			{
				vector2 = vector2.normalized * scrollCorrection.magnitude;
			}
			transform.position += vector2;
		}
		UpdateAutoCentering();
	}

	private void UpdateAutoCentering()
	{
		_autoCenteringTimeLeft = Mathf.Max(0f, _autoCenteringTimeLeft - Time.deltaTime);
		if (_autoCenteringTimeLeft == 0f)
		{
			return;
		}
		if (!_autoCenteringInitialized)
		{
			MapLocation[] componentsInChildren = GetComponentsInChildren<MapLocation>();
			int num = 0;
			foreach (MapLocation mapLocation in componentsInChildren)
			{
				if (mapLocation.HasMission)
				{
					num++;
				}
			}
			if (num == 0)
			{
				return;
			}
			_autoCenteringTarget = Vector3.zero;
			foreach (MapLocation mapLocation2 in componentsInChildren)
			{
				if (mapLocation2.HasMission)
				{
					_autoCenteringTarget += mapLocation2.transform.position;
				}
			}
			_autoCenteringTarget *= 1f / (float)num;
			_autoCenteringTarget.z = 0f;
			_autoCenteringInitialized = true;
		}
		Vector3 position = GUIUtils.UICamera().transform.position;
		position.z = 0f;
		_dx = (position - _autoCenteringTarget) * Time.deltaTime * Mathf.Pow(_autoCenteringTimeLeft / 1.5f, 0.75f);
	}

	public Vector3 GetOffset()
	{
		return transform.position - _x0;
	}
}
