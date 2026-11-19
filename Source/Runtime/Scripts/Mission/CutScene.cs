using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CutScene : MonoBehaviour
{
	public bool RemoveAllEnemies = true;

	public bool RemoveAllEnemiesAfterwards;

	private readonly List<CutSceneTrack> _tracks = new List<CutSceneTrack>();

	private void OnActivate()
	{
		Util.DoForHierarchy(gameObject, false, (GameObject node) =>
		{
			CutSceneTrack track = node.GetComponent<CutSceneTrack>();
			if (track != null)
			{
				Util.DoForHierarchy(node, false, (GameObject commandNode) =>
				{
					TryAddCommand(commandNode, track);
					return false;
				});
				TryAddTrack(track);
			}
			return false;
		});
		StartCoroutine(CrUpdate());
	}

	private IEnumerator CrUpdate()
	{
		while (!Loading.IsReady || Scene.Player == null || Scene.Camera == null)
		{
			yield return null;
		}
		if (RemoveAllEnemies)
		{
			foreach (Unit enemy in Scene.Enemies)
			{
				Object.Destroy(enemy.gameObject);
			}
		}
		Scene.Player.IgnoreUserInput = true;
		Scene.Player.Speed = 0f;
		foreach (Survivor survivour in Scene.Survivors)
		{
			survivour.EnableAi(false);
		}
		_tracks.ForEach((CutSceneTrack track) =>
		{
			track.Begin();
		});
		while (_tracks.Count > 0)
		{
			HideHud(true);
			for (int i = _tracks.Count - 1; i >= 0; i--)
			{
				if (!_tracks[i].Next())
				{
					_tracks.RemoveAt(i);
				}
			}
			yield return null;
		}
		foreach (Survivor survivour in Scene.Survivors)
		{
			survivour.EnableAi(true);
		}
		HideHud(false);
		Scene.Camera.enabled = true;
		Scene.Player.IgnoreUserInput = false;
		Scene.Player.Speed = 0f;
		TimeControl.Pause(false, TimeControl.PauseTypes.CutScene);
		if (Scene.Camera.HUD != null)
		{
			Scene.Camera.HUD.SetBossName(null);
		}
		if (RemoveAllEnemiesAfterwards)
		{
			foreach (Unit enemy in Scene.Enemies)
			{
				Object.Destroy(enemy.gameObject);
			}
		}
		SendMessageUpwards("OnTaskCompleted");
	}

	private void HideHud(bool hide)
	{
		HudController hUD = Scene.Camera.HUD;
		if (hUD != null)
		{
			hUD.CutSceneMessageOnly(hide);
		}
	}

	private static void TryAddCommand(GameObject node, CutSceneTrack track)
	{
		CutSceneCommand component = node.GetComponent<CutSceneCommand>();
		if (component != null)
		{
			track.AddCommand(component);
		}
	}

	private void TryAddTrack(CutSceneTrack track)
	{
		if (track.IsValid)
		{
			_tracks.Add(track);
		}
	}
}
