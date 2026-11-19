using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class Player : Human
{
	private struct AimingTarget
	{
		public GameObject target;

		public float dt;

		public AimingTarget(GameObject _target, float _dt)
		{
			target = _target;
			dt = _dt;
		}
	}

	public const float fireDelaySec = 0.01f;

	public const float ZoomModeStopCoeff = 0.7f;

	public float fireStateSpeedCoeff = 0.5f;

	public float runningSpeedCoeff = 2f;

	public float ZoomModeSpeedCoeff = 0.1f;

	public float speedDamping = 1f;

	public bool strafeInAimStateOnly = true;

	public GameObject fireSourceObject;

	public float fireStateDelaySec = 0.4f;

	public float AutoRotationDelay = 0.5f;

	public float AutoRotationDamping = 10f;

	public float CharacterFreeRunRotationDamping = 1f;

	public string[] footStepSound;

	public GameObject groundTrigger;

	public GameObject leftFootCollider;

	public GameObject rightFootCollider;

	[HideInInspector]
	public bool IgnoreUserInput;

	private HudController hud;

	private float _speedCoeff = 1f;

	private bool _isVictimLocked;

	private float _fireDelayCountDown;

	private bool _zoomMode;

	private bool _aimMode;

	private float _stillStayTime;

	public Unit MeleeTarget;

	private Vector3 _currentMissionTargetCachedPos;

	private GameObject _subTaskDummy;

	private NavMeshPath _pathToTask;

	private int _currentPathCorner = -1;

	private float _pathLength;

	private bool _isMissionTargetVisible;

	private Weapon _lastLoggedOutOfAmmoWeapon;

	private List<AimingTarget> _aimingHistory = new List<AimingTarget>();

	private float _lastTargetDetectionTime;

	private float _lastTargetDetectionDistance;

	private Vector3 _lastTargetDetectionPoint = Vector3.zero;

	private Vector3 _lastTargetDetectionObjectPos = Vector3.zero;

	private GameObject _lastTargetDetectionObject;

	public bool IsVictimLocked
	{
		get
		{
			return _isVictimLocked;
		}
	}

	public float GetFireDelayCountDownPercent()
	{
		return _fireDelayCountDown / 0.01f;
	}

	public bool IsZoomMode()
	{
		return _zoomMode;
	}

	public float GetRelativeSpeed()
	{
		return speed / baseSpeed;
	}

	public int Level
	{
		get
		{
			return Singleton.PlayerData.Level;
		}
	}

	public Transform GetCurrentMissionTarget()
	{
		if ((bool)Scene.Mission && (bool)Scene.Mission.GetCurrentTask() && (bool)Scene.Mission.GetCurrentTask().GetTarget())
		{
			if (Scene.Mission.GetCurrentTask().IsTargetDynamic())
			{
				return Scene.Mission.GetCurrentTask().GetTarget().transform;
			}
			return _subTaskDummy.transform;
		}
		return null;
	}

	public float GetMissionTargetPathLength()
	{
		return _pathLength;
	}

	public bool IsMissionTargetVisible()
	{
		return _isMissionTargetVisible;
	}

	protected override void Awake()
	{
		base.Awake();
		LogTo("Char.Player");
		_pathToTask = new NavMeshPath();
		_subTaskDummy = new GameObject("PlayerSubTaskDummy");
		_subTaskDummy.transform.position = transform.position;
		hud = Util.Find("/Hud").GetComponent<HudController>();
		GetComponent<HumanAnimation>().ProceduralOrthopedistEnabled = true;
		GetComponent<HumanAnimation>().ProceduralAimingEnabled = true;
		Physics.IgnoreCollision(collider, groundTrigger.collider);
		Physics.IgnoreCollision(collider, leftFootCollider.collider);
		Physics.IgnoreCollision(collider, rightFootCollider.collider);
	}

	protected override void Start()
	{
		base.Start();
		foreach (PlayerData.Slots slot in Enum.GetValues(typeof(PlayerData.Slots)))
		{
			string equippedItem = Singleton.PlayerData.GetEquippedItem(slot);
			if (equippedItem != null)
			{
				Inventory.AddEquippable(equippedItem, slot);
			}
		}
		if (!Inventory.SetCurrentSlot(PlayerData.Instance.RecentlyEquipped))
		{
			Inventory.SetAnyCurrentWeapon();
		}
		if (!Singleton.PlayerData.IsSpecialHuntReceived && Singleton.PlayerData.CurrentMissionIndex == Singleton.ShopData.SpecialHunt.missionIndex)
		{
			Singleton.PlayerData.IsSpecialHuntPlayed = true;
			string saleWeapon = Singleton.ShopData.SpecialHunt.saleWeapon;
			WeaponShopItem weaponShopItem = Singleton.ShopData.GetShopItem(saleWeapon) as WeaponShopItem;
			Inventory.AddEquippable(weaponShopItem.item, weaponShopItem.Slot);
			Inventory.SetCurrentSlot(weaponShopItem.Slot);
		}
		foreach (KeyValuePair<string, int> consumable in Singleton.PlayerData.GetConsumables())
		{
			Inventory.AddConsumable(consumable.Key, consumable.Value, true);
		}
		if (Singleton.MetaMission.playerHealth >= 0f)
		{
			health = Singleton.MetaMission.playerHealth;
		}
		if (Singleton.MetaMission.playerWeapon != null)
		{
			SetCurrentWeapon(Singleton.MetaMission.playerWeapon);
		}
		hud.RegisterPlayer(this);
		Singleton.Visual.ShowOutline(gameObject, Color.black, 2);
		if (sourceOfInput == InputFrom.User)
		{
			Scene.Camera.LookAt(transform);
		}
		List<Weapon> list = new List<Weapon>();
		foreach (GameObject weapon in Inventory.Weapons)
		{
			if ((bool)weapon.GetComponent<Weapon>())
			{
				list.Add(weapon.GetComponent<Weapon>());
			}
		}
		GameAnalytics.CacheLogMissionWeaponsAtEnd(list);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Update()
	{
		if (TimeControl.IsPaused())
		{
			return;
		}
		if (!hud)
		{
			return;
		}
		_aimMode = hud.IsAimMode();
		_zoomMode = hud.IsZoomMode();
		wantToShoot = _aimMode || _zoomMode;
		if (wantToShoot && IsShooting && Inventory.Weapon.GetAmmoInClip() == 0 && !Inventory.Weapon.IsLauncher() && !IsReloading && Util.DoRarely(this, "Clang-Clang", 1f / Inventory.Weapon.GetRateOfFire()))
		{
			SoundManager.Play("gun_all_empty_click", false);
			if (_lastLoggedOutOfAmmoWeapon != Inventory.Weapon)
			{
				_lastLoggedOutOfAmmoWeapon = Inventory.Weapon;
				bool flag = true;
				Weapon weapon = null;
				foreach (GameObject weapon2 in Inventory.Weapons)
				{
					Weapon component = weapon2.GetComponent<Weapon>();
					if ((bool)component && !(component is MeleeWeapon) && component.GetAmmoInClip() > 0)
					{
						flag = false;
					}
					if ((bool)component && component is MeleeWeapon)
					{
						weapon = component;
					}
				}
				Mission.Type missionType = Mission.Type.Story1;
				Mission.Difficulty difficulty = Mission.Difficulty.Normal;
				int num = -1;
				if (Singleton.PlayerData.HasMission(Singleton.PlayerData.CurrentLocation))
				{
					num = Singleton.PlayerData.GetMission(Singleton.PlayerData.CurrentLocation);
					missionType = Singleton.GameData.Missions[num].Type;
					difficulty = Singleton.PlayerData.CurrentMissionDifficulty;
				}
				if (flag)
				{
					GameAnalytics.CacheLogWeaponOutOfAmmoCompletely(missionType, num, Singleton.PlayerData.Level, weapon ? weapon.gameObject.name : string.Empty);
					GameAnalytics.LogCompletelyOutOfAmmo(missionType, difficulty);
				}
				else
				{
					GameAnalytics.CacheLogWeaponOutOfAmmo(missionType, num, Singleton.PlayerData.Level, Inventory.Weapon ? Inventory.Weapon.gameObject.name : string.Empty);
				}
			}
		}
		isShooting = false;
		_isVictimLocked = false;
		if (state == State.Moving)
		{
			Aim();
		}
		if (state == State.Moving || state == State.Pain || state == State.Reloading || state == State.Throwing)
		{
			Move();
		}
		UpdateTask();
		base.Update();
	}

	public override void Aim()
	{
		Weapon weapon = Inventory.Weapon;
		if (!weapon)
		{
			return;
		}
		Ray ray = Scene.Camera.camera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
		shootingTo = ray.origin + ray.direction * Scene.Camera.camera.far;
		shootingToObject = null;
		if (!_zoomMode && wantToShoot && !IsReloading)
		{
			GameObject hitObject;
			bool flag = SpreadAim(this, ray.origin, ray.direction * Scene.Camera.camera.far, Inventory.Weapon.GetSpreadAngle(), out shootingTo, out hitObject);
			shootingToObject = hitObject;
			if (flag)
			{
				_lastTargetDetectionTime = Time.time;
				_lastTargetDetectionDistance = (shootingTo - ray.origin).magnitude;
				_lastTargetDetectionPoint = shootingTo;
				_lastTargetDetectionObject = hitObject;
				_lastTargetDetectionObjectPos = hitObject.transform.position;
			}
			else
			{
				float unusedDetectionValue1;
				float unusedDetectionValue2;
				float unusedDetectionValue3;
				float unusedDetectionValue4;
				float num = Mathf.Clamp01((_lastTargetDetectionDistance - 1f) / 24f);
				float num2 = 0.125f * (1f - num) + 0.25f * num;
				if (Time.time - _lastTargetDetectionTime <= num2)
				{
					flag = _lastTargetDetectionObject != null;
					shootingTo = _lastTargetDetectionPoint + (_lastTargetDetectionObject ? _lastTargetDetectionObject.transform.position - _lastTargetDetectionObjectPos : Vector3.zero);
					hitObject = _lastTargetDetectionObject;
					shootingToObject = _lastTargetDetectionObject;
				}
			}
			Vector3 lhs = ray.direction;
			lhs.y = 0f;
			lhs.Normalize();
			Vector3 forward = base.gameObject.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			if (Vector3.Dot(lhs, forward) >= 0.96f)
			{
				float unusedAimValue1;
				float unusedAimValue2;
				float unusedAimValue3;
				float unusedAimValue4;
				GameObject gameObject = hitObject;
				while ((bool)gameObject && gameObject.GetComponent<Character>() == null)
				{
					gameObject = gameObject.GetParent();
				}
				float value = 0f;
				if ((bool)gameObject)
				{
					value = ((gameObject.transform.position - transform.position).magnitude - 2.5f) / 47.5f;
				}
				value = Mathf.Clamp01(value);
				float num3 = 0.01f * (1f - value) + 0.24f * value;
				_aimingHistory.Add(new AimingTarget(gameObject, Time.deltaTime));
				bool flag2 = true;
				float num4 = 0f;
				GameObject target = _aimingHistory[0].target;
				foreach (AimingTarget item in _aimingHistory)
				{
					if (item.target != target)
					{
						flag2 = false;
					}
					num4 += item.dt;
				}
				if (!flag2)
				{
					_aimingHistory.Clear();
				}
				if (num4 >= num3 && _aimingHistory.Count > 0)
				{
					_aimingHistory.RemoveAt(0);
				}
				else
				{
					flag2 = false;
				}
				while (hitObject != null && !hitObject.GetComponent<Character>())
				{
					hitObject = hitObject.GetParent();
				}
				if ((bool)hitObject && flag && flag2)
				{
					float sqrMagnitude = (hitObject.transform.position - transform.position).sqrMagnitude;
					Character component = hitObject.GetComponent<Character>();
					if (sqrMagnitude > weapon.MaxAutofireDistance * weapon.MaxAutofireDistance)
					{
						if (!weapon.IsMelee())
						{
							hud.ShowCursorInfo(Strings.GetString("out of range"));
						}
					}
					else if ((bool)component && component is Human)
					{
						if (component is Bot)
						{
							hud.ShowCursorInfo(Strings.GetString("protect"));
						}
					}
					else if (weapon.IsExplosive() && sqrMagnitude * 0.9f < weapon.GetExplosionRadius() * weapon.GetExplosionRadius())
					{
						hud.ShowCursorInfo(Strings.GetString("too close"));
					}
					else
					{
						hud.ClearCursorInfo();
						_isVictimLocked = true;
						if (_fireDelayCountDown > 0f)
						{
							_fireDelayCountDown -= Time.deltaTime;
						}
						else
						{
							isShooting = true;
						}
					}
				}
				if (Singleton.Settings.MiniZombieControls)
				{
					isShooting = true;
				}
			}
		}
		else
		{
			_aimingHistory.Clear();
		}
		if (_zoomMode && (hud.ConsumeShootTap() || weapon.IsShooting()))
		{
			isShooting = true;
			RaycastHit[] array = Physics.RaycastAll(ray.origin, ray.direction, Scene.MainCamera.far, Const.LayerMaskHitByProjectile);
			if (array.Length > 0)
			{
				RaycastHit raycastHit = new RaycastHit();
				raycastHit.distance = float.PositiveInfinity;
				int num5 = 0;
				foreach (RaycastHit raycastHit2 in array)
				{
					if (raycastHit2.collider.gameObject != base.gameObject)
					{
						if (raycastHit.distance > raycastHit2.distance)
						{
							raycastHit = raycastHit2;
						}
						num5++;
					}
				}
				if (num5 > 0)
				{
					shootingTo = raycastHit.point;
					shootingToObject = raycastHit.collider.gameObject;
				}
			}
		}
		if (Singleton.Settings.MeleeButton && Inventory.Weapon.IsMelee() && hud.ConsumeShootTap())
		{
			isShooting = true;
		}
		if (!_isVictimLocked)
		{
			_fireDelayCountDown = direction.magnitude > 0.001f ? 0f : 0.01f;
		}
	}

	private static bool SpreadAim(Character shooter, Vector3 origin, Vector3 dir, float spreadAngle, out Vector3 hitPoint, out GameObject hitObject)
	{
		RaycastHit hitInfo;
		hitPoint = origin + dir;
		Ray ray = new Ray(origin, dir.normalized);
		float magnitude = dir.magnitude;
		float f = (float)Math.PI / 180f * (spreadAngle / 2f);
		float num = (float)Math.PI / 180f * (90f - spreadAngle / 2f);
		Vector3 vector2;
		Vector3 normalized = dir.normalized;
		Collider[] array;
		int num2 = 0;
		float num3 = float.PositiveInfinity;
		Vector3 vector = Vector3.zero;
		GameObject gameObject = null;
		foreach (Character @char in Scene.Chars)
		{
			if (!Scene.FilterChar(@char, false) || @char == shooter)
			{
				continue;
			}
			float num4 = Vector3.Dot(normalized, (@char.gameObject.transform.position - origin).normalized);
			if (num4 <= 0f)
			{
				continue;
			}
			float magnitude2 = (@char.gameObject.transform.position - origin).magnitude;
			if (magnitude2 > magnitude)
			{
				continue;
			}
			float radius = magnitude2 * Mathf.Tan(f);
			vector2 = origin + normalized * magnitude2;
			array = Physics.OverlapSphere(vector2, radius, Const.LayerMaskAllHitable);
			foreach (Collider collider in array)
			{
				hitPoint = (vector2 + collider.bounds.center) * 0.5f;
				if (collider.Raycast(ray, out hitInfo, magnitude))
				{
					hitPoint = hitInfo.point;
				}
				if (num3 > magnitude2 && collider.gameObject != shooter.gameObject)
				{
					num3 = magnitude2;
					vector = hitPoint;
					gameObject = collider.gameObject;
					num2++;
				}
			}
		}
		if (num2 > 0)
		{
			Vector3 vector3 = vector - origin;
			float magnitude3 = vector3.magnitude;
			vector3.Normalize();
			bool hitObstacle = Physics.Raycast(origin, vector3, out hitInfo, magnitude3, Const.LayerMaskObstacles);
			if (!hitObstacle || Mathf.Abs(magnitude3 - hitInfo.distance) < 0.1f)
			{
				hitPoint = vector;
				hitObject = gameObject;
				return true;
			}
		}
		RaycastHit[] array3 = Physics.RaycastAll(ray, magnitude, Const.LayerMaskHitByProjectile);
		RaycastHit raycastHit = new RaycastHit();
		raycastHit.distance = float.PositiveInfinity;
		gameObject = null;
		num2 = 0;
		foreach (RaycastHit raycastHit2 in array3)
		{
			if (raycastHit2.collider.gameObject.GetComponent<Character>() == shooter)
			{
				continue;
			}
			if (raycastHit.distance > raycastHit2.distance)
			{
				raycastHit = raycastHit2;
				gameObject = raycastHit2.collider.gameObject;
				num2++;
			}
		}
		if (num2 > 0)
		{
			hitPoint = raycastHit.point;
			hitObject = gameObject;
			return ((1 << raycastHit.collider.gameObject.layer) & Const.LayerMaskAllHitable) != 0;
		}
		hitObject = null;
		return false;
	}

	private void Move()
	{
		if (IgnoreUserInput)
		{
			return;
		}
		if (state == State.Throwing)
		{
			speed = 0f;
			return;
		}
		Transform transform = Scene.MainCamera.transform;
		Vector3 normalized = Util.ZeroY(Quaternion.Euler(0f, Scene.Camera.angleYOffset, 0f) * transform.forward).normalized;
		Vector3 normalized2 = Util.ZeroY(Quaternion.Euler(0f, Scene.Camera.angleYOffset, 0f) * transform.right).normalized;
		Vector2 characterMovementInput = hud.GetCharacterMovementInput();
		Vector3 vector = characterMovementInput.x * normalized2 + characterMovementInput.y * normalized;
		direction = vector.normalized;
		if (!wantToShoot && direction.magnitude > 0.001f)
		{
			base.transform.eulerAngles = Quaternion.LookRotation(direction).eulerAngles;
		}
		if (!wantToShoot && direction.magnitude < 0.001f)
		{
			_stillStayTime += Time.deltaTime;
			if (_stillStayTime > AutoRotationDelay)
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(normalized), AutoRotationDamping * Time.deltaTime);
			}
		}
		else
		{
			_stillStayTime = 0f;
		}
		float num = 1f;
		num = _zoomMode ? ZoomModeSpeedCoeff
			: wantToShoot ? fireStateSpeedCoeff
			: Input.GetKey(KeyCode.LeftShift) ? runningSpeedCoeff
			: lastMeleeDamageTime != 0f && Time.time - lastMeleeDamageTime <= meleeRecoveryDuration
				? meleeRecoveryMinSpeedMod + (1f - meleeRecoveryMinSpeedMod) * (Time.time - lastMeleeDamageTime) / meleeRecoveryDuration
				: 1f;
		_speedCoeff = Mathf.Lerp(_speedCoeff, num, Time.deltaTime * speedDamping);
		speed = Mathf.Min(vector.magnitude, 1f) * baseSpeed * _speedCoeff;
	}

	public void OnTaskChanged(Task newTask)
	{
		hud.SetMissionInfo(newTask.GetText());
		_isMissionTargetVisible = false;
		if ((bool)newTask.GetTarget() && !newTask.IsTargetDynamic())
		{
			NavMeshHit hit;
			NavMesh.SamplePosition(Scene.Mission.GetCurrentTask().GetTarget().transform.position, out hit, 10f, -1);
			if (hit.hit)
			{
				_currentMissionTargetCachedPos = hit.position;
			}
			else
			{
				log.Error(Scene.Mission.GetCurrentTask().GetTarget(), "Task object is far from nav mesh");
			}
			UpdatePathToTask();
			if (_pathToTask.corners.Length <= 0)
			{
				log.Error("No path to task");
				return;
			}
			_subTaskDummy.transform.position = _pathToTask.corners[_currentPathCorner];
		}
	}

	private void UpdateTask()
	{
		Task task = Scene.Mission ? Scene.Mission.GetCurrentTask() : null;
		if (!task)
		{
			return;
		}
		Vector3 position = transform.position;
		if (Util.DoRarelyReal(this, "task_path_update", 3f))
		{
			UpdatePathToTask();
		}
		if (Util.DoRarelyReal(this, "target_visible_update", 1f))
		{
			_isMissionTargetVisible = !Util.AnyObstacles(position, _currentMissionTargetCachedPos + Vector3.up);
		}
		if (task.IsTextDynamic() && Util.DoRarely(this, "update_text", 0.3f))
		{
			hud.SetMissionInfo(task.GetText(), false);
		}
		float unusedDistance;
		float magnitude = (_subTaskDummy.transform.position - position).magnitude;
		if (magnitude < 3.873f)
		{
			_currentPathCorner++;
			if (_currentPathCorner >= _pathToTask.corners.Length)
			{
				_currentPathCorner = _pathToTask.corners.Length - 1;
			}
			if (_currentPathCorner >= 0)
			{
				_subTaskDummy.transform.position = _pathToTask.corners[_currentPathCorner];
			}
		}
		_pathLength = magnitude + PathHelper.GetPathLength(_pathToTask, _currentPathCorner);
	}

	private void UpdatePathToTask()
	{
		if ((bool)Scene.Mission && (bool)Scene.Mission.GetCurrentTask() && (bool)Scene.Mission.GetCurrentTask().GetTarget() && !Scene.Mission.GetCurrentTask().IsTargetDynamic())
		{
			NavMeshHit hit;
			NavMesh.SamplePosition(_currentMissionTargetCachedPos, out hit, 3f, -1);
			bool hit2 = hit.hit;
			bool flag = false;
			if (hit2)
			{
				flag = NavMesh.CalculatePath(transform.position, _currentMissionTargetCachedPos, -1, _pathToTask);
			}
			if (!hit2 || !flag || _pathToTask.corners.Length == 0)
			{
				log.Warn(Scene.Mission.GetCurrentTask(), "can not find path to '{0}'", Scene.Mission.GetCurrentTask().GetTarget().name);
			}
			else
			{
				_currentPathCorner = 0;
				_subTaskDummy.transform.position = _pathToTask.corners[_currentPathCorner];
			}
		}
	}

	public override bool CanBeHurt()
	{
		return state != State.Throwing && !Singleton.Settings.InvulnerablePlayer && base.CanBeHurt();
	}

	protected override bool CanBeStunned(DamageInfo info)
	{
		return false;
	}

	protected override void OnHurt(DamageInfo info)
	{
		if (!(info.source is Player))
		{
			MusicManager.OnPlayerDamage();
		}
		GameObject arrow = hud.CreateDamageArrow();
		arrow.transform.GetComponentInChildren<PackedSprite>().hideAtStart = false;
		Util.SetGameObjectVisible(arrow, true);
		arrow.AddComponent<MiniScript>();
		arrow.GetComponent<MiniScript>().Charge(0.5f, -info.momentum, null, (MiniScript script) =>
		{
			arrow.transform.GetComponentInChildren<PackedSprite>().SetColor(new Color(1f, 1f, 1f, Mathf.Min(1f, 2f * (1f - 1f * script.Timer.Progress()))));
			script.gameObject.transform.rotation = Quaternion.FromToRotation(Util.XyzToXz0(Scene.MainCamera.transform.forward), Util.XyzToXz0(script.Param<Vector3>()));
		}, (MiniScript script) =>
		{
			UnityEngine.Object.Destroy(script.gameObject);
		});
		base.OnHurt(info);
	}

	protected override void OnDying(DamageInfo info)
	{
		Singleton.GameEvents.OnPlayerKill(this, info.source as Unit);
		SoundManager.Play("Evelyn/evelyn_death", false);
		Scene.Camera.SetLowPassFilter(550f, 3f);
		base.OnDying(info);
	}

	public void OnFootstep(Collider footCollider)
	{
		if (footStepSound != null && footStepSound.Length > 0)
		{
			SoundManager.Play(footStepSound[UnityEngine.Random.Range(0, footStepSound.Length)], footCollider.transform.position, 5f, 25f, false);
		}
		Singleton.PlayerData.PlayerStatistics.OnFootstep();
	}

	public void TryConsume(string itemName)
	{
		Consumable consumable = Inventory.TryConsume(itemName);
		if ((bool)consumable)
		{
			Singleton.GameEvents.OnConsumableUsed(this, consumable);
		}
	}

	public void SetCurrentWeapon(string itemName)
	{
		if (state == State.Reloading)
		{
			ChangeState(State.Moving);
		}
		Inventory.SetCurrentWeapon(itemName);
	}

	public void SetCurrentSlot(PlayerData.Slots slot)
	{
		if (!(Inventory.GetCurrentSlot() != slot))
		{
			return;
		}
		if (state == State.Reloading)
		{
			ChangeState(State.Moving);
		}
		Inventory.SetCurrentSlot(slot);
		PlayerData.Instance.RecentlyEquipped = slot;
	}

	public void SwitchWeapon()
	{
		if (state == State.Reloading)
		{
			ChangeState(State.Moving);
		}
		Inventory.SwitchWeapon();
	}

	public void StandUp()
	{
		ChangeState(State.Appearing, State.Moving, 0f);
	}

	protected override void OnDrawGizmos()
	{
		if (!(Application.isPlaying))
		{
			return;
		}
		for (int i = 0; i < _pathToTask.corners.Length - 1; i++)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(_pathToTask.corners[i], _pathToTask.corners[i + 1]);
		}
		base.OnDrawGizmos();
	}

	private void OnPlayDrawAnimation(UnityEngine.Object obj)
	{
		string drawAnimationName = _humanAnimation.DrawAnimationName;
		animation.Play(drawAnimationName);
		animation.Blend(drawAnimationName, 0f, animation[drawAnimationName].length);
	}
}
