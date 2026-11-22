// PC port: path fallback for bots (survivors) standing off the NavMesh.
// Bot.UpdateAi repaths from its own position every 0.5 s and, when NavMesh.CalculatePath fails, just stops (Reset) and
// tries again later. CalculatePath fails when the start point is not within the query extents of the NavMesh, so a bot
// pushed a little off the rebuilt NavMesh (next to props, in a carved hole) never moves again (a survivor in
// Outdoor_Square froze next to a barrel, 2026-11-20). The rebuilt NavMesh is not the 3.5 one, so this happens where the
// original did not. Fallback: snap the start (and target) to the nearest NavMesh point and path from there; the bot
// walks back onto the NavMesh along that path.
using UnityEngine;

public static class NewUnityNavCompat
{
	/// <summary>Seconds between fallback log lines per bot.</summary>
	private const float LogInterval = 5f;

	/// <summary>Path from the NavMesh point nearest to bot (within Util.SamplePosition's 5 m) to target. False if none.</summary>
	public static bool FindPathFromNearestNavMesh(Character bot, Vector3 target, out UnityEngine.AI.NavMeshPath path)
	{
		bool found = Util.FindPath(bot.transform.position, target, out path, true, true);
		if (Util.DoRarely(bot, "pc_nav_fallback_log", LogInterval))
		{
			UnityEngine.AI.NavMeshHit hit;
			string distance = "none within 5 m";
			if (UnityEngine.AI.NavMesh.SamplePosition(bot.transform.position, out hit, 5f, -1))
			{
				distance = hit.distance.ToString("F2") + " m";
			}
			string result = "not found";
			if (found)
			{
				result = "found";
			}
			Debug.Log("[NewUnityNavCompat] '" + bot.name + "' at " + bot.transform.position.ToString("F2") + ": direct path failed (nearest NavMesh "
				+ distance + "), fallback path " + result);
		}
		return found;
	}
}
