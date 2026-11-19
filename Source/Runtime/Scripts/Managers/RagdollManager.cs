using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviourAndLog
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
			inf = bounds.center - bounds.extents / 2f;
			sup = bounds.center + bounds.extents / 2f;
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
	}

	private struct ManagedData
	{
		public bool managed;

		public Transform transform;

		public float velocity;

		public float distance;

		public float prevDistance;
	}

	public enum LimbType
	{
		Hand,
		Leg,
		Spine
	}

	public float DissolveAcceleration = 0.01f;

	public int MaxNumberOfManagedObjects = 64;

	public GameObject DismemberedJointModel;

	public GameObject DismemberedSpineJointModel;

	public static RagdollManager instance = null;

	private ManagedData[] _managedData;

	private int _nextManagedDataID;

	private int _numMamagedObjects;

	private int _numObservedRigidbodies;

	private int _numObservedRadgolls;

	private int _numRagdollsSpawnedThisFrame;

	private int _maxRagdollsSpawnedPerFrame = Const.MaxRagdollsSpawnedPerFrame;

	private int _maxRagdollsOnScene = Const.MaxRagdollsOnScene;

	private bool _preinstanceLock;

	private List<GameObject> _requested = new List<GameObject>();

	private Dictionary<GameObject, GameObject> _preinstanced = new Dictionary<GameObject, GameObject>();

	private void Awake()
	{
		instance = this;
		LogTo("Other.RagdollManager");
		_managedData = new ManagedData[MaxNumberOfManagedObjects];
		for (int i = 0; i < MaxNumberOfManagedObjects; i++)
		{
			_managedData[i].managed = false;
			_managedData[i].transform = null;
			_managedData[i].velocity = 0f;
			_managedData[i].distance = 0f;
			_managedData[i].prevDistance = 0f;
		}
		Singleton.GameEvents.OnFPSDecreased += new Action<int>(OnFPSDecreased);
		Singleton.GameEvents.OnFPSIncreased += new Action<int>(OnFPSIncreased);
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnFPSDecreased -= new Action<int>(OnFPSDecreased);
		}
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnFPSIncreased -= new Action<int>(OnFPSIncreased);
		}
		instance = null;
	}

	private void Update()
	{
		float unusedLocal1;
		if (_numMamagedObjects > 0)
		{
			for (int i = 0; i < MaxNumberOfManagedObjects; i++)
			{
				bool flag = false;
				if (!_managedData[i].managed)
				{
					continue;
				}
				if (_managedData[i].transform == null)
				{
					flag = true;
				}
				else
				{
					_managedData[i].velocity += DissolveAcceleration * Time.deltaTime;
					_managedData[i].distance -= _managedData[i].velocity * Time.deltaTime;
					if (_managedData[i].prevDistance - _managedData[i].distance >= 0.0025f)
					{
						_managedData[i].transform.position += Vector3.down * (_managedData[i].prevDistance - _managedData[i].distance);
						_managedData[i].prevDistance = _managedData[i].distance;
					}
					if (_managedData[i].distance <= 0f)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (_managedData[i].transform != null)
					{
						UnityEngine.Object.Destroy(_managedData[i].transform.gameObject);
					}
					_managedData[i].managed = false;
					_managedData[i].transform = null;
					_managedData[i].velocity = 0f;
					_managedData[i].distance = 0f;
					_managedData[i].prevDistance = 0f;
					_numMamagedObjects--;
				}
			}
		}
		_numRagdollsSpawnedThisFrame = 0;
	}

	private void OnFPSDecreased(int fps)
	{
		if (fps < Const.MinEffectsFPS)
		{
			_maxRagdollsSpawnedPerFrame = 0;
			_maxRagdollsOnScene = 0;
		}
		else if (fps < Const.LiteEffectsFPS)
		{
			_maxRagdollsSpawnedPerFrame = Const.MaxRagdollsSpawnedPerFrame / 2;
			_maxRagdollsOnScene = Const.MaxRagdollsOnScene / 2;
		}
	}

	private void OnFPSIncreased(int fps)
	{
		if (fps > Const.FullEffectsFPS)
		{
			_maxRagdollsSpawnedPerFrame = Const.MaxRagdollsSpawnedPerFrame;
			_maxRagdollsOnScene = Const.MaxRagdollsOnScene;
		}
		else if (fps > Const.LiteEffectsFPS)
		{
			_maxRagdollsSpawnedPerFrame = Const.MaxRagdollsSpawnedPerFrame / 2;
			_maxRagdollsOnScene = Const.MaxRagdollsOnScene / 2;
		}
	}

	private AABB ComputeAABB(GameObject gameObject)
	{
		AABB aABB = null;
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		SkinnedMeshRenderer component2 = gameObject.GetComponent<SkinnedMeshRenderer>();
		if ((bool)component)
		{
			aABB = new AABB(component.bounds);
		}
		else if ((bool)component2)
		{
			aABB = new AABB(component2.bounds);
		}
		int childCount = gameObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			AABB aABB2 = ComputeAABB(gameObject.transform.GetChild(i).gameObject);
			if (aABB != null && aABB2 != null)
			{
				aABB.Add(aABB2);
			}
			else if (aABB2 != null)
			{
				aABB = aABB2;
			}
		}
		return aABB;
	}

	private void FreezeLimb(GameObject limb)
	{
		if ((bool)limb.GetComponent<Collider>())
		{
			UnityEngine.Object.Destroy(limb.GetComponent<Collider>());
		}
		if ((bool)limb.GetComponent<CharacterJoint>())
		{
			UnityEngine.Object.Destroy(limb.GetComponent<CharacterJoint>());
		}
		if ((bool)limb.GetComponent<Rigidbody>())
		{
			UnityEngine.Object.Destroy(limb.GetComponent<Rigidbody>());
		}
		int childCount = limb.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			FreezeLimb(limb.transform.GetChild(i).gameObject);
		}
	}

	public static bool IsRagdoll(GameObject gameObject)
	{
		return gameObject.GetComponentsInChildren<Rigidbody>().Length > 1;
	}

	public static float GetHierarchyMass(GameObject rootGameObject)
	{
		float num = (!(!rootGameObject.GetComponent<Rigidbody>()) ? rootGameObject.GetComponent<Rigidbody>().mass : 0f);
		for (int i = 0; i < rootGameObject.transform.childCount; i++)
		{
			num += GetHierarchyMass(rootGameObject.transform.GetChild(i).gameObject);
		}
		return num;
	}

	public static Rigidbody GetRootRigidBody(GameObject gameObject)
	{
		if ((bool)gameObject.GetComponent<Rigidbody>())
		{
			return gameObject.GetComponent<Rigidbody>();
		}
		int childCount = gameObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Rigidbody rootRigidBody = GetRootRigidBody(gameObject.transform.GetChild(i).gameObject);
			if ((bool)rootRigidBody)
			{
				return rootRigidBody;
			}
		}
		return null;
	}

	private void CharacterObserver(Character observedCharacter)
	{
		if (observedCharacter is Bomber)
		{
			UnityEngine.Object.Destroy(observedCharacter.gameObject);
			return;
		}
		float unusedSleepValue1;
		RaycastHit hitInfo;
		GameObject gameObject = Util.FindObjectWithTagInHierarchy(observedCharacter.gameObject, "HipBone");
		float unusedSleepValue2;
		if ((bool)gameObject && Physics.Raycast(gameObject.transform.position + Vector3.up * 0.1f, Vector3.down, out hitInfo, 2f, Const.LayerMaskObstacles))
		{
			EffectsManager.instance.OnRagdollSleep(hitInfo.point + hitInfo.normal * 0.01f);
		}
		UnityEngine.Object.Destroy(observedCharacter);
		UnityEngine.Object.Destroy(observedCharacter.GetComponent<CharacterController>());
		UnityEngine.Object.Destroy(observedCharacter.GetComponent<UnityEngine.AI.NavMeshAgent>());
		UnityEngine.Object.Destroy(observedCharacter.GetComponent<Animation>());
		ManageAndDestroy(observedCharacter.gameObject);
	}

	private IEnumerator SleepingStateObserver(UnityEngine.Object parameter)
	{
		float unusedLocal1;
		float unusedLocal2;
		float unusedLocal3;
		GameObject observedObject = parameter as GameObject;
		bool isRagdoll = IsRagdoll(observedObject);
		if (isRagdoll)
		{
			_numObservedRadgolls++;
		}
		else
		{
			_numObservedRigidbodies++;
		}
		Rigidbody observedBody = GetRootRigidBody(observedObject);
		float observationTime = 0f;
		float yieldTime = UnityEngine.Random.Range(0.5f, 1f);
		yield return new WaitForSeconds(yieldTime);
		observationTime += yieldTime;
		while (true)
		{
			while (observedObject && observedBody)
			{
				if (observationTime >= 10f)
				{
					UnityEngine.Object.Destroy(observedObject);
					if (isRagdoll)
					{
						_numObservedRadgolls--;
					}
					else
					{
						_numObservedRigidbodies--;
					}
					yield break;
				}
				if ((observedBody.IsSleeping() || observedBody.linearVelocity.magnitude < 0.05f) && observedBody.GetComponent<FixedJoint>() == null)
				{
					FreezeLimb(observedObject);
					RaycastHit hit;
					if ((bool)EffectsManager.instance && ((bool)observedObject.GetComponent<Dismemberment>() || (bool)observedObject.GetComponent<Disappearance>()) && Physics.Raycast(observedBody.GetComponent<Collider>().bounds.center + Vector3.up * 0.1f, Vector3.down, out hit, 2f, Const.LayerMaskObstacles))
					{
						EffectsManager.instance.OnRagdollSleep(hit.point + hit.normal * 0.01f);
					}
					ManageAndDestroy(observedObject);
					if (isRagdoll)
					{
						_numObservedRadgolls--;
					}
					else
					{
						_numObservedRigidbodies--;
					}
					yield break;
				}
				yieldTime = UnityEngine.Random.Range(0.25f, 0.5f);
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 0.5f));
				observationTime += yieldTime;
			}
			if (!observedObject)
			{
				break;
			}
			if (!observedBody)
			{
				observedBody = GetRootRigidBody(observedObject);
			}
			if (!observedBody)
			{
				break;
			}
		}
		yield break;
	}

	private void ActivatePreinstancedObject(GameObject obj, bool flag)
	{
		if (flag)
		{
			obj.SetActiveRecursively(flag);
		}
		Rigidbody[] componentsInChildren = obj.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = !flag;
		}
		SkinnedMeshRenderer[] componentsInChildren2 = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
		{
			skinnedMeshRenderer.enabled = flag;
		}
		MeshRenderer[] componentsInChildren3 = obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren3)
		{
			meshRenderer.enabled = flag;
		}
		if ((bool)obj.GetComponent<Dismemberment>())
		{
			obj.GetComponent<Dismemberment>().enabled = flag;
		}
		if (!flag)
		{
			obj.SetActiveRecursively(flag);
		}
	}

	private IEnumerator Preinstance()
	{
		_preinstanceLock = true;
		WaitForSeconds wait1000 = new WaitForSeconds(1f);
		WaitForSeconds wait100 = new WaitForSeconds(0.1f);
		WaitForSeconds wait = wait1000;
		while (_requested.Count > 0)
		{
			List<GameObject> deferred = new List<GameObject>();
			while (_requested.Count > 0)
			{
				yield return wait;
				if (!_preinstanced.ContainsKey(_requested[0]))
				{
					GameObject obj = UnityEngine.Object.Instantiate(_requested[0]) as GameObject;
					obj.SetParent(Util.FindOrCreate("/RagdollsCache"));
					ActivatePreinstancedObject(obj, false);
					_preinstanced.Add(_requested[0], obj);
					if (obj.GetComponent<Dismemberment>() || obj.GetComponent<Disappearance>())
					{
						wait = wait1000;
					}
					else
					{
						wait = wait100;
					}
				}
				else
				{
					deferred.Add(_requested[0]);
				}
				_requested.RemoveAt(0);
			}
			_requested = deferred;
		}
		_preinstanceLock = false;
		yield break;
	}

	public void ManageAndDestroy(GameObject gameObject)
	{
		if (_numMamagedObjects >= MaxNumberOfManagedObjects)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		AABB aABB = ComputeAABB(gameObject);
		if (aABB != null)
		{
			Vector3[] array = new Vector3[4]
			{
				aABB.inf + aABB.Height(),
				aABB.inf + aABB.Width() + aABB.Height(),
				aABB.inf + aABB.Depth() + aABB.Height(),
				aABB.inf + aABB.Width() + aABB.Depth() + aABB.Height()
			};
			float num = 0f;
			float unusedLocal1;
			foreach (Vector3 origin in array)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(origin, Vector3.down, out hitInfo, 10f, Const.LayerMaskObstacles))
				{
					num = Mathf.Max(num, hitInfo.distance);
				}
			}
			int num2 = 0;
			while ((bool)_managedData[_nextManagedDataID].transform)
			{
				_nextManagedDataID++;
				if (_nextManagedDataID >= MaxNumberOfManagedObjects)
				{
					_nextManagedDataID = 0;
				}
				num2++;
				if (num2 >= MaxNumberOfManagedObjects)
				{
					break;
				}
			}
			if (num2 < MaxNumberOfManagedObjects)
			{
				_managedData[_nextManagedDataID].managed = true;
				_managedData[_nextManagedDataID].transform = gameObject.transform;
				_managedData[_nextManagedDataID].velocity = 0f;
				_managedData[_nextManagedDataID].distance = num;
				_managedData[_nextManagedDataID].prevDistance = num;
				_nextManagedDataID++;
				if (_nextManagedDataID >= MaxNumberOfManagedObjects)
				{
					_nextManagedDataID = 0;
				}
				_numMamagedObjects++;
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
				return;
			}
		}
	}

	public void Preinstance(GameObject prefab)
	{
		_requested.Add(prefab);
		if (!_preinstanceLock)
		{
			StartCoroutine(Preinstance());
		}
	}

	public GameObject Instantiate(GameObject prefab)
	{
		GameObject value = null;
		if (_preinstanced.TryGetValue(prefab, out value))
		{
			ActivatePreinstancedObject(value, true);
			int num = _requested.FindIndex((GameObject x) => x == prefab);
			if (num != -1)
			{
				_requested.RemoveAt(num);
				_requested.Insert(0, prefab);
			}
			_preinstanced.Remove(prefab);
			return value;
		}
		return UnityEngine.Object.Instantiate(prefab) as GameObject;
	}

	public void Manage(GameObject gameObject)
	{
		if (IsRagdoll(gameObject))
		{
			_numRagdollsSpawnedThisFrame++;
		}
		StartCoroutine("SleepingStateObserver", gameObject);
	}

	public void ManageCharacter(Character character)
	{
		CharacterObserver(character);
	}

	public void ShowDismemberedJoint(Transform boneTransform, Vector3 axis, LimbType limbType)
	{
		float unusedOffset1;
		float unusedOffset2;
		float unusedOffset3;
		float unusedOffset4;
		if ((bool)DismemberedJointModel)
		{
			GameObject gameObject = null;
			if (limbType == LimbType.Spine)
			{
				gameObject = UnityEngine.Object.Instantiate(instance.DismemberedSpineJointModel) as GameObject;
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate(instance.DismemberedJointModel) as GameObject;
			}
			gameObject.transform.position = boneTransform.position;
			gameObject.transform.rotation = boneTransform.rotation;
			gameObject.transform.position += axis.x * gameObject.transform.right * 0.01f;
			gameObject.transform.position += axis.y * gameObject.transform.up * 0.01f;
			gameObject.transform.position += axis.z * gameObject.transform.forward * 0.01f;
			switch (limbType)
			{
			case LimbType.Hand:
				gameObject.transform.localScale = Vector3.one * 0.6f;
				break;
			case LimbType.Leg:
				gameObject.transform.localScale = Vector3.one * 1f;
				break;
			case LimbType.Spine:
				gameObject.transform.localScale = Vector3.one * 1f;
				break;
			}
			gameObject.transform.parent = boneTransform.parent;
			if (limbType == LimbType.Spine)
			{
				gameObject.transform.RotateAround(gameObject.transform.right, 90f);
				gameObject.transform.RotateAround(gameObject.transform.up, 90f);
			}
			gameObject.GetComponent<Renderer>().useLightProbes = true;
		}
	}

	public int NumObservedRagdolls
	{
		get
		{
			return _numObservedRadgolls;
		}
	}

	public int NumObservedRigidbodies
	{
		get
		{
			return _numObservedRigidbodies;
		}
	}

	public bool CanSpawnRagdoll
	{
		get
		{
			return _numRagdollsSpawnedThisFrame < _maxRagdollsSpawnedPerFrame && _numObservedRadgolls < _maxRagdollsOnScene;
		}
	}
}
