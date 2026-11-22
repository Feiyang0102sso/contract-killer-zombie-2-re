// PC port: helpers for original code that works "per frame" instead of "per second".
// The iOS game always ran capped at ORI_FPS_LIMIT (QualitySetter sets Application.targetFrameRate = 60), so per-frame
// steps, decays and forces were tuned at that rate. The PC port runs uncapped (see PcLaunchArgs), so those spots are
// rescaled with these helpers to behave at any frame rate exactly as they did at 60 fps.
using UnityEngine;

public static class PcFrameRate
{
	/// <summary>
	/// Frame rate cap of the original iOS game (Application.targetFrameRate = 60). Per-frame game logic was tuned at
	/// this rate; at 165 fps such logic would otherwise run 2.75x as often.
	/// </summary>
	public const float ORI_FPS_LIMIT = 60f;

	/// <summary>How many original 60 fps frames a time step covers: 1 at 60 fps, 0.25 at 240 fps.</summary>
	public static float OriginalFrames(float deltaTime)
	{
		return deltaTime * ORI_FPS_LIMIT;
	}

	/// <summary>
	/// A factor the original applied once per frame (e.g. "*= 0.96" decay), spread over deltaTime:
	/// factor ^ OriginalFrames(deltaTime). Gives the same result per second at any frame rate.
	/// </summary>
	public static float PerFrameFactor(float factor, float deltaTime)
	{
		return Mathf.Pow(factor, OriginalFrames(deltaTime));
	}

	/// <summary>
	/// Real (unscaled) time of this frame, capped at Time.maximumDeltaTime like the original's deltaTime.
	/// Replaces the original "Time.deltaTime / Time.timeScale": on the frame after a load Unity 6 reports the capped
	/// deltaTime of the long frame (0.333) while timeScale is already the pause value 0.001, so that division added
	/// 333 s at once (measured: every launch added 333 s to the save's play time).
	/// </summary>
	public static float RealDeltaTime
	{
		get
		{
			return Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);
		}
	}

	/// <summary>
	/// Converts an amount measured over this frame (e.g. a per-frame scroll step) to the amount over one original frame.
	/// Returns the amount unchanged when deltaTime is 0 (first frame).
	/// </summary>
	public static float PerOriginalFrame(float amountThisFrame, float deltaTime)
	{
		float frames = OriginalFrames(deltaTime);
		if (frames <= 0f)
		{
			return amountThisFrame;
		}
		return amountThisFrame / frames;
	}
}
