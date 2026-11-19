Shader "CKZ2/Depth Mask" {
SubShader { 
 LOD 100
 Tags { "QUEUE"="Overlay+1003" }
 Pass {
  Tags { "QUEUE"="Overlay+1003" }
  ZTest Always
  Cull Front
  ColorMask 0
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
struct appdata_pc {
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  tmpvar_1 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  tmpvar_1.z = 0.0;
  o.pos = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = float4(0.0, 0.0, 0.0, 0.0);
	return pc_FragColor;
}
ENDCG
 }
}
}