using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderQueue : MonoBehaviour
{
	private class AABB
	{
		public Vector3 inf;

		public Vector3 sup;

		public AABB()
		{
			inf = Vector3.zero;
			sup = Vector3.zero;
		}

		public AABB(Bounds bounds)
		{
			inf = bounds.center - bounds.extents;
			sup = bounds.center + bounds.extents;
		}

		public void Add(AABB aabb)
		{
			inf.x = Mathf.Min(inf.x, aabb.inf.x);
			inf.y = Mathf.Min(inf.y, aabb.inf.y);
			inf.z = Mathf.Min(inf.z, aabb.inf.z);
			sup.x = Mathf.Max(sup.x, aabb.sup.x);
			sup.y = Mathf.Max(sup.y, aabb.sup.y);
			sup.z = Mathf.Max(sup.z, aabb.sup.z);
		}

		public Vector3 Center()
		{
			return inf + (sup - inf) * 0.5f;
		}

		public Vector3 Height()
		{
			return new Vector3(0f, sup.y - inf.y, 0f);
		}

		public Vector3 Width()
		{
			return new Vector3(sup.x - inf.x, 0f, 0f);
		}

		public Vector3 Depth()
		{
			return new Vector3(0f, 0f, sup.z - inf.z);
		}

		public Vector3 Corner(int i)
		{
			switch (i)
			{
			case 0:
				return inf;
			case 1:
				return inf + Width();
			case 2:
				return inf + Width() + Depth();
			case 3:
				return inf + Depth();
			case 4:
				return inf + Height();
			case 5:
				return inf + Width() + Height();
			case 6:
				return inf + Width() + Depth() + Height();
			case 7:
				return inf + Depth() + Height();
			default:
				return sup;
			}
		}

		public void DebugDraw(Color color)
		{
			Debug.DrawLine(Corner(0), Corner(1), color);
			Debug.DrawLine(Corner(1), Corner(2), color);
			Debug.DrawLine(Corner(2), Corner(3), color);
			Debug.DrawLine(Corner(3), Corner(0), color);
			Debug.DrawLine(Corner(4), Corner(5), color);
			Debug.DrawLine(Corner(5), Corner(6), color);
			Debug.DrawLine(Corner(6), Corner(7), color);
			Debug.DrawLine(Corner(7), Corner(4), color);
			Debug.DrawLine(Corner(0), Corner(0) + Height(), color);
			Debug.DrawLine(Corner(1), Corner(1) + Height(), color);
			Debug.DrawLine(Corner(2), Corner(2) + Height(), color);
			Debug.DrawLine(Corner(3), Corner(3) + Height(), color);
		}
	}

	private const int BackgroundQueue = 1000;

	private const int OpaqueQueue = 2000;

	private const int TransparentQueue = 3000;

	private const int OverlayQueue = 4000;

	public float OverrideableRadius = 10f;

	private Dictionary<GameObject, int> _overridedObjects = new Dictionary<GameObject, int>();

	private Dictionary<SkinnedMeshRenderer, List<Material>> _originalSkins = new Dictionary<SkinnedMeshRenderer, List<Material>>();

	private Dictionary<MeshRenderer, List<Material>> _originalMeshes = new Dictionary<MeshRenderer, List<Material>>();

	private Dictionary<GameObject, List<MeshRenderer>> _meshCache = new Dictionary<GameObject, List<MeshRenderer>>();

	private AABB GetObjectBounds(GameObject gameObject)
	{
		AABB aABB = null;
		MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			AABB aABB2 = new AABB(meshRenderer.bounds);
			if (aABB != null)
			{
				aABB.Add(aABB2);
			}
			else
			{
				aABB = aABB2;
			}
		}
		if (!gameObject.isStatic)
		{
			SkinnedMeshRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
			{
				AABB aABB3 = new AABB(skinnedMeshRenderer.bounds);
				if (aABB != null)
				{
					aABB.Add(aABB3);
				}
				else
				{
					aABB = aABB3;
				}
			}
		}
		return aABB;
	}

	private Material OverrideMaterial(Material source, int queue)
	{
		Material material = new Material(source);
		material.name = string.Format("{0}(Overrided)", source.name);
		material.renderQueue = queue;
		return material;
	}

	private void SetRenderQueue(SkinnedMeshRenderer skin, int queue)
	{
		if (!_originalSkins.ContainsKey(skin))
		{
			List<Material> list = new List<Material>();
			list.Add(skin.material);
			for (int i = 1; i < skin.materials.Length; i++)
			{
				list.Add(skin.materials[i]);
			}
			_originalSkins.Add(skin, list);
			skin.material = OverrideMaterial(list[0], queue);
			for (int j = 1; j < skin.materials.Length; j++)
			{
				skin.materials[j] = OverrideMaterial(list[j], queue);
			}
		}
		else
		{
			skin.material.renderQueue = queue;
			for (int k = 1; k < skin.materials.Length; k++)
			{
				skin.materials[k].renderQueue = queue;
			}
		}
	}

	private void SetRenderQueue(MeshRenderer mesh, int queue)
	{
		if (!_originalMeshes.ContainsKey(mesh))
		{
			List<Material> list = new List<Material>();
			list.Add(mesh.material);
			for (int i = 1; i < mesh.materials.Length; i++)
			{
				list.Add(mesh.materials[i]);
			}
			_originalMeshes.Add(mesh, list);
			mesh.material = OverrideMaterial(list[0], queue);
			for (int j = 1; j < mesh.materials.Length; j++)
			{
				mesh.materials[j] = OverrideMaterial(list[j], queue);
			}
		}
		else
		{
			mesh.material.renderQueue = queue;
			for (int k = 1; k < mesh.materials.Length; k++)
			{
				mesh.materials[k].renderQueue = queue;
			}
		}
	}

	private void SetRenderQueue(GameObject gameObject, int queue)
	{
		if (!_meshCache.ContainsKey(gameObject))
		{
			MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
			List<MeshRenderer> list = new List<MeshRenderer>();
			foreach (MeshRenderer item in componentsInChildren)
			{
				list.Add(item);
			}
			_meshCache.Add(gameObject, list);
		}
		foreach (MeshRenderer item2 in _meshCache[gameObject])
		{
			SetRenderQueue(item2, queue);
		}
		if (_overridedObjects.ContainsKey(gameObject))
		{
			_overridedObjects.Remove(gameObject);
		}
		_overridedObjects.Add(gameObject, queue);
	}

	private void RestoreRenderQueue(SkinnedMeshRenderer skin)
	{
		if (_originalSkins.ContainsKey(skin))
		{
			List<Material> list = _originalSkins[skin];
			skin.material = list[0];
			for (int i = 1; i < skin.materials.Length; i++)
			{
				skin.materials[i] = list[i];
			}
			_originalSkins.Remove(skin);
		}
	}

	private void RestoreRenderQueue(MeshRenderer mesh)
	{
		if (_originalMeshes.ContainsKey(mesh))
		{
			List<Material> list = _originalMeshes[mesh];
			mesh.material = list[0];
			for (int i = 1; i < mesh.materials.Length; i++)
			{
				mesh.materials[i] = list[i];
			}
			_originalMeshes.Remove(mesh);
		}
	}

	private void RestoreRenderQueue(GameObject gameObject)
	{
		if (_overridedObjects.ContainsKey(gameObject))
		{
			_overridedObjects.Remove(gameObject);
		}
		foreach (MeshRenderer item in _meshCache[gameObject])
		{
			RestoreRenderQueue(item);
		}
		_meshCache.Remove(gameObject);
	}

	private void Awake()
	{
		SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		MeshRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (SkinnedMeshRenderer skin in componentsInChildren)
		{
			SetRenderQueue(skin, 1001);
		}
		foreach (MeshRenderer mesh in componentsInChildren2)
		{
			SetRenderQueue(mesh, 1001);
		}
		StartCoroutine("Run");
	}

	private void Update()
	{
	}

	private IEnumerator Run()
	{
		WaitForEndOfFrame Sleep = new WaitForEndOfFrame();
		SortedList<float, GameObject> nextOverridedObjects = new SortedList<float, GameObject>();
		List<GameObject> restorableObjects = new List<GameObject>();
		int coroutineFrames = 0;
		while (true)
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, OverrideableRadius, Const.LayerMaskHitByProjectile);
			nextOverridedObjects.Clear();
			foreach (Collider collider in colliders)
			{
				GameObject prospect = collider.gameObject;
				if (!nextOverridedObjects.ContainsValue(prospect))
				{
					AABB prospectAABB = GetObjectBounds(prospect);
					float distance;
					for (distance = (prospectAABB.Center() - transform.position).magnitude; nextOverridedObjects.ContainsKey(distance); distance += 0.001f)
					{
					}
					nextOverridedObjects.Add(distance, prospect);
				}
			}
			restorableObjects.Clear();
			foreach (KeyValuePair<GameObject, int> iterator in _overridedObjects)
			{
				if (!nextOverridedObjects.ContainsValue(iterator.Key) && iterator.Key != gameObject)
				{
					restorableObjects.Add(iterator.Key);
				}
			}
			float fps = 1f / Time.deltaTime;
			int numIterationsTillSleep = (int)((float)(restorableObjects.Count + nextOverridedObjects.Count) / fps);
			int numIterations = 0;
			foreach (GameObject iterator in restorableObjects)
			{
				RestoreRenderQueue(iterator);
				numIterations++;
				if (numIterations >= numIterationsTillSleep)
				{
					numIterations = 0;
					coroutineFrames++;
					yield return Sleep;
				}
			}
			int numOverridedObjects = nextOverridedObjects.Count;
			int queue = 2000 - numOverridedObjects;
			foreach (KeyValuePair<float, GameObject> iterator in nextOverridedObjects)
			{
				SetRenderQueue(iterator.Value, queue);
				queue++;
				numIterations++;
				if (numIterations >= numIterationsTillSleep)
				{
					numIterations = 0;
					coroutineFrames++;
					yield return Sleep;
				}
			}
		}
	}
}
