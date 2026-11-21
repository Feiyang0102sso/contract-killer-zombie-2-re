using UnityEngine;

public class PostEffectManager : MonoBehaviour
{
	public enum PostEffect
	{
		Normal,
		Dying,
		Resurrect,
		Charger,
		Bomber,
		DamageBoost,
		LowHealth,
		Count
	}

	public enum PostEffectShader
	{
		NoShader,
		Bloom,
		LowHealth,
		Abberation
	}

	public delegate void ShaderPostRender(RenderTexture sourceText);

	private static readonly string TEXTURE_NAME = "__CaptureTexture";

	private RenderTexture _screenTexture;

	private bool _effectState;

	private LowHealth _lowHealthEffect;

	private ChromaticAberrationWithVignetting _chromaticAberrationEffect;

	private MobileBloom _bloomEffect;

	private PostEffect _currentEffect = PostEffect.Count;

	public ShaderPostRender OnShaderPostRender = delegate
	{
	};

	private void Awake()
	{
		_chromaticAberrationEffect = GetComponent<ChromaticAberrationWithVignetting>();
		_lowHealthEffect = GetComponent<LowHealth>();
		_bloomEffect = GetComponent<MobileBloom>();
		_screenTexture = null;
	}

	private void Update()
	{
		UpdateAbberation();
		UpdateLowHealth();
	}

	private void Start()
	{
		SetPostEffect(PostEffect.Normal, 0f);
	}

	public void OnLowHealth(bool lowHealth)
	{
		if (lowHealth)
		{
			if (!_lowHealthEffect.isVisible && _currentEffect == PostEffect.Normal)
			{
				SetPostEffect(PostEffect.LowHealth, 0f);
			}
		}
		else if (_lowHealthEffect.isVisible)
		{
			_lowHealthEffect.IsLowHealth = false;
		}
		else if (_currentEffect == PostEffect.LowHealth)
		{
			SetPostEffect(PostEffect.Normal, 0f);
		}
	}

	private void UpdateAbberation()
	{
		if (!_chromaticAberrationEffect.isVisible && _chromaticAberrationEffect.enabled)
		{
			SetPostEffect(PostEffect.Normal, 0f);
		}
	}

	private void UpdateLowHealth()
	{
		if (!_lowHealthEffect.isVisible && _lowHealthEffect.enabled)
		{
			SetPostEffect(PostEffect.Normal, 0f);
		}
	}

	private void SetShader(PostEffectShader shaderSet)
	{
		OffPostEffectShaders();
		SetCameraTexture(shaderSet != PostEffectShader.NoShader);
		switch (shaderSet)
		{
		case PostEffectShader.Abberation:
			_chromaticAberrationEffect.enabled = true;
			return;
		case PostEffectShader.Bloom:
			_bloomEffect.enabled = true;
			return;
		case PostEffectShader.LowHealth:
			_lowHealthEffect.enabled = true;
			return;
		case PostEffectShader.NoShader:
			return;
		}
	}

	public void SetPostEffect(PostEffect effectSet, float duration)
	{
		if (duration < 0f)
		{
			effectSet = _currentEffect;
		}
		if (effectSet == _currentEffect && duration >= 0f)
		{
			return;
		}
		if (effectSet == PostEffect.Dying)
		{
			if (_chromaticAberrationEffect == null || !Singleton.Quality.ChromaticAberrations)
			{
				SetShader(PostEffectShader.NoShader);
				_currentEffect = PostEffect.Normal;
				return;
			}
			_currentEffect = effectSet;
			_chromaticAberrationEffect.setEffect = effectSet;
			SetShader(PostEffectShader.Abberation);
			return;
		}
		if (_currentEffect != PostEffect.Normal && effectSet != PostEffect.Normal && duration >= 0f)
		{
			return;
		}
		if (effectSet == PostEffect.LowHealth)
		{
			if (_lowHealthEffect == null || !Singleton.Quality.LowHealthEffect)
			{
				_currentEffect = PostEffect.Normal;
				return;
			}
			_currentEffect = effectSet;
			SetShader(PostEffectShader.LowHealth);
			return;
		}
		if (effectSet == PostEffect.Bomber || effectSet == PostEffect.Charger || effectSet == PostEffect.DamageBoost || effectSet == PostEffect.Resurrect)
		{
			if (_chromaticAberrationEffect == null || !Singleton.Quality.ChromaticAberrations)
			{
				SetShader(PostEffectShader.NoShader);
				_currentEffect = PostEffect.Normal;
				return;
			}
			_currentEffect = effectSet;
			_chromaticAberrationEffect.setEffect = effectSet;
			_chromaticAberrationEffect.setTimer = duration;
			SetShader(PostEffectShader.Abberation);
			return;
		}
		_currentEffect = effectSet;
		SetShader(PostEffectShader.NoShader);
	}

	private void CreateRenderTexture(out RenderTexture renderTexture, int Width, int Height, int DepthBits, FilterMode filterMode)
	{
#if UNITY_STANDALONE
		// PC port: Extreme level keeps 32-bit color and MSAA while a post effect is on
		if (PcExtremeQuality.Enabled)
		{
			renderTexture = PcExtremeQuality.CreatePostEffectTexture(Width, Height);
		}
		else
		{
			renderTexture = new RenderTexture(Width, Height, DepthBits, RenderTextureFormat.RGB565);
		}
#else
		renderTexture = new RenderTexture(Width, Height, DepthBits, RenderTextureFormat.RGB565);
#endif
		renderTexture.Create();
		renderTexture.name = TEXTURE_NAME;
		renderTexture.hideFlags = HideFlags.HideAndDontSave;
		renderTexture.filterMode = filterMode;
	}

	public void SetCameraTexture(bool setText)
	{
		if (setText)
		{
			if (_screenTexture == null)
			{
				CreateRenderTexture(out _screenTexture, Screen.width, Screen.height, 16, FilterMode.Point);
			}
			GetComponent<Camera>().targetTexture = _screenTexture;
		}
		else
		{
			GetComponent<Camera>().targetTexture = null;
		}
	}

	private void OffPostEffectShaders()
	{
		_lowHealthEffect.enabled = false;
		_chromaticAberrationEffect.enabled = false;
		_bloomEffect.enabled = false;
	}

	private void OnPostRender()
	{
		OnShaderPostRender(_screenTexture);
	}
}
