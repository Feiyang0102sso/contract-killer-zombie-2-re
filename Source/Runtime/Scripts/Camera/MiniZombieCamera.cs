using System;
using UnityEngine;

public class MiniZombieCamera : MonoBehaviour
{
	public enum ECharacterState
	{
		Stand,
		StandAim,
		Run,
		RunAim,
		ZoomMode
	}

	[Serializable]
	public class StateDependentParam
	{
		public float standVal;

		public float standAimVal;

		public float runVal;

		public float runAimVal;

		public float zoomVal;

		public StateDependentParam(float stand, float run)
			: this(stand, stand, run, run, stand)
		{
		}

		public StateDependentParam(float stand, float standAim, float run, float runAim)
			: this(stand, standAim, run, runAim, standAim)
		{
		}

		public StateDependentParam(float stand, float standAim, float run, float runAim, float zoom)
		{
			standVal = stand;
			standAimVal = standAim;
			runVal = run;
			runAimVal = runAim;
			zoomVal = zoom;
		}

		public float GetValue(ECharacterState state)
		{
			switch (state)
			{
			case ECharacterState.Stand:
				return standVal;
			case ECharacterState.StandAim:
				return standAimVal;
			case ECharacterState.Run:
				return runVal;
			case ECharacterState.RunAim:
				return runAimVal;
			case ECharacterState.ZoomMode:
			default:
				return zoomVal;
			}
		}
	}

	public enum Effect
	{
		Normal,
		Dying,
		Resurrect,
		Charger,
		Bomber,
		DamageBoost,
		LowHealth,
		Count
	}

	private struct TargetedRotation
	{
		public bool enabled;

		public float targetAngleY;

		public Transform targetTransform;

		public float lastAngleWithTarget;
	}

	public const float collidedMovementDampingDuration = 2f;

	public const float maxMovementDampingDistance = 5f;

	public const float freeRunRotateCoeff = 3f;

	private const float addDistanceForDying = 4f;

	public Vector3 targetCenterOffset = Vector3.zero;

	public float baseDistance = 1.1f;

	public float baseHeight = 0.25f;

	public StateDependentParam heightFromSpeedCoeff = new StateDependentParam(1f, 1f, 1f, 1f);

	public StateDependentParam distanceFromSpeedCoeff = new StateDependentParam(1f, 1f, 1f, 1f);

	public StateDependentParam fov = new StateDependentParam(50f, 33.3f, 50f, 50f);

	public StateDependentParam rightOffset = new StateDependentParam(0.5f, 0.5f, 0.5f, 0.5f);

	public StateDependentParam maxPitchAngle = new StateDependentParam(15f, 15f, 15f, 15f);

	public StateDependentParam minPitchAngle = new StateDependentParam(-25f, -10f, -10f, -10f);

	public StateDependentParam pitchHeightModifier = new StateDependentParam(0.075f, 0.075f, 0.075f, 0.075f);

	public StateDependentParam pitchDistanceModifier = new StateDependentParam(0f, 0f, 0f, 0f);

	public StateDependentParam sensivityX = new StateDependentParam(0.33f, 0.33f);

	public StateDependentParam sensivityY = new StateDependentParam(0.33f, 0.33f);

	public float distanceDamping = 15f;

	public float fovDamping = 15f;

	public float pitchAngleClampDamping = 10f;

	public float pitchHeightDamping = 10f;

	public float meleeDistanceCoeff = 3f;

	public float meleeHeightCoeff = 1.5f;

	public float characterRotationDamping = 20f;

	public float angleYOffset = -10f;

	public float targetedRotationDamping = 10f;

	public float collidedMovementDamping = 2f;

	public float horseRotationDamping = 1f;

	public float magicAngle = 45f;

	[NonSerialized]
	public Effect CameraEffect;

	public float sightSpreadScaleCoeff = 4f;

	public float sightSize = 15f;

	public float sightThickness = 2f;

	public float sightLength = 7.5f;

	public float sightOnShootScaleCoeff = 5f;

