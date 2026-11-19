using Glu.Localization;
using UnityEngine;

public class InfectionLevelWarning : MonoBehaviour
{
	public SpriteText Text;

	private void Awake()
	{
		Text.Text = Strings.GetString("Infection level is critical!");
		gameObject.SetActiveRecursively(Singleton.GameData.InfectionLevel.Enabled);
	}

	private void Update()
	{
		gameObject.SetActiveRecursively(Singleton.PlayerData.IsInfectionLevelCritical());
	}
}
