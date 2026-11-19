using UnityEngine;

public class MapHand : MonoBehaviour
{
	public GameObject LightSpot;

	public GameObject LeftHandReflex;

	private Camera _camera;

	private Vector3 _viewport = Vector3.zero;

	private Vector3 _lightSpotOffset = Vector3.zero;

	private float _leftHandReflexOffsetMin = float.PositiveInfinity;

	private float _leftHandReflexOffsetMax;

	private Vector3 _jxLimit = new Vector3(8f, 6f, 1f);

	private Vector3 _jx = Vector3.zero;

	private Vector3 _jdx = Vector3.zero;

	private void Start()
	{
		if ((bool)LightSpot)
		{
			_lightSpotOffset = LightSpot.transform.position - transform.position;
		}
		if ((bool)LeftHandReflex)
		{
			_leftHandReflexOffsetMax = (LeftHandReflex.transform.position - transform.position).magnitude;
		}
		foreach (Camera camera in Camera.allCameras)
		{
			if (camera.orthographic)
			{
				_camera = camera;
				_viewport = new Vector3(camera.orthographicSize * camera.aspect, camera.orthographicSize, 0f);
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		float unusedLocal1;
		float unusedLocal2;
		Vector3 vector = -_jx * 5f - _jdx * 0.75f;
		_jdx += vector * Time.fixedDeltaTime;
		_jx += _jdx * Time.fixedDeltaTime;
		_jx.x = _jx.x > _jxLimit.x ? _jxLimit.x : _jx.x;
		_jx.x = _jx.x < -_jxLimit.x ? -_jxLimit.x : _jx.x;
		_jx.y = _jx.y > _jxLimit.y ? _jxLimit.y : _jx.y;
		_jx.y = _jx.y < -_jxLimit.y ? -_jxLimit.y : _jx.y;
		_jx.z = _jx.z > _jxLimit.z ? _jxLimit.z : _jx.z;
		_jx.z = _jx.z < -_jxLimit.z ? -_jxLimit.z : _jx.z;
		float unusedLocal3;
		float unusedLocal4;
		float unusedLocal5;
		float unusedLocal6;
		float value = _jdx.magnitude / 4f;
		value = Mathf.Clamp01(value);
		value = Mathf.Pow(1f - value, 0.25f);
		if (Random.Range(0f, 1f) < value)
		{
			_jdx += new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f), Random.Range(-8f, 8f)) * Time.fixedDeltaTime;
		}
	}

	private void Update()
	{
		if (!_camera)
		{
			return;
		}
		Vector3 offset = transform.parent.GetComponent<MapScroller>().GetOffset();
		Vector3 v = new Vector3(GetComponent<PackedSprite>().width / 2f, GetComponent<PackedSprite>().height / 2f, 0f);
		Vector3 v2 = new Vector3(0f, GetComponent<PackedSprite>().height / 2.25f, 0f);
		Vector3 vector = transform.localToWorldMatrix.MultiplyPoint(v);
		Vector3 vector2 = transform.localToWorldMatrix.MultiplyPoint(v2);
		Vector3 vector3 = _camera.transform.localToWorldMatrix.MultiplyPoint(_viewport);
		float speed;
		vector3 += _jx * 10f;
		vector3 += Vector3.right * _jxLimit.x;
		vector3 += Vector3.up * _jxLimit.y;
		Vector3 vector4 = vector3 - vector;
		Vector3 vector5 = new Vector3(vector4.x > 0f ? vector4.x : 0f, vector4.y < 0f ? vector4.y : 0f, 0f);
		transform.position += vector5;
		vector = transform.localToWorldMatrix.MultiplyPoint(v);
		vector2 = transform.localToWorldMatrix.MultiplyPoint(v2);
		vector3 = _camera.transform.localToWorldMatrix.MultiplyPoint(_viewport);
		Vector3 vector6 = vector3 - vector2;
		vector6.z = 0f;
		float minX;
		float maxX;
		float minY;
		float maxY;
		float targetX;
		float targetY;
		float horizontalDistance;
		float verticalDistance;
		float num = Mathf.Pow(Mathf.Clamp01(vector6.magnitude / 100f), 0.5f);
		float num2 = 0f * (1f - num) + 5f * num;
		Vector3 vector7 = vector6 * num2 * Time.deltaTime;
		if (vector7.magnitude > vector6.magnitude)
		{
			vector7 = vector6;
		}
		transform.position += vector7;
		transform.localScale = Vector3.one * (1f + _jx.z * 0.125f);
		if ((bool)LightSpot)
		{
			Vector3 vector8 = new Vector3(offset.x * 0.05f, offset.y * 0.25f, 0f);
			LightSpot.transform.position = transform.position + _lightSpotOffset - vector8;
			LightSpot.transform.localScale = Vector3.one * (3.083333f + _jx.z * 0.5f);
		}
		if ((bool)LeftHandReflex && (bool)LightSpot)
		{
			float magnitude = (LightSpot.transform.position - LeftHandReflex.transform.position).magnitude;
			if (_leftHandReflexOffsetMin > magnitude)
			{
				_leftHandReflexOffsetMin = magnitude;
			}
			if (_leftHandReflexOffsetMin < _leftHandReflexOffsetMax)
			{
				float value = (magnitude - _leftHandReflexOffsetMin) / (_leftHandReflexOffsetMax - _leftHandReflexOffsetMin);
				value = Mathf.Pow(1f - Mathf.Clamp01(value), 4f);
				LeftHandReflex.GetComponent<PackedSprite>().Color = new Color(1f, 1f, 1f, value);
			}
		}
	}
}
