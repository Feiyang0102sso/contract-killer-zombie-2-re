Shader "CKZ2/Self-Illuminated with Intensity Ramp (Simple)" {
Properties {
 _Multiplier ("Multiplier", Float) = 1
 _AnimationSpeed ("Animation Speed", Float) = 1
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _RampTex ("Intensity (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 400
 Tags { "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float _AnimationSpeed;
sampler2D _RampTex;
float _Multiplier;
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
  float2 tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = v._glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.y = 0.0;
  tmpvar_4.x = (_Time.y * _AnimationSpeed);
  tmpvar_2 = tmpvar_4;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_RampTex, i.xlv_TEXCOORD1);
  float4 tmpvar_2;
  tmpvar_2 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float4 tmpvar_3;
  tmpvar_3 = clamp(((tmpvar_2 * _Multiplier) * tmpvar_1.x), 0.0, 1.0);
  c = tmpvar_3;
  pc_FragColor = c;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback "CKZ2/Self-Illuminated (Simple)"
}