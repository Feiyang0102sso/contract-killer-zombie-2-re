using UnityEngine;

public class AnimateSharedMaterial : MonoBehaviour
{
	private Material _material;

	private Material _sharedMaterial;

	private void Start()
	{
		_sharedMaterial = GetComponent<Renderer>().sharedMaterial;
		_material = GetComponent<Renderer>().material;
	}

	private void Update()
	{
		_sharedMaterial.CopyPropertiesFromMaterial(_material);
	}
}
