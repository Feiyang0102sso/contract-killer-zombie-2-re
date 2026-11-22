Shader "CKZ2/Lightmap2x + Unlit + VirtualVertexGlossy-Add" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _SpecOffset ("Specular Offset from Camera", Vector) = (1,10,2,0)
 _SpecRange ("Specular Range", Float) = 20
 _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
 _Shininess ("Shininess", Range(0.01,1)) = 0.078125
}
SubShader { 
 LOD 400
 Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ LIGHTMAP_ON
// PC port: translated from the original GLES code by gles_to_hlsl.py
#if defined(LIGHTMAP_ON)
float _SpecRange;
float3 _SpecOffset;
float3 _SpecColor;
float _Shininess;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord1 : TEXCOORD1;
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3x3 tmpvar_2;
  tmpvar_2._m00_m10_m20 = UNITY_MATRIX_MV._m00_m10_m20_m30.xyz;
  tmpvar_2._m01_m11_m21 = UNITY_MATRIX_MV._m01_m11_m21_m31.xyz;
  tmpvar_2._m02_m12_m22 = UNITY_MATRIX_MV._m02_m12_m22_m32.xyz;
  float3 tmpvar_3;
  tmpvar_3 = ((mul(UNITY_MATRIX_MV, v._glesVertex)).xyz - (_SpecOffset * float3(1.0, 1.0, -1.0)));
  float3 tmpvar_4;
  tmpvar_4 = (((_SpecColor * pow(clamp(dot((mul(tmpvar_2, normalize(v._glesNormal))), normalize(((float3(0.0, 0.0, 1.0) + normalize(-(tmpvar_3))) * 0.5))), 0.0, 1.0), (_Shininess * 128.0))) * 2.0) * (1.0 - clamp((length(tmpvar_3) / _SpecRange), 0.0, 1.0)));
  tmpvar_1 = tmpvar_4;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = ((v._glesMultiTexCoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
  o.xlv_TEXCOORD2 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz + (i.xlv_TEXCOORD2 * tmpvar_1.w));
  c.xyz = (c.xyz * (4.0 * UNITY_SAMPLE_TEX2D(unity_Lightmap, i.xlv_TEXCOORD1).xyz));
  pc_FragColor = c;
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
float _SpecRange;
float3 _SpecOffset;
float3 _SpecColor;
float _Shininess;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  float3x3 tmpvar_2;
  tmpvar_2._m00_m10_m20 = UNITY_MATRIX_MV._m00_m10_m20_m30.xyz;
  tmpvar_2._m01_m11_m21 = UNITY_MATRIX_MV._m01_m11_m21_m31.xyz;
  tmpvar_2._m02_m12_m22 = UNITY_MATRIX_MV._m02_m12_m22_m32.xyz;
  float3 tmpvar_3;
  tmpvar_3 = ((mul(UNITY_MATRIX_MV, v._glesVertex)).xyz - (_SpecOffset * float3(1.0, 1.0, -1.0)));
  float3 tmpvar_4;
  tmpvar_4 = (((_SpecColor * pow(clamp(dot((mul(tmpvar_2, normalize(v._glesNormal))), normalize(((float3(0.0, 0.0, 1.0) + normalize(-(tmpvar_3))) * 0.5))), 0.0, 1.0), (_Shininess * 128.0))) * 2.0) * (1.0 - clamp((length(tmpvar_3) / _SpecRange), 0.0, 1.0)));
  tmpvar_1 = tmpvar_4;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD2 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz + (i.xlv_TEXCOORD2 * tmpvar_1.w));
  pc_FragColor = c;
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "CKZ2/Lightmap + Unlit + Texture"
}