Shader "PostEffectSimpleBlurShader1" {
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
float _Treshold1;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD3 : TEXCOORD0;
	float4 xlv_TEXCOORD2 : TEXCOORD1;
	float4 xlv_TEXCOORD1 : TEXCOORD2;
	float4 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = v._glesMultiTexCoord0.xy;
  tmpvar_1.xy = tmpvar_2;
  float4 tmpvar_3;
  tmpvar_3.x = _MainTex_TexelSize.x;
  tmpvar_3.y = _MainTex_TexelSize.y;
  tmpvar_3.z = -(_MainTex_TexelSize.x);
  tmpvar_3.w = -(_MainTex_TexelSize.y);
  float4 tmpvar_4;
  tmpvar_4.x = _MainTex_TexelSize.x;
  tmpvar_4.y = _MainTex_TexelSize.y;
  tmpvar_4.z = -(_MainTex_TexelSize.x);
  tmpvar_4.w = -(_MainTex_TexelSize.y);
  float4 tmpvar_5;
  tmpvar_5.x = _MainTex_TexelSize.x;
  tmpvar_5.y = _MainTex_TexelSize.y;
  tmpvar_5.z = -(_MainTex_TexelSize.x);
  tmpvar_5.w = -(_MainTex_TexelSize.y);
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = (v._glesMultiTexCoord0.xyxy + tmpvar_3);
  o.xlv_TEXCOORD2 = (v._glesMultiTexCoord0.xyxy + (tmpvar_4 * 2.0));
  o.xlv_TEXCOORD3 = (v._glesMultiTexCoord0.xyxy + (tmpvar_5 * 3.0));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tmpvar_1;
  float p1;
  float mask;
  float2 coords;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.xy;
  coords = tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = ((coords - 0.5) * 2.0);
  coords = tmpvar_3;
  float tmpvar_4;
  tmpvar_4 = dot(tmpvar_3, tmpvar_3);
  float tmpvar_5;
  tmpvar_5 = (1.0 - (tmpvar_4 * 0.5));
  mask = tmpvar_5;
  float tmpvar_6;
  tmpvar_6 = ((1.0 - tmpvar_4) * _Treshold1);
  p1 = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = ((1.0 - p1) / 6.0);
  float4 tmpvar_8;
  tmpvar_8 = tex2D(_MainTex, i.xlv_TEXCOORD0.xy);
  float4 tmpvar_9;
  tmpvar_9 = tex2D(_MainTex, i.xlv_TEXCOORD1.xy);
  float4 tmpvar_10;
  tmpvar_10 = tex2D(_MainTex, i.xlv_TEXCOORD1.zw);
  float4 tmpvar_11;
  tmpvar_11 = tex2D(_MainTex, i.xlv_TEXCOORD2.xy);
  float4 tmpvar_12;
  tmpvar_12 = tex2D(_MainTex, i.xlv_TEXCOORD2.zw);
  float4 tmpvar_13;
  tmpvar_13 = tex2D(_MainTex, i.xlv_TEXCOORD3.xy);
  float4 tmpvar_14;
  tmpvar_14 = tex2D(_MainTex, i.xlv_TEXCOORD3.zw);
  tmpvar_1 = ((((((((p1 * tmpvar_8) + (tmpvar_7 * tmpvar_9)) + (tmpvar_7 * tmpvar_10)) + (tmpvar_7 * tmpvar_11)) + (tmpvar_7 * tmpvar_12)) + (tmpvar_7 * tmpvar_13)) + (tmpvar_7 * tmpvar_14)) * mask);
  pc_FragColor = tmpvar_1;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback Off
}