using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dismemberment : MonoBehaviour
{
	public enum Mode
	{
		HideBone,
		HideBoneAndClonePrefab
	}

	public enum Placement
	{
		Simple,
		LimbToBone,
		LimbToRoot
	}

	public static float InvisibleScale = 0.005f;

	public static Vector3 InvisibleBoneScale = Vector3.one * InvisibleScale;

	public static Vector3 DismemberedJointScale = Vector3.one * 1f / InvisibleScale;

	public Placement PrefabPlacement;

	public GameObject HeadBone;

	public GameObject LeftHandBone;

	public GameObject LeftHandPrefab;

	public GameObject RightHandBone;

	public GameObject RightHandPrefab;

	public GameObject LeftLegBone;

	public GameObject LeftLegPrefab;

	public GameObject RightLegBone;

	public GameObject RightLegPrefab;

	public GameObject SpineBone;

	private Rigidbody _rootRigidbody;

	private Matrix4x4 _leftHandMeshToRoot = Matrix4x4.identity;

	private Matrix4x4 _rightHandMeshToRoot = Matrix4x4.identity;

	private Matrix4x4 _leftLegMeshToRoot = Matrix4x4.identity;

	private Matrix4x4 _rightLegMeshToRoot = Matrix4x4.identity;

	private Matrix4x4 _leftHandMeshToBone = Matrix4x4.identity;

	private Matrix4x4 _rightHandMeshToBone = Matrix4x4.identity;

	private Matrix4x4 _leftLegMeshToBone = Matrix4x4.identity;

	private Matrix4x4 _rightLegMeshToBone = Matrix4x4.identity;

	private float _lifetime;

	private Vector3[] _vels;

	private Rigidbody[] _limbs;

	private List<Transform> _hidingBones = new List<Transform>();

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		Quaternion result = new Quaternion();
		result.w = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] + m[1, 1] + m[2, 2])) / 2f;
		result.x = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] - m[1, 1] - m[2, 2])) / 2f;
		result.y = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] + m[1, 1] - m[2, 2])) / 2f;
		result.z = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] - m[1, 1] + m[2, 2])) / 2f;
		result.x *= Mathf.Sign(result.x * (m[2, 1] - m[1, 2]));
		result.y *= Mathf.Sign(result.y * (m[0, 2] - m[2, 0]));
		result.z *= Mathf.Sign(result.z * (m[1, 0] - m[0, 1]));
		return result;
	}

	private void DisableLimb(GameObject limb)
	{
		if ((bool)limb)
		{
			if ((bool)limb.GetComponent<CharacterJoint>())
			{
				Object.Destroy(limb.GetComponent<CharacterJoint>());
			}
			if ((bool)limb.GetComponent<Rigidbody>())
			{
				Object.Destroy(limb.GetComponent<Rigidbody>());
			}
			if ((bool)limb.GetComponent<SphereCollider>())
			{
				Object.Destroy(limb.GetComponent<SphereCollider>());
			}
			if ((bool)limb.GetComponent<BoxCollider>())
			{
				Object.Destroy(limb.GetComponent<BoxCollider>());
			}
			if ((bool)limb.GetComponent<CapsuleCollider>())
			{
				Object.Destroy(limb.GetComponent<CapsuleCollider>());
			}
			int childCount = limb.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DisableLimb(limb.transform.GetChild(i).gameObject);
			}
		}
	}

	private IEnumerator DismemberCoroutine(Object parameter)
	{
		yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
		Dismember(parameter as GameObject, Mode.HideBoneAndClonePrefab);
	}

	private void ReplaceTexture(GameObject obj)
	{
		SkinnedMeshRenderer componentInChildren = GetComponentInChildren<SkinnedMeshRenderer>();
		foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
		{
			renderer.material.mainTexture = componentInChildren.material.mainTexture;
		}
	}

	public void Dismember(GameObject bone)
	{
		StartCoroutine("DismemberCoroutine", bone);
	}

	public void DismemberInstantly(GameObject bone)
	{
		Dismember(bone, Mode.HideBoneAndClonePrefab);
	}

	public GameObject Dismember(GameObject bone, Mode mode)
	{
		GameObject result = null;
		if (!bone)
		{
			return result;
		}
		bool flag = false;
		if (bone == HeadBone)
		{
			if (mode == Mode.HideBoneAndClonePrefab)
			{
				flag = true;
				GameObject gameObject = Object.Instantiate(HeadBone) as GameObject;
				gameObject.transform.parent = Scene.Groups.Ragdolls;
				gameObject.transform.position = HeadBone.transform.position;
				gameObject.transform.eulerAngles = HeadBone.transform.eulerAngles;
				gameObject.GetComponent<Rigidbody>().linearVelocity = HeadBone.GetComponent<Rigidbody>().linearVelocity;
				gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Util.Range(0f, 1000f), Util.Range(0f, 1000f), Util.Range(0f, 1000f));
				gameObject.GetComponent<Rigidbody>().angularDamping = 5f;
				gameObject.GetComponent<Rigidbody>().linearDamping = 0.5f;
				Object.Destroy(gameObject.GetComponent<CharacterJoint>());
				result = gameObject;
				if ((bool)RagdollManager.instance)
				{
					RagdollManager.instance.Manage(gameObject);
				}
			}
			_hidingBones.Add(HeadBone.transform);
			// PC port: the hidden head keeps its rigidbody and collider, scaled to 0.005 by LateUpdate. PhysX 4 recomputes
			// the inertia from the shrunken collider, the near-zero inertia blows up the joint chain and flings or
			// stretches the corpse. Drop its physics like the hidden hands and legs (it is invisible anyway).
			DisableLimb(HeadBone);
			HeadBone = null;
		}
		else if (bone == SpineBone)
		{
			flag = true;
			Vector3 position = SpineBone.transform.position;
			float unusedSpineValue1;
			float unusedSpineValue2;
			if ((bool)LeftHandBone)
			{
				ApplyBlowImpulse(Dismember(LeftHandBone, Mode.HideBoneAndClonePrefab), position, 5f);
			}
			if ((bool)RightHandBone)
			{
				ApplyBlowImpulse(Dismember(RightHandBone, Mode.HideBoneAndClonePrefab), position, 5f);
			}
			if ((bool)HeadBone)
			{
				ApplyBlowImpulse(Dismember(HeadBone, Mode.HideBoneAndClonePrefab), position, 5f);
			}
			RagdollManager.instance.ShowDismemberedJoint(SpineBone.transform, Vector3.forward, RagdollManager.LimbType.Spine);
			_hidingBones.Add(SpineBone.transform);
			// PC port: the original nulls SpineBone before DisableLimb, so the hidden chest/head keep their physics at
			// scale 0.005 (and 0.000025) and PhysX 4's near-zero inertia flings the hips and legs. Disable first.
			DisableLimb(SpineBone);
			SpineBone = null;
			Rigidbody rootRigidBody = RagdollManager.GetRootRigidBody(base.gameObject);
			if ((bool)rootRigidBody)
			{
				Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>(rootRigidBody);
				foreach (Rigidbody rigidbody in componentsInChildren)
				{
					if (!rigidbody.isKinematic)
					{
						rigidbody.linearVelocity = Vector3.zero;
					}
				}
				FixedJoint obj = rootRigidBody.gameObject.AddComponent<FixedJoint>();
				Object.Destroy(obj, 0.4f);
				if ((bool)Scene.EffectsManager)
				{
					Scene.EffectsManager.DrawHeadshot(rootRigidBody.transform.position);
				}
			}
			SoundManager.Play("skill_shot_2", 2000, false);
		}
		else if (bone == LeftHandBone)
		{
			if (mode == Mode.HideBoneAndClonePrefab)
			{
				flag = true;
				GameObject gameObject2 = Object.Instantiate(LeftHandPrefab) as GameObject;
				ReplaceTexture(gameObject2);
				gameObject2.transform.parent = Scene.Groups.Ragdolls;
				gameObject2.GetComponent<Rigidbody>().linearVelocity = LeftHandBone.GetComponent<Rigidbody>().linearVelocity;
				gameObject2.transform.position = LeftHandBone.transform.position;
				gameObject2.transform.eulerAngles = LeftHandBone.transform.eulerAngles;
				result = gameObject2;
				if ((bool)RagdollManager.instance)
				{
					RagdollManager.instance.Manage(gameObject2);
				}
			}
			RagdollManager.instance.ShowDismemberedJoint(LeftHandBone.transform, Vector3.forward, RagdollManager.LimbType.Hand);
			_hidingBones.Add(LeftHandBone.transform);
			LeftHandBone = null;
			DisableLimb(bone);
		}
		else if (bone == RightHandBone)
		{
			if (mode == Mode.HideBoneAndClonePrefab)
			{
				flag = true;
				GameObject gameObject3 = Object.Instantiate(RightHandPrefab) as GameObject;
				ReplaceTexture(gameObject3);
				gameObject3.transform.parent = Scene.Groups.Ragdolls;
				gameObject3.GetComponent<Rigidbody>().linearVelocity = RightHandBone.GetComponent<Rigidbody>().linearVelocity;
				gameObject3.transform.position = RightHandBone.transform.position;
				gameObject3.transform.eulerAngles = RightHandBone.transform.eulerAngles;
				result = gameObject3;
				if ((bool)RagdollManager.instance)
				{
					RagdollManager.instance.Manage(gameObject3);
				}
			}
			RagdollManager.instance.ShowDismemberedJoint(RightHandBone.transform, -Vector3.forward, RagdollManager.LimbType.Hand);
			_hidingBones.Add(RightHandBone.transform);
			RightHandBone = null;
			DisableLimb(bone);
		}
		else if (bone == LeftLegBone)
		{
			if (mode == Mode.HideBoneAndClonePrefab)
			{
				flag = true;
				GameObject gameObject4 = Object.Instantiate(LeftLegPrefab) as GameObject;
				ReplaceTexture(gameObject4);
				gameObject4.transform.parent = Scene.Groups.Ragdolls;
				gameObject4.GetComponent<Rigidbody>().linearVelocity = LeftLegBone.GetComponent<Rigidbody>().linearVelocity;
				gameObject4.transform.position = LeftLegBone.transform.position;
				gameObject4.transform.eulerAngles = LeftLegBone.transform.eulerAngles;
				result = gameObject4;
				if ((bool)RagdollManager.instance)
				{
					RagdollManager.instance.Manage(gameObject4);
				}
			}
			RagdollManager.instance.ShowDismemberedJoint(LeftLegBone.transform, Vector3.right, RagdollManager.LimbType.Leg);
			_hidingBones.Add(LeftLegBone.transform);
			LeftLegBone = null;
			DisableLimb(bone);
		}
		else if (bone == RightLegBone)
		{
			if (mode == Mode.HideBoneAndClonePrefab)
			{
				flag = true;
				GameObject gameObject5 = Object.Instantiate(RightLegPrefab) as GameObject;
				ReplaceTexture(gameObject5);
				gameObject5.transform.parent = Scene.Groups.Ragdolls;
				gameObject5.GetComponent<Rigidbody>().linearVelocity = RightLegBone.GetComponent<Rigidbody>().linearVelocity;
				gameObject5.transform.position = RightLegBone.transform.position;
				gameObject5.transform.eulerAngles = RightLegBone.transform.eulerAngles;
				result = gameObject5;
				if ((bool)RagdollManager.instance)
				{
					RagdollManager.instance.Manage(gameObject5);
				}
			}
			RagdollManager.instance.ShowDismemberedJoint(RightLegBone.transform, Vector3.right, RagdollManager.LimbType.Leg);
			_hidingBones.Add(RightLegBone.transform);
			RightLegBone = null;
			DisableLimb(bone);
		}
		if (flag)
		{
			Vector3 vector = bone.transform.position;
			CharacterJoint component = bone.gameObject.GetComponent<CharacterJoint>();
			if ((bool)component)
			{
				vector = (vector + component.connectedBody.transform.position) / 2f;
			}
			Scene.EffectsManager.DrawBloodSplatter(vector);
		}
		return result;
	}

	private void ApplyBlowImpulse(GameObject limb, Vector3 epicenter, float massRelativeImpulse)
	{
		if ((bool)limb && (bool)limb.GetComponent<Rigidbody>())
		{
			massRelativeImpulse = Random.Range(massRelativeImpulse * 0.5f, massRelativeImpulse);
			Renderer component = limb.GetComponent<Renderer>();
			component = component ? component : limb.GetComponentInChildren<Renderer>();
			Vector3 vector = Vector3.zero;
			if ((bool)component)
			{
				Vector3 extents = component.bounds.extents;
				vector.x = Random.Range(-extents.x, extents.x);
				vector.y = Random.Range(-extents.y, extents.y);
				vector.z = Random.Range(-extents.z, extents.z);
				vector = limb.transform.TransformDirection(vector);
			}
			Vector3 force = (limb.transform.position - epicenter).normalized * massRelativeImpulse * limb.GetComponent<Rigidbody>().mass;
			limb.GetComponent<Rigidbody>().AddForceAtPosition(force, limb.transform.position + vector, ForceMode.Impulse);
		}
	}

	private void Awake()
	{
		_limbs = GetComponentsInChildren<Rigidbody>();
		if (PrefabPlacement != Placement.Simple)
		{
			_rootRigidbody = RagdollManager.GetRootRigidBody(gameObject);
			_leftHandMeshToRoot = _rootRigidbody.transform.worldToLocalMatrix * LeftHandBone.transform.localToWorldMatrix;
			_leftLegMeshToRoot = _rootRigidbody.transform.worldToLocalMatrix * LeftLegBone.transform.localToWorldMatrix;
			_rightHandMeshToRoot = _rootRigidbody.transform.worldToLocalMatrix * RightHandBone.transform.localToWorldMatrix;
			_rightLegMeshToRoot = _rootRigidbody.transform.worldToLocalMatrix * RightLegBone.transform.localToWorldMatrix;
			_leftHandMeshToBone = _leftHandMeshToRoot;
			_leftLegMeshToBone = _leftLegMeshToRoot;
			_rightHandMeshToBone = _rightHandMeshToRoot;
			_rightLegMeshToBone = _rightLegMeshToRoot;
		}
	}

	private void Start()
	{
		if ((bool)RagdollManager.instance)
		{
			RagdollManager.instance.Manage(gameObject);
		}
	}

	private void FixedUpdate()
	{
		_lifetime += Time.fixedDeltaTime;
		float unusedLocal1;
		float unusedLocal2;
		if (TimeControl.Scale <= 0.5f && _lifetime >= 0.5f)
		{
			if (_vels == null)
			{
				_vels = new Vector3[_limbs.Length];
				for (int i = 0; i < _limbs.Length; i++)
				{
					if ((bool)_limbs[i] && !_limbs[i].isKinematic)
					{
						_vels[i] = _limbs[i].linearVelocity;
					}
				}
			}
			for (int j = 0; j < _limbs.Length; j++)
			{
				if ((bool)_limbs[j] && !_limbs[j].isKinematic && !_limbs[j].IsSleeping())
				{
					Vector3 velocity = _limbs[j].linearVelocity;
					if (velocity.sqrMagnitude > _vels[j].sqrMagnitude)
					{
						_limbs[j].linearVelocity = velocity.normalized * _vels[j].magnitude;
					}
					else
					{
						_vels[j] = velocity;
					}
				}
			}
		}
	}

	private void LateUpdate()
	{
		if (!(_hidingBones.Count <= 0))
		{
			foreach (Transform hidingBone in _hidingBones)
			{
				hidingBone.localScale = InvisibleBoneScale;
			}
		}
	}
}
