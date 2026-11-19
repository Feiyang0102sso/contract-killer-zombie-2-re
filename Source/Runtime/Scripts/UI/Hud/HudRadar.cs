using System.Collections.Generic;
using UnityEngine;

public class HudRadar : MonoBehaviour
{
	private class RadarPoint
	{
		public Transform Source;

		public Transform PointTransform;

		public PackedSprite PointSprite;

		public Color PointColor;

		public float Scale = 1f;

		public bool IsOutdated;
	}

	private const float MaxRadarSqrDistance = 9000f;

	private const float BlinkingAlphaMin = 0.5f;

	private const float BlinkingAlphaMax = 1f;

	private const float BlinkingFadeOutDamping = 30f;

	private const float BlinkingFadeInDamping = 2f;

	private const float BlinkingScale = 1.3f;

	public GameObject RedPointPrefab;

	public HudArrow MissionArrow;

	public float Zoom = 1f;

	public float RadarRadius = 20f;

	public float EffectTime = 2f;

	public float EffectDelay = 3f;

	public float EffectAlphaFrom = 0.5f;

	public float EffectAlphaTo;

	public float EffectScaleMax = 2f;

	public PackedSprite ScanEffect;

	private Player _player;

	private Transform _playerTransform;

	private Transform _cameraTransform;

	private float _effectRadius;

	private Dictionary<Transform, RadarPoint> _points = new Dictionary<Transform, RadarPoint>();

	private List<Transform> _toDelete = new List<Transform>(32);

	private void Awake()
	{
		_effectRadius = ScanEffect.GetComponent<PackedSprite>().width * 0.5f;
		MissionArrow.Radius = 23.5f;
		enabled = false;
	}

	public void RegisterPlayer(Player player)
	{
		_player = player;
		_playerTransform = player.transform;
		_cameraTransform = Scene.MainCamera.transform;
		enabled = true;
		MissionArrow.SetCenterTransform(_playerTransform);
	}

	private float GetRadarSqrDistance(Transform source)
	{
		return source == null ? float.MaxValue : ((source.position - _playerTransform.position).sqrMagnitude * Zoom * Zoom);
	}

	private void MakeAllPointsOutdated()
	{
		foreach (KeyValuePair<Transform, RadarPoint> point in _points)
		{
			point.Value.IsOutdated = true;
		}
	}

	private void UpdateTargets()
	{
		MakeAllPointsOutdated();
		foreach (Zombie zombie in Scene.Zombies)
		{
			UpdateTarget(zombie, Color.red);
		}
		foreach (Special special in Scene.Specials)
		{
			UpdateTarget(special, Color.magenta);
		}
		foreach (Survivor survivor in Scene.Survivors)
		{
			UpdateTarget(survivor, Color.cyan);
		}
		float unusedTargetValue;
		Task currentTask = Scene.Mission.GetCurrentTask();
		if (currentTask != null && currentTask.IsTargetTrackable() && _player != null && _player.GetMissionTargetPathLength() < 30f)
		{
			UpdateTarget(currentTask.GetTarget().transform, Color.yellow, 1.2f);
		}
		UpdateTarget(_playerTransform, Color.green, 1f);
	}

	private void UpdateTarget(Character character, Color color)
	{
		if (!(!character.IsDying()))
		{
			return;
		}
		UpdateTarget(character.transform, color, 1f);
	}

	private void UpdateTarget(Transform source, Color color, float scale)
	{
		RadarPoint value;
		_points.TryGetValue(source, out value);
		if (GetRadarSqrDistance(source) > 9000f)
		{
			return;
		}
		if (value == null)
		{
			value = new RadarPoint();
			value.PointTransform = Singleton.Cache.PerPrefabCache.Instantiate(RedPointPrefab, 6).transform;
			value.PointTransform.parent = transform;
			value.PointColor = color;
			value.PointSprite = value.PointTransform.GetComponent<PackedSprite>();
			value.Scale = scale;
			_points[source] = value;
		}
		value.IsOutdated = false;
	}

	private void Update()
	{
		if (Util.DoRarelyReal(this, "updating_radar", 0.5f))
		{
			UpdateTargets();
		}
		float num = Time.time % (EffectTime + EffectDelay);
		float t = (num > EffectTime ? 0f : (num / EffectTime));
		float num2 = Mathf.Lerp(0f, EffectScaleMax, t);
		float a = Mathf.Lerp(EffectAlphaFrom, EffectAlphaTo, t);
		ScanEffect.transform.localScale = new Vector3(num2, num2, num2);
		ScanEffect.Color = new Color(1f, 1f, 1f, a);
		float num3 = num2 * num2 * _effectRadius * _effectRadius;
		_toDelete.Clear();
		Vector2 normalized = Util.XyzToXz(_cameraTransform.right).normalized;
		Vector2 normalized2 = Util.XyzToXz(_cameraTransform.forward).normalized;
		foreach (KeyValuePair<Transform, RadarPoint> point in _points)
		{
			Transform key = point.Key;
			RadarPoint value = point.Value;
			float radarSqrDistance = GetRadarSqrDistance(key);
			if (radarSqrDistance > 13500f || key == null || value.IsOutdated)
			{
				Singleton.Cache.PerPrefabCache.Destroy(value.PointTransform.gameObject);
				_toDelete.Add(key);
				continue;
			}
			Vector2 vector = Util.XyzToXz(key.position - _playerTransform.position);
			Vector2 vector2 = new Vector2(Vector3.Dot(normalized, vector), Vector3.Dot(normalized2, vector)) * Zoom;
			value.PointTransform.localPosition = Vector3.ClampMagnitude(new Vector3(vector2.x, vector2.y, -1f), RadarRadius);
			float num4 = num3 - radarSqrDistance;
			value.PointColor.a = Mathf.Lerp(value.PointColor.a, num4 > 0f ? 1f : 0.5f, Time.deltaTime * (num4 > 0f ? 30f : 2f));
			value.PointSprite.Color = value.PointColor;
			float num5 = Mathf.Lerp(1f, 1.3f, Util.FractionInInterval(value.PointSprite.Color.a, 0.5f, 1f));
			float num6 = value.Scale * num5;
			value.PointTransform.localScale = new Vector3(num6, num6, num6);
		}
		foreach (Transform item in _toDelete)
		{
			_points.Remove(item);
		}
		MissionArrow.SetTargetTransform(_player.GetCurrentMissionTarget());
	}
}
