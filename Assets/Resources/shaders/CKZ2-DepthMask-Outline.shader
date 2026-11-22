Shader "CKZ2/Depth Mask Outline" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0.6,0.75,0.8,1)
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Overlay+1004" }
 Pass {
  Tags { "QUEUE"="Overlay+1004" }
  ZTest NotEqual
  ZWrite Off
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float _SilhouetteWidth;
float4 _SilhouetteColor;
struct appdata_pc {
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1 = tmpvar_2;
  float4 tmpvar_3;
  tmpvar_3.w = 0.0;
  tmpvar_3.xyz = normalize(v._glesNormal);
  tmpvar_1.xy = (tmpvar_2.xy + (normalize((mul(UNITY_MATRIX_MVP, tmpvar_3)).xy) * (_SilhouetteWidth * tmpvar_2.w)));
  tmpvar_1.z = 0.0;
  o.pos = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = _SilhouetteColor;
	return pc_FragColor;
}
ENDCG
 }
}
}