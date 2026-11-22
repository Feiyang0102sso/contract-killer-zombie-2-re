// PC port: turns on joint projection for every ragdoll joint.
// Explosions give each ragdoll body its own velocity (momentum / mass, Character.ExplosionAnimator), so light parts such as
// the head and hands get far faster than the hips. Unity 6 (PhysX 4) joints have projection off by default and only pull
// the bodies back together over several frames, so necks and limbs visibly stretch (measured up to 0.5 m and more).
// Projection snaps a body back once its joint error passes projectionDistance / projectionAngle (Unity defaults kept).
using UnityEngine;

public static class NewUnityRagdollJoints
{
	/// <summary>Enables projection on every CharacterJoint under root (inactive children included). Safe to call twice.</summary>
	public static void EnableProjection(GameObject root)
	{
		if (root == null)
		{
			return;
		}
		CharacterJoint[] joints = root.GetComponentsInChildren<CharacterJoint>(true);
		foreach (CharacterJoint joint in joints)
		{
			joint.enableProjection = true;
		}
	}
}
