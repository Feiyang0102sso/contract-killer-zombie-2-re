using UnityEngine;

public class OcclusionController : MonoBehaviour
{
	private struct Occluder
	{
		public Transform transform;

		public float worldRadius;

		public float localRadius;

		public Occluder(Transform t, float wR, float lR)
		{
			transform = t;
			worldRadius = wR;
			localRadius = lR;
		}
	}

	public const int MaxOccluders = 3;

	public const float ShadowAltitude = 0.05f;

	public const float ColorSamplingDistanceTreshold = 1f;

	private string[] _occluderName = new string[3] { "_Occluder1", "_Occluder2", "_Occluder3" };

	private string _ambientColorName = "_Ambient";

	private Occluder[] _occluders;

	private int _numOccluders;

	private Material _material;

	private bool _adaptiveMode;

	private Vector3 _lastColorSamplingPos = Vector3.zero;

	private Transform _thisTransform;

	private MeshRenderer _thisMeshRenderer;

	private void Awake()
	{
		_thisTransform = transform;
		_thisMeshRenderer = GetComponent<MeshRenderer>();
		_material = _thisMeshRenderer.material;
	}

	private void Update()
	{
		if (_adaptiveMode && _numOccluders > 0)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < _numOccluders; i++)
			{
				zero += _occluders[i].transform.position;
			}
			zero *= 1f / (float)_numOccluders;
			float unusedLocal1;
			RaycastHit[] array = Physics.RaycastAll(zero, Vector3.down, 10f, Const.LayerMaskObstacles);
			float num = 10f;
			foreach (RaycastHit raycastHit in array)
			{
				if (raycastHit.distance < num)
				{
					num = raycastHit.distance;
				}
			}
			_thisTransform.position = zero + Vector3.down * (num - 0.05f);
		}
	}

	private void LateUpdate()
	{
		if (_occluders != null && _material != null)
		{
			for (int i = 0; i < _numOccluders; i++)
			{
				Vector4 vector = _thisTransform.worldToLocalMatrix.MultiplyPoint(_occluders[i].transform.position);
				vector.w = _occluders[i].localRadius;
				vector.z = (vector.z < _occluders[i].localRadius ? _occluders[i].localRadius : vector.z);
				_material.SetVector(_occluderName[i], vector);
			}
			if ((bool)LightmapSettings.lightProbes && (_lastColorSamplingPos - _thisTransform.position).magnitude > 1f)
			{
				_lastColorSamplingPos = _thisTransform.position;
				LightProbeSampler.InterpolateSH(_thisTransform.position, _thisMeshRenderer);
				_material.SetColor(_ambientColorName, LightProbeSampler.ShadeSH(Vector3.down));
			}
		}
	}

	public void SetAdaptiveMode(bool flag)
	{
		_adaptiveMode = flag;
	}

	public void Cleanup()
	{
		_occluders = null;
		_numOccluders = 0;
	}

	public void AddOccluder(Transform transform, float radius)
	{
		if (_occluders == null)
		{
			_occluders = new Occluder[3];
		}
		if (_numOccluders < 3)
		{
			float magnitude = _thisTransform.worldToLocalMatrix.MultiplyVector(new Vector3(radius, 0f, 0f)).magnitude;
			_occluders[_numOccluders] = new Occluder(transform, radius, magnitude);
			_numOccluders++;
		}
	}
}
