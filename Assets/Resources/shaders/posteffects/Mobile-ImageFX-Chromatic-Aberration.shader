Shader "Mobile/ImageFX/ChromaticAberration" {
Properties {
 _MainTex ("Base", 2D) = "white" {}
 _BlurTex ("Blurred Base", 2D) = "white" {}
 _ChromaticAberration ("Chromatic Aberration", Float) = 2
 _VignetteColor ("Vignette Color", Color) = (0,0,0,1)
 _Blur ("Blur", Float) = 0.1
}
SubShader { 
 Pass {
  ZTest Always
  ZWrite Off
  Cull Off
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by gles_to_hlsl.py
sampler2D _MainTex;
sampler2D _BlurTex;
float _Blur;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = ((v._glesMultiTexCoord0.xy - 0.5) * 2.0);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float blur;
  float tmpvar_1;
  tmpvar_1 = clamp((_Blur * dot(i.xlv_TEXCOORD1, i.xlv_TEXCOORD1)), 0.0, 1.0);
  blur = tmpvar_1;
  pc_FragColor = lerp(tex2D(_MainTex, i.xlv_TEXCOORD0), tex2D(_BlurTex, i.xlv_TEXCOORD0), ((float4)(blur)));
	return pc_FragColor;
}
ENDCG
 }
 Pass {
  ZTest Always
  ZWrite Off
  Cull Off
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile LATERAL LONGITUDINAL_HORIZONTAL LONGITUDINAL_RADIAL LONGITUDINAL_VERTICAL
#pragma multi_compile VIGNETTING_OFF VIGNETTING_ON
// PC port: translated from the original GLES code by gles_to_hlsl.py
#if defined(LONGITUDINAL_HORIZONTAL) && defined(VIGNETTING_ON)
float4 _MainTex_TexelSize;
float _ChromaticAberration;
float4 _VignetteColor;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  tmpvar_1 = v._glesMultiTexCoord0.xy;
  tmpvar_1.x = (v._glesMultiTexCoord0.x + (_MainTex_TexelSize.x * _ChromaticAberration));
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = ((v._glesMultiTexCoord0.xy - 0.5) * 2.0);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 vignette;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  col.y = tex2D(_MainTex, i.xlv_TEXCOORD1).y;
  float4 tmpvar_1;
  tmpvar_1 = (_VignetteColor * dot(i.xlv_TEXCOORD2, i.xlv_TEXCOORD2));
  vignette = tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (col - (col * vignette));
  col = tmpvar_2;
  pc_FragColor = tmpvar_2;
	return pc_FragColor;
}
#elif defined(VIGNETTING_ON) && defined(LONGITUDINAL_VERTICAL)
float4 _MainTex_TexelSize;
float _ChromaticAberration;
float4 _VignetteColor;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  tmpvar_1 = v._glesMultiTexCoord0.xy;
  tmpvar_1.y = (v._glesMultiTexCoord0.y + (_MainTex_TexelSize.y * _ChromaticAberration));
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = ((v._glesMultiTexCoord0.xy - 0.5) * 2.0);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 vignette;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  col.y = tex2D(_MainTex, i.xlv_TEXCOORD1).y;
  float4 tmpvar_1;
  tmpvar_1 = (_VignetteColor * dot(i.xlv_TEXCOORD2, i.xlv_TEXCOORD2));
  vignette = tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (col - (col * vignette));
  col = tmpvar_2;
  pc_FragColor = tmpvar_2;
	return pc_FragColor;
}
#elif defined(VIGNETTING_ON) && defined(LONGITUDINAL_RADIAL)
float4 _MainTex_TexelSize;
float _ChromaticAberration;
float4 _VignetteColor;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = (v._glesMultiTexCoord0.xy + ((((v._glesMultiTexCoord0.xy - 0.5) * 2.0) * _MainTex_TexelSize.xy) * _ChromaticAberration));
  o.xlv_TEXCOORD2 = ((v._glesMultiTexCoord0.xy - 0.5) * 2.0);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 vignette;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  col.y = tex2D(_MainTex, i.xlv_TEXCOORD1).y;
  float4 tmpvar_1;
  tmpvar_1 = (_VignetteColor * dot(i.xlv_TEXCOORD2, i.xlv_TEXCOORD2));
  vignette = tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (col - (col * vignette));
  col = tmpvar_2;
  pc_FragColor = tmpvar_2;
	return pc_FragColor;
}
#elif defined(VIGNETTING_ON) && defined(LATERAL)
float4 _VignetteColor;
float4 _MainTex_TexelSize;
sampler2D _MainTex;
float _ChromaticAberration;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD2 = ((v._glesMultiTexCoord0.xy - 0.5) * 2.0);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 vignette;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float tmpvar_1;
  tmpvar_1 = dot(i.xlv_TEXCOORD2, i.xlv_TEXCOORD2);
  float2 tmpvar_2;
  tmpvar_2 = (i.xlv_TEXCOORD0 - (((_MainTex_TexelSize.xy * _ChromaticAberration) * i.xlv_TEXCOORD2) * tmpvar_1));
  col.y = tex2D(_MainTex, tmpvar_2).y;
  float4 tmpvar_3;
  tmpvar_3 = (_VignetteColor * tmpvar_1);
  vignette = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4 = (col - (col * vignette));
  col = tmpvar_4;
  pc_FragColor = tmpvar_4;
	return pc_FragColor;
}
#elif defined(LONGITUDINAL_HORIZONTAL) && !defined(VIGNETTING_ON)
float4 _MainTex_TexelSize;
float _ChromaticAberration;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  tmpvar_1 = v._glesMultiTexCoord0.xy;
  tmpvar_1.x = (v._glesMultiTexCoord0.x + (_MainTex_TexelSize.x * _ChromaticAberration));
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  col.y = tex2D(_MainTex, i.xlv_TEXCOORD1).y;
  pc_FragColor = col;
	return pc_FragColor;
}
#elif !defined(VIGNETTING_ON) && defined(LONGITUDINAL_VERTICAL)
float4 _MainTex_TexelSize;
float _ChromaticAberration;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  tmpvar_1 = v._glesMultiTexCoord0.xy;
  tmpvar_1.y = (v._glesMultiTexCoord0.y + (_MainTex_TexelSize.y * _ChromaticAberration));
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  col.y = tex2D(_MainTex, i.xlv_TEXCOORD1).y;
  pc_FragColor = col;
	return pc_FragColor;
}
#elif !defined(VIGNETTING_ON) && defined(LONGITUDINAL_RADIAL)
float4 _MainTex_TexelSize;
float _ChromaticAberration;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = (v._glesMultiTexCoord0.xy + ((((v._glesMultiTexCoord0.xy - 0.5) * 2.0) * _MainTex_TexelSize.xy) * _ChromaticAberration));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  col.y = tex2D(_MainTex, i.xlv_TEXCOORD1).y;
  pc_FragColor = col;
	return pc_FragColor;
}
#else // VIGNETTING_OFF LATERAL
float4 _MainTex_TexelSize;
sampler2D _MainTex;
float _ChromaticAberration;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD2 = ((v._glesMultiTexCoord0.xy - 0.5) * 2.0);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 col;
  col = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float2 tmpvar_1;
  tmpvar_1 = (i.xlv_TEXCOORD0 - (((_MainTex_TexelSize.xy * _ChromaticAberration) * i.xlv_TEXCOORD2) * dot(i.xlv_TEXCOORD2, i.xlv_TEXCOORD2)));
  col.y = tex2D(_MainTex, tmpvar_1).y;
  pc_FragColor = col;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback Off
}