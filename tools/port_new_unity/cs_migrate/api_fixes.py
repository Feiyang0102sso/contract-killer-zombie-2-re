"""Pilot project: the parts of the Unity 6 API migration that the API updater cannot handle, as exact one-by-one replacements.

Each replacement requires the original text to occur exactly once, otherwise it exits with an error, so nothing is changed in the wrong place.
This list doubles as the record of the changes moved into ckz2_repo_pc.

Usage: python tools/port_new_unity/cs_migrate/api_fixes.py <project root> (edits the code under Source/Runtime)
"""

import sys
from pathlib import Path

# (file, original text, new text, note)
FIXES = [
    # AssetBundle.Load was renamed to LoadAsset in Unity 5
    ("Packages/AssetBundles/Scripts/IndexInfo.cs",
     "obj = assetBundle.Load(assetPath);",
     "obj = assetBundle.LoadAsset(assetPath);",
     "AssetBundle.Load -> LoadAsset"),
    ("Packages/AssetBundles/Scripts/IndexInfo.cs",
     "obj = assetBundle.Load(assetPath, assetType);",
     "obj = assetBundle.LoadAsset(assetPath, assetType);",
     "AssetBundle.Load(type) -> LoadAsset(type)"),
    ("Packages/Localization/StreamGetters/AssetBundleStreamGetter.cs",
     "assetBundle.Load(name2, typeof(TextAsset))",
     "assetBundle.LoadAsset(name2, typeof(TextAsset))",
     "AssetBundle.Load(type) -> LoadAsset(type)"),
    # The iOS soft keyboard auto-rotation properties were removed; PC has no soft keyboard
    ("Packages/EZGUI/GUI/Management/UIManager.cs",
     "\t\t\tTouchScreenKeyboard.autorotateToPortrait = autoRotateKeyboardPortrait;\n"
     "\t\t\tTouchScreenKeyboard.autorotateToPortraitUpsideDown = autoRotateKeyboardPortraitUpsideDown;\n"
     "\t\t\tTouchScreenKeyboard.autorotateToLandscapeLeft = autoRotateKeyboardLandscapeLeft;\n"
     "\t\t\tTouchScreenKeyboard.autorotateToLandscapeRight = autoRotateKeyboardLandscapeRight;\n",
     "",
     "remove TouchScreenKeyboard.autorotate*"),
    # MeshCollider.smoothSphereCollisions was removed
    ("Packages/EZGUI/GUI/Support/AutoSpriteControlBase.cs",
     "meshCollider.smoothSphereCollisions = meshCollider2.smoothSphereCollisions;",
     "",
     "remove smoothSphereCollisions"),
    ("Packages/EZGUI/GUI/Support/ControlBase.cs",
     "meshCollider.smoothSphereCollisions = meshCollider2.smoothSphereCollisions;",
     "",
     "remove smoothSphereCollisions"),
    # Graphics.Blit(src, null, ...): in 3.5 null means the screen; newer versions write back into Camera.main's targetTexture when it has one (exactly what PostEffectManager does),
    # leaving the back buffer empty and the whole screen white. Use NewUnityBlit.ToScreen to draw straight to the back buffer instead.
    ("Scripts/Camera/ChromaticAberrationWithVignetting.cs",
     "Graphics.Blit(sourceText, null, m_material, 1);",
     "NewUnityBlit.ToScreen(sourceText, m_material, 1);",
     "Blit to screen -> NewUnityBlit.ToScreen"),
    ("Scripts/Camera/ChromaticAberrationWithVignetting.cs",
     "Graphics.Blit(sourceText, null, null);",
     "NewUnityBlit.ToScreen(sourceText, null);",
     "Blit to screen -> NewUnityBlit.ToScreen"),
    ("Scripts/Camera/LowHealth.cs",
     "Graphics.Blit(sourceText, null, _lowHealth);",
     "NewUnityBlit.ToScreen(sourceText, _lowHealth);",
     "Blit to screen -> NewUnityBlit.ToScreen"),
    ("Scripts/Camera/MobileBloom.cs",
     "Graphics.Blit(sourceText, null, apply, good ? 4 : 0);",
     "NewUnityBlit.ToScreen(sourceText, apply, good ? 4 : 0);",
     "Blit to screen -> NewUnityBlit.ToScreen"),
    # BoxCollider.extents was removed; extents = size / 2
    ("Scripts/Chars/Human/HumanWeapon.cs",
     "gameObject.GetComponent<BoxCollider>().extents = meshRenderer.bounds.extents;",
     "gameObject.GetComponent<BoxCollider>().size = meshRenderer.bounds.extents * 2f;",
     "BoxCollider.extents -> size"),
    # The old light probe API GetInterpolatedLightProbe(float[27]) was removed, and Unity 6 cannot read the 3.5 probe data either.
    # NewUnitySceneLighting interpolates the original probe data instead, with exactly the same parameters and coefficient layout as 3.5.
    ("Scripts/Game/Visual.cs",
     "LightmapSettings.lightProbes.GetInterpolatedLightProbe(position, renderer, _probe);",
     "NewUnitySceneLighting.GetInterpolatedLightProbe(position, renderer, _probe);",
     "GetInterpolatedLightProbe -> NewUnitySceneLighting"),
    # Rigidbody.SetMaxAngularVelocity became a property
    ("Scripts/Items/Weapons/Launcher.cs",
     "gameObject2.GetComponent<Rigidbody>().SetMaxAngularVelocity(100f);",
     "gameObject2.GetComponent<Rigidbody>().maxAngularVelocity = 100f;",
     "SetMaxAngularVelocity -> maxAngularVelocity"),
    # sleepVelocity / sleepAngularVelocity merged into the mass-normalized kinetic energy threshold sleepThreshold (about 0.5 * v^2)
    ("Scripts/Spawn/SmartSpawnPoint.cs",
     "component.sleepVelocity = 0.2f;\n\t\t\t\t\tcomponent.sleepAngularVelocity = 0.2f;",
     "component.sleepThreshold = 0.5f * 0.2f * 0.2f;",
     "sleepVelocity/sleepAngularVelocity -> sleepThreshold"),
    # Debug VRAM statistics: drop the mobile texture formats that no longer exist
    ("Scripts/Utils/MemoryPrint.cs",
     "\t\tcase TextureFormat.PVRTC_2BPP_RGB:\n\t\t\treturn 2;\n"
     "\t\tcase TextureFormat.PVRTC_2BPP_RGBA:\n\t\t\treturn 2;\n"
     "\t\tcase TextureFormat.PVRTC_4BPP_RGB:\n\t\t\treturn 4;\n"
     "\t\tcase TextureFormat.PVRTC_4BPP_RGBA:\n\t\t\treturn 4;\n",
     "",
     "remove PVRTC cases"),
    ("Scripts/Utils/MemoryPrint.cs",
     "\t\tcase TextureFormat.ATC_RGB4:\n\t\t\treturn 4;\n"
     "\t\tcase TextureFormat.ATC_RGBA8:\n\t\t\treturn 8;\n",
     "",
     "remove ATC cases"),
    ("Scripts/Utils/MemoryPrint.cs",
     "\t\tcase TextureFormat.ATF_RGB_DXT1:\n"
     "\t\tcase TextureFormat.ATF_RGBA_JPG:\n"
     "\t\tcase TextureFormat.ATF_RGB_JPG:\n"
     "\t\t\treturn 0;\n",
     "",
     "remove ATF cases"),
    # AssetBundle.mainAsset no longer exists in post-Unity 5 bundles; the index bundle holds a single text asset, so take that
    ("Packages/AssetBundles/Scripts/IndexInfo.cs",
     "TextAsset textAsset = downloadInfo.assetBundle.mainAsset as TextAsset;",
     "TextAsset textAsset = null;\n"
     "\t\t\t\t\t\tTextAsset[] indexAssets = downloadInfo.assetBundle.LoadAllAssets<TextAsset>();\n"
     "\t\t\t\t\t\tif (indexAssets.Length > 0)\n"
     "\t\t\t\t\t\t{\n"
     "\t\t\t\t\t\t\ttextAsset = indexAssets[0];\n"
     "\t\t\t\t\t\t}",
     "AssetBundle.mainAsset -> LoadAllAssets<TextAsset>()[0]"),
    # Unity 6 does not allow creating a NavMeshPath in a MonoBehaviour field initializer; moved to Awake
    ("Scripts/Managers/PlayerProgress.cs",
     "private UnityEngine.AI.NavMeshPath _tempPath = new UnityEngine.AI.NavMeshPath();",
     "private UnityEngine.AI.NavMeshPath _tempPath;",
     "NavMeshPath field initializer (part 1)"),
    ("Scripts/Managers/PlayerProgress.cs",
     "\tprivate void Awake()\n\t{\n\t\tLogTo(\"Player.PlayerProgress\");",
     "\tprivate void Awake()\n\t{\n\t\t_tempPath = new UnityEngine.AI.NavMeshPath();\n\t\tLogTo(\"Player.PlayerProgress\");",
     "NavMeshPath field initializer (part 2)"),
    # Newer XmlSerializer does not write KeyValuePair's read-only properties, so saved A/B test parameters load back with null keys;
    # skip null keys and let the game regenerate the parameters as originally designed (TODO: switch to a type that serializes correctly to keep the first launch time, etc.)
    ("Packages/ABTesting/Scripts/Parameters.cs",
     "\t\t\t\tif (datum.Key.Equals(key))",
     "\t\t\t\tif (datum.Key != null && datum.Key.Equals(key))",
     "ABTesting Parameters null key guard"),
    # StorageManager's XML read/write entry points go through NewUnityXmlPairs, fixing how KeyValuePair lists are saved and loaded (see NewUnityXmlPairs.cs)
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(data.GetType());\n"
     "\t\txmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);",
     "#if UNITY_IOS\n"
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(data.GetType());\n"
     "\t\txmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);\n"
     "#else\n"
     "\t\tPcXmlPairs.Serialize(XmlWriter.Create(memoryStream), data);\n"
     "#endif",
     "StorageManager.WriteXmlToLocation -> NewUnityXmlPairs"),
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(type);\n"
     "\t\tobj = xmlSerializer.Deserialize(XmlReader.Create(memoryStream));",
     "#if UNITY_IOS\n"
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(type);\n"
     "\t\tobj = xmlSerializer.Deserialize(XmlReader.Create(memoryStream));\n"
     "#else\n"
     "\t\tobj = NewUnityXmlPairs.Deserialize(XmlReader.Create(memoryStream), type);\n"
     "#endif",
     "StorageManager.ReadXmlFromLocation -> NewUnityXmlPairs"),
    # Shop config: the groups / itemRefs getters return a new filtered copy (FindAll). When newer XmlSerializer reads a collection property
    # it first sets an empty list and then adds elements to the list returned by the getter, so everything went into the temporary copy and the shop had no groups (null reference and black screen when opening the shop).
    # Old Mono filled a new list and set it once. Move the XML attributes onto rawGroups / rawItemRefs, which read and write the fields directly; element names are unchanged.
    ("Packages/ShopConfig/Scripts/ShopConfig.cs",
     "\t[XmlArrayItem(\"group\")]\n\t[XmlArray(\"groups\")]\n\tpublic List<ShopConfigGroup> groups\n",
     "\t[XmlIgnore]\n\tpublic List<ShopConfigGroup> groups\n",
     "ShopConfig.groups -> XmlIgnore"),
    ("Packages/ShopConfig/Scripts/ShopConfig.cs",
     "\tpublic List<ShopConfigGroup> rawGroups\n",
     "\t[XmlArrayItem(\"group\")]\n\t[XmlArray(\"groups\")]\n\tpublic List<ShopConfigGroup> rawGroups\n",
     "ShopConfig.rawGroups <- XML groups"),
    ("Packages/ShopConfig/Scripts/ShopConfigGroup.cs",
     "\t[XmlArray(\"itemRefs\")]\n\t[XmlArrayItem(\"itemRef\")]\n\tpublic List<Reference> itemRefs\n",
     "\t[XmlIgnore]\n\tpublic List<Reference> itemRefs\n",
     "ShopConfigGroup.itemRefs -> XmlIgnore"),
    ("Packages/ShopConfig/Scripts/ShopConfigGroup.cs",
     "\tpublic List<Reference> rawItemRefs\n",
     "\t[XmlArray(\"itemRefs\")]\n\t[XmlArrayItem(\"itemRef\")]\n\tpublic List<Reference> rawItemRefs\n",
     "ShopConfigGroup.rawItemRefs <- XML itemRefs"),
    # Input: PC has no touch; EZ GUI's touch polling reads PcInput instead (touches synthesized from the mouse plus taps injected by automated tests)
    ("Packages/EZGUI/GUI/Management/UIManager.cs",
     "\t\tnumActivePointers = Mathf.Min(numTouches, Input.touchCount);",
     "\t\tnumActivePointers = Mathf.Min(numTouches, PcInput.touchCount);",
     "UIManager.PollTouchpad touchCount -> PcInput"),
    ("Packages/EZGUI/GUI/Management/UIManager.cs",
     "\t\t\ttouch = Input.GetTouch(i);",
     "\t\t\ttouch = PcInput.GetTouch(i);",
     "UIManager.PollTouchpad GetTouch -> PcInput"),
    ("Packages/Diagnostics/Scripts/Logging/OnScreenLog.cs",
     "\t\t\t\tif (Input.touchCount > 0)",
     "\t\t\t\tif (PcInput.touchCount > 0)",
     "OnScreenLog touchCount -> PcInput"),
    ("Packages/Diagnostics/Scripts/Logging/OnScreenLog.cs",
     "\t\t\t\t\tTouch touch = Input.GetTouch(0);",
     "\t\t\t\t\tTouch touch = PcInput.GetTouch(0);",
     "OnScreenLog GetTouch -> PcInput"),
    # CharacterController overlap recovery (on by default since Unity 5, absent from 3.5's PhysX 2.8) pushes overlapping controllers apart during Move.
    # Character.UpdateMovement derives speed from displacement / deltaTime; with timeScale=0.001 in the tutorial level one push gets amplified to thousands of m/s,
    # so the tutorial zombies' TargetToAttack flies off the map and they lose their target and wander (pc_pilot_tools/README.md, day fixes section 1). Turned off, matching the original.
    ("Scripts/Chars/Basic/Character.cs",
     "\t\t\tcharacterController = GetComponentInChildren<CharacterController>();\n"
     "\t\t}\n"
     "\t\thealth = healthTotal;\n",
     "\t\t\tcharacterController = GetComponentInChildren<CharacterController>();\n"
     "\t\t}\n"
     "\t\tPcPhysics.MatchLegacyController(characterController);\n"
     "\t\thealth = healthTotal;\n",
     "Character.Awake: disable CharacterController overlap recovery"),
    # A 3.5 controller that starts out overlapping static geometry stays put; the newer sweep ignores the initial overlap and passes straight through.
    # TargetToAttack starts 0.09 m below the road surface and falls through once overlap recovery is off, so Move must follow the 3.5 behaviour as well (NewUnityPhysics.LegacyMove).
    ("Scripts/Chars/Basic/Character.cs",
     "\t\t\tcharacterController.Move(vector);\n",
     "\t\t\tPcPhysics.LegacyMove(characterController, vector, transform);\n",
     "Character.UpdateMovement: legacy stuck-on-overlap Move"),
    # In 3.5 active is not hierarchical: after DeactivateNode turns off an [Action]/[Task] node, children that are active themselves (Effect, Character, ActivateItem, etc.)
    # keep running in 3.5 but are deactivated with their parent in Unity 6 (white screen forever after the tutorial explosion, see pc_pilot_tools/README.md, day fixes section 2). Move them out when finishing.
    ("Scripts/Mission/Mission.cs",
     "\t\t\t\tsubNode.active = false;\n"
     "\t\t\t\treturn true;\n"
     "\t\t\t});\n"
     "\t\t}\n"
     "\t}\n",
     "\t\t\t\tsubNode.active = false;\n"
     "\t\t\t\treturn true;\n"
     "\t\t\t});\n"
     "\t\t}\n"
     "\t\tPcLegacyActive.RescueLegacyActiveChildren(node);\n"
     "\t}\n",
     "Mission.DeactivateNode: keep 3.5-active children running"),
    # Result statistics: AddStat first sets the templates StatText / StatVal / StatBonus inactive, then clones a template for each row.
    # In the original the clones are active (the code then explicitly sets clones with an empty bonus inactive, so clones are shown by default);
    # since Unity 4 clones inherit the template's inactive state, so no stat rows show, the typing animation never starts, and tapping to skip hits a null reference in UIAnimationTypingText.
    # Activate explicitly after cloning (pc_pilot_tools/README.md, day fixes section 4).
    ("Scripts/UI/Widgets/GameStatsWidget.cs",
     "\t\tstatText.isClone = true;\n",
     "\t\tstatText.isClone = true;\n\t\tstatText.gameObject.SetActive(true);\n",
     "GameStatsWidget: activate cloned stat text"),
    ("Scripts/UI/Widgets/GameStatsWidget.cs",
     "\t\tstatVal.isClone = true;\n",
     "\t\tstatVal.isClone = true;\n\t\tstatVal.gameObject.SetActive(true);\n",
     "GameStatsWidget: activate cloned stat value"),
    ("Scripts/UI/Widgets/GameStatsWidget.cs",
     "\t\t\tspriteText.isClone = true;\n",
     "\t\t\tspriteText.isClone = true;\n\t\t\tspriteText.gameObject.SetActive(true);\n",
     "GameStatsWidget: activate cloned stat bonus"),
    # Saves: PlayerStatistics holds XmlDictionary<KeyValuePair<string, string>, int> (hits, kills, etc. per weapon and enemy).
    # Newer XmlSerializer writes KeyValuePair keys as empty elements, so the second key repeats on load and the whole save fails to load and is treated as a new game (pc_pilot_tools/README.md, day fixes section 5).
    ("Packages/StorageManager/Scripts/XmlDictionary.cs",
     "\t\t\tTKey key = (TKey)xmlSerializer.Deserialize(reader);\n",
     "\t\t\tTKey key = (TKey)NewUnityXmlPairs.DeserializeKey(reader, xmlSerializer, typeof(TKey));\n",
     "XmlDictionary.ReadXml: KeyValuePair keys via NewUnityXmlPairs"),
    ("Packages/StorageManager/Scripts/XmlDictionary.cs",
     "\t\t\txmlSerializer.Serialize(writer, key);\n",
     "\t\t\tPcXmlPairs.SerializeKey(writer, xmlSerializer, key);\n",
     "XmlDictionary.WriteXml: KeyValuePair keys via NewUnityXmlPairs"),
    # Resources contains assets with the same name but different types (the export also put dependent materials and meshes into Resources: weaponicons/shotgun1.mat and Shotgun1.png,
    # weapons/*.asset meshes and *.prefab). An untyped Resources.Load may return the material / mesh first, which becomes null after "as":
    # the free 12-Gauge Pump hits a null reference in Weapon.CreateHudIcon when entering a level (pc_pilot_tools/README.md, day fixes section 9). Specify the type as intended.
    ("Scripts/Items/Weapons/Weapon.cs",
     "_hudIcon = Resources.Load(HudIconTexturePath) as Texture2D;",
     "_hudIcon = Resources.Load(HudIconTexturePath, typeof(Texture2D)) as Texture2D;",
     "Weapon.HudIconTexture: typed Resources.Load"),
    ("Scripts/Items/Weapons/Weapon.cs",
     "_scopeModel = Resources.Load(ScopeModelPath) as GameObject;",
     "_scopeModel = Resources.Load(ScopeModelPath, typeof(GameObject)) as GameObject;",
     "Weapon.ScopeModel: typed Resources.Load"),
    ("Scripts/Items/Weapons/Weapon.cs",
     "_model = Resources.Load(ModelPath) as GameObject;",
     "_model = Resources.Load(ModelPath, typeof(GameObject)) as GameObject;",
     "Weapon.Model: typed Resources.Load"),
]


