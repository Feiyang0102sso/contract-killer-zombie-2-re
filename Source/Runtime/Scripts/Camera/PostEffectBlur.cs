using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Test Image Effect 2")]
public class PostEffectBlur : MonoBehaviour
{
	public Shader blurShader;

	private Material m_BlurMaterial;

	public float threshold1 = 0.9f;

	public float blurDist1 = 1f;

	public float blurDist2 = 3f;

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
		blurMaterial.SetFloat("_blurDist1", blurDist1);
		blurMaterial.SetFloat("_blurDist2", blurDist2);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Graphics.Blit(source, destination, blurMaterial);
		Debug.Log(string.Format("Blur {0:0.#########}", Time.realtimeSinceStartup - realtimeSinceStartup));
	}
}
