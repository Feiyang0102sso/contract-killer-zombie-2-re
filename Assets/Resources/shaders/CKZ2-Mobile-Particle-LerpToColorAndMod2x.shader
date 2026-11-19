Shader "CKZ2/Mobile Particle Lerp-To-Color And Mod2x" {
Properties {
 _MainTex ("Particle Texture", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
  Blend DstColor SrcColor
  ColorMask RGB
  SetTexture [_MainTex] { ConstantColor (0.5,0.5,0.5,1) combine texture lerp(primary) constant }
 }
}
}