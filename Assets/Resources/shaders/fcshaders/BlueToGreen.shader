Shader "Blue To Green" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
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
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((v._glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 texcol;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  texcol = tmpvar_1;
  texcol.y = texcol.z;
  pc_FragColor = texcol;
	return pc_FragColor;
}
ENDCG
 }
}
}