def apply_fix(assets_dir, relative, old, new, note):
    """Applies one exact replacement to a single file; the original text must occur exactly once."""
    path = assets_dir / relative
    raw = path.read_bytes()
    # Keep the file's BOM and line endings: match on \n, restore \r\n when writing back
    has_crlf = b"\r\n" in raw
    text = raw.decode("utf-8-sig")
    if has_crlf:
        text = text.replace("\r\n", "\n")
    count = text.count(old)
    # Safe to rerun: if the new text is already there it was applied before (the new text may contain the original, e.g. when wrapped in #if, so check the new text first);
    # for deletion entries (empty new text) check that the original text is gone
    if new != "":
        already_applied = new in text
    else:
        already_applied = count == 0
    if already_applied:
        print(f"[skip] {relative}: already applied ({note})")
        return
    if count != 1:
        print(f"[fail] {relative}: expected 1 match, found {count} ({note})")
        sys.exit(1)
    text = text.replace(old, new)
    if has_crlf:
        text = text.replace("\n", "\r\n")
    bom = b""
    if raw.startswith(b"\xef\xbb\xbf"):
        bom = b"\xef\xbb\xbf"
    path.write_bytes(bom + text.encode("utf-8"))
    print(f"[ok] {relative}: {note}")


def main():
    """Applies FIXES in order."""
    assets_dir = Path(sys.argv[1]) / "Source" / "Runtime"
    for relative, old, new, note in FIXES:
        apply_fix(assets_dir, relative, old, new, note)


if __name__ == "__main__":
    main()
