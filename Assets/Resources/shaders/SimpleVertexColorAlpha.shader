Shader "CKZ2/Simple Vertex Color Alpha" {
SubShader { 
 Pass {
  Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by gles_to_hlsl.py
struct appdata_pc {
	float4 _glesColor : COLOR;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_COLOR : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_COLOR = v._glesColor;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tmpvar_1;
  tmpvar_1 = i.xlv_COLOR;
  pc_FragColor = tmpvar_1;
	return pc_FragColor;
}
ENDCG
 }
}
Fallback "VertexLit"
}