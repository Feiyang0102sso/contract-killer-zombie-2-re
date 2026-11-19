"""试点工程：Unity 6 API 迁移中 API 更新器处理不了的部分，逐条精确替换。

每条替换都要求原文恰好出现一次，否则报错退出，避免改错地方。
这份清单同时是以后搬进 ckz2_repo_pc 的改动记录。

用法：python tools/api_fixes.py <工程根目录>（改 Source/Runtime 下的代码）
"""

import sys
from pathlib import Path

# (文件, 原文, 新文, 说明)
FIXES = [
    # AssetBundle.Load 在 Unity 5 改名为 LoadAsset
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
    # iOS 软键盘自动旋转属性已删除；PC 没有软键盘
    ("Packages/EZGUI/GUI/Management/UIManager.cs",
     "\t\t\tTouchScreenKeyboard.autorotateToPortrait = autoRotateKeyboardPortrait;\n"
     "\t\t\tTouchScreenKeyboard.autorotateToPortraitUpsideDown = autoRotateKeyboardPortraitUpsideDown;\n"
     "\t\t\tTouchScreenKeyboard.autorotateToLandscapeLeft = autoRotateKeyboardLandscapeLeft;\n"
     "\t\t\tTouchScreenKeyboard.autorotateToLandscapeRight = autoRotateKeyboardLandscapeRight;\n",
     "",
     "remove TouchScreenKeyboard.autorotate*"),
    # MeshCollider.smoothSphereCollisions 已删除
    ("Packages/EZGUI/GUI/Support/AutoSpriteControlBase.cs",
     "meshCollider.smoothSphereCollisions = meshCollider2.smoothSphereCollisions;",
     "",
     "remove smoothSphereCollisions"),
    ("Packages/EZGUI/GUI/Support/ControlBase.cs",
     "meshCollider.smoothSphereCollisions = meshCollider2.smoothSphereCollisions;",
     "",
     "remove smoothSphereCollisions"),
    # Graphics.Blit(src, null, ...)：3.5 里 null 表示屏幕；新版在 Camera.main 有 targetTexture 时写回那张 RT（PostEffectManager 正是这样），
    # 后备缓冲区没有画面，整屏白色。改用 PcBlit.ToScreen 直接画到后备缓冲区。
    ("Scripts/Camera/ChromaticAberrationWithVignetting.cs",
     "Graphics.Blit(sourceText, null, m_material, 1);",
     "PcBlit.ToScreen(sourceText, m_material, 1);",
     "Blit to screen -> PcBlit.ToScreen"),
    ("Scripts/Camera/ChromaticAberrationWithVignetting.cs",
     "Graphics.Blit(sourceText, null, null);",
     "PcBlit.ToScreen(sourceText, null);",
     "Blit to screen -> PcBlit.ToScreen"),
    ("Scripts/Camera/LowHealth.cs",
     "Graphics.Blit(sourceText, null, _lowHealth);",
     "PcBlit.ToScreen(sourceText, _lowHealth);",
     "Blit to screen -> PcBlit.ToScreen"),
    ("Scripts/Camera/MobileBloom.cs",
     "Graphics.Blit(sourceText, null, apply, good ? 4 : 0);",
     "PcBlit.ToScreen(sourceText, apply, good ? 4 : 0);",
     "Blit to screen -> PcBlit.ToScreen"),
    # BoxCollider.extents 已删除；extents = size / 2
    ("Scripts/Chars/Human/HumanWeapon.cs",
     "gameObject.GetComponent<BoxCollider>().extents = meshRenderer.bounds.extents;",
     "gameObject.GetComponent<BoxCollider>().size = meshRenderer.bounds.extents * 2f;",
     "BoxCollider.extents -> size"),
    # 旧光照探针接口 GetInterpolatedLightProbe(float[27]) 已删除，3.5 的探针数据 Unity 6 也读不了。
    # 改由 PcSceneLighting 用原版探针数据插值，参数和系数排列与 3.5 完全相同。
    ("Scripts/Game/Visual.cs",
     "LightmapSettings.lightProbes.GetInterpolatedLightProbe(position, renderer, _probe);",
     "PcSceneLighting.GetInterpolatedLightProbe(position, renderer, _probe);",
     "GetInterpolatedLightProbe -> PcSceneLighting"),
    # Rigidbody.SetMaxAngularVelocity 改为属性
    ("Scripts/Items/Weapons/Launcher.cs",
     "gameObject2.GetComponent<Rigidbody>().SetMaxAngularVelocity(100f);",
     "gameObject2.GetComponent<Rigidbody>().maxAngularVelocity = 100f;",
     "SetMaxAngularVelocity -> maxAngularVelocity"),
    # sleepVelocity / sleepAngularVelocity 合并为按质量归一化的动能阈值 sleepThreshold（约 0.5 * v^2）
    ("Scripts/Spawn/SmartSpawnPoint.cs",
     "component.sleepVelocity = 0.2f;\n\t\t\t\t\tcomponent.sleepAngularVelocity = 0.2f;",
     "component.sleepThreshold = 0.5f * 0.2f * 0.2f;",
     "sleepVelocity/sleepAngularVelocity -> sleepThreshold"),
    # 调试用的显存统计：删掉已不存在的移动端纹理格式
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
    # AssetBundle.mainAsset 在 Unity 5 之后的资源包里没有了；索引包里只有一个文本资源，直接取它
    ("Packages/AssetBundles/Scripts/IndexInfo.cs",
     "TextAsset textAsset = downloadInfo.assetBundle.mainAsset as TextAsset;",
     "TextAsset textAsset = null;\n"
     "\t\t\t\t\t\tTextAsset[] indexAssets = downloadInfo.assetBundle.LoadAllAssets<TextAsset>();\n"
     "\t\t\t\t\t\tif (indexAssets.Length > 0)\n"
     "\t\t\t\t\t\t{\n"
     "\t\t\t\t\t\t\ttextAsset = indexAssets[0];\n"
     "\t\t\t\t\t\t}",
     "AssetBundle.mainAsset -> LoadAllAssets<TextAsset>()[0]"),
    # 存档键：iOS 要求路径在沙盒内，PC 改用 PcNative 的映射
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\tprivate static string PathToKey(string path)\n\t{\n\t\tif (Path.IsPathRooted(path))",
     "\tprivate static string PathToKey(string path)\n\t{\n"
     "#if !UNITY_IOS\n"
     "\t\treturn PcNative.StoragePathToKey(path);\n"
     "#else\n"
     "\t\tif (Path.IsPathRooted(path))",
     "StorageManager.PathToKey PC branch (part 1)"),
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\t\tthrow new StorageManagerException(\"Cannot use relative file path for access on iOS\");\n\t}",
     "\t\tthrow new StorageManagerException(\"Cannot use relative file path for access on iOS\");\n#endif\n\t}",
     "StorageManager.PathToKey PC branch (part 2)"),
    # Unity 6 不允许在 MonoBehaviour 的字段初始化器里创建 NavMeshPath，挪到 Awake
    ("Scripts/Managers/PlayerProgress.cs",
     "private UnityEngine.AI.NavMeshPath _tempPath = new UnityEngine.AI.NavMeshPath();",
     "private UnityEngine.AI.NavMeshPath _tempPath;",
     "NavMeshPath field initializer (part 1)"),
    ("Scripts/Managers/PlayerProgress.cs",
     "\tprivate void Awake()\n\t{\n\t\tLogTo(\"Player.PlayerProgress\");",
     "\tprivate void Awake()\n\t{\n\t\t_tempPath = new UnityEngine.AI.NavMeshPath();\n\t\tLogTo(\"Player.PlayerProgress\");",
     "NavMeshPath field initializer (part 2)"),
    # 新版 XmlSerializer 不写 KeyValuePair 的只读属性，存出的 A/B 测试参数读回后键为 null；
    # 跳过空键，让游戏按原逻辑重新生成参数（TODO：换成可正确序列化的类型以保留首次启动时间等）
    ("Packages/ABTesting/Scripts/Parameters.cs",
     "\t\t\t\tif (datum.Key.Equals(key))",
     "\t\t\t\tif (datum.Key != null && datum.Key.Equals(key))",
     "ABTesting Parameters null key guard"),
    # StorageManager 的 XML 读写入口改走 PcXmlPairs，修正 KeyValuePair 列表的存取（见 PcPort/PcXmlPairs.cs）
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(data.GetType());\n"
     "\t\txmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);",
     "#if UNITY_IOS\n"
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(data.GetType());\n"
     "\t\txmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);\n"
     "#else\n"
     "\t\tPcXmlPairs.Serialize(XmlWriter.Create(memoryStream), data);\n"
     "#endif",
     "StorageManager.WriteXmlToLocation -> PcXmlPairs"),
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(type);\n"
     "\t\tobj = xmlSerializer.Deserialize(XmlReader.Create(memoryStream));",
     "#if UNITY_IOS\n"
     "\t\tXmlSerializer xmlSerializer = new XmlSerializer(type);\n"
     "\t\tobj = xmlSerializer.Deserialize(XmlReader.Create(memoryStream));\n"
     "#else\n"
     "\t\tobj = PcXmlPairs.Deserialize(XmlReader.Create(memoryStream), type);\n"
     "#endif",
     "StorageManager.ReadXmlFromLocation -> PcXmlPairs"),
    ("Packages/StorageManager/Scripts/StorageManager.cs",
     "\t\t\tXmlSerializer xmlSerializer = new XmlSerializer(type);\n"
     "\t\t\tobj = xmlSerializer.Deserialize(textReader);",
     "#if UNITY_IOS\n"
     "\t\t\tXmlSerializer xmlSerializer = new XmlSerializer(type);\n"
     "\t\t\tobj = xmlSerializer.Deserialize(textReader);\n"
     "#else\n"
     "\t\t\tobj = PcXmlPairs.Deserialize(XmlReader.Create(textReader), type);\n"
     "#endif",
     "StorageManager.ReadXmlFromLocationWithoutChecking -> PcXmlPairs"),
    # 商店配置：groups / itemRefs 的 getter 返回过滤后的新副本（FindAll）。新版 XmlSerializer 读集合属性时
    # 先 set 一个空列表、再调 getter 往返回的列表里加元素，结果全加进了临时副本，商店没有分组（打开商店空引用、黑屏）。
    # 旧版 Mono 是填满新列表后一次 set。把 XML 特性挪到直接读写字段的 rawGroups / rawItemRefs，元素名不变。
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
    # 输入：PC 没有触摸，EZ GUI 的触摸轮询改读 PcInput（鼠标合成的触摸 + 自动测试注入的点击）
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
    # CharacterController 的 overlap recovery（Unity 5 起默认开启，3.5 的 PhysX 2.8 没有）：Move 时把重叠的控制器推出去。
    # Character.UpdateMovement 用“位移 / deltaTime”反推速度，教程关 timeScale=0.001 时一次推出被放大成几千 m/s，
    # 教程里僵尸要攻击的 TargetToAttack 因此飞出地图，僵尸失去目标改成闲逛（白天修复第 1 节）。关掉，与原版一致。
    ("Scripts/Chars/Basic/Character.cs",
     "\t\t\tcharacterController = GetComponentInChildren<CharacterController>();\n"
     "\t\t}\n"
     "\t\thealth = healthTotal;\n",
     "\t\t\tcharacterController = GetComponentInChildren<CharacterController>();\n"
     "\t\t}\n"
     "\t\tPcPhysics.MatchLegacyController(characterController);\n"
     "\t\thealth = healthTotal;\n",
     "Character.Awake: disable CharacterController overlap recovery"),
    # 3.5 的控制器一开始就和静态几何体重叠时原地不动；新版的扫掠忽略初始重叠，直接穿过去。
    # TargetToAttack 一开始就插在路面下 0.09 m，关掉 overlap recovery 后会穿地掉下去，所以 Move 也要照 3.5 的行为（PcPhysics.LegacyMove）。
    ("Scripts/Chars/Basic/Character.cs",
     "\t\t\tcharacterController.Move(vector);\n",
     "\t\t\tPcPhysics.LegacyMove(characterController, vector, transform);\n",
     "Character.UpdateMovement: legacy stuck-on-overlap Move"),
    # 3.5 的 active 不是层级的：DeactivateNode 关掉 [Action]/[Task] 节点后，子物体里自己 active 的（Effect、Character、ActivateItem 等）
    # 在 3.5 里照样运行，Unity 6 里却随父节点失活（教程爆炸后一直白屏，见白天修复第 2 节）。结束时把它们移出来。
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
    # 结算统计：AddStat 先把模板 StatText / StatVal / StatBonus 设成 inactive，再克隆模板做每一行。
    # 原版里克隆体是 active 的（代码随后专门把 bonus 为空的克隆体设成 inactive，可见克隆体默认显示）；
    # Unity 4 起克隆体继承模板的 inactive，统计行全不显示，打字动画也没启动，点击跳过时 UIAnimationTypingText 空引用。
    # 克隆后显式激活（白天修复第 4 节）。
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
    # 存档：PlayerStatistics 里有 XmlDictionary<KeyValuePair<string, string>, int>（按武器、敌人统计命中、击杀等）。
    # 新版 XmlSerializer 把 KeyValuePair 键写成空元素，读回时第二个键就重复，整个存档读失败、被当成新档（白天修复第 5 节）。
    ("Packages/StorageManager/Scripts/XmlDictionary.cs",
     "\t\t\tTKey key = (TKey)xmlSerializer.Deserialize(reader);\n",
     "\t\t\tTKey key = (TKey)PcXmlPairs.DeserializeKey(reader, xmlSerializer, typeof(TKey));\n",
     "XmlDictionary.ReadXml: KeyValuePair keys via PcXmlPairs"),
    ("Packages/StorageManager/Scripts/XmlDictionary.cs",
     "\t\t\txmlSerializer.Serialize(writer, key);\n",
     "\t\t\tPcXmlPairs.SerializeKey(writer, xmlSerializer, key);\n",
     "XmlDictionary.WriteXml: KeyValuePair keys via PcXmlPairs"),
    # Resources 里有同名不同类型的资源（导出把依赖的材质、网格也放进了 Resources：weaponicons/shotgun1.mat 与 Shotgun1.png，
    # weapons/*.asset 网格与 *.prefab）。不指定类型的 Resources.Load 可能先拿到材质 / 网格，as 之后变成 null：
    # 免费送的 12-Gauge Pump 进关卡时 Weapon.CreateHudIcon 空引用（白天修复第 9 节）。按原意指定类型。
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
    """对单个文件做一次精确替换；原文必须恰好出现一次。"""
    path = assets_dir / relative
    raw = path.read_bytes()
    # 保留原文件的 BOM 与换行风格：统一按 \n 匹配，写回时恢复 \r\n
    has_crlf = b"\r\n" in raw
    text = raw.decode("utf-8-sig")
    if has_crlf:
        text = text.replace("\r\n", "\n")
    count = text.count(old)
    # 可重复运行：新文已在，说明之前改过了（新文可能包含原文，例如外面包一层 #if，所以先查新文）；
    # 删除类条目（新文为空）则看原文是否已不在
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
    """依次应用 FIXES。"""
    assets_dir = Path(sys.argv[1]) / "Source" / "Runtime"
    for relative, old, new, note in FIXES:
        apply_fix(assets_dir, relative, old, new, note)


if __name__ == "__main__":
    main()
