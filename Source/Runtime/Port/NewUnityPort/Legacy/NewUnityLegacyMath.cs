// PC port: Unity 3.5 math where newer Unity changed the result.
// Vector3.Angle: 3.5 computed Acos(Clamp(Dot(from.normalized, to.normalized), -1, 1)), so a zero vector gave 90 degrees.
// Unity 2017+ returns 0 for a zero vector. Spitter's spit damage loops over all characters, itself included, and relied on
// the 90 degrees to skip itself (to = 0); with 0 degrees it hit itself every 0.25 s while spitting and died after a spit.
using UnityEngine;

public static class NewUnityLegacyMath
{
	/// <summary>Vector3.Angle as in Unity 3.5 (90 degrees when either vector is zero).</summary>
	public static float Angle(Vector3 from, Vector3 to)
	{
		float dot = Vector3.Dot(from.normalized, to.normalized);
		return Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
	}
}
