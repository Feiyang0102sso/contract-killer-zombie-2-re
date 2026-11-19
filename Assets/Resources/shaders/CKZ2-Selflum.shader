Shader "CKZ2/Self-Illuminated" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _SelfLumColor ("Illumination Color (RGBA)", Color) = (1,0,0,1)
}
SubShader { 
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Color [_SelfLumColor]
  ZWrite Off
  Cull Off
  Fog {
   Color (1,1,1,1)
  }
  ColorMask RGB
  Offset -1, -1
  SetTexture [_MainTex] { ConstantColor (1,1,1,0) combine texture * primary }
 }
}
}