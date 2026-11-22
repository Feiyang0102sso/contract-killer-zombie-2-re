// Addon (The Pit): keeps the psycho of The Pit standing still behind his glass (added by PitEnhance).
// He is a generic Survivor, so the Bot AI turned him around: every 15 s an idle look-around, and aiming at zombies
// with his machinegun. In the ending cut-scene he then said his line with his back to Evelyn.
// His rotation in the scene already faces the pool and the glass, so it is kept as it is, and his AI stays off
// (CutScene turns the AI of every survivor back on when a cut-scene ends, so it is switched off again each frame).
using UnityEngine;

public class PitPsychoPose : MonoBehaviour
{
	/// <summary>Rotation from the scene file, facing the pool.</summary>
	private Quaternion _sceneRotation;

	private Survivor _survivor;

	private void Awake()
	{
		_survivor = GetComponent<Survivor>();
		_sceneRotation = transform.rotation;
		// No idle look-around even for the frame the AI might run before LateUpdate
		_survivor.timeBeforeRotation = -1f;
		_survivor.EnableAi(false);
		Debug.Log("[PitPsychoPose] " + name + " held facing " + _sceneRotation.eulerAngles);
	}

	private void LateUpdate()
	{
		_survivor.EnableAi(false);
		transform.rotation = _sceneRotation;
	}
}
