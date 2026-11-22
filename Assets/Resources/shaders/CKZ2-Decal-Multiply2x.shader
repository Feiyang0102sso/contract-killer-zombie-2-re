Shader "CKZ2/Decal (Multiply2x)" {
Properties {
 _MainTex ("Texture", 2D) = "white" {}
 _UV1 ("1st Tiling (X,Y) Offset (X,Y)", Vector) = (1,1,0,0)
 _UV2 ("2nd Tiling (X,Y) Offset (X,Y)", Vector) = (1,1,0,0)
 _UV3 ("3rd Tiling (X,Y) Offset (X,Y)", Vector) = (1,1,0,0)
 _UV4 ("4th Tiling (X,Y) Offset (X,Y)", Vector) = (1,1,0,0)
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
  Blend DstColor SrcColor
CGPROGRAM
// UNITY_SHADER_NO_UPGRADE
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
// PC port: translated from the original GLES code by tools/portNewUnity/asset_convert/gles_to_hlsl.py
float4 _UV4;
float4 _UV3;
float4 _UV2;
float4 _UV1;
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
  float seed;
  float tmpvar_1;
  tmpvar_1 = frac(((unity_ObjectToWorld._m00_m10_m20_m30.w * unity_ObjectToWorld._m01_m11_m21_m31.w) * unity_ObjectToWorld._m02_m12_m22_m32.w));
  seed = tmpvar_1;
  float tmpvar_2;
  tmpvar_2 = step(seed, 0.25);
  float tmpvar_3;
  tmpvar_3 = (1.0 - tmpvar_2);
  float tmpvar_4;
  tmpvar_4 = (tmpvar_3 * step(seed, 0.5));
  float tmpvar_5;
  tmpvar_5 = (tmpvar_3 * (1.0 - tmpvar_4));
  float tmpvar_6;
  tmpvar_6 = (tmpvar_5 * step(seed, 0.75));
  float tmpvar_7;
  tmpvar_7 = (tmpvar_5 * (1.0 - tmpvar_6));
  o.pos = (mul(UNITY_MATRIX_MVP, v._glesVertex));
  o.xlv_TEXCOORD0 = ((((((v._glesMultiTexCoord0.xy * _UV1.xy) + _UV1.zw) * tmpvar_2) + (((v._glesMultiTexCoord0.xy * _UV2.xy) + _UV2.zw) * tmpvar_4)) + (((v._glesMultiTexCoord0.xy * _UV3.xy) + _UV3.zw) * tmpvar_6)) + (((v._glesMultiTexCoord0.xy * _UV4.xy) + _UV4.zw) * tmpvar_7));
	return o;
}
float4 frag (v2f_pc i) : SV_Target
{
	float4 pc_FragColor = 0;
  pc_FragColor = tex2D(_MainTex, i.xlv_TEXCOORD0);
	return pc_FragColor;
}
ENDCG
 }
}
}