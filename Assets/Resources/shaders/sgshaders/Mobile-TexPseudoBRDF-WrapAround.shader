Shader "Mobile/BRDFLit (Supports Backlight)" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
 _BRDFTex ("NdotL NdotH (RGBA)", 2D) = "white" {}
}
SubShader { 
 LOD 400
 Tags { "LIGHTMODE"="ForwardBase" "RenderType"="Opaque" }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardBase" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile_fwdbase
// PC port: translated from the original GLES code by gles_to_hlsl.py
#if defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && defined(VERTEXLIGHT_ON)
float4 _MainTex_ST;
float4 _BumpMap_ST;
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _BRDFTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])
struct appdata_pc {
	float4 _glesTANGENT : TANGENT;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD4 : TEXCOORD0;
	float3 xlv_TEXCOORD3 : TEXCOORD1;
	float3 xlv_TEXCOORD2 : TEXCOORD2;
	float3 xlv_TEXCOORD1 : TEXCOORD3;
	float4 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 shlight;
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_8;
  tmpvar_8._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_8._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_8._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_9;
  tmpvar_9 = (mul(tmpvar_8, (tmpvar_2 * pc_unity_Scale.w)));
  float3x3 tmpvar_10;
  tmpvar_10._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_10._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_10._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_11;
  tmpvar_11._m00_m10_m20.x = tmpvar_10._m00_m10_m20.x;
  tmpvar_11._m00_m10_m20.y = tmpvar_10._m01_m11_m21.x;
  tmpvar_11._m00_m10_m20.z = tmpvar_10._m02_m12_m22.x;
  tmpvar_11._m01_m11_m21.x = tmpvar_10._m00_m10_m20.y;
  tmpvar_11._m01_m11_m21.y = tmpvar_10._m01_m11_m21.y;
  tmpvar_11._m01_m11_m21.z = tmpvar_10._m02_m12_m22.y;
  tmpvar_11._m02_m12_m22.x = tmpvar_10._m00_m10_m20.z;
  tmpvar_11._m02_m12_m22.y = tmpvar_10._m01_m11_m21.z;
  tmpvar_11._m02_m12_m22.z = tmpvar_10._m02_m12_m22.z;
  float3 tmpvar_12;
  tmpvar_12 = (mul(tmpvar_11, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_4 = tmpvar_12;
  float4 tmpvar_13;
  tmpvar_13.w = 1.0;
  tmpvar_13.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_14;
  tmpvar_14 = normalize((mul(tmpvar_11, (((mul(pc_World2Object, tmpvar_13)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_6 = tmpvar_14;
  float4 tmpvar_15;
  tmpvar_15.w = 1.0;
  tmpvar_15.xyz = tmpvar_9;
  float3 tmpvar_16;
  float4 normal;
  normal = tmpvar_15;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHAr, normal);
  x1.x = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot(unity_SHAg, normal);
  x1.y = tmpvar_18;
  float tmpvar_19;
  tmpvar_19 = dot(unity_SHAb, normal);
  x1.z = tmpvar_19;
  float4 tmpvar_20;
  tmpvar_20 = (normal.xyzz * normal.yzzx);
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHBr, tmpvar_20);
  x2.x = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHBg, tmpvar_20);
  x2.y = tmpvar_22;
  float tmpvar_23;
  tmpvar_23 = dot(unity_SHBb, tmpvar_20);
  x2.z = tmpvar_23;
  float tmpvar_24;
  tmpvar_24 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_24;
  float3 tmpvar_25;
  tmpvar_25 = (unity_SHC.xyz * vC);
  x3 = tmpvar_25;
  tmpvar_16 = ((x1 + x2) + x3);
  shlight = tmpvar_16;
  tmpvar_5 = shlight;
  float3 tmpvar_26;
  tmpvar_26 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float4 tmpvar_27;
  tmpvar_27 = (unity_4LightPosX0 - tmpvar_26.x);
  float4 tmpvar_28;
  tmpvar_28 = (unity_4LightPosY0 - tmpvar_26.y);
  float4 tmpvar_29;
  tmpvar_29 = (unity_4LightPosZ0 - tmpvar_26.z);
  float4 tmpvar_30;
  tmpvar_30 = (((tmpvar_27 * tmpvar_27) + (tmpvar_28 * tmpvar_28)) + (tmpvar_29 * tmpvar_29));
  float4 tmpvar_31;
  tmpvar_31 = (max(float4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_27 * tmpvar_9.x) + (tmpvar_28 * tmpvar_9.y)) + (tmpvar_29 * tmpvar_9.z)) * rsqrt(tmpvar_30))) * (1.0/((1.0 + (tmpvar_30 * unity_4LightAtten0)))));
  float3 tmpvar_32;
  tmpvar_32 = (tmpvar_5 + ((((unity_LightColor[0].xyz * tmpvar_31.x) + (unity_LightColor[1].xyz * tmpvar_31.y)) + (unity_LightColor[2].xyz * tmpvar_31.z)) + (unity_LightColor[3].xyz * tmpvar_31.w)));
  tmpvar_5 = tmpvar_32;
  float4 o_i0;
  float4 tmpvar_33;
  tmpvar_33 = (tmpvar_7 * 0.5);
  o_i0 = tmpvar_33;
  float2 tmpvar_34;
  tmpvar_34.x = tmpvar_33.x;
  tmpvar_34.y = (tmpvar_33.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_34 + tmpvar_33.w);
  o_i0.zw = tmpvar_7.zw;
  o.pos = tmpvar_7;
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
  o.xlv_TEXCOORD4 = o_i0;
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
  tmpvar_3 = tex2D(_MainTex, tmpvar_1);
  float3 tmpvar_4;
  tmpvar_4 = ((tex2D(_BumpMap, tmpvar_2).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_5;
  tmpvar_5.x = ((dot(tmpvar_4, i.xlv_TEXCOORD1) * 0.5) + 0.5);
  tmpvar_5.y = dot(tmpvar_4, normalize((i.xlv_TEXCOORD1 + i.xlv_TEXCOORD3)));
  float4 tmpvar_6;
  tmpvar_6 = tex2D(_BRDFTex, tmpvar_5);
  c_i0.xyz = ((tmpvar_3.xyz * (tmpvar_6.xyz + (tmpvar_3.w * tmpvar_6.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL) && !defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && defined(VERTEXLIGHT_ON)
float4 _MainTex_ST;
float4 _BumpMap_ST;
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _BRDFTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])
struct appdata_pc {
	float4 _glesTANGENT : TANGENT;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float4 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 shlight;
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_7;
  tmpvar_7._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_7._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_7._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_8;
  tmpvar_8 = (mul(tmpvar_7, (tmpvar_2 * pc_unity_Scale.w)));
  float3x3 tmpvar_9;
  tmpvar_9._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_9._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_9._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_10;
  tmpvar_10._m00_m10_m20.x = tmpvar_9._m00_m10_m20.x;
  tmpvar_10._m00_m10_m20.y = tmpvar_9._m01_m11_m21.x;
  tmpvar_10._m00_m10_m20.z = tmpvar_9._m02_m12_m22.x;
  tmpvar_10._m01_m11_m21.x = tmpvar_9._m00_m10_m20.y;
  tmpvar_10._m01_m11_m21.y = tmpvar_9._m01_m11_m21.y;
  tmpvar_10._m01_m11_m21.z = tmpvar_9._m02_m12_m22.y;
  tmpvar_10._m02_m12_m22.x = tmpvar_9._m00_m10_m20.z;
  tmpvar_10._m02_m12_m22.y = tmpvar_9._m01_m11_m21.z;
  tmpvar_10._m02_m12_m22.z = tmpvar_9._m02_m12_m22.z;
  float3 tmpvar_11;
  tmpvar_11 = (mul(tmpvar_10, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_4 = tmpvar_11;
  float4 tmpvar_12;
  tmpvar_12.w = 1.0;
  tmpvar_12.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_13;
  tmpvar_13 = normalize((mul(tmpvar_10, (((mul(pc_World2Object, tmpvar_12)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_6 = tmpvar_13;
  float4 tmpvar_14;
  tmpvar_14.w = 1.0;
  tmpvar_14.xyz = tmpvar_8;
  float3 tmpvar_15;
  float4 normal;
  normal = tmpvar_14;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHAr, normal);
  x1.x = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHAg, normal);
  x1.y = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot(unity_SHAb, normal);
  x1.z = tmpvar_18;
  float4 tmpvar_19;
  tmpvar_19 = (normal.xyzz * normal.yzzx);
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHBr, tmpvar_19);
  x2.x = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHBg, tmpvar_19);
  x2.y = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHBb, tmpvar_19);
  x2.z = tmpvar_22;
  float tmpvar_23;
  tmpvar_23 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_23;
  float3 tmpvar_24;
  tmpvar_24 = (unity_SHC.xyz * vC);
  x3 = tmpvar_24;
  tmpvar_15 = ((x1 + x2) + x3);
  shlight = tmpvar_15;
  tmpvar_5 = shlight;
  float3 tmpvar_25;
  tmpvar_25 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float4 tmpvar_26;
  tmpvar_26 = (unity_4LightPosX0 - tmpvar_25.x);
  float4 tmpvar_27;
  tmpvar_27 = (unity_4LightPosY0 - tmpvar_25.y);
  float4 tmpvar_28;
  tmpvar_28 = (unity_4LightPosZ0 - tmpvar_25.z);
  float4 tmpvar_29;
  tmpvar_29 = (((tmpvar_26 * tmpvar_26) + (tmpvar_27 * tmpvar_27)) + (tmpvar_28 * tmpvar_28));
  float4 tmpvar_30;
  tmpvar_30 = (max(float4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_26 * tmpvar_8.x) + (tmpvar_27 * tmpvar_8.y)) + (tmpvar_28 * tmpvar_8.z)) * rsqrt(tmpvar_29))) * (1.0/((1.0 + (tmpvar_29 * unity_4LightAtten0)))));
  float3 tmpvar_31;
  tmpvar_31 = (tmpvar_5 + ((((unity_LightColor[0].xyz * tmpvar_30.x) + (unity_LightColor[1].xyz * tmpvar_30.y)) + (unity_LightColor[2].xyz * tmpvar_30.z)) + (unity_LightColor[3].xyz * tmpvar_30.w)));
  tmpvar_5 = tmpvar_31;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
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
  tmpvar_3 = tex2D(_MainTex, tmpvar_1);
  float3 tmpvar_4;
  tmpvar_4 = ((tex2D(_BumpMap, tmpvar_2).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_5;
  tmpvar_5.x = ((dot(tmpvar_4, i.xlv_TEXCOORD1) * 0.5) + 0.5);
  tmpvar_5.y = dot(tmpvar_4, normalize((i.xlv_TEXCOORD1 + i.xlv_TEXCOORD3)));
  float4 tmpvar_6;
  tmpvar_6 = tex2D(_BRDFTex, tmpvar_5);
  c_i0.xyz = ((tmpvar_3.xyz * (tmpvar_6.xyz + (tmpvar_3.w * tmpvar_6.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  pc_FragColor = c;
	return pc_FragColor;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON)
float4 _MainTex_ST;
float4 _BumpMap_ST;
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _BRDFTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])
struct appdata_pc {
	float4 _glesTANGENT : TANGENT;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD4 : TEXCOORD0;
	float3 xlv_TEXCOORD3 : TEXCOORD1;
	float3 xlv_TEXCOORD2 : TEXCOORD2;
	float3 xlv_TEXCOORD1 : TEXCOORD3;
	float4 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 shlight;
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_8;
  tmpvar_8._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_8._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_8._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3x3 tmpvar_9;
  tmpvar_9._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_9._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_9._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_10;
  tmpvar_10._m00_m10_m20.x = tmpvar_9._m00_m10_m20.x;
  tmpvar_10._m00_m10_m20.y = tmpvar_9._m01_m11_m21.x;
  tmpvar_10._m00_m10_m20.z = tmpvar_9._m02_m12_m22.x;
  tmpvar_10._m01_m11_m21.x = tmpvar_9._m00_m10_m20.y;
  tmpvar_10._m01_m11_m21.y = tmpvar_9._m01_m11_m21.y;
  tmpvar_10._m01_m11_m21.z = tmpvar_9._m02_m12_m22.y;
  tmpvar_10._m02_m12_m22.x = tmpvar_9._m00_m10_m20.z;
  tmpvar_10._m02_m12_m22.y = tmpvar_9._m01_m11_m21.z;
  tmpvar_10._m02_m12_m22.z = tmpvar_9._m02_m12_m22.z;
  float3 tmpvar_11;
  tmpvar_11 = (mul(tmpvar_10, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_4 = tmpvar_11;
  float4 tmpvar_12;
  tmpvar_12.w = 1.0;
  tmpvar_12.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_13;
  tmpvar_13 = normalize((mul(tmpvar_10, (((mul(pc_World2Object, tmpvar_12)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_6 = tmpvar_13;
  float4 tmpvar_14;
  tmpvar_14.w = 1.0;
  tmpvar_14.xyz = (mul(tmpvar_8, (tmpvar_2 * pc_unity_Scale.w)));
  float3 tmpvar_15;
  float4 normal;
  normal = tmpvar_14;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHAr, normal);
  x1.x = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHAg, normal);
  x1.y = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot(unity_SHAb, normal);
  x1.z = tmpvar_18;
  float4 tmpvar_19;
  tmpvar_19 = (normal.xyzz * normal.yzzx);
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHBr, tmpvar_19);
  x2.x = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHBg, tmpvar_19);
  x2.y = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHBb, tmpvar_19);
  x2.z = tmpvar_22;
  float tmpvar_23;
  tmpvar_23 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_23;
  float3 tmpvar_24;
  tmpvar_24 = (unity_SHC.xyz * vC);
  x3 = tmpvar_24;
  tmpvar_15 = ((x1 + x2) + x3);
  shlight = tmpvar_15;
  tmpvar_5 = shlight;
  float4 o_i0;
  float4 tmpvar_25;
  tmpvar_25 = (tmpvar_7 * 0.5);
  o_i0 = tmpvar_25;
  float2 tmpvar_26;
  tmpvar_26.x = tmpvar_25.x;
  tmpvar_26.y = (tmpvar_25.y * _ProjectionParams.x);
  o_i0.xy = (tmpvar_26 + tmpvar_25.w);
  o_i0.zw = tmpvar_7.zw;
  o.pos = tmpvar_7;
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
  o.xlv_TEXCOORD4 = o_i0;
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
  tmpvar_3 = tex2D(_MainTex, tmpvar_1);
  float3 tmpvar_4;
  tmpvar_4 = ((tex2D(_BumpMap, tmpvar_2).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_5;
  tmpvar_5.x = ((dot(tmpvar_4, i.xlv_TEXCOORD1) * 0.5) + 0.5);
  tmpvar_5.y = dot(tmpvar_4, normalize((i.xlv_TEXCOORD1 + i.xlv_TEXCOORD3)));
  float4 tmpvar_6;
  tmpvar_6 = tex2D(_BRDFTex, tmpvar_5);
  c_i0.xyz = ((tmpvar_3.xyz * (tmpvar_6.xyz + (tmpvar_3.w * tmpvar_6.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  pc_FragColor = c;
	return pc_FragColor;
}
#else // DIRECTIONAL SHADOWS_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
float4 _MainTex_ST;
float4 _BumpMap_ST;
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _BRDFTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])
struct appdata_pc {
	float4 _glesTANGENT : TANGENT;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float3 xlv_TEXCOORD1 : TEXCOORD2;
	float4 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 shlight;
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_7;
  tmpvar_7._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_7._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_7._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3x3 tmpvar_8;
  tmpvar_8._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_8._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_8._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_9;
  tmpvar_9._m00_m10_m20.x = tmpvar_8._m00_m10_m20.x;
  tmpvar_9._m00_m10_m20.y = tmpvar_8._m01_m11_m21.x;
  tmpvar_9._m00_m10_m20.z = tmpvar_8._m02_m12_m22.x;
  tmpvar_9._m01_m11_m21.x = tmpvar_8._m00_m10_m20.y;
  tmpvar_9._m01_m11_m21.y = tmpvar_8._m01_m11_m21.y;
  tmpvar_9._m01_m11_m21.z = tmpvar_8._m02_m12_m22.y;
  tmpvar_9._m02_m12_m22.x = tmpvar_8._m00_m10_m20.z;
  tmpvar_9._m02_m12_m22.y = tmpvar_8._m01_m11_m21.z;
  tmpvar_9._m02_m12_m22.z = tmpvar_8._m02_m12_m22.z;
  float3 tmpvar_10;
  tmpvar_10 = (mul(tmpvar_9, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_4 = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11.w = 1.0;
  tmpvar_11.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_12;
  tmpvar_12 = normalize((mul(tmpvar_9, (((mul(pc_World2Object, tmpvar_11)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_6 = tmpvar_12;
  float4 tmpvar_13;
  tmpvar_13.w = 1.0;
  tmpvar_13.xyz = (mul(tmpvar_7, (tmpvar_2 * pc_unity_Scale.w)));
  float3 tmpvar_14;
  float4 normal;
  normal = tmpvar_13;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_15;
  tmpvar_15 = dot(unity_SHAr, normal);
  x1.x = tmpvar_15;
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHAg, normal);
  x1.y = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHAb, normal);
  x1.z = tmpvar_17;
  float4 tmpvar_18;
  tmpvar_18 = (normal.xyzz * normal.yzzx);
  float tmpvar_19;
  tmpvar_19 = dot(unity_SHBr, tmpvar_18);
  x2.x = tmpvar_19;
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHBg, tmpvar_18);
  x2.y = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHBb, tmpvar_18);
  x2.z = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_22;
  float3 tmpvar_23;
  tmpvar_23 = (unity_SHC.xyz * vC);
  x3 = tmpvar_23;
  tmpvar_14 = ((x1 + x2) + x3);
  shlight = tmpvar_14;
  tmpvar_5 = shlight;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
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
  tmpvar_3 = tex2D(_MainTex, tmpvar_1);
  float3 tmpvar_4;
  tmpvar_4 = ((tex2D(_BumpMap, tmpvar_2).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_5;
  tmpvar_5.x = ((dot(tmpvar_4, i.xlv_TEXCOORD1) * 0.5) + 0.5);
  tmpvar_5.y = dot(tmpvar_4, normalize((i.xlv_TEXCOORD1 + i.xlv_TEXCOORD3)));
  float4 tmpvar_6;
  tmpvar_6 = tex2D(_BRDFTex, tmpvar_5);
  c_i0.xyz = ((tmpvar_3.xyz * (tmpvar_6.xyz + (tmpvar_3.w * tmpvar_6.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_3.xyz * i.xlv_TEXCOORD2));
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "Mobile/Diffuse"
}