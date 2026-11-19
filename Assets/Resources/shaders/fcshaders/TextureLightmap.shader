Shader "CK2/Texture Lightmap" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="Vertex" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  SetTexture [_MainTex] { combine texture }
 }
 Pass {
  Tags { "LIGHTMODE"="VertexLM" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord1", TexCoord0
   Bind "texcoord", TexCoord1
  }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture }
  SetTexture [_MainTex] { combine texture * previous double, texture alpha * primary alpha }
 }
 Pass {
  Tags { "LIGHTMODE"="VertexLMRGBM" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord1", TexCoord0
   Bind "texcoord", TexCoord1
  }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture * texture alpha double }
  SetTexture [_MainTex] { combine texture * previous quad, texture alpha * primary alpha }
 }
}
}