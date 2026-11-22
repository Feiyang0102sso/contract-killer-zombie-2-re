Shader "Hidden/MobileBloom" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Bloom ("Bloom (RGB)", 2D) = "black" {}
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
sampler2D _Bloom;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD0 : TEXCOORD0;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_2;
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xyxy;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = (tex2D(_MainTex, i.xlv_TEXCOORD0.xy) + tex2D(_Bloom, i.xlv_TEXCOORD0.zw));
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _MainTex_TexelSize;
float4 _Parameter;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1_3 : TEXCOORD0;
	float2 xlv_TEXCOORD1_2 : TEXCOORD1;
	float2 xlv_TEXCOORD1_1 : TEXCOORD2;
	float2 xlv_TEXCOORD1 : TEXCOORD3;
	float2 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float2 tmpvar_2[4];
  float4 tmpvar_3;
  tmpvar_3 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_3;
  tmpvar_2[0] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(1.5, 1.5)));
  tmpvar_2[1] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(-1.5, 1.5)));
  tmpvar_2[2] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(-1.5, -1.5)));
  tmpvar_2[3] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(1.5, -1.5)));
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_2[0];
  o.xlv_TEXCOORD1_1 = tmpvar_2[1];
  o.xlv_TEXCOORD1_2 = tmpvar_2[2];
  o.xlv_TEXCOORD1_3 = tmpvar_2[3];
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float2 tmpvar_1[4];
  tmpvar_1[0] = i.xlv_TEXCOORD1;
  tmpvar_1[1] = i.xlv_TEXCOORD1_1;
  tmpvar_1[2] = i.xlv_TEXCOORD1_2;
  tmpvar_1[3] = i.xlv_TEXCOORD1_3;
  pc_FragColor = clamp((max(max(max(max(tex2D(_MainTex, i.xlv_TEXCOORD0), tex2D(_MainTex, tmpvar_1[0])), tex2D(_MainTex, tmpvar_1[1])), tex2D(_MainTex, tmpvar_1[2])), tex2D(_MainTex, tmpvar_1[3])) - _Parameter.w), 0.0, 1.0);
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _MainTex_TexelSize;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD0_3 : TEXCOORD0;
	float2 xlv_TEXCOORD0_2 : TEXCOORD1;
	float2 xlv_TEXCOORD0_1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float2 tmpvar_2[4];
  float4 tmpvar_3;
  tmpvar_3 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_3;
  tmpvar_2[0] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(0.0, -1.5)));
  tmpvar_2[1] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(0.0, -0.5)));
  tmpvar_2[2] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(0.0, 0.5)));
  tmpvar_2[3] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(0.0, 1.5)));
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = tmpvar_2[0];
  o.xlv_TEXCOORD0_1 = tmpvar_2[1];
  o.xlv_TEXCOORD0_2 = tmpvar_2[2];
  o.xlv_TEXCOORD0_3 = tmpvar_2[3];
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float2 tmpvar_1[4];
  tmpvar_1[0] = i.xlv_TEXCOORD0;
  tmpvar_1[1] = i.xlv_TEXCOORD0_1;
  tmpvar_1[2] = i.xlv_TEXCOORD0_2;
  tmpvar_1[3] = i.xlv_TEXCOORD0_3;
  pc_FragColor = ((((tex2D(_MainTex, tmpvar_1[0]) + tex2D(_MainTex, tmpvar_1[1])) + tex2D(_MainTex, tmpvar_1[2])) + tex2D(_MainTex, tmpvar_1[3])) * 0.25);
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _MainTex_TexelSize;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD0_3 : TEXCOORD0;
	float2 xlv_TEXCOORD0_2 : TEXCOORD1;
	float2 xlv_TEXCOORD0_1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float2 tmpvar_2[4];
  float4 tmpvar_3;
  tmpvar_3 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_3;
  tmpvar_2[0] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(-1.5, 0.0)));
  tmpvar_2[1] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(-0.5, 0.0)));
  tmpvar_2[2] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(0.5, 0.0)));
  tmpvar_2[3] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(1.5, 0.0)));
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = tmpvar_2[0];
  o.xlv_TEXCOORD0_1 = tmpvar_2[1];
  o.xlv_TEXCOORD0_2 = tmpvar_2[2];
  o.xlv_TEXCOORD0_3 = tmpvar_2[3];
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float2 tmpvar_1[4];
  tmpvar_1[0] = i.xlv_TEXCOORD0;
  tmpvar_1[1] = i.xlv_TEXCOORD0_1;
  tmpvar_1[2] = i.xlv_TEXCOORD0_2;
  tmpvar_1[3] = i.xlv_TEXCOORD0_3;
  pc_FragColor = ((((tex2D(_MainTex, tmpvar_1[0]) + tex2D(_MainTex, tmpvar_1[1])) + tex2D(_MainTex, tmpvar_1[2])) + tex2D(_MainTex, tmpvar_1[3])) * 0.25);
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _Parameter;
sampler2D _MainTex;
float4 _ColorMix;
sampler2D _Bloom;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD0 : TEXCOORD0;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_2;
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xyxy;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float colorDistance;
  float4 color;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0.xy);
  float tmpvar_2;
  tmpvar_2 = dot(abs((tmpvar_1.xyz - _ColorMix.xyz)), float3(0.22, 0.707, 0.071));
  colorDistance = tmpvar_2;
  float4 tmpvar_3;
  tmpvar_3 = lerp(tmpvar_1, _ColorMix, ((float4)((_Parameter.x * colorDistance))));
  color = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4 = (color + tex2D(_Bloom, i.xlv_TEXCOORD0.zw));
  color = tmpvar_4;
  pc_FragColor = tmpvar_4;
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _MainTex_TexelSize;
float4 _Parameter;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1_3 : TEXCOORD0;
	float2 xlv_TEXCOORD1_2 : TEXCOORD1;
	float2 xlv_TEXCOORD1_1 : TEXCOORD2;
	float2 xlv_TEXCOORD1 : TEXCOORD3;
	float2 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float2 tmpvar_2[4];
  float4 tmpvar_3;
  tmpvar_3 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_3;
  tmpvar_2[0] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(1.5, 1.5)));
  tmpvar_2[1] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(-1.5, 1.5)));
  tmpvar_2[2] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(-1.5, -1.5)));
  tmpvar_2[3] = (v._glesMultiTexCoord0.xy + (_MainTex_TexelSize.xy * float2(1.5, -1.5)));
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_2[0];
  o.xlv_TEXCOORD1_1 = tmpvar_2[1];
  o.xlv_TEXCOORD1_2 = tmpvar_2[2];
  o.xlv_TEXCOORD1_3 = tmpvar_2[3];
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float2 tmpvar_1[4];
  tmpvar_1[0] = i.xlv_TEXCOORD1;
  tmpvar_1[1] = i.xlv_TEXCOORD1_1;
  tmpvar_1[2] = i.xlv_TEXCOORD1_2;
  tmpvar_1[3] = i.xlv_TEXCOORD1_3;
  pc_FragColor = clamp(((max(max(max(max(tex2D(_MainTex, i.xlv_TEXCOORD0), tex2D(_MainTex, tmpvar_1[0])), tex2D(_MainTex, tmpvar_1[1])), tex2D(_MainTex, tmpvar_1[2])), tex2D(_MainTex, tmpvar_1[3])) + float4(0.25, 0.0, 0.0, 0.0)) - _Parameter.w), 0.0, 1.0);
	return pc_FragColor;
}
ENDCG
 }
}
Fallback Off
}