Shader "Mobile/SeparableBlur" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "" {}
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
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float4 offsets;
float4 _MainTex_TexelSize;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD4 : TEXCOORD0;
	float2 xlv_TEXCOORD3 : TEXCOORD1;
	float2 xlv_TEXCOORD2 : TEXCOORD2;
	float2 xlv_TEXCOORD1 : TEXCOORD3;
	float2 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_2;
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = (v._glesMultiTexCoord0.xy + ((offsets.xy * _MainTex_TexelSize.xy) * 1.38462));
  o.xlv_TEXCOORD2 = (v._glesMultiTexCoord0.xy - ((offsets.xy * _MainTex_TexelSize.xy) * 1.38462));
  o.xlv_TEXCOORD3 = (v._glesMultiTexCoord0.xy + ((offsets.xy * _MainTex_TexelSize.xy) * 3.23077));
  o.xlv_TEXCOORD4 = (v._glesMultiTexCoord0.xy - ((offsets.xy * _MainTex_TexelSize.xy) * 3.23077));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = (((((0.227027 * tex2D(_MainTex, i.xlv_TEXCOORD0)) + (0.316216 * tex2D(_MainTex, i.xlv_TEXCOORD1))) + (0.316216 * tex2D(_MainTex, i.xlv_TEXCOORD2))) + (0.0702703 * tex2D(_MainTex, i.xlv_TEXCOORD3))) + (0.0702703 * tex2D(_MainTex, i.xlv_TEXCOORD4)));
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
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float4 offsets;
float4 _MainTex_TexelSize;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_2;
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = (v._glesMultiTexCoord0.xyxy + ((float4(1.38462, 1.38462, -1.38462, -1.38462) * _MainTex_TexelSize.xyxy) * offsets.xyxy));
  o.xlv_TEXCOORD2 = (v._glesMultiTexCoord0.xyxy + ((float4(3.23077, 3.23077, -3.23077, -3.23077) * _MainTex_TexelSize.xyxy) * offsets.xyxy));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = (((((0.227027 * tex2D(_MainTex, i.xlv_TEXCOORD0)) + (0.316216 * tex2D(_MainTex, i.xlv_TEXCOORD1.xy))) + (0.316216 * tex2D(_MainTex, i.xlv_TEXCOORD1.zw))) + (0.0702703 * tex2D(_MainTex, i.xlv_TEXCOORD2.xy))) + (0.0702703 * tex2D(_MainTex, i.xlv_TEXCOORD2.zw)));
	return pc_FragColor;
}
ENDCG
 }
}
Fallback Off
}