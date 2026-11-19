using System;
using UnityEngine;

public class MobileBloom : MonoBehaviour
{
	public float intensity = 0.5f;

	public Color colorMix = Color.white;

	public float colorMixBlend = 0.25f;

	public bool good = true;

	public float agonyTint;

	public float agonySpeed = 2.75f;

	private Shader bloomShader;

	private Material apply;

	private RenderTextureFormat rtFormat = RenderTextureFormat.Default;

	private void Start()
	{
		FindShaders();
		CreateMaterials();
	}

	private void OnEnable()
	{
		PostEffectManager component = GetComponent<PostEffectManager>();
		component.OnShaderPostRender += new PostEffectManager.ShaderPostRender(ShaderRender);
	}

	private void OnDisable()
	{
		PostEffectManager component = GetComponent<PostEffectManager>();
		component.OnShaderPostRender -= new PostEffectManager.ShaderPostRender(ShaderRender);
	}

	private void FindShaders()
	{
		if (!bloomShader)
		{
			bloomShader = Shader.Find("Hidden/MobileBloom");
		}
	}

	private void CreateMaterials()
	{
		if (!apply)
		{
			apply = new Material(bloomShader);
			apply.hideFlags = HideFlags.DontSave;
		}
	}

	private void OnDamage()
	{
		agonyTint = 1f;
	}

	public void ShaderRender(RenderTexture sourceText)
	{
		agonyTint = Mathf.Clamp01(agonyTint - Time.deltaTime * agonySpeed);
		RenderTexture temporary = RenderTexture.GetTemporary(sourceText.width / 4, sourceText.height / 4, 0, rtFormat);
		RenderTexture temporary2 = RenderTexture.GetTemporary(sourceText.width / 4, sourceText.height / 4, 0, rtFormat);
		apply.SetColor("_ColorMix", colorMix);
		apply.SetVector("_Parameter", new Vector4(colorMixBlend * 0.25f, 0f, 0f, 1f - intensity - agonyTint));
		Graphics.Blit(sourceText, temporary, apply, (agonyTint < 0.5f) ? 1 : 5);
		Graphics.Blit(temporary, temporary2, apply, 2);
		Graphics.Blit(temporary2, temporary, apply, 3);
		apply.SetTexture("_Bloom", temporary);
		Graphics.Blit(sourceText, null, apply, good ? 4 : 0);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
