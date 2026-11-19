Shader "CKZ2/Bumped Specular with LightProbes - CG - ver2" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
 _ProbeStrength ("Probe Strength", Range(0.5,2)) = 1
 _ProbeIntensity ("Probe Intensity", Range(0,4)) = 0.1
 _ProbeShininess ("Probe Shininess", Range(0.1,128)) = 2
 _RimIntensity ("Rim Intensity", Range(0,4)) = 1
 _RimPower ("Rim Power", Range(0.5,8)) = 3
}
SubShader { 
 LOD 600
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float4 _MainTex_ST;
float _RimPower;
float _RimIntensity;
float _ProbeStrength;
float _ProbeShininess;
float _ProbeIntensity;
sampler2D _MainTex;
sampler2D _BumpMap;
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
	float3 xlv_TEXCOORD5 : TEXCOORD4;
	float2 xlv_TEXCOORD0 : TEXCOORD5;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float2 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float3 tmpvar_8;
  float2 tmpvar_9;
  tmpvar_9 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3 = tmpvar_9;
  float4 tmpvar_10;
  tmpvar_10.w = 1.0;
  tmpvar_10.xyz = _WorldSpaceCameraPos;
  float3x3 tmpvar_11;
  tmpvar_11._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_11._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_11._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_12;
  tmpvar_12 = ((mul(tmpvar_11, (v._glesVertex.xyz - ((mul(pc_World2Object, tmpvar_10)).xyz * pc_unity_Scale.w)))) * pc_unity_Scale.w);
  tmpvar_4 = tmpvar_12;
  float3x3 tmpvar_13;
  tmpvar_13._m00_m10_m20 = tmpvar_1.xyz;
  tmpvar_13._m01_m11_m21 = (cross(tmpvar_2, tmpvar_1.xyz) * v._glesTANGENT.w);
  tmpvar_13._m02_m12_m22 = tmpvar_2;
  float3x3 tmpvar_14;
  tmpvar_14._m00_m10_m20.x = tmpvar_13._m00_m10_m20.x;
  tmpvar_14._m00_m10_m20.y = tmpvar_13._m01_m11_m21.x;
  tmpvar_14._m00_m10_m20.z = tmpvar_13._m02_m12_m22.x;
  tmpvar_14._m01_m11_m21.x = tmpvar_13._m00_m10_m20.y;
  tmpvar_14._m01_m11_m21.y = tmpvar_13._m01_m11_m21.y;
  tmpvar_14._m01_m11_m21.z = tmpvar_13._m02_m12_m22.y;
  tmpvar_14._m02_m12_m22.x = tmpvar_13._m00_m10_m20.z;
  tmpvar_14._m02_m12_m22.y = tmpvar_13._m01_m11_m21.z;
  tmpvar_14._m02_m12_m22.z = tmpvar_13._m02_m12_m22.z;
  float4 v_i0_i1;
  v_i0_i1.x = unity_ObjectToWorld._m00_m10_m20_m30.x;
  v_i0_i1.y = unity_ObjectToWorld._m01_m11_m21_m31.x;
  v_i0_i1.z = unity_ObjectToWorld._m02_m12_m22_m32.x;
  v_i0_i1.w = unity_ObjectToWorld._m03_m13_m23_m33.x;
  float3 tmpvar_15;
  tmpvar_15 = ((mul(tmpvar_14, v_i0_i1.xyz)) * pc_unity_Scale.w);
  tmpvar_5 = tmpvar_15;
  float4 v_i0_i1_i2;
  v_i0_i1_i2.x = unity_ObjectToWorld._m00_m10_m20_m30.y;
  v_i0_i1_i2.y = unity_ObjectToWorld._m01_m11_m21_m31.y;
  v_i0_i1_i2.z = unity_ObjectToWorld._m02_m12_m22_m32.y;
  v_i0_i1_i2.w = unity_ObjectToWorld._m03_m13_m23_m33.y;
  float3 tmpvar_16;
  tmpvar_16 = ((mul(tmpvar_14, v_i0_i1_i2.xyz)) * pc_unity_Scale.w);
  tmpvar_6 = tmpvar_16;
  float4 v_i0_i1_i2_i3;
  v_i0_i1_i2_i3.x = unity_ObjectToWorld._m00_m10_m20_m30.z;
  v_i0_i1_i2_i3.y = unity_ObjectToWorld._m01_m11_m21_m31.z;
  v_i0_i1_i2_i3.z = unity_ObjectToWorld._m02_m12_m22_m32.z;
  v_i0_i1_i2_i3.w = unity_ObjectToWorld._m03_m13_m23_m33.z;
  float3 tmpvar_17;
  tmpvar_17 = ((mul(tmpvar_14, v_i0_i1_i2_i3.xyz)) * pc_unity_Scale.w);
  tmpvar_7 = tmpvar_17;
  float4 tmpvar_18;
  tmpvar_18.w = 1.0;
  tmpvar_18.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_19;
  tmpvar_19 = normalize((mul(tmpvar_14, (((mul(pc_World2Object, tmpvar_18)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz))));
  tmpvar_8 = tmpvar_19;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD5 = tmpvar_4;
  o.xlv_TEXCOORD1 = tmpvar_5;
  o.xlv_TEXCOORD2 = tmpvar_6;
  o.xlv_TEXCOORD3 = tmpvar_7;
  o.xlv_TEXCOORD4 = tmpvar_8;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float fresnel0;
  float3 probeReflectedColor;
  float3 worldRefl;
  float3 col;
  float3 tmpvar_1;
  tmpvar_1 = ((tex2D(_BumpMap, i.xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  float3 tmpvar_2;
  tmpvar_2.x = dot(i.xlv_TEXCOORD1, tmpvar_1);
  tmpvar_2.y = dot(i.xlv_TEXCOORD2, tmpvar_1);
  tmpvar_2.z = dot(i.xlv_TEXCOORD3, tmpvar_1);
  float4 tmpvar_3;
  tmpvar_3 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float4 tmpvar_4;
  tmpvar_4.w = 1.0;
  tmpvar_4.xyz = tmpvar_2;
  float4 normal_i0;
  normal_i0 = tmpvar_4;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_5;
  tmpvar_5 = dot(unity_SHAr, normal_i0);
  x1.x = tmpvar_5;
  float tmpvar_6;
  tmpvar_6 = dot(unity_SHAg, normal_i0);
  x1.y = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = dot(unity_SHAb, normal_i0);
  x1.z = tmpvar_7;
  float4 tmpvar_8;
  tmpvar_8 = (normal_i0.xyzz * normal_i0.yzzx);
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHBr, tmpvar_8);
  x2.x = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHBg, tmpvar_8);
  x2.y = tmpvar_10;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHBb, tmpvar_8);
  x2.z = tmpvar_11;
  float tmpvar_12;
  tmpvar_12 = ((normal_i0.x * normal_i0.x) - (normal_i0.y * normal_i0.y));
  vC = tmpvar_12;
  float3 tmpvar_13;
  tmpvar_13 = (unity_SHC.xyz * vC);
  x3 = tmpvar_13;
  float3 tmpvar_14;
  tmpvar_14 = (tmpvar_3.xyz * (_ProbeStrength * ((x1 + x2) + x3)));
  col = tmpvar_14;
  float3 tmpvar_15;
  tmpvar_15 = normalize(reflect(i.xlv_TEXCOORD5, tmpvar_2));
  worldRefl = tmpvar_15;
  float4 tmpvar_16;
  tmpvar_16.w = 1.0;
  tmpvar_16.xyz = worldRefl;
  float3 x3_i0;
  float vC_i0;
  float3 x2_i0;
  float3 x1_i0;
  float tmpvar_17;
  tmpvar_17 = dot(unity_SHAr, tmpvar_16);
  x1_i0.x = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot(unity_SHAg, tmpvar_16);
  x1_i0.y = tmpvar_18;
  float tmpvar_19;
  tmpvar_19 = dot(unity_SHAb, tmpvar_16);
  x1_i0.z = tmpvar_19;
  float4 tmpvar_20;
  tmpvar_20 = (worldRefl.xyzz * worldRefl.yzzx);
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHBr, tmpvar_20);
  x2_i0.x = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHBg, tmpvar_20);
  x2_i0.y = tmpvar_22;
  float tmpvar_23;
  tmpvar_23 = dot(unity_SHBb, tmpvar_20);
  x2_i0.z = tmpvar_23;
  float tmpvar_24;
  tmpvar_24 = ((worldRefl.x * worldRefl.x) - (worldRefl.y * worldRefl.y));
  vC_i0 = tmpvar_24;
  float3 tmpvar_25;
  tmpvar_25 = (unity_SHC.xyz * vC_i0);
  x3_i0 = tmpvar_25;
  float3 tmpvar_26;
  tmpvar_26 = (_ProbeStrength * ((x1_i0 + x2_i0) + x3_i0));
  probeReflectedColor = tmpvar_26;
  float tmpvar_27;
  tmpvar_27 = clamp(dot(i.xlv_TEXCOORD4, tmpvar_1), 0.0, 1.0);
  fresnel0 = tmpvar_27;
  float3 tmpvar_28;
  tmpvar_28 = (col + (probeReflectedColor * ((_ProbeIntensity * pow(fresnel0, _ProbeShininess)) * tmpvar_3.w)));
  col = tmpvar_28;
  float3 tmpvar_29;
  tmpvar_29 = (col + (((probeReflectedColor * _RimIntensity) * pow((1.0 - fresnel0), _RimPower)) * tmpvar_3.w));
  col = tmpvar_29;
  float4 tmpvar_30;
  tmpvar_30.w = 1.0;
  tmpvar_30.xyz = col;
  pc_FragColor = tmpvar_30;
	return pc_FragColor;
}
ENDCG
 }
}
SubShader { 
 LOD 500
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float _RimPower;
float _RimIntensity;
float _ProbeStrength;
float _ProbeShininess;
float _ProbeIntensity;
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _BumpMap;
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
	float3 xlv_TEXCOORD5 : TEXCOORD0;
	float3 xlv_TEXCOORD4 : TEXCOORD1;
	float3 xlv_TEXCOORD3 : TEXCOORD2;
	float3 xlv_TEXCOORD2 : TEXCOORD3;
	float3 xlv_TEXCOORD1 : TEXCOORD4;
	float2 xlv_TEXCOORD0 : TEXCOORD5;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 probeReflectedColor;
  float2 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float3 tmpvar_8;
  float2 tmpvar_9;
  tmpvar_9 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3 = tmpvar_9;
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
  float4 v_i0;
  v_i0.x = unity_ObjectToWorld._m00_m10_m20_m30.x;
  v_i0.y = unity_ObjectToWorld._m01_m11_m21_m31.x;
  v_i0.z = unity_ObjectToWorld._m02_m12_m22_m32.x;
  v_i0.w = unity_ObjectToWorld._m03_m13_m23_m33.x;
  float3 tmpvar_12;
  tmpvar_12 = ((mul(tmpvar_11, v_i0.xyz)) * pc_unity_Scale.w);
  tmpvar_4 = tmpvar_12;
  float4 v_i0_i1;
  v_i0_i1.x = unity_ObjectToWorld._m00_m10_m20_m30.y;
  v_i0_i1.y = unity_ObjectToWorld._m01_m11_m21_m31.y;
  v_i0_i1.z = unity_ObjectToWorld._m02_m12_m22_m32.y;
  v_i0_i1.w = unity_ObjectToWorld._m03_m13_m23_m33.y;
  float3 tmpvar_13;
  tmpvar_13 = ((mul(tmpvar_11, v_i0_i1.xyz)) * pc_unity_Scale.w);
  tmpvar_5 = tmpvar_13;
  float4 v_i0_i1_i2;
  v_i0_i1_i2.x = unity_ObjectToWorld._m00_m10_m20_m30.z;
  v_i0_i1_i2.y = unity_ObjectToWorld._m01_m11_m21_m31.z;
  v_i0_i1_i2.z = unity_ObjectToWorld._m02_m12_m22_m32.z;
  v_i0_i1_i2.w = unity_ObjectToWorld._m03_m13_m23_m33.z;
  float3 tmpvar_14;
  tmpvar_14 = ((mul(tmpvar_11, v_i0_i1_i2.xyz)) * pc_unity_Scale.w);
  tmpvar_6 = tmpvar_14;
  float3x3 tmpvar_15;
  tmpvar_15._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_15._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_15._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_16;
  tmpvar_16.w = 1.0;
  tmpvar_16.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_17;
  tmpvar_17 = normalize((((mul(pc_World2Object, tmpvar_16)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz));
  float3x3 tmpvar_18;
  tmpvar_18._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_18._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_18._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_19;
  tmpvar_19.w = 1.0;
  tmpvar_19.xyz = normalize(reflect(((mul(tmpvar_18, -(tmpvar_17))) * pc_unity_Scale.w), (mul(tmpvar_15, (tmpvar_2 * pc_unity_Scale.w)))));
  float4 normal;
  normal = tmpvar_19;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHAr, normal);
  x1.x = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHAg, normal);
  x1.y = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHAb, normal);
  x1.z = tmpvar_22;
  float4 tmpvar_23;
  tmpvar_23 = (normal.xyzz * normal.yzzx);
  float tmpvar_24;
  tmpvar_24 = dot(unity_SHBr, tmpvar_23);
  x2.x = tmpvar_24;
  float tmpvar_25;
  tmpvar_25 = dot(unity_SHBg, tmpvar_23);
  x2.y = tmpvar_25;
  float tmpvar_26;
  tmpvar_26 = dot(unity_SHBb, tmpvar_23);
  x2.z = tmpvar_26;
  float tmpvar_27;
  tmpvar_27 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_27;
  float3 tmpvar_28;
  tmpvar_28 = (unity_SHC.xyz * vC);
  x3 = tmpvar_28;
  float3 tmpvar_29;
  tmpvar_29 = (_ProbeStrength * ((x1 + x2) + x3));
  probeReflectedColor = tmpvar_29;
  float tmpvar_30;
  tmpvar_30 = clamp(dot(tmpvar_17, (tmpvar_2 * pc_unity_Scale.w)), 0.0, 1.0);
  float3 tmpvar_31;
  tmpvar_31 = (probeReflectedColor * (_ProbeIntensity * pow(tmpvar_30, _ProbeShininess)));
  tmpvar_7 = tmpvar_31;
  float3 tmpvar_32;
  tmpvar_32 = ((probeReflectedColor * _RimIntensity) * pow((1.0 - tmpvar_30), _RimPower));
  tmpvar_8 = tmpvar_32;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
  o.xlv_TEXCOORD4 = tmpvar_7;
  o.xlv_TEXCOORD5 = tmpvar_8;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tex;
  float3 tmpvar_1;
  tmpvar_1 = ((tex2D(_BumpMap, i.xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  float3 tmpvar_2;
  tmpvar_2.x = dot(i.xlv_TEXCOORD1, tmpvar_1);
  tmpvar_2.y = dot(i.xlv_TEXCOORD2, tmpvar_1);
  tmpvar_2.z = dot(i.xlv_TEXCOORD3, tmpvar_1);
  float4 tmpvar_3;
  tmpvar_3 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  tex = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4.w = 1.0;
  tmpvar_4.xyz = tmpvar_2;
  float4 normal_i0;
  normal_i0 = tmpvar_4;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_5;
  tmpvar_5 = dot(unity_SHAr, normal_i0);
  x1.x = tmpvar_5;
  float tmpvar_6;
  tmpvar_6 = dot(unity_SHAg, normal_i0);
  x1.y = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = dot(unity_SHAb, normal_i0);
  x1.z = tmpvar_7;
  float4 tmpvar_8;
  tmpvar_8 = (normal_i0.xyzz * normal_i0.yzzx);
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHBr, tmpvar_8);
  x2.x = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHBg, tmpvar_8);
  x2.y = tmpvar_10;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHBb, tmpvar_8);
  x2.z = tmpvar_11;
  float tmpvar_12;
  tmpvar_12 = ((normal_i0.x * normal_i0.x) - (normal_i0.y * normal_i0.y));
  vC = tmpvar_12;
  float3 tmpvar_13;
  tmpvar_13 = (unity_SHC.xyz * vC);
  x3 = tmpvar_13;
  float3 tmpvar_14;
  tmpvar_14 = (tmpvar_3.xyz * (_ProbeStrength * ((x1 + x2) + x3)));
  tex.xyz = tmpvar_14;
  tex.xyz = (tex.xyz + (i.xlv_TEXCOORD4 * tmpvar_3.w));
  tex.xyz = (tex.xyz + (i.xlv_TEXCOORD5 * tmpvar_3.w));
  pc_FragColor = tex;
	return pc_FragColor;
}
ENDCG
 }
}
SubShader { 
 LOD 400
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float _RimPower;
float _RimIntensity;
float _ProbeStrength;
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _BumpMap;
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
	float3 xlv_TEXCOORD5 : TEXCOORD0;
	float3 xlv_TEXCOORD3 : TEXCOORD1;
	float3 xlv_TEXCOORD2 : TEXCOORD2;
	float3 xlv_TEXCOORD1 : TEXCOORD3;
	float2 xlv_TEXCOORD0 : TEXCOORD4;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 probeReflectedColor;
  float2 tmpvar_3;
  float3 tmpvar_4;
  float3 tmpvar_5;
  float3 tmpvar_6;
  float3 tmpvar_7;
  float2 tmpvar_8;
  tmpvar_8 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_3 = tmpvar_8;
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
  float4 v_i0;
  v_i0.x = unity_ObjectToWorld._m00_m10_m20_m30.x;
  v_i0.y = unity_ObjectToWorld._m01_m11_m21_m31.x;
  v_i0.z = unity_ObjectToWorld._m02_m12_m22_m32.x;
  v_i0.w = unity_ObjectToWorld._m03_m13_m23_m33.x;
  float3 tmpvar_11;
  tmpvar_11 = ((mul(tmpvar_10, v_i0.xyz)) * pc_unity_Scale.w);
  tmpvar_4 = tmpvar_11;
  float4 v_i0_i1;
  v_i0_i1.x = unity_ObjectToWorld._m00_m10_m20_m30.y;
  v_i0_i1.y = unity_ObjectToWorld._m01_m11_m21_m31.y;
  v_i0_i1.z = unity_ObjectToWorld._m02_m12_m22_m32.y;
  v_i0_i1.w = unity_ObjectToWorld._m03_m13_m23_m33.y;
  float3 tmpvar_12;
  tmpvar_12 = ((mul(tmpvar_10, v_i0_i1.xyz)) * pc_unity_Scale.w);
  tmpvar_5 = tmpvar_12;
  float4 v_i0_i1_i2;
  v_i0_i1_i2.x = unity_ObjectToWorld._m00_m10_m20_m30.z;
  v_i0_i1_i2.y = unity_ObjectToWorld._m01_m11_m21_m31.z;
  v_i0_i1_i2.z = unity_ObjectToWorld._m02_m12_m22_m32.z;
  v_i0_i1_i2.w = unity_ObjectToWorld._m03_m13_m23_m33.z;
  float3 tmpvar_13;
  tmpvar_13 = ((mul(tmpvar_10, v_i0_i1_i2.xyz)) * pc_unity_Scale.w);
  tmpvar_6 = tmpvar_13;
  float3x3 tmpvar_14;
  tmpvar_14._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_14._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_14._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_15;
  tmpvar_15.w = 1.0;
  tmpvar_15.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_16;
  tmpvar_16 = normalize((((mul(pc_World2Object, tmpvar_15)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz));
  float3x3 tmpvar_17;
  tmpvar_17._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_17._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_17._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_18;
  tmpvar_18.w = 1.0;
  tmpvar_18.xyz = normalize(reflect(((mul(tmpvar_17, -(tmpvar_16))) * pc_unity_Scale.w), (mul(tmpvar_14, (tmpvar_2 * pc_unity_Scale.w)))));
  float4 normal;
  normal = tmpvar_18;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_19;
  tmpvar_19 = dot(unity_SHAr, normal);
  x1.x = tmpvar_19;
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHAg, normal);
  x1.y = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHAb, normal);
  x1.z = tmpvar_21;
  float4 tmpvar_22;
  tmpvar_22 = (normal.xyzz * normal.yzzx);
  float tmpvar_23;
  tmpvar_23 = dot(unity_SHBr, tmpvar_22);
  x2.x = tmpvar_23;
  float tmpvar_24;
  tmpvar_24 = dot(unity_SHBg, tmpvar_22);
  x2.y = tmpvar_24;
  float tmpvar_25;
  tmpvar_25 = dot(unity_SHBb, tmpvar_22);
  x2.z = tmpvar_25;
  float tmpvar_26;
  tmpvar_26 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_26;
  float3 tmpvar_27;
  tmpvar_27 = (unity_SHC.xyz * vC);
  x3 = tmpvar_27;
  float3 tmpvar_28;
  tmpvar_28 = (_ProbeStrength * ((x1 + x2) + x3));
  probeReflectedColor = tmpvar_28;
  float3 tmpvar_29;
  tmpvar_29 = ((probeReflectedColor * _RimIntensity) * pow((1.0 - clamp(dot(tmpvar_16, (tmpvar_2 * pc_unity_Scale.w)), 0.0, 1.0)), _RimPower));
  tmpvar_7 = tmpvar_29;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_3;
  o.xlv_TEXCOORD1 = tmpvar_4;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_6;
  o.xlv_TEXCOORD5 = tmpvar_7;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tex;
  float3 tmpvar_1;
  tmpvar_1 = ((tex2D(_BumpMap, i.xlv_TEXCOORD0).xyz * 2.0) - 1.0);
  float3 tmpvar_2;
  tmpvar_2.x = dot(i.xlv_TEXCOORD1, tmpvar_1);
  tmpvar_2.y = dot(i.xlv_TEXCOORD2, tmpvar_1);
  tmpvar_2.z = dot(i.xlv_TEXCOORD3, tmpvar_1);
  float4 tmpvar_3;
  tmpvar_3 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  tex = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4.w = 1.0;
  tmpvar_4.xyz = tmpvar_2;
  float4 normal_i0;
  normal_i0 = tmpvar_4;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_5;
  tmpvar_5 = dot(unity_SHAr, normal_i0);
  x1.x = tmpvar_5;
  float tmpvar_6;
  tmpvar_6 = dot(unity_SHAg, normal_i0);
  x1.y = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = dot(unity_SHAb, normal_i0);
  x1.z = tmpvar_7;
  float4 tmpvar_8;
  tmpvar_8 = (normal_i0.xyzz * normal_i0.yzzx);
  float tmpvar_9;
  tmpvar_9 = dot(unity_SHBr, tmpvar_8);
  x2.x = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHBg, tmpvar_8);
  x2.y = tmpvar_10;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHBb, tmpvar_8);
  x2.z = tmpvar_11;
  float tmpvar_12;
  tmpvar_12 = ((normal_i0.x * normal_i0.x) - (normal_i0.y * normal_i0.y));
  vC = tmpvar_12;
  float3 tmpvar_13;
  tmpvar_13 = (unity_SHC.xyz * vC);
  x3 = tmpvar_13;
  float3 tmpvar_14;
  tmpvar_14 = (tmpvar_3.xyz * (_ProbeStrength * ((x1 + x2) + x3)));
  tex.xyz = tmpvar_14;
  tex.xyz = (tex.xyz + (i.xlv_TEXCOORD5 * tmpvar_3.w));
  pc_FragColor = tex;
	return pc_FragColor;
}
ENDCG
 }
}
SubShader { 
 LOD 300
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float _RimPower;
float _RimIntensity;
float _ProbeStrength;
float4 _MainTex_ST;
sampler2D _MainTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])
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
  tmpvar_1 = normalize(v._glesNormal);
  float3 probeReflectedColor;
  float2 tmpvar_2;
  float3 tmpvar_3;
  float3 tmpvar_4;
  float2 tmpvar_5;
  tmpvar_5 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_2 = tmpvar_5;
  float3x3 tmpvar_6;
  tmpvar_6._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_6._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_6._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_7;
  tmpvar_7 = (mul(tmpvar_6, (tmpvar_1 * pc_unity_Scale.w)));
  float4 tmpvar_8;
  tmpvar_8.w = 1.0;
  tmpvar_8.xyz = tmpvar_7;
  float4 normal;
  normal = tmpvar_8;
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
  float3 tmpvar_18;
  tmpvar_18 = (_ProbeStrength * ((x1 + x2) + x3));
  tmpvar_3 = tmpvar_18;
  float4 tmpvar_19;
  tmpvar_19.w = 1.0;
  tmpvar_19.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_20;
  tmpvar_20 = normalize((((mul(pc_World2Object, tmpvar_19)).xyz * pc_unity_Scale.w) - v._glesVertex.xyz));
  float3x3 tmpvar_21;
  tmpvar_21._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_21._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_21._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_22;
  tmpvar_22.w = 1.0;
  tmpvar_22.xyz = normalize(reflect(((mul(tmpvar_21, -(tmpvar_20))) * pc_unity_Scale.w), tmpvar_7));
  float4 normal_i0;
  normal_i0 = tmpvar_22;
  float3 x3_i0;
  float vC_i0;
  float3 x2_i0;
  float3 x1_i0;
  float tmpvar_23;
  tmpvar_23 = dot(unity_SHAr, normal_i0);
  x1_i0.x = tmpvar_23;
  float tmpvar_24;
  tmpvar_24 = dot(unity_SHAg, normal_i0);
  x1_i0.y = tmpvar_24;
  float tmpvar_25;
  tmpvar_25 = dot(unity_SHAb, normal_i0);
  x1_i0.z = tmpvar_25;
  float4 tmpvar_26;
  tmpvar_26 = (normal_i0.xyzz * normal_i0.yzzx);
  float tmpvar_27;
  tmpvar_27 = dot(unity_SHBr, tmpvar_26);
  x2_i0.x = tmpvar_27;
  float tmpvar_28;
  tmpvar_28 = dot(unity_SHBg, tmpvar_26);
  x2_i0.y = tmpvar_28;
  float tmpvar_29;
  tmpvar_29 = dot(unity_SHBb, tmpvar_26);
  x2_i0.z = tmpvar_29;
  float tmpvar_30;
  tmpvar_30 = ((normal_i0.x * normal_i0.x) - (normal_i0.y * normal_i0.y));
  vC_i0 = tmpvar_30;
  float3 tmpvar_31;
  tmpvar_31 = (unity_SHC.xyz * vC_i0);
  x3_i0 = tmpvar_31;
  float3 tmpvar_32;
  tmpvar_32 = (_ProbeStrength * ((x1_i0 + x2_i0) + x3_i0));
  probeReflectedColor = tmpvar_32;
  float3 tmpvar_33;
  tmpvar_33 = ((probeReflectedColor * _RimIntensity) * pow((1.0 - clamp(dot(tmpvar_20, (tmpvar_1 * pc_unity_Scale.w)), 0.0, 1.0)), _RimPower));
  tmpvar_4 = tmpvar_33;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_2;
  o.xlv_TEXCOORD1 = tmpvar_3;
  o.xlv_TEXCOORD2 = tmpvar_4;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tex;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  tex = tmpvar_1;
  tex.xyz = (tmpvar_1.xyz * i.xlv_TEXCOORD1);
  tex.xyz = (tex.xyz + i.xlv_TEXCOORD2);
  pc_FragColor = tex;
	return pc_FragColor;
}
ENDCG
 }
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float _ProbeStrength;
float4 _MainTex_ST;
sampler2D _MainTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  float3 tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_3;
  float3x3 tmpvar_4;
  tmpvar_4._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_4._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_4._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_5;
  tmpvar_5.w = 1.0;
  tmpvar_5.xyz = (mul(tmpvar_4, (normalize(v._glesNormal) * pc_unity_Scale.w)));
  float4 normal;
  normal = tmpvar_5;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_6;
  tmpvar_6 = dot(unity_SHAr, normal);
  x1.x = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = dot(unity_SHAg, normal);
  x1.y = tmpvar_7;
  float tmpvar_8;
  tmpvar_8 = dot(unity_SHAb, normal);
  x1.z = tmpvar_8;
  float4 tmpvar_9;
  tmpvar_9 = (normal.xyzz * normal.yzzx);
  float tmpvar_10;
  tmpvar_10 = dot(unity_SHBr, tmpvar_9);
  x2.x = tmpvar_10;
  float tmpvar_11;
  tmpvar_11 = dot(unity_SHBg, tmpvar_9);
  x2.y = tmpvar_11;
  float tmpvar_12;
  tmpvar_12 = dot(unity_SHBb, tmpvar_9);
  x2.z = tmpvar_12;
  float tmpvar_13;
  tmpvar_13 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_13;
  float3 tmpvar_14;
  tmpvar_14 = (unity_SHC.xyz * vC);
  x3 = tmpvar_14;
  float3 tmpvar_15;
  tmpvar_15 = (_ProbeStrength * ((x1 + x2) + x3));
  tmpvar_2 = tmpvar_15;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tex;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  tex = tmpvar_1;
  tex.xyz = (tmpvar_1.xyz * i.xlv_TEXCOORD1);
  pc_FragColor = tex;
	return pc_FragColor;
}
ENDCG
 }
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float4 _MainTex_ST;
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
  float2 tmpvar_2;
  tmpvar_2 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_2;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = tex2D(_MainTex, i.xlv_TEXCOORD0);
	return pc_FragColor;
}
ENDCG
 }
}
}