Shader "CKZ2/Lightmap-ReflectiveCubemap-VirtualVertexGlossy-Add" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _EnvTex ("Cubemap env", CUBE) = "black" {}
 _SpecOffset ("Specular Offset from Camera", Vector) = (1,10,2,0)
 _SpecRange ("Specular Range", Float) = 20
 _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
 _Shininess ("Shininess", Range(0.01,2)) = 0.078125
 _ReflectionCoeff ("Reflection Coeff", Range(0.01,1)) = 0.5
 _ReflectionMagicShift ("Reflection Magic Shift", Range(-0.6,0.6)) = 0
}
SubShader { 
 LOD 500
 Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ LIGHTMAP_ON
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
#if defined(LIGHTMAP_ON)
float _SpecRange;
float3 _SpecOffset;
float3 _SpecColor;
float _Shininess;
float _ReflectionMagicShift;
float _ReflectionCoeff;
sampler2D _MainTex;
samplerCUBE _EnvTex;
struct appdata_pc {
	float4 _glesMultiTexCoord1 : TEXCOORD1;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float2 xlv_TEXCOORD1 : TEXCOORD2;
	float2 xlv_TEXCOORD0 : TEXCOORD3;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  tmpvar_1 = normalize(v._glesNormal);
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = reflect(((mul(unity_ObjectToWorld, v._glesVertex)).xyz - _WorldSpaceCameraPos), ((mul(tmpvar_3, tmpvar_1)) * _ReflectionMagicShift));
  float3 tmpvar_5;
  tmpvar_5.x = -(tmpvar_4.x);
  tmpvar_5.y = tmpvar_4.y;
  tmpvar_5.z = -(tmpvar_4.z);
  float3x3 tmpvar_6;
  tmpvar_6._m00_m10_m20 = UNITY_MATRIX_MV._m00_m10_m20_m30.xyz;
  tmpvar_6._m01_m11_m21 = UNITY_MATRIX_MV._m01_m11_m21_m31.xyz;
  tmpvar_6._m02_m12_m22 = UNITY_MATRIX_MV._m02_m12_m22_m32.xyz;
  float3 tmpvar_7;
  tmpvar_7 = ((mul(UNITY_MATRIX_MV, v._glesVertex)).xyz - (_SpecOffset * float3(1.0, 1.0, -1.0)));
  float3 tmpvar_8;
  tmpvar_8 = (((_SpecColor * pow(clamp(dot((mul(tmpvar_6, tmpvar_1)), normalize(((float3(0.0, 0.0, 1.0) + normalize(-(tmpvar_7))) * 0.5))), 0.0, 1.0), (_Shininess * 128.0))) * 2.0) * (1.0 - clamp((length(tmpvar_7) / _SpecRange), 0.0, 1.0)));
  tmpvar_2 = tmpvar_8;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = ((v._glesMultiTexCoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz + (i.xlv_TEXCOORD3 * tmpvar_1.w));
  c.xyz = (c.xyz * (2.0 * UNITY_SAMPLE_TEX2D(unity_Lightmap, i.xlv_TEXCOORD1).xyz));
  float4 tmpvar_2;
  tmpvar_2 = texCUBE(_EnvTex, i.xlv_TEXCOORD2);
  float3 tmpvar_3;
  tmpvar_3 = (c.xyz + ((tmpvar_2 * tmpvar_1.w) * _ReflectionCoeff).xyz);
  c.xyz = tmpvar_3;
  pc_FragColor = c;
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
float _SpecRange;
float3 _SpecOffset;
float3 _SpecColor;
float _Shininess;
float _ReflectionMagicShift;
float _ReflectionCoeff;
sampler2D _MainTex;
samplerCUBE _EnvTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD3 : TEXCOORD0;
	float3 xlv_TEXCOORD2 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  tmpvar_1 = normalize(v._glesNormal);
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3._m00_m10_m20 = unity_ObjectToWorld._m00_m10_m20_m30.xyz;
  tmpvar_3._m01_m11_m21 = unity_ObjectToWorld._m01_m11_m21_m31.xyz;
  tmpvar_3._m02_m12_m22 = unity_ObjectToWorld._m02_m12_m22_m32.xyz;
  float3 tmpvar_4;
  tmpvar_4 = reflect(((mul(unity_ObjectToWorld, v._glesVertex)).xyz - _WorldSpaceCameraPos), ((mul(tmpvar_3, tmpvar_1)) * _ReflectionMagicShift));
  float3 tmpvar_5;
  tmpvar_5.x = -(tmpvar_4.x);
  tmpvar_5.y = tmpvar_4.y;
  tmpvar_5.z = -(tmpvar_4.z);
  float3x3 tmpvar_6;
  tmpvar_6._m00_m10_m20 = UNITY_MATRIX_MV._m00_m10_m20_m30.xyz;
  tmpvar_6._m01_m11_m21 = UNITY_MATRIX_MV._m01_m11_m21_m31.xyz;
  tmpvar_6._m02_m12_m22 = UNITY_MATRIX_MV._m02_m12_m22_m32.xyz;
  float3 tmpvar_7;
  tmpvar_7 = ((mul(UNITY_MATRIX_MV, v._glesVertex)).xyz - (_SpecOffset * float3(1.0, 1.0, -1.0)));
  float3 tmpvar_8;
  tmpvar_8 = (((_SpecColor * pow(clamp(dot((mul(tmpvar_6, tmpvar_1)), normalize(((float3(0.0, 0.0, 1.0) + normalize(-(tmpvar_7))) * 0.5))), 0.0, 1.0), (_Shininess * 128.0))) * 2.0) * (1.0 - clamp((length(tmpvar_7) / _SpecRange), 0.0, 1.0)));
  tmpvar_2 = tmpvar_8;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD2 = tmpvar_5;
  o.xlv_TEXCOORD3 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz + (i.xlv_TEXCOORD3 * tmpvar_1.w));
  float4 tmpvar_2;
  tmpvar_2 = texCUBE(_EnvTex, i.xlv_TEXCOORD2);
  float3 tmpvar_3;
  tmpvar_3 = (c.xyz + ((tmpvar_2 * tmpvar_1.w) * _ReflectionCoeff).xyz);
  c.xyz = tmpvar_3;
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "CKZ2/Lightmap + Unlit + VirtualVertexGlossy-Add"
}