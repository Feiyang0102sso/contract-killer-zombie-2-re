Shader "Normandy/Postprocess/LowHealth" {
Properties {
 _MainTex ("Background", 2D) = "white" {}
}
SubShader { 
 Pass {
  // PC port: was "ZTest False" with default back-face culling; PcBlit's full-screen quad got rejected, the backbuffer kept
  // the last frame (3D frozen at low health, HUD still updating). Same state as the working chromatic aberration shader.
  ZTest Always
  ZWrite Off
  Cull Off
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _Saturation;
float _Opacity;
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
  float4 pos;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  pos = tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = v._glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_3;
  o.pos = pos;
  o.xlv_TEXCOORD0 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tmpvar_1;
  float3 tmpvar_2;
  tmpvar_2 = tex2D(_MainTex, i.xlv_TEXCOORD0).xyz;
  float3 tmpvar_3;
  tmpvar_3 = lerp(tmpvar_2, ((float3)(dot(tmpvar_2, _Saturation.xyz))), _Saturation.www);
  float2 tmpvar_4;
  tmpvar_4 = (i.xlv_TEXCOORD0 - 0.5);
  float3 tmpvar_5;
  tmpvar_5 = lerp(tmpvar_3, float3(1.0, 0.0, 0.0), ((float3)((_Opacity * dot(tmpvar_4, tmpvar_4)))));
  tmpvar_1 = tmpvar_5.xyzz;
  pc_FragColor = tmpvar_1;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback Off
}