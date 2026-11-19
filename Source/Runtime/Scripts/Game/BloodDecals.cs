using System;
using UnityEngine;

public class BloodDecals : MonoBehaviour
{
	private MeshRenderer[] _bloodDecals;

	private void Awake()
	{
		_bloodDecals = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < _bloodDecals.Length; i++)
		{
			_bloodDecals[i].gameObject.SetActiveRecursively(false);
		}
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnEnemyKill += new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
		}
	}

	private void OnDestroy()
	{
		if (Singleton.Exists())
		{
			Singleton.GameEvents.OnEnemyKill -= new Action<Character, Unit, Weapon, Consumable>(OnEnemyKill);
		}
	}

	private void Update()
	{
	}

	private void OnEnemyKill(Character character, Unit unit, Weapon weapon, Consumable consumable)
	{
		if (!unit)
		{
			return;
		}
		float num = float.MaxValue;
		MeshRenderer meshRenderer = null;
		for (int i = 0; i < _bloodDecals.Length; i++)
		{
			float magnitude = (_bloodDecals[i].transform.position - unit.transform.position).magnitude;
			if (magnitude < num && !_bloodDecals[i].gameObject.active)
			{
				num = magnitude;
				meshRenderer = _bloodDecals[i];
			}
		}
		if ((bool)meshRenderer)
		{
			meshRenderer.gameObject.SetActiveRecursively(true);
		}
	}
}
