using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TexturesReplacer
{
	private struct TextureImporterBatch
	{
		public CustomTextureQuality MinimalQuality;

		public string[] Textures;
	}

	public const string TexturesFolder = "CopiedTextures";

	private static TextureImporterBatch[] Batches = new TextureImporterBatch[3]
	{
		new TextureImporterBatch
		{
			MinimalQuality = CustomTextureQuality.Medium,
			Textures = new string[5] { "Assets/3D/Objects/Skies/Textures/CHT_night.png", "Assets/3D/Objects/Skies/Textures/DH_midday.png", "Assets/3D/Objects/Skies/Textures/Dusk1.png", "Assets/3D/Objects/Skies/Textures/Dusk2.png", "Assets/3D/Objects/Skies/Textures/Dusk3.png" }
		},
		new TextureImporterBatch
		{
			MinimalQuality = CustomTextureQuality.High,
			Textures = new string[20]
			{
				"Assets/Sprite Atlases/GUICityMap.png", "Assets/Sprite Atlases/GUICityMap2.png", "Assets/Sprite Atlases/GUICityMap3.png", "Assets/Sprite Atlases/GUICityMap4.png", "Assets/Sprite Atlases/GUICommon.png", "Assets/Sprite Atlases/GUICommon2.png", "Assets/Sprite Atlases/GUIHud.png", "Assets/Sprite Atlases/GUIHud2.png", "Assets/Sprite Atlases/GUIHud3.png", "Assets/Sprite Atlases/GUIHud4.png",
				"Assets/Sprite Atlases/GUIHudAdditive.png", "Assets/Sprite Atlases/GUILocations.png", "Assets/Sprite Atlases/GUIProgressBar.png", "Assets/Sprite Atlases/HealthBar.png", "Assets/Sprite Atlases/PlayerHealthBar.png", "Assets/Sprite Atlases/ProgressBar.png", "Assets/Ui/CityMap/map_left.png", "Assets/Ui/CityMap/map_top_right.png", "Assets/Ui/CityMap/map_down_right.png", "Assets/Ui/CityMap/LightSpot.png"
			}
		},
		new TextureImporterBatch
		{
			MinimalQuality = CustomTextureQuality.Medium,
			Textures = new string[1] { "Assets/Ui/Common/zombies_back.png" }
		}
	};

	private static Dictionary<string, Texture2D> textureMap = null;

	private static void InitTextureMap()
	{
		textureMap = new Dictionary<string, Texture2D>();
		foreach (TextureImporterBatch textureImporterBatch in Batches)
		{
			if (textureImporterBatch.MinimalQuality <= Singleton.Quality.CustomTextureQuality)
			{
				foreach (string path in textureImporterBatch.Textures)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
					textureMap[fileNameWithoutExtension] = null;
				}
			}
			else
			{
			}
		}
	}

	public static void Clear()
	{
		textureMap = null;
	}

	public static void ReplaceTexturesAccordingToQuality()
	{
		if (textureMap == null)
		{
			InitTextureMap();
		}
		HashSet<Material> hashSet = new HashSet<Material>();
		foreach (Object obj in Object.FindSceneObjectsOfType(typeof(Renderer)))
		{
			foreach (Material material in (obj as Renderer).sharedMaterials)
			{
				if (material == null)
				{
					continue;
				}
				if (!hashSet.Add(material))
				{
					continue;
				}
				if (material.mainTexture == null)
				{
					continue;
				}
				Texture2D texture2D = LoadReplacingTexture(material.mainTexture.name);
				if (!texture2D)
				{
					continue;
				}
				material.mainTexture = texture2D;
				// PC port: in the editor this material is a project asset; revert it when Play ends
				PcEditorAssetGuard.Track(material);
				Debug.Log("Replacing texture: " + texture2D);
			}
		}
	}

	private static Texture2D LoadReplacingTexture(string textureName)
	{
		if (textureMap == null)
		{
			InitTextureMap();
		}
		Texture2D value;
		if (!textureMap.TryGetValue(textureName, out value))
		{
			return null;
		}
		if (value == null)
		{
			Object obj = Resources.Load("CopiedTextures/" + textureName);
			if (!obj)
			{
				textureMap.Remove(textureName);
				return null;
			}
			value = obj as Texture2D;
			if (!value)
			{
				return null;
			}
			textureMap[textureName] = value;
		}
		return value;
	}

	public static Texture2D GetTexture(string name)
	{
		if (textureMap == null)
		{
			InitTextureMap();
		}
		if (textureMap.ContainsKey(name) && textureMap[name] != null)
		{
			Debug.LogWarning(string.Format("replaced: {0} -> {1}", name, textureMap[name]));
		}
		Texture2D value;
		if (!textureMap.TryGetValue(name, out value))
		{
			return Resources.Load(name) as Texture2D;
		}
		if (value != null)
		{
			return value;
		}
		value = LoadReplacingTexture(name);
		return value ? value : (Resources.Load(name) as Texture2D);
	}
}
