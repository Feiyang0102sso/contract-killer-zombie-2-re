// PC port: cut-scene command that fades the screen to black and waits for it, then (when the command ends) fades back in,
// so the next thing (the mission result popup when this is the last task) appears out of the dark.
// Uses the game's own screen fader (Singleton.UI.Fader, the one level loading uses). Used by PcPitEnhance.
using UnityEngine;

public sealed class PcCutSceneFadeOut : CutSceneCommand
{
	/// <summary>Seconds to fade to black.</summary>
	public float FadeToBlackSeconds = 1.5f;

	/// <summary>Seconds to fade back in afterwards. Kept short: Loader ignores level loads while the fader runs.</summary>
	public float FadeBackSeconds = 1f;

	private bool _done;

	public override void Begin()
	{
		_done = false;
		SpriteFader fader = Singleton.UI.Fader;
		fader.OnFadeComplete = OnFadeComplete;
		fader.FadeIn(FadeToBlackSeconds, Color.black);
		UnityEngine.Debug.Log("[PcCutSceneFadeOut] fading to black over " + FadeToBlackSeconds + " s");
	}

	private void OnFadeComplete(SpriteFader.State state)
	{
		Singleton.UI.Fader.OnFadeComplete = null;
		_done = true;
	}

	public override bool Next()
	{
		return !_done;
	}

	public override void End()
	{
		Singleton.UI.Fader.FadeOut(FadeBackSeconds);
		UnityEngine.Debug.Log("[PcCutSceneFadeOut] black reached, fading back in over " + FadeBackSeconds + " s");
	}
}
