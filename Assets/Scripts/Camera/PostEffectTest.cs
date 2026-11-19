using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Test Image Effect 1")]
public class PostEffectTest : MonoBehaviour
{
	public Shader blurShader;

	private Material m_BlurMaterial;

	public float threshold1 = 0.3f;

	public float threshold2 = 0.3f;

	protected Material blurMaterial
	{
		get
		{
			if (m_BlurMaterial == null)
			{
				m_BlurMaterial = new Material(blurShader);
				m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMaterial;
		}
	}

	protected void OnDisable()
	{
		if ((bool)m_BlurMaterial)
		{
			Object.DestroyImmediate(m_BlurMaterial);
		}
	}

	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
		if (!blurMaterial.shader.isSupported)
		{
			enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		blurMaterial.SetFloat("_Treshold1", threshold1);
		blurMaterial.SetFloat("_Treshold2", threshold2);
		Graphics.Blit(source, destination, blurMaterial);
	}
}
