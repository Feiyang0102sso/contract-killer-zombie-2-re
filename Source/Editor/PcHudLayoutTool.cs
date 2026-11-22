// HUD layout tool: move UI nodes in the Scene view during Play mode, then save every moved node to its prefab at once.
// Menu: CKZ2/HUD Layout Tool.
//
// Why Play mode: UIAlign places top-level HUD nodes from a screen edge + m_margin, measured with the UI camera,
// which only exists while the game runs. Dragging alone doesn't stick (UIAlign re-applies m_margin on load),
// so saving converts the dragged position back into m_margin.
//
// Moved nodes are recorded by a tracker that runs whether or not the window is open (Undo modifications plus a
// watch on the selected nodes' local positions), so any way of moving works
// (Move tool, Inspector, the nudge buttons). "Focus Scene view on HUD" shows and unlocks only the Gui layer,
// so clicks don't land on the level geometry behind the HUD. Leaving Play mode with unsaved moves asks to save.
//
// Saving edits the prefab YAML text directly (only m_LocalPosition and UIAlign.m_margin of each node).
// Loading the prefab instead would run EZGUI's ExecuteInEditMode scripts, which rebuild meshes and could
// corrupt the asset on save.
//
// Key hints (PcKeyHint_*) are created in code, not in any prefab: saving rewrites their position constants in
// HudController.Pc.cs or PcKeyHints.cs instead. Other nodes the game creates at runtime can't be saved.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class PcHudLayoutTool : EditorWindow
{
	/// <summary>Suffix Unity adds to instantiated objects.</summary>
	private const string CloneSuffix = "(Clone)";

	/// <summary>Where runtime-loaded UI prefabs live (Resources.Load).</summary>
	private const string PrefabFolder = "Assets/Resources";

	/// <summary>Layer all EZGUI HUD sprites are on.</summary>
	private const string GuiLayerName = "Gui";

	/// <summary>HUD root framed by "Focus Scene view on HUD" when nothing is selected.</summary>
	private const string DefaultHudRootName = "Hud";

	/// <summary>Default step of the nudge buttons, in UI units.</summary>
	private const float DefaultNudgeStep = 1f;

	/// <summary>Parent of crate P hints (ActivateItem's ButtonCenter/Button).</summary>
	private const string PickUpHintParentName = "Button";
	/// <summary>Scene view size (m) when focusing a crate P hint: icon and crate lid both in view.</summary>
	private const float PickUpHintViewSize = 1.5f;

	private const string HudHintSource = "Source/Runtime/Port/PcPort/Input/HudController.Pc.cs";
	private const string PickUpHintSource = "Source/Runtime/Port/PcPort/Input/PcKeyHints.cs";

	/// <summary>Where a key hint's position lives in code, keyed by the name of the node it is attached to.</summary>
	private class HintConstant
	{
		public string ParentName;
		public string SourcePath;
		public string Name;

		public HintConstant(string parentName, string sourcePath, string name)
		{
			ParentName = parentName;
			SourcePath = sourcePath;
			Name = name;
		}
	}

	private static readonly HintConstant[] HintConstants = new HintConstant[]
	{
		new HintConstant("SettingsButton", HudHintSource, "PcHintPauseOffset"),
		new HintConstant("Weapon", HudHintSource, "PcHintWeaponSelectOffset"),
		new HintConstant("AmmoBack", HudHintSource, "PcHintReloadOffset"),
		new HintConstant("LeftArrow", HudHintSource, "PcHintItemArrowOffset"),
		new HintConstant("RightArrow", HudHintSource, "PcHintItemArrowOffset"),
		new HintConstant("GrenadeScrollList", HudHintSource, "PcHintUseItemOffset"),
		// 3D icons of ActivateItem (ammo crate, bonus box)
		new HintConstant("Button", PickUpHintSource, "PickUpHintLocalPosition"),
	};

	/// <summary>Weapon wheel slot numbers: hints sit under Slots/&lt;slot&gt;, all sharing one constant.</summary>
	private const string WeaponSlotsName = "Slots";
	private static readonly HintConstant WeaponSlotHint = new HintConstant(null, HudHintSource, "PcHintWeaponSlotOffset");

	private string _status = string.Empty;

	private float _nudgeStep = DefaultNudgeStep;

	/// <summary>Scene view layer settings saved by "Focus Scene view on HUD", restored on close.</summary>
	private bool _layersChanged;
	private int _savedVisibleLayers;
	private int _savedLockedLayers;

	private Vector2 _scroll;

	[MenuItem("CKZ2/HUD Layout Tool")]
	public static void Open()
	{
		GetWindow<PcHudLayoutTool>("HUD Layout");
	}

	private void OnDisable()
	{
		RestoreSceneLayers();
	}

	/// <summary>Keeps the moved list on screen up to date.</summary>
	private void OnInspectorUpdate()
	{
		if (Application.isPlaying)
		{
			Repaint();
		}
	}

	private void OnGUI()
	{
		EditorGUILayout.HelpBox(
			"1. Play into a level, press Esc to pause, click Focus Scene view on HUD.\n" +
			"2. Select nodes in the Hierarchy or Scene view and move them (W tool, Inspector, or the arrows below).\n" +
			"3. Click Save all. Leaving Play mode with unsaved moves also asks to save.",
			MessageType.Info);

		List<Transform> movedNodes = MoveTracker.GetMovedNodes();

		// Save all: always at the top
		GUI.enabled = Application.isPlaying && movedNodes.Count > 0;
		if (GUILayout.Button("Save all (" + movedNodes.Count + " moved)", GUILayout.Height(32)))
		{
			_status = MoveTracker.SaveAll();
		}
		GUI.enabled = true;

		if (_status.Length > 0)
		{
			EditorGUILayout.HelpBox(_status, MessageType.None);
		}

		EditorGUILayout.BeginHorizontal();
		GUI.enabled = Application.isPlaying;
		if (GUILayout.Button("Focus Scene view on HUD"))
		{
			FocusSceneView();
		}
		GUI.enabled = _layersChanged;
		if (GUILayout.Button("Restore Scene view layers"))
		{
			RestoreSceneLayers();
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();

		GUI.enabled = Application.isPlaying;
		if (GUILayout.Button("Focus nearest crate P hint"))
		{
			FocusNearestPickUpHint();
		}
		GUI.enabled = true;

		// Fallback when a move was not picked up automatically
		GUI.enabled = Application.isPlaying && Selection.transforms.Length > 0;
		if (GUILayout.Button("Add selection to moved list"))
		{
			foreach (Transform node in Selection.transforms)
			{
				MoveTracker.MarkMoved(node);
			}
		}
		GUI.enabled = true;

		EditorGUILayout.BeginHorizontal();
		_nudgeStep = EditorGUILayout.FloatField("Nudge selection", _nudgeStep);
		GUI.enabled = Application.isPlaying && Selection.transforms.Length > 0;
		if (GUILayout.Button("←", GUILayout.Width(30)))
		{
			NudgeSelection(-_nudgeStep, 0f);
		}
		if (GUILayout.Button("→", GUILayout.Width(30)))
		{
			NudgeSelection(_nudgeStep, 0f);
		}
		if (GUILayout.Button("↑", GUILayout.Width(30)))
		{
			NudgeSelection(0f, _nudgeStep);
		}
		if (GUILayout.Button("↓", GUILayout.Width(30)))
		{
			NudgeSelection(0f, -_nudgeStep);
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();

		Transform selected = Selection.activeTransform;
		if (selected != null)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Selected", string.Join("/", GetNamePath(selected)));
			EditorGUILayout.LabelField("Local position", FormatVector3(selected.localPosition));
			UIAlign align = selected.GetComponent<UIAlign>();
			if (align != null)
			{
				EditorGUILayout.LabelField("UIAlign", align.m_mode + ", " + align.m_horizontal + " / " + align.m_vertical + ", margin " + FormatVector2(align.m_margin));
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Moved (not saved yet)", EditorStyles.boldLabel);
		_scroll = EditorGUILayout.BeginScrollView(_scroll);
		foreach (Transform node in movedNodes)
		{
			EditorGUILayout.LabelField(string.Join("/", GetNamePath(node)), FormatVector3(node.localPosition));
		}
		EditorGUILayout.EndScrollView();
	}

	/// <summary>Moves every selected node in screen plane and records it as moved.</summary>
	private void NudgeSelection(float x, float y)
	{
		foreach (Transform node in Selection.transforms)
		{
			Undo.RecordObject(node, "Nudge HUD node");
			node.position = node.position + new Vector3(x, y, 0f);
			MoveTracker.MarkMoved(node);
		}
		SceneView.RepaintAll();
	}

	/// <summary>
	/// Records moved HUD nodes while playing, independent of whether the window is open.
	/// Two sources: Undo property modifications (Move tool, Inspector) and a per-frame watch of the selection
	/// (catches anything else that changes a selected node's local position).
	/// </summary>
	[InitializeOnLoad]
	private static class MoveTracker
	{
		/// <summary>Nodes moved since the last save, in the order they were first moved.</summary>
		private static readonly List<Transform> MovedNodes = new List<Transform>();

		/// <summary>Last seen local position of each selected node.</summary>
		private static readonly Dictionary<Transform, Vector3> WatchedPositions = new Dictionary<Transform, Vector3>();

		static MoveTracker()
		{
			Undo.postprocessModifications += OnPostprocessModifications;
			EditorApplication.update += WatchSelection;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		public static List<Transform> GetMovedNodes()
		{
			RemoveDestroyedNodes();
			return MovedNodes;
		}

		public static void MarkMoved(Transform node)
		{
			if (MovedNodes.Contains(node))
			{
				return;
			}
			// Only nodes that can be saved: under a prefab in Resources, or a PC-port key hint
			if (!IsRuntimeCreated(node) && FindPrefabPath(StripClone(node.root.name)) == null)
			{
				return;
			}
			MovedNodes.Add(node);
			Debug.Log("[PcHudLayoutTool] moved: " + string.Join("/", GetNamePath(node)));
		}

		private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
		{
			if (!Application.isPlaying)
			{
				return modifications;
			}
			foreach (UndoPropertyModification modification in modifications)
			{
				Transform node = modification.currentValue.target as Transform;
				if (node != null && modification.currentValue.propertyPath.StartsWith("m_LocalPosition"))
				{
					MarkMoved(node);
				}
			}
			return modifications;
		}

		private static void WatchSelection()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			Transform[] selection = Selection.transforms;
			foreach (Transform node in selection)
			{
				Vector3 lastPosition;
				if (WatchedPositions.TryGetValue(node, out lastPosition) && lastPosition != node.localPosition)
				{
					MarkMoved(node);
				}
			}
			// Re-snapshot only the current selection, so a node starts being watched when it gets selected
			WatchedPositions.Clear();
			foreach (Transform node in selection)
			{
				WatchedPositions[node] = node.localPosition;
			}
		}

		/// <summary>Leaving Play mode destroys the HUD: offer to save unsaved moves first.</summary>
		private static void OnPlayModeStateChanged(PlayModeStateChange change)
		{
			if (change == PlayModeStateChange.ExitingPlayMode)
			{
				RemoveDestroyedNodes();
				if (MovedNodes.Count > 0)
				{
					bool save = EditorUtility.DisplayDialog("HUD Layout",
						MovedNodes.Count + " moved HUD node(s) are not saved. Save them?",
						"Save all", "Discard");
					if (save)
					{
						SaveAll();
					}
				}
			}
			else if (change == PlayModeStateChange.EnteredEditMode)
			{
				MovedNodes.Clear();
				WatchedPositions.Clear();
			}
		}

		private static void RemoveDestroyedNodes()
		{
			for (int i = MovedNodes.Count - 1; i >= 0; i--)
			{
				if (MovedNodes[i] == null)
				{
					MovedNodes.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Saves every moved node, then clears the list. Prefab nodes: one file write per prefab, a node that fails
		/// doesn't block the others. Key hints: their position constants in the PC-port source are rewritten.
		/// Returns a report for the window.
		/// </summary>
		public static string SaveAll()
		{
			RemoveDestroyedNodes();
			Dictionary<string, List<PrefabYaml.NodeEdit>> editsByPrefab = new Dictionary<string, List<PrefabYaml.NodeEdit>>();
			Dictionary<HintConstant, Vector3> hintValues = new Dictionary<HintConstant, Vector3>();
			StringBuilder report = new StringBuilder();

			foreach (Transform node in MovedNodes)
			{
				string nodePath = string.Join("/", GetNamePath(node));
				if (IsRuntimeCreated(node))
				{
					HintConstant constant = FindHintConstant(node);
					if (constant == null)
					{
						AddReport(report, "Not saved " + nodePath + ": no position constant known for this key hint");
						continue;
					}
					// Shared constants (Q/E, weapon slots): the last moved one wins
					Vector3 offset = node.localPosition;
					if (constant.Name == "PcHintItemArrowOffset")
					{
						// Q / E cancel their arrow's rotation (LeftArrow is turned 180 degrees) and mirror x:
						// convert back to the shared constant (see HudController.AttachPcItemArrowHint)
						offset = HudController.PcItemArrowLocalToOffset(node.parent, node.localPosition);
					}
					hintValues[constant] = offset;
					continue;
				}
				List<string> namePath = GetNamePath(node);
				string prefabPath = FindPrefabPath(namePath[0]);
				PrefabYaml.NodeEdit edit = new PrefabYaml.NodeEdit();
				edit.NamePath = namePath;
				edit.LocalPosition = node.localPosition;
				UIAlign align = node.GetComponent<UIAlign>();
				if (align != null)
				{
					edit.HasMargin = true;
					edit.Margin = ComputeMarginAfterDrag(align);
				}
				if (!editsByPrefab.ContainsKey(prefabPath))
				{
					editsByPrefab[prefabPath] = new List<PrefabYaml.NodeEdit>();
				}
				editsByPrefab[prefabPath].Add(edit);
			}

			foreach (KeyValuePair<string, List<PrefabYaml.NodeEdit>> pair in editsByPrefab)
			{
				SavePrefab(pair.Key, pair.Value, report);
			}
			SaveHintConstants(hintValues, report);

			MovedNodes.Clear();
			return report.ToString().TrimEnd();
		}

		private static void SavePrefab(string prefabPath, List<PrefabYaml.NodeEdit> edits, StringBuilder report)
		{
			Dictionary<PrefabYaml.NodeEdit, string> nodeErrors = new Dictionary<PrefabYaml.NodeEdit, string>();
			string fileError = PrefabYaml.WriteNodes(prefabPath, edits, nodeErrors);
			if (fileError != null)
			{
				AddReport(report, "Not saved " + prefabPath + ": " + fileError);
				return;
			}
			AssetDatabase.ImportAsset(prefabPath);
			foreach (PrefabYaml.NodeEdit edit in edits)
			{
				string nodePath = string.Join("/", edit.NamePath);
				string error;
				if (nodeErrors.TryGetValue(edit, out error))
				{
					AddReport(report, "Not saved " + nodePath + ": " + error);
					continue;
				}
				string line = "Saved " + nodePath + " " + FormatVector3(edit.LocalPosition);
				if (edit.HasMargin)
				{
					line += ", margin " + FormatVector2(edit.Margin);
				}
				AddReport(report, line);
			}
		}

		/// <summary>Rewrites "Name = new Vector3(...)" in the PC-port source for each moved key hint.</summary>
		private static void SaveHintConstants(Dictionary<HintConstant, Vector3> hintValues, StringBuilder report)
		{
			Dictionary<string, string> sources = new Dictionary<string, string>();
			foreach (KeyValuePair<HintConstant, Vector3> pair in hintValues)
			{
				HintConstant constant = pair.Key;
				if (!sources.ContainsKey(constant.SourcePath))
				{
					sources[constant.SourcePath] = File.ReadAllText(constant.SourcePath);
				}
				Regex pattern = new Regex(Regex.Escape(constant.Name) + @" = new Vector3\([^)]*\)");
				string source = sources[constant.SourcePath];
				if (!pattern.IsMatch(source))
				{
					AddReport(report, "Not saved " + constant.Name + ": not found in " + constant.SourcePath);
					continue;
				}
				Vector3 value = pair.Value;
				string replacement = constant.Name + " = new Vector3(" + FormatFloat(value.x) + "f, " + FormatFloat(value.y) + "f, " + FormatFloat(value.z) + "f)";
				sources[constant.SourcePath] = pattern.Replace(source, replacement, 1);
				AddReport(report, "Saved " + replacement + " in " + Path.GetFileName(constant.SourcePath));
			}
			foreach (KeyValuePair<string, string> pair in sources)
			{
				File.WriteAllText(pair.Key, pair.Value);
			}
			if (sources.Count > 0)
			{
				// File.WriteAllText bypasses the AssetDatabase: without a refresh, pressing Play right after saving
				// runs the old code. In Play mode Unity recompiles once Play mode ends.
				AssetDatabase.Refresh();
				AddReport(report, "Source changed: scripts recompile now (or after Play mode ends)");
			}
		}

		private static void AddReport(StringBuilder report, string line)
		{
			Debug.Log("[PcHudLayoutTool] " + line);
			report.AppendLine(line);
		}
	}

	/// <summary>Front orthographic view, only the Gui layer visible and pickable, framed on the HUD.</summary>
	private void FocusSceneView()
	{
		SceneView sceneView = SceneView.lastActiveSceneView;
		if (sceneView == null)
		{
			_status = "Open a Scene view first.";
			return;
		}
		int guiLayer = LayerMask.NameToLayer(GuiLayerName);
		if (guiLayer < 0)
		{
			_status = "Layer '" + GuiLayerName + "' not found.";
			return;
		}
		Transform hudRoot = null;
		if (Selection.activeTransform != null)
		{
			hudRoot = Selection.activeTransform.root;
		}
		else
		{
			GameObject hudObject = GameObject.Find(DefaultHudRootName);
			if (hudObject != null)
			{
				hudRoot = hudObject.transform;
			}
		}

		if (!_layersChanged)
		{
			_savedVisibleLayers = Tools.visibleLayers;
			_savedLockedLayers = Tools.lockedLayers;
			_layersChanged = true;
		}
		int guiMask = 1 << guiLayer;
		Tools.visibleLayers = guiMask;
		Tools.lockedLayers = ~guiMask;

		// 2D mode = looking along +Z, orthographic: the same direction as the UI camera
		sceneView.in2DMode = true;
		Bounds bounds;
		if (hudRoot != null && TryGetHudBounds(hudRoot, guiLayer, out bounds))
		{
			sceneView.Frame(bounds, false);
		}
		sceneView.Repaint();
		_status = string.Empty;
		Debug.Log("[PcHudLayoutTool] scene view focused on the Gui layer");
	}

	/// <summary>
	/// 3D view on the crate P hint (ammo crate / bonus box) nearest to the game camera: leaves 2D mode, shows all
	/// layers (crates are on Gui3d, hidden by "Focus Scene view on HUD"), selects the hint and looks at it the way the
	/// game camera does. The hint is only drawn while the crate's text is shown, so stand next to the crate first.
	/// </summary>
	private void FocusNearestPickUpHint()
	{
		SceneView sceneView = SceneView.lastActiveSceneView;
		if (sceneView == null)
		{
			_status = "Open a Scene view first.";
			return;
		}
		Camera gameCamera = Camera.main;
		if (gameCamera == null)
		{
			_status = "No game camera.";
			return;
		}
		Transform nearestHint = null;
		float nearestDistance = float.PositiveInfinity;
		foreach (PcKeyHintRoot hint in FindObjectsByType<PcKeyHintRoot>(FindObjectsSortMode.None))
		{
			// Crate hints sit under ActivateItem's ButtonCenter/Button
			if (hint.transform.parent == null || hint.transform.parent.name != PickUpHintParentName)
			{
				continue;
			}
			float distance = Vector3.Distance(hint.transform.position, gameCamera.transform.position);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearestHint = hint.transform;
			}
		}
		if (nearestHint == null)
		{
			_status = "No crate P hint in the scene.";
			return;
		}

		RestoreSceneLayers();
		sceneView.in2DMode = false;
		sceneView.orthographic = false;
		Selection.activeGameObject = nearestHint.gameObject;
		EditorGUIUtility.PingObject(nearestHint.gameObject);
		sceneView.LookAt(nearestHint.position, gameCamera.transform.rotation, PickUpHintViewSize);
		sceneView.Repaint();
		_status = string.Empty;
		Debug.Log("[PcHudLayoutTool] scene view on " + string.Join("/", GetNamePath(nearestHint)));
	}

	private void RestoreSceneLayers()
	{
		if (!_layersChanged)
		{
			return;
		}
		Tools.visibleLayers = _savedVisibleLayers;
		Tools.lockedLayers = _savedLockedLayers;
		_layersChanged = false;
		SceneView.RepaintAll();
	}

	/// <summary>Bounds of all active Gui-layer renderers under root.</summary>
	private static bool TryGetHudBounds(Transform root, int guiLayer, out Bounds bounds)
	{
		bounds = new Bounds();
		bool found = false;
		foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
		{
			if (renderer.gameObject.layer != guiLayer)
			{
				continue;
			}
			if (!found)
			{
				bounds = renderer.bounds;
				found = true;
			}
			else
			{
				bounds.Encapsulate(renderer.bounds);
			}
		}
		return found;
	}

	/// <summary>
	/// UIAlign places the node from m_margin on load (Screen or Parent mode), so a dragged node needs a new margin:
	/// run UpdateAlignment to see where the current margin puts it, then add the drag offset to the margin.
	/// </summary>
	private static Vector2 ComputeMarginAfterDrag(UIAlign align)
	{
		Transform node = align.transform;
		Vector3 dragged = node.position;
		align.UpdateAlignment();
		Vector3 aligned = node.position;
		node.position = dragged;
		Vector2 margin = align.m_margin;
		if (align.m_horizontal != UIAlign.Horizontal.None)
		{
			margin.x += dragged.x - aligned.x;
		}
		if (align.m_vertical != UIAlign.Vertical.None)
		{
			margin.y += dragged.y - aligned.y;
		}
		align.m_margin = margin;
		return margin;
	}

	/// <summary>Position constant of a key hint, found from the node it is attached to.</summary>
	private static HintConstant FindHintConstant(Transform hint)
	{
		Transform parent = hint.parent;
		if (parent == null)
		{
			return null;
		}
		// Weapon wheel slots: Slots/<slot>/PcKeyHint_<n>
		if (parent.parent != null && parent.parent.name == WeaponSlotsName)
		{
			return WeaponSlotHint;
		}
		foreach (HintConstant constant in HintConstants)
		{
			if (constant.ParentName == parent.name)
			{
				return constant;
			}
		}
		return null;
	}

	/// <summary>Names from the root to node, with "(Clone)" removed (root name = prefab name).</summary>
	private static List<string> GetNamePath(Transform node)
	{
		List<string> names = new List<string>();
		Transform current = node;
		while (current != null)
		{
			names.Insert(0, StripClone(current.name));
			current = current.parent;
		}
		return names;
	}

	private static string StripClone(string name)
	{
		if (name.EndsWith(CloneSuffix))
		{
			return name.Substring(0, name.Length - CloneSuffix.Length);
		}
		return name;
	}

	/// <summary>PC-port nodes created in code (PcKeyHints) are not in any prefab.</summary>
	private static bool IsRuntimeCreated(Transform node)
	{
		return node.name.StartsWith("PcKeyHint_");
	}

	private static string FindPrefabPath(string rootName)
	{
		string path = PrefabFolder + "/" + rootName + ".prefab";
		if (File.Exists(path))
		{
			return path;
		}
		return null;
	}

	private static string FormatFloat(float value)
	{
		return value.ToString("0.###", CultureInfo.InvariantCulture);
	}

	private static string FormatVector2(Vector2 value)
	{
		return "(" + FormatFloat(value.x) + ", " + FormatFloat(value.y) + ")";
	}

	private static string FormatVector3(Vector3 value)
	{
		return "(" + FormatFloat(value.x) + ", " + FormatFloat(value.y) + ", " + FormatFloat(value.z) + ")";
	}

	/// <summary>Minimal text editing of a Unity prefab YAML file: find nodes by name path, rewrite two lines each.</summary>
	private static class PrefabYaml
	{
		private static readonly Regex DocumentHeader = new Regex(@"^--- !u!(\d+) &(-?\d+)");
		private static readonly Regex FileIdField = new Regex(@"\{fileID: (-?\d+)");

		/// <summary>New values for one node.</summary>
		public class NodeEdit
		{
			public List<string> NamePath;
			public Vector3 LocalPosition;
			public bool HasMargin;
			public Vector2 Margin;
		}

		/// <summary>One YAML document (one serialized object) as a line range.</summary>
		private class Document
		{
			public int ClassId;
			public string FileId;
			public int FirstLine;
			public int EndLine;
		}

		/// <summary>
		/// Applies the edits and writes the file once. Edits that can't be applied go to nodeErrors and are skipped.
		/// Returns an error only when the file itself can't be handled (file untouched).
		/// </summary>
		public static string WriteNodes(string prefabPath, List<NodeEdit> edits, Dictionary<NodeEdit, string> nodeErrors)
		{
			string text = File.ReadAllText(prefabPath);
			string newline = "\n";
			if (text.Contains("\r\n"))
			{
				newline = "\r\n";
			}
			string[] lines = text.Split(new string[] { newline }, System.StringSplitOptions.None);
			List<Document> documents = SplitDocuments(lines);

			// GameObject id -> name; Transform id -> (GameObject id, father Transform id)
			Dictionary<string, string> gameObjectNames = new Dictionary<string, string>();
			Dictionary<string, string> transformGameObject = new Dictionary<string, string>();
			Dictionary<string, string> transformFather = new Dictionary<string, string>();
			Dictionary<string, Document> transformDocuments = new Dictionary<string, Document>();
			Dictionary<string, Document> alignDocuments = new Dictionary<string, Document>();
			foreach (Document document in documents)
			{
				if (document.ClassId == 1)
				{
					string name = ReadField(lines, document, "m_Name:");
					if (name != null)
					{
						gameObjectNames[document.FileId] = name;
					}
				}
				else if (document.ClassId == 4)
				{
					transformGameObject[document.FileId] = ReadFileId(lines, document, "m_GameObject:");
					transformFather[document.FileId] = ReadFileId(lines, document, "m_Father:");
					transformDocuments[document.FileId] = document;
				}
				else if (document.ClassId == 114)
				{
					// UIAlign is the MonoBehaviour that has both m_horizontal and m_margin
					if (FindLine(lines, document, "m_horizontal:") >= 0 && FindLine(lines, document, "m_margin:") >= 0)
					{
						alignDocuments[ReadFileId(lines, document, "m_GameObject:")] = document;
					}
				}
			}

			string rootTransform = null;
			foreach (KeyValuePair<string, string> pair in transformFather)
			{
				if (pair.Value == "0")
				{
					rootTransform = pair.Key;
				}
			}
			if (rootTransform == null)
			{
				return "root Transform not found";
			}

			bool anyApplied = false;
			foreach (NodeEdit edit in edits)
			{
				// Walk the name path from the root Transform
				string currentTransform = rootTransform;
				string error = null;
				for (int i = 1; i < edit.NamePath.Count; i++)
				{
					string child = FindChild(currentTransform, edit.NamePath[i], transformFather, transformGameObject, gameObjectNames);
					if (child == null)
					{
						error = "'" + edit.NamePath[i] + "' is not in the prefab (created by the game at runtime); move a node above it instead";
						break;
					}
					currentTransform = child;
				}
				if (error != null)
				{
					nodeErrors[edit] = error;
					continue;
				}

				Document transformDocument = transformDocuments[currentTransform];
				int positionLine = FindLine(lines, transformDocument, "m_LocalPosition:");
				if (positionLine < 0)
				{
					nodeErrors[edit] = "m_LocalPosition not found";
					continue;
				}
				int marginLine = -1;
				if (edit.HasMargin)
				{
					string gameObjectId = transformGameObject[currentTransform];
					Document alignDocument;
					if (!alignDocuments.TryGetValue(gameObjectId, out alignDocument))
					{
						nodeErrors[edit] = "UIAlign not found in the prefab";
						continue;
					}
					marginLine = FindLine(lines, alignDocument, "m_margin:");
				}

				Vector3 position = edit.LocalPosition;
				lines[positionLine] = ReplaceValue(lines[positionLine], "{x: " + Number(position.x) + ", y: " + Number(position.y) + ", z: " + Number(position.z) + "}");
				if (marginLine >= 0)
				{
					lines[marginLine] = ReplaceValue(lines[marginLine], "{x: " + Number(edit.Margin.x) + ", y: " + Number(edit.Margin.y) + "}");
				}
				anyApplied = true;
			}

			if (anyApplied)
			{
				File.WriteAllText(prefabPath, string.Join(newline, lines));
			}
			return null;
		}

		private static List<Document> SplitDocuments(string[] lines)
		{
			List<Document> documents = new List<Document>();
			Document current = null;
			for (int i = 0; i < lines.Length; i++)
			{
				Match match = DocumentHeader.Match(lines[i]);
				if (!match.Success)
				{
					continue;
				}
				if (current != null)
				{
					current.EndLine = i;
				}
				current = new Document();
				current.ClassId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
				current.FileId = match.Groups[2].Value;
				current.FirstLine = i;
				documents.Add(current);
			}
			if (current != null)
			{
				current.EndLine = lines.Length;
			}
			return documents;
		}

		/// <summary>Index of the first line in the document whose trimmed text starts with key, or -1.</summary>
		private static int FindLine(string[] lines, Document document, string key)
		{
			for (int i = document.FirstLine + 1; i < document.EndLine; i++)
			{
				if (lines[i].TrimStart().StartsWith(key))
				{
					return i;
				}
			}
			return -1;
		}

		private static string ReadField(string[] lines, Document document, string key)
		{
			int index = FindLine(lines, document, key);
			if (index < 0)
			{
				return null;
			}
			string trimmed = lines[index].TrimStart();
			return trimmed.Substring(key.Length).Trim();
		}

		private static string ReadFileId(string[] lines, Document document, string key)
		{
			string value = ReadField(lines, document, key);
			if (value == null)
			{
				return null;
			}
			Match match = FileIdField.Match(value);
			if (!match.Success)
			{
				return null;
			}
			return match.Groups[1].Value;
		}

		private static string FindChild(string fatherTransform, string childName, Dictionary<string, string> transformFather, Dictionary<string, string> transformGameObject, Dictionary<string, string> gameObjectNames)
		{
			foreach (KeyValuePair<string, string> pair in transformFather)
			{
				if (pair.Value != fatherTransform)
				{
					continue;
				}
				string gameObjectId = transformGameObject[pair.Key];
				string name;
				if (gameObjectNames.TryGetValue(gameObjectId, out name) && name == childName)
				{
					return pair.Key;
				}
			}
			return null;
		}

		/// <summary>Keeps "  key: " and replaces what follows.</summary>
		private static string ReplaceValue(string line, string newValue)
		{
			int colon = line.IndexOf(':');
			return line.Substring(0, colon + 1) + " " + newValue;
		}

		private static string Number(float value)
		{
			return value.ToString("R", CultureInfo.InvariantCulture);
		}
	}
}
