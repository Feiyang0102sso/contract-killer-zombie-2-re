Shader "CKZ2/Lightmap + Emissive Oscillating" {
Properties {
 _MainTex ("Base (RGB) Emissive (A)", 2D) = "white" {}
 _EmissiveColor ("Emissive Color", Color) = (0.5,0.5,0.5,1)
 _OscillationFrequency ("Oscillation Frequency", Float) = 1
}
SubShader { 
 LOD 100
 Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float _OscillationFrequency;
sampler2D _MainTex;
float3 _EmissiveColor;
struct appdata_pc {
	float4 _glesMultiTexCoord1 : TEXCOORD1;
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
  tmpvar_1.x = clamp(((1.0 + sin(((v._glesVertex.x + v._glesVertex.z) + (_Time.y * _OscillationFrequency)))) / 2.0), 0.0, 1.0);
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = ((v._glesMultiTexCoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
  o.xlv_TEXCOORD2 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz * (2.0 * UNITY_SAMPLE_TEX2D(unity_Lightmap, i.xlv_TEXCOORD1).xyz));
  float3 tmpvar_2;
  tmpvar_2 = (c.xyz + (_EmissiveColor * (tmpvar_1.w * i.xlv_TEXCOORD2.x)));
  c.xyz = tmpvar_2;
  pc_FragColor = c;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback "Diffuse"
}