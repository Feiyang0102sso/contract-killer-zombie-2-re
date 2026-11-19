Shader "CKZ2/Self-Illuminated (Simple)" {
Properties {
 _Multiplier ("Multiplier", Float) = 1
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float _Multiplier;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD0 : TEXCOORD0;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = v._glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_2;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float4 tmpvar_2;
  tmpvar_2 = (tmpvar_1 * _Multiplier);
  c = tmpvar_2;
  pc_FragColor = c;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback "Unlit/Texture"
}