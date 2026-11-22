Shader "CKZ2/Self-Illuminated (Advanced)" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}
SubShader { 
 LOD 400
 Tags { "RenderType"="Opaque" }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardBase" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile_fwdbase
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
#if defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && defined(VERTEXLIGHT_ON)
float4 _MainTex_ST;
float4 _Illum_ST;
sampler2D _ShadowMapTexture;
sampler2D _MainTex;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float4 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 shlight;
  float4 tmpvar_1;
  float3 tmpvar_2;
  float3 tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1.zw = ((v._glesMultiTexCoord0.xy * _Illum_ST.xy) + _Illum_ST.zw);
  float3x3 tmpvar_5;
  tmpvar_5._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_5._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_5._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_6;
  tmpvar_6 = (mul(tmpvar_5, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_2 = tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7.w = 1.0;
  tmpvar_7.xyz = tmpvar_6;
  float3 tmpvar_8;
  float4 normal;
  normal = tmpvar_7;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHAr, normal);
  x1.x = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHAg, normal);
  x1.y = tmpvar_10;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHAb, normal);
  x1.z = tmpvar_11;
  float4 tmpvar_12;
  tmpvar_12 = (normal.xyzz * normal.yzzx);
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHBr, tmpvar_12);
  x2.x = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot(unity_SHBg, tmpvar_12);
  x2.y = tmpvar_14;
  float tmpvar_15;
  tmpvar_15 = dot(unity_SHBb, tmpvar_12);
  x2.z = tmpvar_15;
  float tmpvar_16;
  tmpvar_16 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_16;
  float3 tmpvar_17;
  tmpvar_17 = (unity_SHC.xyz * vC);
  x3 = tmpvar_17;
  tmpvar_8 = ((x1 + x2) + x3);
  shlight = tmpvar_8;
  tmpvar_3 = shlight;
  float3 tmpvar_18;
  tmpvar_18 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float4 tmpvar_19;
  tmpvar_19 = (unity_4LightPosX0 - tmpvar_18.x);
  float4 tmpvar_20;
  tmpvar_20 = (unity_4LightPosY0 - tmpvar_18.y);
  float4 tmpvar_21;
  tmpvar_21 = (unity_4LightPosZ0 - tmpvar_18.z);
  float4 tmpvar_22;
  tmpvar_22 = (((tmpvar_19 * tmpvar_19) + (tmpvar_20 * tmpvar_20)) + (tmpvar_21 * tmpvar_21));
  float4 tmpvar_23;
  tmpvar_23 = (max(float4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_19 * tmpvar_6.x) + (tmpvar_20 * tmpvar_6.y)) + (tmpvar_21 * tmpvar_6.z)) * rsqrt(tmpvar_22))) * (1.0/((1.0 + (tmpvar_22 * unity_4LightAtten0)))));
  float3 tmpvar_24;
  tmpvar_24 = (tmpvar_3 + ((((unity_LightColor[0].xyz * tmpvar_23.x) + (unity_LightColor[1].xyz * tmpvar_23.y)) + (unity_LightColor[2].xyz * tmpvar_23.z)) + (unity_LightColor[3].xyz * tmpvar_23.w)));
  tmpvar_3 = tmpvar_24;
  float4 o_i0;
  float4 tmpvar_25;
  tmpvar_25 = (tmpvar_4 * 0.5);
  o_i0 = tmpvar_25;
  float2 tmpvar_26;
  tmpvar_26.x = tmpvar_25.x;
  tmpvar_26.y = (tmpvar_25.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_26 + tmpvar_25.w);
  o_i0.zw = tmpvar_4.zw;
  o.pos = tmpvar_4;
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
  o.xlv_TEXCOORD3 = o_i0;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float2 tmpvar_1;
  tmpvar_1 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.zw;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, tmpvar_1) * _Color);
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_3.xyz * _LightColor0.xyz) * ((max(0.0, dot(i.xlv_TEXCOORD1, _WorldSpaceLightPos0.xyz)) * tex2Dproj(_ShadowMapTexture, i.xlv_TEXCOORD3).x) * 2.0));
  c_i0_i1.w = tmpvar_3.w;
  c = c_i0_i1;
  c.xyz = (c_i0_i1.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  c.xyz = (c.xyz + (tmpvar_3.xyz * tex2D(_MainTex, tmpvar_2).w));
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
float4 _MainTex_ST;
float4 _Illum_ST;
sampler2D _ShadowMapTexture;
sampler2D _MainTex;
float4 _Color;
struct appdata_pc {
	float4 _glesMultiTexCoord1 : TEXCOORD1;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1.zw = ((v._glesMultiTexCoord0.xy * _Illum_ST.xy) + _Illum_ST.zw);
  float4 o_i0;
  float4 tmpvar_3;
  tmpvar_3 = (tmpvar_2 * 0.5);
  o_i0 = tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = tmpvar_3.x;
  tmpvar_4.y = (tmpvar_3.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_4 + tmpvar_3.w);
  o_i0.zw = tmpvar_2.zw;
  o.pos = tmpvar_2;
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = ((v._glesMultiTexCoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
  o.xlv_TEXCOORD2 = o_i0;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float2 tmpvar_1;
  tmpvar_1 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.zw;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, tmpvar_1) * _Color);
  c = float4(0.0, 0.0, 0.0, 0.0);
  c.xyz = (tmpvar_3.xyz * min((2.0 * UNITY_SAMPLE_TEX2D(unity_Lightmap, i.xlv_TEXCOORD1).xyz), ((float3)((tex2Dproj(_ShadowMapTexture, i.xlv_TEXCOORD2).x * 2.0)))));
  c.w = tmpvar_3.w;
  c.xyz = (c.xyz + (tmpvar_3.xyz * tex2D(_MainTex, tmpvar_2).w));
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL) && !defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && defined(VERTEXLIGHT_ON)
float4 _MainTex_ST;
float4 _Illum_ST;
sampler2D _MainTex;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 shlight;
  float4 tmpvar_1;
  float3 tmpvar_2;
  float3 tmpvar_3;
  tmpvar_1.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1.zw = ((v._glesMultiTexCoord0.xy * _Illum_ST.xy) + _Illum_ST.zw);
  float3x3 tmpvar_4;
  tmpvar_4._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_4._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_4._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_5;
  tmpvar_5 = (mul(tmpvar_4, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_2 = tmpvar_5;
  float4 tmpvar_6;
  tmpvar_6.w = 1.0;
  tmpvar_6.xyz = tmpvar_5;
  float3 tmpvar_7;
  float4 normal;
  normal = tmpvar_6;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_8;
  tmpvar_8 = dot(unity_SHAr, normal);
  x1.x = tmpvar_8;
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHAg, normal);
  x1.y = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHAb, normal);
  x1.z = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11 = (normal.xyzz * normal.yzzx);
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHBr, tmpvar_11);
  x2.x = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHBg, tmpvar_11);
  x2.y = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot(unity_SHBb, tmpvar_11);
  x2.z = tmpvar_14;
  float tmpvar_15;
  tmpvar_15 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_15;
  float3 tmpvar_16;
  tmpvar_16 = (unity_SHC.xyz * vC);
  x3 = tmpvar_16;
  tmpvar_7 = ((x1 + x2) + x3);
  shlight = tmpvar_7;
  tmpvar_3 = shlight;
  float3 tmpvar_17;
  tmpvar_17 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float4 tmpvar_18;
  tmpvar_18 = (unity_4LightPosX0 - tmpvar_17.x);
  float4 tmpvar_19;
  tmpvar_19 = (unity_4LightPosY0 - tmpvar_17.y);
  float4 tmpvar_20;
  tmpvar_20 = (unity_4LightPosZ0 - tmpvar_17.z);
  float4 tmpvar_21;
  tmpvar_21 = (((tmpvar_18 * tmpvar_18) + (tmpvar_19 * tmpvar_19)) + (tmpvar_20 * tmpvar_20));
  float4 tmpvar_22;
  tmpvar_22 = (max(float4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_18 * tmpvar_5.x) + (tmpvar_19 * tmpvar_5.y)) + (tmpvar_20 * tmpvar_5.z)) * rsqrt(tmpvar_21))) * (1.0/((1.0 + (tmpvar_21 * unity_4LightAtten0)))));
  float3 tmpvar_23;
  tmpvar_23 = (tmpvar_3 + ((((unity_LightColor[0].xyz * tmpvar_22.x) + (unity_LightColor[1].xyz * tmpvar_22.y)) + (unity_LightColor[2].xyz * tmpvar_22.z)) + (unity_LightColor[3].xyz * tmpvar_22.w)));
  tmpvar_3 = tmpvar_23;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float2 tmpvar_1;
  tmpvar_1 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.zw;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, tmpvar_1) * _Color);
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_3.xyz * _LightColor0.xyz) * (max(0.0, dot(i.xlv_TEXCOORD1, _WorldSpaceLightPos0.xyz)) * 2.0));
  c_i0_i1.w = tmpvar_3.w;
  c = c_i0_i1;
  c.xyz = (c_i0_i1.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  c.xyz = (c.xyz + (tmpvar_3.xyz * tex2D(_MainTex, tmpvar_2).w));
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL) && !defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
float4 _MainTex_ST;
float4 _Illum_ST;
sampler2D _MainTex;
float4 _Color;
struct appdata_pc {
	float4 _glesMultiTexCoord1 : TEXCOORD1;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float4 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1.zw = ((v._glesMultiTexCoord0.xy * _Illum_ST.xy) + _Illum_ST.zw);
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = ((v._glesMultiTexCoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float2 tmpvar_1;
  tmpvar_1 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.zw;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, tmpvar_1) * _Color);
  c = float4(0.0, 0.0, 0.0, 0.0);
  c.xyz = (tmpvar_3.xyz * (2.0 * UNITY_SAMPLE_TEX2D(unity_Lightmap, i.xlv_TEXCOORD1).xyz));
  c.w = tmpvar_3.w;
  c.xyz = (c.xyz + (tmpvar_3.xyz * tex2D(_MainTex, tmpvar_2).w));
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON)
float4 _MainTex_ST;
float4 _Illum_ST;
sampler2D _ShadowMapTexture;
sampler2D _MainTex;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float4 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 shlight;
  float4 tmpvar_1;
  float3 tmpvar_2;
  float3 tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1.zw = ((v._glesMultiTexCoord0.xy * _Illum_ST.xy) + _Illum_ST.zw);
  float3x3 tmpvar_5;
  tmpvar_5._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_5._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_5._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_6;
  tmpvar_6 = (mul(tmpvar_5, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_2 = tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7.w = 1.0;
  tmpvar_7.xyz = tmpvar_6;
  float3 tmpvar_8;
  float4 normal;
  normal = tmpvar_7;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHAr, normal);
  x1.x = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHAg, normal);
  x1.y = tmpvar_10;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHAb, normal);
  x1.z = tmpvar_11;
  float4 tmpvar_12;
  tmpvar_12 = (normal.xyzz * normal.yzzx);
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHBr, tmpvar_12);
  x2.x = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot(unity_SHBg, tmpvar_12);
  x2.y = tmpvar_14;
  float tmpvar_15;
  tmpvar_15 = dot(unity_SHBb, tmpvar_12);
  x2.z = tmpvar_15;
  float tmpvar_16;
  tmpvar_16 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_16;
  float3 tmpvar_17;
  tmpvar_17 = (unity_SHC.xyz * vC);
  x3 = tmpvar_17;
  tmpvar_8 = ((x1 + x2) + x3);
  shlight = tmpvar_8;
  tmpvar_3 = shlight;
  float4 o_i0;
  float4 tmpvar_18;
  tmpvar_18 = (tmpvar_4 * 0.5);
  o_i0 = tmpvar_18;
  float2 tmpvar_19;
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = (tmpvar_18.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_19 + tmpvar_18.w);
  o_i0.zw = tmpvar_4.zw;
  o.pos = tmpvar_4;
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
  o.xlv_TEXCOORD3 = o_i0;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float2 tmpvar_1;
  tmpvar_1 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.zw;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, tmpvar_1) * _Color);
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_3.xyz * _LightColor0.xyz) * ((max(0.0, dot(i.xlv_TEXCOORD1, _WorldSpaceLightPos0.xyz)) * tex2Dproj(_ShadowMapTexture, i.xlv_TEXCOORD3).x) * 2.0));
  c_i0_i1.w = tmpvar_3.w;
  c = c_i0_i1;
  c.xyz = (c_i0_i1.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  c.xyz = (c.xyz + (tmpvar_3.xyz * tex2D(_MainTex, tmpvar_2).w));
  pc_FragColor = c;
	return pc_FragColor;
}
#else // DIRECTIONAL SHADOWS_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
float4 _MainTex_ST;
float4 _Illum_ST;
sampler2D _MainTex;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 shlight;
  float4 tmpvar_1;
  float3 tmpvar_2;
  float3 tmpvar_3;
  tmpvar_1.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1.zw = ((v._glesMultiTexCoord0.xy * _Illum_ST.xy) + _Illum_ST.zw);
  float3x3 tmpvar_4;
  tmpvar_4._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_4._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_4._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_5;
  tmpvar_5 = (mul(tmpvar_4, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_2 = tmpvar_5;
  float4 tmpvar_6;
  tmpvar_6.w = 1.0;
  tmpvar_6.xyz = tmpvar_5;
  float3 tmpvar_7;
  float4 normal;
  normal = tmpvar_6;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_8;
  tmpvar_8 = dot(unity_SHAr, normal);
  x1.x = tmpvar_8;
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHAg, normal);
  x1.y = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHAb, normal);
  x1.z = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11 = (normal.xyzz * normal.yzzx);
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHBr, tmpvar_11);
  x2.x = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHBg, tmpvar_11);
  x2.y = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot(unity_SHBb, tmpvar_11);
  x2.z = tmpvar_14;
  float tmpvar_15;
  tmpvar_15 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_15;
  float3 tmpvar_16;
  tmpvar_16 = (unity_SHC.xyz * vC);
  x3 = tmpvar_16;
  tmpvar_7 = ((x1 + x2) + x3);
  shlight = tmpvar_7;
  tmpvar_3 = shlight;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float2 tmpvar_1;
  tmpvar_1 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.zw;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, tmpvar_1) * _Color);
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_3.xyz * _LightColor0.xyz) * (max(0.0, dot(i.xlv_TEXCOORD1, _WorldSpaceLightPos0.xyz)) * 2.0));
  c_i0_i1.w = tmpvar_3.w;
  c = c_i0_i1;
  c.xyz = (c_i0_i1.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  c.xyz = (c.xyz + (tmpvar_3.xyz * tex2D(_MainTex, tmpvar_2).w));
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardAdd" "RenderType"="Opaque" }
  ZWrite Off
  Fog {
   Color (0,0,0,0)
  }
  Blend One One
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile_fwdadd
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
#if defined(POINT)
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _LightTexture0;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
float4x4 unity_WorldToLight;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = (mul(tmpvar_3, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_1 = tmpvar_4;
  float3 tmpvar_5;
  tmpvar_5 = (_WorldSpaceLightPos0.xyz - (mul(unity_ObjectToWorld, v._glesVertex)).xyz);
  tmpvar_2 = tmpvar_5;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = tmpvar_2;
  o.xlv_TEXCOORD3 = (mul(unity_WorldToLight, (mul(unity_ObjectToWorld, v._glesVertex)))).xyz;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 lightDir;
  float4 tmpvar_1;
  tmpvar_1 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  float3 tmpvar_2;
  tmpvar_2 = normalize(i.xlv_TEXCOORD2);
  lightDir = tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = ((float2)(dot(i.xlv_TEXCOORD3, i.xlv_TEXCOORD3)));
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_1.xyz * _LightColor0.xyz) * ((max(0.0, dot(i.xlv_TEXCOORD1, lightDir)) * tex2D(_LightTexture0, tmpvar_3).w) * 2.0));
  c_i0_i1.w = tmpvar_1.w;
  c = c_i0_i1;
  c.w = 0.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL)
