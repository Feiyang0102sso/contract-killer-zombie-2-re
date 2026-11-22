Shader "CKZ2/Character with Rim Lighting" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _RimIntensity ("Rim Intensity", Range(0,4)) = 1
 _RimPower ("Rim Power", Range(0.5,8)) = 3
}
SubShader { 
 LOD 400
 Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry-2" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry-2" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ LIGHTMAP_ON
// PC port: translated from the original GLES code by gles_to_hlsl.py
#if defined(LIGHTMAP_ON)
float _RimPower;
float _RimIntensity;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesTANGENT : TANGENT;
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
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 tmpvar_3;
  float3x3 tmpvar_4;
  tmpvar_4._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_4._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_4._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3x3 tmpvar_5;
  tmpvar_5._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_5._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_5._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_6;
  tmpvar_6.w = 0.0;
  tmpvar_6.xyz = tmpvar_2;
  float4 tmpvar_7;
  tmpvar_7.w = 1.0;
  tmpvar_7.xyz = (mul(tmpvar_4, tmpvar_2));
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
  float4 tmpvar_18;
  tmpvar_18.w = 1.0;
  tmpvar_18.xyz = (mul(tmpvar_5, tmpvar_1.xyz));
  float3 tmpvar_19;
  float4 normal_i0;
  normal_i0 = tmpvar_18;
  float3 x3_i0;
  float vC_i0;
  float3 x2_i0;
  float3 x1_i0;
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHAr, normal_i0);
  x1_i0.x = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHAg, normal_i0);
  x1_i0.y = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHAb, normal_i0);
  x1_i0.z = tmpvar_22;
  float4 tmpvar_23;
  tmpvar_23 = (normal_i0.xyzz * normal_i0.yzzx);
  float tmpvar_24;
  tmpvar_24 = dot(unity_SHBr, tmpvar_23);
  x2_i0.x = tmpvar_24;
  float tmpvar_25;
  tmpvar_25 = dot(unity_SHBg, tmpvar_23);
  x2_i0.y = tmpvar_25;
  float tmpvar_26;
  tmpvar_26 = dot(unity_SHBb, tmpvar_23);
  x2_i0.z = tmpvar_26;
  float tmpvar_27;
  tmpvar_27 = ((normal_i0.x * normal_i0.x) - (normal_i0.y * normal_i0.y));
  vC_i0 = tmpvar_27;
  float3 tmpvar_28;
  tmpvar_28 = (unity_SHC.xyz * vC_i0);
  x3_i0 = tmpvar_28;
  tmpvar_19 = ((x1_i0 + x2_i0) + x3_i0);
  float3 tmpvar_29;
  tmpvar_29 = (tmpvar_8 + ((tmpvar_19 * _RimIntensity) * pow((1.0 - (mul(UNITY_MATRIX_MV, tmpvar_6)).z), _RimPower)));
  tmpvar_3 = tmpvar_29;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz * i.xlv_TEXCOORD1);
  pc_FragColor = c;
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
float _RimPower;
float _RimIntensity;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesTANGENT : TANGENT;
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
  float4 tmpvar_1;
  tmpvar_1.xyz = normalize(v._glesTANGENT.xyz);
  tmpvar_1.w = v._glesTANGENT.w;
  float3 tmpvar_2;
  tmpvar_2 = normalize(v._glesNormal);
  float3 tmpvar_3;
  float3x3 tmpvar_4;
  tmpvar_4._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_4._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_4._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3x3 tmpvar_5;
  tmpvar_5._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_5._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_5._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_6;
  tmpvar_6.w = 0.0;
  tmpvar_6.xyz = tmpvar_2;
  float4 tmpvar_7;
  tmpvar_7.w = 1.0;
  tmpvar_7.xyz = (mul(tmpvar_4, tmpvar_2));
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
  float4 tmpvar_18;
  tmpvar_18.w = 1.0;
  tmpvar_18.xyz = (mul(tmpvar_5, tmpvar_1.xyz));
  float3 tmpvar_19;
  float4 normal_i0;
  normal_i0 = tmpvar_18;
  float3 x3_i0;
  float vC_i0;
  float3 x2_i0;
  float3 x1_i0;
  float tmpvar_20;
  tmpvar_20 = dot(unity_SHAr, normal_i0);
  x1_i0.x = tmpvar_20;
  float tmpvar_21;
  tmpvar_21 = dot(unity_SHAg, normal_i0);
  x1_i0.y = tmpvar_21;
  float tmpvar_22;
  tmpvar_22 = dot(unity_SHAb, normal_i0);
  x1_i0.z = tmpvar_22;
  float4 tmpvar_23;
  tmpvar_23 = (normal_i0.xyzz * normal_i0.yzzx);
  float tmpvar_24;
  tmpvar_24 = dot(unity_SHBr, tmpvar_23);
  x2_i0.x = tmpvar_24;
  float tmpvar_25;
  tmpvar_25 = dot(unity_SHBg, tmpvar_23);
  x2_i0.y = tmpvar_25;
  float tmpvar_26;
  tmpvar_26 = dot(unity_SHBb, tmpvar_23);
  x2_i0.z = tmpvar_26;
  float tmpvar_27;
  tmpvar_27 = ((normal_i0.x * normal_i0.x) - (normal_i0.y * normal_i0.y));
  vC_i0 = tmpvar_27;
  float3 tmpvar_28;
  tmpvar_28 = (unity_SHC.xyz * vC_i0);
  x3_i0 = tmpvar_28;
  tmpvar_19 = ((x1_i0 + x2_i0) + x3_i0);
  float3 tmpvar_29;
  tmpvar_29 = (tmpvar_8 + ((tmpvar_19 * _RimIntensity) * pow((1.0 - (mul(UNITY_MATRIX_MV, tmpvar_6)).z), _RimPower)));
  tmpvar_3 = tmpvar_29;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz * i.xlv_TEXCOORD1);
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "Mobile/Diffuse"
}