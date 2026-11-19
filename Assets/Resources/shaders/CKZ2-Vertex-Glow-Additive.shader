Shader "CKZ2/Per-Vertex Glow (Additive)" {
Properties {
 _MainTex ("Base (RGB) Emissive (A)", 2D) = "white" {}
 _EmissiveColor ("Emissive Color", Color) = (0.5,0.5,0.5,1)
 _FadeOutDistanceNear ("Fadeout: near dist", Float) = 1
 _FadeOutDistanceFar ("Fadeout: far dist", Float) = 10
 _GrowSize ("Grow size", Float) = 1
 _BlinkFreq1 ("Blinking frequency 1", Float) = 0
 _BlinkFreq2 ("Blinking frequency 2", Float) = 0
 _BlinkTreshold ("Blinking treshold", Float) = 0
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
  Blend One One
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
float _GrowSize;
float _FadeOutDistanceNear;
float _FadeOutDistanceFar;
float3 _EmissiveColor;
float _BlinkTreshold;
float _BlinkFreq2;
float _BlinkFreq1;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesColor : COLOR;
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
  float tmpvar_2;
  tmpvar_2 = length((_WorldSpaceCameraPos - (mul(unity_ObjectToWorld, v._glesVertex)).xyz));
  float tmpvar_3;
  tmpvar_3 = (1.0 - clamp((tmpvar_2 / _FadeOutDistanceNear), 0.0, 1.0));
  float tmpvar_4;
  tmpvar_4 = clamp(((tmpvar_2 - _FadeOutDistanceNear) / (_FadeOutDistanceFar - _FadeOutDistanceNear)), 0.0, 1.0);
  float tmpvar_5;
  tmpvar_5 = ((unity_ObjectToWorld._m00_m10_m20_m30.w * unity_ObjectToWorld._m01_m11_m21_m31.w) * unity_ObjectToWorld._m02_m12_m22_m32.w);
  float tmpvar_6;
  tmpvar_6 = clamp(sign((_BlinkFreq1 + _BlinkFreq2)), 0.0, 1.0);
  float tmpvar_7;
  tmpvar_7 = ((tmpvar_4 * (1.0 - tmpvar_6)) + ((tmpvar_4 * step(_BlinkTreshold, (((sin((tmpvar_5 + (_Time.y * _BlinkFreq1))) + sin((tmpvar_5 + (_Time.y * _BlinkFreq2)))) + 2.0) / 4.0))) * tmpvar_6));
  float4 tmpvar_8;
  tmpvar_8.w = 0.0;
  tmpvar_8.xyz = (normalize(v._glesNormal) * (((_GrowSize * tmpvar_7) * v._glesColor.x) - (clamp((tmpvar_3 / tmpvar_3), 0.0, 1.0) * _GrowSize)));
  float4 tmpvar_9;
  tmpvar_9.xyz = (_EmissiveColor * tmpvar_7);
  tmpvar_9.w = tmpvar_7;
  tmpvar_1 = tmpvar_9;
  o.pos = (mul(UNITY_MATRIX_MVP, (v._glesVertex + tmpvar_8)));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  float4 c;
  float4 tmpvar_1;
  tmpvar_1 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c = tmpvar_1;
  c.xyz = (tmpvar_1.xyz * i.xlv_TEXCOORD1.xyz);
  pc_FragColor = c;
	return pc_FragColor;
}
ENDCG
 }
}
}