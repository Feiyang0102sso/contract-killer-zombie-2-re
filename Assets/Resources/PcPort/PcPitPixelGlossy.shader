// PC port, The Pit only (applied by PcPitEnhance): per-pixel version of "CKZ2/Lightmap + Unlit + VirtualVertexGlossy-Add".
// Same formula and properties as the original: a virtual light at _SpecOffset in view space, Blinn highlight
// pow(N.H, _Shininess * 128) * _SpecColor * 2, fading to nothing at _SpecRange, masked by the texture alpha, then times the
// lightmap (x2, dLDR). The original evaluates it once per vertex; the Pit's walls have few vertices and a sharp exponent,
// so the highlight on the shiny trim strips popped on and off as the camera moved. Here it is evaluated per pixel.
// Lower shader LODs fall back to the original (its LOD 150 subshader has no highlight at all).
Shader "CKZ2/PC/Lightmap + Unlit + PixelGlossy-Add" {
Properties {
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _SpecOffset ("Specular Offset from Camera", Vector) = (1,10,2,0)
 _SpecRange ("Specular Range", Float) = 20
 _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
 _Shininess ("Shininess", Range(0.01,1)) = 0.078125
}
SubShader {
 LOD 400
 Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="ForwardBase" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ LIGHTMAP_ON
#include "UnityCG.cginc"

float _SpecRange;
float3 _SpecOffset;
float3 _SpecColor;
float _Shininess;
sampler2D _MainTex;

struct appdata_pc {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
	float2 uvLightmap : TEXCOORD1;
};

struct v2f_pc {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uvLightmap : TEXCOORD1;
	// View-space position relative to the virtual light, and view-space normal (as in the original vertex shader)
	float3 lightToPoint : TEXCOORD2;
	float3 viewNormal : TEXCOORD3;
};

v2f_pc vert (appdata_pc v)
{
	v2f_pc o = (v2f_pc)0;
	float3 viewPosition = mul(UNITY_MATRIX_MV, v.vertex).xyz;
	o.lightToPoint = viewPosition - _SpecOffset * float3(1.0, 1.0, -1.0);
	o.viewNormal = mul((float3x3)UNITY_MATRIX_MV, normalize(v.normal));
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	o.uvLightmap = v.uvLightmap * unity_LightmapST.xy + unity_LightmapST.zw;
	return o;
}

float4 frag (v2f_pc i) : SV_Target
{
	float3 normal = normalize(i.viewNormal);
	float3 halfVector = normalize((float3(0.0, 0.0, 1.0) + normalize(-i.lightToPoint)) * 0.5);
	float highlight = pow(saturate(dot(normal, halfVector)), _Shininess * 128.0);
	float fade = 1.0 - saturate(length(i.lightToPoint) / _SpecRange);
	float3 specular = _SpecColor * highlight * 2.0 * fade;

	float4 color = tex2D(_MainTex, i.uv);
	color.rgb = color.rgb + specular * color.a;
#if defined(LIGHTMAP_ON)
	color.rgb = color.rgb * (2.0 * UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLightmap).rgb);
#endif
	return color;
}
ENDCG
 }
}
Fallback "CKZ2/Lightmap + Unlit + VirtualVertexGlossy-Add"
}
