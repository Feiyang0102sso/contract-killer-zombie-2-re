using System;
using System.Collections.Generic;
using UnityEngine;

public class Goo : MonoBehaviour
{
	private struct LastGoo
	{
		public float time;

		public Vector3 place;
	}

	private const int lastGooLimit = 10;

	[NonSerialized]
	public Character source;

	private static Queue<LastGoo> lastGoo = new Queue<LastGoo>();

	private void OnCollisionEnter(Collision collision)
	{
		Hit(collision);
	}

	public void Hit(Collision collision)
	{
		UnityEngine.Object.Destroy(base.gameObject);
		SoundManager.PlayExclusive("Zombie/Spitter/spitter_acid_1", transform.position, 5f, 25f, false);
		ContactPoint contactPoint = collision.contacts[0];
		Vector3 vector = contactPoint.normal;
		if (Vector3.Dot(vector, Vector3.up) < 0f)
		{
			vector = -vector;
		}
		if (Mathf.Abs(Vector3.Angle(vector, Vector3.up)) > 10f)
		{
			return;
		}
		if (source != null && Mathf.Abs(source.transform.position.y - contactPoint.point.y) > 0.1f)
		{
			return;
		}
		foreach (LastGoo item in lastGoo)
		{
			if (Time.time - item.time < 5f && Vector3.Distance(contactPoint.point, item.place) < 2.5f)
			{
				return;
			}
		}
		lastGoo.Enqueue(new LastGoo
		{
			time = Time.time,
			place = contactPoint.point
		});
		while (lastGoo.Count > 10)
		{
			lastGoo.Dequeue();
		}
		GameObject gameObject = Scene.EffectsManager.DrawSurfaceGoo(contactPoint.point + Vector3.up * 0.05f);
		GooDamager gooDamager = gameObject.AddComponent<GooDamager>();
		gooDamager.source = source;
	}
}
