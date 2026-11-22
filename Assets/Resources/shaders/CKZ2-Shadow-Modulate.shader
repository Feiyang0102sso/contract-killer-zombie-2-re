Shader "CKZ2/Shadow AO (Modulate)" {
Properties {
 _Ambient ("Ambient", Color) = (0,0,0,1)
 _Occluder1 ("Occluder 1", Vector) = (0,0,0,1)
 _Occluder2 ("Occluder 2", Vector) = (0,0,0,1)
 _Occluder3 ("Occluder 3", Vector) = (0,0,0,1)
}
SubShader { 
 LOD 100
 Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
  Blend DstColor Zero
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by gles_to_hlsl.py
float4 _Occluder3;
float4 _Occluder2;
float4 _Occluder1;
float4 _Ambient;
struct appdata_pc {
	float3 _glesNormal : NORMAL;
	float4 _glesColor : COLOR;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD1 : TEXCOORD0;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float3 tmpvar_1;
  tmpvar_1 = normalize(v._glesNormal);
  float3 shl;
  float4 tmpvar_2;
  float3 worldPos;
  float3 tmpvar_3;
  tmpvar_3 = v._glesVertex.xyz;
  worldPos = tmpvar_3;
  float3 tmpvar_4;
  tmpvar_4 = (_Occluder1.xyz - worldPos);
  float tmpvar_5;
  tmpvar_5 = length(tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (_Occluder1.w / tmpvar_5);
  float3 tmpvar_7;
  tmpvar_7 = (_Occluder2.xyz - worldPos);
  float tmpvar_8;
  tmpvar_8 = length(tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (_Occluder2.w / tmpvar_8);
  float3 tmpvar_10;
  tmpvar_10 = (_Occluder3.xyz - worldPos);
  float tmpvar_11;
  tmpvar_11 = length(tmpvar_10);
  float tmpvar_12;
  tmpvar_12 = (_Occluder3.w / tmpvar_11);
  float3 tmpvar_13;
  tmpvar_13 = ((float3)(clamp((((((dot(tmpvar_1, (tmpvar_4 * (1.0/(tmpvar_5)))) * tmpvar_6) * tmpvar_6) + ((dot(tmpvar_1, (tmpvar_7 * (1.0/(tmpvar_8)))) * tmpvar_9) * tmpvar_9)) + ((dot(tmpvar_1, (tmpvar_10 * (1.0/(tmpvar_11)))) * tmpvar_12) * tmpvar_12)) * (1.0 - v._glesColor.xyz)).x, 0.0, 1.0)));
  float3 tmpvar_14;
  tmpvar_14 = lerp(float3(1.0, 1.0, 1.0), _Ambient.xyz, tmpvar_13);
  shl = tmpvar_14;
  float4 tmpvar_15;
  tmpvar_15.w = 1.0;
  tmpvar_15.xyz = shl;
  tmpvar_2 = tmpvar_15;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD1 = tmpvar_2;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = i.xlv_TEXCOORD1;
	return pc_FragColor;
}
ENDCG
 }
}
}