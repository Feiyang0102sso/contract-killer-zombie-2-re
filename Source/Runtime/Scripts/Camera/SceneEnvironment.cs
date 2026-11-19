using System.Collections.Generic;
using UnityEngine;

public class SceneEnvironment : MonoBehaviour
{
	public Cubemap EnvironmentMap;

	private Dictionary<Material, Material> _sharedMaterialsMap = new Dictionary<Material, Material>();

	private void Awake()
	{
		if ((bool)EnvironmentMap)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				for (int j = 0; j < renderer.sharedMaterials.Length; j++)
				{
					Material material = renderer.sharedMaterials[j];
					if (material.shader.name == "CKZ2/Lightmap-ReflectiveCubemap")
					{
						Material value = null;
						_sharedMaterialsMap.TryGetValue(material, out value);
						if (value == null)
						{
							value = new Material(material);
							value.SetTexture("_EnvTex", EnvironmentMap);
							value.hideFlags = HideFlags.HideAndDontSave;
							_sharedMaterialsMap[material] = value;
						}
						Material[] sharedMaterials = renderer.sharedMaterials;
						sharedMaterials[j] = value;
						renderer.sharedMaterials = sharedMaterials;
					}
				}
			}
		}
		Object.Destroy(this);
	}
}
