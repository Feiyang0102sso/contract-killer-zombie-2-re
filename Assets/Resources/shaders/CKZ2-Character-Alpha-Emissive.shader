Shader "CKZ2/Character + Emissive" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}
SubShader { 
 LOD 300
 Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry-2" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry-2" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ LIGHTMAP_ON
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
#if defined(LIGHTMAP_ON)
sampler2D _MainTex;
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
  float3 tmpvar_1;
  float3x3 tmpvar_2;
  tmpvar_2._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_2._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_2._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_3;
  tmpvar_3.w = 1.0;
  tmpvar_3.xyz = (mul(tmpvar_2, normalize(v._glesNormal)));
  float3 tmpvar_4;
  float4 normal;
  normal = tmpvar_3;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_5;
  tmpvar_5 = dot(unity_SHAr, normal);
  x1.x = tmpvar_5;
  float tmpvar_6;
  tmpvar_6 = dot(unity_SHAg, normal);
  x1.y = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = dot(unity_SHAb, normal);
  x1.z = tmpvar_7;
  float4 tmpvar_8;
  tmpvar_8 = (normal.xyzz * normal.yzzx);
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
  tmpvar_12 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_12;
  float3 tmpvar_13;
  tmpvar_13 = (unity_SHC.xyz * vC);
  x3 = tmpvar_13;
  tmpvar_4 = ((x1 + x2) + x3);
  tmpvar_1 = tmpvar_4;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz * (i.xlv_TEXCOORD1 + (tmpvar_1.w * tmpvar_1.xyz)));
  c.w = 1.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
sampler2D _MainTex;
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
  float3 tmpvar_1;
  float3x3 tmpvar_2;
  tmpvar_2._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_2._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_2._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_3;
  tmpvar_3.w = 1.0;
  tmpvar_3.xyz = (mul(tmpvar_2, normalize(v._glesNormal)));
  float3 tmpvar_4;
  float4 normal;
  normal = tmpvar_3;
  float3 x3;
  float vC;
  float3 x2;
  float3 x1;
  float tmpvar_5;
  tmpvar_5 = dot(unity_SHAr, normal);
  x1.x = tmpvar_5;
  float tmpvar_6;
  tmpvar_6 = dot(unity_SHAg, normal);
  x1.y = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = dot(unity_SHAb, normal);
  x1.z = tmpvar_7;
  float4 tmpvar_8;
  tmpvar_8 = (normal.xyzz * normal.yzzx);
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
  tmpvar_12 = ((normal.x * normal.x) - (normal.y * normal.y));
  vC = tmpvar_12;
  float3 tmpvar_13;
  tmpvar_13 = (unity_SHC.xyz * vC);
  x3 = tmpvar_13;
  tmpvar_4 = ((x1 + x2) + x3);
  tmpvar_1 = tmpvar_4;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz * (i.xlv_TEXCOORD1 + (tmpvar_1.w * tmpvar_1.xyz)));
  c.w = 1.0;
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "Mobile/Diffuse"
}