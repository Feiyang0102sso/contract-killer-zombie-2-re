Shader "Mobile/BRDFLit (Supports Backlight, Shadowgun style Light Probes)" {
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
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
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
	float4 xlv_TEXCOORD5 : TEXCOORD0;
	float3 xlv_TEXCOORD4 : TEXCOORD1;
	float3 xlv_TEXCOORD3 : TEXCOORD2;
	float3 xlv_TEXCOORD2 : TEXCOORD3;
	float3 xlv_TEXCOORD1 : TEXCOORD4;
	float4 xlv_TEXCOORD0 : TEXCOORD5;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float3 tmpvar_8;
  float3x3 tmpvar_9;
  tmpvar_9._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_9._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_9._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_10;
  tmpvar_10.w = 1.0;
  tmpvar_10.xyz = (mul(tmpvar_9, (tmpvar_2 * pc_unity_Scale.w)));
  float3 tmpvar_11;
  float4 normal;
  normal = tmpvar_10;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHAr, normal);
  x1.x = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHAg, normal);
  x1.y = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot(unity_SHAb, normal);
  x1.z = tmpvar_14;
  float4 tmpvar_15;
  tmpvar_15 = (normal.xyzz * normal.yzzx);
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHBr, tmpvar_15);
  x2.x = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHBg, tmpvar_15);
  x2.y = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot(unity_SHBb, tmpvar_15);
  x2.z = tmpvar_18;
  float tmpvar_19;
  tmpvar_19 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_19;
  float3 tmpvar_20;
  tmpvar_20 = (unity_SHC.xyz * vC);
  x3 = tmpvar_20;
  tmpvar_11 = ((x1 + x2) + x3);
  tmpvar_8 = tmpvar_11;
  tmpvar_4 = tmpvar_8;
  float4 tmpvar_21;
  tmpvar_21 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_22;
  tmpvar_22._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_22._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_22._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_23;
  tmpvar_23 = (mul(tmpvar_22, (tmpvar_2 * pc_unity_Scale.w)));
  float3x3 tmpvar_24;
  tmpvar_24._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_24._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_24._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_25;
  tmpvar_25._m00_m10_m20.x = tmpvar_24._m00_m10_m20.x;
  tmpvar_25._m00_m10_m20.y = tmpvar_24._m01_m11_m21.x;
  tmpvar_25._m00_m10_m20.z = tmpvar_24._m02_m12_m22.x;
  tmpvar_25._m01_m11_m21.x = tmpvar_24._m00_m10_m20.y;
  tmpvar_25._m01_m11_m21.y = tmpvar_24._m01_m11_m21.y;
  tmpvar_25._m01_m11_m21.z = tmpvar_24._m02_m12_m22.y;
  tmpvar_25._m02_m12_m22.x = tmpvar_24._m00_m10_m20.z;
  tmpvar_25._m02_m12_m22.y = tmpvar_24._m01_m11_m21.z;
  tmpvar_25._m02_m12_m22.z = tmpvar_24._m02_m12_m22.z;
  float3 tmpvar_26;
  tmpvar_26 = (mul(tmpvar_25, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_5 = tmpvar_26;
  float4 tmpvar_27;
  tmpvar_27.w = 1.0;
  tmpvar_27.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_28;
  tmpvar_28 = normalize((mul(tmpvar_25, (((mul(pc_World2Object, tmpvar_27)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_7 = tmpvar_28;
  float3 tmpvar_29;
  tmpvar_29 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float4 tmpvar_30;
  tmpvar_30 = (unity_4LightPosX0 - tmpvar_29.x);
  float4 tmpvar_31;
  tmpvar_31 = (unity_4LightPosY0 - tmpvar_29.y);
  float4 tmpvar_32;
  tmpvar_32 = (unity_4LightPosZ0 - tmpvar_29.z);
  float4 tmpvar_33;
  tmpvar_33 = (((tmpvar_30 * tmpvar_30) + (tmpvar_31 * tmpvar_31)) + (tmpvar_32 * tmpvar_32));
  float4 tmpvar_34;
  tmpvar_34 = (max(float4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_30 * tmpvar_23.x) + (tmpvar_31 * tmpvar_23.y)) + (tmpvar_32 * tmpvar_23.z)) * rsqrt(tmpvar_33))) * (1.0/((1.0 + (tmpvar_33 * unity_4LightAtten0)))));
  float3 tmpvar_35;
  tmpvar_35 = ((((unity_LightColor[0].xyz * tmpvar_34.x) + (unity_LightColor[1].xyz * tmpvar_34.y)) + (unity_LightColor[2].xyz * tmpvar_34.z)) + (unity_LightColor[3].xyz * tmpvar_34.w));
  tmpvar_6 = tmpvar_35;
  float4 o_i0_i1;
  float4 tmpvar_36;
  tmpvar_36 = (tmpvar_21 * 0.5);
  o_i0_i1 = tmpvar_36;
  float2 tmpvar_37;
  tmpvar_37.x = tmpvar_36.x;
  tmpvar_37.y = (tmpvar_36.y * _ProjectionParams.x);
  o_i0_i1.xy = (tmpvar_37 + tmpvar_36.w);
  o_i0_i1.zw = tmpvar_21.zw;
  o.pos = tmpvar_21;
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
  o.xlv_TEXCOORD4 = tmpvar_7;
  o.xlv_TEXCOORD5 = o_i0_i1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_3;
  tmpvar_3 = i.xlv_TEXCOORD0.zw;
  tmpvar_1 = i.xlv_TEXCOORD1;
  float4 tmpvar_4;
  tmpvar_4 = tex2D(_MainTex, tmpvar_2);
  float3 tmpvar_5;
  tmpvar_5 = ((tex2D(_BumpMap, tmpvar_3).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_6;
  tmpvar_6.x = ((dot(tmpvar_5, i.xlv_TEXCOORD2) * 0.5) + 0.5);
  tmpvar_6.y = dot(tmpvar_5, normalize((i.xlv_TEXCOORD2 + i.xlv_TEXCOORD4)));
  float4 tmpvar_7;
  tmpvar_7 = tex2D(_BRDFTex, tmpvar_6);
  c_i0.xyz = (((tmpvar_1 * tmpvar_4.xyz) * (tmpvar_7.xyz + (tmpvar_4.w * tmpvar_7.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_4.xyz * i.xlv_TEXCOORD3));
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
	float3 xlv_TEXCOORD4 : TEXCOORD0;
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
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float3 tmpvar_8;
  float3x3 tmpvar_9;
  tmpvar_9._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_9._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_9._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_10;
  tmpvar_10.w = 1.0;
  tmpvar_10.xyz = (mul(tmpvar_9, (tmpvar_2 * pc_unity_Scale.w)));
  float3 tmpvar_11;
  float4 normal;
  normal = tmpvar_10;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHAr, normal);
  x1.x = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHAg, normal);
  x1.y = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot(unity_SHAb, normal);
  x1.z = tmpvar_14;
  float4 tmpvar_15;
  tmpvar_15 = (normal.xyzz * normal.yzzx);
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHBr, tmpvar_15);
  x2.x = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHBg, tmpvar_15);
  x2.y = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot(unity_SHBb, tmpvar_15);
  x2.z = tmpvar_18;
  float tmpvar_19;
  tmpvar_19 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_19;
  float3 tmpvar_20;
  tmpvar_20 = (unity_SHC.xyz * vC);
  x3 = tmpvar_20;
  tmpvar_11 = ((x1 + x2) + x3);
  tmpvar_8 = tmpvar_11;
  tmpvar_4 = tmpvar_8;
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_21;
  tmpvar_21._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_21._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_21._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_22;
  tmpvar_22 = (mul(tmpvar_21, (tmpvar_2 * pc_unity_Scale.w)));
  float3x3 tmpvar_23;
  tmpvar_23._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_23._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_23._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_24;
  tmpvar_24._m00_m10_m20.x = tmpvar_23._m00_m10_m20.x;
  tmpvar_24._m00_m10_m20.y = tmpvar_23._m01_m11_m21.x;
  tmpvar_24._m00_m10_m20.z = tmpvar_23._m02_m12_m22.x;
  tmpvar_24._m01_m11_m21.x = tmpvar_23._m00_m10_m20.y;
  tmpvar_24._m01_m11_m21.y = tmpvar_23._m01_m11_m21.y;
  tmpvar_24._m01_m11_m21.z = tmpvar_23._m02_m12_m22.y;
  tmpvar_24._m02_m12_m22.x = tmpvar_23._m00_m10_m20.z;
  tmpvar_24._m02_m12_m22.y = tmpvar_23._m01_m11_m21.z;
  tmpvar_24._m02_m12_m22.z = tmpvar_23._m02_m12_m22.z;
  float3 tmpvar_25;
  tmpvar_25 = (mul(tmpvar_24, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_5 = tmpvar_25;
  float4 tmpvar_26;
  tmpvar_26.w = 1.0;
  tmpvar_26.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_27;
  tmpvar_27 = normalize((mul(tmpvar_24, (((mul(pc_World2Object, tmpvar_26)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_7 = tmpvar_27;
  float3 tmpvar_28;
  tmpvar_28 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float4 tmpvar_29;
  tmpvar_29 = (unity_4LightPosX0 - tmpvar_28.x);
  float4 tmpvar_30;
  tmpvar_30 = (unity_4LightPosY0 - tmpvar_28.y);
  float4 tmpvar_31;
  tmpvar_31 = (unity_4LightPosZ0 - tmpvar_28.z);
  float4 tmpvar_32;
  tmpvar_32 = (((tmpvar_29 * tmpvar_29) + (tmpvar_30 * tmpvar_30)) + (tmpvar_31 * tmpvar_31));
  float4 tmpvar_33;
  tmpvar_33 = (max(float4(0.0, 0.0, 0.0, 0.0), ((((tmpvar_29 * tmpvar_22.x) + (tmpvar_30 * tmpvar_22.y)) + (tmpvar_31 * tmpvar_22.z)) * rsqrt(tmpvar_32))) * (1.0/((1.0 + (tmpvar_32 * unity_4LightAtten0)))));
  float3 tmpvar_34;
  tmpvar_34 = ((((unity_LightColor[0].xyz * tmpvar_33.x) + (unity_LightColor[1].xyz * tmpvar_33.y)) + (unity_LightColor[2].xyz * tmpvar_33.z)) + (unity_LightColor[3].xyz * tmpvar_33.w));
  tmpvar_6 = tmpvar_34;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
  o.xlv_TEXCOORD4 = tmpvar_7;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_3;
  tmpvar_3 = i.xlv_TEXCOORD0.zw;
  tmpvar_1 = i.xlv_TEXCOORD1;
  float4 tmpvar_4;
  tmpvar_4 = tex2D(_MainTex, tmpvar_2);
  float3 tmpvar_5;
  tmpvar_5 = ((tex2D(_BumpMap, tmpvar_3).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_6;
  tmpvar_6.x = ((dot(tmpvar_5, i.xlv_TEXCOORD2) * 0.5) + 0.5);
  tmpvar_6.y = dot(tmpvar_5, normalize((i.xlv_TEXCOORD2 + i.xlv_TEXCOORD4)));
  float4 tmpvar_7;
  tmpvar_7 = tex2D(_BRDFTex, tmpvar_6);
  c_i0.xyz = (((tmpvar_1 * tmpvar_4.xyz) * (tmpvar_7.xyz + (tmpvar_4.w * tmpvar_7.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_4.xyz * i.xlv_TEXCOORD3));
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
	float4 xlv_TEXCOORD5 : TEXCOORD0;
	float3 xlv_TEXCOORD4 : TEXCOORD1;
	float3 xlv_TEXCOORD3 : TEXCOORD2;
	float3 xlv_TEXCOORD2 : TEXCOORD3;
	float3 xlv_TEXCOORD1 : TEXCOORD4;
	float4 xlv_TEXCOORD0 : TEXCOORD5;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float3x3 tmpvar_8;
  tmpvar_8._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_8._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_8._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_9;
  tmpvar_9.w = 1.0;
  tmpvar_9.xyz = (mul(tmpvar_8, (tmpvar_2 * pc_unity_Scale.w)));
  float3 tmpvar_10;
  float4 normal;
  normal = tmpvar_9;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHAr, normal);
  x1.x = tmpvar_11;
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHAg, normal);
  x1.y = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHAb, normal);
  x1.z = tmpvar_13;
  float4 tmpvar_14;
  tmpvar_14 = (normal.xyzz * normal.yzzx);
  float tmpvar_15;
  tmpvar_15 = dot(unity_SHBr, tmpvar_14);
  x2.x = tmpvar_15;
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHBg, tmpvar_14);
  x2.y = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHBb, tmpvar_14);
  x2.z = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_18;
  float3 tmpvar_19;
  tmpvar_19 = (unity_SHC.xyz * vC);
  x3 = tmpvar_19;
  tmpvar_10 = ((x1 + x2) + x3);
  tmpvar_7 = tmpvar_10;
  tmpvar_4 = tmpvar_7;
  float4 tmpvar_20;
  tmpvar_20 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_21;
  tmpvar_21._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_21._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_21._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_22;
  tmpvar_22._m00_m10_m20.x = tmpvar_21._m00_m10_m20.x;
  tmpvar_22._m00_m10_m20.y = tmpvar_21._m01_m11_m21.x;
  tmpvar_22._m00_m10_m20.z = tmpvar_21._m02_m12_m22.x;
  tmpvar_22._m01_m11_m21.x = tmpvar_21._m00_m10_m20.y;
  tmpvar_22._m01_m11_m21.y = tmpvar_21._m01_m11_m21.y;
  tmpvar_22._m01_m11_m21.z = tmpvar_21._m02_m12_m22.y;
  tmpvar_22._m02_m12_m22.x = tmpvar_21._m00_m10_m20.z;
  tmpvar_22._m02_m12_m22.y = tmpvar_21._m01_m11_m21.z;
  tmpvar_22._m02_m12_m22.z = tmpvar_21._m02_m12_m22.z;
  float3 tmpvar_23;
  tmpvar_23 = (mul(tmpvar_22, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_5 = tmpvar_23;
  float4 tmpvar_24;
  tmpvar_24.w = 1.0;
  tmpvar_24.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_25;
  tmpvar_25 = normalize((mul(tmpvar_22, (((mul(pc_World2Object, tmpvar_24)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_6 = tmpvar_25;
  float4 o_i0_i1;
  float4 tmpvar_26;
  tmpvar_26 = (tmpvar_20 * 0.5);
  o_i0_i1 = tmpvar_26;
  float2 tmpvar_27;
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = (tmpvar_26.y * _ProjectionParams.x);
  o_i0_i1.xy = (tmpvar_27 + tmpvar_26.w);
  o_i0_i1.zw = tmpvar_20.zw;
  o.pos = tmpvar_20;
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = float3(0.0, 0.0, 0.0);
  o.xlv_TEXCOORD4 = tmpvar_6;
  o.xlv_TEXCOORD5 = o_i0_i1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_3;
  tmpvar_3 = i.xlv_TEXCOORD0.zw;
  tmpvar_1 = i.xlv_TEXCOORD1;
  float4 tmpvar_4;
  tmpvar_4 = tex2D(_MainTex, tmpvar_2);
  float3 tmpvar_5;
  tmpvar_5 = ((tex2D(_BumpMap, tmpvar_3).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_6;
  tmpvar_6.x = ((dot(tmpvar_5, i.xlv_TEXCOORD2) * 0.5) + 0.5);
  tmpvar_6.y = dot(tmpvar_5, normalize((i.xlv_TEXCOORD2 + i.xlv_TEXCOORD4)));
  float4 tmpvar_7;
  tmpvar_7 = tex2D(_BRDFTex, tmpvar_6);
  c_i0.xyz = (((tmpvar_1 * tmpvar_4.xyz) * (tmpvar_7.xyz + (tmpvar_4.w * tmpvar_7.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_4.xyz * i.xlv_TEXCOORD3));
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
	float3 xlv_TEXCOORD4 : TEXCOORD0;
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
  float4 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float3x3 tmpvar_8;
  tmpvar_8._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_8._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_8._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_9;
  tmpvar_9.w = 1.0;
  tmpvar_9.xyz = (mul(tmpvar_8, (tmpvar_2 * pc_unity_Scale.w)));
  float3 tmpvar_10;
  float4 normal;
  normal = tmpvar_9;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHAr, normal);
  x1.x = tmpvar_11;
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHAg, normal);
  x1.y = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = dot(unity_SHAb, normal);
  x1.z = tmpvar_13;
  float4 tmpvar_14;
  tmpvar_14 = (normal.xyzz * normal.yzzx);
  float tmpvar_15;
  tmpvar_15 = dot(unity_SHBr, tmpvar_14);
  x2.x = tmpvar_15;
  float tmpvar_16;
  tmpvar_16 = dot(unity_SHBg, tmpvar_14);
  x2.y = tmpvar_16;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHBb, tmpvar_14);
  x2.z = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_18;
  float3 tmpvar_19;
  tmpvar_19 = (unity_SHC.xyz * vC);
  x3 = tmpvar_19;
  tmpvar_10 = ((x1 + x2) + x3);
  tmpvar_7 = tmpvar_10;
  tmpvar_4 = tmpvar_7;
  tmpvar_3.xy = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3.zw = ((v._glesMultiTexCoord0.xy * _BumpMap_ST.xy) + _BumpMap_ST.zw);
  float3x3 tmpvar_20;
  tmpvar_20._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_20._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_20._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_21;
  tmpvar_21._m00_m10_m20.x = tmpvar_20._m00_m10_m20.x;
  tmpvar_21._m00_m10_m20.y = tmpvar_20._m01_m11_m21.x;
  tmpvar_21._m00_m10_m20.z = tmpvar_20._m02_m12_m22.x;
  tmpvar_21._m01_m11_m21.x = tmpvar_20._m00_m10_m20.y;
  tmpvar_21._m01_m11_m21.y = tmpvar_20._m01_m11_m21.y;
  tmpvar_21._m01_m11_m21.z = tmpvar_20._m02_m12_m22.y;
  tmpvar_21._m02_m12_m22.x = tmpvar_20._m00_m10_m20.z;
  tmpvar_21._m02_m12_m22.y = tmpvar_20._m01_m11_m21.z;
  tmpvar_21._m02_m12_m22.z = tmpvar_20._m02_m12_m22.z;
  float3 tmpvar_22;
  tmpvar_22 = (mul(tmpvar_21, (mul(pc_World2Object, _WorldSpaceLightPos0)).xyz));
  tmpvar_5 = tmpvar_22;
  float4 tmpvar_23;
  tmpvar_23.w = 1.0;
  tmpvar_23.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_24;
  tmpvar_24 = normalize((mul(tmpvar_21, (((mul(pc_World2Object, tmpvar_23)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_6 = tmpvar_24;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = float3(0.0, 0.0, 0.0);
  o.xlv_TEXCOORD4 = tmpvar_6;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float3 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = i.xlv_TEXCOORD0.xy;
  float2 tmpvar_3;
  tmpvar_3 = i.xlv_TEXCOORD0.zw;
  tmpvar_1 = i.xlv_TEXCOORD1;
  float4 tmpvar_4;
  tmpvar_4 = tex2D(_MainTex, tmpvar_2);
  float3 tmpvar_5;
  tmpvar_5 = ((tex2D(_BumpMap, tmpvar_3).xyz * 2.0) - 1.0);
  float4 c_i0;
  float2 tmpvar_6;
  tmpvar_6.x = ((dot(tmpvar_5, i.xlv_TEXCOORD2) * 0.5) + 0.5);
  tmpvar_6.y = dot(tmpvar_5, normalize((i.xlv_TEXCOORD2 + i.xlv_TEXCOORD4)));
  float4 tmpvar_7;
  tmpvar_7 = tex2D(_BRDFTex, tmpvar_6);
  c_i0.xyz = (((tmpvar_1 * tmpvar_4.xyz) * (tmpvar_7.xyz + (tmpvar_4.w * tmpvar_7.w))) * 2.0);
  c_i0.w = 0.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_4.xyz * i.xlv_TEXCOORD3));
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "Mobile/Diffuse"
}