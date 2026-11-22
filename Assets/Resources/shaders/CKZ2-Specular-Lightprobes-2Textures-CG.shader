Shader "CKZ2/Specular with LightProbes, 2 Textures - CG" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _DamageTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _Sphere1 ("Sphere 1", Vector) = (50,50,50,10)
 _Sphere2 ("Sphere 2", Vector) = (50,50,50,10)
 _Sphere3 ("Sphere 3", Vector) = (50,50,50,10)
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
float4 _Sphere3;
float4 _Sphere2;
float4 _Sphere1;
float4 _MainTex_ST;
sampler2D _MainTex;
sampler2D _DamageTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_unity_Scale float4(1.0, 1.0, 1.0, 1.0 / pc_ObjectScale())
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float xlv_TEXCOORD2 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  float3 tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_3;
  float3 tmpvar_4;
  tmpvar_4 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  float3x3 tmpvar_5;
  tmpvar_5._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_5._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_5._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float4 tmpvar_6;
  tmpvar_6.w = 1.0;
  tmpvar_6.xyz = (mul(tmpvar_5, (normalize(v._glesNormal) * pc_unity_Scale.w)));
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
  tmpvar_2 = tmpvar_7;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = ((step(_Sphere1.w, distance(tmpvar_4, _Sphere1.xyz)) * step(_Sphere2.w, distance(tmpvar_4, _Sphere2.xyz))) * step(_Sphere3.w, distance(tmpvar_4, _Sphere3.xyz)));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tex;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float4 tmpvar_2;
  tmpvar_2 = tex2D(_DamageTex, (i.xlv_TEXCOORD0 * 4.0));
  float4 tmpvar_3;
  tmpvar_3 = ((i.xlv_TEXCOORD2 * tmpvar_1) + ((1.0 - i.xlv_TEXCOORD2) * tmpvar_2));
  tex = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4.w = 1.0;
  tmpvar_4.xyz = tex.xyz;
  pc_FragColor = tmpvar_4;
	return pc_FragColor;
}
ENDCG
 }
}
}