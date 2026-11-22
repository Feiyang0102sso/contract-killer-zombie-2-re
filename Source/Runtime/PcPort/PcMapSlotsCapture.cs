// PC port test tool: renders the whole city map with a mission icon on every MapLocation (every spot a mission can appear),
// plus a strip with every mission icon template, and writes the spot list as JSON. Triggered by -pcMapSlots (PcBootDiagnostics).
// Only changes the live map objects; nothing is saved. Restart the game afterwards.
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class PcMapSlotsCapture
{
	/// <summary>Output pixels per map world unit.</summary>
	private const float PixelsPerUnit = 8f;

	/// <summary>Empty border around the map bounds, in world units.</summary>
	private const float BorderUnits = 12f;

	/// <summary>Largest texture side allowed.</summary>
	private const int MaxTextureSide = 8192;

	/// <summary>Gap between icons in the template strip, in world units.</summary>
	private const float StripSpacing = 40f;

	/// <summary>Map decorations left out of the capture (the hands holding the map cover some spots, the torch spot darkens the edges).</summary>
	private static readonly string[] HiddenDecorations = { "_LeftHand", "_LeftHandReflex", "_RightHand", "_LightSpot" };

	/// <summary>Building highlight sprites under the map that have no MapLocation.</summary>
	private static readonly string[] SpareBuildings = { "Storage3", "CivilArea02" };

	/// <summary>Renders the map and the icon strip into directory. Must run on the CityMap scene.</summary>
	public static IEnumerator Run(string directory)
	{
		MapNarrative narrative = Object.FindObjectOfType<MapNarrative>();
		if (narrative == null)
		{
			Debug.Log("[PcMapSlots] no MapNarrative, not on the city map");
			yield break;
		}
		Directory.CreateDirectory(directory);
		MapScroller scroller = narrative.GetComponentInChildren<MapScroller>();
		Transform mapRoot = scroller.transform;
		MapLocation[] locations = narrative.GetComponentsInChildren<MapLocation>(true);
		Debug.Log("[PcMapSlots] " + locations.Length + " map locations under " + mapRoot.name);

		// Hide the icons of the missions the save currently has
		foreach (MapLocation location in locations)
		{
			foreach (UIButton existing in location.GetComponentsInChildren<UIButton>(true))
			{
				existing.gameObject.SetActive(false);
			}
		}

		// One icon per location, placed exactly like MapLocation.PlotUpdate does
		StringBuilder json = new StringBuilder();
		json.Append("[\n");
		List<GameObject> icons = new List<GameObject>();
		List<MapLocation> placed = new List<MapLocation>();
		foreach (MapLocation location in locations)
		{
			Mission.Type type = Mission.Type.KillAll;
			Color color = MapLocation.Normal;
			if (location.Location == GameData.MissionLocation.AmberArea)
			{
				type = Mission.Type.SurvivalGates;
				color = MapLocation.Survival;
			}
			if (location.Location == GameData.MissionLocation.GoldenArea)
			{
				type = Mission.Type.SurvivalArena;
				color = MapLocation.Survival;
			}
			GameObject template = narrative.GetMissionButton(type);
			GameObject icon = PlaceIcon(template, location.transform, location.IconOffset, color);
			icons.Add(icon);
			placed.Add(location);
			// Highlight the building the same way a location with a mission is shown
			PackedSprite building = location.GetComponent<PackedSprite>();
			building.Color = Color.white;
		}

		// Show the unused highlight of Storage3 too (CivilArea02 covers DangerArea's building, so it stays hidden)
		mapRoot.Find("Storage3").GetComponent<PackedSprite>().Color = Color.white;

		// Let EZGUI build the new sprites before rendering
		yield return null;
		yield return null;

		Bounds bounds = MapBounds(mapRoot);
		Debug.Log("[PcMapSlots] map bounds center=" + bounds.center + " size=" + bounds.size);
		Camera camera = CreateCaptureCamera(bounds);
		for (int i = 0; i < placed.Count; i++)
		{
			MapLocation location = placed[i];
			Vector3 buildingPixel = ToImagePixel(camera, location.transform.position);
			Vector3 iconPixel = ToImagePixel(camera, icons[i].transform.position);
			if (i > 0)
			{
				json.Append(",\n");
			}
			json.Append("  {\"name\": \"" + location.name + "\", \"area\": \"" + location.Location + "\", \"minLevel\": " + location.Level);
			json.Append(", \"building\": [" + Mathf.RoundToInt(buildingPixel.x) + ", " + Mathf.RoundToInt(buildingPixel.y) + "]");
			json.Append(", \"icon\": [" + Mathf.RoundToInt(iconPixel.x) + ", " + Mathf.RoundToInt(iconPixel.y) + "]");
			json.Append(", \"world\": [" + location.transform.position.x.ToString("F2") + ", " + location.transform.position.y.ToString("F2") + "]}");
		}
		// Building highlights with no MapLocation (never used by the game)
		foreach (string spareName in SpareBuildings)
		{
			Transform spare = mapRoot.Find(spareName);
			Vector3 sparePixel = ToImagePixel(camera, spare.position);
			json.Append(",\n  {\"name\": \"" + spareName + "\", \"area\": \"none\", \"minLevel\": 0");
			json.Append(", \"building\": [" + Mathf.RoundToInt(sparePixel.x) + ", " + Mathf.RoundToInt(sparePixel.y) + "]");
			json.Append(", \"world\": [" + spare.position.x.ToString("F2") + ", " + spare.position.y.ToString("F2") + "]}");
		}
		json.Append("\n]\n");
		File.WriteAllText(Path.Combine(directory, "map_slots.json"), json.ToString());

		yield return new WaitForEndOfFrame();
		SavePng(camera, mapRoot, Path.Combine(directory, "map_slots.png"));
		Object.Destroy(camera.gameObject);

		// Strip of every icon template, laid out in a row below the map
		yield return CaptureTemplates(narrative, mapRoot, bounds, Path.Combine(directory, "map_icons.png"));
		Debug.Log("[PcMapSlots] done, written to " + directory);
	}

	/// <summary>Instantiates a mission button template on a location (same math as MapLocation.PlotUpdate).</summary>
	private static GameObject PlaceIcon(GameObject template, Transform parent, Vector3 offset, Color color)
	{
		GameObject icon = Object.Instantiate(template, parent.position, parent.rotation) as GameObject;
		icon.transform.parent = parent;
		icon.transform.position += offset;
		icon.transform.position += new Vector3(0f, 0f, template.transform.position.z - icon.transform.position.z);
		UIButton button = icon.GetComponent<UIButton>();
		icon.transform.position += new Vector3(0f, button.height / 2f, 0f);
		icon.SetActive(true);
		button.Color = color;
		UIPressableButton pressable = icon.GetComponent<UIPressableButton>();
		pressable.SetBaseColor(color);
		pressable.OnPressColor = color;
		return icon;
	}

	/// <summary>Union of the enabled renderers under the map root.</summary>
	private static Bounds MapBounds(Transform mapRoot)
	{
		Renderer[] renderers = mapRoot.GetComponentsInChildren<Renderer>();
		Bounds bounds = new Bounds(mapRoot.position, Vector3.zero);
		bool first = true;
		foreach (Renderer renderer in renderers)
		{
			if (!renderer.enabled)
			{
				continue;
			}
			if (first)
			{
				bounds = renderer.bounds;
				first = false;
			}
			else
			{
				bounds.Encapsulate(renderer.bounds);
			}
		}
		bounds.Expand(new Vector3(BorderUnits * 2f, BorderUnits * 2f, 0f));
		return bounds;
	}

	/// <summary>Orthographic copy of the UI camera that frames the given bounds, rendering into its own texture.</summary>
	private static Camera CreateCaptureCamera(Bounds bounds)
	{
		Camera uiCamera = GUIUtils.UICamera();
		float scale = PixelsPerUnit;
		float largest = Mathf.Max(bounds.size.x, bounds.size.y) * scale;
		if (largest > MaxTextureSide)
		{
			scale = scale * MaxTextureSide / largest;
		}
		int width = Mathf.RoundToInt(bounds.size.x * scale);
		int height = Mathf.RoundToInt(bounds.size.y * scale);
		GameObject holder = new GameObject("PcMapSlotsCamera");
		Camera camera = holder.AddComponent<Camera>();
		camera.CopyFrom(uiCamera);
		camera.orthographic = true;
		camera.orthographicSize = bounds.size.y / 2f;
		camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, uiCamera.transform.position.z);
		camera.targetTexture = new RenderTexture(width, height, 24);
		camera.aspect = (float)width / height;
		camera.enabled = false;
		Debug.Log("[PcMapSlots] capture " + width + "x" + height + " from camera " + uiCamera.name);
		return camera;
	}

	/// <summary>World point to image pixel (origin top left, as in the PNG).</summary>
	private static Vector3 ToImagePixel(Camera camera, Vector3 world)
	{
		Vector3 screen = camera.WorldToScreenPoint(world);
		return new Vector3(screen.x, camera.targetTexture.height - screen.y, 0f);
	}

	/// <summary>Renders only the renderers under keepRoot and writes a PNG.</summary>
	private static void SavePng(Camera camera, Transform keepRoot, string path)
	{
		// Hide everything else (the HUD sits in front of the UI camera, inside the captured area)
		List<Renderer> hidden = new List<Renderer>();
		foreach (Renderer renderer in Object.FindObjectsOfType<Renderer>())
		{
			if (!renderer.enabled)
			{
				continue;
			}
			bool decoration = System.Array.IndexOf(HiddenDecorations, renderer.gameObject.name) >= 0;
			if (decoration || !renderer.transform.IsChildOf(keepRoot))
			{
				renderer.enabled = false;
				hidden.Add(renderer);
			}
		}
		RenderTexture target = camera.targetTexture;
		camera.Render();
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = target;
		Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
		image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
		image.Apply();
		RenderTexture.active = previous;
		File.WriteAllBytes(path, image.EncodeToPNG());
		Object.Destroy(image);
		foreach (Renderer renderer in hidden)
		{
			renderer.enabled = true;
		}
		Debug.Log("[PcMapSlots] wrote " + path);
	}

	/// <summary>Places every mission button template (plus the special hunt one) in a row and captures it.</summary>
	private static IEnumerator CaptureTemplates(MapNarrative narrative, Transform mapRoot, Bounds mapBounds, string path)
	{
		GameObject strip = new GameObject("PcMapSlotsStrip");
		strip.transform.parent = mapRoot;
		Vector3 start = new Vector3(mapBounds.min.x + StripSpacing, mapBounds.min.y - 100f, mapRoot.position.z);
		strip.transform.position = start;
		List<string> names = new List<string>();
		int index = 0;
		foreach (MapNarrative.MissionButton entry in narrative.MissionButtons)
		{
			Transform slot = new GameObject("slot" + index).transform;
			slot.parent = strip.transform;
			slot.position = start + new Vector3(index * StripSpacing, 0f, 0f);
			PlaceIcon(entry.Button, slot, Vector3.zero, Color.white);
			names.Add(entry.MissionType.ToString());
			index++;
		}
		Transform huntSlot = new GameObject("slotHunt").transform;
		huntSlot.parent = strip.transform;
		huntSlot.position = start + new Vector3(index * StripSpacing, 0f, 0f);
		PlaceIcon(narrative.SpecialHuntButton, huntSlot, Vector3.zero, Color.white);
		names.Add("SpecialHunt");
		index++;
		yield return null;
		yield return null;

		Bounds bounds = new Bounds(start + new Vector3((index - 1) * StripSpacing / 2f, 15f, 0f), new Vector3(index * StripSpacing, 50f, 0f));
		Camera camera = CreateCaptureCamera(bounds);
		yield return new WaitForEndOfFrame();
		SavePng(camera, strip.transform, path);
		Object.Destroy(camera.gameObject);
		File.WriteAllText(Path.ChangeExtension(path, ".txt"), string.Join(",", names.ToArray()) + "\n");
		Debug.Log("[PcMapSlots] icon strip order: " + string.Join(",", names.ToArray()));
	}
}
