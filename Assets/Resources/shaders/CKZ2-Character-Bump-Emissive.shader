Shader "CKZ2/Character - Bumped Emissive" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
 _ProbeStrength ("Probe Strength", Range(0.5,2)) = 1
 _EmissionMultiplier ("Emission Multiplier", Float) = 1
}
SubShader { 
 LOD 650
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _MainTex_ST;
float _ProbeStrength;
sampler2D _MainTex;
float _EmissionMultiplier;
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
  tmpvar_14 = (tmpvar_3.xyz * ((_ProbeStrength * ((x1 + x2) + x3)) + ((tmpvar_3.w * tmpvar_3.xyz) * _EmissionMultiplier)));
  col = tmpvar_14;
  float4 tmpvar_15;
  tmpvar_15.w = 1.0;
  tmpvar_15.xyz = col;
  pc_FragColor = tmpvar_15;
	return pc_FragColor;
}
ENDCG
 }
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float _ProbeStrength;
float4 _MainTex_ST;
sampler2D _MainTex;
float _EmissionMultiplier;
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
  float3 tmpvar_2;
  tmpvar_2 = (tmpvar_1.xyz * (i.xlv_TEXCOORD1 + ((tmpvar_1.w * tmpvar_1.xyz) * _EmissionMultiplier)));
  tex.xyz = tmpvar_2;
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
float _ProbeStrength;
float4 _MainTex_ST;
sampler2D _MainTex;
float _EmissionMultiplier;
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
  float3 tmpvar_2;
  tmpvar_2 = (tmpvar_1.xyz * (i.xlv_TEXCOORD1 + ((tmpvar_1.w * tmpvar_1.xyz) * _EmissionMultiplier)));
  tex.xyz = tmpvar_2;
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
// PC port: translated from the original GLES code by gles_to_hlsl.py
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