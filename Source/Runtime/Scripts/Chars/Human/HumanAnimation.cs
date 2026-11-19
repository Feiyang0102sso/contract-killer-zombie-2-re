using System;
using UnityEngine;

public class HumanAnimation : MonoBehaviourAndLog
{
	private struct PainActuator
	{
		public bool enabled;

		public Vector3 tilt;

		public float phase;

		public float weight;
	}

	private const float runAnimSpeed = 4f;

	private const float walkAnimSpeed = 1.42f;

	private const float slowInjuredWalkAnimSpeed = 0.67f;

	private const float fastInjuredWalkAnimSpeed = 2.4f;

	private const float strafeAnimSpeed = 1f;

	public const float crippledMaxBaseSpeed = 2.5f;

	private const float fadeLength = 0.3f;

	public float walkSpeed = 4f;

	public float shootPrepareTimeSec = 0.5f;

	public int maxPainActuators = 8;

	private Human _owner;

	private Weapon _weapon;

	private HumanWeapon _humanWeapon;

	private Animation _animation;

	private string _run = "carbine_run";

	private string _walk = "carbine_walk";

	private string _idle = "carbine_idle";

	private string _leg_idle = "leg_idle";

	private string _shoot = "carbine_shoot";

	private string _draw = "carbine_draw";

	private string _reload = "carbine_reload";

	private string _reloadStart = string.Empty;

	private string _reloadCycle = string.Empty;

	private string _reloadEnd = string.Empty;

	private Transform _hip;

	private Transform _spine;

	private Transform _lineOfSight;

	private float _proceduralAimWeight;

	private PainActuator[] _painActuators;

	private int _nextPainActuatorId;

	private int _numActivePainActuators;

	private bool _orthopedistEnabled;

	private Vector2 _tiltAndTurnOrtho = Vector2.zero;

	private int _tiltAndTurnOrthoStep;

	private bool _aimingEnabled;

	private Vector2 _tiltAndTurn = Vector2.zero;

	public string[] ReloadingSequence
	{
		get
		{
			string[] result = new string[1] { _reload };
			if ((bool)_weapon)
			{
				Firearm firearm = _weapon as Firearm;
				if ((bool)firearm && firearm.NumPellets > 1 && firearm.AnimationPrefix == "shotgun")
				{
					int num = firearm.GetAmmoClipSize() - firearm.GetAmmoInClip();
					if (firearm.GetAmmoInBackpack() >= 0)
					{
						num = (!(num <= firearm.GetAmmoInBackpack()) ? firearm.GetAmmoInBackpack() : num);
					}
					string[] array = new string[2 + num];
					array[0] = _reloadStart;
					for (int i = 0; i < num; i++)
					{
						array[i + 1] = _reloadCycle;
					}
					array[array.Length - 1] = _reloadEnd;
					return array;
				}
				return result;
			}
			return result;
		}
	}

	private void Awake()
	{
		LogTo("Char.Human.HumanAnimation");
		_owner = GetComponent<Human>();
		_humanWeapon = GetComponent<HumanWeapon>();
		_animation = GetComponent<Animation>();
		_weapon = null;
		_painActuators = new PainActuator[maxPainActuators];
		_hip = this.QuickFindChild("Bip01hip_Bone").transform;
		GameObject gameObject = this.QuickFindChild("Bip01hip_Bone/Bip01spine_Bone");
		_spine = (gameObject ? gameObject.transform : null);
		GameObject gameObject2 = this.QuickFindChild("Bip01hip_Bone/Bip01spine_Bone/Bip01chest_Bone/line_of_sight");
		_lineOfSight = (gameObject2 ? gameObject2.transform : null);
	}

