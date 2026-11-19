using UnityEngine;

public class HumanWeapon : MonoBehaviourAndLog
{
	private const float HorizontalThrowVel = 15f;

	private const float VerticalThrowRatio = 0.2f;

	private const float ThrowDistanceToVelocityRatio = 1.5f;

	private GameObject m_weaponModel1;

	private GameObject m_weaponModel2;

	private GameObject m_magazineModel1;

	private GameObject m_magazineModel2;

	private GameObject m_barrel1;

	private GameObject m_barrel2;

	private MotionCapture m_magazine1MC;

	private MotionCapture m_magazine2MC;

	private MotionCapture m_rightHandMC;

	private Weapon m_weapon;

	private static int g_AnimationEventListeners = 0;

	private void Awake()
	{
		m_rightHandMC = this.FindChildInHierarchy("Bip01R_hand_Bone").AddComponent<MotionCapture>();
		LogTo("Char.Human.HumanWeapon");
	}

	private void Start()
	{
		SetupAnimationEvents();
		InstantiateWeaponModel(GetComponent<Human>().Inventory.Weapon);
	}

	private void OnDestroy()
	{
		g_AnimationEventListeners--;
		g_AnimationEventListeners = (!(g_AnimationEventListeners >= 0) ? 0 : g_AnimationEventListeners);
	}

	private void SetupAnimationEvents()
	{
		g_AnimationEventListeners++;
		if (g_AnimationEventListeners > 1)
		{
			return;
		}
		AnimationEvent animationEvent = new AnimationEvent();
		animationEvent.functionName = "OnReloadingAnimationEvent";
		animationEvent.time = 0.6f;
		animationEvent.intParameter = 0;
		animation["carbine_reload"].clip.AddEvent(animationEvent);
		AnimationEvent animationEvent2 = new AnimationEvent();
		animationEvent2.functionName = "OnReloadingAnimationEvent";
		animationEvent2.time = 1.1f;
		animationEvent2.intParameter = 1;
		animation["carbine_reload"].clip.AddEvent(animationEvent2);
		AnimationEvent animationEvent3 = new AnimationEvent();
		animationEvent3.functionName = "OnReloadingAnimationEvent";
		animationEvent3.time = 13f / 30f;
		animationEvent3.intParameter = 0;
		animation["pistols_reload"].clip.AddEvent(animationEvent3);
		AnimationEvent animationEvent4 = new AnimationEvent();
		animationEvent4.functionName = "OnReloadingAnimationEvent";
		animationEvent4.time = 11f / 15f;
		animationEvent4.intParameter = 1;
		animation["pistols_reload"].clip.AddEvent(animationEvent4);
		AnimationEvent animationEvent5 = new AnimationEvent();
		animationEvent5.functionName = "OnReloadingAnimationEvent";
		animationEvent5.time = 1.3f;
		animationEvent5.intParameter = 2;
		animation["pistols_reload"].clip.AddEvent(animationEvent5);
		AnimationEvent animationEvent6 = new AnimationEvent();
		animationEvent6.functionName = "OnReloadingAnimationEvent";
		animationEvent6.time = 1.7333333f;
		animationEvent6.intParameter = 3;
		animation["pistols_reload"].clip.AddEvent(animationEvent6);
		AnimationEvent animationEvent7 = new AnimationEvent();
		animationEvent7.functionName = "OnReloadingAnimationEvent";
		animationEvent7.time = 0.6f;
		animationEvent7.intParameter = 0;
		animation["ams_reload"].clip.AddEvent(animationEvent7);
		AnimationEvent animationEvent8 = new AnimationEvent();
		animationEvent8.functionName = "OnReloadingAnimationEvent";
		animationEvent8.time = 1.1f;
		animationEvent8.intParameter = 1;
		animation["ams_reload"].clip.AddEvent(animationEvent8);
		AnimationEvent animationEvent9 = new AnimationEvent();
		animationEvent9.functionName = "OnThrowAnimationEvent";
		animationEvent9.time = 0f;
		animationEvent9.intParameter = 0;
		animation["throw"].clip.AddEvent(animationEvent9);
		AnimationEvent animationEvent10 = new AnimationEvent();
		animationEvent10.functionName = "OnThrowAnimationEvent";
		animationEvent10.time = 0.2f;
		animationEvent10.intParameter = 1;
		animation["throw"].clip.AddEvent(animationEvent10);
		AnimationEvent animationEvent11 = new AnimationEvent();
		animationEvent11.functionName = "OnThrowAnimationEvent";
		animationEvent11.time = 1f;
		animationEvent11.intParameter = 2;
		animation["throw"].clip.AddEvent(animationEvent11);
		AnimationEvent animationEvent12 = new AnimationEvent();
		animationEvent12.functionName = "OnChamberShell";
		animationEvent12.time = 19f / 30f;
		animationEvent12.intParameter = 0;
		animation["shotgun_shoot"].clip.AddEvent(animationEvent12);
		AnimationEvent animationEvent13 = new AnimationEvent();
		animationEvent13.functionName = "OnLauncherReloadingEvent";
		animationEvent13.time = 4f / 15f;
		animationEvent13.intParameter = 1;
		animation["rpg_reload"].clip.AddEvent(animationEvent13);
		AnimationEvent animationEvent14 = new AnimationEvent();
		animationEvent14.functionName = "OnLauncherReloadingEvent";
		animationEvent14.time = 1.4666667f;
		animationEvent14.intParameter = 0;
		animation["rpg_reload"].clip.AddEvent(animationEvent14);
		AnimationEvent animationEvent15 = new AnimationEvent();
		animationEvent15.functionName = "OnLauncherReloadingEvent";
		animationEvent15.time = 19f / 30f;
		animationEvent15.intParameter = 1;
		animation["Arbalest_reload"].clip.AddEvent(animationEvent15);
		AnimationEvent animationEvent16 = new AnimationEvent();
		animationEvent16.functionName = "OnLauncherReloadingEvent";
		animationEvent16.time = 1.5f;
		animationEvent16.intParameter = 0;
		animation["Arbalest_reload"].clip.AddEvent(animationEvent16);
	}

