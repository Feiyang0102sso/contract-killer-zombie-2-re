using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RadialBlur : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_center;

	[SerializeField]
	private float m_blurSpread = 3f;

	private Material m_radialBlurMaterial;

	public Vector2 center
	{
		get
		{
			return m_center;
		}
		set
		{
			m_center = value;
			if ((bool)m_radialBlurMaterial)
			{
				m_radialBlurMaterial.SetVector("_Center", new Vector4(m_center.x * 0.5f + 0.5f, 1f - (m_center.y * 0.5f + 0.5f), 0f, 0f));
			}
		}
	}

	public float blurSpread
	{
		get
		{
			return m_blurSpread;
		}
		set
		{
			m_blurSpread = value;
			if ((bool)m_radialBlurMaterial)
			{
				m_radialBlurMaterial.SetFloat("_BlurSpread", -m_blurSpread);
			}
		}
	}

	private void Awake()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
		if (!SystemInfo.supportsRenderTextures)
		{
			enabled = false;
			return;
		}
		m_radialBlurMaterial = new Material(Shader.Find("Mobile/RadialBlur"));
		m_radialBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	private void Start()
	{
		m_radialBlurMaterial.SetVector("_Center", new Vector4(m_center.x * 0.5f + 0.5f, 1f - (m_center.y * 0.5f + 0.5f), 0f, 0f));
		m_radialBlurMaterial.SetFloat("_BlurSpread", -m_blurSpread);
	}

	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, m_radialBlurMaterial);
	}

	private void OnDestroy()
	{
		Object.DestroyImmediate(m_radialBlurMaterial);
		m_radialBlurMaterial = null;
	}
}
