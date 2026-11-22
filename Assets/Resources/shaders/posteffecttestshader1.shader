Shader "PostEffectTestShader1" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Treshold ("Treshold", Float) = 0.2
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
float4 _MainTex_TexelSize;
float _Treshold2;
float _Treshold1;
sampler2D _MainTex;
static const float4x4 pc_TextureMatrix0 = float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1);
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD0_4 : TEXCOORD0;
	float2 xlv_TEXCOORD0_3 : TEXCOORD1;
	float2 xlv_TEXCOORD0_2 : TEXCOORD2;
	float2 xlv_TEXCOORD0_1 : TEXCOORD3;
	float2 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1[5];
  float4 tmpvar_2;
  tmpvar_2.zw = float2(0.0, 0.0);
  tmpvar_2.x = v._glesMultiTexCoord0.x;
  tmpvar_2.y = v._glesMultiTexCoord0.y;
  float4 tmpvar_3;
  tmpvar_3 = (mul(pc_TextureMatrix0, tmpvar_2));
  tmpvar_1[0] = tmpvar_3.xy;
  float2 tmpvar_4;
  tmpvar_4.x = -(_MainTex_TexelSize.x);
  tmpvar_4.y = -(_MainTex_TexelSize.y);
  tmpvar_1[1] = (tmpvar_3.xy + tmpvar_4);
  float2 tmpvar_5;
  tmpvar_5.x = _MainTex_TexelSize.x;
  tmpvar_5.y = -(_MainTex_TexelSize.y);
  tmpvar_1[2] = (tmpvar_3.xy + tmpvar_5);
  float2 tmpvar_6;
  tmpvar_6.x = -(_MainTex_TexelSize.x);
  tmpvar_6.y = _MainTex_TexelSize.y;
  tmpvar_1[3] = (tmpvar_3.xy + tmpvar_6);
  tmpvar_1[4] = (tmpvar_3.xy + _MainTex_TexelSize.xy);
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1[0];
  o.xlv_TEXCOORD0_1 = tmpvar_1[1];
  o.xlv_TEXCOORD0_2 = tmpvar_1[2];
  o.xlv_TEXCOORD0_3 = tmpvar_1[3];
  o.xlv_TEXCOORD0_4 = tmpvar_1[4];
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float2 tmpvar_1[5];
  tmpvar_1[0] = i.xlv_TEXCOORD0;
  tmpvar_1[1] = i.xlv_TEXCOORD0_1;
  tmpvar_1[2] = i.xlv_TEXCOORD0_2;
  tmpvar_1[3] = i.xlv_TEXCOORD0_3;
  tmpvar_1[4] = i.xlv_TEXCOORD0_4;
  float4 color;
  float p2;
  float p1;
  p1 = _Treshold1;
  p2 = _Treshold2;
  float4 tmpvar_2;
  tmpvar_2 = tex2D(_MainTex, tmpvar_1[0]);
  color = tmpvar_2;
  float4 tmpvar_3;
  tmpvar_3 = tex2D(_MainTex, tmpvar_1[1]);
  float4 tmpvar_4;
  tmpvar_4 = tex2D(_MainTex, tmpvar_1[2]);
  float4 tmpvar_5;
  tmpvar_5 = tex2D(_MainTex, tmpvar_1[3]);
  float4 tmpvar_6;
  tmpvar_6 = tex2D(_MainTex, tmpvar_1[4]);
  float4 tmpvar_7;
  tmpvar_7 = (((((color * p1) + (p2 * tmpvar_3)) + (p2 * tmpvar_4)) + (p2 * tmpvar_5)) + (p2 * tmpvar_6));
  color = tmpvar_7;
  pc_FragColor = tmpvar_7;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback Off
}