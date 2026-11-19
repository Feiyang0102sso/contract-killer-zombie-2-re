Shader "CKZ2/Unlit Scroll2Layer Mul Sine" {
Properties {
 _MainTex ("Base layer (RGB)", 2D) = "white" {}
 _ScrollX ("Base layer Scroll speed X", Float) = 1
 _ScrollY ("Base layer Scroll speed Y", Float) = 0
 _Scroll2X ("2nd layer Scroll speed X", Float) = 1
 _Scroll2Y ("2nd layer Scroll speed Y", Float) = 0
 _SineAmplX ("Base layer sine amplitude X", Float) = 0.5
 _SineAmplY ("Base layer sine amplitude Y", Float) = 0.5
 _SineFreqX ("Base layer sine freq X", Float) = 10
 _SineFreqY ("Base layer sine freq Y", Float) = 10
 _SineAmplX2 ("2nd layer sine amplitude X", Float) = 0.5
 _SineAmplY2 ("2nd layer sine amplitude Y", Float) = 0.5
 _SineFreqX2 ("2nd layer sine freq X", Float) = 10
 _SineFreqY2 ("2nd layer sine freq Y", Float) = 10
 _MMultiplier ("Layer Multiplier", Float) = 2
 _GlassThickness ("Glass Thikness", Float) = 0.125
 _GlassColor ("Glass Color", Color) = (0.99,0.7,0.99,1)
}
SubShader { 
 LOD 600
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
#pragma multi_compile _ LIGHTMAP_ON
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
#if defined(LIGHTMAP_ON)
float _SineFreqY2;
float _SineFreqY;
float _SineFreqX2;
float _SineFreqX;
float _SineAmplY2;
float _SineAmplY;
float _SineAmplX2;
float _SineAmplX;
float _ScrollY;
float _ScrollX;
float _Scroll2Y;
float _Scroll2X;
float _MMultiplier;
float _GlassThickness;
sampler2D _MainTex;
float4 _GlassColor;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float membrane;
  float3 worldNorm;
  float3 worldPos;
  float4 tmpvar_1;
  float4 tmpvar_2;
  float3 tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = _ScrollX;
  tmpvar_4.y = _ScrollY;
  tmpvar_1.xy = (v._glesMultiTexCoord0.xy + (tmpvar_4 * _Time.xy));
  float2 tmpvar_5;
  tmpvar_5.x = _Scroll2X;
  tmpvar_5.y = _Scroll2Y;
  tmpvar_1.zw = (v._glesMultiTexCoord0.xy + (tmpvar_5 * _Time.xy));
  tmpvar_1.x = (tmpvar_1.x + (sin((_Time * _SineFreqX)) * _SineAmplX).x);
  tmpvar_1.y = (tmpvar_1.y + (sin((_Time * _SineFreqY)) * _SineAmplY).x);
  tmpvar_1.z = (tmpvar_1.z + (sin((_Time * _SineFreqX2)) * _SineAmplX2).x);
  tmpvar_1.w = (tmpvar_1.w + (sin((_Time * _SineFreqY2)) * _SineAmplY2).x);
  float3 tmpvar_6;
  tmpvar_6 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  worldPos = tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7.w = 0.0;
  tmpvar_7.xyz = normalize(v._glesNormal);
  float3 tmpvar_8;
  tmpvar_8 = normalize((mul(unity_ObjectToWorld, tmpvar_7))).xyz;
  worldNorm = tmpvar_8;
  float tmpvar_9;
  tmpvar_9 = abs(dot(normalize((worldPos - _WorldSpaceCameraPos)), worldNorm));
  membrane = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = pow(membrane, _GlassThickness);
  membrane = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11.zw = float2(0.0, 0.0);
  tmpvar_11.x = _MMultiplier;
  tmpvar_11.y = membrane;
  tmpvar_2 = tmpvar_11;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = ((((tex2D(_MainTex, i.xlv_TEXCOORD0.xy) * tex2D(_MainTex, i.xlv_TEXCOORD0.zw)) * i.xlv_TEXCOORD1.x) * i.xlv_TEXCOORD1.y) + (_GlassColor * (1.0 - i.xlv_TEXCOORD1.y)));
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
float _SineFreqY2;
float _SineFreqY;
float _SineFreqX2;
float _SineFreqX;
float _SineAmplY2;
float _SineAmplY;
float _SineAmplX2;
float _SineAmplX;
float _ScrollY;
float _ScrollX;
float _Scroll2Y;
float _Scroll2X;
float _MMultiplier;
float _GlassThickness;
sampler2D _MainTex;
float4 _GlassColor;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float membrane;
  float3 worldNorm;
  float3 worldPos;
  float4 tmpvar_1;
  float4 tmpvar_2;
  float3 tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = _ScrollX;
  tmpvar_4.y = _ScrollY;
  tmpvar_1.xy = (v._glesMultiTexCoord0.xy + (tmpvar_4 * _Time.xy));
  float2 tmpvar_5;
  tmpvar_5.x = _Scroll2X;
  tmpvar_5.y = _Scroll2Y;
  tmpvar_1.zw = (v._glesMultiTexCoord0.xy + (tmpvar_5 * _Time.xy));
  tmpvar_1.x = (tmpvar_1.x + (sin((_Time * _SineFreqX)) * _SineAmplX).x);
  tmpvar_1.y = (tmpvar_1.y + (sin((_Time * _SineFreqY)) * _SineAmplY).x);
  tmpvar_1.z = (tmpvar_1.z + (sin((_Time * _SineFreqX2)) * _SineAmplX2).x);
  tmpvar_1.w = (tmpvar_1.w + (sin((_Time * _SineFreqY2)) * _SineAmplY2).x);
  float3 tmpvar_6;
  tmpvar_6 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  worldPos = tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7.w = 0.0;
  tmpvar_7.xyz = normalize(v._glesNormal);
  float3 tmpvar_8;
  tmpvar_8 = normalize((mul(unity_ObjectToWorld, tmpvar_7))).xyz;
  worldNorm = tmpvar_8;
  float tmpvar_9;
  tmpvar_9 = abs(dot(normalize((worldPos - _WorldSpaceCameraPos)), worldNorm));
  membrane = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = pow(membrane, _GlassThickness);
  membrane = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11.zw = float2(0.0, 0.0);
  tmpvar_11.x = _MMultiplier;
  tmpvar_11.y = membrane;
  tmpvar_2 = tmpvar_11;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = ((((tex2D(_MainTex, i.xlv_TEXCOORD0.xy) * tex2D(_MainTex, i.xlv_TEXCOORD0.zw)) * i.xlv_TEXCOORD1.x) * i.xlv_TEXCOORD1.y) + (_GlassColor * (1.0 - i.xlv_TEXCOORD1.y)));
	return pc_FragColor;
}
#endif
ENDCG
 }
}
SubShader { 
 LOD 400
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
#pragma multi_compile _ LIGHTMAP_ON
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
#if defined(LIGHTMAP_ON)
float _ScrollY;
float _ScrollX;
float _Scroll2Y;
float _Scroll2X;
float _MMultiplier;
float _GlassThickness;
sampler2D _MainTex;
float4 _GlassColor;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float membrane;
  float3 worldNorm;
  float3 worldPos;
  float4 tmpvar_1;
  float4 tmpvar_2;
  float3 tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = _ScrollX;
  tmpvar_4.y = _ScrollY;
  tmpvar_1.xy = (v._glesMultiTexCoord0.xy + (tmpvar_4 * _Time.xy));
  float2 tmpvar_5;
  tmpvar_5.x = _Scroll2X;
  tmpvar_5.y = _Scroll2Y;
  tmpvar_1.zw = (v._glesMultiTexCoord0.xy + (tmpvar_5 * _Time.xy));
  float3 tmpvar_6;
  tmpvar_6 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  worldPos = tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7.w = 0.0;
  tmpvar_7.xyz = normalize(v._glesNormal);
  float3 tmpvar_8;
  tmpvar_8 = normalize((mul(unity_ObjectToWorld, tmpvar_7))).xyz;
  worldNorm = tmpvar_8;
  float tmpvar_9;
  tmpvar_9 = abs(dot(normalize((worldPos - _WorldSpaceCameraPos)), worldNorm));
  membrane = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = pow(membrane, _GlassThickness);
  membrane = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11.zw = float2(0.0, 0.0);
  tmpvar_11.x = _MMultiplier;
  tmpvar_11.y = membrane;
  tmpvar_2 = tmpvar_11;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = ((((tex2D(_MainTex, i.xlv_TEXCOORD0.xy) * tex2D(_MainTex, i.xlv_TEXCOORD0.zw)) * i.xlv_TEXCOORD1.x) * i.xlv_TEXCOORD1.y) + (_GlassColor * (1.0 - i.xlv_TEXCOORD1.y)));
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
float _ScrollY;
float _ScrollX;
float _Scroll2Y;
float _Scroll2X;
float _MMultiplier;
float _GlassThickness;
sampler2D _MainTex;
float4 _GlassColor;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float3 _glesNormal : NORMAL;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float membrane;
  float3 worldNorm;
  float3 worldPos;
  float4 tmpvar_1;
  float4 tmpvar_2;
  float3 tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = _ScrollX;
  tmpvar_4.y = _ScrollY;
  tmpvar_1.xy = (v._glesMultiTexCoord0.xy + (tmpvar_4 * _Time.xy));
  float2 tmpvar_5;
  tmpvar_5.x = _Scroll2X;
  tmpvar_5.y = _Scroll2Y;
  tmpvar_1.zw = (v._glesMultiTexCoord0.xy + (tmpvar_5 * _Time.xy));
  float3 tmpvar_6;
  tmpvar_6 = (mul(unity_ObjectToWorld, v._glesVertex)).xyz;
  worldPos = tmpvar_6;
  float4 tmpvar_7;
  tmpvar_7.w = 0.0;
  tmpvar_7.xyz = normalize(v._glesNormal);
  float3 tmpvar_8;
  tmpvar_8 = normalize((mul(unity_ObjectToWorld, tmpvar_7))).xyz;
  worldNorm = tmpvar_8;
  float tmpvar_9;
  tmpvar_9 = abs(dot(normalize((worldPos - _WorldSpaceCameraPos)), worldNorm));
  membrane = tmpvar_9;
  float tmpvar_10;
  tmpvar_10 = pow(membrane, _GlassThickness);
  membrane = tmpvar_10;
  float4 tmpvar_11;
  tmpvar_11.zw = float2(0.0, 0.0);
  tmpvar_11.x = _MMultiplier;
  tmpvar_11.y = membrane;
  tmpvar_2 = tmpvar_11;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = ((((tex2D(_MainTex, i.xlv_TEXCOORD0.xy) * tex2D(_MainTex, i.xlv_TEXCOORD0.zw)) * i.xlv_TEXCOORD1.x) * i.xlv_TEXCOORD1.y) + (_GlassColor * (1.0 - i.xlv_TEXCOORD1.y)));
	return pc_FragColor;
}
#endif
ENDCG
 }
}
SubShader { 
 LOD 200
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
#pragma multi_compile _ LIGHTMAP_ON
// PC 移植：由 pc_pilot_tools/gles_to_hlsl.py 从原版 GLES 代码翻译
#if defined(LIGHTMAP_ON)
float _ScrollY;
float _ScrollX;
float _Scroll2Y;
float _Scroll2X;
float _MMultiplier;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  float3 tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = _ScrollX;
  tmpvar_4.y = _ScrollY;
  tmpvar_1.xy = (v._glesMultiTexCoord0.xy + (tmpvar_4 * _Time.xy));
  float2 tmpvar_5;
  tmpvar_5.x = _Scroll2X;
  tmpvar_5.y = _Scroll2Y;
  tmpvar_1.zw = (v._glesMultiTexCoord0.xy + (tmpvar_5 * _Time.xy));
  float4 tmpvar_6;
  tmpvar_6 = ((float4)(_MMultiplier));
  tmpvar_2 = tmpvar_6;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = ((tex2D(_MainTex, i.xlv_TEXCOORD0.xy) * tex2D(_MainTex, i.xlv_TEXCOORD0.zw)) * i.xlv_TEXCOORD1.x);
	return pc_FragColor;
}
#else // LIGHTMAP_OFF
float _ScrollY;
float _ScrollX;
float _Scroll2Y;
float _Scroll2X;
float _MMultiplier;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float4 xlv_TEXCOORD1 : TEXCOORD1;
	float4 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float4 tmpvar_1;
  float4 tmpvar_2;
  float3 tmpvar_3;
  float2 tmpvar_4;
  tmpvar_4.x = _ScrollX;
  tmpvar_4.y = _ScrollY;
  tmpvar_1.xy = (v._glesMultiTexCoord0.xy + (tmpvar_4 * _Time.xy));
  float2 tmpvar_5;
  tmpvar_5.x = _Scroll2X;
  tmpvar_5.y = _Scroll2Y;
  tmpvar_1.zw = (v._glesMultiTexCoord0.xy + (tmpvar_5 * _Time.xy));
  float4 tmpvar_6;
  tmpvar_6 = ((float4)(_MMultiplier));
  tmpvar_2 = tmpvar_6;
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = tmpvar_1;
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = ((tex2D(_MainTex, i.xlv_TEXCOORD0.xy) * tex2D(_MainTex, i.xlv_TEXCOORD0.zw)) * i.xlv_TEXCOORD1.x);
	return pc_FragColor;
}
#endif
ENDCG
 }
}
Fallback "CKZ2/Transparent by Multiply2x"
}