Shader "CKZ2/Inverse Per-Vertex Glow (Additive)" {
Properties {
 _MainTex ("Base (RGB) Emissive (A)", 2D) = "white" {}
 _EmissiveColor ("Emissive Color", Color) = (0.5,0.5,0.5,1)
 _FadeOutDistanceNear ("Fadeout: near dist", Float) = 1
 _FadeOutDistanceFar ("Fadeout: far dist", Float) = 10
 _GrowSize ("Grow size", Float) = 1
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
  tmpvar_2 = clamp(((length((_WorldSpaceCameraPos - (mul(unity_ObjectToWorld, v._glesVertex)).xyz)) - _FadeOutDistanceNear) / (_FadeOutDistanceFar - _FadeOutDistanceNear)), 0.0, 1.0);
  float4 tmpvar_3;
  tmpvar_3.w = 0.0;
  tmpvar_3.xyz = (normalize(v._glesNormal) * ((_GrowSize * (1.0 - tmpvar_2)) * v._glesColor.x));
  float4 tmpvar_4;
  tmpvar_4.xyz = (_EmissiveColor * tmpvar_2);
  tmpvar_4.w = tmpvar_2;
  tmpvar_1 = tmpvar_4;
  o.pos = (mul(UNITY_MATRIX_MVP, (v._glesVertex + tmpvar_3)));
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