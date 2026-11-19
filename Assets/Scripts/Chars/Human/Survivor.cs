using Glu.Localization;
using UnityEngine;

public class Survivor : Bot
{
	public string HordeSound;

	public string ResqueSound;

	protected override void Awake()
	{
		base.Awake();
		GameObject gameObject = this.QuickFindChild("RescueArea");
		if (!gameObject)
		{
			gameObject = Object.Instantiate(Resources.Load("RescueArea")) as GameObject;
			gameObject.RemoveClonePostfix();
			gameObject.transform.parent = transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
		}
		if (Singleton.MetaMission.survivorHealth >= 0f)
		{
			health = Singleton.MetaMission.survivorHealth;
		}
	}

	protected override void Start()
	{
		base.Start();
		foreach (HumanInventory.StartWeapon startWeapon in Inventory.startWeapons)
		{
			if (startWeapon.Name.Length == 0)
			{
				continue;
			}
			Inventory.AddEquippable(startWeapon.Name, startWeapon.Slot);
		}
		Inventory.SetAnyCurrentWeapon();
	}

	protected override void Update()
	{
		float unusedLocal1;
		float unusedLocal2;
		if (mode != Mode.FollowCharacter && (!gameObject.GetParent() || !gameObject.GetParent().GetComponent<PeriodicMovement>()) && ResqueSound.Length > 0 && !SoundManager.IsPlaying(ResqueSound) && Util.ChancePerTime(50f, this, "BOT_CRY", 15f))
		{
			SoundManager.PlayExclusive(ResqueSound, transform.position, 5f, 25f, false);
		}
		base.Update();
	}

	private void OnEnterArea(AreaVisitInfo info)
	{
		if (info.visitor is Player && info.area.name == "RescueArea")
		{
			mode = Mode.FollowCharacter;
			info.area.active = false;
			Object.Destroy(info.area);
			if (Singleton.Tutorial == null)
			{
				Report(Util.RandomFrom<string>(Strings.GetString("Thanks for saving me!"), Strings.GetString("Finally someone saved me!"), Strings.GetString("Oh, another survivor!"), Strings.GetString("You are not zombie, are you?")));
			}
		}
	}

	public override bool CanBeHurt()
	{
		return !Singleton.Settings.InvulnerableSurvivor && base.CanBeHurt();
	}

	public void OnWaveStarted()
	{
		float unusedLocal1;
		float unusedLocal2;
		if (HordeSound.Length > 0 && Util.ChancePerTime(50f, this, "BOT_BEWARE", 40f))
		{
			Invoke("PlayHordeSound", Random.Range(2f, 3f));
		}
	}

	private void PlayHordeSound()
	{
		SoundManager.PlayExclusive(HordeSound, transform.position, 5f, 25f, false);
	}
}
