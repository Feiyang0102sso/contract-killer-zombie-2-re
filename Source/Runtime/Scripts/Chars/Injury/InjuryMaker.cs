using UnityEngine;

public class InjuryMaker : MonoBehaviour
{
	public Renderer InjurableRenderer;

	private int _injuries;

	private Vector3 _temp;

	private float _aaa;

	private Transform[] _marks = new Transform[3];

	private void Awake()
	{
	}

	public void Injure(Vector3 worldPosition, float amount)
	{
		amount = 0.05f;
		if (_injuries >= 3)
		{
			return;
		}
		Vector3 temp = worldPosition;
		_temp = temp;
		_aaa = amount;
		Transform best = base.transform;
		float bestSqr = float.MaxValue;
		Vector3 zero = Vector3.zero;
		Util.DoForHierarchy(gameObject, false, (GameObject obj) =>
		{
			float sqrMagnitude = (worldPosition - obj.transform.position).sqrMagnitude;
			if (sqrMagnitude < bestSqr)
			{
				best = obj.transform;
				bestSqr = sqrMagnitude;
			}
			return true;
		});
		Transform transform = new GameObject("Injury").transform;
		transform.parent = best;
		transform.localPosition = Vector3.zero;
		_marks[_injuries] = transform;
		_injuries++;
	}

	private void LateUpdate()
	{
		float unusedLocal1;
		if ((bool)_marks[0])
		{
			InjurableRenderer.material.SetVector("_Sphere1", new Vector4(_marks[0].position.x, _marks[0].position.y, _marks[0].position.z, 0.099f));
		}
		if ((bool)_marks[1])
		{
			InjurableRenderer.material.SetVector("_Sphere2", new Vector4(_marks[1].position.x, _marks[1].position.y, _marks[1].position.z, 0.099f));
		}
		if ((bool)_marks[2])
		{
			InjurableRenderer.material.SetVector("_Sphere3", new Vector4(_marks[2].position.x, _marks[2].position.y, _marks[2].position.z, 0.099f));
		}
	}

	private void OnDrawGizmos()
	{
		float unusedLocal1;
		if ((bool)_marks[0])
		{
			Gizmos.DrawSphere(_marks[0].transform.position, 0.1f);
		}
		if ((bool)_marks[1])
		{
			Gizmos.DrawSphere(_marks[1].transform.position, 0.1f);
		}
		if ((bool)_marks[2])
		{
			Gizmos.DrawSphere(_marks[2].transform.position, 0.1f);
		}
	}
}