	public float sightOnShootScaleCoeffDamping = 7f;

	public Color sightColorBase = new Color(1f, 1f, 1f, 0.5f);

	public Color sightColorWantToShoot = new Color(1f, 1f, 1f, 0.5f);

	public Color sightColorAimed = new Color(1f, 0f, 0f, 0.5f);

	public Color sightColorInfo = new Color(0.4f, 0.4f, 0.4f, 1f);

	private CountdownTimer _onEnableDamping = new CountdownTimer();

	public Material sightMaterial;

	private Transform _target;

	private Player _playerController;

	private float _distance;

	private float _height;

	private float _fov;

	private float _rightOffset;

	private float _maxPitchAngle;

	private float _minPitchAngle;

	private float _pitchHeightModifier;

	private float _pitchDistanceModifier;

	private float _cameraAngleX;

	private float _cameraAngleY;

	private bool _angleXLockEnabled;

	private float _angleXLockFrom;

	private float _angleXLockTo;

	private bool _angleYLockEnabled;

	private float _angleYLockFrom;

	private float _angleYLockTo;

	private float _characterRelativeMoveSpeed;

	private float _sightAdditionalSpreadCoeff = 1f;

	private ECharacterState _characterState;

	private HudController _hudController;

	private ChromaticAberrationWithVignetting _chromaticAberrationEffect;

	private LowHealth _lowHealthEffect;

	private MobileBloom _bloomEffect;

	private PostEffectManager _postEffectMan;

	private TargetedRotation _targetedRotation;

	private CharacterController _collisionTester;

	private CountdownTimer _moveDampingEnabledCountdownTimer = new CountdownTimer();

	private CountdownTimer _shakingTimer;

	private float _shakingAmplitude;

	private float _shakingFrequency;

	private float _shakingDecrease;

	private Vector2 _shakingPhase = Vector2.zero;

	private float _shakingAngle;

	private CountdownTimer _flinchTimer;

	private Vector3 _flinchAngles;

	private AudioLowPassFilter _lowPassFilter;

	private CountdownTimer _lowPassTimer;

	private float _lowPassFrequency = 22000f;

	public HudController HUD
	{
		get
		{
			return _hudController;
		}
	}

	public float distance
	{
		get
		{
			if (_characterState == ECharacterState.ZoomMode)
			{
				return 0f;
			}
			if ((bool)_playerController && _playerController.Inventory.Weapon.IsMelee())
			{
				return baseDistance * meleeDistanceCoeff;
			}
			return baseDistance * (1f + _characterRelativeMoveSpeed * distanceFromSpeedCoeff.GetValue(_characterState)) + (float)(((bool)_playerController && _playerController.IsDyingOrAppearing()) ? 1 : 0) * 4f;
		}
	}

	public float height
	{
		get
		{
			if (_characterState == ECharacterState.ZoomMode)
			{
				return 0.5f;
			}
			if ((bool)_playerController && _playerController.Inventory.Weapon.IsMelee())
			{
				return baseHeight * meleeHeightCoeff;
			}
			return baseHeight * (1f + _characterRelativeMoveSpeed * heightFromSpeedCoeff.GetValue(_characterState));
		}
	}

	public float GetCurrentFov()
	{
		if (_characterState == ECharacterState.ZoomMode)
		{
			return _playerController.Inventory.Weapon.ZoomModeFov;
		}
		return fov.GetValue(_characterState);
	}

	public void ResetOnEnableDamping()
	{
		_onEnableDamping.Reset();
	}

	public void AddRotationDelta(Vector2 delta)
	{
	}

