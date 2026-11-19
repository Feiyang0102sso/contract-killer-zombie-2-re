using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponDamager : MonoBehaviourAndLog
{
	public MeleeWeapon Weapon;

	private Vector3 _velocity;

	private Vector3 _prevPos;

	private BoxCollider _boxCollider;

	private Character _lastDamagedCharacter;

	private List<KeyValuePair<Collider, float>> _lacerationDamageTime = new List<KeyValuePair<Collider, float>>();

	private void Awake()
	{
		LogTo("Char.Human.Melee");
	}

	private void Start()
	{
		_boxCollider = GetComponent<BoxCollider>();
	}

	private void OnTriggerEnter(Collider collider)
	{
		float unusedTriggerTime;
		if (!Weapon.HasLoopMeleeAnimation)
		{
			Character character = collider.gameObject.FindComponentUpwards<Character>();
			if (character && !Util.DoRarely(character, "ON_MELEE_ENTER", 0.33f))
			{
				return;
			}
			Damage(collider, 1f);
		}
		else
		{
			_lacerationDamageTime.Add(new KeyValuePair<Collider, float>(collider, Time.time));
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!(!Weapon.HasLoopMeleeAnimation))
		{
			for (int i = 0; i < _lacerationDamageTime.Count; i++)
			{
				if (_lacerationDamageTime[i].Key == collider)
				{
					Damage(collider, Time.time - _lacerationDamageTime[i].Value);
					_lacerationDamageTime.RemoveAt(i);
				}
			}
		}
	}

	private void Damage(Collider collider, float rate)
	{
		if (!Weapon.IsPerformingAttack)
		{
			return;
		}
		GameObject gameObject = collider.gameObject;
		Character character = gameObject.FindComponentUpwards<Character>();
		if (Weapon.Owner == character)
		{
			return;
		}
		HitInfo info = new HitInfo(Weapon.Damage * rate, Weapon.DamageType, Weapon.Owner, Util.Chance(30f) ? Scene.MainCamera.transform.forward * _velocity.magnitude * Weapon.Mass : _velocity * Weapon.Mass, Weapon.DefragProbability, Weapon.MutilationProbability, Weapon.PushbackChance, Weapon.CriticalMultiplier, _boxCollider.bounds.center, gameObject, 100f);
		if ((bool)character && character != _lastDamagedCharacter)
		{
			_lastDamagedCharacter = character;
			Vector3 point = (collider.bounds.center + _boxCollider.bounds.center) * 0.5f;
			Scene.EffectsManager.OnHit(collider, point, -_velocity.normalized, _velocity, false, character != null, info);
		}
		if ((bool)character)
		{
			character.OnDamage(info);
		}
		if (Util.DoRarely(this, "emit_sound", 1f))
		{
			Vector3 position = transform.position;
			float unusedHearingRange;
			foreach (Unit item in Scene.Nearby<Unit>(position, 30f))
			{
				item.OnHearShotSound(position);
			}
		}
	}

	private void ApplyAccumulatedDamage()
	{
		float unusedLocal1;
		for (int num = _lacerationDamageTime.Count - 1; num >= 0; num--)
		{
			if ((bool)_lacerationDamageTime[num].Key)
			{
				if (Time.time - _lacerationDamageTime[num].Value >= 0.25f)
				{
					Damage(_lacerationDamageTime[num].Key, Time.time - _lacerationDamageTime[num].Value);
					_lacerationDamageTime.Add(new KeyValuePair<Collider, float>(_lacerationDamageTime[num].Key, Time.time));
					_lacerationDamageTime.RemoveAt(num);
				}
			}
			else
			{
				_lacerationDamageTime.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		Vector3 position = transform.position;
		_velocity = (position - _prevPos) / Time.deltaTime;
		_prevPos = position;
		if (Weapon.HasLoopMeleeAnimation)
		{
			ApplyAccumulatedDamage();
		}
	}

	public void OnAttack()
	{
		_lastDamagedCharacter = null;
	}
}
