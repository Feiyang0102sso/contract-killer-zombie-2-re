using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class ChromaticAberrationWithVignetting : MonoBehaviour
{
	public enum AberrationType
	{
		LONGITUDINAL_HORIZONTAL,
		LONGITUDINAL_VERTICAL,
		LONGITUDINAL_RADIAL,
		LATERAL
	}

	[SerializeField]
	private AberrationType m_aberrationType;

	[SerializeField]
	private float m_chromaticAberration = 2f;

	[SerializeField]
	private bool m_vignetting;

	[SerializeField]
	private Color m_vignetteColor = Color.black;

	[SerializeField]
	private float m_blur;

	[SerializeField]
	private float m_blurSpread = 1.5f;

	private Material m_material;

	private Material m_separableBlurMaterial;

	private bool _visible;

	private Color _startColor;

	private CountdownTimer _shakingTimer;

	private PostEffectManager.PostEffect _curEffect;

	private float _timer = 10f;

	public Shader ChromaticAberrationShader;

	public Shader BlurShader;

	private static readonly string[] ABERRATION_TYPE_KEYWORDS = new string[4] { "LONGITUDINAL_HORIZONTAL", "LONGITUDINAL_VERTICAL", "LONGITUDINAL_RADIAL", "LATERAL" };

	private static readonly string[] VIGNETTING_KEYWORDS = new string[2] { "VIGNETTING_OFF", "VIGNETTING_ON" };

	private RenderTexture m_fullRezColor;

	private RenderTexture m_halfRezColor;

	private RenderTexture m_quarterRezColor;

	private RenderTexture m_secondQuarterRezColor;

	public AberrationType aberrationType
	{
		get
		{
			return m_aberrationType;
		}
		set
		{
			if ((bool)m_material)
			{
				Shader.DisableKeyword(ABERRATION_TYPE_KEYWORDS[(int)m_aberrationType]);
			}
			m_aberrationType = value;
			if ((bool)m_material)
			{
				Shader.EnableKeyword(ABERRATION_TYPE_KEYWORDS[(int)m_aberrationType]);
			}
		}
	}

	public float chromaticAberration
	{
		get
		{
			return m_chromaticAberration;
		}
		set
		{
			m_chromaticAberration = value;
			if ((bool)m_material)
			{
				m_material.SetFloat("_ChromaticAberration", m_chromaticAberration);
			}
		}
	}

	public bool vignetting
	{
		get
		{
			return m_vignetting;
		}
		set
		{
			if ((bool)m_material)
			{
				Shader.DisableKeyword(VIGNETTING_KEYWORDS[m_vignetting ? 1 : 0]);
			}
			m_vignetting = value;
			if ((bool)m_material)
			{
				Shader.EnableKeyword(VIGNETTING_KEYWORDS[m_vignetting ? 1 : 0]);
			}
		}
	}

	public Color vignetteColor
	{
		get
		{
			return m_vignetteColor;
		}
		set
		{
			m_vignetteColor = value;
			if ((bool)m_material)
			{
				m_material.SetColor("_VignetteColor", Color.white - m_vignetteColor);
			}
		}
	}

	public float blur
	{
		get
		{
			return m_blur;
		}
		set
		{
			m_blur = value;
			if (m_blur < 0f)
			{
				m_blur = 0f;
				DestroyBlurTemps();
			}
			if ((bool)m_material)
			{
				m_material.SetFloat("_Blur", m_blur);
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
			if (m_blurSpread < 0f)
			{
				m_blurSpread = 0f;
			}
		}
	}

	public bool isVisible
	{
		get
		{
			return _visible;
		}
	}

	public PostEffectManager.PostEffect setEffect
	{
		set
		{
			_curEffect = value;
		}
	}

	public float setTimer
	{
		set
		{
			_timer = value;
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
		m_material = new Material(ChromaticAberrationShader);
		m_material.hideFlags = HideFlags.HideAndDontSave;
		m_separableBlurMaterial = new Material(BlurShader);
		m_separableBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	private void Start()
	{
		for (int i = 0; i < ABERRATION_TYPE_KEYWORDS.Length; i++)
		{
			Shader.DisableKeyword(ABERRATION_TYPE_KEYWORDS[i]);
		}
		Shader.EnableKeyword(ABERRATION_TYPE_KEYWORDS[(int)m_aberrationType]);
		m_material.SetFloat("_ChromaticAberration", m_chromaticAberration);
		for (int j = 0; j < VIGNETTING_KEYWORDS.Length; j++)
		{
			Shader.DisableKeyword(VIGNETTING_KEYWORDS[j]);
		}
		Shader.EnableKeyword(VIGNETTING_KEYWORDS[m_vignetting ? 1 : 0]);
		m_material.SetColor("_VignetteColor", Color.white - m_vignetteColor);
		m_material.SetFloat("_Blur", m_blur);
	}

	private void Update()
	{
		AbberationEffectUpdate();
	}

	private void OnEnable()
	{
		_visible = true;
		PostEffectManager component = GetComponent<PostEffectManager>();
		component.OnShaderPostRender += new PostEffectManager.ShaderPostRender(ShaderRender);
		switch (_curEffect)
		{
		case PostEffectManager.PostEffect.Bomber:
			_shakingTimer = new CountdownTimer(2f);
			m_chromaticAberration = 0f;
			m_vignetting = true;
			vignetteColor = new Color(0.86f, 0.91f, 0.2f);
			_startColor = new Color(0.86f, 0.91f, 0.2f);
			m_blur = 8f;
			m_blurSpread = 0.6f;
			break;
		case PostEffectManager.PostEffect.Charger:
			_shakingTimer = new CountdownTimer(1f);
			m_chromaticAberration = 0f;
			m_vignetting = true;
			vignetteColor = new Color(0.95f, 0.68f, 0.64f);
			_startColor = new Color(0.95f, 0.68f, 0.64f);
			m_blur = 6.5f;
			m_blurSpread = 0.25f;
			break;
		case PostEffectManager.PostEffect.DamageBoost:
			_shakingTimer = new CountdownTimer(_timer);
			m_aberrationType = AberrationType.LATERAL;
			m_chromaticAberration = 10f;
			m_vignetting = true;
			vignetteColor = new Color(0.61f, 0.59f, 0.8f);
			_startColor = new Color(0.61f, 0.59f, 0.8f);
			m_blur = 1.5f;
			m_blurSpread = 0.6f;
			break;
		case PostEffectManager.PostEffect.Dying:
			m_aberrationType = AberrationType.LONGITUDINAL_RADIAL;
			m_chromaticAberration = 2f;
			m_vignetting = true;
			vignetteColor = new Color(0.25f, 0f, 0f, 1f);
			_startColor = new Color(0.25f, 0f, 0f, 1f);
			m_blur = 3f;
			m_blurSpread = 1f;
			break;
		case PostEffectManager.PostEffect.Resurrect:
			m_aberrationType = AberrationType.LONGITUDINAL_RADIAL;
			m_chromaticAberration = 10f;
			m_vignetting = false;
			m_blur = 1f;
			m_blurSpread = 1f;
			break;
		}
	}

	private void OnDisable()
	{
		PostEffectManager component = GetComponent<PostEffectManager>();
		component.OnShaderPostRender -= new PostEffectManager.ShaderPostRender(ShaderRender);
		_shakingTimer = null;
		_visible = false;
	}

	private void AbberationEffectUpdate()
	{
		Vector3 vector = new Vector3(_startColor.r, _startColor.g, _startColor.b);
		if (_curEffect == PostEffectManager.PostEffect.Bomber || _curEffect == PostEffectManager.PostEffect.Charger)
		{
			vector = Vector3.Lerp(vector, new Vector3(1f, 1f, 1f), _shakingTimer.Progress());
			vignetteColor = new Color(vector.x, vector.y, vector.z);
			_visible = _shakingTimer.IsRunning();
		}
		else if (_curEffect == PostEffectManager.PostEffect.DamageBoost)
		{
			float num = Mathf.Pow(_shakingTimer.Progress(), 4f);
			m_chromaticAberration = 10f * (1f - num);
			vector = Vector3.Lerp(vector, new Vector3(1f, 1f, 1f), _shakingTimer.Progress());
			vignetteColor = new Color(vector.x, vector.y, vector.z);
			m_blur = 1.5f * (1f - num);
			_visible = _shakingTimer.IsRunning();
		}
	}

	public void ShaderRender(RenderTexture sourceText)
	{
		int unusedLocal1;
		int unusedLocal2;
		if (m_fullRezColor == null || m_fullRezColor.width != sourceText.width || m_fullRezColor.height != sourceText.height)
		{
			m_fullRezColor = new RenderTexture(sourceText.width, sourceText.height, 0);
			m_halfRezColor = new RenderTexture(sourceText.width / 2, sourceText.height / 2, 0);
			m_quarterRezColor = new RenderTexture(sourceText.width / 4, sourceText.height / 4, 0);
			m_secondQuarterRezColor = new RenderTexture(sourceText.width / 4, sourceText.height / 4, 0);
			m_material.SetTexture("_BlurTex", m_quarterRezColor);
		}
		if (m_blur > 0f)
		{
			Graphics.Blit(sourceText, m_halfRezColor);
			Graphics.Blit(m_halfRezColor, m_quarterRezColor);
			for (int i = 0; i < 1; i++)
			{
				m_separableBlurMaterial.SetVector("offsets", new Vector4(m_blurSpread, 0f, 0f, 0f));
				Graphics.Blit(m_quarterRezColor, m_secondQuarterRezColor, m_separableBlurMaterial, 1);
				m_separableBlurMaterial.SetVector("offsets", new Vector4(0f, m_blurSpread, 0f, 0f));
				Graphics.Blit(m_secondQuarterRezColor, m_quarterRezColor, m_separableBlurMaterial, 1);
			}
			Graphics.Blit(sourceText, m_fullRezColor, m_material, 0);
			sourceText = m_fullRezColor;
		}
		if (m_chromaticAberration != 0f || m_vignetting)
		{
			Graphics.Blit(sourceText, null, m_material, 1);
		}
		else
		{
			Graphics.Blit(sourceText, null, null);
		}
	}

	private void DestroyBlurTemps()
	{
		if (m_fullRezColor != null)
		{
			UnityEngine.Object.DestroyImmediate(m_fullRezColor);
			m_fullRezColor = null;
		}
		if (m_halfRezColor != null)
		{
			UnityEngine.Object.DestroyImmediate(m_halfRezColor);
			m_halfRezColor = null;
		}
		if (m_quarterRezColor != null)
		{
			UnityEngine.Object.DestroyImmediate(m_quarterRezColor);
			m_quarterRezColor = null;
		}
		if (m_secondQuarterRezColor != null)
		{
			UnityEngine.Object.DestroyImmediate(m_secondQuarterRezColor);
			m_secondQuarterRezColor = null;
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_material)
		{
			UnityEngine.Object.DestroyImmediate(m_material);
		}
		m_material = null;
		DestroyBlurTemps();
	}
}
