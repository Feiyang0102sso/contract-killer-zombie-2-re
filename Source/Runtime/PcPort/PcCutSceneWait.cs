// PC port: cut-scene command that holds the current shot for a while (the original command set has no wait;
// a MoveCamera to the same spot takes no time). Real time, like the other commands. Used by PcPitEnhance.
using UnityEngine;

public sealed class PcCutSceneWait : CutSceneCommand
{
	/// <summary>Seconds to wait.</summary>
	public float Seconds = 1f;

	private float _endTime;

	public override void Begin()
	{
		_endTime = TimeControl.RealTime + Seconds;
	}

	public override bool Next()
	{
		return TimeControl.RealTime < _endTime;
	}
}
