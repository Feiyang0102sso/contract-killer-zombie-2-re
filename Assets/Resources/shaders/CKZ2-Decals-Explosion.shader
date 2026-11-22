Shader "CKZ2/Decal Explosion (Alpha)" {
Properties {
 _MainTex ("Texture", 2D) = "white" {}
 _Time0 ("Time0", Float) = 0
 _Time1 ("Time1", Float) = 0
 _ColorFalloff ("Color Falloff", Float) = 1
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent-1" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent-1" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
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
float _Time1;
float _Time0;
float _ColorFalloff;
sampler2D _MainTex;
struct appdata_pc {
	float4 _glesMultiTexCoord0 : TEXCOORD0;
	float4 _glesVertex : POSITION;
};
struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
  float2 tmpvar_1;
  tmpvar_1.x = (1.0 - clamp(((_Time.y - _Time0) / (_Time1 - _Time0)), 0.0, 1.0));
  tmpvar_1.y = pow(tmpvar_1.x, _ColorFalloff);
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
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
  float4 tmpvar_2;
  tmpvar_2.xyz = (tmpvar_1.xyz * i.xlv_TEXCOORD1.y);
  tmpvar_2.w = (tmpvar_1.w * i.xlv_TEXCOORD1.x);
  c = tmpvar_2;
  pc_FragColor = c;
	return pc_FragColor;
}
ENDCG
 }
}
}