	private void Awake()
	{
		_targetedRotation.enabled = false;
		_chromaticAberrationEffect = GetComponent<ChromaticAberrationWithVignetting>();
		_lowHealthEffect = GetComponent<LowHealth>();
		_bloomEffect = GetComponent<MobileBloom>();
		_postEffectMan = GetComponent<PostEffectManager>();
		_distance = distance;
		_height = height;
		_rightOffset = rightOffset.GetValue(_characterState);
		_fov = fov.GetValue(_characterState);
		_maxPitchAngle = maxPitchAngle.GetValue(_characterState);
		_minPitchAngle = minPitchAngle.GetValue(_characterState);
		_pitchHeightModifier = pitchHeightModifier.GetValue(_characterState);
		_pitchDistanceModifier = pitchDistanceModifier.GetValue(_characterState);
		Singleton.MakeSureItExists();
		float[] array = new float[32];
		for (int i = 0; i < 32; i++)
		{
			array[i] = GetComponent<Camera>().farClipPlane;
		}
		array[Const.LayerDecal] = Singleton.Quality.DecalLayerCullDistance;
		array[Const.LayerEyeCandy] = Singleton.Quality.EyeCandyLayerCullDistance;
		GetComponent<Camera>().layerCullDistances = array;
		foreach (EZCameraSettings eZCameraSettings in UIManager.instance.uiCameras)
		{
			AudioListener component = eZCameraSettings.camera.GetComponent<AudioListener>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
	}

	public void LookAt(Transform newTarget)
	{
		_target = newTarget;
		if (!((bool)_target))
		{
			return;
		}
		_cameraAngleY = _target.rotation.eulerAngles.y + angleYOffset;
		transform.rotation = Quaternion.Euler(_cameraAngleX, _cameraAngleY, 0f);
		_collisionTester = GetComponent<CharacterController>();
		_playerController = _target.GetComponent<Player>();
		transform.position = _playerController.transform.position + -_playerController.transform.forward * 2f + Vector3.up * 2f;
	}

	private void UpdateCharacterInfo()
	{
		ECharacterState characterState = _characterState;
		_characterRelativeMoveSpeed = _playerController.GetRelativeSpeed();
		bool wantToShoot = _playerController.WantToShoot;
		bool flag = _playerController.IsZoomMode();
		if (_playerController.IsDyingOrAppearing())
		{
			_characterState = ECharacterState.Stand;
		}
		else if (flag)
		{
			_characterState = ECharacterState.ZoomMode;
		}
		else if (_characterRelativeMoveSpeed < 0.001f)
		{
			_characterState = (wantToShoot ? ECharacterState.StandAim : ECharacterState.Stand);
		}
		else
		{
			_characterState = (wantToShoot ? ECharacterState.RunAim : ECharacterState.Run);
		}
		if (_characterState != characterState)
		{
			OnCharacterStateChanged(characterState, _characterState);
		}
	}

	private void OnCharacterStateChanged(ECharacterState before, ECharacterState now)
	{
		if (before == ECharacterState.ZoomMode || now == ECharacterState.ZoomMode)
		{
			ChangeFovInstant();
		}
	}

	public void ChangeFovInstant()
	{
		_fov = GetCurrentFov();
	}

	private void Start()
	{
		_hudController = Singleton.UI.Hud;
	}

	private void LateUpdate()
	{
		if (_target == null || TimeControl.IsPaused())
		{
			return;
		}
		UpdateCharacterInfo();
		Vector2 vector = _hudController.GetCameraRotationDeltaAndReset();
		if (_targetedRotation.enabled)
		{
			vector = Vector2.zero;
		}
		float cameraSensitivity = Singleton.Settings.GetCameraSensitivity();
		float num = Mathf.Tan(_fov * ((float)Math.PI / 180f) * 0.5f) / Mathf.Tan(fov.GetValue(ECharacterState.Stand) * ((float)Math.PI / 180f) * 0.5f);
		float num2 = vector.x * sensivityX.GetValue(_characterState) * num * cameraSensitivity;
		float num3 = vector.y * sensivityY.GetValue(_characterState) * num * cameraSensitivity;
		bool flag = _characterState == ECharacterState.ZoomMode;
		_distance = flag ? distance : Mathf.Lerp(_distance, distance, Time.deltaTime * distanceDamping);
		_height = flag ? height : Mathf.Lerp(_height, height, Time.deltaTime * distanceDamping);
		_rightOffset = flag ? rightOffset.GetValue(_characterState) : Mathf.Lerp(_rightOffset, rightOffset.GetValue(_characterState), Time.deltaTime * distanceDamping);
		_fov = Mathf.Lerp(_fov, GetCurrentFov(), Time.deltaTime * fovDamping);
		_minPitchAngle = Mathf.Lerp(_minPitchAngle, minPitchAngle.GetValue(_characterState), Time.deltaTime * pitchAngleClampDamping);
		_maxPitchAngle = Mathf.Lerp(_maxPitchAngle, maxPitchAngle.GetValue(_characterState), Time.deltaTime * pitchAngleClampDamping);
		_pitchHeightModifier = Mathf.Lerp(_pitchHeightModifier, pitchHeightModifier.GetValue(_characterState), Time.deltaTime * pitchHeightDamping);
		_pitchDistanceModifier = Mathf.Lerp(_pitchDistanceModifier, pitchDistanceModifier.GetValue(_characterState), Time.deltaTime * pitchHeightDamping);
		_sightAdditionalSpreadCoeff = Mathf.Lerp(_sightAdditionalSpreadCoeff, 1f, Time.deltaTime * sightOnShootScaleCoeffDamping);
		_cameraAngleX = _cameraAngleX - num3;
		_cameraAngleX = Mathf.Clamp(_cameraAngleX, _minPitchAngle, _maxPitchAngle);
		_cameraAngleY = _cameraAngleY + num2;
		float num4 = _target.transform.eulerAngles.y;
		if (_targetedRotation.enabled)
		{
			_cameraAngleY = Mathf.Lerp(_cameraAngleY, _targetedRotation.targetAngleY, Time.deltaTime * targetedRotationDamping);
			num4 = _cameraAngleY - angleYOffset;
			if (Mathf.Abs(_cameraAngleY - _targetedRotation.targetAngleY) < 0.1f)
			{
				_targetedRotation.enabled = false;
			}
			else if (Mathf.Abs(_cameraAngleY - _targetedRotation.targetAngleY) < 10f && vector.sqrMagnitude > 10.01f)
			{
				_targetedRotation.enabled = false;
			}
			if (_targetedRotation.targetTransform != null)
			{
				Vector3 vector2 = _targetedRotation.targetTransform.position - transform.position;
				Vector2 vector3 = new Vector2(vector2.x, vector2.z);
				float target = Vector2.Angle(vector3, Vector2.up) * Mathf.Sign(Vector3.Dot(vector3.normalized, Vector2.right));
				float f = Mathf.DeltaAngle(_cameraAngleY, target);
				float num5 = Mathf.Abs(f);
				if (num5 - _targetedRotation.lastAngleWithTarget > 0f || num5 < 2f)
				{
					_targetedRotation.enabled = false;
				}
				_targetedRotation.lastAngleWithTarget = num5;
			}
		}
		if (_characterState == ECharacterState.RunAim || _characterState == ECharacterState.StandAim || _characterState == ECharacterState.ZoomMode)
		{
			num4 = Mathf.LerpAngle(num4, _cameraAngleY - angleYOffset, Time.deltaTime * characterRotationDamping);
		}
		Vector3 eulerAngles = _target.transform.rotation.eulerAngles;
		eulerAngles.y = num4;
		_target.transform.eulerAngles = eulerAngles;
		if (_angleXLockEnabled)
		{
			_cameraAngleX = Mathf.Clamp(_cameraAngleX, _angleXLockFrom, _angleXLockTo);
		}
		if (_angleYLockEnabled)
		{
			_cameraAngleY = Mathf.Clamp(_cameraAngleY, _angleYLockFrom, _angleYLockTo);
		}
		Vector3 eulerAngles2 = transform.rotation.eulerAngles;
		eulerAngles2.y = _cameraAngleY;
		eulerAngles2.x = _cameraAngleX;
		if (_flinchTimer != null)
		{
			float num6 = _flinchTimer.Progress();
			float unusedFlinchValue;
			float num7 = num6 < 0.1f ? num6 / 0.1f : Mathf.Lerp(1f, 0f, (num6 - 0.1f) / 0.9f);
			eulerAngles2 += _flinchAngles * num7;
			if (!_flinchTimer.IsRunning())
			{
				_flinchTimer = null;
			}
		}
		float num8 = NormAngle(eulerAngles2.x) / 90f;
		float num9 = 1f + _pitchHeightModifier * num8;
		float num10 = 1f + _pitchDistanceModifier * Mathf.Abs(num8);
		transform.eulerAngles = eulerAngles2;
		Vector3 vector4 = _target.position + _target.TransformDirection(targetCenterOffset) + transform.right * _rightOffset;
		Vector3 normalized = Util.ZeroY(-transform.forward).normalized;
		Vector3 newPosition = vector4;
		newPosition += Vector3.up * _height * num9;
		newPosition += normalized * _distance * num10;
		newPosition = AdjustLineOfSight(newPosition);
		if (_shakingTimer != null)
		{
			float num11 = _shakingAmplitude * Mathf.Pow(1f - _shakingTimer.Progress(), _shakingDecrease);
			float unusedShakeValue1;
			float unusedShakeValue2;
			float unusedShakeValue3;
			newPosition += transform.right * Mathf.Cos(_shakingAngle) * num11 * Mathf.Sin(_shakingPhase.x + _shakingFrequency * (_shakingTimer.TotalTime() - _shakingTimer.LeftTime()));
			newPosition += transform.up * Mathf.Sin(_shakingAngle) * num11 * Mathf.Sin(_shakingPhase.y + _shakingFrequency * (_shakingTimer.TotalTime() - _shakingTimer.LeftTime()));
			if (!_shakingTimer.IsRunning())
			{
				_shakingTimer = null;
			}
		}
		if (_lowPassTimer != null)
		{
			float num12 = _lowPassTimer.TotalTime() / 2f;
			float num13 = 22000f;
			if (_lowPassTimer.LeftTime() > num12)
			{
				float value = (_lowPassTimer.LeftTime() - num12) / num12;
				value = Mathf.Clamp01(value);
				value = Mathf.Pow(value, 3f);
				num13 = _lowPassFrequency * (1f - value) + 22000f * value;
			}
			else
			{
				float value2 = _lowPassTimer.LeftTime() / num12;
				value2 = Mathf.Clamp01(value2);
				value2 = Mathf.Pow(value2, 2f);
				num13 = _lowPassFrequency * value2 + 22000f * (1f - value2);
			}
			_lowPassFilter.cutoffFrequency = num13;
			if (!_lowPassTimer.IsRunning())
			{
				_lowPassFilter.enabled = false;
				_lowPassFilter = null;
				_lowPassTimer = null;
			}
		}
		if ((transform.position - newPosition).sqrMagnitude > 25f)
		{
			_moveDampingEnabledCountdownTimer.Reset();
		}
		if (_moveDampingEnabledCountdownTimer.IsRunning())
		{
			float num14 = 1f + _moveDampingEnabledCountdownTimer.Progress() * 5f;
			transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * collidedMovementDamping * num14);
		}
		else if (_onEnableDamping.IsRunning())
		{
			transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 2f);
		}
		else
		{
			transform.position = newPosition;
		}
		GetComponent<Camera>().fieldOfView = _fov;
		if (_playerController.Health < 200f)
		{
			_postEffectMan.OnLowHealth(true);
		}
		else
		{
			_postEffectMan.OnLowHealth(false);
		}
		CheckPlayerHide();
	}

	private void OnEnable()
	{
		_onEnableDamping.Run(2.4f);
		if (_target != null)
		{
			_cameraAngleY = _target.rotation.eulerAngles.y + angleYOffset;
		}
	}

	private static float NormAngle(float angle)
	{
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	private void CheckPlayerHide()
	{
	}

	public void AutoRotate180()
	{
		Vector3 forward = transform.forward;
		Vector3 position = _playerController.transform.position;
		float f = 180f;
		Transform target = null;
		foreach (Unit item in Scene.Nearby<Unit>(position, 15f))
		{
			Vector3 lhs = item.transform.position - position;
			float num = Vector2.Angle(new Vector2(forward.x, forward.z), new Vector2(lhs.x, lhs.z)) * Mathf.Sign(Vector3.Dot(lhs, transform.right));
			if (Mathf.Abs(num) < Mathf.Abs(f) && Mathf.Abs(num) > 90f)
			{
				f = num;
				target = item.transform;
			}
		}
		AutoRotate(180f * Mathf.Sign(f), target);
	}

	public void AutoRotate(float angle, Transform target)
	{
		if (!(!_targetedRotation.enabled))
		{
			return;
		}
		_targetedRotation.targetTransform = target;
		_targetedRotation.targetAngleY = _cameraAngleY + angle;
		_targetedRotation.lastAngleWithTarget = 180f;
		_targetedRotation.enabled = true;
	}

	private Vector3 AdjustLineOfSight(Vector3 newPosition)
	{
		if (_collisionTester != null)
		{
			Vector3 position = _collisionTester.transform.position;
			Vector3 vector = _target.gameObject.transform.position + Vector3.up;
			_collisionTester.transform.position = vector;
			Vector3 desiredPosition = newPosition;
			_collisionTester.Move(desiredPosition - vector);
			Vector3 position2 = _collisionTester.transform.position;
			_collisionTester.transform.position = transform.position;
			_collisionTester.Move(position2 - _collisionTester.transform.position);
			Vector3 position3 = _collisionTester.transform.position;
			_collisionTester.transform.position = position;
			if (position3 != newPosition)
			{
				_moveDampingEnabledCountdownTimer.Run(2f);
			}
			return position3;
		}
		return newPosition;
	}

	public void SetEffect(PostEffectManager.PostEffect _effect)
	{
		SetEffect(_effect, 0f);
	}

	public void SetEffect(PostEffectManager.PostEffect _effect, float duration)
	{
		_postEffectMan.SetPostEffect(_effect, duration);
	}

	private void OnPostRender()
	{
		if (!_playerController)
		{
			return;
		}
		float num = Screen.width / 2;
		float num2 = Screen.height / 2;
		bool flag = _playerController.IsVictimLocked;
		bool wantToShoot = _playerController.WantToShoot;
		if (Singleton.Settings.MiniZombieControls)
		{
			flag = wantToShoot;
		}
		Weapon weapon = _playerController.Inventory.Weapon;
		if (!weapon)
		{
			return;
		}
		if (_hudController && _hudController.IsScopeShowing())
		{
			return;
		}
		float spreadAngle = weapon.GetSpreadAngle();
		float num3 = GetComponent<Camera>().pixelWidth / GetComponent<Camera>().fieldOfView;
		sightSize = spreadAngle * num3 * _sightAdditionalSpreadCoeff;
		float num4 = Mathf.Sqrt((spreadAngle < sightLength ? sightLength : spreadAngle) * num3 * _sightAdditionalSpreadCoeff);
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		sightMaterial.SetPass(0);
		GL.Begin(7);
		if (weapon.IsAmmoEmpty())
		{
			GL.Color(Color.Lerp(sightColorBase, sightColorAimed, (Mathf.Sin(TimeControl.GameTime * 12f) + 1f) * 0.5f));
		}
		else if ((bool)_hudController && _hudController.IsShowingCursorInfo())
		{
			GL.Color(sightColorInfo);
		}
		else if (flag)
		{
			GL.Color(sightColorAimed);
		}
		else if (wantToShoot)
		{
			GL.Color(sightColorWantToShoot);
		}
		else
		{
			GL.Color(sightColorBase);
		}
		float num5 = sightThickness * 0.5f;
		float num6 = sightSize * 0.5f;
		GL.Vertex3(num - num6 - num4, num2 - num5, 0f);
		GL.Vertex3(num - num6 - num4, num2 + num5, 0f);
		GL.Vertex3(num - num6, num2 + num5, 0f);
		GL.Vertex3(num - num6, num2 - num5, 0f);
		GL.Vertex3(num + num6, num2 - num5, 0f);
		GL.Vertex3(num + num6, num2 + num5, 0f);
		GL.Vertex3(num + num6 + num4, num2 + num5, 0f);
		GL.Vertex3(num + num6 + num4, num2 - num5, 0f);
		GL.Vertex3(num - num5, num2 - num6 - num4, 0f);
		GL.Vertex3(num - num5, num2 - num6, 0f);
		GL.Vertex3(num + num5, num2 - num6, 0f);
		GL.Vertex3(num + num5, num2 - num6 - num4, 0f);
		GL.Vertex3(num - num5, num2 + num6, 0f);
		GL.Vertex3(num - num5, num2 + num6 + num4, 0f);
		GL.Vertex3(num + num5, num2 + num6 + num4, 0f);
		GL.Vertex3(num + num5, num2 + num6, 0f);
		GL.End();
		GL.PopMatrix();
	}

	public void OnWeaponShot()
	{
		_sightAdditionalSpreadCoeff = sightOnShootScaleCoeff;
	}

	public void OnExplosion(Vector3 epicenter)
	{
		float magnitude = (epicenter - transform.position).magnitude;
		float unusedLocal1;
		float unusedLocal2;
		float unusedLocal3;
		float unusedLocal4;
		float unusedLocal5;
		float unusedLocal6;
		float num = Mathf.Clamp01(magnitude / 25f);
		float amplitude = 0.3f * (1f - num) + 0.03f * num;
		float frequency = 40f * (1f - num) + 120f * num;
		Shake(0.5f, amplitude, frequency, 3f);
	}

	public void OnCharger(bool blur)
	{
		Shake(1f, 0.08f, 50f, 3f);
		if (blur)
		{
			SetEffect(PostEffectManager.PostEffect.Charger);
		}
	}

	public void OnBomber()
	{
		Shake(2f, 0.3f, 5f, 2f);
		SetEffect(PostEffectManager.PostEffect.Bomber);
	}

	public void OnDamageBoost(float duration)
	{
		Shake(duration, 0f, 1f, 1f);
		SetEffect(PostEffectManager.PostEffect.DamageBoost, duration);
	}

	public void Shake(float time, float amplitude, float frequency, float decrease)
	{
		_shakingTimer = new CountdownTimer(time);
		_shakingAmplitude = amplitude;
		_shakingFrequency = frequency;
		_shakingDecrease = decrease;
		_shakingAngle = Util.Range(0f, (float)Math.PI * 2f);
	}

	public void Flinch(float time, Vector3 angles)
	{
		_flinchTimer = new CountdownTimer(time);
		_flinchAngles = angles;
	}

	public void SetLowPassFilter(float frequency, float effectTime)
	{
		float unusedLocal1;
		float unusedLocal2;
		if (Singleton.Quality.AudioLowPassFilterEnabled)
		{
			_lowPassFilter = GetComponent<AudioLowPassFilter>();
			if ((bool)_lowPassFilter)
			{
				_lowPassTimer = new CountdownTimer(effectTime);
				_lowPassFrequency = Mathf.Clamp(frequency, 10f, 22000f);
				_lowPassFilter.cutoffFrequency = 22000f;
				_lowPassFilter.enabled = true;
			}
			else
			{
				Debug.LogError("Kein Tiefpassfilter Kamera gefunden!");
			}
		}
	}

	public void SetAngleYLock(bool enabled)
	{
		SetAngleYLock(enabled, 0f, 0f);
	}

	public void SetAngleXLock(bool enabled, float angleXFrom, float angleXTo)
	{
		_angleXLockEnabled = enabled;
		_angleXLockFrom = angleXFrom;
		_angleXLockTo = angleXTo;
	}

	public void SetAngleYLock(bool enabled, float angleYFrom, float angleYTo)
	{
		_angleYLockEnabled = enabled;
		_angleYLockFrom = angleYFrom;
		_angleYLockTo = angleYTo;
	}
}