	private void Update()
	{
	}

	public void OnSwitchWeapon(Weapon weapon)
	{
		Weapon weapon2 = m_weapon;
		InstantiateWeaponModel(weapon);
		if (weapon.AnimationPrefix == "rpg")
		{
			HideAmmoClip(m_magazineModel1);
			weapon.SendMessage("ShowDummyAmmo", weapon.GetAmmoInClip() > 0, SendMessageOptions.RequireReceiver);
		}
		if ((bool)GetComponent<Player>())
		{
			SoundManager.PlayExclusive("Evelyn/Evelyn_weapon_change", false);
		}
		if (weapon != null && weapon2 != null)
		{
			Mission.Type missionType = Mission.Type.Story1;
			int num = -1;
			if (Singleton.PlayerData.HasMission(Singleton.PlayerData.CurrentLocation))
			{
				num = Singleton.PlayerData.GetMission(Singleton.PlayerData.CurrentLocation);
				missionType = Singleton.GameData.Missions[num].Type;
			}
			if (weapon.Owner is Player)
			{
				GameAnalytics.CacheLogWeaponSwitched(missionType, num, Singleton.PlayerData.Level, weapon.gameObject.name, weapon2.gameObject.name);
			}
		}
	}

	private void InstantiateWeaponModel(Weapon weapon)
	{
		if ((bool)m_weapon)
		{
			m_weapon.OnHide();
		}
		m_barrel1 = null;
		m_barrel2 = null;
		if ((bool)m_weaponModel1)
		{
			Object.Destroy(m_weaponModel1);
		}
		if ((bool)m_weaponModel2)
		{
			Object.Destroy(m_weaponModel2);
		}
		if ((bool)m_magazineModel1)
		{
			Object.Destroy(m_magazineModel1);
		}
		if ((bool)m_magazineModel2)
		{
			Object.Destroy(m_magazineModel2);
		}
		m_magazine1MC = null;
		m_magazine2MC = null;
		m_weapon = weapon;
		GetComponent<HumanAnimation>().SetWeapon(weapon);
		if ((bool)weapon && (bool)weapon.Model)
		{
			GameObject gameObject = weapon.InstantiateModel();
			MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				meshRenderer.useLightProbes = true;
			}
			switch (weapon.Operation)
			{
			case Weapon.OperationType.OneHanded:
				m_weaponModel1 = ConnectWeaponModel("weapon1", gameObject);
				m_magazineModel1 = ConnectWeaponModel("magazine1", gameObject);
				if ((bool)m_magazineModel1)
				{
					m_magazine1MC = m_magazineModel1.AddComponent<MotionCapture>();
				}
				m_barrel1 = m_weaponModel1.FindChildInHierarchy("barrel1");
				m_barrel1 = (m_barrel1 ? m_barrel1 : m_magazineModel1.FindChildInHierarchy("barrel1"));
				break;
			case Weapon.OperationType.TwoHanded:
				m_weaponModel1 = ConnectWeaponModel("weapon1", gameObject);
				m_magazineModel1 = ConnectWeaponModel("magazine1", gameObject);
				if ((bool)m_magazineModel1)
				{
					m_magazine1MC = m_magazineModel1.AddComponent<MotionCapture>();
				}
				m_barrel1 = m_weaponModel1.FindChildInHierarchy("barrel1");
				m_barrel1 = (m_barrel1 ? m_barrel1 : m_magazineModel1.FindChildInHierarchy("barrel1"));
				break;
			case Weapon.OperationType.DualWield:
				m_weaponModel1 = ConnectWeaponModel("weapon1", gameObject);
				m_magazineModel1 = ConnectWeaponModel("magazine1", gameObject);
				if ((bool)m_magazineModel1)
				{
					m_magazine1MC = m_magazineModel1.AddComponent<MotionCapture>();
				}
				m_barrel1 = m_weaponModel1.FindChildInHierarchy("barrel1");
				m_barrel1 = (m_barrel1 ? m_barrel1 : m_magazineModel1.FindChildInHierarchy("barrel1"));
				m_weaponModel2 = ConnectWeaponModel("weapon2", gameObject);
				m_magazineModel2 = ConnectWeaponModel("magazine2", gameObject);
				if ((bool)m_magazineModel2)
				{
					m_magazine2MC = m_magazineModel2.AddComponent<MotionCapture>();
				}
				m_barrel2 = m_weaponModel2.FindChildInHierarchy("barrel2");
				m_barrel2 = (m_barrel2 ? m_barrel2 : m_magazineModel2.FindChildInHierarchy("barrel2"));
				break;
			}
			if ((bool)m_weaponModel1)
			{
				Singleton.Visual.ShowOutline(m_weaponModel1, Color.black, 2);
			}
			if ((bool)m_weaponModel2)
			{
				Singleton.Visual.ShowOutline(m_weaponModel2, Color.black, 2);
			}
			weapon.SetupModel(m_weaponModel1, m_weaponModel2);
			Object.Destroy(gameObject);
			base.gameObject.SendMessage("OnPlayDrawAnimation", weapon, SendMessageOptions.DontRequireReceiver);
		}
		if ((bool)m_weapon)
		{
			m_weapon.OnDraw();
		}
	}

	private GameObject ConnectWeaponModel(string dummyName, GameObject source)
	{
		GameObject gameObject = source.FindChildInHierarchy(dummyName);
		GameObject gameObject2 = base.gameObject.FindChildInHierarchy(dummyName);
		if ((bool)gameObject && (bool)gameObject2)
		{
			gameObject.transform.position = gameObject2.transform.position;
			gameObject.transform.eulerAngles = gameObject2.transform.eulerAngles;
			gameObject.transform.parent = gameObject2.transform;
			return gameObject;
		}
		return null;
	}

	private void AnimateAmmoClip(GameObject ammoClipObject, MotionCapture motion)
	{
		MeshRenderer[] componentsInChildren = ammoClipObject.GetComponentsInChildren<MeshRenderer>();
		GameObject gameObject = null;
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (gameObject == null)
			{
				gameObject = Object.Instantiate(ammoClipObject) as GameObject;
				gameObject.transform.parent = null;
				gameObject.transform.position = ammoClipObject.transform.position;
				gameObject.transform.eulerAngles = ammoClipObject.transform.eulerAngles;
				gameObject.transform.parent = Scene.Groups.Ragdolls;
				gameObject.AddComponent<Rigidbody>();
				gameObject.AddComponent<BoxCollider>();
				gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
				gameObject.GetComponent<Rigidbody>().velocity = motion.Velocity();
				gameObject.GetComponent<Rigidbody>().angularVelocity = motion.AngularVelocity();
				gameObject.GetComponent<BoxCollider>().extents = meshRenderer.bounds.extents;
				gameObject.GetComponent<BoxCollider>().center += Vector3.forward * meshRenderer.bounds.extents.z * 0.5f;
				gameObject.GetComponent<BoxCollider>().material.bounciness = 0.1f;
				gameObject.GetComponent<BoxCollider>().material.dynamicFriction = 0.9f;
				gameObject.GetComponent<BoxCollider>().material.bounceCombine = PhysicMaterialCombine.Minimum;
				gameObject.GetComponent<BoxCollider>().material.frictionCombine = PhysicMaterialCombine.Minimum;
				gameObject.layer = Const.LayerRagdoll;
				Singleton.Visual.HideOutline(gameObject);
				RagdollManager.instance.Manage(gameObject);
			}
		}
		foreach (MeshRenderer meshRenderer2 in componentsInChildren)
		{
			meshRenderer2.enabled = false;
		}
	}

	private void HideAmmoClip(GameObject ammoClipObject)
	{
		MeshRenderer[] componentsInChildren = ammoClipObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = false;
		}
	}

	private void ShowAmmoClip(GameObject ammoClipObject)
	{
		MeshRenderer[] componentsInChildren = ammoClipObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = true;
		}
	}

	private void OnReloadingAnimationEvent(int state)
	{
		switch (GetComponent<Human>().Inventory.Weapon.Operation)
		{
		case Weapon.OperationType.OneHanded:
		case Weapon.OperationType.TwoHanded:
			switch (state)
			{
			case 0:
				AnimateAmmoClip(m_magazineModel1, m_magazine1MC);
				break;
			case 1:
				ShowAmmoClip(m_magazineModel1);
				break;
			}
			break;
		case Weapon.OperationType.DualWield:
			switch (state)
			{
			case 0:
				AnimateAmmoClip(m_magazineModel1, m_magazine1MC);
				break;
			case 1:
				ShowAmmoClip(m_magazineModel1);
				break;
			case 2:
				AnimateAmmoClip(m_magazineModel2, m_magazine2MC);
				break;
			case 3:
				ShowAmmoClip(m_magazineModel2);
				break;
			}
			break;
		}
	}

	private void OnLauncherReloadingEvent(int state)
	{
		switch (state)
		{
		case 0:
			HideAmmoClip(m_magazineModel1);
			BroadcastMessage("ShowDummyAmmo", true, SendMessageOptions.RequireReceiver);
			break;
		case 1:
			ShowAmmoClip(m_magazineModel1);
			break;
		}
	}

	private Vector3 AimThrow(Vector3 coneAxis, float coneAngle, out float throwVel)
	{
		throwVel = 15f;
		Vector3 vector = coneAxis;
		vector.y = 0f;
		vector.Normalize();
		float maxAngle = coneAngle;
		Vector3 result = vector;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		foreach (Unit enemy in Scene.Enemies)
		{
			NavMeshAgent component = enemy.GetComponent<NavMeshAgent>();
			Vector3 position = enemy.transform.position;
			Vector3 vector2 = component ? component.velocity : Vector3.zero;
			Vector3 up = Vector3.up;
			Vector3 vector3 = position + up + vector2 * 0.5f - m_rightHandMC.GetPosition();
			RaycastHit[] array = Physics.RaycastAll(m_rightHandMC.GetPosition(), vector3.normalized, vector3.magnitude, Const.LayerMaskObstacles);
			if (array.Length > 0)
			{
				continue;
			}
			vector3.y = 0f;
			float magnitude = vector3.magnitude;
			vector3 *= 1f / magnitude;
			float num3 = Mathf.Abs(Firearm.GetAngle(vector, vector3, Vector3.up));
			if (num3 < maxAngle && num3 * magnitude < num * num2)
			{
				throwVel = magnitude * 1.5f;
				num = num3;
				num2 = magnitude;
				result = vector3;
			}
		}
		return result;
	}

	private void OnThrowAnimationEvent(int state)
	{
		switch (state)
		{
		case 0:
			Util.SetGameObjectVisible(m_weaponModel1, false);
			if ((bool)m_magazineModel1)
			{
				Util.SetGameObjectVisible(m_magazineModel1, false);
			}
			break;
		case 1:
		{
			GameObject objectToThrow = GetComponent<Human>().ObjectToThrow;
			Consumable consumableToThrow = GetComponent<Human>().ConsumableToThrow;
			float throwVel = 15f;
			Vector3 vector = Scene.MainCamera.transform.forward;
			ExplosiveProjectile component = objectToThrow.GetComponent<ExplosiveProjectile>();
			if ((bool)component && component.AutoAiming && !Scene.Mission.IsSurvivalGates())
			{
				vector = AimThrow(Scene.MainCamera.transform.forward, Scene.MainCamera.fov / 2f, out throwVel);
				throwVel = throwVel > 15f ? 15f : throwVel;
			}
			GameObject gameObject = AbstractProjectile.Create(GetComponent<Human>().ObjectToThrow, m_rightHandMC.GetPosition(), vector * throwVel + Vector3.up * throwVel * 0.2f, GetComponent<Human>(), consumableToThrow);
			float unusedAngularVelocity;
			gameObject.rigidbody.angularVelocity = new Vector3(Util.Range(-10f, 10f), 0f, Util.Range(-10f, 10f));
			break;
		}
		case 2:
			Util.SetGameObjectVisible(m_weaponModel1, true);
			if ((bool)m_magazineModel1)
			{
				Util.SetGameObjectVisible(m_magazineModel1, true);
			}
			break;
		}
	}

	private void OnChamberShell(int state)
	{
		if ((bool)m_weapon && m_weapon.GetAmmoInClip() > 0)
		{
			SoundManager.Play("shotgun_reload_end", GetBarrel(0).position, 5f, 25f, false);
		}
	}

	public void ShowWeapon(bool show)
	{
		if ((bool)m_weaponModel1)
		{
			m_weaponModel1.SetActiveRecursively(show);
		}
		if ((bool)m_weaponModel2)
		{
			m_weaponModel2.SetActiveRecursively(show);
		}
		if ((bool)m_magazineModel1)
		{
			m_magazineModel1.SetActiveRecursively(show);
		}
		if ((bool)m_magazineModel2)
		{
			m_magazineModel2.SetActiveRecursively(show);
		}
	}

	public Transform GetBarrel(int weaponIndex)
	{
		switch (weaponIndex)
		{
		case 0:
			return m_barrel1 ? m_barrel1.transform : transform;
		case 1:
			return m_barrel2 ? m_barrel2.transform : transform;
		default:
			return transform;
		}
	}
}
