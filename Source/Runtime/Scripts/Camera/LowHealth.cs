using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LowHealth : MonoBehaviour
{
	public Shader LowHealthShader;

	public Vector4 Saturation;

	public Vector4 LowHealthSaturation;

	public float LowHealthDesaturationSpeed;

	private Material _lowHealth;

	private bool _isLowHealth;

	private bool _visible;

	private float _lowHealthDesaturationValue;

	private float _opacity = 1f;

	public bool IsLowHealth
	{
		get
		{
			return _isLowHealth;
		}
		set
		{
			_isLowHealth = value;
		}
	}

	public bool isVisible
	{
		get
		{
			return _visible;
		}
	}

	private void Awake()
	{
		if (!Singleton.Quality.LowHealthEffect)
		{
			enabled = false;
			return;
		}
		_lowHealth = new Material(LowHealthShader);
		_lowHealth.hideFlags = HideFlags.HideAndDontSave;
	}

	private void OnEnable()
	{
		PostEffectManager component = GetComponent<PostEffectManager>();
		component.OnShaderPostRender += new PostEffectManager.ShaderPostRender(ShaderRender);
		_visible = true;
		_isLowHealth = true;
	}

	private void OnDisable()
	{
		PostEffectManager component = GetComponent<PostEffectManager>();
		component.OnShaderPostRender -= new PostEffectManager.ShaderPostRender(ShaderRender);
		_visible = false;
		_isLowHealth = false;
	}

	private void Update()
	{
		UpdateLowHealth();
		UpdateOpacity();
	}

	public void UpdateLowHealth()
	{
		if (_isLowHealth)
		{
			_lowHealthDesaturationValue += LowHealthDesaturationSpeed * Time.deltaTime;
			if (_lowHealthDesaturationValue > 1f)
			{
				_lowHealthDesaturationValue = 1f;
			}
		}
		else
		{
			_lowHealthDesaturationValue -= LowHealthDesaturationSpeed * Time.deltaTime;
			if (_lowHealthDesaturationValue < 0f)
			{
				_lowHealthDesaturationValue = 0f;
				_visible = false;
			}
		}
		_lowHealth.SetVector("_Saturation", Vector4.Lerp(Saturation, LowHealthSaturation, _lowHealthDesaturationValue));
	}

	private void UpdateOpacity()
	{
		_opacity = 0.6f + 0.4f * Mathf.Cos((float)Math.PI * 2f * (1f / LowHealthDesaturationSpeed) * Time.time);
		_lowHealth.SetFloat("_Opacity", _opacity);
	}

	public void ShaderRender(RenderTexture sourceText)
	{
		PcBlit.ToScreen(sourceText, _lowHealth);
	}

	private void OnDestroy()
	{
		GetComponent<Camera>().targetTexture = null;
		if ((bool)_lowHealth)
		{
			UnityEngine.Object.DestroyImmediate(_lowHealth);
		}
		_lowHealth = null;
	}
}
