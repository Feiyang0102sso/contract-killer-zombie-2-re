Shader "Hidden/Sepiatone Effect" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
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
static const float4x4 pc_TextureMatrix0 = float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1);
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
  float4 tmpvar_3;
  tmpvar_3.zw = float2(0.0, 0.0);
  tmpvar_3.x = v._glesMultiTexCoord0.x;
  tmpvar_3.y = v._glesMultiTexCoord0.y;
  tmpvar_2 = (mul(pc_TextureMatrix0, tmpvar_3)).xy;
  tmpvar_1 = tmpvar_2;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 xlat_var_output;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  xlat_var_output = (float4(0.191, -0.054, -0.221, 0.0) + dot(float3(0.299, 0.587, 0.114), tmpvar_1.xyz));
  xlat_var_output.w = tmpvar_1.w;
  pc_FragColor = xlat_var_output;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback Off
}