Shader "CKZ2/Color NoZWrite" {
Properties {
 _Color ("Color", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float4 _Color;
struct appdata_pc {
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 tmpvar_1;
  tmpvar_1 = _Color;
  pc_FragColor = tmpvar_1;
	return pc_FragColor;
}
ENDCG
 }
}
}