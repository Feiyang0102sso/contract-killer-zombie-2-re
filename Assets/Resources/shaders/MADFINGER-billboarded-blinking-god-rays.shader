Shader "MADFINGER/Transparent/Blinking GodRays Billboarded" {
Properties {
 _MainTex ("Base texture", 2D) = "white" {}
 _FadeOutDistNear ("Near fadeout dist", Float) = 10
 _FadeOutDistFar ("Far fadeout dist", Float) = 10000
 _Multiplier ("Color multiplier", Float) = 1
 _Bias ("Bias", Float) = 0
 _TimeOnDuration ("ON duration", Float) = 0.5
 _TimeOffDuration ("OFF duration", Float) = 0.5
 _BlinkingTimeOffsScale ("Blinking time offset scale (seconds)", Float) = 5
 _SizeGrowStartDist ("Size grow start dist", Float) = 5
 _SizeGrowEndDist ("Size grow end dist", Float) = 50
 _MaxGrowSize ("Max grow size", Float) = 2.5
 _NoiseAmount ("Noise amount (when zero, pulse wave is used)", Range(0,0.5)) = 0
 _VerticalBillboarding ("Vertical billboarding amount", Range(0,1)) = 1
 _ViewerOffset ("Viewer offset", Float) = 0
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
  Blend One One
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float _ViewerOffset;
float _VerticalBillboarding;
float _TimeOnDuration;
float _TimeOffDuration;
float _SizeGrowStartDist;
float _SizeGrowEndDist;
float _NoiseAmount;
float _Multiplier;
float _MaxGrowSize;
float _FadeOutDistNear;
float _FadeOutDistFar;
float4 _Color;
float _BlinkingTimeOffsScale;
float _Bias;
sampler2D _MainTex;
#define pc_ObjectScale() length(unity_ObjectToWorld._m00_m10_m20)
#define pc_World2Object float4x4(unity_WorldToObject[0] * pc_ObjectScale(), unity_WorldToObject[1] * pc_ObjectScale(), unity_WorldToObject[2] * pc_ObjectScale(), unity_WorldToObject[3])
struct appdata_pc {
	float4 _glesMultiTexCoord1 : TEXCOORD1;
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
  float3 tmpvar_1;
  tmpvar_1 = normalize(v._glesNormal);
  float4 tmpvar_2;
  float3 localDir;
  float3 tmpvar_3;
  tmpvar_3.z = 0.0;
  tmpvar_3.xy = (float2(0.5, 0.5) - v._glesColor.xy);
  float3 tmpvar_4;
  tmpvar_4 = (tmpvar_3 * v._glesMultiTexCoord1.xyy);
  float3 tmpvar_5;
  tmpvar_5 = (v._glesVertex.xyz + tmpvar_4);
  float4 tmpvar_6;
  tmpvar_6.w = 1.0;
  tmpvar_6.xyz = _WorldSpaceCameraPos;
  float3 tmpvar_7;
  tmpvar_7 = ((mul(pc_World2Object, tmpvar_6)).xyz - tmpvar_5);
  localDir = tmpvar_7;
  localDir.y = lerp(0.0, tmpvar_7.y, _VerticalBillboarding);
  float tmpvar_8;
  tmpvar_8 = length(localDir);
  float3 dir;
  dir = (localDir / tmpvar_8);
  float tmpvar_9;
  tmpvar_9 = abs(dir.y);
  float3 tmpvar_10;
  if ((tmpvar_9 > 0.999)) {
    tmpvar_10 = float3(0.0, 0.0, 1.0);
  } else {
    tmpvar_10 = float3(0.0, 1.0, 0.0);
  };
  float3 tmpvar_11;
  tmpvar_11 = normalize(cross(tmpvar_10, dir));
  float3 tmpvar_12;
  tmpvar_12 = cross(dir, tmpvar_11);
  float tmpvar_13;
  tmpvar_13 = min((max((tmpvar_8 - _SizeGrowStartDist), 0.0) / _SizeGrowEndDist), 1.0);
  float3 tmpvar_14;
  tmpvar_14 = (((tmpvar_5 - ((tmpvar_11 * tmpvar_4.x) + (tmpvar_12 * tmpvar_4.y))) + (((tmpvar_11 * tmpvar_1.x) + (tmpvar_12 * tmpvar_1.y)) * (((tmpvar_13 * tmpvar_13) * _MaxGrowSize) * v._glesColor.w))) + (_ViewerOffset * localDir));
  float tmpvar_15;
  tmpvar_15 = (_Time.y + (_BlinkingTimeOffsScale * v._glesColor.z));
  float y;
  y = (_TimeOnDuration + _TimeOffDuration);
  float tmpvar_16;
  tmpvar_16 = (tmpvar_15 / y);
  float tmpvar_17;
  tmpvar_17 = (frac(abs(tmpvar_16)) * y);
  float tmpvar_18;
  if ((tmpvar_16 >= 0.0)) {
    tmpvar_18 = tmpvar_17;
  } else {
    tmpvar_18 = -(tmpvar_17);
  };
  float tmpvar_19;
  tmpvar_19 = (smoothstep(0.0, (_TimeOnDuration * 0.25), tmpvar_18) * (1.0 - smoothstep((_TimeOnDuration * 0.75), _TimeOnDuration, tmpvar_18)));
  float tmpvar_20;
  tmpvar_20 = (tmpvar_15 * (6.28319 / _TimeOnDuration));
  float tmpvar_21;
  tmpvar_21 = ((_NoiseAmount * (sin(tmpvar_20) * ((0.5 * cos(((tmpvar_20 * 0.6366) + 56.7272))) + 0.5))) + (1.0 - _NoiseAmount));
  float tmpvar_22;
  if ((_NoiseAmount < 0.01)) {
    tmpvar_22 = tmpvar_19;
  } else {
    tmpvar_22 = tmpvar_21;
  };
  float4 tmpvar_23;
  tmpvar_23.w = 1.0;
  tmpvar_23.xyz = tmpvar_14;
  float tmpvar_24;
  tmpvar_24 = clamp((tmpvar_8 / _FadeOutDistNear), 0.0, 1.0);
  float tmpvar_25;
  tmpvar_25 = (1.0 - clamp((max((tmpvar_8 - _FadeOutDistFar), 0.0) * 0.2), 0.0, 1.0));
  float tmpvar_26;
  tmpvar_26 = (tmpvar_24 * tmpvar_24);
  float4 tmpvar_27;
  tmpvar_27 = (((((tmpvar_26 * tmpvar_26) * (tmpvar_25 * tmpvar_25)) * _Color) * _Multiplier) * (tmpvar_22 + _Bias));
  tmpvar_2 = tmpvar_27;
  o.pos = (mul(UNITY_MATRIX_MVP, tmpvar_23));
  o.xlv_TEXCOORD0 = v._glesMultiTexCoord0.xy;
  o.xlv_TEXCOORD1 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = (tex2D(_MainTex, i.xlv_TEXCOORD0) * i.xlv_TEXCOORD1);
	return pc_FragColor;
}
ENDCG
 }
}
}