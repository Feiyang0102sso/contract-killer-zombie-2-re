Shader "CKZ2/Diffuse-Animated-Per Vertex Hightlight" {
Properties {
 _MainTex ("Base texture", 2D) = "white" {}
 _Multiplier ("Color multiplier", Float) = 1
 _TimeDelay ("Time delay", Vector) = (0.05,0.05,-0.05,0)
 _TimeShift ("Time shift", Float) = 5
 _Duration ("Total duration", Float) = 3
 _BlinkDuration ("Blink duration", Float) = 0.27
 _Color ("Color", Color) = (1,1,1,1)
}
SubShader { 
 LOD 100
 Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float _TimeShift;
float4 _TimeDelay;
float _Multiplier;
float _Duration;
float4 _Color;
float _BlinkDuration;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  float tmpvar_3;
  tmpvar_3 = (((_Time.y + dot(tmpvar_2, _TimeDelay)) + _TimeShift) / _Duration);
  float tmpvar_4;
  tmpvar_4 = (frac(abs(tmpvar_3)) * _Duration);
  float tmpvar_5;
  if ((tmpvar_3 >= 0.0)) {
    tmpvar_5 = tmpvar_4;
  } else {
    tmpvar_5 = -(tmpvar_4);
  };
  float4 tmpvar_6;
  tmpvar_6 = ((_Color * _Multiplier) * sin(((3.14159 * (tmpvar_5 * step(tmpvar_5, _BlinkDuration))) / _BlinkDuration)));
  tmpvar_1 = tmpvar_6;
  o.pos = tmpvar_2;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = (tex2D(_MainTex, i.xlv_TEXCOORD0) + i.xlv_TEXCOORD1);
	return pc_FragColor;
}
ENDCG
 }
}
Fallback "Mobile/Diffuse"
}