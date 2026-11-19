using UnityEngine;

public class AnimateSharedMaterial : MonoBehaviour
{
	private Material _material;

	private Material _sharedMaterial;

	private void Start()
	{
		_sharedMaterial = renderer.sharedMaterial;
		_material = renderer.material;
	}

	private void Update()
	{
		_sharedMaterial.CopyPropertiesFromMaterial(_material);
	}
}
