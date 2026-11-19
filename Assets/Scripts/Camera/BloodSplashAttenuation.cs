using System.Collections.Generic;
using UnityEngine;

public class BloodSplashAttenuation : MonoBehaviour
{
	public float AttenuationTime = 10f;

	private Projector _projector;

	private float _lifeTime;

	private float _initialFarClipPlane;

	private Vector3 _splashCenter = Vector3.zero;

	private float _splashRadius;

	private static int _numActiveSplashes = 0;

	private static Dictionary<GameObject, int> _globallyAffectedObjects;

	private List<GameObject> _locallyAffectedObjects;

	private void SetLayerForHierarchy(GameObject gameObject, int layer)
	{
		gameObject.layer = layer;
		int childCount = gameObject.transform.GetChildCount();
		for (int i = 0; i < childCount; i++)
		{
			SetLayerForHierarchy(gameObject.transform.GetChild(i).gameObject, layer);
		}
	}

	private void Awake()
	{
		if (_globallyAffectedObjects == null)
		{
			_globallyAffectedObjects = new Dictionary<GameObject, int>();
		}
		_numActiveSplashes++;
		_projector = gameObject.GetComponent<Projector>();
		if ((bool)_projector)
		{
			_initialFarClipPlane = _projector.farClipPlane;
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void OnDestroy()
	{
		_numActiveSplashes--;
		if (!(_locallyAffectedObjects == null))
		{
			foreach (GameObject locallyAffectedObject in _locallyAffectedObjects)
			{
				if ((bool)locallyAffectedObject)
				{
					int num = _globallyAffectedObjects[locallyAffectedObject];
					num--;
					if (num == 0)
					{
						_globallyAffectedObjects.Remove(locallyAffectedObject);
						SetLayerForHierarchy(locallyAffectedObject, Const.LayerDefault);
					}
					else
					{
						_globallyAffectedObjects.Remove(locallyAffectedObject);
						_globallyAffectedObjects.Add(locallyAffectedObject, num);
					}
				}
			}
		}
	}

	private void Update()
	{
		_lifeTime += Time.deltaTime;
		_lifeTime = (_lifeTime > AttenuationTime ? AttenuationTime : _lifeTime);
		float num = 1f - _lifeTime / AttenuationTime;
		_projector.farClipPlane = _initialFarClipPlane * num;
		if (_lifeTime == AttenuationTime)
		{
			Object.Destroy(gameObject);
		}
	}

	public void SetSplashSphere(Vector3 center, float radius)
	{
		_splashCenter = center;
		_splashRadius = radius;
		Collider[] array = Physics.OverlapSphere(center, radius, Const.LayerMaskObstacles);
		foreach (Collider collider in array)
		{
			GameObject reference = collider.gameObject;
			Context component = reference.GetComponent<Context>();
			if ((bool)component)
			{
				reference = component.Reference;
			}
			if (_locallyAffectedObjects == null)
			{
				_locallyAffectedObjects = new List<GameObject>();
			}
			_locallyAffectedObjects.Add(reference);
			if (_globallyAffectedObjects.ContainsKey(reference))
			{
				int num = _globallyAffectedObjects[reference];
				_globallyAffectedObjects.Remove(reference);
				_globallyAffectedObjects.Add(reference, num + 1);
				continue;
			}
			_globallyAffectedObjects.Add(reference, 1);
			if ((bool)component)
			{
				SetLayerForHierarchy(reference, Const.LayerDefault);
			}
			else
			{
				reference.layer = Const.LayerDefault;
			}
		}
	}

	public Vector3 GetSplashSphereCenter()
	{
		return _splashCenter;
	}

	public float GetSplashSphereRadius()
	{
		return _splashRadius;
	}

	public static int GetNumActiveSplashes()
	{
		return _numActiveSplashes;
	}
}
