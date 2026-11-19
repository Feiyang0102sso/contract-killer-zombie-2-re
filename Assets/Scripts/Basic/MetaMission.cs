public class MetaMission : MonoBehaviourAndLog
{
	public bool isFirstPart { get; set; }

	public Mission.Type missionType { get; set; }

	public float playerHealth { get; set; }

	public string playerWeapon { get; set; }

	public float survivorHealth { get; set; }

	private void Awake()
	{
		LogTo("Game.MetaMission");
		NewMission((Mission.Type)(-1));
	}

	public void NewMission(Mission.Type missionType_)
	{
		isFirstPart = true;
		missionType = missionType_;
		playerHealth = -1f;
		playerWeapon = null;
		survivorHealth = -1f;
	}

	public void EndPart()
	{
		isFirstPart = false;
		if (Loader.IsGameScene() && (bool)Scene.Player)
		{
			playerHealth = Scene.Player.Health;
			playerWeapon = Scene.Player.Inventory.Weapon.name;
		}
		Survivor survivor = Util.FindObjectOfType<Survivor>();
		if ((bool)survivor)
		{
			survivorHealth = survivor.Health;
		}
	}
}
