using UnityEngine;

public class MotionCapture : MonoBehaviourAndLog
{
	private Vector3[] _position = new Vector3[3];

	private Vector3[] _rotation = new Vector3[3];

	private float[] _time = new float[3];

	private uint _index;

	private uint _frames;

	private void Update()
	{
		if (!(!TimeControl.IsPaused()))
		{
			return;
		}
		_position[_index] = transform.position;
		_rotation[_index] = transform.rotation.eulerAngles;
		_time[_index] = Time.time;
		_index++;
		_index %= 3u;
		_frames++;
	}

	public Vector3 GetPosition()
	{
		return transform.position;
	}

	public Vector3 Velocity()
	{
		return Interpolate(_position);
	}

	public Vector3 AngularVelocity()
	{
		return Interpolate(_rotation);
	}

	public Vector3 Interpolate(Vector3[] values)
	{
		if (_frames < 3)
		{
			return Vector3.zero;
		}
		uint index = _index;
		uint num = (_index + 1) % 3;
		uint num2 = (_index + 2) % 3;
		float num3 = _time[num] - _time[index];
		float num4 = _time[num2] - _time[num];
		Vector3 vector = (values[num] - values[index]) / num3;
		Vector3 vector2 = (values[num2] - values[num]) / num4;
		return vector * num3 / (num3 + num4) + vector2 * num4 / (num3 + num4);
	}
}