	private void Start()
	{
		_animation.wrapMode = WrapMode.Loop;
		_animation["carbine_run"].layer = -1;
		_animation["carbine_walk"].layer = -1;
		_animation["sawn_shotgun_run"].layer = -1;
		_animation["sawn_shotgun_walk"].layer = -1;
		_animation["Arbalest_run"].layer = -1;
		_animation["Arbalest_walk"].layer = -1;
		_animation["pistols_run"].layer = -1;
		_animation["pistols_walk"].layer = -1;
		_animation["machinegun_walk"].layer = -1;
		_animation["machinegun_run"].layer = -1;
		_animation["strafe_right"].layer = -1;
		_animation["strafe_left"].layer = -1;
		_animation["carbine_idle"].layer = -1;
		_animation["sawn_shotgun_idle"].layer = -1;
		_animation["Arbalest_idle"].layer = -1;
		_animation["pistols_idle"].layer = -1;
		_animation["machinegun_idle"].layer = -1;
		_animation["fast_injured_walk"].layer = -1;
		_animation["rpg_walk"].layer = -1;
		_animation["rpg_run"].layer = -1;
		_animation["rpg_idle"].layer = -1;
		_animation["melee_walk"].layer = -1;
		_animation["melee_run"].layer = -1;
		_animation["melee_idle"].layer = -1;
		_animation["leg_idle"].layer = -1;
		_animation["leg_idle"].AddMixingTransform(_hip, false);
		_animation["leg_idle"].AddMixingTransform(transform.Find("Bip01hip_Bone/Bip01L_thigh_Bone"));
		_animation["leg_idle"].AddMixingTransform(transform.Find("Bip01hip_Bone/Bip01R_thigh_Bone"));
		_animation["carbine_draw"].wrapMode = WrapMode.Once;
		_animation["sawn_shotgun_draw"].layer = -1;
		_animation["pistols_draw"].wrapMode = WrapMode.Once;
		_animation["machinegun_draw"].wrapMode = WrapMode.Once;
		_animation["death"].wrapMode = WrapMode.Once;
		_animation["stand_up"].wrapMode = WrapMode.Once;
		_animation["carbine_shoot"].layer = 1;
		_animation["carbine_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["carbine_shoot"].speed = 0f;
		_animation["carbine_shoot"].AddMixingTransform(_spine);
		_animation["carbine_reload"].layer = 1;
		_animation["carbine_reload"].wrapMode = WrapMode.Once;
		_animation["carbine_reload"].speed = 1f;
		_animation["carbine_reload"].AddMixingTransform(_spine);
		_animation["carbine_draw"].layer = 1;
		_animation["carbine_draw"].wrapMode = WrapMode.Once;
		_animation["carbine_draw"].speed = 1f;
		_animation["carbine_draw"].AddMixingTransform(_spine);
		_animation["sawn_shotgun_shoot"].layer = 1;
		_animation["sawn_shotgun_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["sawn_shotgun_shoot"].speed = 0f;
		_animation["sawn_shotgun_shoot"].AddMixingTransform(_spine);
		_animation["sawn_shotgun_reload"].layer = 1;
		_animation["sawn_shotgun_reload"].wrapMode = WrapMode.Once;
		_animation["sawn_shotgun_reload"].speed = 1f;
		_animation["sawn_shotgun_reload"].AddMixingTransform(_spine);
		_animation["sawn_shotgun_draw"].layer = 1;
		_animation["sawn_shotgun_draw"].wrapMode = WrapMode.Once;
		_animation["sawn_shotgun_draw"].speed = 1f;
		_animation["sawn_shotgun_draw"].AddMixingTransform(_spine);
		_animation["Arbalest_shoot"].layer = 1;
		_animation["Arbalest_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["Arbalest_shoot"].speed = 1f;
		_animation["Arbalest_shoot"].AddMixingTransform(_spine);
		_animation["Arbalest_reload"].layer = 1;
		_animation["Arbalest_reload"].wrapMode = WrapMode.Once;
		_animation["Arbalest_reload"].speed = 1f;
		_animation["Arbalest_reload"].AddMixingTransform(_spine);
		_animation["Arbalest_draw"].layer = 1;
		_animation["Arbalest_draw"].wrapMode = WrapMode.Once;
		_animation["Arbalest_draw"].speed = 1f;
		_animation["Arbalest_draw"].AddMixingTransform(_spine);
		_animation["machinegun_shoot"].layer = 1;
		_animation["machinegun_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["machinegun_shoot"].speed = 0f;
		_animation["machinegun_shoot"].AddMixingTransform(_spine);
		_animation["machinegun_reload"].layer = 1;
		_animation["machinegun_reload"].wrapMode = WrapMode.Once;
		_animation["machinegun_reload"].speed = 1f;
		_animation["machinegun_reload"].AddMixingTransform(_spine);
		_animation["machinegun_draw"].layer = 1;
		_animation["machinegun_draw"].wrapMode = WrapMode.Once;
		_animation["machinegun_draw"].speed = 1f;
		_animation["machinegun_draw"].AddMixingTransform(_spine);
		_animation["pistols_shoot"].layer = 1;
		_animation["pistols_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["pistols_shoot"].speed = 0f;
		_animation["pistols_shoot"].AddMixingTransform(_spine);
		_animation["pistols_reload"].layer = 1;
		_animation["pistols_reload"].wrapMode = WrapMode.Once;
		_animation["pistols_reload"].speed = 1f;
		_animation["pistols_reload"].AddMixingTransform(_spine);
		_animation["pistols_draw"].layer = 1;
		_animation["pistols_draw"].wrapMode = WrapMode.Once;
		_animation["pistols_draw"].speed = 1f;
		_animation["pistols_draw"].AddMixingTransform(_spine);
		_animation["ams_shoot"].layer = 1;
		_animation["ams_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["ams_shoot"].speed = 0f;
		_animation["ams_shoot"].AddMixingTransform(_spine);
		_animation["ams_reload"].layer = 1;
		_animation["ams_reload"].wrapMode = WrapMode.Once;
		_animation["ams_reload"].speed = 1f;
		_animation["ams_reload"].AddMixingTransform(_spine);
		_animation["ams_draw"].layer = 1;
		_animation["ams_draw"].wrapMode = WrapMode.Once;
		_animation["ams_draw"].speed = 1f;
		_animation["ams_draw"].AddMixingTransform(_spine);
		_animation["bolt_rifle_shoot"].layer = 1;
		_animation["bolt_rifle_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["bolt_rifle_shoot"].speed = 0f;
		_animation["bolt_rifle_shoot"].AddMixingTransform(_spine);
		_animation["bolt_rifle_reload"].layer = 1;
		_animation["bolt_rifle_reload"].wrapMode = WrapMode.Once;
		_animation["bolt_rifle_reload"].speed = 1f;
		_animation["bolt_rifle_reload"].AddMixingTransform(_spine);
		_animation["bolt_rifle_draw"].layer = 1;
		_animation["bolt_rifle_draw"].wrapMode = WrapMode.Once;
		_animation["bolt_rifle_draw"].speed = 1f;
		_animation["bolt_rifle_draw"].AddMixingTransform(_spine);
		_animation["rpg_draw"].layer = 1;
		_animation["rpg_draw"].wrapMode = WrapMode.Once;
		_animation["rpg_draw"].speed = 1f;
		_animation["rpg_draw"].AddMixingTransform(_spine);
		_animation["rpg_shoot"].layer = 1;
		_animation["rpg_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["rpg_shoot"].speed = 1f;
		_animation["rpg_shoot"].AddMixingTransform(_spine);
		_animation["rpg_reload"].layer = 1;
		_animation["rpg_reload"].wrapMode = WrapMode.Once;
		_animation["rpg_reload"].speed = 1f;
		_animation["rpg_reload"].AddMixingTransform(_spine);
		if ((bool)_animation.GetClip("launcher_draw"))
		{
			_animation["launcher_draw"].layer = 1;
			_animation["launcher_draw"].wrapMode = WrapMode.Once;
			_animation["launcher_draw"].speed = 1f;
			_animation["launcher_draw"].AddMixingTransform(_spine);
			_animation["launcher_shoot"].layer = 1;
			_animation["launcher_shoot"].wrapMode = WrapMode.ClampForever;
			_animation["launcher_shoot"].speed = 1f;
			_animation["launcher_shoot"].AddMixingTransform(_spine);
			_animation["launcher_reload"].layer = 1;
			_animation["launcher_reload"].wrapMode = WrapMode.Once;
			_animation["launcher_reload"].speed = 1f;
			_animation["launcher_reload"].AddMixingTransform(_spine);
		}
		_animation["shotgun_draw"].layer = 1;
		_animation["shotgun_draw"].wrapMode = WrapMode.Once;
		_animation["shotgun_draw"].speed = 1f;
		_animation["shotgun_draw"].AddMixingTransform(_spine);
		_animation["shotgun_shoot"].layer = 1;
		_animation["shotgun_shoot"].wrapMode = WrapMode.ClampForever;
		_animation["shotgun_shoot"].speed = 1f;
		_animation["shotgun_shoot"].AddMixingTransform(_spine);
		_animation["shotgun_reload_start"].layer = 1;
		_animation["shotgun_reload_start"].wrapMode = WrapMode.ClampForever;
		_animation["shotgun_reload_start"].speed = 1f;
		_animation["shotgun_reload_start"].AddMixingTransform(_spine);
		_animation["shotgun_reload_loop"].layer = 1;
		_animation["shotgun_reload_loop"].wrapMode = WrapMode.ClampForever;
		_animation["shotgun_reload_loop"].speed = 1f;
		_animation["shotgun_reload_loop"].AddMixingTransform(_spine);
		_animation["shotgun_reload_end"].layer = 1;
		_animation["shotgun_reload_end"].wrapMode = WrapMode.Once;
		_animation["shotgun_reload_end"].speed = 1f;
		_animation["shotgun_reload_end"].AddMixingTransform(_spine);
		_animation["throw"].layer = 1;
		_animation["throw"].wrapMode = WrapMode.Once;
		_animation["throw"].speed = 1f;
		_animation["throw"].AddMixingTransform(_spine);
		_animation["melee_draw"].layer = 1;
		_animation["melee_draw"].wrapMode = WrapMode.Once;
		_animation["melee_draw"].speed = 1f;
		_animation["melee_draw"].AddMixingTransform(_spine);
		_animation["melee_attack1"].layer = 2;
		_animation["melee_attack1"].wrapMode = WrapMode.ClampForever;
		_animation["melee_attack1"].speed = 1f;
		_animation["melee_attack1"].AddMixingTransform(_spine);
		_animation["melee_attack1_leg"].layer = 0;
		_animation["melee_attack1_leg"].wrapMode = WrapMode.Once;
		_animation["melee_attack1_leg"].speed = 1f;
		_animation["melee_attack2"].layer = 2;
		_animation["melee_attack2"].wrapMode = WrapMode.ClampForever;
		_animation["melee_attack2"].speed = 1f;
		_animation["melee_attack2"].AddMixingTransform(_spine);
		_animation["melee_attack2_leg"].layer = 0;
		_animation["melee_attack2_leg"].wrapMode = WrapMode.Once;
		_animation["melee_attack2_leg"].speed = 1f;
		_animation["melee_attack3"].layer = 2;
		_animation["melee_attack3"].wrapMode = WrapMode.ClampForever;
		_animation["melee_attack3"].speed = 1f;
		_animation["melee_attack3"].AddMixingTransform(_spine);
		_animation["melee_attack3_leg"].layer = 0;
		_animation["melee_attack3_leg"].wrapMode = WrapMode.Once;
		_animation["melee_attack3_leg"].speed = 1f;
		_animation["chainsaw_draw"].layer = 1;
		_animation["chainsaw_draw"].wrapMode = WrapMode.Once;
		_animation["chainsaw_draw"].speed = 1f;
		_animation["chainsaw_draw"].AddMixingTransform(_spine);
		_animation["chainsaw_attack"].layer = 2;
		_animation["chainsaw_attack"].wrapMode = WrapMode.ClampForever;
		_animation["chainsaw_attack"].speed = 1f;
		_animation["chainsaw_attack"].AddMixingTransform(_spine);
		_animation["chainsaw_attack_leg"].layer = 0;
		_animation["chainsaw_attack_leg"].wrapMode = WrapMode.Once;
		_animation["chainsaw_attack_leg"].speed = 1f;
		_animation["chainsaw_attack2"].layer = 2;
		_animation["chainsaw_attack2"].wrapMode = WrapMode.Loop;
		_animation["chainsaw_attack2"].speed = 1f;
		_animation["chainsaw_attack2"].AddMixingTransform(_spine);
		_animation["chainsaw_attack2_leg"].layer = 0;
		_animation["chainsaw_attack2_leg"].wrapMode = WrapMode.Once;
		_animation["chainsaw_attack2_leg"].speed = 1f;
		_animation.SyncLayer(-1);
		_animation.Stop();
	}

	private void ResetWeaponAnimation()
	{
		if ((bool)_animation[_run])
		{
			_animation.Blend(_run, 0f, 0f);
		}
		if ((bool)_animation[_walk])
		{
			_animation.Blend(_walk, 0f, 0f);
		}
		if ((bool)_animation[_idle])
		{
			_animation.Blend(_idle, 0f, 0f);
		}
		if ((bool)_animation[_leg_idle])
		{
			_animation.Blend(_leg_idle, 0f, 0f);
		}
		if ((bool)_animation[_shoot])
		{
			_animation.Blend(_shoot, 0f, 0f);
		}
		if ((bool)_animation[_draw])
		{
			_animation.Blend(_draw, 0f, 0f);
		}
		if ((bool)_weapon)
		{
			_weapon.ResetAnimations();
		}
	}

	private void Update()
	{
		if (!_owner.AllowExternalAnimation() || TimeControl.IsPaused())
		{
			return;
		}
		Weapon.OperationType operationType = Weapon.OperationType.OneHanded;
		if ((bool)_weapon)
		{
			operationType = _weapon.Operation;
		}
		float num = _owner.Speed;
		Vector3 direction = _owner.Direction;
		if (Mathf.Abs(num) < 0.01f)
		{
			num = 0f;
		}
		float num2 = Vector3.Dot(direction, transform.forward) * num;
		float num3 = Mathf.Abs(num2);
		float num4 = Vector3.Dot(direction, transform.right) * num;
		float num5 = Mathf.Abs(num4);
		float num6 = 1f;
		if (num3 < num5)
		{
			num6 = num3 / num5;
		}
		float num7 = 1f;
		if (num5 < num3)
		{
			num7 = num5 / num3;
		}
		bool flag = num3 > 0f;
		bool flag2 = _owner.baseSpeed <= 2.5f;
		bool flag3 = num3 > walkSpeed && !flag2;
		bool flag4 = num2 >= 0f;
		bool flag5 = num4 > 0f;
		bool flag6 = num5 > 0f;
		float num8 = (((_owner.WantToShoot || _owner.IsInDanger) && !flag3 && (bool)_weapon && !_weapon.IsMelee()) ? 1 : 0);
		string text = flag2 ? "fast_injured_walk" : _walk;
		float num9 = flag2 ? 2.4f : 1.42f;
		float targetWeight = flag && flag3 ? 1f * num6 : 0f;
		_animation.Blend(_run, targetWeight, 0.3f);
		_animation[_run].speed = num2 / 4f;
		float targetWeight2 = flag && !flag3 ? 1f * num6 : 0f;
		_animation.Blend(text, targetWeight2, 0.3f);
		_animation[text].speed = num2 / num9;
		float num10 = num > 0f ? 0f : 1f;
		float num11 = 0f;
		if (_owner.IsReloading)
		{
			num10 = 0f;
			num11 = num > 0f ? 0f : num8;
		}
		else
		{
			num10 *= 1f - num8;
			num11 = num > 0f ? 0f : num8;
		}
		_animation.Blend(_idle, num10, 0.3f);
		_animation.Blend(_leg_idle, num11, 0.3f);
		float targetWeight3 = flag6 && ((flag5 && flag4) || (!flag5 && !flag4)) ? 1f * num7 : 0f;
		float targetWeight4 = flag6 && ((!flag5 && flag4) || (flag5 && !flag4)) ? 1f * num7 : 0f;
		_animation.Blend("strafe_right", targetWeight3, 0.3f);
		_animation["strafe_right"].speed = num5 / 1f * (flag4 ? 1f : -1f);
		_animation.Blend("strafe_left", targetWeight4, 0.3f);
		_animation["strafe_left"].speed = num5 / 1f * (flag4 ? 1f : -1f);
		if ((bool)_weapon && !_weapon.IsMelee())
		{
			_animation.Blend(_shoot, !_owner.IsReloading ? num8 : 0f, shootPrepareTimeSec);
		}
	}

	private void LateUpdate()
	{
		int unusedAnimationIndex;
		float unusedAnimationValue1;
		float unusedAnimationValue2;
		if (!_animation.isPlaying || TimeControl.IsPaused())
		{
			return;
		}
		float num = 0.25f / Time.deltaTime;
		if (_owner.Speed > 0.01f)
		{
			num = 1f;
		}
		if (_orthopedistEnabled && _owner.Speed > 0.01f && _tiltAndTurnOrthoStep > 0 && !_weapon.IsMelee())
		{
			Vector3 normalized = _spine.forward.normalized;
			Vector3 normalized2 = (_spine.right * -1f).normalized;
			Vector3 normalized3 = (_spine.up * -1f).normalized;
			Vector3 normalized4 = _spine.parent.forward.normalized;
			Vector3 normalized5 = (_spine.parent.right * -1f).normalized;
			Vector3 normalized6 = (_spine.parent.up * -1f).normalized;
			normalized4 = _spine.parent.parent.right.normalized;
			normalized5 = _spine.parent.parent.up.normalized;
			normalized6 = _spine.parent.parent.forward.normalized;
			Vector3 v = normalized2 * Vector3.Dot(normalized2, normalized5) + normalized * Vector3.Dot(normalized, normalized5);
			Vector3 v2 = normalized3 * Vector3.Dot(normalized3, normalized5) + normalized2 * Vector3.Dot(normalized2, normalized5);
			v.Normalize();
			v2.Normalize();
			Vector2 zero = Vector2.zero;
			zero.x = Firearm.GetAngle(v, normalized2, normalized3);
			zero.y = Firearm.GetAngle(v2, normalized2, normalized);
			Vector2 vector = zero - _tiltAndTurnOrtho;
			_spine.Rotate(normalized3, -vector.x, Space.World);
			_spine.Rotate(normalized, -vector.y, Space.World);
		}
		else if (_orthopedistEnabled && _tiltAndTurnOrthoStep < 8)
		{
			Vector3 normalized7 = _spine.forward.normalized;
			Vector3 normalized8 = (_spine.right * -1f).normalized;
			Vector3 normalized9 = (_spine.up * -1f).normalized;
			Vector3 normalized10 = _spine.parent.forward.normalized;
			Vector3 normalized11 = (_spine.parent.right * -1f).normalized;
			Vector3 normalized12 = (_spine.parent.up * -1f).normalized;
			normalized10 = _spine.parent.parent.right.normalized;
			normalized11 = _spine.parent.parent.up.normalized;
			normalized12 = _spine.parent.parent.forward.normalized;
			Vector3 v3 = normalized8 * Vector3.Dot(normalized8, normalized11) + normalized7 * Vector3.Dot(normalized7, normalized11);
			Vector3 v4 = normalized9 * Vector3.Dot(normalized9, normalized11) + normalized8 * Vector3.Dot(normalized8, normalized11);
			v3.Normalize();
			v4.Normalize();
			Vector2 zero2 = Vector2.zero;
			zero2.x = Firearm.GetAngle(v3, normalized8, normalized9);
			zero2.y = Firearm.GetAngle(v4, normalized8, normalized7);
			_tiltAndTurnOrthoStep++;
			if (_tiltAndTurnOrthoStep > 1)
			{
				_tiltAndTurnOrtho = ((_tiltAndTurnOrthoStep - 1) * _tiltAndTurnOrtho + zero2) / _tiltAndTurnOrthoStep;
			}
			else
			{
				_tiltAndTurnOrtho = zero2;
			}
		}
		if (_aimingEnabled && (_owner.WantToShoot || _owner.IsInDanger) && !_owner.IsReloading && !_weapon.IsMelee())
		{
			Vector3 position = _spine.position;
			Vector3 normalized13 = _spine.forward.normalized;
			Vector3 normalized14 = (_spine.right * -1f).normalized;
			Vector3 normalized15 = (_spine.up * -1f).normalized;
			Transform barrel = _humanWeapon.GetBarrel(0);
			Vector3 shotDirection = _owner.ShootingTo - barrel.position;
			Vector3 normalized16 = shotDirection.normalized;
			if ((bool)_lineOfSight)
			{
				Vector3 normalized17 = _lineOfSight.right.normalized;
				Vector3 normalized18 = _lineOfSight.forward.normalized;
				Vector3 vector2 = -_lineOfSight.up.normalized;
				Vector3 position2 = _lineOfSight.position;
				float num2 = Vector3.Dot(normalized16, vector2);
				num2 = num2 < 0f ? 0f : num2;
				Vector3 v5 = normalized17 * Vector3.Dot(normalized17, normalized16) + vector2 * Vector3.Dot(vector2, normalized16);
				v5.Normalize();
				float num3 = Firearm.GetAngle(v5, vector2, normalized18) * num2;
				_tiltAndTurn.y = _tiltAndTurn.y * (1f - 1f / num) + num3 * (1f / num);
				num3 = _tiltAndTurn.y;
				_spine.Rotate(normalized14, -num3 * _proceduralAimWeight * num2, Space.World);
				position = _spine.position;
				normalized13 = _spine.forward.normalized;
				normalized14 = (_spine.right * -1f).normalized;
				normalized15 = (_spine.up * -1f).normalized;
				Vector3 v6 = normalized18 * Vector3.Dot(normalized18, normalized16) + vector2 * Vector3.Dot(vector2, normalized16);
				v6.Normalize();
				num3 = Firearm.GetAngle(v6, vector2, normalized17) * num2;
				_tiltAndTurn.x = _tiltAndTurn.x * (1f - 1f / num) + num3 * (1f / num);
				num3 = _tiltAndTurn.x;
				_spine.Rotate(normalized13, -num3 * _proceduralAimWeight * num2, Space.World);
				UnityEngine.Debug.DrawRay(_lineOfSight.position, -_lineOfSight.up * 2f, Color.red);
				UnityEngine.Debug.DrawLine(_lineOfSight.position, _owner.ShootingTo, Color.green);
			}
			else
			{
				float num4 = Vector3.Dot(normalized16, normalized15);
				num4 = num4 < 0f ? 0f : num4;
				Vector3 v7 = normalized13 * Vector3.Dot(normalized13, normalized16) + normalized15 * Vector3.Dot(normalized15, normalized16);
				v7.Normalize();
				float angle = Firearm.GetAngle(v7, normalized15, normalized14);
				_tiltAndTurn.y = _tiltAndTurn.y * (1f - 1f / num) + angle * (1f / num);
				angle = _tiltAndTurn.y;
				_spine.Rotate(normalized14, -angle * _proceduralAimWeight * num4, Space.World);
				position = _spine.position;
				normalized13 = _spine.forward.normalized;
				normalized14 = (_spine.right * -1f).normalized;
				normalized15 = (_spine.up * -1f).normalized;
				Vector3 v8 = normalized14 * Vector3.Dot(normalized14, normalized16) + normalized15 * Vector3.Dot(normalized15, normalized16);
				v8.Normalize();
				angle = Firearm.GetAngle(v8, normalized15, normalized13);
				_tiltAndTurn.x = _tiltAndTurn.x * (1f - 1f / num) + angle * (1f / num);
				angle = _tiltAndTurn.x;
				_spine.Rotate(normalized13, -angle * _proceduralAimWeight * num4, Space.World);
			}
			_proceduralAimWeight += 2f * Time.deltaTime;
			_proceduralAimWeight = _proceduralAimWeight > 1f ? 1f : _proceduralAimWeight;
		}
		else
		{
			_proceduralAimWeight = 0f;
			_tiltAndTurn = Vector2.zero;
		}
		if (_numActivePainActuators > 0)
		{
			float unusedPainValue1;
			float unusedPainValue2;
			float num5 = 1f;
			for (int i = 0; i < maxPainActuators; i++)
			{
				if (_painActuators[i].enabled)
				{
					float num6 = Mathf.Pow(1f - _painActuators[i].phase / (float)Math.PI, 0.45f);
					_painActuators[i].phase += num6 * Time.deltaTime * (float)Math.PI / 0.5f;
					if (_painActuators[i].phase < (float)Math.PI)
					{
						_painActuators[i].weight = Mathf.Sin(_painActuators[i].phase);
						num5 += _painActuators[i].weight;
					}
					else
					{
						_painActuators[i].enabled = false;
						_numActivePainActuators--;
					}
				}
			}
			Vector3 up = Vector3.up;
			for (int j = 0; j < maxPainActuators; j++)
			{
				if (_painActuators[j].enabled)
				{
					_painActuators[j].weight *= 1f / num5;
					up += _painActuators[j].tilt * _painActuators[j].weight;
				}
			}
			up.Normalize();
			Vector3 position3 = _spine.position;
			Vector3 normalized19 = _spine.forward.normalized;
			Vector3 normalized20 = (_spine.right * -1f).normalized;
			Vector3 normalized21 = (_spine.up * -1f).normalized;
			Vector3 v9 = Vector3.forward * Vector3.Dot(Vector3.forward, up) + Vector3.up * Vector3.Dot(Vector3.up, up);
			Vector3 v10 = Vector3.right * Vector3.Dot(Vector3.right, up) + Vector3.up * Vector3.Dot(Vector3.up, up);
			v9.Normalize();
			v10.Normalize();
			float angle2 = Firearm.GetAngle(v9, Vector3.up, Vector3.right);
			float angle3 = Firearm.GetAngle(v10, Vector3.up, Vector3.forward);
			_spine.Rotate(normalized19, angle2, Space.World);
			_spine.Rotate(normalized21, angle3, Space.World);
		}
	}

	public void OnWeaponShot()
	{
		if (!(!_weapon.IsMelee()))
		{
			return;
		}
		_animation[_shoot].time = 0f;
	}

	public void AnimateDamage(Vector3 damageDirOs)
	{
		float unusedDamageAngle;
		float unusedDamageWeight;
		Vector3 axis = Vector3.Cross(Vector3.up, damageDirOs);
		Quaternion q = Quaternion.AngleAxis(45f + UnityEngine.Random.Range(-15f, 15f), axis);
		Matrix4x4 matrix4x = new Matrix4x4();
		matrix4x.SetTRS(Vector3.zero, q, Vector3.one);
		if (!_painActuators[_nextPainActuatorId].enabled)
		{
			_numActivePainActuators++;
		}
		_painActuators[_nextPainActuatorId].enabled = true;
		_painActuators[_nextPainActuatorId].tilt = matrix4x.MultiplyVector(Vector3.up);
		_painActuators[_nextPainActuatorId].phase = 0f;
		_painActuators[_nextPainActuatorId].weight = 0f;
		_nextPainActuatorId++;
		_nextPainActuatorId = _nextPainActuatorId >= maxPainActuators ? 0 : _nextPainActuatorId;
	}

	public void SetWeapon(Weapon weapon)
	{
		string unusedLocal1;
		string unusedLocal2;
		string unusedLocal3;
		string unusedLocal4;
		string unusedLocal5;
		string unusedLocal6;
		string unusedLocal7;
		string unusedLocal8;
		string unusedLocal9;
		ResetWeaponAnimation();
		_weapon = weapon;
		_run = "run";
		_walk = "carbine_walk";
		_idle = "idle";
		_shoot = "carbine_shoot";
		_draw = "carbine_draw";
		_reload = "carbine_reload";
		_reloadStart = string.Empty;
		_reloadCycle = string.Empty;
		_reloadEnd = string.Empty;
		if (_weapon)
		{
			_run = _weapon.AnimationPrefix + "_run";
			_walk = _weapon.AnimationPrefix + "_walk";
			_idle = _weapon.AnimationPrefix + "_idle";
			_shoot = _weapon.AnimationPrefix + "_shoot";
			_draw = _weapon.AnimationPrefix + "_draw";
			_reload = _weapon.AnimationPrefix + "_reload";
			Firearm firearm = _weapon as Firearm;
			if ((bool)firearm)
			{
				_animation[_shoot].speed = _animation[_shoot].length * firearm.RateOfFire;
				if (firearm.NumPellets > 1 && firearm.AnimationPrefix == "shotgun")
				{
					_reload = string.Empty;
					_reloadStart = _weapon.AnimationPrefix + "_reload_start";
					_reloadCycle = _weapon.AnimationPrefix + "_reload_loop";
					_reloadEnd = _weapon.AnimationPrefix + "_reload_end";
				}
			}
			_animation[_draw].time = 0f;
		}
	}

	public bool IsReadyToShoot()
	{
		if (_weapon.IsMelee())
		{
			return true;
		}
		if ((bool)_weapon)
		{
			return _animation[_shoot].weight > 0.99f || _owner.Hidden;
		}
		return false;
	}

	public bool ProceduralOrthopedistEnabled
	{
		get
		{
			return _orthopedistEnabled;
		}
		set
		{
			_orthopedistEnabled = value;
		}
	}

	public bool ProceduralAimingEnabled
	{
		get
		{
			return _aimingEnabled;
		}
		set
		{
			_aimingEnabled = value;
		}
	}

	public bool IsShootingAnimationCompleted
	{
		get
		{
			return _animation[_shoot].time > _animation[_shoot].length * 0.99f || _owner.Hidden;
		}
	}

	public string DrawAnimationName
	{
		get
		{
			return _draw;
		}
	}
}
