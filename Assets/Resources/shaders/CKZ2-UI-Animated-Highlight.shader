Shader "CKZ2/UI-Animated-Per Vertex Hightlight" {
Properties {
 _MainTex ("Base texture", 2D) = "white" {}
 _Multiplier ("Color multiplier", Float) = 1
 _TimeDelayX ("Time delay X", Float) = 1
 _TimeDelayY ("Time delay Y", Float) = 1
 _TimeShift ("Time shift", Float) = 1
 _Duration ("Total duration", Float) = 0.5
 _BlinkDuration ("Blink duration", Float) = 0.5
 _Color ("Color", Color) = (1,1,1,1)
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
  Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float _TimeShift;
float _TimeDelayY;
float _TimeDelayX;
float _Multiplier;
float _Duration;
float4 _Color;
float _BlinkDuration;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesColor : COLOR;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  float tmpvar_3;
  tmpvar_3 = (_BlinkDuration / 3.14159);
  float2 tmpvar_4;
  tmpvar_4.x = _TimeDelayX;
  tmpvar_4.y = _TimeDelayY;
  float tmpvar_5;
  tmpvar_5 = (((_Time.y + dot(tmpvar_2.xy, tmpvar_4)) + _TimeShift) / _Duration);
  float tmpvar_6;
  tmpvar_6 = (frac(abs(tmpvar_5)) * _Duration);
  float tmpvar_7;
  if ((tmpvar_5 >= 0.0)) {
    tmpvar_7 = tmpvar_6;
  } else {
    tmpvar_7 = -(tmpvar_6);
  };
  float4 tmpvar_8;
  tmpvar_8 = ((_Color * _Multiplier) * sin(((tmpvar_7 * step(tmpvar_7, (3.14159 * tmpvar_3))) / tmpvar_3)));
  tmpvar_1 = tmpvar_8;
  o.pos = tmpvar_2;
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = v._glesColor;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * i.xlv_TEXCOORD2);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.w * (tmpvar_1.xyz + i.xlv_TEXCOORD1.xyz));
  pc_FragColor = c;
	return pc_FragColor;
}
ENDCG
 }
}
}