float4 _MainTex_ST;
sampler2D _MainTex;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = (mul(tmpvar_3, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_1 = tmpvar_4;
  float3 tmpvar_5;
  tmpvar_5 = _WorldSpaceLightPos0.xyz;
  tmpvar_2 = tmpvar_5;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 lightDir;
  float4 tmpvar_1;
  tmpvar_1 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  lightDir = i.xlv_TEXCOORD2;
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_1.xyz * _LightColor0.xyz) * (max(0.0, dot(i.xlv_TEXCOORD1, lightDir)) * 2.0));
  c_i0_i1.w = tmpvar_1.w;
  c = c_i0_i1;
  c.w = 0.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(SPOT)
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _LightTextureB0;
sampler2D _LightTexture0;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
float4x4 unity_WorldToLight;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = (mul(tmpvar_3, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_1 = tmpvar_4;
  float3 tmpvar_5;
  tmpvar_5 = (_WorldSpaceLightPos0.xyz - (mul(unity_ObjectToWorld, v._glesVertex)).xyz);
  tmpvar_2 = tmpvar_5;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = tmpvar_2;
  o.xlv_TEXCOORD3 = (mul(unity_WorldToLight, (mul(unity_ObjectToWorld, v._glesVertex))));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 lightDir;
  float4 tmpvar_1;
  tmpvar_1 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  float3 tmpvar_2;
  tmpvar_2 = normalize(i.xlv_TEXCOORD2);
  lightDir = tmpvar_2;
  float3 LightCoord_i0;
  LightCoord_i0 = i.xlv_TEXCOORD3.xyz;
  float2 tmpvar_3;
  tmpvar_3 = ((float2)(dot(LightCoord_i0, LightCoord_i0)));
  float atten;
  atten = ((float((i.xlv_TEXCOORD3.z > 0.0)) * tex2D(_LightTexture0, ((i.xlv_TEXCOORD3.xy / i.xlv_TEXCOORD3.w) + 0.5)).w) * tex2D(_LightTextureB0, tmpvar_3).w);
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_1.xyz * _LightColor0.xyz) * ((max(0.0, dot(i.xlv_TEXCOORD1, lightDir)) * atten) * 2.0));
  c_i0_i1.w = tmpvar_1.w;
  c = c_i0_i1;
  c.w = 0.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(POINT_COOKIE)
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _LightTextureB0;
samplerCUBE _LightTexture0;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
float4x4 unity_WorldToLight;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = (mul(tmpvar_3, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_1 = tmpvar_4;
  float3 tmpvar_5;
  tmpvar_5 = (_WorldSpaceLightPos0.xyz - (mul(unity_ObjectToWorld, v._glesVertex)).xyz);
  tmpvar_2 = tmpvar_5;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = tmpvar_2;
  o.xlv_TEXCOORD3 = (mul(unity_WorldToLight, (mul(unity_ObjectToWorld, v._glesVertex)))).xyz;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 lightDir;
  float4 tmpvar_1;
  tmpvar_1 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  float3 tmpvar_2;
  tmpvar_2 = normalize(i.xlv_TEXCOORD2);
  lightDir = tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = ((float2)(dot(i.xlv_TEXCOORD3, i.xlv_TEXCOORD3)));
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_1.xyz * _LightColor0.xyz) * ((max(0.0, dot(i.xlv_TEXCOORD1, lightDir)) * (tex2D(_LightTextureB0, tmpvar_3).w * texCUBE(_LightTexture0, i.xlv_TEXCOORD3).w)) * 2.0));
  c_i0_i1.w = tmpvar_1.w;
  c = c_i0_i1;
  c.w = 0.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#else // DIRECTIONAL_COOKIE
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _LightTexture0;
float4 _LightColor0;
float4 _Color;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
float4x4 unity_WorldToLight;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = (mul(tmpvar_3, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  tmpvar_1 = tmpvar_4;
  float3 tmpvar_5;
  tmpvar_5 = _WorldSpaceLightPos0.xyz;
  tmpvar_2 = tmpvar_5;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = tmpvar_2;
  o.xlv_TEXCOORD3 = (mul(unity_WorldToLight, (mul(unity_ObjectToWorld, v._glesVertex)))).xy;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 lightDir;
  float4 tmpvar_1;
  tmpvar_1 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  lightDir = i.xlv_TEXCOORD2;
  float4 c_i0_i1;
  c_i0_i1.xyz = ((tmpvar_1.xyz * _LightColor0.xyz) * ((max(0.0, dot(i.xlv_TEXCOORD1, lightDir)) * tex2D(_LightTexture0, i.xlv_TEXCOORD3).w) * 2.0));
  c_i0_i1.w = tmpvar_1.w;
  c = c_i0_i1;
  c.w = 0.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "Unlit/Texture"
}